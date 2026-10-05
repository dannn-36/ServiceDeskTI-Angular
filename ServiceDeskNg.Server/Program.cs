using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Server.Controllers;
using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Hubs;
using ServiceDeskNg.Server.Repositories;
using ServiceDeskNg.Server.Repositories.Interfaces;
using ServiceDeskNg.Server.Security;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server
{
    public class Program
    {
        /// Entorno usado por las pruebas de integración: registran su propia base en memoria.
        public const string EntornoPruebas = "Testing";

        public static void Main(string[] args)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var builder = WebApplication.CreateBuilder(args);

            ConfigurarBaseDeDatos(builder);
            ConfigurarServicios(builder.Services, builder.Configuration);
            ConfigurarSeguridad(builder);
            ConfigurarApi(builder);

            var app = builder.Build();

            VerificarConexion(app);
            ConfigurarPipeline(app);

            app.Run();
        }

        // ======================================================
        // Base de datos
        // ======================================================

        private static void ConfigurarBaseDeDatos(WebApplicationBuilder builder)
        {
            if (builder.Environment.IsEnvironment(EntornoPruebas))
                return;

            var cadena = builder.Configuration.GetConnectionString("ServiceDeskDB");

            if (string.IsNullOrWhiteSpace(cadena))
            {
                throw new InvalidOperationException(
                    "Falta la cadena de conexión 'ConnectionStrings:ServiceDeskDB'. " +
                    "En desarrollo configúrela con: dotnet user-secrets set \"ConnectionStrings:ServiceDeskDB\" " +
                    "\"Server=localhost;Port=3306;Database=servicedesk;User=...;Password=...\" " +
                    "(ver README). En producción, use la variable de entorno ConnectionStrings__ServiceDeskDB.");
            }

            builder.Services.AddDbContext<ServiceDeskContext>(options =>
                options.UseMySql(
                    cadena,
                    new MySqlServerVersion(new Version(8, 0, 41)),
                    // Reintenta automáticamente ante cortes transitorios de conexión con MySQL.
                    mysql => mysql.EnableRetryOnFailure(maxRetryCount: 3)));
        }

        // ======================================================
        // Inyección de dependencias
        // ======================================================

        private static void ConfigurarServicios(IServiceCollection services, IConfiguration configuracion)
        {
            // Un único repositorio genérico para todas las entidades.
            services.AddScoped(typeof(IRepositorio<>), typeof(EfRepository<>));

            services.AddScoped<SesionService>();
            services.AddScoped<UsuarioService>();
            services.AddScoped<AuditoriaService>();
            services.AddScoped<CatalogoTicketsService>();
            services.AddScoped<TicketService>();
            services.AddScoped<MetricasService>();
            services.AddScoped<TicketMensajeService>();
            services.AddScoped<AgenteService>();
            services.AddScoped<EndUserService>();
            services.AddScoped<SupervisorService>();
            services.AddScoped<AdministradorService>();
            services.AddScoped<RespaldoService>();
            services.AddSingleton<ReportesPdfService>();

            services.Configure<OpcionesSla>(configuracion.GetSection(OpcionesSla.Seccion));
            services.Configure<OpcionesRespaldo>(configuracion.GetSection(OpcionesRespaldo.Seccion));
        }

        // ======================================================
        // Autenticación, autorización y límites
        // ======================================================

        private static void ConfigurarSeguridad(WebApplicationBuilder builder)
        {
            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "servicedesk.sesion";

                    // Inaccesible desde JavaScript: un XSS no puede robar la sesión.
                    options.Cookie.HttpOnly = true;

                    // No viaja en peticiones iniciadas desde otros sitios (mitiga CSRF).
                    options.Cookie.SameSite = SameSiteMode.Strict;

                    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                        ? CookieSecurePolicy.SameAsRequest
                        : CookieSecurePolicy.Always;

                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;

                    // Es una API: se responde con códigos HTTP en lugar de redirigir a una página de login.
                    options.Events.OnRedirectToLogin = contexto =>
                        EscribirError(contexto.HttpContext, StatusCodes.Status401Unauthorized,
                            "Debe iniciar sesión.");
                    options.Events.OnRedirectToAccessDenied = contexto =>
                        EscribirError(contexto.HttpContext, StatusCodes.Status403Forbidden,
                            "No tiene permisos para esta operación.");

                    options.Events.OnValidatePrincipal = ValidarSesionContraBaseDeDatosAsync;
                });

            // Todo endpoint exige sesión salvo que se marque [AllowAnonymous] explícitamente.
            // Así un controlador nuevo nunca nace público por olvido.
            builder.Services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

            var intentosPorMinuto = builder.Configuration.GetValue("Seguridad:IntentosLoginPorMinuto", 10);

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (contexto, ct) =>
                    await contexto.HttpContext.Response.WriteAsJsonAsync(
                        new { message = "Demasiados intentos de inicio de sesión. Espere un minuto e inténtelo de nuevo." },
                        ct);

                // Frena ataques de fuerza bruta contra el login, por dirección IP.
                options.AddPolicy(AuthController.PoliticaLimiteLogin, contexto =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = intentosPorMinuto,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));
            });

            var origenes = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>()
                ?? ["http://localhost:4200", "https://localhost:59435"];

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy => policy
                    .WithOrigins(origenes)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });
        }

        /// Cada petición autenticada comprueba que su sesión siga abierta en la tabla `sesiones`
        /// y que la cuenta siga activa. Cerrar sesión o desactivar a un usuario invalida
        /// su cookie al instante, aunque todavía no haya caducado.
        private static async Task ValidarSesionContraBaseDeDatosAsync(CookieValidatePrincipalContext contexto)
        {
            var principal = contexto.Principal;
            var idSesion = principal?.IdSesion();
            var claimUsuario = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var sesionValida = false;

            if (idSesion is int sesion && int.TryParse(claimUsuario, out var idUsuario))
            {
                var sesiones = contexto.HttpContext.RequestServices.GetRequiredService<SesionService>();
                sesionValida = await sesiones.EsValidaAsync(sesion, idUsuario, contexto.HttpContext.RequestAborted);
            }

            if (!sesionValida)
            {
                contexto.RejectPrincipal();
                await contexto.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }

        private static Task EscribirError(HttpContext http, int estado, string mensaje)
        {
            http.Response.StatusCode = estado;
            return http.Response.WriteAsJsonAsync(new { message = mensaje });
        }

        // ======================================================
        // Controladores, JSON, SignalR y errores
        // ======================================================

        private static void ConfigurarApi(WebApplicationBuilder builder)
        {
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    // Errores de validación con la misma forma { message } que el resto de la API,
                    // más el detalle por campo en `errors`.
                    options.InvalidModelStateResponseFactory = contexto =>
                    {
                        var errores = contexto.ModelState
                            .Where(entrada => entrada.Value?.Errors.Count > 0)
                            .ToDictionary(
                                entrada => entrada.Key,
                                entrada => entrada.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

                        var primero = errores.Values.SelectMany(m => m).FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
                            ?? "Los datos enviados no son válidos.";

                        return new BadRequestObjectResult(new { message = primero, errors = errores });
                    };
                });

            builder.Services.AddSignalR();
            builder.Services.AddOpenApi();

            builder.Services.AddExceptionHandler<ManejadorGlobalErrores>();
            builder.Services.AddProblemDetails();
        }

        // ======================================================
        // Arranque
        // ======================================================

        private static void VerificarConexion(WebApplication app)
        {
            if (app.Environment.IsEnvironment(EntornoPruebas))
                return;

            using var scope = app.Services.CreateScope();
            var contexto = scope.ServiceProvider.GetRequiredService<ServiceDeskContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                if (contexto.Database.CanConnect())
                    logger.LogInformation("Conexión a la base de datos verificada.");
                else
                    logger.LogError("No se pudo conectar a la base de datos. Revise la cadena de conexión.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al verificar la conexión con la base de datos.");
            }
        }

        private static void ConfigurarPipeline(WebApplication app)
        {
            app.UseExceptionHandler();

            if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment(EntornoPruebas))
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseDefaultFiles();

            app.UseRouting();
            app.UseCors("AllowAngular");
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            // La aplicación Angular publicada (index.html, JS, CSS) es pública:
            // el control de acceso se hace en la API.
            app.MapStaticAssets().AllowAnonymous();

            // Documento OpenAPI en desarrollo (y en pruebas, que verifican que se genera bien).
            if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment(EntornoPruebas))
                app.MapOpenApi().AllowAnonymous();

            app.MapControllers();
            app.MapHub<ChatHub>("/chathub");
            app.MapFallbackToFile("/index.html").AllowAnonymous();
        }
    }
}
