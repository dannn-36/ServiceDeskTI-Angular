using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly ICrudRepository<Usuario> _repository;

        public UsuariosController(ICrudRepository<Usuario> repository)
        {
            _repository = repository;
        }

        // GET: api/Usuarios
        [HttpGet]
        public ActionResult<IEnumerable<Usuario>> GetAll()
        {
            var usuarios = _repository.GetAll();
            return Ok(usuarios);
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public ActionResult<Usuario> GetById(int id)
        {
            var usuario = _repository.GetById(id);
            if (usuario == null)
                return NotFound();

            return Ok(usuario);
        }

        // POST: api/Usuarios
        [HttpPost]
        public IActionResult Create([FromBody] Usuario usuario)
        {
            if (usuario == null)
                return BadRequest("El usuario no puede ser nulo.");

            _repository.Add(usuario);
            return CreatedAtAction(nameof(GetById), new { id = usuario.IdUsuario }, usuario);
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Usuario usuario)
        {
            if (usuario == null || id != usuario.IdUsuario)
                return BadRequest("Datos inválidos.");

            _repository.Update(usuario);
            return NoContent();
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var usuario = _repository.GetById(id);
            if (usuario == null)
                return NotFound();

            _repository.Delete(id);
            return NoContent();
        }
    }
}
