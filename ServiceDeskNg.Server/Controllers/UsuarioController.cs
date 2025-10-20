using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Controllers
{
    /// <summary>
    /// Controlador de API para operaciones sobre usuarios.
    /// Ruta base: api/Usuario
    /// Provee endpoints CRUD y autenticación (login).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;

        public UsuarioController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        /// <summary>
        /// GET api/Usuario
        /// Obtiene todos los usuarios. Si se especifica includeRelations=true, devuelve también las relaciones
        /// (por ejemplo, administradores/agentes asociados) para facilitar vistas detalladas.
        /// </summary>
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

        /// <summary>
        /// GET api/Usuario/{id}
        /// Obtiene un usuario por su identificador.
        /// Retorna 404 si no se encuentra.
        /// </summary>
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

        /// <summary>
        /// POST api/Usuario
        /// Crea un nuevo usuario. Espera un objeto Usuario en el body.
        /// Retorna 201 con la ubicación del nuevo recurso.
        /// </summary>
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

        /// <summary>
        /// PUT api/Usuario/{id}
        /// Actualiza un usuario existente. El ID en la ruta debe coincidir con usuario.IdUsuario.
        /// Retorna 204 en caso de éxito.
        /// </summary>
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

        /// <summary>
        /// DELETE api/Usuario/{id}
        /// Elimina un usuario por su ID. Retorna 204 en caso de éxito o 404 si no existe.
        /// </summary>
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


        /// <summary>
        /// POST api/Usuario/login
        /// Endpoint para autenticación básica. Recibe correo y contraseña.
        /// Si la autenticación es correcta, devuelve el usuario (sin contraseña) y un mensaje de éxito.
        /// </summary>
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


    /// <summary>
    /// DTO para la petición de login.
    /// Contiene las credenciales necesarias para la autenticación.
    /// </summary>
    public class LoginRequest
    {
        public string CorreoUsuario { get; set; } = null!;
        public string ContrasenaUsuario { get; set; } = null!;
    }
}
