// OBSOLETO: reemplazado por Models/Dtos. Se conserva solo hasta que se borre este archivo.
namespace ServiceDeskNg.Server.Models.Obsoleto
{
    public class UsuarioUpdateDto
    {
        public string NombreUsuario { get; set; }
        public string CorreoUsuario { get; set; }
        public string? ContrasenaUsuario { get; set; }
        public string? DepartamentoUsuario { get; set; }
        public string? EstadoUsuario { get; set; }
        public string? UbicacionUsuario { get; set; }
    }
}
