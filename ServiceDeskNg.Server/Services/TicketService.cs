using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Services
{
    /// Reglas de negocio de los tickets: alta, cambios, baja, asignación de agentes
    /// y balanceo de carga. Los reportes y paneles viven en MetricasService.
    public class TicketService
    {
        private readonly IRepositorio<Ticket> _tickets;
        private readonly IRepositorio<Agente> _agentes;
        private readonly IRepositorio<EndUser> _clientes;
        private readonly CatalogoTicketsService _catalogo;
        private readonly ILogger<TicketService> _logger;

        public TicketService(
            IRepositorio<Ticket> tickets,
            IRepositorio<Agente> agentes,
            IRepositorio<EndUser> clientes,
            CatalogoTicketsService catalogo,
            ILogger<TicketService> logger)
        {
            _tickets = tickets;
            _agentes = agentes;
            _clientes = clientes;
            _catalogo = catalogo;
            _logger = logger;
        }

        // ======================================================
        // Consultas
        // ======================================================

        public Task<List<TicketDto>> ListarAsync(CancellationToken ct = default) =>
            ConsultaDto()
                .OrderByDescending(t => t.FechaHoraCreacionTicket)
                .ToListAsync(ct);

        public async Task<TicketDto> ObtenerAsync(int id, CancellationToken ct = default)
        {
            var ticket = await ConsultaDto().FirstOrDefaultAsync(t => t.IdTicket == id, ct);
            return ticket ?? throw new KeyNotFoundException($"No se encontró el ticket con ID {id}");
        }

        public async Task<List<TicketDto>> ListarPorClienteAsync(int idCliente, CancellationToken ct = default)
        {
            if (!await _clientes.Query().AnyAsync(c => c.IdCliente == idCliente, ct))
                throw new KeyNotFoundException($"No se encontró el cliente con ID {idCliente}");

            return await ConsultaDto()
                .Where(t => t.IdCliente == idCliente)
                .OrderByDescending(t => t.FechaHoraCreacionTicket)
                .ToListAsync(ct);
        }

        public async Task<List<TicketDto>> ListarPorAgenteAsync(int idAgente, CancellationToken ct = default)
        {
            if (!await _agentes.Query().AnyAsync(a => a.IdAgente == idAgente, ct))
                throw new KeyNotFoundException($"No se encontró el agente con ID {idAgente}");

            return await ConsultaDto()
                .Where(t => t.IdAgenteAsignado == idAgente)
                .OrderByDescending(t => t.FechaHoraCreacionTicket)
                .ToListAsync(ct);
        }

        /// Comprueba que quien pide el ticket tenga algo que ver con él.
        /// Administración y supervisión ven todos; el cliente, solo los suyos;
        /// el agente, solo los que tiene asignados.
        public async Task AsegurarAccesoAsync(
            int idTicket,
            bool visionGlobal,
            int? idCliente,
            int? idAgente,
            CancellationToken ct = default)
        {
            if (visionGlobal)
            {
                if (!await _tickets.Query().AnyAsync(t => t.IdTicket == idTicket, ct))
                    throw new KeyNotFoundException($"No se encontró el ticket con ID {idTicket}");
                return;
            }

            var datos = await _tickets.Query()
                .Where(t => t.IdTicket == idTicket)
                .Select(t => new { t.IdCliente, t.IdAgenteAsignado })
                .FirstOrDefaultAsync(ct)
                ?? throw new KeyNotFoundException($"No se encontró el ticket con ID {idTicket}");

            var esSuyoComoCliente = idCliente is int cliente && datos.IdCliente == cliente;
            var esSuyoComoAgente = idAgente is int agente && datos.IdAgenteAsignado == agente;

            if (!esSuyoComoCliente && !esSuyoComoAgente)
                throw new AccesoDenegadoException("No tiene acceso a este ticket.");
        }

        // ======================================================
        // Alta, cambios y baja
        // ======================================================

        /// Crea el ticket a nombre de `idCliente` y le asigna automáticamente
        /// el agente disponible con menos carga activa.
        public async Task<TicketDto> CrearAsync(
            TicketCreateDto dto,
            int idCliente,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (!await _clientes.Query().AnyAsync(c => c.IdCliente == idCliente, ct))
                throw new KeyNotFoundException($"No se encontró el cliente con ID {idCliente}");

            if (!await _catalogo.ExisteCategoriaAsync(dto.IdCategoriaTicket, ct))
                throw new ArgumentException("La categoría indicada no existe.");

            var idEstado = dto.IdEstadoTicket is int estadoPedido && estadoPedido > 0
                ? (await _catalogo.ExisteEstadoAsync(estadoPedido, ct)
                    ? estadoPedido
                    : throw new ArgumentException("El estado indicado no existe."))
                : await _catalogo.IdEstadoAbiertoAsync(ct);

            var ahora = DateTime.UtcNow;

            var ticket = new Ticket
            {
                IdCliente = idCliente,
                IdCategoriaTicket = dto.IdCategoriaTicket,
                IdEstadoTicket = idEstado,
                TituloTicket = dto.TituloTicket.Trim(),
                DescripcionTicket = dto.DescripcionTicket.Trim(),
                PrioridadTicket = NormalizarPrioridad(dto.PrioridadTicket),
                UbicacionTicket = dto.UbicacionTicket,
                DepartamentoTicket = dto.DepartamentoTicket,
                FechaHoraCreacionTicket = ahora,
                FechaHoraActualizacionTicket = ahora,
                IdAgenteAsignado = await ElegirAgenteConMenosCargaAsync(ct)
            };

            await _tickets.AddAsync(ticket, ct);

            _logger.LogInformation(
                "Ticket {IdTicket} creado por el cliente {IdCliente}; agente asignado: {IdAgente}",
                ticket.IdTicket,
                idCliente,
                ticket.IdAgenteAsignado?.ToString() ?? "ninguno disponible");

            return await ObtenerAsync(ticket.IdTicket, ct);
        }

        /// Actualiza los datos editables del ticket.
        /// El agente asignado no se toca aquí: para eso está AsignarAgenteAsync.
        public async Task ActualizarAsync(int id, TicketUpdateDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var ticket = await _tickets.QueryParaEscritura()
                .FirstOrDefaultAsync(t => t.IdTicket == id, ct)
                ?? throw new KeyNotFoundException($"No se encontró el ticket con ID {id}");

            if (!await _catalogo.ExisteEstadoAsync(dto.IdEstadoTicket, ct))
                throw new ArgumentException("El estado indicado no existe.");

            if (!await _catalogo.ExisteCategoriaAsync(dto.IdCategoriaTicket, ct))
                throw new ArgumentException("La categoría indicada no existe.");

            ticket.TituloTicket = dto.TituloTicket.Trim();
            ticket.DescripcionTicket = dto.DescripcionTicket.Trim();
            ticket.IdEstadoTicket = dto.IdEstadoTicket;
            ticket.IdCategoriaTicket = dto.IdCategoriaTicket;
            ticket.PrioridadTicket = NormalizarPrioridad(dto.PrioridadTicket);
            ticket.UbicacionTicket = dto.UbicacionTicket;
            ticket.DepartamentoTicket = dto.DepartamentoTicket;
            ticket.FechaHoraActualizacionTicket = DateTime.UtcNow;

            await _tickets.GuardarCambiosAsync(ct);
        }

        public async Task EliminarAsync(int id, CancellationToken ct = default)
        {
            if (!await _tickets.Query().AnyAsync(t => t.IdTicket == id, ct))
                throw new KeyNotFoundException($"No se encontró el ticket con ID {id}");

            await _tickets.DeleteAsync(id, ct);
        }

        // ======================================================
        // Asignación y balanceo
        // ======================================================

        public async Task AsignarAgenteAsync(int idTicket, int idAgente, CancellationToken ct = default)
        {
            var ticket = await _tickets.QueryParaEscritura()
                .FirstOrDefaultAsync(t => t.IdTicket == idTicket, ct)
                ?? throw new KeyNotFoundException($"No se encontró el ticket con ID {idTicket}");

            if (!await _agentes.Query().AnyAsync(a => a.IdAgente == idAgente, ct))
                throw new KeyNotFoundException($"No se encontró el agente con ID {idAgente}");

            ticket.IdAgenteAsignado = idAgente;
            ticket.FechaHoraActualizacionTicket = DateTime.UtcNow;
            await _tickets.GuardarCambiosAsync(ct);
        }

        public async Task EscalarCategoriaAsync(
            int idTicket,
            string nuevaCategoria,
            CancellationToken ct = default)
        {
            var ticket = await _tickets.QueryParaEscritura()
                .FirstOrDefaultAsync(t => t.IdTicket == idTicket, ct)
                ?? throw new KeyNotFoundException($"No se encontró el ticket con ID {idTicket}");

            var categoria = await _catalogo.CategoriaPorNombreAsync(nuevaCategoria, ct);

            ticket.IdCategoriaTicket = categoria.IdCategoria;
            ticket.FechaHoraActualizacionTicket = DateTime.UtcNow;
            await _tickets.GuardarCambiosAsync(ct);
        }

        /// Reparte los tickets que nadie tiene asignado entre los agentes disponibles,
        /// dando cada uno al que menos carga activa tenga en ese momento.
        public async Task<int> AsignarSinAgenteAsync(CancellationToken ct = default)
        {
            var agentes = await AgentesDisponiblesAsync(ct);
            if (agentes.Count == 0)
                throw new ConflictoNegocioException("No hay agentes disponibles para asignar tickets.");

            var idsEstadosActivos = await _catalogo.IdsEstadosActivosAsync(ct);

            var sinAsignar = await _tickets.QueryParaEscritura()
                .Where(t => t.IdAgenteAsignado == null && idsEstadosActivos.Contains(t.IdEstadoTicket))
                .OrderBy(t => t.FechaHoraCreacionTicket)
                .ToListAsync(ct);

            if (sinAsignar.Count == 0)
                return 0;

            var carga = await CargaActivaPorAgenteAsync(agentes, idsEstadosActivos, ct);
            var ahora = DateTime.UtcNow;

            foreach (var ticket in sinAsignar)
            {
                var idAgente = carga.OrderBy(par => par.Value).ThenBy(par => par.Key).First().Key;
                ticket.IdAgenteAsignado = idAgente;
                ticket.FechaHoraActualizacionTicket = ahora;
                carga[idAgente]++;
            }

            await _tickets.GuardarCambiosAsync(ct);

            _logger.LogInformation("Se asignaron {Cantidad} tickets sin agente", sinAsignar.Count);
            return sinAsignar.Count;
        }

        /// Equilibra la carga entre agentes disponibles.
        /// Primero reparte lo que está sin asignar y después mueve tickets que
        /// todavía nadie ha empezado (estado "Abierto") desde los agentes más
        /// cargados hacia los más libres, hasta que la diferencia sea de un ticket.
        /// No mueve tickets en progreso para no interrumpir conversaciones en curso.
        public async Task<int> RedistribuirAsync(CancellationToken ct = default)
        {
            var agentes = await AgentesDisponiblesAsync(ct);
            if (agentes.Count == 0)
                throw new ConflictoNegocioException("No hay agentes disponibles para redistribuir tickets.");

            var movimientos = await AsignarSinAgenteAsync(ct);

            if (agentes.Count < 2)
                return movimientos;

            var idsEstadosActivos = await _catalogo.IdsEstadosActivosAsync(ct);
            var idEstadoAbierto = await _catalogo.IdEstadoAbiertoAsync(ct);

            var activos = await _tickets.QueryParaEscritura()
                .Where(t => t.IdAgenteAsignado != null && idsEstadosActivos.Contains(t.IdEstadoTicket))
                .ToListAsync(ct);

            var carga = await CargaActivaPorAgenteAsync(agentes, idsEstadosActivos, ct);
            var ahora = DateTime.UtcNow;

            // Cada vuelta mueve como máximo un ticket, y nunca más vueltas que tickets activos.
            for (var vuelta = 0; vuelta < activos.Count; vuelta++)
            {
                var masCargado = carga.OrderByDescending(par => par.Value).ThenBy(par => par.Key).First();
                var masLibre = carga.OrderBy(par => par.Value).ThenBy(par => par.Key).First();

                if (masCargado.Value - masLibre.Value <= 1)
                    break;

                var movible = activos
                    .Where(t => t.IdAgenteAsignado == masCargado.Key
                                && t.IdEstadoTicket == idEstadoAbierto)
                    .OrderByDescending(t => t.FechaHoraCreacionTicket)
                    .FirstOrDefault();

                if (movible is null)
                {
                    // Ese agente no tiene tickets movibles: se le saca del reparto
                    // para poder seguir equilibrando al resto.
                    carga.Remove(masCargado.Key);
                    if (carga.Count < 2)
                        break;
                    continue;
                }

                movible.IdAgenteAsignado = masLibre.Key;
                movible.FechaHoraActualizacionTicket = ahora;
                carga[masCargado.Key]--;
                carga[masLibre.Key]++;
                movimientos++;
            }

            await _tickets.GuardarCambiosAsync(ct);

            _logger.LogInformation("Redistribución terminada: {Cantidad} tickets movidos", movimientos);
            return movimientos;
        }

        /// Agente disponible con menos tickets activos, o null si no hay ninguno disponible.
        public async Task<int?> ElegirAgenteConMenosCargaAsync(CancellationToken ct = default)
        {
            var agentes = await AgentesDisponiblesAsync(ct);
            if (agentes.Count == 0)
            {
                _logger.LogWarning("No hay agentes disponibles: el ticket queda sin asignar");
                return null;
            }

            var idsEstadosActivos = await _catalogo.IdsEstadosActivosAsync(ct);
            var carga = await CargaActivaPorAgenteAsync(agentes, idsEstadosActivos, ct);

            return carga.OrderBy(par => par.Value).ThenBy(par => par.Key).First().Key;
        }

        // ======================================================
        // Apoyo
        // ======================================================

        private Task<List<int>> AgentesDisponiblesAsync(CancellationToken ct) =>
            _agentes.Query()
                .Where(a => a.DisponibilidadAgente == true)
                .Select(a => a.IdAgente)
                .ToListAsync(ct);

        private async Task<Dictionary<int, int>> CargaActivaPorAgenteAsync(
            List<int> idsAgentes,
            List<int> idsEstadosActivos,
            CancellationToken ct)
        {
            var conteos = await _tickets.Query()
                .Where(t => t.IdAgenteAsignado != null
                            && idsAgentes.Contains(t.IdAgenteAsignado.Value)
                            && idsEstadosActivos.Contains(t.IdEstadoTicket))
                .GroupBy(t => t.IdAgenteAsignado!.Value)
                .Select(grupo => new { IdAgente = grupo.Key, Cantidad = grupo.Count() })
                .ToListAsync(ct);

            return idsAgentes.ToDictionary(
                id => id,
                id => conteos.FirstOrDefault(c => c.IdAgente == id)?.Cantidad ?? 0);
        }

        private static string NormalizarPrioridad(string? prioridad)
        {
            if (string.IsNullOrWhiteSpace(prioridad))
                return PrioridadesTicket.Media;

            var normalizada = prioridad.Trim().ToLowerInvariant();

            return PrioridadesTicket.EsValida(normalizada)
                ? normalizada
                : throw new ArgumentException(
                    $"Prioridad inválida. Valores permitidos: {string.Join(", ", PrioridadesTicket.Todas)}.");
        }

        /// Proyección a DTO: evita devolver entidades de EF con sus grafos de navegación.
        private IQueryable<TicketDto> ConsultaDto() =>
            _tickets.Query().Select(t => new TicketDto
            {
                IdTicket = t.IdTicket,
                IdCliente = t.IdCliente,
                IdAgenteAsignado = t.IdAgenteAsignado,
                IdEstadoTicket = t.IdEstadoTicket,
                IdCategoriaTicket = t.IdCategoriaTicket,
                TituloTicket = t.TituloTicket,
                DescripcionTicket = t.DescripcionTicket,
                PrioridadTicket = t.PrioridadTicket,
                UbicacionTicket = t.UbicacionTicket,
                DepartamentoTicket = t.DepartamentoTicket,
                FechaHoraCreacionTicket = t.FechaHoraCreacionTicket,
                FechaHoraActualizacionTicket = t.FechaHoraActualizacionTicket,
                NombreEstado = t.IdEstadoTicketNavigation!.NombreEstado,
                NombreCategoria = t.IdCategoriaTicketNavigation!.NombreCategoria,
                NombreCliente = t.IdClienteNavigation!.IdUsuarioNavigation.NombreUsuario,
                NombreAgente = t.IdAgenteAsignadoNavigation == null
                    ? null
                    : t.IdAgenteAsignadoNavigation.IdUsuarioNavigation.NombreUsuario
            });
    }
}
