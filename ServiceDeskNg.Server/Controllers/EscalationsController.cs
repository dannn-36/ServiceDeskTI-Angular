using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Security;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Controllers
{
    /// Escalaciones del panel de supervisión.
    /// Antes devolvía tres tickets inventados ("TK-1245", "TK-1243"...) escritos en el código.
    /// Ahora se calculan con los tickets reales: urgentes, vencidos según SLA y de prioridad alta.
    [ApiController]
    [Route("api/escalations")]
    [Authorize(Roles = RolesApp.Gestion)]
    public class EscalationsController : ControllerBase
    {
        private readonly MetricasService _metricas;

        public EscalationsController(MetricasService metricas)
        {
            _metricas = metricas;
        }

        [HttpGet]
        public async Task<ActionResult<List<EscalacionDto>>> GetEscalations(CancellationToken ct) =>
            Ok(await _metricas.EscalacionesAsync(ct));
    }
}
