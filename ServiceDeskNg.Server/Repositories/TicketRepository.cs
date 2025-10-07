using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Repositories
{
    public class TicketRepository : ICrudRepository<Ticket>
    {
        private readonly ServiceDeskContext _context;
        public TicketRepository(ServiceDeskContext context)
        {
            _context = context;
        }
        public IEnumerable<Ticket> GetAll()
        {
            return _context.Tickets.ToList();
        }
        public Ticket GetById(int id)
        {
            return _context.Tickets.Find(id);
        }
        public void Add(Ticket entity)
        {
            _context.Tickets.Add(entity);
            _context.SaveChanges();
        }
        public void Update(Ticket entity)
        {
            _context.Tickets.Update(entity);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var ticket = _context.Tickets.Find(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                _context.SaveChanges();
            }
        }
    }
}
