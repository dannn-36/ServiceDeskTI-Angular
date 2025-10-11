using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminModuleController : ControllerBase
    {
        private readonly ICrudRepository<Administrador> _adminRe;
        private readonly ICrudRepository<Usuario> _usuarioRe;
        private readonly ICrudRepository<NivelesAcceso> _nivelAccesoRe;
        private readonly ICrudRepository<Agente> _agenteRe;
        private readonly ICrudRepository<EndUser> _EndUserRe;
        private readonly ICrudRepository<Sesion> _sesionRe;
        private readonly ICrudRepository<Supervisor> _supervisorRe;
        

        public AdminModuleController(ICrudRepository<Administrador> repository)
        {
            _adminRe = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Administrador>> GetAll()
        {
            var items = _adminRe.GetAll();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public ActionResult<Administrador> GetById(int id)
        {
            var item = _adminRe.GetById(id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Administrador entity)
        {
            if (entity == null)
                return BadRequest("El administrador no puede ser nulo.");
            _adminRe.Add(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.IdAdmin }, entity);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Administrador entity)
        {
            if (entity == null || id != entity.IdAdmin)
                return BadRequest("Datos inválidos.");
            _adminRe.Update(entity);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var item = _adminRe.GetById(id);
            if (item == null)
                return NotFound();
            _adminRe.Delete(id);
            return NoContent();
        }
    }
}

