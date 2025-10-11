using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly ICrudRepository<EndUser> _repository;

        public ClientesController(ICrudRepository<EndUser> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<EndUser>> GetAll()
        {
            var items = _repository.GetAll();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public ActionResult<EndUser> GetById(int id)
        {
            var item = _repository.GetById(id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] EndUser entity)
        {
            if (entity == null)
                return BadRequest("El cliente no puede ser nulo.");
            _repository.Add(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.IdCliente }, entity);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] EndUser entity)
        {
            if (entity == null || id != entity.IdCliente)
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
