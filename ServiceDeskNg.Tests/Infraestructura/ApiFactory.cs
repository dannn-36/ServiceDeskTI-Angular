using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceDeskNg.Server;
using ServiceDeskNg.Server.Data;

namespace ServiceDeskNg.Tests.Infraestructura
{
    /// Levanta la API completa (pipeline, autenticación, autorización, controladores)
    /// sobre una base de datos en memoria con los datos de prueba.
    public class ApiFactory : WebApplicationFactory<Program>
    {
        public DatosDePrueba Datos { get; } = new();

        /// Límite de intentos de login por minuto; las pruebas de fuerza bruta lo bajan.
        public int IntentosLoginPorMinuto { get; init; } = 1000;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(Program.EntornoPruebas);
            builder.UseSetting("Seguridad:IntentosLoginPorMinuto", IntentosLoginPorMinuto.ToString());

            builder.ConfigureServices(services =>
                services.AddScoped(_ => new ServiceDeskContext(DatosDePrueba.Opciones(Datos.NombreBaseDatos))));
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            using var contexto = Datos.NuevoContexto();
            Datos.Sembrar(contexto);

            return host;
        }

        /// Cliente HTTP con su propio almacén de cookies (una "pestaña" de navegador).
        /// Se usa https porque fuera de Development la cookie de sesión es Secure.
        public HttpClient NuevoNavegador() =>
            CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                HandleCookies = true,
                AllowAutoRedirect = false
            });

        public async Task<HttpClient> NavegadorAutenticadoAsync(string correo)
        {
            var navegador = NuevoNavegador();
            var respuesta = await navegador.PostAsJsonAsync("/api/auth/login", new
            {
                correoUsuario = correo,
                contrasenaUsuario = DatosDePrueba.Contrasena
            });

            respuesta.EnsureSuccessStatusCode();
            return navegador;
        }
    }
}
