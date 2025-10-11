using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Repositories;

namespace ServiceDeskNg.Server.Services
{
    public class TicketsEstadoService
    {
        private readonly TicketsEstadoRepository _ticketsEstadoRepo;
        private readonly ServiceDeskContext _context;

        public TicketsEstadoService(TicketsEstadoRepository ticketsEstadoRepo, ServiceDeskContext context)
        {
            _ticketsEstadoRepo = ticketsEstadoRepo;
            _context = context;
        }

        // ✅ Obtener todos los estados de tickets
        public IEnumerable<Models.TicketsEstado> GetAll()
        {
            return _ticketsEstadoRepo.GetAll();
        }

        // ✅ Obtener un estado de ticket por ID
        public Models.TicketsEstado GetById(int id)
        {
            var estado = _ticketsEstadoRepo.GetById(id);
            if (estado == null)
                throw new KeyNotFoundException($"No se encontró el estado de ticket con ID {id}");

            return estado;
        }

        // ✅ Crear un nuevo estado de ticket
        public void Create(Models.TicketsEstado entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(entity.NombreEstado))
                throw new ArgumentException("El nombre del estado es obligatorio.");

            // Corregido: uso de TicketsEstados (plural)
            var existing = _context.TicketsEstados
                .FirstOrDefault(e => e.NombreEstado == entity.NombreEstado);

            if (existing != null)
                throw new InvalidOperationException("Ya existe un estado de ticket con ese nombre.");

            _ticketsEstadoRepo.Add(entity);
        }

        // ✅ Actualizar un estado de ticket existente
        public void Update(Models.TicketsEstado entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var existing = _ticketsEstadoRepo.GetById(entity.IdEstado);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró el estado de ticket con ID {entity.IdEstado}");

            if (string.IsNullOrWhiteSpace(entity.NombreEstado))
                throw new ArgumentException("El nombre del estado es obligatorio.");

            // Corregido: uso de TicketsEstados (plural)
            var duplicate = _context.TicketsEstados
                .FirstOrDefault(e => e.NombreEstado == entity.NombreEstado && e.IdEstado != entity.IdEstado);

            if (duplicate != null)
                throw new InvalidOperationException("Ya existe otro estado de ticket con ese nombre.");

            _ticketsEstadoRepo.Update(entity);
        }

        // ✅ Eliminar un estado de ticket por ID
        public void Delete(int id)
        {
            var existing = _ticketsEstadoRepo.GetById(id);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró el estado de ticket con ID {id}");

            _ticketsEstadoRepo.Delete(id);
        }
    }
}
