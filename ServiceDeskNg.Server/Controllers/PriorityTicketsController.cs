using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Security;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Controllers
{
    /// Tickets de prioridad alta o urgente pendientes de resolver.
    /// Antes filtraba por "urgent"/"high", valores que no existen en el ENUM
    /// de la base de datos ('baja','media','alta','urgente'), y siempre devolvía vacío.
    [ApiController]
    [Route("api/priority-tickets")]
    [Authorize(Roles = RolesApp.Gestion)]
    public class PriorityTicketsController : ControllerBase
    {
        private readonly MetricasService _metricas;

        public PriorityTicketsController(MetricasService metricas)
        {
            _metricas = metricas;
        }

        [HttpGet]
        public async Task<ActionResult<List<TicketPanelDto>>> GetPriorityTickets(CancellationToken ct) =>
            Ok(await _metricas.PrioritariosAsync(ct));
    }
}
