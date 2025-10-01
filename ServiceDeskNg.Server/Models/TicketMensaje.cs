using System;
using System.Collections.Generic;

namespace ServiceDeskNg.Server.Models;

public partial class TicketMensaje
{
    public int IdMensaje { get; set; }

    public int IdTicket { get; set; }

    public int IdUsuario { get; set; }

    public string MensajeTicket { get; set; } = null!;

    public DateTime? FechaHoraCreacionMensaje { get; set; }

    public virtual Ticket IdTicketNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
