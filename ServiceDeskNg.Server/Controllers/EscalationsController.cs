using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ServiceDeskNg.Server.Controllers
{
    [ApiController]
    [Route("api/escalations")]
    public class EscalationsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetEscalations()
        {
            var escalations = new List<object>
            {
                new { id = "TK-1245", title = "Falla crítica en producción", escalatedTo = "Gerencia TI", reason = "Impacto alto", time = "3h 15min", status = "critical" },
                new { id = "TK-1243", title = "Pérdida de datos cliente", escalatedTo = "Director Técnico", reason = "Datos sensibles", time = "5h 30min", status = "critical" },
                new { id = "TK-1240", title = "Caída de servicios web", escalatedTo = "Equipo DevOps", reason = "SLA vencido", time = "2h 45min", status = "pending" }
            };
            return Ok(escalations);
        }
    }
}
