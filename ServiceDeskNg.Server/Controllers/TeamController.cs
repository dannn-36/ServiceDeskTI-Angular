using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Security;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Controllers
{
    /// Métricas del equipo de agentes para el panel de supervisión.
    [ApiController]
    [Route("api/team")]
    [Authorize(Roles = RolesApp.Gestion)]
    public class TeamController : ControllerBase
    {
        private readonly MetricasService _metricas;

        public TeamController(MetricasService metricas)
        {
            _metricas = metricas;
        }

        [HttpGet]
        public async Task<ActionResult<List<MiembroEquipoDto>>> GetTeamMembers(CancellationToken ct) =>
            Ok(await _metricas.EquipoAsync(ct));

        /// Antes devolvía números aleatorios (new Random()). Ahora son indicadores medidos.
        [HttpGet("comparison")]
        public async Task<ActionResult<List<ComparativaAgenteDto>>> GetAgentComparison(CancellationToken ct) =>
            Ok(await _metricas.ComparativaAgentesAsync(ct));
    }
}
