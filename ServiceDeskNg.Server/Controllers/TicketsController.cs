using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ICrudRepository<Ticket> _repository;

        public TicketsController(ICrudRepository<Ticket> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Ticket>> GetAll()
        {
            var items = _repository.GetAll();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public ActionResult<Ticket> GetById(int id)
        {
            var item = _repository.GetById(id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Ticket entity)
        {
            if (entity == null)
                return BadRequest("El ticket no puede ser nulo.");
            _repository.Add(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.IdTicket }, entity);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Ticket entity)
        {
            if (entity == null || id != entity.IdTicket)
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
