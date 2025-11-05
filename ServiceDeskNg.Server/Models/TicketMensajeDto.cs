namespace ServiceDeskNg.Server.Models
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