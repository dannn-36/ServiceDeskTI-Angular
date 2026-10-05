using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Security;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Controllers
{
    /// Consulta de la bitácora de auditoría (solo lectura).
    /// Ya no existe un POST: los registros los escribe únicamente el servidor,
    /// porque una bitácora que el cliente puede rellenar no prueba nada.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = RolesApp.Administrador)]
    public class AuditoriaController : ControllerBase
    {
        private readonly AuditoriaService _auditoria;

        public AuditoriaController(AuditoriaService auditoria)
        {
            _auditoria = auditoria;
        }

        /// GET api/auditoria?limite=200&amp;accion=LOGIN_FALLIDO
        [HttpGet]
        public async Task<ActionResult<List<AuditoriaDto>>> GetAll(
            [FromQuery] int limite = 200,
            [FromQuery] string? accion = null,
            CancellationToken ct = default) =>
            Ok(await _auditoria.ListarAsync(limite, accion, ct));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AuditoriaDto>> GetById(int id, CancellationToken ct) =>
            Ok(await _auditoria.ObtenerAsync(id, ct));
    }
}
