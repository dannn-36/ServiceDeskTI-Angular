using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketMensajesController : ControllerBase
    {
        private readonly ICrudRepository<TicketMensaje> _repository;

        public TicketMensajesController(ICrudRepository<TicketMensaje> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TicketMensaje>> GetAll()
        {
            var items = _repository.GetAll();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public ActionResult<TicketMensaje> GetById(int id)
        {
            var item = _repository.GetById(id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] TicketMensaje entity)
        {
            if (entity == null)
                return BadRequest("El mensaje de ticket no puede ser nulo.");
            _repository.Add(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.IdMensaje }, entity);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] TicketMensaje entity)
        {
            if (entity == null || id != entity.IdMensaje)
                return BadRequest("Datos inv�lidos.");
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
