using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Repositories
{
    public class SupervisorRepository : ICrudRepository<Supervisor>
    {
        private readonly ServiceDeskContext _context;
        public SupervisorRepository(ServiceDeskContext context)
        {
            _context = context;
        }
        public IEnumerable<Supervisor> GetAll()
        {
            return _context.Supervisores.ToList();
        }
        public Supervisor GetById(int id)
        {
            return _context.Supervisores.Find(id);
        }
        public void Add(Supervisor entity)
        {
            _context.Supervisores.Add(entity);
            _context.SaveChanges();
        }
        public void Update(Supervisor entity)
        {
            _context.Supervisores.Update(entity);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var supervisor = _context.Supervisores.Find(id);
            if (supervisor != null)
            {
                _context.Supervisores.Remove(supervisor);
                _context.SaveChanges();
            }
        }
    }
}
