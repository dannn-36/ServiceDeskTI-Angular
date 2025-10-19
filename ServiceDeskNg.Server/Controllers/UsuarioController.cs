using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;

        public UsuarioController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] bool includeRelations = false)
        {
            try
            {
                var usuarios = _usuarioService.GetAll(includeRelations);
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los usuarios", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var usuario = _usuarioService.GetById(id);
                return Ok(usuario);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el usuario", error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Create([FromBody] Usuario usuario)
        {
            try
            {
                if (usuario == null)
                    return BadRequest(new { message = "Los datos del usuario son inválidos." });

                _usuarioService.Create(usuario);
                return CreatedAtAction(nameof(GetById), new { id = usuario.IdUsuario }, usuario);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el usuario", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Usuario usuario)
        {
            try
            {
                if (usuario == null || usuario.IdUsuario != id)
                    return BadRequest(new { message = "Datos inválidos para actualizar el usuario." });

                _usuarioService.Update(usuario);
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
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el usuario", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _usuarioService.Delete(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el usuario", error = ex.Message });
            }
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.CorreoUsuario) || string.IsNullOrWhiteSpace(request.ContrasenaUsuario))
                    return BadRequest(new { message = "Correo y contraseña son obligatorios." });

                var usuario = _usuarioService.Authenticate(request.CorreoUsuario, request.ContrasenaUsuario);

                // Evitar devolver la contraseña
                usuario.ContrasenaUsuario = null!;

                return Ok(new
                {
                    message = "Autenticación exitosa",
                    usuario
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error en la autenticación del usuario", error = ex.Message });
            }
        }
    }


    public class LoginRequest
    {
        public string CorreoUsuario { get; set; } = null!;
        public string ContrasenaUsuario { get; set; } = null!;
    }
}
