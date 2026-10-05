using System.ComponentModel.DataAnnotations;

namespace ServiceDeskNg.Server.Models.Dtos
{
    public class TicketMensajeDto
    {
        public int IdMensaje { get; set; }
        public int IdTicket { get; set; }
        public int IdUsuario { get; set; }
        public string MensajeTicket { get; set; } = null!;
        public DateTime? FechaHoraCreacionMensaje { get; set; }
        public string UsuarioNombre { get; set; } = "Desconocido";
    }

    public class TicketMensajeCreateDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe asociarse a un ticket válido.")]
        public int IdTicket { get; set; }

        [Required(ErrorMessage = "El mensaje es obligatorio.")]
        [StringLength(2000, MinimumLength = 1)]
        public string MensajeTicket { get; set; } = null!;
    }

    /// Registro del log de auditoría tal como lo consume el panel de administración.
    public class AuditoriaDto
    {
        public int IdAuditoria { get; set; }
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = "Desconocido";
        public string AccionAuditoria { get; set; } = null!;
        public string? DetalleAuditoria { get; set; }
        public DateTime? FechaAuditoria { get; set; }
    }
}
