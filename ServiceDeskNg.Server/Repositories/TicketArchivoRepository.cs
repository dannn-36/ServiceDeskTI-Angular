using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Repositories
{
    public class TicketArchivoRepository : ICrudRepository<TicketArchivo>
    {
        private readonly ServiceDeskContext _context;
        public TicketArchivoRepository(ServiceDeskContext context)
        {
            _context = context;
        }
        public IEnumerable<TicketArchivo> GetAll()
        {
            return _context.TicketArchivos.ToList();
        }
        public TicketArchivo GetById(int id)
        {
            return _context.TicketArchivos.Find(id);
        }
        public void Add(TicketArchivo entity)
        {
            _context.TicketArchivos.Add(entity);
            _context.SaveChanges();
        }
        public void Update(TicketArchivo entity)
        {
            _context.TicketArchivos.Update(entity);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var archivo = _context.TicketArchivos.Find(id);
            if (archivo != null)
            {
                _context.TicketArchivos.Remove(archivo);
                _context.SaveChanges();
            }
        }
    }
}
