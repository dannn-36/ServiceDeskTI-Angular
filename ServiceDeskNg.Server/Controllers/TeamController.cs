using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Services;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Data;
using System.Linq;

namespace ServiceDeskNg.Server.Controllers
{
    [ApiController]
    [Route("api/team")]
    public class TeamController : ControllerBase
    {
        private readonly AgenteService _agenteService;
        private readonly ServiceDeskContext _context;

        public TeamController(AgenteService agenteService, ServiceDeskContext context)
        {
            _agenteService = agenteService;
            _context = context;
        }

        [HttpGet]
        public IActionResult GetTeamMembers()
        {
            var agentes = _agenteService.GetAll(true).Select(a => new {
                idAgente = a.IdAgente,
                name = a.IdUsuarioNavigation?.NombreUsuario ?? $"Agente {a.IdAgente}",
                status = a.DisponibilidadAgente == true ? "available" : "busy",
                tickets = _context.Tickets.Count(t => t.IdAgenteAsignado == a.IdAgente),
                avgTime = "2.0", // Aquí puedes calcular el tiempo promedio si tienes esa info
                satisfaction = 4.5 // Aquí puedes poner la satisfacción si tienes esa info
            }).ToList();
            return Ok(agentes);
        }
    }
}
