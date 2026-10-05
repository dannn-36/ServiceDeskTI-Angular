// OBSOLETO: reemplazado por Models/Dtos. Se conserva solo hasta que se borre este archivo.
namespace ServiceDeskNg.Server.Models.Obsoleto
{
    public class UsuarioDto
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string CorreoUsuario { get; set; }
        public string? EstadoUsuario { get; set; }
        public string? DepartamentoUsuario { get; set; }
        public string? UbicacionUsuario { get; set; }
        public DateTime? FechaHoraCreacionUsuario { get; set; }
        public string TipoUsuario { get; set; } // Este será el rol calculado
    }
}
