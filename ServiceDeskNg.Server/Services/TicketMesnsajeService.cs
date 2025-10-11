using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Repositories;

namespace ServiceDeskNg.Server.Services
{
    public class TicketMensajeService
    {
        private readonly TicketMensajeRepository _ticketMensajeRepo;
        private readonly ServiceDeskContext _context;

        public TicketMensajeService(TicketMensajeRepository ticketMensajeRepo, ServiceDeskContext context)
        {
            _ticketMensajeRepo = ticketMensajeRepo;
            _context = context;
        }

        // ✅ Obtener todos los mensajes de todos los tickets
        public IEnumerable<Models.TicketMensaje> GetAll()
        {
            return _ticketMensajeRepo.GetAll();
        }

        // ✅ Obtener un mensaje de ticket por ID
        public Models.TicketMensaje GetById(int id)
        {
            var mensaje = _ticketMensajeRepo.GetById(id);
            if (mensaje == null)
                throw new KeyNotFoundException($"No se encontró el mensaje con ID {id}");

            return mensaje;
        }

        // ✅ Obtener todos los mensajes de un ticket específico
        public IEnumerable<Models.TicketMensaje> GetMensajesByTicketId(int ticketId, bool includeRelations = false)
        {
            // Primero validamos que el ticket exista
            bool ticketExists = _context.Tickets.Any(t => t.IdTicket == ticketId);
            if (!ticketExists)
                throw new KeyNotFoundException($"No se encontró el ticket con ID {ticketId}");

            // Si se solicitan relaciones, incluimos la info del usuario que escribió el mensaje
            if (includeRelations)
            {
                return _context.TicketMensajes
                    .Where(m => m.IdTicket == ticketId)
                    .Include(m => m.IdUsuarioNavigation) // incluye datos del usuario que envió el mensaje
                    .OrderBy(m => m.FechaHoraCreacionMensaje)
                    .ToList();
            }

            // Si no se piden relaciones, solo devolvemos los mensajes planos
            return _context.TicketMensajes
                .Where(m => m.IdTicket == ticketId)
                .OrderBy(m => m.FechaHoraCreacionMensaje)
                .ToList();
        }

        // ✅ Crear un nuevo mensaje
        public void Create(Models.TicketMensaje entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.IdTicket == 0)
                throw new ArgumentException("Debe asociarse a un ticket válido.");

            if (string.IsNullOrWhiteSpace(entity.MensajeTicket))
                throw new ArgumentException("El mensaje es obligatorio.");

            bool ticketExists = _context.Tickets.Any(t => t.IdTicket == entity.IdTicket);
            if (!ticketExists)
                throw new KeyNotFoundException($"No existe el ticket con ID {entity.IdTicket}");

            _ticketMensajeRepo.Add(entity);
        }

        // ✅ Eliminar un mensaje por ID
        public void Delete(int id)
        {
            var mensaje = _ticketMensajeRepo.GetById(id);
            if (mensaje == null)
                throw new KeyNotFoundException($"No se encontró el mensaje con ID {id}");

            _ticketMensajeRepo.Delete(id);
        }
    }
}
