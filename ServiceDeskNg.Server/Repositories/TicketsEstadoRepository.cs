using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Repositories
{
    public class TicketsEstadoRepository : ICrudRepository<TicketsEstado>
    {
        private readonly ServiceDeskContext _context;
        public TicketsEstadoRepository(ServiceDeskContext context)
        {
            _context = context;
        }
        public IEnumerable<TicketsEstado> GetAll()
        {
            return _context.TicketsEstados.ToList();
        }
        public TicketsEstado GetById(int id)
        {
            return _context.TicketsEstados.Find(id);
        }
        public void Add(TicketsEstado entity)
        {
            _context.TicketsEstados.Add(entity);
            _context.SaveChanges();
        }
        public void Update(TicketsEstado entity)
        {
            _context.TicketsEstados.Update(entity);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var estado = _context.TicketsEstados.Find(id);
            if (estado != null)
            {
                _context.TicketsEstados.Remove(estado);
                _context.SaveChanges();
            }
        }
    }
}
