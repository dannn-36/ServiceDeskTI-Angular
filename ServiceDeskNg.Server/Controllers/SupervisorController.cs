using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Security;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = RolesApp.Administrador)]
    public class SupervisorController : ControllerBase
    {
        private readonly SupervisorService _supervisores;

        public SupervisorController(SupervisorService supervisores)
        {
            _supervisores = supervisores;
        }

        [HttpGet]
        public async Task<ActionResult<List<SupervisorDto>>> GetAll(CancellationToken ct) =>
            Ok(await _supervisores.ListarAsync(ct));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SupervisorDto>> GetById(int id, CancellationToken ct) =>
            Ok(await _supervisores.ObtenerAsync(id, ct));

        [HttpPost]
        public async Task<ActionResult<SupervisorDto>> Create([FromBody] SupervisorCreateDto dto, CancellationToken ct)
        {
            var creado = await _supervisores.CrearAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = creado.IdSupervisor }, creado);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] SupervisorCreateDto dto, CancellationToken ct)
        {
            await _supervisores.ActualizarAsync(id, dto, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _supervisores.EliminarAsync(id, ct);
            return NoContent();
        }
    }
}
