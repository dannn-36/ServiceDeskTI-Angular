using System.Security.Cryptography.X509Certificates;
using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories;

namespace ServiceDeskNg.Server.Services
{
    public class SupervisorService
    {
        private readonly SupervisorRepository _supervisorRepo;
        private readonly ServiceDeskContext _context;

        public SupervisorService(SupervisorRepository supervisorRepo, ServiceDeskContext context)
        {
            _supervisorRepo = supervisorRepo;
            _context = context;
        }

        // Obtener todos los supervisores (con relaciones opcionales)
        public IEnumerable<Supervisor> GetAll(bool includeRelations = false)
        {
            if (includeRelations)
            {
                return _context.Supervisores
                    .Select(s => new Supervisor
                    {
                        IdSupervisor = s.IdSupervisor,
                        IdUsuario = s.IdUsuario,
                        IdNivel = s.IdNivel,
                        AreaResponsabilidadSupervisor = s.AreaResponsabilidadSupervisor,

                    })
                    .ToList();
            }
            return _supervisorRepo.GetAll();

            
        }
        // Obtener un supervisor por ID
        public Supervisor GetById(int id)
        {
            var supervisor = _supervisorRepo.GetById(id);
            if (supervisor == null)
                throw new KeyNotFoundException($"No se encontró el supervisor con ID {id}");
            return supervisor;
        }
        // Crear un nuevo supervisor
        public void Create(Supervisor entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (entity.IdUsuario == 0)
                throw new ArgumentException("Debe asociarse un usuario válido.");
            if (string.IsNullOrWhiteSpace(entity.AreaResponsabilidadSupervisor))
                throw new ArgumentException("El área de responsabilidad es obligatoria.");
            var existing = _context.Supervisores.FirstOrDefault(s => s.IdUsuario == entity.IdUsuario);
            if (existing != null)
                throw new InvalidOperationException("Ya existe un supervisor vinculado a ese usuario.");
            _supervisorRepo.Add(entity);
        }

        public void Update(Supervisor entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            var existing = _supervisorRepo.GetById(entity.IdSupervisor);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró el supervisor con ID {entity.IdSupervisor}");
            _supervisorRepo.Update(entity);
        }

        public void Delete(int id)
        {
            var existing = _supervisorRepo.GetById(id);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró el supervisor con ID {id}");
            _supervisorRepo.Delete(id);
        }
    }
}
