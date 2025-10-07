using System;
using System.Collections.Generic;

namespace ServiceDeskNg.Server.Models;

public partial class NivelesAcceso
{
    public int IdNivel { get; set; }

    public int Nivel { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Administrador> Administradores { get; set; } = new List<Administrador>();

    public virtual ICollection<Agente> Agentes { get; set; } = new List<Agente>();

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<Supervisor> Supervisores { get; set; } = new List<Supervisor>();
}
