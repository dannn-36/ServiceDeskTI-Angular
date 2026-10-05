using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Tests.Infraestructura
{
    /// Construye los servicios reales sobre un contexto de prueba,
    /// con el mismo cableado que hace el contenedor de dependencias.
    public static class FabricaServicios
    {
        public static CatalogoTicketsService Catalogo(ServiceDeskContext c) =>
            new(new EfRepository<TicketsEstado>(c), new EfRepository<TicketsCategoria>(c));

        public static TicketService Tickets(ServiceDeskContext c) =>
            new(
                new EfRepository<Ticket>(c),
                new EfRepository<Agente>(c),
                new EfRepository<EndUser>(c),
                Catalogo(c),
                NullLogger<TicketService>.Instance);

        public static MetricasService Metricas(ServiceDeskContext c, int horasSla = 48) =>
            new(
                new EfRepository<Ticket>(c),
                new EfRepository<Agente>(c),
                Catalogo(c),
                Options.Create(new OpcionesSla { HorasVencimiento = horasSla }));

        public static SesionService Sesiones(ServiceDeskContext c) =>
            new(new EfRepository<Sesion>(c), new EfRepository<Usuario>(c));

        public static UsuarioService Usuarios(ServiceDeskContext c) =>
            new(
                c,
                new EfRepository<Usuario>(c),
                new EfRepository<NivelesAcceso>(c),
                Sesiones(c),
                NullLogger<UsuarioService>.Instance);
    }
}
