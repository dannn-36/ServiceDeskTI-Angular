namespace ServiceDeskNg.Server.Common
{
    /// Rutas de las herramientas de MySQL usadas para respaldar y restaurar.
    /// Host, usuario, contraseña y base de datos ya no se duplican aquí:
    /// se leen de la misma cadena de conexión que usa la aplicación.
    public class OpcionesRespaldo
    {
        public const string Seccion = "Respaldo";

        public string MySqlDumpPath { get; set; } = "mysqldump";

        public string MySqlPath { get; set; } = "mysql";

        /// Tamaño máximo aceptado para un archivo de restauración.
        public long TamanoMaximoBytes { get; set; } = 200L * 1024 * 1024;

        /// Tiempo máximo de ejecución de mysqldump / mysql.
        public int TiempoMaximoSegundos { get; set; } = 300;
    }
}
