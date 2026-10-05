using System.Diagnostics;
using Microsoft.Extensions.Options;
using MySqlConnector;
using ServiceDeskNg.Server.Common;

namespace ServiceDeskNg.Server.Services
{
    /// Respaldo y restauración de la base de datos con las herramientas oficiales de MySQL.
    ///
    /// Cambios respecto a la versión anterior:
    ///  - La contraseña viaja en la variable de entorno MYSQL_PWD del proceso hijo,
    ///    no en la línea de comandos (que cualquier usuario del equipo puede listar).
    ///  - Los argumentos se pasan con ArgumentList, sin concatenar cadenas.
    ///  - stdout y stderr se leen a la vez, para que el proceso no se bloquee
    ///    si llena el búfer de errores.
    ///  - El volcado se copia como bytes, sin pasar por string, para no alterar la codificación.
    ///  - Hay tiempo máximo de ejecución.
    public class RespaldoService
    {
        private readonly OpcionesRespaldo _opciones;
        private readonly string _cadenaConexion;
        private readonly ILogger<RespaldoService> _logger;

        public RespaldoService(
            IOptions<OpcionesRespaldo> opciones,
            IConfiguration configuracion,
            ILogger<RespaldoService> logger)
        {
            _opciones = opciones.Value;
            _cadenaConexion = configuracion.GetConnectionString("ServiceDeskDB")
                ?? throw new InvalidOperationException("Falta la cadena de conexión 'ServiceDeskDB'.");
            _logger = logger;
        }

        public string NombreBaseDatos => new MySqlConnectionStringBuilder(_cadenaConexion).Database;

        public async Task<byte[]> GenerarRespaldoAsync(CancellationToken ct = default)
        {
            var conexion = new MySqlConnectionStringBuilder(_cadenaConexion);

            var proceso = CrearProceso(_opciones.MySqlDumpPath, conexion, redirigirEntrada: false);
            proceso.StartInfo.ArgumentList.Add("--single-transaction");
            proceso.StartInfo.ArgumentList.Add("--routines");
            proceso.StartInfo.ArgumentList.Add("--triggers");
            proceso.StartInfo.ArgumentList.Add(conexion.Database);

            using var limite = CrearLimiteDeTiempo(ct);
            IniciarProceso(proceso, _opciones.MySqlDumpPath);

            using var salida = new MemoryStream();
            var copiaSalida = proceso.StandardOutput.BaseStream.CopyToAsync(salida, limite.Token);
            var lecturaErrores = proceso.StandardError.ReadToEndAsync(limite.Token);

            await Task.WhenAll(copiaSalida, lecturaErrores);
            await proceso.WaitForExitAsync(limite.Token);

            if (proceso.ExitCode != 0)
            {
                _logger.LogError("mysqldump terminó con código {Codigo}: {Error}", proceso.ExitCode, lecturaErrores.Result);
                throw new FalloOperacionException(
                    "No se pudo generar el respaldo. Revise los registros del servidor.");
            }

            return salida.ToArray();
        }

        public async Task RestaurarAsync(Stream archivoSql, CancellationToken ct = default)
        {
            var conexion = new MySqlConnectionStringBuilder(_cadenaConexion);

            var proceso = CrearProceso(_opciones.MySqlPath, conexion, redirigirEntrada: true);
            proceso.StartInfo.ArgumentList.Add(conexion.Database);

            using var limite = CrearLimiteDeTiempo(ct);
            IniciarProceso(proceso, _opciones.MySqlPath);

            var lecturaErrores = proceso.StandardError.ReadToEndAsync(limite.Token);

            await archivoSql.CopyToAsync(proceso.StandardInput.BaseStream, limite.Token);
            await proceso.StandardInput.BaseStream.FlushAsync(limite.Token);
            proceso.StandardInput.Close();

            await proceso.WaitForExitAsync(limite.Token);
            var errores = await lecturaErrores;

            if (proceso.ExitCode != 0)
            {
                _logger.LogError("mysql terminó con código {Codigo}: {Error}", proceso.ExitCode, errores);
                throw new FalloOperacionException(
                    "No se pudo restaurar el respaldo. Verifique que el archivo sea un volcado SQL válido.");
            }
        }

        public long TamanoMaximoBytes => _opciones.TamanoMaximoBytes;

        private static Process CrearProceso(string ejecutable, MySqlConnectionStringBuilder conexion, bool redirigirEntrada)
        {
            var inicio = new ProcessStartInfo
            {
                FileName = ejecutable,
                RedirectStandardOutput = !redirigirEntrada,
                RedirectStandardError = true,
                RedirectStandardInput = redirigirEntrada,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            inicio.ArgumentList.Add($"--host={conexion.Server}");
            inicio.ArgumentList.Add($"--port={conexion.Port}");
            inicio.ArgumentList.Add($"--user={conexion.UserID}");
            inicio.ArgumentList.Add("--default-character-set=utf8mb4");

            // Fuera de la línea de comandos: no aparece en la lista de procesos del sistema.
            inicio.Environment["MYSQL_PWD"] = conexion.Password;

            return new Process { StartInfo = inicio };
        }

        private void IniciarProceso(Process proceso, string ejecutable)
        {
            try
            {
                proceso.Start();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                _logger.LogError(ex, "No se encontró la herramienta de MySQL en {Ruta}", ejecutable);
                throw new FalloOperacionException(
                    $"No se encontró '{Path.GetFileName(ejecutable)}'. Configure la ruta en la sección '{OpcionesRespaldo.Seccion}' de appsettings.");
            }
        }

        private CancellationTokenSource CrearLimiteDeTiempo(CancellationToken ct)
        {
            var limite = CancellationTokenSource.CreateLinkedTokenSource(ct);
            limite.CancelAfter(TimeSpan.FromSeconds(_opciones.TiempoMaximoSegundos));
            return limite;
        }
    }
}
