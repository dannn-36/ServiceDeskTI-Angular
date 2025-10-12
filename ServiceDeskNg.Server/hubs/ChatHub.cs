using Microsoft.AspNetCore.SignalR;
using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Services;
using System;
using System.Threading.Tasks;

namespace ServiceDeskNg.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly TicketMensajeService _mensajeService;
        private readonly ServiceDeskContext _context;

        public ChatHub(TicketMensajeService mensajeService, ServiceDeskContext context)
        {
            _mensajeService = mensajeService;
            _context = context;
        }

        // =====================================================
        // 🔹 Un usuario se une al grupo del ticket
        // =====================================================
        public async Task JoinTicket(string ticketId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ticketId);
            await Clients.Group(ticketId)
                .SendAsync("UserJoined", $" Un nuevo usuario se unió al chat del ticket {ticketId}");
        }

        // =====================================================
        // 🔹 El usuario sale del grupo del ticket
        // =====================================================
        public async Task LeaveTicket(string ticketId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticketId);
            await Clients.Group(ticketId)
                .SendAsync("UserLeft", $" Un usuario abandonó el chat del ticket {ticketId}");
        }

        // =====================================================
        // 🔹 Enviar mensaje (con persistencia en DB)
        // =====================================================
        public async Task SendMessage(string ticketId, string userName, string message, int userId)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            int ticketIdInt;
            if (!int.TryParse(ticketId, out ticketIdInt))
                throw new ArgumentException("El ticketId no es válido.");

            // Crear mensaje
            var nuevoMensaje = new TicketMensaje
            {
                IdTicket = ticketIdInt,
                IdUsuario = userId,
                MensajeTicket = message,
                FechaHoraCreacionMensaje = DateTime.UtcNow
            };

            try
            {
                // Persistir en DB
                _mensajeService.Create(nuevoMensaje);

                // Emitir a todos los usuarios del ticket
                await Clients.Group(ticketId)
                    .SendAsync("ReceiveMessage", new
                    {
                        usuario = userName,
                        mensaje = message,
                        fecha = nuevoMensaje.FechaHoraCreacionMensaje
                    });
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Error al enviar el mensaje: {ex.Message}");
            }
        }

        // =====================================================
        // 🔹 Al desconectarse
        // =====================================================
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Si quieres limpiar grupos, logs, etc. puedes hacerlo aquí
            await base.OnDisconnectedAsync(exception);
        }
    }
}
