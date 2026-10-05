using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Tests.Infraestructura;

namespace ServiceDeskNg.Tests.Unitarias
{
    public class InicializadorBaseDatosTests
    {
        private static InicializadorBaseDatos Crear(ServiceDeskContext contexto, OpcionesInicializacion opciones) =>
            new(
                contexto,
                FabricaServicios.Usuarios(contexto),
                Options.Create(opciones),
                NullLogger<InicializadorBaseDatos>.Instance);

        private static OpcionesInicializacion ConAdmin(bool demo = false) => new()
        {
            AdminNombre = "Admin Inicial",
            AdminCorreo = "admin@servicedesk.local",
            AdminContrasena = "Clave-Inicial-123",
            SembrarDatosDemo = demo
        };

        [Fact]
        public async Task BaseNueva_CreaElPrimerAdministradorConElQueSePuedeEntrar()
        {
            var datos = DatosDePrueba.CrearSoloCatalogos();

            await using (var contexto = datos.NuevoContexto())
                await Crear(contexto, ConAdmin()).EjecutarAsync();

            await using var verificacion = datos.NuevoContexto();
            var identidad = await FabricaServicios.Usuarios(verificacion)
                .AutenticarAsync("admin@servicedesk.local", "Clave-Inicial-123");

            Assert.Equal("Administrador", identidad.Rol);
        }

        [Fact]
        public async Task SiYaHayAdministrador_NoCreaOtro()
        {
            // DatosDePrueba.Crear() ya incluye un administrador.
            var datos = DatosDePrueba.Crear();

            await using (var contexto = datos.NuevoContexto())
                await Crear(contexto, ConAdmin()).EjecutarAsync();

            await using var verificacion = datos.NuevoContexto();
            Assert.Equal(1, await verificacion.Administradores.CountAsync());
            Assert.False(await verificacion.Usuarios.AnyAsync(u => u.CorreoUsuario == "admin@servicedesk.local"));
        }

        [Fact]
        public async Task SinCuentaConfigurada_NoInventaCredenciales()
        {
            var datos = DatosDePrueba.CrearSoloCatalogos();

            await using (var contexto = datos.NuevoContexto())
                await Crear(contexto, new OpcionesInicializacion()).EjecutarAsync();

            await using var verificacion = datos.NuevoContexto();
            Assert.False(await verificacion.Usuarios.AnyAsync());
        }

        [Fact]
        public async Task DatosDemo_CarganUnEscenarioCompletoYNoSeDuplicanAlReiniciar()
        {
            var datos = DatosDePrueba.CrearSoloCatalogos();

            // Dos arranques seguidos, como dos "docker compose up".
            for (var arranque = 0; arranque < 2; arranque++)
            {
                await using var contexto = datos.NuevoContexto();
                await Crear(contexto, ConAdmin(demo: true)).EjecutarAsync();
            }

            await using var verificacion = datos.NuevoContexto();

            Assert.Equal(16, await verificacion.Tickets.CountAsync());
            Assert.Equal(3, await verificacion.Agentes.CountAsync());
            Assert.Equal(4, await verificacion.Clientes.CountAsync());
            Assert.True(await verificacion.TicketMensajes.AnyAsync());

            // El escenario cubre los casos que muestran los paneles.
            var metricas = FabricaServicios.Metricas(verificacion);
            Assert.NotEmpty(await metricas.VencidosAsync());
            Assert.NotEmpty(await metricas.PrioritariosAsync());
            Assert.Contains(await metricas.EscalacionesAsync(), e => e.Status == "critical");
            Assert.True(await verificacion.Tickets.AnyAsync(t => t.IdAgenteAsignado == null));
            Assert.True(await verificacion.Agentes.AnyAsync(a => a.DisponibilidadAgente == false));

            // Los usuarios de demostración pueden iniciar sesión.
            var identidad = await FabricaServicios.Usuarios(verificacion)
                .AutenticarAsync("cliente1@servicedesk.local", new OpcionesInicializacion().ContrasenaDemo);
            Assert.Equal("Cliente", identidad.Rol);
        }
    }
}
