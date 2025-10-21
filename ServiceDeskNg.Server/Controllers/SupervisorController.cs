using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Data;
using System.Linq;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupervisorController : ControllerBase
    {
        private readonly ServiceDeskContext _context;
        public SupervisorController(ServiceDeskContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var supervisores = _context.Supervisores.ToList();
            return Ok(supervisores);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var supervisor = _context.Supervisores.Find(id);
            if (supervisor == null) return NotFound();
            return Ok(supervisor);
        }

        [HttpPost]
        public IActionResult Create([FromBody] SupervisorCreateDto dto)
        {
            var supervisor = new Supervisor
            {
                IdUsuario = dto.IdUsuario,
                IdNivel = dto.IdNivel,
                AreaResponsabilidadSupervisor = dto.AreaResponsabilidadSupervisor
            };
            _context.Supervisores.Add(supervisor);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = supervisor.IdSupervisor }, supervisor);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] SupervisorCreateDto dto)
        {
            var supervisor = _context.Supervisores.Find(id);
            if (supervisor == null) return NotFound();
            supervisor.IdNivel = dto.IdNivel;
            supervisor.AreaResponsabilidadSupervisor = dto.AreaResponsabilidadSupervisor;
            _context.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var supervisor = _context.Supervisores.Find(id);
            if (supervisor == null) return NotFound();
            _context.Supervisores.Remove(supervisor);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
