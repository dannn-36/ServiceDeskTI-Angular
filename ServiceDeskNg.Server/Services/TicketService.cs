using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Repositories;
using ServiceDeskNg.Server.Models;

namespace ServiceDeskNg.Server.Services
{
    public class TicketService
    {
        private readonly TicketRepository _ticketRepo;
        private readonly ServiceDeskContext _context;

        public TicketService(TicketRepository ticketRepo, ServiceDeskContext context)
        {
            _ticketRepo = ticketRepo;
            _context = context;
        }

        // ✅ Obtener todos los tickets (con relaciones opcionales)
        public IEnumerable<Ticket> GetAll(bool includeRelations = false)
        {
            if (includeRelations)
            {
                return _context.Tickets
                    .Include(t => t.IdClienteNavigation)
                    .Include(t => t.IdAgenteAsignadoNavigation)
                    .Include(t => t.IdEstadoTicketNavigation)
                    .Include(t => t.IdCategoriaTicketNavigation)
                    .ToList();
            }

            return _ticketRepo.GetAll();
        }

        // ✅ Obtener ticket por ID
        public Ticket GetById(int id, bool includeRelations = false)
        {
            var query = _context.Tickets.AsQueryable();

            if (includeRelations)
            {
                query = query
                    .Include(t => t.IdClienteNavigation)
                    .Include(t => t.IdAgenteAsignadoNavigation)
                    .Include(t => t.IdEstadoTicketNavigation)
                    .Include(t => t.IdCategoriaTicketNavigation);
            }

            var ticket = query.FirstOrDefault(t => t.IdTicket == id);

            if (ticket == null)
                throw new KeyNotFoundException($"No se encontró el ticket con ID {id}");

            return ticket;
        }

        // ✅ Obtener tickets por cliente
        public IEnumerable<Ticket> GetByClienteId(int clienteId)
        {
            bool exists = _context.Clientes.Any(c => c.IdCliente == clienteId);
            if (!exists)
                throw new KeyNotFoundException($"No se encontró el cliente con ID {clienteId}");

            return _context.Tickets
                .Where(t => t.IdCliente == clienteId)
                .Include(t => t.IdEstadoTicketNavigation)
                .Include(t => t.IdCategoriaTicketNavigation)
                .OrderByDescending(t => t.FechaHoraCreacionTicket)
                .ToList();
        }

        // Obtener tickets asignados a un agente
        public IEnumerable<Ticket> GetByAgenteId(int agenteId)
        {
            return _context.Tickets
                .Where(t => t.IdAgenteAsignado == agenteId)
                .Include(t => t.IdEstadoTicketNavigation)
                .Include(t => t.IdCategoriaTicketNavigation)
                .ToList();
        }

        // ✅ Crear nuevo ticket
        public void Create(Ticket entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.IdCliente == 0)
                throw new ArgumentException("Debe asociarse un cliente válido.");

            if (string.IsNullOrWhiteSpace(entity.TituloTicket))
                throw new ArgumentException("El título del ticket es obligatorio.");

            if (string.IsNullOrWhiteSpace(entity.DescripcionTicket))
                throw new ArgumentException("La descripción del ticket es obligatoria.");

            entity.FechaHoraCreacionTicket = DateTime.UtcNow;

            // Si no se especifica estado, por defecto "Abierto" (por ID)
            if (entity.IdEstadoTicket == 0)
            {
                var estadoAbierto = _context.TicketsEstados
                    .FirstOrDefault(e => e.NombreEstado == "Abierto");
                if (estadoAbierto != null)
                    entity.IdEstadoTicket = estadoAbierto.IdEstado;
            }

            // Balanceo: Asignar agente disponible con menos tickets abiertos/en-progreso
            var estadosActivos = _context.TicketsEstados
                .Where(e => e.NombreEstado.ToLower() == "abierto" || e.NombreEstado.ToLower() == "en-progreso")
                .Select(e => e.IdEstado)
                .ToList();

            var agenteDisponible = _context.Agentes
                .Where(a => a.DisponibilidadAgente == true)
                .Select(a => new {
                    Agente = a,
                    TicketsActivos = _context.Tickets.Count(t => t.IdAgenteAsignado == a.IdAgente && estadosActivos.Contains(t.IdEstadoTicket))
                })
                .OrderBy(x => x.TicketsActivos)
                .Select(x => x.Agente)
                .FirstOrDefault();

            if (agenteDisponible != null)
            {
                entity.IdAgenteAsignado = agenteDisponible.IdAgente;
                Console.WriteLine($"[TicketService] Ticket asignado al agente: {agenteDisponible.IdAgente}");
            }
            else
            {
                Console.WriteLine("[TicketService] No hay agentes disponibles para asignar el ticket.");
            }

            _ticketRepo.Add(entity);
            Console.WriteLine($"[TicketService] Ticket creado: {entity.IdTicket}, Agente asignado: {entity.IdAgenteAsignado}");
        }

        // ✅ Actualizar ticket existente
        public void Update(Ticket entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var existing = _ticketRepo.GetById(entity.IdTicket);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró el ticket con ID {entity.IdTicket}");

            if (string.IsNullOrWhiteSpace(entity.TituloTicket))
                throw new ArgumentException("El título del ticket es obligatorio.");

            if (string.IsNullOrWhiteSpace(entity.DescripcionTicket))
                throw new ArgumentException("La descripción del ticket es obligatoria.");

            entity.FechaHoraActualizacionTicket = DateTime.UtcNow;
            _ticketRepo.Update(entity);
        }

        // ✅ Eliminar ticket
        public void Delete(int id)
        {
            var ticket = _ticketRepo.GetById(id);
            if (ticket == null)
                throw new KeyNotFoundException($"No se encontró el ticket con ID {id}");

            _ticketRepo.Delete(id);
        }
    }
}
