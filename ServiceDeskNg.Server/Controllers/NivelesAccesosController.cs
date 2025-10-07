using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NivelesAccesosController : ControllerBase
    {
        private readonly ICrudRepository<NivelesAcceso> _repository;

        public NivelesAccesosController(ICrudRepository<NivelesAcceso> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<NivelesAcceso>> GetAll()
        {
            var items = _repository.GetAll();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public ActionResult<NivelesAcceso> GetById(int id)
        {
            var item = _repository.GetById(id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] NivelesAcceso entity)
        {
            if (entity == null)
                return BadRequest("El nivel de acceso no puede ser nulo.");
            _repository.Add(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.IdNivel }, entity);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] NivelesAcceso entity)
        {
            if (entity == null || id != entity.IdNivel)
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
