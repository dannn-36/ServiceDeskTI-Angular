using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdministradorController : ControllerBase
    {
        private readonly AdministradorService _administradorService;
        private readonly IConfiguration _configuration;
       
        public AdministradorController(
            AdministradorService administradorService)
        {
            _administradorService = administradorService;
        }
        // ===========================================================
        // 🔹 OBTENER TODOS LOS ADMINISTRADORES (con o sin relaciones)
        // GET: api/administrador?includeRelations=true
        // ===========================================================
        [HttpGet]
        public IActionResult GetAll([FromQuery] bool includeRelations = false)
        {
            try
            {
                var administradores = _administradorService.GetAll(includeRelations);
                return Ok(administradores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los administradores", error = ex.Message });
            }
        }
        // ===========================================================
        // 🔹 OBTENER ADMINISTRADOR POR ID
        // GET: api/administrador/{id}?includeRelations=true
        // ===========================================================
        [HttpGet("{id}")]
        public IActionResult GetById(int id, [FromQuery] bool includeRelations = false)
        {
            try
            {
                var administrador = _administradorService.GetById(id, includeRelations);
                return Ok(administrador);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el administrador", error = ex.Message });
            }
        }
        // ===========================================================
        // 🔹 CREAR NUEVO ADMINISTRADOR
        // POST: api/administrador
        // ===========================================================
        [HttpPost]
        public IActionResult Create([FromBody] Administrador administrador)
        {
            try
            {
                _administradorService.Create(administrador);
                return CreatedAtAction(nameof(GetById), new { id = administrador.IdAdmin }, administrador);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el administrador", error = ex.Message });
            }
        }
        // ===========================================================
        // 🔹 ACTUALIZAR ADMINISTRADOR EXISTENTE
        // PUT: api/administrador/{id}
        // ===========================================================
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Administrador administrador)
        {
            try
            {
                if (administrador == null || administrador.IdAdmin != id)
                    return BadRequest(new { message = "Datos inválidos para actualizar el administrador." });
                _administradorService.Update(administrador);
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
                return StatusCode(500, new { message = "Error al actualizar el administrador", error = ex.Message });
            }
        }
    }
}
