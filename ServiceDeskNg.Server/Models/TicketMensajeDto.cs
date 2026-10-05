// OBSOLETO: reemplazado por Models/Dtos. Se conserva solo hasta que se borre este archivo.
namespace ServiceDeskNg.Server.Models.Obsoleto
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
}