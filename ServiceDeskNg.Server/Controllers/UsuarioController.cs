using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Security;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Controllers
{
    /// Gestión de usuarios.
    /// Administración tiene control total; cualquier usuario puede ver y editar su propio perfil.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _usuarios;
        private readonly AuditoriaService _auditoria;

        public UsuarioController(UsuarioService usuarios, AuditoriaService auditoria)
        {
            _usuarios = usuarios;
            _auditoria = auditoria;
        }

        [HttpGet]
        [Authorize(Roles = RolesApp.Administrador)]
        public async Task<ActionResult<List<UsuarioDto>>> GetAll(CancellationToken ct) =>
            Ok(await _usuarios.ListarAsync(ct));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UsuarioDto>> GetById(int id, CancellationToken ct)
        {
            AsegurarAdministradorOPropietario(id);
            return Ok(await _usuarios.ObtenerDtoAsync(id, ct));
        }

        [HttpPost]
        [Authorize(Roles = RolesApp.Administrador)]
        public async Task<ActionResult<UsuarioDto>> Create(
            [FromBody] UsuarioCreateDto dto,
            CancellationToken ct)
        {
            var creado = await _usuarios.CrearConRolAsync(dto, ct);

            await _auditoria.RegistrarAsync(
                User.IdUsuario(),
                AccionesAuditoria.UsuarioCreado,
                $"Usuario {creado.IdUsuario} ({creado.CorreoUsuario}) creado como {creado.TipoUsuario}",
                ct);

            return CreatedAtAction(nameof(GetById), new { id = creado.IdUsuario }, creado);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UsuarioUpdateDto dto,
            CancellationToken ct)
        {
            AsegurarAdministradorOPropietario(id);

            await _usuarios.ActualizarAsync(id, dto, puedeAdministrar: User.EsAdministrador(), ct);

            await _auditoria.RegistrarAsync(
                User.IdUsuario(),
                AccionesAuditoria.UsuarioActualizado,
                string.IsNullOrWhiteSpace(dto.ContrasenaUsuario)
                    ? $"Usuario {id} actualizado"
                    : $"Usuario {id} actualizado (incluye cambio de contraseña)",
                ct);

            return NoContent();
        }

        /// Elimina al usuario, o lo desactiva si tiene historial que conservar.
        /// La respuesta indica cuál de las dos cosas ocurrió.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = RolesApp.Administrador)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var resultado = await _usuarios.DarDeBajaAsync(id, User.IdUsuario(), ct);

            await _auditoria.RegistrarAsync(
                User.IdUsuario(),
                AccionesAuditoria.UsuarioEliminado,
                resultado.Eliminado ? $"Usuario {id} eliminado" : $"Usuario {id} desactivado",
                ct);

            return Ok(new { message = resultado.Mensaje, eliminado = resultado.Eliminado });
        }

        private void AsegurarAdministradorOPropietario(int idUsuario)
        {
            if (!User.EsAdministrador() && User.IdUsuario() != idUsuario)
                throw new AccesoDenegadoException("Solo puede consultar o modificar su propio perfil.");
        }
    }
}
