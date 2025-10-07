using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Repositories
{
    public class AuditoriaRepository : ICrudRepository<Auditoria>
    {
        private readonly ServiceDeskContext _context;
        public AuditoriaRepository(ServiceDeskContext context)
        {
            _context = context;
        }
        public IEnumerable<Auditoria> GetAll()
        {
            return _context.Auditoria.ToList();
        }
        public Auditoria GetById(int id)
        {
            return _context.Auditoria.Find(id);
        }
        public void Add(Auditoria entity)
        {
            _context.Auditoria.Add(entity);
            _context.SaveChanges();
        }
        public void Update(Auditoria entity)
        {
            _context.Auditoria.Update(entity);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var auditoria = _context.Auditoria.Find(id);
            if (auditoria != null)
            {
                _context.Auditoria.Remove(auditoria);
                _context.SaveChanges();
            }
        }
    }
}
