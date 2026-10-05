using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Security;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Data
{
    /// Prepara una base de datos recién creada:
    ///  1. Crea el primer administrador si no hay ninguno (con la cuenta indicada en la configuración).
    ///  2. Opcionalmente, carga un escenario de demostración completo.
    /// Es idempotente: se puede ejecutar en cada arranque sin duplicar nada.
    public class InicializadorBaseDatos
    {
        private const string DominioDemo = "@servicedesk.local";

        private readonly ServiceDeskContext _context;
        private readonly UsuarioService _usuarios;
        private readonly OpcionesInicializacion _opciones;
        private readonly ILogger<InicializadorBaseDatos> _logger;

        public InicializadorBaseDatos(
            ServiceDeskContext context,
            UsuarioService usuarios,
            IOptions<OpcionesInicializacion> opciones,
            ILogger<InicializadorBaseDatos> logger)
        {
            _context = context;
            _usuarios = usuarios;
            _opciones = opciones.Value;
            _logger = logger;
        }

        public async Task EjecutarAsync(CancellationToken ct = default)
        {
            await CrearAdministradorInicialAsync(ct);

            if (_opciones.SembrarDatosDemo)
                await SembrarDatosDemoAsync(ct);
        }

        // ======================================================
        // Primer administrador
        // ======================================================

        private async Task CrearAdministradorInicialAsync(CancellationToken ct)
        {
            if (await _context.Administradores.AnyAsync(ct))
                return;

            if (string.IsNullOrWhiteSpace(_opciones.AdminCorreo) || string.IsNullOrWhiteSpace(_opciones.AdminContrasena))
            {
                _logger.LogWarning(
                    "No hay ningún administrador. Configure {Seccion}:AdminCorreo y {Seccion}:AdminContrasena para crear el primero.",
                    OpcionesInicializacion.Seccion,
                    OpcionesInicializacion.Seccion);
                return;
            }

            await CrearUsuarioSiNoExisteAsync(
                _opciones.AdminNombre,
                _opciones.AdminCorreo,
                _opciones.AdminContrasena,
                RolesApp.Administrador,
                "Tecnología",
                ct);

            _logger.LogInformation("Administrador inicial creado: {Correo}", _opciones.AdminCorreo);
        }

        // ======================================================
        // Escenario de demostración
        // ======================================================

        private sealed record TicketDemo(
            int Cliente,
            int? Agente,
            string Estado,
            string Prioridad,
            string Categoria,
            string Titulo,
            string Descripcion,
            double HorasAtras,
            double? HorasHastaResolver = null,
            string[]? Conversacion = null);

        private async Task SembrarDatosDemoAsync(CancellationToken ct)
        {
            if (await _context.Tickets.AnyAsync(ct))
            {
                _logger.LogInformation("La base ya tiene tickets: no se cargan datos de demostración.");
                return;
            }

            var clave = _opciones.ContrasenaDemo;

            await CrearUsuarioSiNoExisteAsync("Sofía Herrera", "supervisor" + DominioDemo, clave, RolesApp.Supervisor, "Mesa de servicio", ct);

            var agentes = new[]
            {
                await CrearUsuarioSiNoExisteAsync("Andrés Martínez", "agente1" + DominioDemo, clave, RolesApp.Agente, "Soporte N1", ct),
                await CrearUsuarioSiNoExisteAsync("Beatriz Gómez", "agente2" + DominioDemo, clave, RolesApp.Agente, "Soporte N1", ct),
                await CrearUsuarioSiNoExisteAsync("Carlos Ruiz", "agente3" + DominioDemo, clave, RolesApp.Agente, "Redes", ct)
            };

            var clientes = new[]
            {
                await CrearUsuarioSiNoExisteAsync("María López", "cliente1" + DominioDemo, clave, RolesApp.Cliente, "Finanzas", ct),
                await CrearUsuarioSiNoExisteAsync("Jorge Pérez", "cliente2" + DominioDemo, clave, RolesApp.Cliente, "Ventas", ct),
                await CrearUsuarioSiNoExisteAsync("Lucía Fernández", "cliente3" + DominioDemo, clave, RolesApp.Cliente, "Recursos Humanos", ct),
                await CrearUsuarioSiNoExisteAsync("Pedro Sánchez", "cliente4" + DominioDemo, clave, RolesApp.Cliente, "Logística", ct)
            };

            var idsAgente = new List<int>();
            foreach (var idUsuario in agentes)
                idsAgente.Add(await _context.Agentes.Where(a => a.IdUsuario == idUsuario).Select(a => a.IdAgente).FirstAsync(ct));

            var idsCliente = new List<int>();
            foreach (var idUsuario in clientes)
                idsCliente.Add(await _context.Clientes.Where(c => c.IdUsuario == idUsuario).Select(c => c.IdCliente).FirstAsync(ct));

            // El tercer agente aparece como no disponible: no recibe tickets en el reparto automático.
            var agenteNoDisponible = await _context.Agentes.FirstAsync(a => a.IdAgente == idsAgente[2], ct);
            agenteNoDisponible.DisponibilidadAgente = false;
            agenteNoDisponible.EspecialidadAgente = "Redes y comunicaciones";
            await _context.SaveChangesAsync(ct);

            var estados = await _context.TicketsEstados
                .ToDictionaryAsync(e => e.NombreEstado.Trim().ToLower(), e => e.IdEstado, ct);
            var categorias = await _context.TicketsCategorias
                .ToDictionaryAsync(c => c.NombreCategoria.Trim().ToLower(), c => c.IdCategoria, ct);

            var ahora = DateTime.UtcNow;
            var creados = 0;

            foreach (var demo in EscenarioDemo())
            {
                if (!estados.TryGetValue(demo.Estado, out var idEstado)
                    || !categorias.TryGetValue(demo.Categoria, out var idCategoria))
                {
                    _logger.LogWarning("Se omite el ticket de demostración '{Titulo}': falta su estado o categoría en el catálogo.", demo.Titulo);
                    continue;
                }

                var creado = ahora.AddHours(-demo.HorasAtras);
                var actualizado = demo.HorasHastaResolver is double horas
                    ? creado.AddHours(horas)
                    : creado.AddHours(demo.HorasAtras * 0.4);

                var ticket = new Ticket
                {
                    IdCliente = idsCliente[demo.Cliente],
                    IdAgenteAsignado = demo.Agente is int indice ? idsAgente[indice] : null,
                    IdEstadoTicket = idEstado,
                    IdCategoriaTicket = idCategoria,
                    PrioridadTicket = demo.Prioridad,
                    TituloTicket = demo.Titulo,
                    DescripcionTicket = demo.Descripcion,
                    FechaHoraCreacionTicket = creado,
                    FechaHoraActualizacionTicket = actualizado
                };

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync(ct);

                if (demo.Conversacion is { Length: > 0 } conversacion)
                {
                    var autorCliente = clientes[demo.Cliente];
                    var autorAgente = demo.Agente is int a ? agentes[a] : autorCliente;

                    for (var i = 0; i < conversacion.Length; i++)
                    {
                        _context.TicketMensajes.Add(new TicketMensaje
                        {
                            IdTicket = ticket.IdTicket,
                            IdUsuario = i % 2 == 0 ? autorCliente : autorAgente,
                            MensajeTicket = conversacion[i],
                            FechaHoraCreacionMensaje = creado.AddMinutes(10 + i * 7)
                        });
                    }

                    await _context.SaveChangesAsync(ct);
                }

                creados++;
            }

            _logger.LogInformation("Datos de demostración cargados: {Cantidad} tickets.", creados);
        }

        /// Escenario variado: tickets activos, resueltos, vencidos según SLA,
        /// urgentes sin asignar y conversaciones, para que todos los paneles tengan contenido.
        private static IEnumerable<TicketDemo> EscenarioDemo() =>
        [
            new(0, 0, "en-progreso", "alta", "hardware", "El portátil no enciende",
                "Desde esta mañana el portátil no enciende aunque el cargador está conectado.", 5,
                Conversacion:
                [
                    "Hola, el portátil no enciende desde esta mañana.",
                    "Hola María, ¿se enciende alguna luz al conectar el cargador?",
                    "Solo parpadea la luz naranja.",
                    "Parece la batería. Pasa por el puesto de soporte y te dejo un equipo de préstamo."
                ]),
            new(1, 1, "abierto", "media", "software", "Excel se cierra al abrir archivos con macros",
                "Cada vez que abro el reporte de ventas con macros, Excel se cierra sin mostrar ningún error.", 3),
            new(2, 0, "resuelto", "media", "red", "Sin acceso a la VPN",
                "No puedo conectarme a la VPN desde casa; el cliente se queda en 'Conectando...'.", 30, 4,
                [
                    "No consigo conectarme a la VPN.",
                    "Hemos renovado tu certificado. Prueba de nuevo, por favor.",
                    "¡Ya funciona, gracias!"
                ]),
            new(3, null, "abierto", "urgente", "acceso", "Cuenta bloqueada en el sistema de nóminas",
                "Mi cuenta quedó bloqueada y hoy hay que cerrar la nómina.", 1),
            new(0, 1, "pendiente-usuario", "baja", "correo", "Configurar la firma del correo",
                "Necesito la firma corporativa nueva en Outlook.", 26),
            new(1, 0, "abierto", "media", "hardware", "Impresora del piso 2 atascada",
                "La impresora del segundo piso muestra un atasco aunque no hay papel dentro.", 60),
            new(2, 1, "resuelto", "alta", "software", "Instalación de Visual Studio Code",
                "Solicito la instalación de Visual Studio Code para el equipo de analítica.", 50, 6),
            new(3, 0, "resuelto", "baja", "telefono", "Extensión telefónica sin tono",
                "La extensión 2214 no tiene tono desde ayer.", 75, 2),
            new(0, 2, "en-progreso", "urgente", "red", "Caída de la red en la sala de juntas",
                "No hay red cableada ni wifi en la sala de juntas y hay una reunión con clientes a las 16:00.", 8,
                Conversacion:
                [
                    "¡Se cayó la red en la sala de juntas!",
                    "Estoy revisando el switch de la planta. Te aviso en 15 minutos."
                ]),
            new(1, null, "abierto", "alta", "acceso", "Permisos para la carpeta compartida de Finanzas",
                "Necesito acceso de lectura a la carpeta de Finanzas para el cierre trimestral.", 2),
            new(2, 1, "cerrado", "media", "otro", "Solicitud de segundo monitor",
                "Pido un segundo monitor para el puesto de selección de personal.", 120, 20),
            new(3, 1, "en-progreso", "media", "software", "Actualizar el antivirus",
                "El antivirus avisa de que la licencia caduca en tres días.", 10),
            new(0, 0, "resuelto", "urgente", "correo", "No llegan correos externos",
                "Desde las 9:00 no recibimos correos de clientes.", 100, 3),
            new(1, 2, "pendiente", "alta", "hardware", "Teclado con teclas que no responden",
                "Varias teclas del teclado no responden (A, S y la barra espaciadora).", 55),
            new(2, 0, "reabierto", "media", "software", "El error de Excel volvió a aparecer",
                "El problema de Excel que se resolvió la semana pasada ha vuelto.", 20),
            new(3, null, "abierto", "baja", "otro", "Cambio de fondo de pantalla corporativo",
                "¿Se puede poner el nuevo fondo corporativo en los equipos de logística?", 0.5)
        ];

        // ======================================================
        // Apoyo
        // ======================================================

        /// Crea el usuario con su rol si el correo no existe. Devuelve su id.
        private async Task<int> CrearUsuarioSiNoExisteAsync(
            string nombre,
            string correo,
            string contrasena,
            string rol,
            string departamento,
            CancellationToken ct)
        {
            var existente = await _context.Usuarios
                .Where(u => u.CorreoUsuario == correo)
                .Select(u => (int?)u.IdUsuario)
                .FirstOrDefaultAsync(ct);

            if (existente is int id)
                return id;

            var creado = await _usuarios.CrearConRolAsync(new UsuarioCreateDto
            {
                NombreUsuario = nombre,
                CorreoUsuario = correo,
                ContrasenaUsuario = contrasena,
                TipoUsuario = rol,
                DepartamentoUsuario = departamento,
                EstadoUsuario = "activo"
            }, ct);

            return creado.IdUsuario;
        }
    }
}
