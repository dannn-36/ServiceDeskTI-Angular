using System;
using System.Collections.Generic;

namespace ServiceDeskNg.Server.Models;

public partial class Supervisor
{
    public int IdSupervisor { get; set; }

    public int IdUsuario { get; set; }

    public int IdNivel { get; set; }

    public string? AreaResponsabilidadSupervisor { get; set; }

    public virtual NivelesAcceso IdNivelNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
