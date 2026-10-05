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
    public class EndUserController : ControllerBase
    {
        private readonly EndUserService _clientes;

        public EndUserController(EndUserService clientes)
        {
            _clientes = clientes;
        }

        /// El personal necesita la lista para abrir tickets en nombre de un cliente.
        [HttpGet]
        [Authorize(Roles = RolesApp.Personal)]
        public async Task<ActionResult<List<EndUserDto>>> GetAll(CancellationToken ct) =>
            Ok(await _clientes.ListarAsync(ct));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EndUserDto>> GetById(int id, CancellationToken ct)
        {
            if (!User.TieneVisionGlobal() && User.IdCliente() != id)
                throw new AccesoDenegadoException();

            return Ok(await _clientes.ObtenerAsync(id, ct));
        }

        [HttpGet("by-usuario/{idUsuario:int}")]
        public async Task<ActionResult<EndUserDto>> GetByUsuarioId(int idUsuario, CancellationToken ct)
        {
            if (!User.TieneVisionGlobal() && User.IdUsuario() != idUsuario)
                throw new AccesoDenegadoException();

            return Ok(await _clientes.ObtenerPorUsuarioAsync(idUsuario, ct));
        }

        [HttpPost]
        [Authorize(Roles = RolesApp.Administrador)]
        public async Task<ActionResult<EndUserDto>> Create([FromBody] EndUserCreateDto dto, CancellationToken ct)
        {
            var creado = await _clientes.CrearAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = creado.IdCliente }, creado);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = RolesApp.Administrador)]
        public async Task<IActionResult> Update(int id, [FromBody] EndUserCreateDto dto, CancellationToken ct)
        {
            await _clientes.ActualizarAsync(id, dto, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = RolesApp.Administrador)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _clientes.EliminarAsync(id, ct);
            return NoContent();
        }
    }
}
