using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Repositories
{
    public class SesionRepository : ICrudRepository<Sesion>
    {
        private readonly ServiceDeskContext _context;
        public SesionRepository(ServiceDeskContext context)
        {
            _context = context;
        }
        public IEnumerable<Sesion> GetAll()
        {
            return _context.Sesiones.ToList();
        }
        public Sesion GetById(int id)
        {
            return _context.Sesiones.Find(id);
        }
        public void Add(Sesion entity)
        {
            _context.Sesiones.Add(entity);
            _context.SaveChanges();
        }
        public void Update(Sesion entity)
        {
            _context.Sesiones.Update(entity);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var sesion = _context.Sesiones.Find(id);
            if (sesion != null)
            {
                _context.Sesiones.Remove(sesion);
                _context.SaveChanges();
            }
        }
    }
}
