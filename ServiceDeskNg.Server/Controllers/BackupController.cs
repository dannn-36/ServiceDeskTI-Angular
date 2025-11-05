using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ServiceDeskNg.Server.Controllers
{
    [ApiController]
    [Route("api/backup")]
    public class BackupController : ControllerBase
    {
        private readonly IConfiguration _config;
        public BackupController(IConfiguration config)
        {
            _config = config;
        }

        // GET: api/backup
        [HttpGet]
        public async Task<IActionResult> CreateBackup()
        {
            string dbHost = _config["Database:Host"] ?? "localhost";
            string dbUser = _config["Database:User"] ?? "root";
            string dbPass = _config["Database:Password"] ?? "";
            string dbName = _config["Database:Name"] ?? "servicedesk";
            string mysqldumpPath = _config["Database:MySqlDumpPath"] ?? "mysqldump";
            string fileName = $"backup_{dbName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.sql";

            var psi = new ProcessStartInfo
            {
                FileName = mysqldumpPath,
                Arguments = $"-h {dbHost} -u {dbUser} --password={dbPass} {dbName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                // Devuelve el error como JSON para que el frontend lo maneje correctamente
                return StatusCode(500, new { message = $"Error al crear respaldo: {error}" });
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(output);
            return File(bytes, "application/sql", fileName);
        }

        // POST: api/backup/restore
        [HttpPost("restore")]
        public async Task<IActionResult> RestoreBackup()
        {
            var file = Request.Form.Files[0];
            if (file == null || file.Length == 0)
                return StatusCode(400, new { message = "No se recibió archivo de respaldo." });

            string dbHost = _config["Database:Host"] ?? "localhost";
            string dbUser = _config["Database:User"] ?? "root";
            string dbPass = _config["Database:Password"] ?? "";
            string dbName = _config["Database:Name"] ?? "servicedesk";
            string mysqlPath = _config["Database:MySqlPath"] ?? "mysql";

            var tempPath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(tempPath))
            {
                await file.CopyToAsync(stream);
            }

            var psi = new ProcessStartInfo
            {
                FileName = mysqlPath,
                Arguments = $"-h {dbHost} -u {dbUser} --password={dbPass} {dbName}",
                RedirectStandardInput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            using (var reader = new StreamReader(tempPath))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    await process.StandardInput.WriteLineAsync(line);
                }
            }
            process.StandardInput.Close();
            var error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();
            System.IO.File.Delete(tempPath);

            if (process.ExitCode != 0)
            {
                return StatusCode(500, new { message = $"Error al restaurar respaldo: {error}" });
            }

            return Ok(new { message = "Restauración completada correctamente." });
        }
    }
}
