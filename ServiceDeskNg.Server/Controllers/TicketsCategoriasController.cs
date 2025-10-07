using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsCategoriasController : ControllerBase
    {
        private readonly ICrudRepository<TicketsCategoria> _repository;

        public TicketsCategoriasController(ICrudRepository<TicketsCategoria> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TicketsCategoria>> GetAll()
        {
            var items = _repository.GetAll();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public ActionResult<TicketsCategoria> GetById(int id)
        {
            var item = _repository.GetById(id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] TicketsCategoria entity)
        {
            if (entity == null)
                return BadRequest("La categoría de ticket no puede ser nula.");
            _repository.Add(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.IdCategoria }, entity);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] TicketsCategoria entity)
        {
            if (entity == null || id != entity.IdCategoria)
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
