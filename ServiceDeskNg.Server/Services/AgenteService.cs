using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories;

namespace ServiceDeskNg.Server.Services
{
    public class AgenteService
    {
        private readonly AgenteRepository _agenteRepo;
        private readonly ServiceDeskContext _context;

        public AgenteService(AgenteRepository agenteRepo, ServiceDeskContext context)
        {
            _agenteRepo = agenteRepo;
            _context = context;
        }

        // Obtener todos los agentes (con relaciones opcionales)
        public IEnumerable<Agente> GetAll(bool includeRelations = false)
        {
            if (includeRelations)
            {
                return _context.Agentes
                    .Select(a => new Agente
                    {
                        IdAgente = a.IdAgente,
                        IdUsuario = a.IdUsuario,
                        IdNivel = a.IdNivel,
                        EspecialidadAgente = a.EspecialidadAgente,
                        DisponibilidadAgente = a.DisponibilidadAgente,
                        IdUsuarioNavigation = a.IdUsuarioNavigation,
                        IdNivelNavigation = a.IdNivelNavigation
                    })
                    .ToList();
            }
            return _agenteRepo.GetAll();
        }

        // Obtener un agente por ID
        public Agente GetById(int id)
        {
            var agente = _agenteRepo.GetById(id);
            if (agente == null)
                throw new KeyNotFoundException($"No se encontró el agente con ID {id}");
            return agente;
        }

        // Crear un nuevo agente
        public void Create(Agente entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (entity.IdUsuario == 0)
                throw new ArgumentException("Debe asociarse un usuario válido.");
            if (string.IsNullOrWhiteSpace(entity.EspecialidadAgente))
                throw new ArgumentException("La especialidad es obligatoria.");
            var existing = _context.Agentes.FirstOrDefault(a => a.IdUsuario == entity.IdUsuario);
            if (existing != null)
                throw new InvalidOperationException("Ya existe un agente vinculado a ese usuario.");
            _agenteRepo.Add(entity);
        }

        // Actualizar un agente existente
        public void Update(Agente entity)
        {
            var existing = _agenteRepo.GetById(entity.IdAgente);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró el agente con ID {entity.IdAgente}");
            if (string.IsNullOrWhiteSpace(entity.EspecialidadAgente))
                throw new ArgumentException("La especialidad no puede estar vacía.");
            _agenteRepo.Update(entity);
        }

        // Eliminar un agente
        public void Delete(int id)
        {
            var agente = _agenteRepo.GetById(id);
            if (agente == null)
                throw new KeyNotFoundException($"No existe el agente con ID {id}");
            _agenteRepo.Delete(id);
        }
    }
}
