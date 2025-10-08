using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Repositories
{
    public class TicketMensajeRepository : ICrudRepository<TicketMensaje>
    {
        private readonly ServiceDeskContext _context;
        public TicketMensajeRepository(ServiceDeskContext context)
        {
            _context = context;
        }
        public IEnumerable<TicketMensaje> GetAll()
        {
            return _context.TicketMensajes.ToList();
        }
        public TicketMensaje GetById(int id)
        {
            return _context.TicketMensajes.Find(id);
        }
        public void Add(TicketMensaje entity)
        {
            _context.TicketMensajes.Add(entity);
            _context.SaveChanges();
        }
        public void Update(TicketMensaje entity)
        {
            _context.TicketMensajes.Update(entity);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var mensaje = _context.TicketMensajes.Find(id);
            if (mensaje != null)
            {
                _context.TicketMensajes.Remove(mensaje);
                _context.SaveChanges();
            }
        }
    }
}
