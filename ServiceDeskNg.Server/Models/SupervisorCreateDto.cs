namespace ServiceDeskNg.Server.Models
{
    public class SupervisorCreateDto
    {
        public int IdUsuario { get; set; }
        public int IdNivel { get; set; }
        public string? AreaResponsabilidadSupervisor { get; set; }
    }
}