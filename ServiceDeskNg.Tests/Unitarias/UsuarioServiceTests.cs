using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Security;
using ServiceDeskNg.Tests.Infraestructura;

namespace ServiceDeskNg.Tests.Unitarias
{
    public class UsuarioServiceTests
    {
        private readonly DatosDePrueba _datos = DatosDePrueba.Crear();

        [Fact]
        public async Task Autenticar_CorreoInexistenteYContrasenaIncorrecta_DanElMismoMensaje()
        {
            await using var contexto = _datos.NuevoContexto();
            var servicio = FabricaServicios.Usuarios(contexto);

            var correoInexistente = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => servicio.AutenticarAsync("nadie@servicedesk.test", DatosDePrueba.Contrasena));

            var contrasenaIncorrecta = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => servicio.AutenticarAsync(_datos.Admin.CorreoUsuario, "otra-clave"));

            // Si los mensajes difirieran, la API permitiría averiguar qué correos existen.
            Assert.Equal(correoInexistente.Message, contrasenaIncorrecta.Message);
        }

        [Fact]
        public async Task Autenticar_CredencialesCorrectas_DevuelveRolEIdentificadores()
        {
            await using var contexto = _datos.NuevoContexto();
            var identidad = await FabricaServicios.Usuarios(contexto)
                .AutenticarAsync(_datos.UsuarioAgente1.CorreoUsuario, DatosDePrueba.Contrasena);

            Assert.Equal(RolesApp.Agente, identidad.Rol);
            Assert.Equal(_datos.IdAgente1, identidad.IdAgente);
            Assert.Null(identidad.IdCliente);
        }

        [Fact]
        public async Task Autenticar_CuentaInactiva_EsRechazada()
        {
            await using (var contexto = _datos.NuevoContexto())
            {
                var usuario = await contexto.Usuarios.SingleAsync(u => u.IdUsuario == _datos.UsuarioCliente1.IdUsuario);
                usuario.EstadoUsuario = "inactivo";
                await contexto.SaveChangesAsync();
            }

            await using var otroContexto = _datos.NuevoContexto();
            var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                FabricaServicios.Usuarios(otroContexto)
                    .AutenticarAsync(_datos.UsuarioCliente1.CorreoUsuario, DatosDePrueba.Contrasena));

            Assert.Contains("inactiva", error.Message);
        }

        [Fact]
        public async Task CrearConRol_GuardaHashBCryptYFilaDeRol()
        {
            await using (var contexto = _datos.NuevoContexto())
            {
                var creado = await FabricaServicios.Usuarios(contexto).CrearConRolAsync(new UsuarioCreateDto
                {
                    NombreUsuario = "Nuevo Agente",
                    CorreoUsuario = "nuevo.agente@servicedesk.test",
                    ContrasenaUsuario = "Otra-Clave-456",
                    TipoUsuario = "Agente"
                });

                Assert.Equal(RolesApp.Agente, creado.TipoUsuario);
            }

            await using var verificacion = _datos.NuevoContexto();
            var guardado = await verificacion.Usuarios
                .Include(u => u.Agentes)
                .SingleAsync(u => u.CorreoUsuario == "nuevo.agente@servicedesk.test");

            Assert.NotEqual("Otra-Clave-456", guardado.ContrasenaUsuario);
            Assert.True(BCrypt.Net.BCrypt.Verify("Otra-Clave-456", guardado.ContrasenaUsuario));
            Assert.Single(guardado.Agentes);
        }

        [Theory]
        [InlineData("Cliente", 1)]
        [InlineData("Agente", 2)]
        [InlineData("Supervisor", 3)]
        [InlineData("Administrador", 4)]
        public async Task CrearConRol_AsignaElNivelDeAccesoQueCorrespondeAlRol(string rol, int nivelEsperado)
        {
            var correo = $"nivel.{rol.ToLowerInvariant()}@servicedesk.test";

            await using (var contexto = _datos.NuevoContexto())
            {
                await FabricaServicios.Usuarios(contexto).CrearConRolAsync(new UsuarioCreateDto
                {
                    NombreUsuario = $"Usuario {rol}",
                    CorreoUsuario = correo,
                    ContrasenaUsuario = "Otra-Clave-456",
                    TipoUsuario = rol
                });
            }

            // Antes todos los usuarios recibían el nivel 1 ("acceso básico"), incluso los administradores.
            await using var verificacion = _datos.NuevoContexto();
            var idUsuario = await verificacion.Usuarios.Where(u => u.CorreoUsuario == correo).Select(u => u.IdUsuario).SingleAsync();
            var idNivel = rol switch
            {
                "Cliente" => await verificacion.Clientes.Where(c => c.IdUsuario == idUsuario).Select(c => c.IdNivel).SingleAsync(),
                "Agente" => await verificacion.Agentes.Where(a => a.IdUsuario == idUsuario).Select(a => a.IdNivel).SingleAsync(),
                "Supervisor" => await verificacion.Supervisores.Where(s => s.IdUsuario == idUsuario).Select(s => s.IdNivel).SingleAsync(),
                _ => await verificacion.Administradores.Where(a => a.IdUsuario == idUsuario).Select(a => a.IdNivel).SingleAsync()
            };
            var nivel = await verificacion.NivelesAccesos.SingleAsync(n => n.IdNivel == idNivel);

            Assert.Equal(nivelEsperado, nivel.Nivel);
        }

        [Fact]
        public async Task CrearConRol_AceptaElAliasHistoricoEndUser()
        {
            await using var contexto = _datos.NuevoContexto();
            var creado = await FabricaServicios.Usuarios(contexto).CrearConRolAsync(new UsuarioCreateDto
            {
                NombreUsuario = "Cliente Nuevo",
                CorreoUsuario = "cliente.nuevo@servicedesk.test",
                ContrasenaUsuario = "Otra-Clave-456",
                TipoUsuario = "EndUser"
            });

            Assert.Equal(RolesApp.Cliente, creado.TipoUsuario);
        }

        [Fact]
        public async Task CrearConRol_CorreoDuplicado_EsConflicto()
        {
            await using var contexto = _datos.NuevoContexto();

            await Assert.ThrowsAsync<ConflictoNegocioException>(() =>
                FabricaServicios.Usuarios(contexto).CrearConRolAsync(new UsuarioCreateDto
                {
                    NombreUsuario = "Duplicado",
                    CorreoUsuario = _datos.Admin.CorreoUsuario,
                    ContrasenaUsuario = "Otra-Clave-456",
                    TipoUsuario = "Cliente"
                }));
        }

        [Fact]
        public async Task CrearConRol_RolDesconocido_EsRechazado()
        {
            await using var contexto = _datos.NuevoContexto();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                FabricaServicios.Usuarios(contexto).CrearConRolAsync(new UsuarioCreateDto
                {
                    NombreUsuario = "Hacker",
                    CorreoUsuario = "hacker@servicedesk.test",
                    ContrasenaUsuario = "Otra-Clave-456",
                    TipoUsuario = "SuperUsuario"
                }));
        }

        [Fact]
        public async Task Actualizar_UsuarioSinPermisoDeAdministracion_NoPuedeCambiarSuEstado()
        {
            var id = _datos.UsuarioCliente1.IdUsuario;

            await using (var contexto = _datos.NuevoContexto())
            {
                await FabricaServicios.Usuarios(contexto).ActualizarAsync(id, new UsuarioUpdateDto
                {
                    NombreUsuario = "Carla Renombrada",
                    CorreoUsuario = _datos.UsuarioCliente1.CorreoUsuario,
                    EstadoUsuario = "inactivo"
                }, puedeAdministrar: false);
            }

            await using var verificacion = _datos.NuevoContexto();
            var guardado = await verificacion.Usuarios.SingleAsync(u => u.IdUsuario == id);

            Assert.Equal("Carla Renombrada", guardado.NombreUsuario);
            Assert.Equal("activo", guardado.EstadoUsuario);
        }

        [Fact]
        public async Task DarDeBaja_ConHistorial_DesactivaEnLugarDeEliminar()
        {
            _datos.AgregarTicket(_datos.IdCliente1, _datos.IdAgente1, _datos.IdEstadoAbierto);

            await using (var contexto = _datos.NuevoContexto())
            {
                var resultado = await FabricaServicios.Usuarios(contexto)
                    .DarDeBajaAsync(_datos.UsuarioCliente1.IdUsuario, _datos.Admin.IdUsuario);

                Assert.False(resultado.Eliminado);
            }

            await using var verificacion = _datos.NuevoContexto();
            var guardado = await verificacion.Usuarios.SingleAsync(u => u.IdUsuario == _datos.UsuarioCliente1.IdUsuario);
            Assert.Equal("inactivo", guardado.EstadoUsuario);
        }

        [Fact]
        public async Task DarDeBaja_SinHistorial_EliminaUsuarioYRol()
        {
            await using (var contexto = _datos.NuevoContexto())
            {
                var resultado = await FabricaServicios.Usuarios(contexto)
                    .DarDeBajaAsync(_datos.UsuarioCliente2.IdUsuario, _datos.Admin.IdUsuario);

                Assert.True(resultado.Eliminado);
            }

            await using var verificacion = _datos.NuevoContexto();
            Assert.False(await verificacion.Usuarios.AnyAsync(u => u.IdUsuario == _datos.UsuarioCliente2.IdUsuario));
            Assert.False(await verificacion.Clientes.AnyAsync(c => c.IdUsuario == _datos.UsuarioCliente2.IdUsuario));
        }

        [Fact]
        public async Task DarDeBaja_LaPropiaCuenta_EsConflicto()
        {
            await using var contexto = _datos.NuevoContexto();

            await Assert.ThrowsAsync<ConflictoNegocioException>(() =>
                FabricaServicios.Usuarios(contexto).DarDeBajaAsync(_datos.Admin.IdUsuario, _datos.Admin.IdUsuario));
        }

        [Fact]
        public async Task DarDeBaja_ElUltimoAdministradorActivo_EsConflicto()
        {
            await using var contexto = _datos.NuevoContexto();
            var servicio = FabricaServicios.Usuarios(contexto);

            // Lo intenta el supervisor sobre el único administrador del sistema.
            var error = await Assert.ThrowsAsync<ConflictoNegocioException>(() =>
                servicio.DarDeBajaAsync(_datos.Admin.IdUsuario, _datos.Supervisor.IdUsuario));

            Assert.Contains("administradores", error.Message);
        }

        [Fact]
        public async Task Sesion_CerradaODeUsuarioInactivo_DejaDeSerValida()
        {
            await using var contexto = _datos.NuevoContexto();
            var sesiones = FabricaServicios.Sesiones(contexto);

            var sesion = await sesiones.AbrirAsync(_datos.UsuarioCliente1.IdUsuario);
            Assert.True(await sesiones.EsValidaAsync(sesion.IdSesion, _datos.UsuarioCliente1.IdUsuario));

            // La sesión de un usuario no sirve para otro.
            Assert.False(await sesiones.EsValidaAsync(sesion.IdSesion, _datos.UsuarioCliente2.IdUsuario));

            await sesiones.CerrarAsync(sesion.IdSesion);
            Assert.False(await sesiones.EsValidaAsync(sesion.IdSesion, _datos.UsuarioCliente1.IdUsuario));
        }

        [Fact]
        public void ResolucionRol_PriorizaAdministradorSobreOtrosRoles()
        {
            var usuario = new Usuario
            {
                IdUsuario = 1,
                NombreUsuario = "Mixto",
                CorreoUsuario = "mixto@servicedesk.test",
                Administradores = [new Administrador { IdAdmin = 7 }],
                Agentes = [new Agente { IdAgente = 3 }]
            };

            var identidad = ResolucionRol.Resolver(usuario);

            Assert.Equal(RolesApp.Administrador, identidad.Rol);
            Assert.Equal(7, identidad.IdAdministrador);
        }
    }
}
