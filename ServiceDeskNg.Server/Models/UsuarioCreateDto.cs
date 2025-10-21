namespace ServiceDeskNg.Server.Models
{
    public class UsuarioCreateDto
    {
        public string NombreUsuario { get; set; }
        public string CorreoUsuario { get; set; }
        public string ContrasenaUsuario { get; set; }
        public string? DepartamentoUsuario { get; set; }
        public string? EstadoUsuario { get; set; }
        public string? UbicacionUsuario { get; set; }
        public string TipoUsuario { get; set; }
    }
}
