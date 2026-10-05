using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Security;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AgenteController : ControllerBase
    {
        private readonly AgenteService _agentes;

        public AgenteController(AgenteService agentes)
        {
            _agentes = agentes;
        }

        [HttpGet]
        [Authorize(Roles = RolesApp.Gestion)]
        public async Task<ActionResult<List<AgenteDto>>> GetAll(CancellationToken ct) =>
            Ok(await _agentes.ListarAsync(ct));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AgenteDto>> GetById(int id, CancellationToken ct)
        {
            AsegurarGestionOElPropioAgente(id);
            return Ok(await _agentes.ObtenerAsync(id, ct));
        }

        [HttpGet("by-usuario/{idUsuario:int}")]
        public async Task<ActionResult<AgenteDto>> GetByUsuario(int idUsuario, CancellationToken ct)
        {
            if (!User.TieneVisionGlobal() && User.IdUsuario() != idUsuario)
                throw new AccesoDenegadoException();

            return Ok(await _agentes.ObtenerPorUsuarioAsync(idUsuario, ct));
        }

        [HttpPost]
        [Authorize(Roles = RolesApp.Administrador)]
        public async Task<ActionResult<AgenteDto>> Create([FromBody] AgenteCreateDto dto, CancellationToken ct)
        {
            var creado = await _agentes.CrearAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = creado.IdAgente }, creado);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = RolesApp.Administrador)]
        public async Task<IActionResult> Update(int id, [FromBody] AgenteCreateDto dto, CancellationToken ct)
        {
            await _agentes.ActualizarAsync(id, dto, ct);
            return NoContent();
        }

        /// El propio agente (o supervisión) indica si puede recibir tickets nuevos.
        [HttpPut("{id:int}/disponibilidad")]
        public async Task<IActionResult> CambiarDisponibilidad(
            int id,
            [FromBody] CambioDisponibilidadRequest request,
            CancellationToken ct)
        {
            AsegurarGestionOElPropioAgente(id);
            await _agentes.CambiarDisponibilidadAsync(id, request.Disponible, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = RolesApp.Administrador)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _agentes.EliminarAsync(id, ct);
            return NoContent();
        }

        private void AsegurarGestionOElPropioAgente(int idAgente)
        {
            if (!User.TieneVisionGlobal() && User.IdAgente() != idAgente)
                throw new AccesoDenegadoException();
        }
    }

    public class CambioDisponibilidadRequest
    {
        public bool Disponible { get; set; }
    }
}
