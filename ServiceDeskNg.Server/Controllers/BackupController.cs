using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Security;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Controllers
{
    /// Respaldo y restauración de la base de datos. Exclusivo de administración
    /// (antes era público: cualquiera podía descargar la base entera o sobrescribirla).
    [ApiController]
    [Route("api/backup")]
    [Authorize(Roles = RolesApp.Administrador)]
    public class BackupController : ControllerBase
    {
        /// Límite de subida para la restauración (200 MB).
        private const long LimiteSubida = 200L * 1024 * 1024;

        private readonly RespaldoService _respaldo;
        private readonly AuditoriaService _auditoria;

        public BackupController(RespaldoService respaldo, AuditoriaService auditoria)
        {
            _respaldo = respaldo;
            _auditoria = auditoria;
        }

        // GET api/backup
        [HttpGet]
        public async Task<IActionResult> CreateBackup(CancellationToken ct)
        {
            var volcado = await _respaldo.GenerarRespaldoAsync(ct);
            var nombre = $"backup_{_respaldo.NombreBaseDatos}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.sql";

            await _auditoria.RegistrarAsync(
                User.IdUsuario(),
                AccionesAuditoria.RespaldoDescargado,
                $"{nombre} ({volcado.Length / 1024} KB)",
                ct);

            return File(volcado, "application/sql", nombre);
        }

        // POST api/backup/restore   (multipart/form-data, campo "archivo")
        [HttpPost("restore")]
        [RequestSizeLimit(LimiteSubida)]
        [RequestFormLimits(MultipartBodyLengthLimit = LimiteSubida)]
        public async Task<IActionResult> RestoreBackup(IFormFile? archivo, CancellationToken ct)
        {
            if (archivo is null || archivo.Length == 0)
                return BadRequest(new { message = "No se recibió ningún archivo de respaldo." });

            if (!archivo.FileName.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "El respaldo debe ser un archivo .sql." });

            if (archivo.Length > _respaldo.TamanoMaximoBytes)
                return BadRequest(new { message = "El archivo supera el tamaño máximo permitido." });

            await using (var contenido = archivo.OpenReadStream())
            {
                await _respaldo.RestaurarAsync(contenido, ct);
            }

            await _auditoria.RegistrarAsync(
                User.IdUsuario(),
                AccionesAuditoria.RespaldoRestaurado,
                $"{archivo.FileName} ({archivo.Length / 1024} KB)",
                ct);

            return Ok(new { message = "Restauración completada correctamente." });
        }
    }
}
