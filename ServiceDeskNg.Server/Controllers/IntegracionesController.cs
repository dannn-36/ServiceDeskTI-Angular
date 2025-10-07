using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IntegracionesController : ControllerBase
    {
        private readonly ICrudRepository<Integracion> _repository;

        public IntegracionesController(ICrudRepository<Integracion> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Integracion>> GetAll()
        {
            var items = _repository.GetAll();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public ActionResult<Integracion> GetById(int id)
        {
            var item = _repository.GetById(id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Integracion entity)
        {
            if (entity == null)
                return BadRequest("La integración no puede ser nula.");
            _repository.Add(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.IdIntegracion }, entity);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Integracion entity)
        {
            if (entity == null || id != entity.IdIntegracion)
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
