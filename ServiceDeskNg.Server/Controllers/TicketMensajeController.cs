using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ServiceDeskNg.Server.Hubs;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Security;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Controllers
{
    /// Historial del chat de cada ticket. Solo lo ve quien tiene acceso al ticket.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketMensajeController : ControllerBase
    {
        private readonly TicketMensajeService _mensajes;
        private readonly TicketService _tickets;
        private readonly IHubContext<ChatHub> _chat;

        public TicketMensajeController(
            TicketMensajeService mensajes,
            TicketService tickets,
            IHubContext<ChatHub> chat)
        {
            _mensajes = mensajes;
            _tickets = tickets;
            _chat = chat;
        }

        [HttpGet("ticket/{ticketId:int}")]
        public async Task<ActionResult<List<TicketMensajeDto>>> GetByTicketId(int ticketId, CancellationToken ct)
        {
            await AsegurarAccesoAsync(ticketId, ct);
            return Ok(await _mensajes.ListarPorTicketAsync(ticketId, ct));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = RolesApp.Administrador)]
        public async Task<ActionResult<TicketMensajeDto>> GetById(int id, CancellationToken ct) =>
            Ok(await _mensajes.ObtenerAsync(id, ct));

        /// Alternativa REST al hub: el mensaje se guarda y se difunde igual
        /// a quienes estén conectados al chat del ticket.
        [HttpPost]
        public async Task<ActionResult<TicketMensajeDto>> Create(
            [FromBody] TicketMensajeCreateDto dto,
            CancellationToken ct)
        {
            await AsegurarAccesoAsync(dto.IdTicket, ct);

            var guardado = await _mensajes.CrearAsync(dto.IdTicket, User.IdUsuario(), dto.MensajeTicket, ct);

            await _chat.Clients
                .Group(ChatHub.Grupo(dto.IdTicket))
                .SendAsync(ChatHub.EventoMensaje, ChatHub.Payload(guardado), ct);

            return CreatedAtAction(nameof(GetById), new { id = guardado.IdMensaje }, guardado);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = RolesApp.Administrador)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _mensajes.EliminarAsync(id, ct);
            return NoContent();
        }

        private Task AsegurarAccesoAsync(int idTicket, CancellationToken ct) =>
            _tickets.AsegurarAccesoAsync(
                idTicket,
                User.TieneVisionGlobal(),
                User.IdCliente(),
                User.IdAgente(),
                ct);
    }
}
