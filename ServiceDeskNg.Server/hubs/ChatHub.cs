using Microsoft.AspNetCore.SignalR;

namespace ServiceDeskNg.Server.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string ticketId, string user, string message)
        {
            await Clients.Group(ticketId).SendAsync("ReceiveMessage", user, message);
        }

        public async Task JoinTicket(string ticketId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ticketId);
        }

        public async Task LeaveTicket(string ticketId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticketId);
        }
    }
}
