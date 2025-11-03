using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Data;
using System.Linq;
using System;

namespace ServiceDeskNg.Server.Controllers
{
    [ApiController]
    [Route("api/priority-tickets")]
    public class PriorityTicketsController : ControllerBase
    {
        private readonly ServiceDeskContext _context;
        public PriorityTicketsController(ServiceDeskContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetPriorityTickets()
        {
            var tickets = _context.Tickets
                .Where(t => t.PrioridadTicket == "urgent" || t.PrioridadTicket == "high")
                .Select(t => new {
                    id = t.IdTicket,
                    title = t.TituloTicket,
                    user = _context.Clientes.Where(u => u.IdCliente == t.IdCliente).Select(u => u.IdUsuarioNavigation.NombreUsuario).FirstOrDefault() ?? "Desconocido",
                    agent = _context.Agentes.Where(a => a.IdAgente == t.IdAgenteAsignado)
                        .Select(a => _context.Usuarios.Where(u => u.IdUsuario == a.IdUsuario).Select(u => u.NombreUsuario).FirstOrDefault())
                        .FirstOrDefault() ?? "Sin asignar",
                    status = _context.TicketsEstados.Where(e => e.IdEstado == t.IdEstadoTicket).Select(e => e.NombreEstado).FirstOrDefault() ?? "",
                    priority = t.PrioridadTicket,
                    category = _context.TicketsCategorias.Where(c => c.IdCategoria == t.IdCategoriaTicket).Select(c => c.NombreCategoria).FirstOrDefault() ?? "",
                    fechaCreacion = t.FechaHoraCreacionTicket
                })
                .ToList()
                .Select(t => new {
                    t.id,
                    t.title,
                    t.user,
                    t.agent,
                    t.status,
                    t.priority,
                    t.category,
                    time = t.fechaCreacion.HasValue ? (DateTime.Now - t.fechaCreacion.Value).TotalMinutes.ToString("0") + " min" : "-"
                })
                .ToList();
            return Ok(tickets);
        }
    }
}
