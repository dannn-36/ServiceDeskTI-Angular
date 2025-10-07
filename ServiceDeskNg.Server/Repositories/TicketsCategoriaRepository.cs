using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Repositories
{
    public class TicketsCategoriaRepository : ICrudRepository<TicketsCategoria>
    {
        private readonly ServiceDeskContext _context;
        public TicketsCategoriaRepository(ServiceDeskContext context)
        {
            _context = context;
        }
        public IEnumerable<TicketsCategoria> GetAll()
        {
            return _context.TicketsCategorias.ToList();
        }
        public TicketsCategoria GetById(int id)
        {
            return _context.TicketsCategorias.Find(id);
        }
        public void Add(TicketsCategoria entity)
        {
            _context.TicketsCategorias.Add(entity);
            _context.SaveChanges();
        }
        public void Update(TicketsCategoria entity)
        {
            _context.TicketsCategorias.Update(entity);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var categoria = _context.TicketsCategorias.Find(id);
            if (categoria != null)
            {
                _context.TicketsCategorias.Remove(categoria);
                _context.SaveChanges();
            }
        }
    }
}
