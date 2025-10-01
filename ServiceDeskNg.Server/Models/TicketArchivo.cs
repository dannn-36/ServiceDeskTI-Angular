using System;
using System.Collections.Generic;

namespace ServiceDeskNg.Server.Models;

public partial class TicketArchivo
{
    public int IdArchivo { get; set; }

    public int IdTicket { get; set; }

    public int IdUsuario { get; set; }

    public string RutaArchivoTicket { get; set; } = null!;

    public DateTime? FechaHoraCreacionMensaje { get; set; }

    public virtual Ticket IdTicketNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
