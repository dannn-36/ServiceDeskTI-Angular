using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Repositories
{
    public class UsuarioRepository : ICrudRepository<Usuario>
    {
        private readonly ServiceDeskContext _context;
        public UsuarioRepository(ServiceDeskContext context)
        {
            _context = context;
        }
        public IEnumerable<Usuario> GetAll()
        {
            return _context.Usuarios.ToList();
        }
        public Usuario GetById(int id)
        {
            return _context.Usuarios.Find(id);
        }

        public void Add(Usuario entity)
        {
            _context.Usuarios.Add(entity);
            _context.SaveChanges();
        }

        public void Update(Usuario entity)
        {
            _context.Usuarios.Update(entity);
        }
        public void Delete(int id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                _context.SaveChanges();
            }
        }



    }
}
