using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SesionesController : ControllerBase
    {
        private readonly ICrudRepository<Sesion> _repository;

        public SesionesController(ICrudRepository<Sesion> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Sesion>> GetAll()
        {
            var items = _repository.GetAll();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public ActionResult<Sesion> GetById(int id)
        {
            var item = _repository.GetById(id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Sesion entity)
        {
            if (entity == null)
                return BadRequest("La sesión no puede ser nula.");
            _repository.Add(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.IdSesion }, entity);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Sesion entity)
        {
            if (entity == null || id != entity.IdSesion)
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
