using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Security;
using ServiceDeskNg.Server.Services;

namespace ServiceDeskNg.Server.Hubs
{
    /// Chat en tiempo real de cada ticket.
    /// Requiere sesión iniciada. El autor de cada mensaje sale de la cookie de sesión:
    /// antes el cliente enviaba su propio userId y userName, y cualquiera podía
    /// escribir haciéndose pasar por otra persona.
    [Authorize]
    public class ChatHub : Hub
    {
        public const string EventoMensaje = "ReceiveMessage";

        private readonly TicketMensajeService _mensajes;
        private readonly TicketService _tickets;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(TicketMensajeService mensajes, TicketService tickets, ILogger<ChatHub> logger)
        {
            _mensajes = mensajes;
            _tickets = tickets;
            _logger = logger;
        }

        public static string Grupo(int idTicket) => $"ticket-{idTicket}";

        /// Payload que reciben los clientes conectados al ticket.
        public static object Payload(TicketMensajeDto mensaje) => new
        {
            idMensaje = mensaje.IdMensaje,
            idTicket = mensaje.IdTicket,
            idUsuario = mensaje.IdUsuario,
            usuario = mensaje.UsuarioNombre,
            mensaje = mensaje.MensajeTicket,
            fecha = mensaje.FechaHoraCreacionMensaje
        };

        public async Task JoinTicket(string ticketId)
        {
            var idTicket = await AutorizarTicketAsync(ticketId);
            await Groups.AddToGroupAsync(Context.ConnectionId, Grupo(idTicket));
        }

        public async Task LeaveTicket(string ticketId)
        {
            if (int.TryParse(ticketId, out var idTicket))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, Grupo(idTicket));
        }

        public async Task SendMessage(string ticketId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            if (message.Length > 2000)
                throw new HubException("El mensaje no puede superar los 2000 caracteres.");

            var idTicket = await AutorizarTicketAsync(ticketId);
            var usuario = Context.User!;

            var guardado = await _mensajes.CrearAsync(idTicket, usuario.IdUsuario(), message);

            await Clients.Group(Grupo(idTicket)).SendAsync(EventoMensaje, Payload(guardado));
        }

        private async Task<int> AutorizarTicketAsync(string ticketId)
        {
            if (!int.TryParse(ticketId, out var idTicket) || idTicket <= 0)
                throw new HubException("El identificador del ticket no es válido.");

            var usuario = Context.User!;

            try
            {
                await _tickets.AsegurarAccesoAsync(
                    idTicket,
                    usuario.TieneVisionGlobal(),
                    usuario.IdCliente(),
                    usuario.IdAgente());
            }
            catch (Exception ex) when (ex is AccesoDenegadoException or KeyNotFoundException)
            {
                _logger.LogWarning(
                    "Usuario {IdUsuario} sin acceso al chat del ticket {IdTicket}",
                    usuario.IdUsuario(),
                    idTicket);
                throw new HubException(ex.Message);
            }

            return idTicket;
        }
    }
}
