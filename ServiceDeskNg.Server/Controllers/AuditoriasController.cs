using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditoriasController : ControllerBase
    {
        private readonly ICrudRepository<Auditoria> _repository;

        public AuditoriasController(ICrudRepository<Auditoria> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Auditoria>> GetAll()
        {
            var items = _repository.GetAll();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public ActionResult<Auditoria> GetById(int id)
        {
            var item = _repository.GetById(id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Auditoria entity)
        {
            if (entity == null)
                return BadRequest("La auditoría no puede ser nula.");
            _repository.Add(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.IdAuditoria }, entity);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Auditoria entity)
        {
            if (entity == null || id != entity.IdAuditoria)
                return BadRequest("Datos inválidos.");
            _repository.Update(entity);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var item = _repository.GetById(id);
            if (item == null)
                return NotFound();
            _repository.Delete(id);
            return NoContent();
        }
    }
}
