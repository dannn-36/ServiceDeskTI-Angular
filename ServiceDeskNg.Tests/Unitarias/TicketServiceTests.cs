using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Tests.Infraestructura;

namespace ServiceDeskNg.Tests.Unitarias
{
    public class TicketServiceTests
    {
        private readonly DatosDePrueba _datos = DatosDePrueba.Crear();

        private TicketCreateDto NuevoTicketDto(string prioridad = "media") => new()
        {
            TituloTicket = "No enciende el monitor",
            DescripcionTicket = "El monitor de la sala 3 no da imagen",
            IdCategoriaTicket = _datos.IdCategoriaHardware,
            PrioridadTicket = prioridad
        };

        [Fact]
        public async Task Crear_AsignaAlAgenteDisponibleConMenosCargaActiva()
        {
            // El agente 1 ya tiene dos tickets activos; el agente 2 ninguno.
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoAbierto);
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoEnProgreso);

            await using var contexto = _datos.NuevoContexto();
            var creado = await FabricaServicios.Tickets(contexto)
                .CrearAsync(NuevoTicketDto(), _datos.IdCliente2);

            Assert.Equal(_datos.IdAgente2, creado.IdAgenteAsignado);
            Assert.Equal(_datos.IdEstadoAbierto, creado.IdEstadoTicket);
            Assert.Equal("abierto", creado.NombreEstado);
            Assert.Equal("Diego Cliente", creado.NombreCliente);
        }

        [Fact]
        public async Task Crear_LosTicketsResueltosNoCuentanComoCarga()
        {
            // El agente 1 solo tiene trabajo terminado; el agente 2 tiene uno activo.
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoResuelto);
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoResuelto);
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente2, _datos.IdEstadoAbierto);

            await using var contexto = _datos.NuevoContexto();
            var creado = await FabricaServicios.Tickets(contexto)
                .CrearAsync(NuevoTicketDto(), _datos.IdCliente1);

            Assert.Equal(_datos.IdAgente1, creado.IdAgenteAsignado);
        }

        [Fact]
        public async Task Crear_LosTicketsReabiertosYEnEsperaDelUsuarioCuentanComoCarga()
        {
            int IdEstado(string nombre)
            {
                using var contexto = _datos.NuevoContexto();
                return contexto.TicketsEstados.Single(e => e.NombreEstado == nombre).IdEstado;
            }

            // El agente 1 tiene dos tickets vivos en estados menos habituales del script.
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, IdEstado("reabierto"));
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, IdEstado("pendiente-usuario"));
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente2, _datos.IdEstadoAbierto);

            await using var contextoServicio = _datos.NuevoContexto();
            var creado = await FabricaServicios.Tickets(contextoServicio)
                .CrearAsync(NuevoTicketDto(), _datos.IdCliente1);

            Assert.Equal(_datos.IdAgente2, creado.IdAgenteAsignado);
        }

        [Fact]
        public async Task Crear_SinAgentesDisponibles_QuedaSinAsignar()
        {
            await using (var contexto = _datos.NuevoContexto())
            {
                foreach (var agente in contexto.Agentes)
                    agente.DisponibilidadAgente = false;
                await contexto.SaveChangesAsync();
            }

            await using var otroContexto = _datos.NuevoContexto();
            var creado = await FabricaServicios.Tickets(otroContexto)
                .CrearAsync(NuevoTicketDto(), _datos.IdCliente1);

            Assert.Null(creado.IdAgenteAsignado);
        }

        [Fact]
        public async Task Crear_ConPrioridadInvalida_LanzaArgumentException()
        {
            await using var contexto = _datos.NuevoContexto();
            var servicio = FabricaServicios.Tickets(contexto);

            var error = await Assert.ThrowsAsync<ArgumentException>(
                () => servicio.CrearAsync(NuevoTicketDto(prioridad: "urgent"), _datos.IdCliente1));

            Assert.Contains("Prioridad inválida", error.Message);
        }

        [Fact]
        public async Task Crear_NormalizaLaPrioridadAMinusculas()
        {
            await using var contexto = _datos.NuevoContexto();
            var creado = await FabricaServicios.Tickets(contexto)
                .CrearAsync(NuevoTicketDto(prioridad: "URGENTE"), _datos.IdCliente1);

            Assert.Equal("urgente", creado.PrioridadTicket);
        }

        [Fact]
        public async Task AsignarSinAgente_RepartePorIgualEntreAgentesDisponibles()
        {
            for (var i = 0; i < 4; i++)
                _datos.AgregarTicket(_datos.IdCliente1, null, _datos.IdEstadoAbierto);

            await using (var contexto = _datos.NuevoContexto())
            {
                var asignados = await FabricaServicios.Tickets(contexto).AsignarSinAgenteAsync();
                Assert.Equal(4, asignados);
            }

            await using var verificacion = _datos.NuevoContexto();
            var porAgente = await verificacion.Tickets
                .GroupBy(t => t.IdAgenteAsignado)
                .Select(g => new { g.Key, Cantidad = g.Count() })
                .ToListAsync();

            Assert.DoesNotContain(porAgente, g => g.Key == null);
            Assert.All(porAgente, g => Assert.Equal(2, g.Cantidad));
        }

        [Fact]
        public async Task Redistribuir_EquilibraLaCargaSinMoverTicketsEnProgreso()
        {
            // Agente 1: cuatro abiertos y uno en progreso. Agente 2: nada.
            for (var i = 0; i < 4; i++)
                _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoAbierto);
            var enCurso = _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoEnProgreso);

            await using (var contexto = _datos.NuevoContexto())
            {
                var movidos = await FabricaServicios.Tickets(contexto).RedistribuirAsync();
                Assert.Equal(2, movidos);
            }

            await using var verificacion = _datos.NuevoContexto();
            var cargaAgente1 = await verificacion.Tickets.CountAsync(t => t.IdAgenteAsignado == _datos.IdAgente1);
            var cargaAgente2 = await verificacion.Tickets.CountAsync(t => t.IdAgenteAsignado == _datos.IdAgente2);

            Assert.True(Math.Abs(cargaAgente1 - cargaAgente2) <= 1);

            var ticketEnCurso = await verificacion.Tickets.SingleAsync(t => t.IdTicket == enCurso.IdTicket);
            Assert.Equal(_datos.IdAgente1, ticketEnCurso.IdAgenteAsignado);
        }

        [Fact]
        public async Task AsignarAgente_PersisteElCambio()
        {
            var ticket = _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoAbierto);

            await using (var contexto = _datos.NuevoContexto())
                await FabricaServicios.Tickets(contexto).AsignarAgenteAsync(ticket.IdTicket, _datos.IdAgente2);

            await using var verificacion = _datos.NuevoContexto();
            var guardado = await verificacion.Tickets.SingleAsync(t => t.IdTicket == ticket.IdTicket);
            Assert.Equal(_datos.IdAgente2, guardado.IdAgenteAsignado);
        }

        [Fact]
        public async Task AsignarAgente_Inexistente_LanzaKeyNotFound()
        {
            var ticket = _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoAbierto);

            await using var contexto = _datos.NuevoContexto();
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => FabricaServicios.Tickets(contexto).AsignarAgenteAsync(ticket.IdTicket, 9999));
        }

        [Fact]
        public async Task AsegurarAcceso_ClienteAjeno_EsDenegado()
        {
            var ticket = _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoAbierto);

            await using var contexto = _datos.NuevoContexto();
            var servicio = FabricaServicios.Tickets(contexto);

            await Assert.ThrowsAsync<AccesoDenegadoException>(() => servicio.AsegurarAccesoAsync(
                ticket.IdTicket, visionGlobal: false, idCliente: _datos.IdCliente2, idAgente: null));
        }

        [Fact]
        public async Task AsegurarAcceso_DuenoAgenteAsignadoYSupervision_TienenAcceso()
        {
            var ticket = _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoAbierto);

            await using var contexto = _datos.NuevoContexto();
            var servicio = FabricaServicios.Tickets(contexto);

            await servicio.AsegurarAccesoAsync(ticket.IdTicket, false, _datos.IdCliente1, null);
            await servicio.AsegurarAccesoAsync(ticket.IdTicket, false, null, _datos.IdAgente1);
            await servicio.AsegurarAccesoAsync(ticket.IdTicket, true, null, null);

            await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
                servicio.AsegurarAccesoAsync(ticket.IdTicket, false, null, _datos.IdAgente2));
        }

        [Fact]
        public async Task Actualizar_NoCambiaElAgenteNiLaFechaDeCreacion()
        {
            var creado = DateTime.UtcNow.AddDays(-2);
            var ticket = _datos.AgregarTicket(
                _datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoAbierto, creado: creado);

            await using (var contexto = _datos.NuevoContexto())
            {
                await FabricaServicios.Tickets(contexto).ActualizarAsync(ticket.IdTicket, new TicketUpdateDto
                {
                    TituloTicket = "Título corregido",
                    DescripcionTicket = "Descripción corregida",
                    IdEstadoTicket = _datos.IdEstadoResuelto,
                    IdCategoriaTicket = _datos.IdCategoriaSoftware,
                    PrioridadTicket = "alta"
                });
            }

            await using var verificacion = _datos.NuevoContexto();
            var guardado = await verificacion.Tickets.SingleAsync(t => t.IdTicket == ticket.IdTicket);

            Assert.Equal("Título corregido", guardado.TituloTicket);
            Assert.Equal(_datos.IdEstadoResuelto, guardado.IdEstadoTicket);
            Assert.Equal(_datos.IdAgente1, guardado.IdAgenteAsignado);
            Assert.Equal(creado, guardado.FechaHoraCreacionTicket!.Value, TimeSpan.FromSeconds(1));
            Assert.True(guardado.FechaHoraActualizacionTicket > creado);
        }
    }
}
