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
    public class AdministradorController : ControllerBase
    {
        private readonly AdministradorService _administradores;

        public AdministradorController(AdministradorService administradores)
        {
            _administradores = administradores;
        }

        [HttpGet]
        public async Task<ActionResult<List<AdministradorDto>>> GetAll(CancellationToken ct) =>
            Ok(await _administradores.ListarAsync(ct));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AdministradorDto>> GetById(int id, CancellationToken ct) =>
            Ok(await _administradores.ObtenerAsync(id, ct));

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] AdministradorUpdateDto dto, CancellationToken ct)
        {
            await _administradores.ActualizarAsync(id, dto, ct);
            return NoContent();
        }
    }
}
