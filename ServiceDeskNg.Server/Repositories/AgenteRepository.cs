using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Repositories
{
    public class AgenteRepository : ICrudRepository<Agente>
    {
        private readonly ServiceDeskContext _context;
        public AgenteRepository(ServiceDeskContext context)
        {
            _context = context;
        }
        public IEnumerable<Agente> GetAll()
        {
            return _context.Agentes.ToList();
        }
        public Agente GetById(int id)
        {
            return _context.Agentes.Find(id);
        }
        public void Add(Agente entity)
        {
            _context.Agentes.Add(entity);
            _context.SaveChanges();
        }
        public void Update(Agente entity)
        {
            _context.Agentes.Update(entity);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var agente = _context.Agentes.Find(id);
            if (agente != null)
            {
                _context.Agentes.Remove(agente);
                _context.SaveChanges();
            }
        }
    }
}
