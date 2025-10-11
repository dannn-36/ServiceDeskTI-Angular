using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Repositories;

namespace ServiceDeskNg.Server.Services
{
    public class TicketsCategoriaService
    {
        private readonly ServiceDeskContext _context;
        private readonly TicketsCategoriaRepository _ticketsCategoriaRepo;
        public TicketsCategoriaService(ServiceDeskContext context, TicketsCategoriaRepository ticketsCategoriaRepos)
        {
            _context = context;
            _ticketsCategoriaRepo = ticketsCategoriaRepos;
        }
        // Obtener todas las categorías de tickets
        public IEnumerable<Models.TicketsCategoria> GetAll()
        {
            return _ticketsCategoriaRepo.GetAll();
        }
        // Obtener una categoría de ticket por ID
        public Models.TicketsCategoria GetById(int id)
        {
            var categoria = _ticketsCategoriaRepo.GetById(id);
            if (categoria == null)
                throw new KeyNotFoundException($"No se encontró la categoría de ticket con ID {id}");
            return categoria;
        }
        // Crear una nueva categoría de ticket
        public void Create(Models.TicketsCategoria entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrWhiteSpace(entity.NombreCategoria))
                throw new ArgumentException("El nombre de la categoría es obligatorio.");
            var existing = _context.TicketsCategorias.FirstOrDefault(c => c.NombreCategoria == entity.NombreCategoria);
            if (existing != null)
                throw new InvalidOperationException("Ya existe una categoría de ticket con ese nombre.");
            _ticketsCategoriaRepo.Add(entity);
        }

        // Actualizar una categoría de ticket existente
        public void Update(Models.TicketsCategoria entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            var existing = _ticketsCategoriaRepo.GetById(entity.IdCategoria);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró la categoría de ticket con ID {entity.IdCategoria}");
            if (string.IsNullOrWhiteSpace(entity.NombreCategoria))
                throw new ArgumentException("El nombre de la categoría es obligatorio.");
            var duplicate = _context.TicketsCategorias.FirstOrDefault(c => c.NombreCategoria == entity.NombreCategoria && c.IdCategoria != entity.IdCategoria);
            if (duplicate != null)
                throw new InvalidOperationException("Ya existe otra categoría de ticket con ese nombre.");
            _ticketsCategoriaRepo.Update(entity);
        }
        // Eliminar una categoría de ticket por ID
        public void Delete(int id)
        {
            var existing = _ticketsCategoriaRepo.GetById(id);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró la categoría de ticket con ID {id}");
            _ticketsCategoriaRepo.Delete(id);
        }

    }

}
