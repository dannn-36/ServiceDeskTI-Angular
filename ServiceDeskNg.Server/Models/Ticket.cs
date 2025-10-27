using System;
using System.Collections.Generic;

namespace ServiceDeskNg.Server.Models;

public partial class Ticket
{
    public int IdTicket { get; set; }

    public int IdCliente { get; set; }

    public int? IdAgenteAsignado { get; set; }

    public int IdEstadoTicket { get; set; }

    public int IdCategoriaTicket { get; set; }

    public string TituloTicket { get; set; } = null!;

    public string DescripcionTicket { get; set; } = null!;

    public string? PrioridadTicket { get; set; }

    public string? UbicacionTicket { get; set; }

    public string? DepartamentoTicket { get; set; }

    public DateTime? FechaHoraCreacionTicket { get; set; }

    public DateTime? FechaHoraActualizacionTicket { get; set; }

    public virtual Agente? IdAgenteAsignadoNavigation { get; set; }

    public virtual TicketsCategoria? IdCategoriaTicketNavigation { get; set; }

    public virtual EndUser? IdClienteNavigation { get; set; }

    public virtual TicketsEstado? IdEstadoTicketNavigation { get; set; }

    public virtual ICollection<Integracion> Integraciones { get; set; } = new List<Integracion>();

    public virtual ICollection<TicketArchivo> TicketArchivos { get; set; } = new List<TicketArchivo>();

    public virtual ICollection<TicketMensaje> TicketMensajes { get; set; } = new List<TicketMensaje>();
}
