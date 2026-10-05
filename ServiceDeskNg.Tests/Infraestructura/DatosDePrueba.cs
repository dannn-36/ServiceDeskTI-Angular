using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;

namespace ServiceDeskNg.Tests.Infraestructura
{
    /// Base de datos en memoria con un escenario mínimo y realista:
    /// un administrador, un supervisor, dos agentes, dos clientes,
    /// los estados y categorías del script de la base de datos.
    public sealed class DatosDePrueba
    {
        public const string Contrasena = "Clave-Segura-123";

        public string NombreBaseDatos { get; } = "servicedesk-" + Guid.NewGuid();

        public int IdEstadoAbierto { get; private set; }
        public int IdEstadoEnProgreso { get; private set; }
        public int IdEstadoPendiente { get; private set; }
        public int IdEstadoResuelto { get; private set; }

        public int IdCategoriaHardware { get; private set; }
        public int IdCategoriaSoftware { get; private set; }

        public Usuario Admin { get; private set; } = null!;
        public Usuario Supervisor { get; private set; } = null!;
        public Usuario UsuarioAgente1 { get; private set; } = null!;
        public Usuario UsuarioAgente2 { get; private set; } = null!;
        public Usuario UsuarioCliente1 { get; private set; } = null!;
        public Usuario UsuarioCliente2 { get; private set; } = null!;

        public int IdAgente1 { get; private set; }
        public int IdAgente2 { get; private set; }
        public int IdCliente1 { get; private set; }
        public int IdCliente2 { get; private set; }

        public static DbContextOptions<ServiceDeskContext> Opciones(string nombreBaseDatos) =>
            new DbContextOptionsBuilder<ServiceDeskContext>()
                .UseInMemoryDatabase(nombreBaseDatos)
                // El proveedor en memoria no tiene transacciones; se ignoran en las pruebas.
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

        /// Contexto nuevo sobre la misma base: simula una petición HTTP distinta.
        public ServiceDeskContext NuevoContexto() => new(Opciones(NombreBaseDatos));

        public static DatosDePrueba Crear()
        {
            var datos = new DatosDePrueba();
            using var contexto = datos.NuevoContexto();
            datos.Sembrar(contexto);
            return datos;
        }

        /// Los catálogos copian literalmente los datos iniciales de Database/DatabaseScript.txt,
        /// para que las pruebas fallen si el código asume valores que la base real no tiene.
        /// Base con solo los catálogos del script SQL, sin ningún usuario (como recién creada).
        public static DatosDePrueba CrearSoloCatalogos()
        {
            var datos = new DatosDePrueba();
            using var contexto = datos.NuevoContexto();
            datos.SembrarCatalogos(contexto);
            return datos;
        }

        private NivelesAcceso _nivelCliente = null!;
        private NivelesAcceso _nivelAgente = null!;
        private NivelesAcceso _nivelSupervisor = null!;
        private NivelesAcceso _nivelAdmin = null!;

        public void Sembrar(ServiceDeskContext contexto)
        {
            SembrarCatalogos(contexto);
            SembrarUsuarios(contexto);
        }

        public void SembrarCatalogos(ServiceDeskContext contexto)
        {
            var nivelCliente = new NivelesAcceso { Nivel = 1, Nombre = "Acceso básico - Usuarios finales" };
            var nivelAgente = new NivelesAcceso { Nivel = 2, Nombre = "Acceso intermedio - Agentes" };
            var nivelSupervisor = new NivelesAcceso { Nivel = 3, Nombre = "Acceso avanzado - Supervisores" };
            var nivelAdmin = new NivelesAcceso { Nivel = 4, Nombre = "Acceso total - Administradores" };
            contexto.NivelesAccesos.AddRange(nivelCliente, nivelAgente, nivelSupervisor, nivelAdmin);

            var abierto = new TicketsEstado { NombreEstado = "abierto" };
            var enProgreso = new TicketsEstado { NombreEstado = "en-progreso" };
            var pendiente = new TicketsEstado { NombreEstado = "pendiente" };
            var pendienteUsuario = new TicketsEstado { NombreEstado = "pendiente-usuario" };
            var resuelto = new TicketsEstado { NombreEstado = "resuelto" };
            var cerrado = new TicketsEstado { NombreEstado = "cerrado" };
            var reabierto = new TicketsEstado { NombreEstado = "reabierto" };
            contexto.TicketsEstados.AddRange(abierto, enProgreso, pendiente, pendienteUsuario, resuelto, cerrado, reabierto);

            var categorias = new[] { "hardware", "software", "red", "correo", "telefono", "acceso", "otro" }
                .Select(nombre => new TicketsCategoria { NombreCategoria = nombre })
                .ToArray();
            contexto.TicketsCategorias.AddRange(categorias);

            contexto.SaveChanges();

            _nivelCliente = nivelCliente;
            _nivelAgente = nivelAgente;
            _nivelSupervisor = nivelSupervisor;
            _nivelAdmin = nivelAdmin;

            IdEstadoAbierto = abierto.IdEstado;
            IdEstadoEnProgreso = enProgreso.IdEstado;
            IdEstadoPendiente = pendiente.IdEstado;
            IdEstadoResuelto = resuelto.IdEstado;
            IdCategoriaHardware = categorias[0].IdCategoria;
            IdCategoriaSoftware = categorias[1].IdCategoria;
        }

        private void SembrarUsuarios(ServiceDeskContext contexto)
        {
            var nivelCliente = _nivelCliente;
            var nivelAgente = _nivelAgente;
            var nivelSupervisor = _nivelSupervisor;
            var nivelAdmin = _nivelAdmin;

            Admin = NuevoUsuario("Ana Admin", "admin@servicedesk.test");
            Supervisor = NuevoUsuario("Sergio Supervisor", "supervisor@servicedesk.test");
            UsuarioAgente1 = NuevoUsuario("Alberto Agente", "agente1@servicedesk.test");
            UsuarioAgente2 = NuevoUsuario("Beatriz Agente", "agente2@servicedesk.test");
            UsuarioCliente1 = NuevoUsuario("Carla Cliente", "cliente1@servicedesk.test");
            UsuarioCliente2 = NuevoUsuario("Diego Cliente", "cliente2@servicedesk.test");

            contexto.Usuarios.AddRange(
                Admin, Supervisor, UsuarioAgente1, UsuarioAgente2, UsuarioCliente1, UsuarioCliente2);
            contexto.SaveChanges();

            contexto.Administradores.Add(new Administrador { IdUsuario = Admin.IdUsuario, IdNivel = nivelAdmin.IdNivel });
            contexto.Supervisores.Add(new Supervisor { IdUsuario = Supervisor.IdUsuario, IdNivel = nivelSupervisor.IdNivel });

            var agente1 = new Agente { IdUsuario = UsuarioAgente1.IdUsuario, IdNivel = nivelAgente.IdNivel, DisponibilidadAgente = true };
            var agente2 = new Agente { IdUsuario = UsuarioAgente2.IdUsuario, IdNivel = nivelAgente.IdNivel, DisponibilidadAgente = true };
            contexto.Agentes.AddRange(agente1, agente2);

            var cliente1 = new EndUser { IdUsuario = UsuarioCliente1.IdUsuario, IdNivel = nivelCliente.IdNivel };
            var cliente2 = new EndUser { IdUsuario = UsuarioCliente2.IdUsuario, IdNivel = nivelCliente.IdNivel };
            contexto.Clientes.AddRange(cliente1, cliente2);

            contexto.SaveChanges();

            IdAgente1 = agente1.IdAgente;
            IdAgente2 = agente2.IdAgente;
            IdCliente1 = cliente1.IdCliente;
            IdCliente2 = cliente2.IdCliente;
        }

        /// Inserta un ticket directamente en la base (sin pasar por las reglas de negocio).
        public Ticket AgregarTicket(
            int idCliente,
            int? idAgente,
            int idEstado,
            string prioridad = "media",
            DateTime? creado = null,
            DateTime? actualizado = null,
            string titulo = "Ticket de prueba")
        {
            using var contexto = NuevoContexto();

            var ticket = new Ticket
            {
                IdCliente = idCliente,
                IdAgenteAsignado = idAgente,
                IdEstadoTicket = idEstado,
                IdCategoriaTicket = IdCategoriaHardware,
                TituloTicket = titulo,
                DescripcionTicket = "Descripción de prueba",
                PrioridadTicket = prioridad,
                FechaHoraCreacionTicket = creado ?? DateTime.UtcNow,
                FechaHoraActualizacionTicket = actualizado ?? creado ?? DateTime.UtcNow
            };

            contexto.Tickets.Add(ticket);
            contexto.SaveChanges();
            return ticket;
        }

        private static Usuario NuevoUsuario(string nombre, string correo) => new()
        {
            NombreUsuario = nombre,
            CorreoUsuario = correo,
            // Factor de trabajo bajo: las pruebas no necesitan la robustez de producción.
            ContrasenaUsuario = BCrypt.Net.BCrypt.HashPassword(Contrasena, workFactor: 4),
            EstadoUsuario = "activo",
            FechaHoraCreacionUsuario = DateTime.UtcNow
        };
    }
}
