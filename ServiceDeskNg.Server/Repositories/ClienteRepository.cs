using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Repositories
{
    public class ClienteRepository : ICrudRepository<Cliente>
    {
        private readonly ServiceDeskContext _context;
        public ClienteRepository(ServiceDeskContext context)
        {
            _context = context;
        }
        public IEnumerable<Cliente> GetAll()
        {
            return _context.Clientes.ToList();
        }
        public Cliente GetById(int id)
        {
            return _context.Clientes.Find(id);
        }
        public void Add(Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            _context.SaveChanges();
        }
        public void Update(Cliente cliente)
        {
            _context.Clientes.Update(cliente);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var cliente = _context.Clientes.Find(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
                _context.SaveChanges();
            }
        }
    }
}
