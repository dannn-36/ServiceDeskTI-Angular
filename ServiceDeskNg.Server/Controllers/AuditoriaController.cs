using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditoriaController : ControllerBase
    {
        private readonly AuditoriaService _auditoriaService;

        public AuditoriaController(AuditoriaService auditoriaService)
        {
            _auditoriaService = auditoriaService;
        }

        // ===========================================================
        // 🔹 OBTENER TODAS LAS AUDITORÍAS 
        // GET: api/auditoria?includeRelations=true
        // ===========================================================
        [HttpGet]
        public IActionResult GetAll([FromQuery] bool includeRelations = false)
        {
            try
            {
                var auditorias = _auditoriaService.GetAll(includeRelations);
                return Ok(auditorias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener las auditorías.",
                    error = ex.Message
                });
            }
        }

        // ===========================================================
        // 🔹 OBTENER AUDITORÍA POR ID
        // GET: api/auditoria/{id}
        // ===========================================================
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var auditoria = _auditoriaService.GetById(id);
                return Ok(auditoria);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener la auditoría.",
                    error = ex.Message
                });
            }
        }

        // ===========================================================
        // 🔹 CREAR NUEVA AUDITORÍA
        // POST: api/auditoria
        // ===========================================================
        [HttpPost]
        public IActionResult Create([FromBody] Auditoria entity)
        {
            if (entity == null)
                return BadRequest(new { message = "No puede ser nulo." });

            try
            {
                _auditoriaService.Create(entity);
                return CreatedAtAction(nameof(GetById),
                    new { id = entity.IdAuditoria },
                    entity);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al crear la auditoría.",
                    error = ex.Message
                });
            }
        }
    }
}
