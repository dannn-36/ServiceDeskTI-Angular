using System;
using System.Collections.Generic;

namespace ServiceDeskNg.Server.Models;

public partial class Agente
{
    public int IdAgente { get; set; }

    public int IdUsuario { get; set; }

    public int IdNivel { get; set; }

    public string? EspecialidadAgente { get; set; }

    public bool? DisponibilidadAgente { get; set; }

    public virtual NivelesAcceso IdNivelNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
