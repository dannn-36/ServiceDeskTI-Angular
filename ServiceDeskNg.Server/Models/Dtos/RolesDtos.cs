namespace ServiceDeskNg.Server.Models.Dtos
{
    /// Vistas de las tablas de roles. Incluyen el nombre del usuario asociado
    /// para que el cliente no tenga que hacer una segunda llamada.
    public class AgenteDto
    {
        public int IdAgente { get; set; }
        public int IdUsuario { get; set; }
        public int IdNivel { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string CorreoUsuario { get; set; } = null!;
        public string? EspecialidadAgente { get; set; }
        public bool DisponibilidadAgente { get; set; }
    }

    public class EndUserDto
    {
        public int IdCliente { get; set; }
        public int IdUsuario { get; set; }
        public int IdNivel { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string CorreoUsuario { get; set; } = null!;
        public string? DepartamentoUsuario { get; set; }
    }

    public class SupervisorDto
    {
        public int IdSupervisor { get; set; }
        public int IdUsuario { get; set; }
        public int IdNivel { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string CorreoUsuario { get; set; } = null!;
        public string? AreaResponsabilidadSupervisor { get; set; }
    }

    public class AdministradorDto
    {
        public int IdAdmin { get; set; }
        public int IdUsuario { get; set; }
        public int IdNivel { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string CorreoUsuario { get; set; } = null!;
        public string? AreaResponsabilidadAdmin { get; set; }
    }

    public class NivelAccesoDto
    {
        public int IdNivel { get; set; }
        public int Nivel { get; set; }
        public string Nombre { get; set; } = null!;
    }
}
