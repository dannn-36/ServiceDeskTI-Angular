using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Services;
using System.Linq;

namespace ServiceDeskNg.Server.Controllers
{

    /// Controlador de API para operaciones sobre usuarios.
    /// Ruta base: api/Usuario
    /// Provee endpoints CRUD y autenticación (login).

    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;

        public UsuarioController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

     
        /// GET api/Usuario
        /// Obtiene todos los usuarios. Si se especifica includeRelations=true, devuelve también las relaciones
        /// (por ejemplo, administradores/agentes asociados) para facilitar vistas detalladas.
      
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var usuariosDto = _usuarioService.GetAllUsuariosDto();
                return Ok(usuariosDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los usuarios", error = ex.Message });
            }
        }

     
        /// GET api/Usuario/{id}
        /// Obtiene un usuario por su identificador.
        /// Retorna 404 si no se encuentra.
       
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

      
        /// POST api/Usuario
        /// Crea un nuevo usuario. Espera un objeto Usuario en el body.
        /// Retorna 201 con la ubicación del nuevo recurso.
        [HttpPost]
        public IActionResult Create([FromBody] UsuarioCreateDto usuarioDto)
        {
            try
            {
                if (usuarioDto == null)
                    return BadRequest(new { message = "Los datos del usuario son inválidos." });

                var usuario = new Usuario
                {
                    NombreUsuario = usuarioDto.NombreUsuario,
                    CorreoUsuario = usuarioDto.CorreoUsuario,
                    ContrasenaUsuario = usuarioDto.ContrasenaUsuario,
                    DepartamentoUsuario = usuarioDto.DepartamentoUsuario,
                    EstadoUsuario = usuarioDto.EstadoUsuario,
                    UbicacionUsuario = usuarioDto.UbicacionUsuario
                };

                _usuarioService.Create(usuario);

                // Obtener el usuario recién creado por correo para asegurar el IdUsuario
                var usuarioCreado = _usuarioService.GetByCorreo(usuario.CorreoUsuario);
                if (usuarioCreado != null)
                {
                    usuario.IdUsuario = usuarioCreado.IdUsuario;
                }

                string tipo = usuarioDto.TipoUsuario;
                if (tipo == "Cliente")
                {
                    var nivel = _usuarioService.GetNivelAccesoPorNombre("EndUser");
                    var endUser = new EndUser
                    {
                        IdUsuario = usuario.IdUsuario,
                        IdNivel = nivel?.IdNivel ?? 1
                    };
                    _usuarioService.CrearEndUser(endUser);
                }
                else if (tipo == "Agente")
                {
                    var nivel = _usuarioService.GetNivelAccesoPorNombre("Agente");
                    var agente = new Agente
                    {
                        IdUsuario = usuario.IdUsuario,
                        IdNivel = nivel?.IdNivel ?? 1
                    };
                    _usuarioService.CrearAgente(agente);
                }
                else if (tipo == "Supervisor")
                {
                    var nivel = _usuarioService.GetNivelAccesoPorNombre("Supervisor");
                    var supervisor = new Supervisor
                    {
                        IdUsuario = usuario.IdUsuario,
                        IdNivel = nivel?.IdNivel ?? 1
                    };
                    _usuarioService.CrearSupervisor(supervisor);
                }
                else if (tipo == "Administrador")
                {
                    var nivel = _usuarioService.GetNivelAccesoPorNombre("Administrador");
                    var admin = new Administrador
                    {
                        IdUsuario = usuario.IdUsuario,
                        IdNivel = nivel?.IdNivel ?? 1
                    };
                    _usuarioService.CrearAdministrador(admin);
                }

                return CreatedAtAction(nameof(GetById), new { id = usuario.IdUsuario }, usuario);
            }
            catch (Exception ex)
            {
                // Log detallado para depuración
                System.Console.WriteLine("ERROR AL CREAR USUARIO: " + ex.ToString());
                return StatusCode(500, new { message = "Error al crear el usuario", error = ex.Message });
            }
        }

        /// PUT api/Usuario/{id}
        /// Actualiza un usuario existente. El ID en la ruta debe coincidir con usuario.IdUsuario.
        /// Retorna 204 en caso de éxito.
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UsuarioUpdateDto usuarioDto)
        {
            try
            {
                if (usuarioDto == null)
                    return BadRequest(new { message = "Datos inválidos para actualizar el usuario." });

                var usuario = _usuarioService.GetById(id);
                if (usuario == null)
                    return NotFound(new { message = "Usuario no encontrado." });

                usuario.NombreUsuario = usuarioDto.NombreUsuario;
                usuario.CorreoUsuario = usuarioDto.CorreoUsuario;
                if (!string.IsNullOrWhiteSpace(usuarioDto.ContrasenaUsuario))
                {
                    usuario.ContrasenaUsuario = BCrypt.Net.BCrypt.HashPassword(usuarioDto.ContrasenaUsuario);
                }
                usuario.DepartamentoUsuario = usuarioDto.DepartamentoUsuario;
                usuario.EstadoUsuario = usuarioDto.EstadoUsuario;
                usuario.UbicacionUsuario = usuarioDto.UbicacionUsuario;

                _usuarioService.Update(usuario);
                return NoContent();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("ERROR AL ACTUALIZAR USUARIO: " + ex.ToString());
                return StatusCode(500, new { message = "Error al actualizar el usuario", error = ex.Message });
            }
        }

        /// DELETE api/Usuario/{id}
        /// Elimina un usuario por su ID. Retorna 204 en caso de éxito o 404 si no existe.
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


     
        
    }


   
}
