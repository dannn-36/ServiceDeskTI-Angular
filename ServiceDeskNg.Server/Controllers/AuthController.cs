using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Services;
using ServiceDeskNg.Server.Data;

namespace ServiceDeskNg.Server.Controllers
{
   
    /// Endpoints de autenticación:
    /// POST  api/auth/login             -> Autentica usuario y crea sesión
    /// POST  api/auth/logout            -> Cierra una sesión por sessionId
    /// GET   api/auth/validate-session/{userId} -> Valida si el usuario tiene sesión activa

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;
        private readonly ServiceDeskContext _context;

        public AuthController(UsuarioService usuarioService, ServiceDeskContext context)
        {
            _usuarioService = usuarioService;
            _context = context;
        }

        /// POST api/auth/login
        /// Recibe credenciales, autentica al usuario y crea una sesión activa en la BD.
        /// Devuelve el usuario (sin contraseña) y el sessionId creado.
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.CorreoUsuario) || string.IsNullOrWhiteSpace(request.ContrasenaUsuario))
                    return BadRequest(new { message = "Correo y contraseña son obligatorios." });

                var usuario = _usuarioService.Authenticate(request.CorreoUsuario, request.ContrasenaUsuario);

                // Crear sesión
                var sesion = new Sesion
                {
                    IdUsuario = usuario.IdUsuario,
                    FechaHoraInicioSesion = DateTime.UtcNow,
                    SesionActiva = true
                };

                _context.Sesiones.Add(sesion);
                _context.SaveChanges();

                // No devolver contraseña
                usuario.ContrasenaUsuario = null!;

                return Ok(new
                {
                    message = "Autenticación exitosa",
                    usuario,
                    sessionId = sesion.IdSesion
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error en la autenticación del usuario", error = ex.Message });
            }
        }

        /// POST api/auth/logout
        /// Cierra la sesión indicada por sessionId (marca FechaHoraFinSesion y SesionActiva = false).
        [HttpPost("logout")]
        public IActionResult Logout([FromBody] LogoutRequest request)
        {
            try
            {
                if (request == null || request.SessionId <= 0)
                    return BadRequest(new { message = "SessionId inválido." });

                // Delegar la lógica al servicio de usuario (usa SesionRepository internamente)
                var sesion = new Sesion { IdSesion = request.SessionId };
                _usuarioService.Logout(sesion);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al cerrar la sesión", error = ex.Message });
            }
        }

        /// GET api/auth/validate-session/{userId}
        /// Comprueba si el usuario tiene una sesión activa. Si existe, devuelve valid=true y sessionId.
        [HttpGet("validate-session/{userId}")]
        public IActionResult ValidateSession(int userId)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new { message = "UserId inválido." });

                _usuarioService.IsUserSessionActive(userId);
                return Ok(new { valid = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al validar la sesión", error = ex.Message });
            }
        }
    }

    /// DTO para login.
    public class LoginRequest
    {
        public string CorreoUsuario { get; set; } = null!;
        public string ContrasenaUsuario { get; set; } = null!;
    }

    /// DTO para logout.
    public class LogoutRequest
    {
        public int SessionId { get; set; }
    }
}

