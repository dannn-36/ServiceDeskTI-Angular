using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Services
{
    /// Mensajes del chat de cada ticket.
    /// El autor del mensaje lo decide siempre el servidor a partir de la sesión:
    /// nunca se toma del cuerpo de la petición ni de los parámetros del hub.
    public class TicketMensajeService
    {
        private readonly IRepositorio<TicketMensaje> _mensajes;
        private readonly IRepositorio<Ticket> _tickets;

        public TicketMensajeService(IRepositorio<TicketMensaje> mensajes, IRepositorio<Ticket> tickets)
        {
            _mensajes = mensajes;
            _tickets = tickets;
        }

        public async Task<List<TicketMensajeDto>> ListarPorTicketAsync(
            int idTicket,
            CancellationToken ct = default)
        {
            if (!await _tickets.Query().AnyAsync(t => t.IdTicket == idTicket, ct))
                throw new KeyNotFoundException($"No se encontró el ticket con ID {idTicket}");

            return await Consulta()
                .Where(m => m.IdTicket == idTicket)
                .OrderBy(m => m.FechaHoraCreacionMensaje)
                .ThenBy(m => m.IdMensaje)
                .ToListAsync(ct);
        }

        public async Task<TicketMensajeDto> ObtenerAsync(int id, CancellationToken ct = default)
        {
            var mensaje = await Consulta().FirstOrDefaultAsync(m => m.IdMensaje == id, ct);
            return mensaje ?? throw new KeyNotFoundException($"No se encontró el mensaje con ID {id}");
        }

        public async Task<TicketMensajeDto> CrearAsync(
            int idTicket,
            int idUsuario,
            string texto,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(texto))
                throw new ArgumentException("El mensaje es obligatorio.");

            if (!await _tickets.Query().AnyAsync(t => t.IdTicket == idTicket, ct))
                throw new KeyNotFoundException($"No existe el ticket con ID {idTicket}");

            var mensaje = new TicketMensaje
            {
                IdTicket = idTicket,
                IdUsuario = idUsuario,
                MensajeTicket = texto.Trim(),
                FechaHoraCreacionMensaje = DateTime.UtcNow
            };

            await _mensajes.AddAsync(mensaje, ct);
            return await ObtenerAsync(mensaje.IdMensaje, ct);
        }

        public async Task EliminarAsync(int id, CancellationToken ct = default)
        {
            if (!await _mensajes.DeleteAsync(id, ct))
                throw new KeyNotFoundException($"No se encontró el mensaje con ID {id}");
        }

        private IQueryable<TicketMensajeDto> Consulta() =>
            _mensajes.Query().Select(m => new TicketMensajeDto
            {
                IdMensaje = m.IdMensaje,
                IdTicket = m.IdTicket,
                IdUsuario = m.IdUsuario,
                MensajeTicket = m.MensajeTicket,
                FechaHoraCreacionMensaje = m.FechaHoraCreacionMensaje,
                UsuarioNombre = m.IdUsuarioNavigation.NombreUsuario
            });
    }
}
