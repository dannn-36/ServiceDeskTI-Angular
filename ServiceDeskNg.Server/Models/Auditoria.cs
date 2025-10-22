using System;
using System.Collections.Generic;

namespace ServiceDeskNg.Server.Models;

public partial class Auditoria
{
    public int IdAuditoria { get; set; }

    public int IdUsuario { get; set; }

    public string AccionAuditoria { get; set; } = null!;

    public string? DetalleAuditoria { get; set; }

    public DateTime? FechaAuditoria { get; set; }

}
