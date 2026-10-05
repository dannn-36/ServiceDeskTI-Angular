using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ServiceDeskNg.Tests.Infraestructura;

namespace ServiceDeskNg.Tests.Integracion
{
    /// Pruebas de extremo a extremo del modelo de seguridad: la API real,
    /// con cookies reales, contra una base de datos en memoria.
    public class SeguridadApiTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _api;

        public SeguridadApiTests(ApiFactory api)
        {
            _api = api;
        }

        [Theory]
        [InlineData("/api/tickets")]
        [InlineData("/api/Usuario")]
        [InlineData("/api/backup")]
        [InlineData("/api/auditoria")]
        [InlineData("/api/team")]
        [InlineData("/api/escalations")]
        [InlineData("/api/auth/me")]
        public async Task SinSesion_LaApiRespondeNoAutorizado(string ruta)
        {
            var respuesta = await _api.NuevoNavegador().GetAsync(ruta);

            Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        }

        [Fact]
        public async Task Login_DevuelveIdentidadYEmiteCookieHttpOnly()
        {
            var navegador = _api.NuevoNavegador();

            var respuesta = await navegador.PostAsJsonAsync("/api/auth/login", new
            {
                correoUsuario = "agente1@servicedesk.test",
                contrasenaUsuario = DatosDePrueba.Contrasena
            });

            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

            var cookie = Assert.Single(respuesta.Headers.GetValues("Set-Cookie"));
            Assert.Contains("servicedesk.sesion=", cookie);
            Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);

            var sesion = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Agente", sesion.GetProperty("rol").GetString());
            Assert.True(sesion.GetProperty("idAgente").GetInt32() > 0);
        }

        [Fact]
        public async Task Login_ConCredencialesErroneas_DaUnMensajeGenerico()
        {
            var navegador = _api.NuevoNavegador();

            var correoInexistente = await navegador.PostAsJsonAsync("/api/auth/login", new
            {
                correoUsuario = "nadie@servicedesk.test",
                contrasenaUsuario = "lo-que-sea"
            });
            var claveIncorrecta = await navegador.PostAsJsonAsync("/api/auth/login", new
            {
                correoUsuario = "admin@servicedesk.test",
                contrasenaUsuario = "lo-que-sea"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, correoInexistente.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, claveIncorrecta.StatusCode);
            Assert.Equal(
                await correoInexistente.Content.ReadAsStringAsync(),
                await claveIncorrecta.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Logout_InvalidaLaSesionEnElServidor()
        {
            var login = await _api.NuevoNavegador().PostAsJsonAsync("/api/auth/login", new
            {
                correoUsuario = "cliente2@servicedesk.test",
                contrasenaUsuario = DatosDePrueba.Contrasena
            });
            var cookie = login.Headers.GetValues("Set-Cookie").Single().Split(';')[0];

            // Dos clientes con la misma cookie: el dueño y una copia (p. ej. robada antes del logout).
            var dueno = ClienteConCookie(cookie);
            var copia = ClienteConCookie(cookie);
            Assert.Equal(HttpStatusCode.OK, (await copia.GetAsync("/api/auth/me")).StatusCode);

            Assert.Equal(HttpStatusCode.NoContent, (await dueno.PostAsync("/api/auth/logout", null)).StatusCode);

            // La copia deja de servir aunque la cookie todavía no haya caducado:
            // la sesión se cerró en la base de datos, no solo en el navegador.
            Assert.Equal(HttpStatusCode.Unauthorized, (await copia.GetAsync("/api/auth/me")).StatusCode);
        }

        private HttpClient ClienteConCookie(string cookie)
        {
            var cliente = _api.Server.CreateClient();
            cliente.BaseAddress = new Uri("https://localhost");
            cliente.DefaultRequestHeaders.Add("Cookie", cookie);
            return cliente;
        }

        [Fact]
        public async Task Cliente_NoPuedeAccederAFuncionesDeAdministracion()
        {
            var cliente = await _api.NavegadorAutenticadoAsync("cliente1@servicedesk.test");

            Assert.Equal(HttpStatusCode.Forbidden, (await cliente.GetAsync("/api/Usuario")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await cliente.GetAsync("/api/backup")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await cliente.GetAsync("/api/tickets")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await cliente.GetAsync("/api/auditoria")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden,
                (await cliente.DeleteAsync($"/api/Usuario/{_api.Datos.Admin.IdUsuario}")).StatusCode);
        }

        [Fact]
        public async Task Agente_NoPuedeVerPerfilesDeOtrosUsuarios()
        {
            var agente = await _api.NavegadorAutenticadoAsync("agente1@servicedesk.test");

            var propio = await agente.GetAsync($"/api/Usuario/{_api.Datos.UsuarioAgente1.IdUsuario}");
            var ajeno = await agente.GetAsync($"/api/Usuario/{_api.Datos.Admin.IdUsuario}");

            Assert.Equal(HttpStatusCode.OK, propio.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, ajeno.StatusCode);
        }

        [Fact]
        public async Task Usuarios_NuncaExponenElHashDeLaContrasena()
        {
            var admin = await _api.NavegadorAutenticadoAsync("admin@servicedesk.test");

            var lista = await admin.GetStringAsync("/api/Usuario");
            var detalle = await admin.GetStringAsync($"/api/Usuario/{_api.Datos.Admin.IdUsuario}");

            Assert.DoesNotContain("contrasena", lista, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("$2a$", lista);
            Assert.DoesNotContain("contrasena", detalle, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Errores_NoFiltranDetallesInternos()
        {
            var admin = await _api.NavegadorAutenticadoAsync("admin@servicedesk.test");

            var respuesta = await admin.GetAsync("/api/tickets/999999");
            var cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
            Assert.True(cuerpo.TryGetProperty("message", out _));
            Assert.False(cuerpo.TryGetProperty("error", out _));
        }

        [Fact]
        public async Task OpenApi_SeGeneraYNoExponeEntidadesInternas()
        {
            var respuesta = await _api.NuevoNavegador().GetAsync("/openapi/v1.json");
            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

            var documento = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(documento.GetProperty("paths").TryGetProperty("/api/tickets", out _));

            // Si algún endpoint expusiera entidades de EF, el contrato incluiría su grafo
            // completo (con el hash de la contraseña del usuario) y, por ciclos, ni siquiera se generaría.
            var esquemas = documento.GetProperty("components").GetProperty("schemas")
                .EnumerateObject().Select(e => e.Name).ToList();
            string[] entidades = ["Usuario", "Ticket", "TicketsCategoria", "TicketsEstado", "Agente", "EndUser", "Sesion"];
            Assert.DoesNotContain(esquemas, entidades.Contains);
        }

        [Fact]
        public async Task Validacion_DevuelveMensajeLegible()
        {
            var admin = await _api.NavegadorAutenticadoAsync("admin@servicedesk.test");

            var respuesta = await admin.PostAsJsonAsync("/api/Usuario", new
            {
                nombreUsuario = "Sin correo",
                correoUsuario = "esto-no-es-un-correo",
                contrasenaUsuario = "corta",
                tipoUsuario = "Cliente"
            });

            var cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(cuerpo.GetProperty("message").GetString()));
            Assert.True(cuerpo.GetProperty("errors").EnumerateObject().Count() >= 2);
        }
    }

    /// Fábrica aparte con un límite de intentos bajo, para no interferir con las demás pruebas.
    public class FuerzaBrutaApiFactory : ApiFactory
    {
        public FuerzaBrutaApiFactory()
        {
            IntentosLoginPorMinuto = 3;
        }
    }

    public class LimiteDeIntentosTests : IClassFixture<FuerzaBrutaApiFactory>
    {
        private readonly FuerzaBrutaApiFactory _api;

        public LimiteDeIntentosTests(FuerzaBrutaApiFactory api)
        {
            _api = api;
        }

        [Fact]
        public async Task Login_TrasDemasiadosIntentos_RespondeTooManyRequests()
        {
            var navegador = _api.NuevoNavegador();
            var estados = new List<HttpStatusCode>();

            for (var i = 0; i < 5; i++)
            {
                var respuesta = await navegador.PostAsJsonAsync("/api/auth/login", new
                {
                    correoUsuario = "admin@servicedesk.test",
                    contrasenaUsuario = $"intento-{i}"
                });
                estados.Add(respuesta.StatusCode);
            }

            Assert.Equal(HttpStatusCode.Unauthorized, estados[0]);
            Assert.Equal(HttpStatusCode.TooManyRequests, estados[^1]);
        }
    }
}
