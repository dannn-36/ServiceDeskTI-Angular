using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Security;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Controllers
{
    /// Autenticación por cookie HttpOnly.
    ///   POST api/auth/login   -> valida credenciales, abre sesión y emite la cookie
    ///   POST api/auth/logout  -> cierra la sesión en base de datos y borra la cookie
    ///   GET  api/auth/me      -> identidad de la sesión actual (para restaurarla al recargar)
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public const string PoliticaLimiteLogin = "login";

        private readonly UsuarioService _usuarios;
        private readonly SesionService _sesiones;
        private readonly AuditoriaService _auditoria;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UsuarioService usuarios,
            SesionService sesiones,
            AuditoriaService auditoria,
            ILogger<AuthController> logger)
        {
            _usuarios = usuarios;
            _sesiones = sesiones;
            _auditoria = auditoria;
            _logger = logger;
        }

        [AllowAnonymous]
        [EnableRateLimiting(PoliticaLimiteLogin)]
        [HttpPost("login")]
        public async Task<ActionResult<SesionUsuarioDto>> Login(
            [FromBody] LoginRequest request,
            CancellationToken ct)
        {
            IdentidadUsuario identidad;

            try
            {
                identidad = await _usuarios.AutenticarAsync(
                    request.CorreoUsuario.Trim(),
                    request.ContrasenaUsuario,
                    ct);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Si el correo corresponde a un usuario real, el intento queda en su bitácora.
                var idUsuario = await _usuarios.BuscarIdPorCorreoAsync(request.CorreoUsuario.Trim(), ct);
                if (idUsuario is int id)
                    await _auditoria.RegistrarAsync(id, AccionesAuditoria.LoginFallido, ex.Message, ct);

                _logger.LogWarning(
                    "Inicio de sesión rechazado desde {Ip}",
                    HttpContext.Connection.RemoteIpAddress);
                throw;
            }

            var sesion = await _sesiones.AbrirAsync(identidad.Usuario.IdUsuario, ct);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                FabricaIdentidad.CrearPrincipal(identidad, sesion.IdSesion),
                new AuthenticationProperties { IsPersistent = false });

            await _auditoria.RegistrarAsync(
                identidad.Usuario.IdUsuario,
                AccionesAuditoria.Login,
                $"Sesión {sesion.IdSesion} abierta como {identidad.Rol}",
                ct);

            return Ok(FabricaIdentidad.CrearDto(identidad));
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var idUsuario = User.IdUsuario();

            if (User.IdSesion() is int idSesion)
                await _sesiones.CerrarAsync(idSesion, ct);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await _auditoria.RegistrarAsync(idUsuario, AccionesAuditoria.Logout, null, ct);

            return NoContent();
        }

        /// Se lee de la base de datos y no de la cookie, para reflejar
        /// cambios de nombre o correo hechos durante la sesión.
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<SesionUsuarioDto>> Me(CancellationToken ct)
        {
            var identidad = await _usuarios.ObtenerIdentidadAsync(User.IdUsuario(), ct);
            return Ok(FabricaIdentidad.CrearDto(identidad));
        }
    }
}
