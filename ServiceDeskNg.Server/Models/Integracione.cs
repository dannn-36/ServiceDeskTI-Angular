using System;
using System.Collections.Generic;

namespace ServiceDeskNg.Server.Models;

public partial class Integracione
{
    public int IdIntegracion { get; set; }

    public int IdTicket { get; set; }

    public string OrigenIntegracion { get; set; } = null!;

    public string? ReferenciaIntegracion { get; set; }

    public string? EstadoIntegracion { get; set; }

    public DateTime? FechaHoraCreacionIntegracion { get; set; }

    public virtual Ticket IdTicketNavigation { get; set; } = null!;
}
