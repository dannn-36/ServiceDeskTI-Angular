using System.ComponentModel.DataAnnotations;
using ServiceDeskNg.Server.Security;

namespace ServiceDeskNg.Server.Models.Dtos
{
    /// Datos que la API expone de un usuario. Nunca incluye la contraseña.
    public class UsuarioDto
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string CorreoUsuario { get; set; } = null!;
        public string? EstadoUsuario { get; set; }
        public string? DepartamentoUsuario { get; set; }
        public string? UbicacionUsuario { get; set; }
        public DateTime? FechaHoraCreacionUsuario { get; set; }

        /// Rol calculado a partir de las tablas de roles.
        public string TipoUsuario { get; set; } = "Sin Rol";
    }

    public class UsuarioCreateDto
    {
        [Required(ErrorMessage = "El nombre del usuario es obligatorio.")]
        [StringLength(100, MinimumLength = 2)]
        public string NombreUsuario { get; set; } = null!;

        [Required(ErrorMessage = "El correo del usuario es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150)]
        public string CorreoUsuario { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(128, MinimumLength = 8,
            ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string ContrasenaUsuario { get; set; } = null!;

        [StringLength(100)]
        public string? DepartamentoUsuario { get; set; }

        [RegularExpression("^(activo|inactivo)$",
            ErrorMessage = "El estado debe ser 'activo' o 'inactivo'.")]
        public string? EstadoUsuario { get; set; }

        [StringLength(100)]
        public string? UbicacionUsuario { get; set; }

        [Required(ErrorMessage = "El tipo de usuario es obligatorio.")]
        public string TipoUsuario { get; set; } = RolesApp.Cliente;
    }

    public class UsuarioUpdateDto
    {
        [Required(ErrorMessage = "El nombre del usuario es obligatorio.")]
        [StringLength(100, MinimumLength = 2)]
        public string NombreUsuario { get; set; } = null!;

        [Required(ErrorMessage = "El correo del usuario es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150)]
        public string CorreoUsuario { get; set; } = null!;

        /// Opcional: si viene vacío, se conserva la contraseña actual.
        [StringLength(128, MinimumLength = 8,
            ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string? ContrasenaUsuario { get; set; }

        [StringLength(100)]
        public string? DepartamentoUsuario { get; set; }

        /// Solo lo aplica un administrador; para el resto se ignora.
        [RegularExpression("^(activo|inactivo)$",
            ErrorMessage = "El estado debe ser 'activo' o 'inactivo'.")]
        public string? EstadoUsuario { get; set; }

        [StringLength(100)]
        public string? UbicacionUsuario { get; set; }
    }
}
