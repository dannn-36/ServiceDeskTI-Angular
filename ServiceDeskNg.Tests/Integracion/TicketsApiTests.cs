using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ServiceDeskNg.Tests.Infraestructura;

namespace ServiceDeskNg.Tests.Integracion
{
    /// Flujos de negocio de tickets a través de la API real.
    public class TicketsApiTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _api;

        public TicketsApiTests(ApiFactory api)
        {
            _api = api;
        }

        [Fact]
        public async Task Cliente_CreaTicketASuNombreAunqueIntenteSuplantarAOtro()
        {
            var cliente = await _api.NavegadorAutenticadoAsync("cliente1@servicedesk.test");

            var respuesta = await cliente.PostAsJsonAsync("/api/tickets", new
            {
                // Intento de crear el ticket a nombre de otro cliente: el servidor lo ignora.
                idCliente = _api.Datos.IdCliente2,
                tituloTicket = "Impresora atascada",
                descripcionTicket = "La impresora del piso 2 no saca hojas",
                idCategoriaTicket = _api.Datos.IdCategoriaHardware,
                prioridadTicket = "alta"
            });

            Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);

            var ticket = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(_api.Datos.IdCliente1, ticket.GetProperty("idCliente").GetInt32());
            Assert.Equal("abierto", ticket.GetProperty("nombreEstado").GetString());
            Assert.Equal(JsonValueKind.Number, ticket.GetProperty("idAgenteAsignado").ValueKind);
        }

        [Fact]
        public async Task Cliente_NoPuedeLeerNiChatearEnTicketsAjenos()
        {
            var ticketAjeno = _api.Datos.AgregarTicket(
                _api.Datos.IdCliente2, _api.Datos.IdAgente1, _api.Datos.IdEstadoAbierto);

            var cliente = await _api.NavegadorAutenticadoAsync("cliente1@servicedesk.test");

            Assert.Equal(HttpStatusCode.Forbidden,
                (await cliente.GetAsync($"/api/tickets/{ticketAjeno.IdTicket}")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden,
                (await cliente.GetAsync($"/api/tickets/cliente/{_api.Datos.IdCliente2}")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden,
                (await cliente.GetAsync($"/api/TicketMensaje/ticket/{ticketAjeno.IdTicket}")).StatusCode);

            var mensaje = await cliente.PostAsJsonAsync("/api/TicketMensaje", new
            {
                idTicket = ticketAjeno.IdTicket,
                mensajeTicket = "Hola, me cuelo en tu chat"
            });
            Assert.Equal(HttpStatusCode.Forbidden, mensaje.StatusCode);
        }

        [Fact]
        public async Task Mensaje_ElAutorLoDecideElServidor()
        {
            var ticket = _api.Datos.AgregarTicket(
                _api.Datos.IdCliente1, _api.Datos.IdAgente1, _api.Datos.IdEstadoAbierto);

            var cliente = await _api.NavegadorAutenticadoAsync("cliente1@servicedesk.test");

            var respuesta = await cliente.PostAsJsonAsync("/api/TicketMensaje", new
            {
                idTicket = ticket.IdTicket,
                mensajeTicket = "¿Alguna novedad?",
                idUsuario = _api.Datos.Admin.IdUsuario
            });

            Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);

            var guardado = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(_api.Datos.UsuarioCliente1.IdUsuario, guardado.GetProperty("idUsuario").GetInt32());
            Assert.Equal("Carla Cliente", guardado.GetProperty("usuarioNombre").GetString());
        }

        [Fact]
        public async Task Agente_SoloPuedeEditarTicketsAsignados()
        {
            var suyo = _api.Datos.AgregarTicket(_api.Datos.IdCliente1, _api.Datos.IdAgente1, _api.Datos.IdEstadoAbierto);
            var ajeno = _api.Datos.AgregarTicket(_api.Datos.IdCliente1, _api.Datos.IdAgente2, _api.Datos.IdEstadoAbierto);

            var agente = await _api.NavegadorAutenticadoAsync("agente1@servicedesk.test");

            object Cambio() => new
            {
                tituloTicket = "Revisado",
                descripcionTicket = "Revisado por el agente",
                idEstadoTicket = _api.Datos.IdEstadoEnProgreso,
                idCategoriaTicket = _api.Datos.IdCategoriaHardware,
                prioridadTicket = "media"
            };

            Assert.Equal(HttpStatusCode.NoContent,
                (await agente.PutAsJsonAsync($"/api/tickets/{suyo.IdTicket}", Cambio())).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden,
                (await agente.PutAsJsonAsync($"/api/tickets/{ajeno.IdTicket}", Cambio())).StatusCode);
        }

        [Fact]
        public async Task Supervisor_ObtienePanelesConDatosReales()
        {
            _api.Datos.AgregarTicket(_api.Datos.IdCliente1, _api.Datos.IdAgente1, _api.Datos.IdEstadoAbierto,
                prioridad: "urgente", titulo: "Servidor caído");

            var supervisor = await _api.NavegadorAutenticadoAsync("supervisor@servicedesk.test");

            var escalaciones = await supervisor.GetFromJsonAsync<JsonElement>("/api/escalations");
            Assert.Contains(escalaciones.EnumerateArray(),
                e => e.GetProperty("title").GetString() == "Servidor caído"
                     && e.GetProperty("status").GetString() == "critical");

            var prioritarios = await supervisor.GetFromJsonAsync<JsonElement>("/api/priority-tickets");
            Assert.Contains(prioritarios.EnumerateArray(),
                t => t.GetProperty("title").GetString() == "Servidor caído");

            var comparativa = await supervisor.GetFromJsonAsync<JsonElement>("/api/team/comparison");
            Assert.All(comparativa.EnumerateArray(), a => Assert.True(a.TryGetProperty("tasaResolucion", out _)));
        }

        [Fact]
        public async Task Sla_ExponeLaPoliticaConfigurada()
        {
            var supervisor = await _api.NavegadorAutenticadoAsync("supervisor@servicedesk.test");

            var sla = await supervisor.GetFromJsonAsync<JsonElement>("/api/tickets/sla");

            Assert.Equal(48, sla.GetProperty("horasVencimiento").GetInt32());
        }

        [Fact]
        public async Task Supervisor_DescargaReportesPdfValidos()
        {
            var supervisor = await _api.NavegadorAutenticadoAsync("supervisor@servicedesk.test");

            foreach (var ruta in new[]
                     {
                         "/api/tickets/reporte-carga",
                         "/api/tickets/reporte-semanal",
                         $"/api/tickets/reporte-individual?idAgente={_api.Datos.IdAgente1}"
                     })
            {
                var respuesta = await supervisor.GetAsync(ruta);
                var bytes = await respuesta.Content.ReadAsByteArrayAsync();

                Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
                Assert.Equal("application/pdf", respuesta.Content.Headers.ContentType?.MediaType);

                // Un PDF de verdad empieza por "%PDF" (antes se devolvía texto plano).
                Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
            }
        }

        [Fact]
        public async Task Admin_DarDeBajaAUnUsuarioConHistorial_LoDesactiva()
        {
            // Se usa el cliente 2 porque el resto de pruebas de esta clase inician sesión como cliente 1.
            _api.Datos.AgregarTicket(_api.Datos.IdCliente2, null, _api.Datos.IdEstadoAbierto);
            var admin = await _api.NavegadorAutenticadoAsync("admin@servicedesk.test");

            var respuesta = await admin.DeleteAsync($"/api/Usuario/{_api.Datos.UsuarioCliente2.IdUsuario}");
            var cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            Assert.False(cuerpo.GetProperty("eliminado").GetBoolean());

            // Y ese usuario ya no puede iniciar sesión.
            var login = await _api.NuevoNavegador().PostAsJsonAsync("/api/auth/login", new
            {
                correoUsuario = "cliente2@servicedesk.test",
                contrasenaUsuario = DatosDePrueba.Contrasena
            });
            Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
        }

        [Fact]
        public async Task Auditoria_RegistraLasAccionesSensibles()
        {
            var admin = await _api.NavegadorAutenticadoAsync("admin@servicedesk.test");

            await admin.PostAsJsonAsync("/api/Usuario", new
            {
                nombreUsuario = "Auditado",
                correoUsuario = "auditado@servicedesk.test",
                contrasenaUsuario = "Clave-Larga-789",
                tipoUsuario = "Cliente"
            });

            var registros = await admin.GetFromJsonAsync<JsonElement>("/api/auditoria?limite=50");
            var acciones = registros.EnumerateArray()
                .Select(r => r.GetProperty("accionAuditoria").GetString())
                .ToList();

            Assert.Contains("LOGIN", acciones);
            Assert.Contains("USUARIO_CREADO", acciones);
        }
    }
}
