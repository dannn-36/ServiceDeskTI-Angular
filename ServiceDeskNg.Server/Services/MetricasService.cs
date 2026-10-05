using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Services
{
    /// Paneles, indicadores y reportes. Todo sale de la base de datos:
    /// este servicio sustituye los endpoints que devolvían valores inventados
    /// con `new Random()` o listas escritas a mano.
    public class MetricasService
    {
        private readonly IRepositorio<Ticket> _tickets;
        private readonly IRepositorio<Agente> _agentes;
        private readonly CatalogoTicketsService _catalogo;
        private readonly OpcionesSla _sla;

        public MetricasService(
            IRepositorio<Ticket> tickets,
            IRepositorio<Agente> agentes,
            CatalogoTicketsService catalogo,
            IOptions<OpcionesSla> sla)
        {
            _tickets = tickets;
            _agentes = agentes;
            _catalogo = catalogo;
            _sla = sla.Value;
        }

        /// Fila plana de ticket con los nombres ya resueltos.
        /// Los cálculos se hacen en memoria porque el volumen es pequeño y así
        /// se evita depender de funciones de fecha propias del motor.
        private sealed record FilaTicket(
            int IdTicket,
            string Titulo,
            string? Descripcion,
            string NombreCliente,
            int? IdAgente,
            string? NombreAgente,
            int IdEstado,
            string NombreEstado,
            string NombreCategoria,
            string? Prioridad,
            DateTime? Creacion,
            DateTime? Actualizacion)
        {
            public string EstadoNormalizado => NombreEstado.Trim().ToLowerInvariant().Replace(' ', '-');

            public bool Finalizado => CatalogoTicketsService.EsEstadoFinalizado(EstadoNormalizado);

            public string PrioridadNormalizada => (Prioridad ?? string.Empty).Trim().ToLowerInvariant();

            public double? HorasResolucion =>
                Finalizado && Creacion.HasValue && Actualizacion.HasValue
                    ? Math.Max(0, (Actualizacion.Value - Creacion.Value).TotalHours)
                    : null;
        }

        private Task<List<FilaTicket>> LeerFilasAsync(CancellationToken ct) =>
            _tickets.Query()
                .Select(t => new FilaTicket(
                    t.IdTicket,
                    t.TituloTicket,
                    t.DescripcionTicket,
                    t.IdClienteNavigation!.IdUsuarioNavigation.NombreUsuario,
                    t.IdAgenteAsignado,
                    t.IdAgenteAsignadoNavigation == null
                        ? null
                        : t.IdAgenteAsignadoNavigation.IdUsuarioNavigation.NombreUsuario,
                    t.IdEstadoTicket,
                    t.IdEstadoTicketNavigation!.NombreEstado,
                    t.IdCategoriaTicketNavigation!.NombreCategoria,
                    t.PrioridadTicket,
                    t.FechaHoraCreacionTicket,
                    t.FechaHoraActualizacionTicket))
                .ToListAsync(ct);

        // ======================================================
        // Paneles
        // ======================================================

        public async Task<List<TicketPanelDto>> PanelAsync(CancellationToken ct = default)
        {
            var filas = await LeerFilasAsync(ct);
            var ahora = DateTime.UtcNow;

            return filas
                .OrderByDescending(f => f.Creacion)
                .Select(f => APanel(f, ahora))
                .ToList();
        }

        /// Tickets de prioridad alta o urgente que siguen sin resolverse.
        public async Task<List<TicketPanelDto>> PrioritariosAsync(CancellationToken ct = default)
        {
            var filas = await LeerFilasAsync(ct);
            var ahora = DateTime.UtcNow;

            return filas
                .Where(f => !f.Finalizado
                            && PrioridadesTicket.Preferentes.Contains(f.PrioridadNormalizada))
                .OrderBy(f => f.PrioridadNormalizada == PrioridadesTicket.Urgente ? 0 : 1)
                .ThenBy(f => f.Creacion)
                .Select(f => APanel(f, ahora))
                .ToList();
        }

        /// Tickets sin resolver que superaron el plazo del SLA.
        public async Task<List<TicketPanelDto>> VencidosAsync(CancellationToken ct = default)
        {
            var filas = await LeerFilasAsync(ct);
            var ahora = DateTime.UtcNow;
            var limite = ahora.AddHours(-_sla.HorasVencimiento);

            return filas
                .Where(f => !f.Finalizado && f.Creacion.HasValue && f.Creacion.Value <= limite)
                .OrderBy(f => f.Creacion)
                .Select(f => APanel(f, ahora))
                .ToList();
        }

        /// Escalaciones reales: lo urgente, lo vencido y lo de prioridad alta.
        /// La versión anterior marcaba como "escalado" cualquier ticket resuelto
        /// o pendiente, de modo que el panel no significaba nada.
        public async Task<List<EscalacionDto>> EscalacionesAsync(CancellationToken ct = default)
        {
            var filas = await LeerFilasAsync(ct);
            var ahora = DateTime.UtcNow;
            var limiteSla = ahora.AddHours(-_sla.HorasVencimiento);

            var escalaciones = new List<EscalacionDto>();

            foreach (var fila in filas)
            {
                var urgente = fila.PrioridadNormalizada == PrioridadesTicket.Urgente;
                var alta = fila.PrioridadNormalizada == PrioridadesTicket.Alta;
                var vencido = fila.Creacion.HasValue && fila.Creacion.Value <= limiteSla;

                if (!urgente && !alta && !vencido)
                    continue;

                var (estado, motivo) = (fila.Finalizado, urgente, vencido) switch
                {
                    (true, _, _) => ("resolved", "Atendido"),
                    (false, true, _) => ("critical", "Prioridad urgente"),
                    (false, false, true) => ("critical", $"SLA vencido (más de {_sla.HorasVencimiento} h)"),
                    _ => ("pending", "Prioridad alta")
                };

                escalaciones.Add(new EscalacionDto
                {
                    Id = fila.IdTicket,
                    Title = fila.Titulo,
                    EscalatedTo = fila.NombreAgente ?? "Sin asignar",
                    Reason = motivo,
                    Time = TiempoRelativo.Formatear(fila.Creacion, ahora),
                    Status = estado
                });
            }

            return escalaciones
                .OrderBy(e => e.Status == "critical" ? 0 : e.Status == "pending" ? 1 : 2)
                .ToList();
        }

        // ======================================================
        // Indicadores
        // ======================================================

        /// Tickets creados y finalizados por día en los últimos siete días.
        /// Un ticket cuenta como finalizado el día de su última actualización,
        /// que es el dato más cercano a la fecha de cierre que guarda el esquema.
        public async Task<RendimientoSemanalDto> RendimientoSemanalAsync(CancellationToken ct = default)
        {
            var filas = await LeerFilasAsync(ct);

            var hoy = DateTime.UtcNow.Date;
            var dias = Enumerable.Range(0, 7).Select(i => hoy.AddDays(-6 + i)).ToArray();

            var etiquetas = dias
                .Select(d => d.ToString("ddd", new System.Globalization.CultureInfo("es-ES")))
                .Select(e => char.ToUpperInvariant(e[0]) + e[1..].TrimEnd('.'))
                .ToArray();

            var creados = new int[7];
            var resueltos = new int[7];

            for (var i = 0; i < dias.Length; i++)
            {
                var dia = dias[i];

                creados[i] = filas.Count(f => f.Creacion.HasValue && f.Creacion.Value.Date == dia);
                resueltos[i] = filas.Count(f => f.Finalizado
                                                && f.Actualizacion.HasValue
                                                && f.Actualizacion.Value.Date == dia);
            }

            return new RendimientoSemanalDto
            {
                Labels = etiquetas,
                Created = creados,
                Resolved = resueltos
            };
        }

        /// Integrantes del equipo con métricas medidas, no estimadas.
        public async Task<List<MiembroEquipoDto>> EquipoAsync(CancellationToken ct = default)
        {
            var agentes = await _agentes.Query()
                .Select(a => new
                {
                    a.IdAgente,
                    Nombre = a.IdUsuarioNavigation.NombreUsuario,
                    Disponible = a.DisponibilidadAgente
                })
                .ToListAsync(ct);

            var filas = await LeerFilasAsync(ct);

            return agentes
                .Select(agente =>
                {
                    var suyos = filas.Where(f => f.IdAgente == agente.IdAgente).ToList();
                    var horas = suyos
                        .Select(f => f.HorasResolucion)
                        .Where(h => h.HasValue)
                        .Select(h => h!.Value)
                        .ToList();

                    return new MiembroEquipoDto
                    {
                        IdAgente = agente.IdAgente,
                        Name = agente.Nombre,
                        Status = agente.Disponible == true ? "available" : "busy",
                        Tickets = suyos.Count(f => !f.Finalizado),
                        AvgTime = horas.Count > 0
                            ? horas.Average().ToString("0.0")
                            : "0.0",

                        // El esquema no tiene encuestas de satisfacción:
                        // se devuelve null y la interfaz muestra "N/D".
                        Satisfaction = null
                    };
                })
                .OrderBy(m => m.Name)
                .ToList();
        }

        /// Comparativa por agente con indicadores verificables.
        public async Task<List<ComparativaAgenteDto>> ComparativaAgentesAsync(CancellationToken ct = default)
        {
            var agentes = await _agentes.Query()
                .Select(a => new { a.IdAgente, Nombre = a.IdUsuarioNavigation.NombreUsuario })
                .ToListAsync(ct);

            var filas = await LeerFilasAsync(ct);

            return agentes
                .Select(agente =>
                {
                    var suyos = filas.Where(f => f.IdAgente == agente.IdAgente).ToList();
                    var resueltos = suyos.Count(f => f.Finalizado);
                    var horas = suyos
                        .Select(f => f.HorasResolucion)
                        .Where(h => h.HasValue)
                        .Select(h => h!.Value)
                        .ToList();

                    return new ComparativaAgenteDto
                    {
                        Name = agente.Nombre,
                        Asignados = suyos.Count,
                        Resueltos = resueltos,
                        Activos = suyos.Count - resueltos,
                        TasaResolucion = suyos.Count == 0
                            ? 0
                            : Math.Round((decimal)resueltos * 100 / suyos.Count, 1),
                        TiempoPromedioHoras = horas.Count > 0
                            ? Math.Round((decimal)horas.Average(), 1)
                            : null
                    };
                })
                .OrderByDescending(c => c.Resueltos)
                .ThenBy(c => c.Name)
                .ToList();
        }

        /// Carga de trabajo por agente (base del reporte PDF).
        public async Task<List<CargaAgenteDto>> CargaPorAgenteAsync(CancellationToken ct = default)
        {
            var agentes = await _agentes.Query()
                .Select(a => new { a.IdAgente, Nombre = a.IdUsuarioNavigation.NombreUsuario })
                .ToListAsync(ct);

            var filas = await LeerFilasAsync(ct);

            return agentes
                .Select(agente =>
                {
                    var suyos = filas.Where(f => f.IdAgente == agente.IdAgente).ToList();
                    var resueltos = suyos.Count(f => f.Finalizado);

                    return new CargaAgenteDto
                    {
                        Nombre = agente.Nombre,
                        TicketsTotales = suyos.Count,
                        TicketsResueltos = resueltos,
                        TicketsActivos = suyos.Count - resueltos
                    };
                })
                .OrderByDescending(c => c.TicketsActivos)
                .ThenBy(c => c.Nombre)
                .ToList();
        }

        /// Resumen para la cabecera del reporte semanal.
        public async Task<ResumenSemanal> ResumenSemanalAsync(CancellationToken ct = default)
        {
            var filas = await LeerFilasAsync(ct);
            var ahora = DateTime.UtcNow;
            var desde = ahora.AddDays(-7);
            var limiteSla = ahora.AddHours(-_sla.HorasVencimiento);

            var delPeriodo = filas.Where(f => f.Creacion.HasValue && f.Creacion.Value >= desde).ToList();
            var finalizados = delPeriodo.Where(f => f.Finalizado).ToList();
            var horas = finalizados
                .Select(f => f.HorasResolucion)
                .Where(h => h.HasValue)
                .Select(h => h!.Value)
                .ToList();

            return new ResumenSemanal(
                Desde: desde,
                Hasta: ahora,
                Creados: delPeriodo.Count,
                Finalizados: finalizados.Count,
                Vencidos: filas.Count(f => !f.Finalizado
                                           && f.Creacion.HasValue
                                           && f.Creacion.Value <= limiteSla),
                SinAsignar: filas.Count(f => !f.Finalizado && f.IdAgente is null),
                HorasPromedioResolucion: horas.Count > 0 ? Math.Round(horas.Average(), 1) : null,
                HorasSla: _sla.HorasVencimiento);
        }

        public sealed record ResumenSemanal(
            DateTime Desde,
            DateTime Hasta,
            int Creados,
            int Finalizados,
            int Vencidos,
            int SinAsignar,
            double? HorasPromedioResolucion,
            int HorasSla);

        private static TicketPanelDto APanel(FilaTicket fila, DateTime ahora) => new()
        {
            Id = fila.IdTicket,
            Title = fila.Titulo,
            Descripcion = fila.Descripcion,
            User = fila.NombreCliente,
            Agent = fila.NombreAgente ?? "Sin asignar",
            IdAgenteAsignado = fila.IdAgente,
            Status = fila.NombreEstado,
            Priority = fila.Prioridad,
            Category = fila.NombreCategoria,
            Time = TiempoRelativo.Formatear(fila.Creacion, ahora),
            FechaHoraCreacionTicket = fila.Creacion,
            FechaHoraActualizacionTicket = fila.Actualizacion
        };
    }
}
