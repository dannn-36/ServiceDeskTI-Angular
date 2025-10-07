using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsEstadosController : ControllerBase
    {
        private readonly ICrudRepository<TicketsEstado> _repository;

        public TicketsEstadosController(ICrudRepository<TicketsEstado> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TicketsEstado>> GetAll()
        {
            var items = _repository.GetAll();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public ActionResult<TicketsEstado> GetById(int id)
        {
            var item = _repository.GetById(id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] TicketsEstado entity)
        {
            if (entity == null)
                return BadRequest("El estado de ticket no puede ser nulo.");
            _repository.Add(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.IdEstado }, entity);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] TicketsEstado entity)
        {
            if (entity == null || id != entity.IdEstado)
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
