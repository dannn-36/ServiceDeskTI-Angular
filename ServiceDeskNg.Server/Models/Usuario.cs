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

    public virtual ICollection<Administrador> Administradores { get; set; } = new List<Administrador>();

    public virtual ICollection<Agente> Agentes { get; set; } = new List<Agente>();

    public virtual ICollection<Auditoria> Auditoria { get; set; } = new List<Auditoria>();

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<Sesion> Sesiones { get; set; } = new List<Sesion>();

    public virtual ICollection<Supervisor> Supervisores { get; set; } = new List<Supervisor>();

    public virtual ICollection<TicketArchivo> TicketArchivos { get; set; } = new List<TicketArchivo>();

    public virtual ICollection<TicketMensaje> TicketMensajes { get; set; } = new List<TicketMensaje>();
}
