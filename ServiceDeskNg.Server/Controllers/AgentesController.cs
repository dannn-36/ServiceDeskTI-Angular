using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentesController : ControllerBase
    {
        private readonly ICrudRepository<Agente> _repository;

        public AgentesController(ICrudRepository<Agente> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Agente>> GetAll()
        {
            var items = _repository.GetAll();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public ActionResult<Agente> GetById(int id)
        {
            var item = _repository.GetById(id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Agente entity)
        {
            if (entity == null)
                return BadRequest("El agente no puede ser nulo.");
            _repository.Add(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.IdAgente }, entity);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Agente entity)
        {
            if (entity == null || id != entity.IdAgente)
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
