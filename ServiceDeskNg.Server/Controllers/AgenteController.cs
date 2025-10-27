using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Data;
using System.Linq;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgenteController : ControllerBase
    {
        private readonly ServiceDeskContext _context;
        public AgenteController(ServiceDeskContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var agentes = _context.Agentes.ToList();
            return Ok(agentes);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var agente = _context.Agentes.Find(id);
            if (agente == null) return NotFound();
            return Ok(agente);
        }

        [HttpPost]
        public IActionResult Create([FromBody] AgenteCreateDto dto)
        {
            var agente = new Agente
            {
                IdUsuario = dto.IdUsuario,
                IdNivel = dto.IdNivel,
                EspecialidadAgente = dto.EspecialidadAgente,
                DisponibilidadAgente = dto.DisponibilidadAgente ?? true
            };
            _context.Agentes.Add(agente);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = agente.IdAgente }, agente);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] AgenteCreateDto dto)
        {
            var agente = _context.Agentes.Find(id);
            if (agente == null) return NotFound();
            agente.IdNivel = dto.IdNivel;
            agente.EspecialidadAgente = dto.EspecialidadAgente;
            agente.DisponibilidadAgente = dto.DisponibilidadAgente ?? true;
            _context.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var agente = _context.Agentes.Find(id);
            if (agente == null) return NotFound();
            _context.Agentes.Remove(agente);
            _context.SaveChanges();
            return NoContent();
        }

        [HttpGet("by-usuario/{idUsuario}")]
        public IActionResult GetByUsuario(int idUsuario)
        {
            var agente = _context.Agentes.FirstOrDefault(a => a.IdUsuario == idUsuario);
            if (agente == null) return NotFound();
            return Ok(agente);
        }
    }
}
