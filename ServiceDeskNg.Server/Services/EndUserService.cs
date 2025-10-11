using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories;
namespace ServiceDeskNg.Server.Services
{
    public class EndUserService
    {

        private readonly EndUserRepository _endUserRepo;
        private readonly ServiceDeskContext _context;
        public EndUserService(EndUserRepository endUserRepo, ServiceDeskContext context) 
        { 
        
            _endUserRepo = endUserRepo;
            _context = context;
        }

        // Obtener todos los usuarios finales (con relaciones opcionales)
        public IEnumerable<EndUser> GetAll(bool includeRelations = false)
        {
            if (includeRelations)
            {
                return _context.Clientes
                    .Select(e => new EndUser
                    {
                        IdCliente = e.IdCliente,
                        IdUsuario = e.IdUsuario,
                        IdNivel = e.IdNivel,
                        IdNivelNavigation = e.IdNivelNavigation,
                        IdUsuarioNavigation = e.IdUsuarioNavigation,
                        Tickets = e.Tickets
                    })
                    .ToList();
            }
            return _endUserRepo.GetAll();
        }

        // Obtener un usuario final por ID
        public EndUser GetById(int id)
        {
            var endUser = _endUserRepo.GetById(id);
            if (endUser == null)
                throw new KeyNotFoundException($"No se encontró el usuario final con ID {id}");
            return endUser;
        }
        // Crear un nuevo usuario final
        public void Create(EndUser entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (entity.IdUsuario == 0)
                throw new ArgumentException("Debe asociarse un usuario válido.");
            var existing = _context.Clientes.FirstOrDefault(e => e.IdUsuario == entity.IdUsuario);
            if (existing != null)
                throw new InvalidOperationException("Ya existe un usuario final vinculado a ese usuario.");
            _endUserRepo.Add(entity);
        }
        // Actualizar un usuario final existente
        public void Update(EndUser entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            var existing = _endUserRepo.GetById(entity.IdCliente);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró el usuario final con ID {entity.IdCliente}");
            _endUserRepo.Update(entity);
        }

        // Eliminar un usuario final por ID
        public void Delete(int id)
        {
            var existing = _endUserRepo.GetById(id);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró el usuario final con ID {id}");
            _endUserRepo.Delete(id);
        }
    }
}
