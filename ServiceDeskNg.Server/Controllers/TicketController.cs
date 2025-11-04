using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceDeskNg.Server.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    public class TicketsController : ControllerBase
    {
        private readonly TicketService _ticketService;
        private readonly TicketsCategoriaService _categoriaService;
        private readonly TicketsEstadoService _estadoService;
        private readonly ServiceDeskContext _context;

        public TicketsController(
            TicketService ticketService,
            TicketsCategoriaService categoriaService,
            TicketsEstadoService estadoService,
            ServiceDeskContext context)
        {
            _ticketService = ticketService;
            _categoriaService = categoriaService;
            _estadoService = estadoService;
            _context = context;
        }

        // GET: api/tickets?includeRelations=true
        [HttpGet]
        public IActionResult GetAll([FromQuery] bool includeRelations = false)
        {
            try
            {
                var tickets = _ticketService.GetAll(includeRelations);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los tickets", error = ex.Message });
            }
        }

        // GET: api/tickets/{id}?includeRelations=true
        [HttpGet("{id}")]
        public IActionResult GetById(int id, [FromQuery] bool includeRelations = false)
        {
            try
            {
                var ticket = _ticketService.GetById(id, includeRelations);
                return Ok(ticket);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el ticket", error = ex.Message });
            }
        }

        // GET: api/tickets/cliente/{idCliente}
        [HttpGet("cliente/{idCliente}")]
        public IActionResult GetByClienteId(int idCliente)
        {
            try
            {
                var tickets = _ticketService.GetByClienteId(idCliente);
                return Ok(tickets);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los tickets del cliente", error = ex.Message });
            }
        }

        // GET: api/tickets/agente/{idAgente}
        [HttpGet("agente/{idAgente}")]
        public IActionResult GetByAgenteId(int idAgente)
        {
            try
            {
                var tickets = _ticketService.GetByAgenteId(idAgente);
                return Ok(tickets);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los tickets del agente", error = ex.Message });
            }
        }

        // POST: api/tickets
        [HttpPost]
        public IActionResult Create([FromBody] Ticket entity)
        {
            if (entity == null)
                return BadRequest(new { message = "El cuerpo de la solicitud no puede ser nulo." });

            try
            {
                _ticketService.Create(entity);
                return CreatedAtAction(nameof(GetById), new { id = entity.IdTicket }, entity);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el ticket", error = ex.Message });
            }
        }

        // PUT: api/tickets/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Ticket entity)
        {
            if (entity == null || id != entity.IdTicket)
                return BadRequest(new { message = "Datos inválidos o ID no coincide." });

            try
            {
                _ticketService.Update(entity);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el ticket", error = ex.Message });
            }
        }

        // DELETE: api/tickets/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _ticketService.Delete(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el ticket", error = ex.Message });
            }
        }

        // GET: api/tickets/categorias
        [HttpGet("categorias")]
        public IActionResult GetCategorias()
        {
            try
            {
                var categorias = _categoriaService.GetAll();
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las categorías de tickets", error = ex.Message });
            }
        }

        // GET: api/tickets/estados
        [HttpGet("estados")]
        public IActionResult GetEstados()
        {
            try
            {
                var estados = _estadoService.GetAll();
                return Ok(estados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los estados de tickets", error = ex.Message });
            }
        }

        // GET: api/tickets/dashboard
        [HttpGet("dashboard")]
        public IActionResult GetDashboardTickets()
        {
            try
            {
                var tickets = _ticketService.GetAll(true)
                    .Select(t => new {
                        id = t.IdTicket,
                        title = t.TituloTicket,
                        user = t.IdClienteNavigation?.IdUsuarioNavigation?.NombreUsuario ?? "Desconocido",
                        agent = t.IdAgenteAsignadoNavigation?.IdUsuarioNavigation?.NombreUsuario ?? "Sin asignar",
                        status = t.IdEstadoTicketNavigation?.NombreEstado ?? "",
                        priority = t.PrioridadTicket,
                        category = t.IdCategoriaTicketNavigation?.NombreCategoria ?? "",
                        time = t.FechaHoraCreacionTicket.HasValue
                            ? (DateTime.Now - t.FechaHoraCreacionTicket.Value).TotalMinutes.ToString("0") + " min"
                            : "-"
                    })
                    .ToList();

                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los tickets para el dashboard", error = ex.Message });
            }
        }

        // POST: api/tickets/assign
        [HttpPost("assign")]
        public IActionResult AssignAgent([FromBody] AssignTicketRequest request)
        {
            if (request == null || request.idTicket <= 0 || request.idAgente <= 0)
                return BadRequest(new { message = "Datos inválidos para asignación." });
            try
            {
                var ticket = _ticketService.GetById(request.idTicket);
                if (ticket == null)
                    return NotFound(new { message = "Ticket no encontrado." });
                ticket.IdAgenteAsignado = request.idAgente;
                _ticketService.Update(ticket);
                return Ok(new { message = "Agente reasignado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al reasignar agente.", error = ex.Message });
            }
        }

        // POST: api/tickets/{id}/escalar
        [HttpPost("{id}/escalar")]
        public IActionResult EscalarTicket(int id, [FromBody] EscalarTicketRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.nuevaCategoria))
                return BadRequest(new { message = "Datos inválidos para escalar." });
            try
            {
                var ticket = _ticketService.GetById(id);
                if (ticket == null)
                    return NotFound(new { message = "Ticket no encontrado." });
                // Buscar la categoría por nombre
                var categoria = _categoriaService.GetAll().FirstOrDefault(c => c.NombreCategoria.ToLower() == request.nuevaCategoria.ToLower());
                if (categoria == null)
                    return BadRequest(new { message = "Categoría no encontrada." });
                ticket.IdCategoriaTicket = categoria.IdCategoria;
                _ticketService.Update(ticket);
                return Ok(new { message = "Ticket escalado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al escalar el ticket.", error = ex.Message });
            }
        }

        // POST: api/tickets/redistributir
        [HttpPost("redistribuir")]
        public IActionResult RedistribuirTickets()
        {
            // Ejemplo de lógica: distribuir tickets activos entre agentes de forma equitativa
            var tickets = _ticketService.GetAll().Where(t => t.IdAgenteAsignado == null).ToList();
            var agentes = _ticketService.GetAll().Select(t => t.IdAgenteAsignado).Distinct().Where(a => a != null).ToList();
            if (agentes.Count == 0 || tickets.Count == 0)
                return Ok(new { message = "No hay agentes o tickets para redistribuir." });
            int i = 0;
            foreach (var ticket in tickets)
            {
                ticket.IdAgenteAsignado = agentes[i % agentes.Count];
                _ticketService.Update(ticket);
                i++;
            }
            return Ok(new { message = "Tickets redistribuidos automáticamente." });
        }

        // POST: api/tickets/asignar-sin-agente
        [HttpPost("asignar-sin-agente")]
        public IActionResult AsignarSinAgente()
        {
            // Asigna todos los tickets sin agente al agente con menos tickets
            var ticketsSinAgente = _ticketService.GetAll().Where(t => t.IdAgenteAsignado == null).ToList();
            var agentes = _ticketService.GetAll().Where(t => t.IdAgenteAsignado != null)
                .GroupBy(t => t.IdAgenteAsignado)
                .Select(g => new { IdAgente = g.Key, Count = g.Count() })
                .OrderBy(g => g.Count)
                .ToList();
            if (agentes.Count == 0 || ticketsSinAgente.Count == 0)
                return Ok(new { message = "No hay agentes o tickets sin asignar." });
            var agenteMenosTickets = agentes.First().IdAgente;
            foreach (var ticket in ticketsSinAgente)
            {
                ticket.IdAgenteAsignado = agenteMenosTickets;
                _ticketService.Update(ticket);
            }
            return Ok(new { message = "Tickets sin asignar han sido distribuidos." });
        }

        // GET: api/tickets/vencidos
        [HttpGet("vencidos")]
        public IActionResult GetTicketsVencidos()
        {
            // Considera vencido si la fecha de creación es mayor a X horas/días y no está resuelto
            var vencidos = _ticketService.GetAll().Where(t =>
                t.FechaHoraCreacionTicket.HasValue &&
                (DateTime.Now - t.FechaHoraCreacionTicket.Value).TotalHours > 48 &&
                t.IdEstadoTicketNavigation != null &&
                t.IdEstadoTicketNavigation.NombreEstado.ToLower() != "resuelto"
            ).ToList();
            return Ok(vencidos);
        }

        // GET: api/tickets/reporte-carga
        [HttpGet("reporte-carga")]
        public IActionResult GenerarReporteCarga()
        {
            // Simula un PDF con información de carga de trabajo
            var pdfBytes = System.Text.Encoding.UTF8.GetBytes("Reporte de carga de trabajo generado.");
            return File(pdfBytes, "application/pdf", "reporte-carga.pdf");
        }

        // GET: api/tickets/weekly-performance
        [HttpGet("weekly-performance")]
        public IActionResult GetWeeklyPerformance()
        {
            // Tickets creados y resueltos por día de la semana (últimos 7 días)
            var now = DateTime.Now;
            var start = now.AddDays(-6).Date;
            var tickets = _ticketService.GetAll(true)
                .Where(t => t.FechaHoraCreacionTicket.HasValue && t.FechaHoraCreacionTicket.Value.Date >= start)
                .ToList();
            var days = new[] { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
            var created = new int[7];
            var resolved = new int[7];
            foreach (var t in tickets)
            {
                var dayIdx = ((int)t.FechaHoraCreacionTicket.Value.DayOfWeek + 6) % 7; // Lunes=0
                created[dayIdx]++;
                if (t.IdEstadoTicketNavigation?.NombreEstado.ToLower() == "resuelto")
                    resolved[dayIdx]++;
            }
            return Ok(new { labels = days, created, resolved });
        }

        // GET: api/team/comparison
        [HttpGet("/api/team/comparison")]
        public IActionResult GetAgentComparison()
        {
            // Simula métricas por agente (puedes mejorar con datos reales)
            var agentes = _context.Agentes.Include(a => a.IdUsuarioNavigation).ToList();
            var result = agentes.Select(a => new {
                name = a.IdUsuarioNavigation?.NombreUsuario ?? "",
                velocidad = new Random().Next(6, 10),
                calidad = new Random().Next(6, 10),
                satisfaccion = new Random().Next(6, 10),
                comunicacion = new Random().Next(6, 10),
                proactividad = new Random().Next(6, 10)
            }).ToList();
            return Ok(result);
        }

        // GET: api/tickets/escalations
        [HttpGet("escalations")]
        public IActionResult GetEscalations()
        {
            try
            {
                var escalations = _ticketService.GetAll(true)
                    .Where(t => t.IdEstadoTicketNavigation != null &&
                                (
                                    t.IdEstadoTicketNavigation.NombreEstado.ToLower().Contains("escalado") ||
                                    t.IdEstadoTicketNavigation.NombreEstado.ToLower().Contains("pendiente") ||
                                    t.IdEstadoTicketNavigation.NombreEstado.ToLower().Contains("crítico") ||
                                    t.IdEstadoTicketNavigation.NombreEstado.ToLower().Contains("critico")
                                ))
                    .Select(t => new {
                        id = t.IdTicket,
                        title = t.TituloTicket,
                        escalatedTo = t.IdAgenteAsignadoNavigation?.IdUsuarioNavigation?.NombreUsuario ?? "Sin asignar",
                        reason = t.PrioridadTicket ?? "-",
                        time = t.FechaHoraCreacionTicket.HasValue ? (DateTime.Now - t.FechaHoraCreacionTicket.Value).TotalMinutes.ToString("0") + " min" : "-",
                        status = t.IdEstadoTicketNavigation.NombreEstado.ToLower().Contains("crítico") || t.IdEstadoTicketNavigation.NombreEstado.ToLower().Contains("critico") ? "critical"
                                : t.IdEstadoTicketNavigation.NombreEstado.ToLower().Contains("pendiente") ? "pending"
                                : "escalated"
                    })
                    .ToList();

                return Ok(escalations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las escalaciones", error = ex.Message });
            }
        }

        // GET: api/tickets/reporte-semanal
        [HttpGet("reporte-semanal")]
        public IActionResult GenerarReporteSemanal([FromQuery] string format = "pdf")
        {
            // Simula un PDF o Excel con información semanal
            var content = format == "pdf"
                ? System.Text.Encoding.UTF8.GetBytes("Reporte semanal generado.")
                : System.Text.Encoding.UTF8.GetBytes("Reporte semanal generado en Excel.");
            var mime = format == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = format == "pdf" ? "reporte-semanal.pdf" : "reporte-semanal.xlsx";
            return File(content, mime, fileName);
        }

        // GET: api/tickets/reporte-individual
        [HttpGet("reporte-individual")]
        public IActionResult GenerarReporteIndividual([FromQuery] string agente, [FromQuery] string format = "pdf")
        {
            var content = format == "pdf"
                ? System.Text.Encoding.UTF8.GetBytes($"Reporte individual generado para {agente}.")
                : System.Text.Encoding.UTF8.GetBytes($"Reporte individual generado en Excel para {agente}.");
            var mime = format == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = format == "pdf" ? $"reporte-individual-{agente}.pdf" : $"reporte-individual-{agente}.xlsx";
            return File(content, mime, fileName);
        }

        // GET: api/tickets/reporte-sla
        [HttpGet("reporte-sla")]
        public IActionResult GenerarReporteSla([FromQuery] string format = "pdf")
        {
            var content = format == "pdf"
                ? System.Text.Encoding.UTF8.GetBytes("Reporte SLA generado.")
                : System.Text.Encoding.UTF8.GetBytes("Reporte SLA generado en Excel.");
            var mime = format == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = format == "pdf" ? "reporte-sla.pdf" : "reporte-sla.xlsx";
            return File(content, mime, fileName);
        }

    }

    public class AssignTicketRequest
    {
        public int idTicket { get; set; }
        public int idAgente { get; set; }
    }

    public class EscalarTicketRequest
    {
        public string nuevaCategoria { get; set; }
    }
}