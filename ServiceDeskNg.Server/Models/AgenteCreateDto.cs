// OBSOLETO: reemplazado por Models/Dtos. Se conserva solo hasta que se borre este archivo.
namespace ServiceDeskNg.Server.Models.Obsoleto
{
    public class AgenteCreateDto
    {
        public int IdUsuario { get; set; }
        public int IdNivel { get; set; }
        public string? EspecialidadAgente { get; set; }
        public bool? DisponibilidadAgente { get; set; }
    }
}