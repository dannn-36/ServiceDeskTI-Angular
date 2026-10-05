using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Security;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Controllers
{
    /// Tickets: consulta, alta, edición, asignación y reportes.
    /// Las reglas de quién puede ver qué viven en TicketService.AsegurarAccesoAsync;
    /// este controlador solo traduce HTTP a llamadas de servicio.
    [ApiController]
    [Route("api/tickets")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly TicketService _tickets;
        private readonly CatalogoTicketsService _catalogo;
        private readonly MetricasService _metricas;
        private readonly AgenteService _agentes;
        private readonly ReportesPdfService _reportes;
        private readonly AuditoriaService _auditoria;

        public TicketsController(
            TicketService tickets,
            CatalogoTicketsService catalogo,
            MetricasService metricas,
            AgenteService agentes,
            ReportesPdfService reportes,
            AuditoriaService auditoria)
        {
            _tickets = tickets;
            _catalogo = catalogo;
            _metricas = metricas;
            _agentes = agentes;
            _reportes = reportes;
            _auditoria = auditoria;
        }

        // ======================================================
        // Consulta
        // ======================================================

        [HttpGet]
        [Authorize(Roles = RolesApp.Gestion)]
        public async Task<ActionResult<List<TicketDto>>> GetAll(CancellationToken ct) =>
            Ok(await _tickets.ListarAsync(ct));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TicketDto>> GetById(int id, CancellationToken ct)
        {
            await AsegurarAccesoAsync(id, ct);
            return Ok(await _tickets.ObtenerAsync(id, ct));
        }

        [HttpGet("cliente/{idCliente:int}")]
        public async Task<ActionResult<List<TicketDto>>> GetByClienteId(int idCliente, CancellationToken ct)
        {
            var esElPropioCliente = User.IdCliente() == idCliente;
            if (!User.TieneVisionGlobal() && !esElPropioCliente)
                throw new AccesoDenegadoException("Solo puede consultar sus propios tickets.");

            return Ok(await _tickets.ListarPorClienteAsync(idCliente, ct));
        }

        [HttpGet("agente/{idAgente:int}")]
        public async Task<ActionResult<List<TicketDto>>> GetByAgenteId(int idAgente, CancellationToken ct)
        {
            var esElPropioAgente = User.IdAgente() == idAgente;
            if (!User.TieneVisionGlobal() && !esElPropioAgente)
                throw new AccesoDenegadoException("Solo puede consultar los tickets que tiene asignados.");

            return Ok(await _tickets.ListarPorAgenteAsync(idAgente, ct));
        }

        [HttpGet("categorias")]
        public async Task<ActionResult<List<CategoriaTicketDto>>> GetCategorias(CancellationToken ct) =>
            Ok(await _catalogo.ListarCategoriasAsync(ct));

        [HttpGet("estados")]
        public async Task<ActionResult<List<EstadoTicketDto>>> GetEstados(CancellationToken ct) =>
            Ok(await _catalogo.ListarEstadosAsync(ct));

        // ======================================================
        // Alta, edición y baja
        // ======================================================

        /// Un cliente siempre crea tickets a su nombre (se toma de la sesión).
        /// El personal puede abrir uno en nombre de un cliente indicando IdCliente.
        [HttpPost]
        public async Task<ActionResult<TicketDto>> Create([FromBody] TicketCreateDto dto, CancellationToken ct)
        {
            int idCliente;

            if (User.EsCliente())
            {
                idCliente = User.IdCliente()
                    ?? throw new AccesoDenegadoException("Su usuario no está registrado como cliente.");
            }
            else
            {
                idCliente = dto.IdCliente
                    ?? throw new ArgumentException("Indique el cliente para el que se abre el ticket.");
            }

            var creado = await _tickets.CrearAsync(dto, idCliente, ct);

            await _auditoria.RegistrarAsync(
                User.IdUsuario(),
                AccionesAuditoria.TicketCreado,
                $"Ticket {creado.IdTicket} creado para el cliente {idCliente}",
                ct);

            return CreatedAtAction(nameof(GetById), new { id = creado.IdTicket }, creado);
        }

        /// Gestión puede editar cualquier ticket; un agente, solo los que tiene asignados.
        [HttpPut("{id:int}")]
        [Authorize(Roles = RolesApp.Personal)]
        public async Task<IActionResult> Update(int id, [FromBody] TicketUpdateDto dto, CancellationToken ct)
        {
            await AsegurarAccesoAsync(id, ct);
            await _tickets.ActualizarAsync(id, dto, ct);

            await _auditoria.RegistrarAsync(
                User.IdUsuario(),
                AccionesAuditoria.TicketActualizado,
                $"Ticket {id} actualizado (estado {dto.IdEstadoTicket}, prioridad {dto.PrioridadTicket ?? "media"})",
                ct);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = RolesApp.Administrador)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _tickets.EliminarAsync(id, ct);
            await _auditoria.RegistrarAsync(
                User.IdUsuario(), AccionesAuditoria.TicketEliminado, $"Ticket {id} eliminado", ct);

            return NoContent();
        }

        // ======================================================
        // Asignación y balanceo (supervisión)
        // ======================================================

        [HttpPost("assign")]
        [Authorize(Roles = RolesApp.Gestion)]
        public async Task<IActionResult> AssignAgent([FromBody] AsignarTicketRequest request, CancellationToken ct)
        {
            await _tickets.AsignarAgenteAsync(request.IdTicket, request.IdAgente, ct);
            await _auditoria.RegistrarAsync(
                User.IdUsuario(),
                AccionesAuditoria.TicketAsignado,
                $"Ticket {request.IdTicket} asignado al agente {request.IdAgente}",
                ct);

            return Ok(new { message = "Agente asignado correctamente." });
        }

        [HttpPost("{id:int}/escalar")]
        [Authorize(Roles = RolesApp.Gestion)]
        public async Task<IActionResult> EscalarTicket(
            int id,
            [FromBody] EscalarTicketRequest request,
            CancellationToken ct)
        {
            await _tickets.EscalarCategoriaAsync(id, request.NuevaCategoria, ct);
            await _auditoria.RegistrarAsync(
                User.IdUsuario(),
                AccionesAuditoria.TicketEscalado,
                $"Ticket {id} escalado a la categoría {request.NuevaCategoria}",
                ct);

            return Ok(new { message = "Ticket escalado correctamente." });
        }

        [HttpPost("redistribuir")]
        [Authorize(Roles = RolesApp.Gestion)]
        public async Task<IActionResult> RedistribuirTickets(CancellationToken ct)
        {
            var movidos = await _tickets.RedistribuirAsync(ct);
            await _auditoria.RegistrarAsync(
                User.IdUsuario(),
                AccionesAuditoria.TicketsRedistribuidos,
                $"{movidos} tickets reasignados por balanceo de carga",
                ct);

            return Ok(new
            {
                message = movidos == 0
                    ? "La carga ya estaba equilibrada; no hubo cambios."
                    : $"Se reasignaron {movidos} tickets para equilibrar la carga.",
                movidos
            });
        }

        [HttpPost("asignar-sin-agente")]
        [Authorize(Roles = RolesApp.Gestion)]
        public async Task<IActionResult> AsignarSinAgente(CancellationToken ct)
        {
            var asignados = await _tickets.AsignarSinAgenteAsync(ct);
            await _auditoria.RegistrarAsync(
                User.IdUsuario(),
                AccionesAuditoria.TicketsRedistribuidos,
                $"{asignados} tickets sin agente asignados",
                ct);

            return Ok(new
            {
                message = asignados == 0
                    ? "No había tickets sin agente."
                    : $"Se asignaron {asignados} tickets que no tenían agente.",
                asignados
            });
        }

        // ======================================================
        // Paneles e indicadores (supervisión)
        // ======================================================

        [HttpGet("dashboard")]
        [Authorize(Roles = RolesApp.Gestion)]
        public async Task<ActionResult<List<TicketPanelDto>>> GetDashboardTickets(CancellationToken ct) =>
            Ok(await _metricas.PanelAsync(ct));

        [HttpGet("vencidos")]
        [Authorize(Roles = RolesApp.Gestion)]
        public async Task<ActionResult<List<TicketPanelDto>>> GetTicketsVencidos(CancellationToken ct) =>
            Ok(await _metricas.VencidosAsync(ct));

        [HttpGet("weekly-performance")]
        [Authorize(Roles = RolesApp.Gestion)]
        public async Task<ActionResult<RendimientoSemanalDto>> GetWeeklyPerformance(CancellationToken ct) =>
            Ok(await _metricas.RendimientoSemanalAsync(ct));

        /// Política de SLA vigente (se configura en appsettings, sección "Sla").
        [HttpGet("sla")]
        [Authorize(Roles = RolesApp.Gestion)]
        public IActionResult GetSla([FromServices] Microsoft.Extensions.Options.IOptions<OpcionesSla> sla) =>
            Ok(new { horasVencimiento = sla.Value.HorasVencimiento });

        // ======================================================
        // Reportes PDF
        // ======================================================

        [HttpGet("reporte-carga")]
        [Authorize(Roles = RolesApp.Gestion)]
        public async Task<IActionResult> GenerarReporteCarga(CancellationToken ct)
        {
            var carga = await _metricas.CargaPorAgenteAsync(ct);
            return File(_reportes.CargaDeTrabajo(carga), "application/pdf", NombreArchivo("reporte-carga"));
        }

        [HttpGet("reporte-semanal")]
        [Authorize(Roles = RolesApp.Gestion)]
        public async Task<IActionResult> GenerarReporteSemanal(CancellationToken ct)
        {
            var resumen = await _metricas.ResumenSemanalAsync(ct);
            var comparativa = await _metricas.ComparativaAgentesAsync(ct);

            return File(
                _reportes.ResumenSemanal(resumen, comparativa),
                "application/pdf",
                NombreArchivo("reporte-semanal"));
        }

        [HttpGet("reporte-individual")]
        [Authorize(Roles = RolesApp.Gestion)]
        public async Task<IActionResult> GenerarReporteIndividual([FromQuery] int idAgente, CancellationToken ct)
        {
            var agente = await _agentes.ObtenerAsync(idAgente, ct);
            var tickets = await _tickets.ListarPorAgenteAsync(idAgente, ct);
            var metricas = (await _metricas.ComparativaAgentesAsync(ct))
                .FirstOrDefault(c => c.Name == agente.NombreUsuario);

            return File(
                _reportes.ReporteIndividual(agente.NombreUsuario, metricas, tickets),
                "application/pdf",
                NombreArchivo($"reporte-agente-{idAgente}"));
        }

        // ======================================================
        // Apoyo
        // ======================================================

        private Task AsegurarAccesoAsync(int idTicket, CancellationToken ct) =>
            _tickets.AsegurarAccesoAsync(
                idTicket,
                User.TieneVisionGlobal(),
                User.IdCliente(),
                User.IdAgente(),
                ct);

        private static string NombreArchivo(string prefijo) =>
            $"{prefijo}-{DateTime.UtcNow:yyyyMMdd-HHmm}.pdf";
    }
}
