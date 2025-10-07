using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Repositories
{
    public class AdministradorRepository : ICrudRepository<Administrador>
    {
        private readonly ServiceDeskContext _context;
        public AdministradorRepository(ServiceDeskContext context)
        {
            _context = context;
        }
        public IEnumerable<Administrador> GetAll()
        {
            return _context.Administradores.ToList();
        }
        public Administrador GetById(int id)
        {
            return _context.Administradores.Find(id);
        }
        public void Add(Administrador entity)
        {
            _context.Administradores.Add(entity);
            _context.SaveChanges();
        }
        public void Update(Administrador entity)
        {
            _context.Administradores.Update(entity);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var administrador = _context.Administradores.Find(id);
            if (administrador != null)
            {
                _context.Administradores.Remove(administrador);
                _context.SaveChanges();
            }
        }
    }
}
