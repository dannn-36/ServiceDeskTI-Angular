using System;
using System.Collections.Generic;

namespace ServiceDeskNg.Server.Models;

public partial class TicketsEstado
{
    public int IdEstado { get; set; }

    public string NombreEstado { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
