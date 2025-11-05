using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Services;
using System.Linq;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketMensajeController : ControllerBase
    {
        private readonly TicketMensajeService _ticketMensajeService;
        private readonly ServiceDeskNg.Server.Data.ServiceDeskContext _context;

        public TicketMensajeController(TicketMensajeService ticketMensajeService, ServiceDeskNg.Server.Data.ServiceDeskContext context)
        {
            _ticketMensajeService = ticketMensajeService;
            _context = context;
        }

        // ===========================================================
        // ✅ GET: api/TicketMensaje
        // Obtiene todos los mensajes registrados
        // ===========================================================
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var mensajes = _ticketMensajeService.GetAll();
                return Ok(mensajes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los mensajes.", error = ex.Message });
            }
        }

        // ===========================================================
        // ✅ GET: api/TicketMensaje/{id}
        // Obtiene un mensaje específico por su ID
        // ===========================================================
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var mensaje = _ticketMensajeService.GetById(id);
                return Ok(mensaje);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el mensaje.", error = ex.Message });
            }
        }

        // ===========================================================
        // ✅ GET: api/TicketMensaje/ticket/{ticketId}
        // Obtiene todos los mensajes asociados a un ticket específico
        // ===========================================================
        [HttpGet("ticket/{ticketId}")]
        public IActionResult GetByTicketId(int ticketId, [FromQuery] bool includeRelations = false)
        {
            try
            {
                var mensajes = _ticketMensajeService.GetMensajesByTicketId(ticketId, true);
                var mensajesDto = mensajes.Select(m => new TicketMensajeDto
                {
                    IdMensaje = m.IdMensaje,
                    IdTicket = m.IdTicket,
                    IdUsuario = m.IdUsuario,
                    MensajeTicket = m.MensajeTicket,
                    FechaHoraCreacionMensaje = m.FechaHoraCreacionMensaje,
                    UsuarioNombre = m.IdUsuarioNavigation != null ? m.IdUsuarioNavigation.NombreUsuario : "Desconocido"
                }).ToList();
                return Ok(mensajesDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los mensajes del ticket.", error = ex.Message });
            }
        }

        // ===========================================================
        // ✅ POST: api/TicketMensaje
        // Crea un nuevo mensaje
        // ===========================================================
        [HttpPost]
        public IActionResult Create([FromBody] TicketMensaje mensaje)
        {
            try
            {
                if (mensaje == null)
                    return BadRequest(new { message = "El cuerpo de la solicitud no puede estar vacío." });

                // Fecha de creación si no se envía
                mensaje.FechaHoraCreacionMensaje ??= DateTime.Now;

                _ticketMensajeService.Create(mensaje);

                return CreatedAtAction(nameof(GetById), new { id = mensaje.IdMensaje }, mensaje);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = "Datos inválidos: " + ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el mensaje.", error = ex.Message });
            }
        }

        // ===========================================================
        // ✅ DELETE: api/TicketMensaje/{id}
        // Elimina un mensaje por su ID
        // ===========================================================
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _ticketMensajeService.Delete(id);
                return Ok(new { message = $"Mensaje con ID {id} eliminado correctamente." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el mensaje.", error = ex.Message });
            }
        }
    }
}
