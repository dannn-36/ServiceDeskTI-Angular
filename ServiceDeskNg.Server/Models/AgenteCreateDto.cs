namespace ServiceDeskNg.Server.Models
{
    public class AgenteCreateDto
    {
        public int IdUsuario { get; set; }
        public int IdNivel { get; set; }
        public string? EspecialidadAgente { get; set; }
        public bool? DisponibilidadAgente { get; set; }
    }
}