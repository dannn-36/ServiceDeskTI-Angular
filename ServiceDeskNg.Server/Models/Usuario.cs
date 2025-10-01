using System;
using System.Collections.Generic;

namespace ServiceDeskNg.Server.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string CorreoUsuario { get; set; } = null!;

    public string ContrasenaUsuario { get; set; } = null!;

    public string? EstadoUsuario { get; set; }

    public string? DepartamentoUsuario { get; set; }

    public string? UbicacionUsuario { get; set; }

    public DateTime? FechaHoraCreacionUsuario { get; set; }

    public virtual ICollection<Administradore> Administradores { get; set; } = new List<Administradore>();

    public virtual ICollection<Agente> Agentes { get; set; } = new List<Agente>();

    public virtual ICollection<Auditorium> Auditoria { get; set; } = new List<Auditorium>();

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<Sesione> Sesiones { get; set; } = new List<Sesione>();

    public virtual ICollection<Supervisore> Supervisores { get; set; } = new List<Supervisore>();

    public virtual ICollection<TicketArchivo> TicketArchivos { get; set; } = new List<TicketArchivo>();

    public virtual ICollection<TicketMensaje> TicketMensajes { get; set; } = new List<TicketMensaje>();
}
