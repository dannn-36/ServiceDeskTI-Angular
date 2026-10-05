using System.ComponentModel.DataAnnotations;

namespace ServiceDeskNg.Server.Models.Dtos
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string CorreoUsuario { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string ContrasenaUsuario { get; set; } = null!;
    }

    /// Identidad que el frontend necesita para pintar la interfaz.
    /// Es lo mismo que devuelve /api/auth/login y /api/auth/me.
    public class SesionUsuarioDto
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string CorreoUsuario { get; set; } = null!;
        public string Rol { get; set; } = null!;

        /// Identificador en la tabla del rol correspondiente (clientes, agentes, ...).
        /// El frontend ya no necesita pedirlo con otra llamada.
        public int? IdCliente { get; set; }
        public int? IdAgente { get; set; }
        public int? IdSupervisor { get; set; }
        public int? IdAdministrador { get; set; }
    }
}
