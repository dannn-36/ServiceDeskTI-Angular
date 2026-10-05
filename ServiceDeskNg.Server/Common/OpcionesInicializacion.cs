namespace ServiceDeskNg.Server.Common
{
    /// Qué hacer con una base de datos recién creada (sección "Inicializacion" de la configuración).
    public class OpcionesInicializacion
    {
        public const string Seccion = "Inicializacion";

        /// Si no existe ningún administrador y se indican correo y contraseña,
        /// se crea uno al arrancar. Sin esto, una base nueva no tiene con quién entrar.
        public string AdminNombre { get; set; } = "Administrador";
        public string? AdminCorreo { get; set; }
        public string? AdminContrasena { get; set; }

        /// Carga usuarios, tickets y conversaciones de ejemplo si la base no tiene tickets.
        /// Pensado para demostraciones y desarrollo; desactivado por defecto.
        public bool SembrarDatosDemo { get; set; }

        /// Contraseña común de los usuarios de demostración.
        public string ContrasenaDemo { get; set; } = "Demo12345!";
    }
}
