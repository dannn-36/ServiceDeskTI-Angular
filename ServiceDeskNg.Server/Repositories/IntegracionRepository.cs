using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Repositories
{
    public class IntegracionRepository : ICrudRepository<Integracion>
    {
        private readonly ServiceDeskContext _context;
        public IntegracionRepository(ServiceDeskContext context)
        {
            _context = context;
        }
        public IEnumerable<Integracion> GetAll()
        {
            return _context.Integraciones.ToList();
        }
        public Integracion GetById(int id)
        {
            return _context.Integraciones.Find(id);
        }
        public void Add(Integracion entity)
        {
            _context.Integraciones.Add(entity);
            _context.SaveChanges();
        }
        public void Update(Integracion entity)
        {
            _context.Integraciones.Update(entity);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var integracion = _context.Integraciones.Find(id);
            if (integracion != null)
            {
                _context.Integraciones.Remove(integracion);
                _context.SaveChanges();
            }
        }
    }
}
