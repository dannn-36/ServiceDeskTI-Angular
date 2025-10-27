using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EndUserController : ControllerBase
    {
        private readonly EndUserService _endUserService;

        public EndUserController(EndUserService endUserService)
        {
            _endUserService = endUserService;
        }

        // GET: api/enduser
        [HttpGet]
        public IActionResult GetAll([FromQuery] bool includeRelations = false)
        {
            try
            {
                var users = _endUserService.GetAll(includeRelations);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los usuarios finales", error = ex.Message });
            }
        }

        // GET: api/enduser/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var user = _endUserService.GetById(id);
                if (user == null)
                    return NotFound(new { message = "Usuario final no encontrado" });
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el usuario final", error = ex.Message });
            }
        }

        // POST: api/enduser
        [HttpPost]
        public IActionResult Create([FromBody] EndUser endUser)
        {
            try
            {
                _endUserService.Create(endUser);
                return CreatedAtAction(nameof(GetById), new { id = endUser.IdCliente }, endUser);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el usuario final", error = ex.Message });
            }
        }

        // PUT: api/enduser/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] EndUser endUser)
        {
            try
            {
                if (endUser == null || endUser.IdCliente != id)
                    return BadRequest(new { message = "Datos inválidos para actualizar el usuario final." });
                _endUserService.Update(endUser);
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
                return StatusCode(500, new { message = "Error al actualizar el usuario final", error = ex.Message });
            }
        }

        // DELETE: api/enduser/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _endUserService.Delete(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el usuario final", error = ex.Message });
            }
        }

        // GET: api/enduser/by-usuario/{idUsuario}
        [HttpGet("by-usuario/{idUsuario}")]
        public IActionResult GetByUsuarioId(int idUsuario)
        {
            try
            {
                var cliente = _endUserService.GetAll().FirstOrDefault(e => e.IdUsuario == idUsuario);
                if (cliente == null)
                    return NotFound(new { message = "No existe cliente para el usuario especificado." });
                return Ok(cliente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al buscar el cliente por usuario.", error = ex.Message });
            }
        }
    }
}