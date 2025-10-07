using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Repositories
{
    public class NivelesAccesoRepository : ICrudRepository<NivelesAcceso>
    {
        private readonly ServiceDeskContext _context;
        public NivelesAccesoRepository(ServiceDeskContext context)
        {
            _context = context;
        }
        public IEnumerable<NivelesAcceso> GetAll()
        {
            return _context.NivelesAccesos.ToList();
        }
        public NivelesAcceso GetById(int id)
        {
            return _context.NivelesAccesos.Find(id);
        }
        public void Add(NivelesAcceso entity)
        {
            _context.NivelesAccesos.Add(entity);
            _context.SaveChanges();
        }
        public void Update(NivelesAcceso entity)
        {
            _context.NivelesAccesos.Update(entity);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var nivel = _context.NivelesAccesos.Find(id);
            if (nivel != null)
            {
                _context.NivelesAccesos.Remove(nivel);
                _context.SaveChanges();
            }
        }
    }
}
