using System;
using System.Collections.Generic;

namespace ServiceDeskNg.Server.Models;

public partial class Administrador
{
    public int IdAdmin { get; set; }

    public int IdUsuario { get; set; }

    public int IdNivel { get; set; }

    public string? AreaResponsabilidadAdmin { get; set; }

    public virtual NivelesAcceso IdNivelNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
