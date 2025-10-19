using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly TicketService _ticketService;
        private readonly TicketsCategoriaService _categoriaService;
        private readonly TicketsEstadoService _estadoService;

        public TicketsController(
            TicketService ticketService,
            TicketsCategoriaService categoriaService,
            TicketsEstadoService estadoService)
        {
            _ticketService = ticketService;
            _categoriaService = categoriaService;
            _estadoService = estadoService;
        }

        // ===========================================================
        // 🔹 OBTENER TODOS LOS TICKETS (con o sin relaciones)
        // GET: api/tickets?includeRelations=true
        // ===========================================================
        [HttpGet]
        public IActionResult GetAll([FromQuery] bool includeRelations = false)
        {
            try
            {
                var tickets = _ticketService.GetAll(includeRelations);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los tickets", error = ex.Message });
            }
        }

        // ===========================================================
        // 🔹 OBTENER TICKET POR ID
        // GET: api/tickets/{id}?includeRelations=true
        // ===========================================================
        [HttpGet("{id}")]
        public IActionResult GetById(int id, [FromQuery] bool includeRelations = false)
        {
            try
            {
                var ticket = _ticketService.GetById(id, includeRelations);
                return Ok(ticket);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el ticket", error = ex.Message });
            }
        }

        // ===========================================================
        // 🔹 OBTENER TICKETS POR CLIENTE
        // GET: api/tickets/cliente/{idCliente}
        // ===========================================================
        [HttpGet("cliente/{idCliente}")]
        public IActionResult GetByClienteId(int idCliente)
        {
            try
            {
                var tickets = _ticketService.GetByClienteId(idCliente);
                return Ok(tickets);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los tickets del cliente", error = ex.Message });
            }
        }


        // ===========================================================
        // 🔹 OBTENER TICKETS POR AGENTE ASIGNADO
        // GET: api/tickets/cliente/{idAgente}
        // ===========================================================

        [HttpGet("agente/{idAgente}")]
        public IActionResult GetByAgenteId(int idAgente)
        {
            try
            {
                var tickets = _ticketService.GetByAgenteId(idAgente);
                return Ok(tickets);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los tickets del agente", error = ex.Message });
            }
        }

        // ===========================================================
        // 🔹 CREAR NUEVO TICKET
        // POST: api/tickets
        // ===========================================================
        [HttpPost]
        public IActionResult Create([FromBody] Ticket entity)
        {
            if (entity == null)
                return BadRequest(new { message = "El cuerpo de la solicitud no puede ser nulo." });

            try
            {
                _ticketService.Create(entity);
                return CreatedAtAction(nameof(GetById), new { id = entity.IdTicket }, entity);
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
                return StatusCode(500, new { message = "Error al crear el ticket", error = ex.Message });
            }
        }

        // ===========================================================
        // 🔹 ACTUALIZAR TICKET EXISTENTE
        // PUT: api/tickets/{id}
        // ===========================================================
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Ticket entity)
        {
            if (entity == null || id != entity.IdTicket)
                return BadRequest(new { message = "Datos inválidos o ID no coincide." });

            try
            {
                _ticketService.Update(entity);
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
                return StatusCode(500, new { message = "Error al actualizar el ticket", error = ex.Message });
            }
        }

        // ===========================================================
        // 🔹 ELIMINAR TICKET POR ID
        // DELETE: api/tickets/{id}
        // ===========================================================
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _ticketService.Delete(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el ticket", error = ex.Message });
            }
        }

        // ===========================================================
        // 🔹 OBTENER CATEGORÍAS DE TICKETS
        // GET: api/tickets/categorias
        // ===========================================================
        [HttpGet("categorias")]
        public IActionResult GetCategorias()
        {
            try
            {
                var categorias = _categoriaService.GetAll();
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las categorías de tickets", error = ex.Message });
            }
        }

        // ===========================================================
        // 🔹 OBTENER ESTADOS DE TICKETS
        // GET: api/tickets/estados
        // ===========================================================
        [HttpGet("estados")]
        public IActionResult GetEstados()
        {
            try
            {
                var estados = _estadoService.GetAll();
                return Ok(estados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los estados de tickets", error = ex.Message });
            }
        }
    }
}
