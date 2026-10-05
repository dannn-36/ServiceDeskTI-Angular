using System.ComponentModel.DataAnnotations;

namespace ServiceDeskNg.Server.Models.Dtos
{
    /// Integrante del equipo de soporte con sus métricas reales.
    public class MiembroEquipoDto
    {
        public int IdAgente { get; set; }
        public string Name { get; set; } = null!;

        /// available | busy  (según disponibilidad declarada del agente)
        public string Status { get; set; } = "busy";

        /// Tickets activos asignados (abierto, en-progreso o pendiente).
        public int Tickets { get; set; }

        /// Horas promedio entre apertura y cierre de sus tickets finalizados.
        public string AvgTime { get; set; } = "0.0";

        /// El sistema todavía no mide satisfacción: se devuelve null y la interfaz muestra "N/D".
        public decimal? Satisfaction { get; set; }
    }

    /// Comparativa entre agentes calculada sobre datos reales (sin valores simulados).
    public class ComparativaAgenteDto
    {
        public string Name { get; set; } = null!;
        public int Asignados { get; set; }
        public int Resueltos { get; set; }
        public int Activos { get; set; }

        /// Porcentaje de tickets finalizados sobre asignados (0-100).
        public decimal TasaResolucion { get; set; }

        /// Horas promedio de resolución; null si todavía no ha cerrado ninguno.
        public decimal? TiempoPromedioHoras { get; set; }
    }

    /// Carga de trabajo por agente, usada por el reporte PDF.
    public class CargaAgenteDto
    {
        public string Nombre { get; set; } = null!;
        public int TicketsActivos { get; set; }
        public int TicketsTotales { get; set; }
        public int TicketsResueltos { get; set; }
    }

    public class AgenteCreateDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe asociarse un usuario válido.")]
        public int IdUsuario { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un nivel de acceso válido.")]
        public int IdNivel { get; set; }

        [StringLength(100)]
        public string? EspecialidadAgente { get; set; }

        public bool? DisponibilidadAgente { get; set; }
    }

    public class SupervisorCreateDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe asociarse un usuario válido.")]
        public int IdUsuario { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un nivel de acceso válido.")]
        public int IdNivel { get; set; }

        [StringLength(100)]
        public string? AreaResponsabilidadSupervisor { get; set; }
    }

    public class AdministradorUpdateDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un nivel de acceso válido.")]
        public int IdNivel { get; set; }

        [StringLength(100)]
        public string? AreaResponsabilidadAdmin { get; set; }
    }

    public class EndUserCreateDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe asociarse un usuario válido.")]
        public int IdUsuario { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un nivel de acceso válido.")]
        public int IdNivel { get; set; }
    }
}
