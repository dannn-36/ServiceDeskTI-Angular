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

        [HttpGet]
        public ActionResult<IEnumerable<Usuario>> GetUsuarios()
        {
            return Ok(_repository.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<Usuario> GetUsuario(int id)
        {
            var usuario = _repository.GetById(id);
            if (usuario == null)
                return NotFound();

            return Ok(usuario);
        }

        [HttpPost]
        public IActionResult PostUsuario([FromBody] Usuario usuario)
        {
            if (usuario == null)
                return BadRequest();

            _repository.Add(usuario);
            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuario }, usuario);
        }

        [HttpPut("{id}")]
        public IActionResult PutUsuario(int id, [FromBody] Usuario usuario)
        {
            if (usuario == null || id != usuario.IdUsuario)
                return BadRequest();

            _repository.Update(usuario);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUsuario(int id)
        {
            var usuario = _repository.GetById(id);
            if (usuario == null)
                return NotFound();

            _repository.Delete(id);
            return NoContent();
        }
    }
}
