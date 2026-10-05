using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Tests.Infraestructura;

namespace ServiceDeskNg.Tests.Unitarias
{
    public class MetricasServiceTests
    {
        private readonly DatosDePrueba _datos = DatosDePrueba.Crear();

        [Fact]
        public async Task Vencidos_IncluyeSoloTicketsSinResolverQueSuperanElSla()
        {
            var ahora = DateTime.UtcNow;
            var vencido = _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoAbierto,
                creado: ahora.AddHours(-50), titulo: "Vencido");
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoResuelto,
                creado: ahora.AddHours(-80), titulo: "Viejo pero resuelto");
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoAbierto,
                creado: ahora.AddHours(-2), titulo: "Reciente");

            await using var contexto = _datos.NuevoContexto();
            var vencidos = await FabricaServicios.Metricas(contexto, horasSla: 48).VencidosAsync();

            var unico = Assert.Single(vencidos);
            Assert.Equal(vencido.IdTicket, unico.Id);
            Assert.Equal("2 d", unico.Time);
        }

        [Fact]
        public async Task Escalaciones_ClasificaUrgentesVencidosYAltos()
        {
            var ahora = DateTime.UtcNow;
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoAbierto, "urgente", ahora, titulo: "Urgente");
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoAbierto, "media", ahora.AddHours(-60), titulo: "Vencido");
            _datos.AgregarTicket(_datos.IdCliente1, null, _datos.IdEstadoPendiente, "alta", ahora, titulo: "Alta");
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente2, _datos.IdEstadoResuelto, "urgente", ahora, titulo: "Urgente resuelto");
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente2, _datos.IdEstadoAbierto, "baja", ahora, titulo: "Normal");

            await using var contexto = _datos.NuevoContexto();
            var escalaciones = await FabricaServicios.Metricas(contexto).EscalacionesAsync();

            Assert.Equal(4, escalaciones.Count);
            Assert.Equal("critical", escalaciones.Single(e => e.Title == "Urgente").Status);
            Assert.Equal("critical", escalaciones.Single(e => e.Title == "Vencido").Status);
            Assert.Equal("pending", escalaciones.Single(e => e.Title == "Alta").Status);
            Assert.Equal("Sin asignar", escalaciones.Single(e => e.Title == "Alta").EscalatedTo);
            Assert.Equal("resolved", escalaciones.Single(e => e.Title == "Urgente resuelto").Status);
            Assert.DoesNotContain(escalaciones, e => e.Title == "Normal");
        }

        [Fact]
        public async Task Comparativa_CalculaTasaYTiempoPromedioReales()
        {
            var inicio = DateTime.UtcNow.AddDays(-3);
            // Agente 1: dos resueltos (4 h y 8 h) y uno activo.
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoResuelto,
                creado: inicio, actualizado: inicio.AddHours(4));
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoResuelto,
                creado: inicio, actualizado: inicio.AddHours(8));
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoAbierto, creado: inicio);

            await using var contexto = _datos.NuevoContexto();
            var comparativa = await FabricaServicios.Metricas(contexto).ComparativaAgentesAsync();

            var agente1 = comparativa.Single(c => c.Name == "Alberto Agente");
            Assert.Equal(3, agente1.Asignados);
            Assert.Equal(2, agente1.Resueltos);
            Assert.Equal(1, agente1.Activos);
            Assert.Equal(66.7m, agente1.TasaResolucion);
            Assert.Equal(6.0m, agente1.TiempoPromedioHoras);

            var agente2 = comparativa.Single(c => c.Name == "Beatriz Agente");
            Assert.Equal(0, agente2.Asignados);
            Assert.Null(agente2.TiempoPromedioHoras);
        }

        [Fact]
        public async Task Equipo_NoInventaSatisfaccionYCuentaSoloTicketsActivos()
        {
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoAbierto);
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoResuelto);

            await using var contexto = _datos.NuevoContexto();
            var equipo = await FabricaServicios.Metricas(contexto).EquipoAsync();

            var agente1 = equipo.Single(m => m.IdAgente == _datos.IdAgente1);
            Assert.Equal(1, agente1.Tickets);
            Assert.Null(agente1.Satisfaction);
            Assert.Equal("available", agente1.Status);
        }

        [Fact]
        public async Task RendimientoSemanal_CuentaCreadosYResueltosPorDia()
        {
            var hoy = DateTime.UtcNow;
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoAbierto, creado: hoy);
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoResuelto,
                creado: hoy.AddDays(-1), actualizado: hoy);
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoAbierto, creado: hoy.AddDays(-20));

            await using var contexto = _datos.NuevoContexto();
            var semana = await FabricaServicios.Metricas(contexto).RendimientoSemanalAsync();

            Assert.Equal(7, semana.Labels.Length);
            Assert.Equal(2, semana.Created.Sum());
            Assert.Equal(1, semana.Created[^1]);
            Assert.Equal(1, semana.Resolved[^1]);
        }

        [Theory]
        [InlineData(30, "30 min")]
        [InlineData(150, "2 h")]
        [InlineData(60 * 50, "2 d")]
        public void TiempoRelativo_FormateaMinutosHorasYDias(int minutos, string esperado)
        {
            var ahora = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc);
            Assert.Equal(esperado, TiempoRelativo.Formatear(ahora.AddMinutes(-minutos), ahora));
        }
    }
}
