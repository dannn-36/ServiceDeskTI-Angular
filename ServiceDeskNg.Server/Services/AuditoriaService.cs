using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories;

namespace ServiceDeskNg.Server.Services
{
    public class AuditoriaService
    {

        private readonly AuditoriaRepository _auditoriaRepo;
        private readonly ServiceDeskContext _context;

        public AuditoriaService(AuditoriaRepository auditoriaRepo, ServiceDeskContext context)
        {
            _auditoriaRepo = auditoriaRepo;
            _context = context;
        }

        // Obtener todas las auditorías
        public IEnumerable<Auditoria> GetAll(bool includeRelations = false)
        {
            if (includeRelations)
            {
                return _context.Auditoria
                    .Select(a => new Auditoria
                    {
                        IdAuditoria = a.IdAuditoria,
                        IdUsuario = a.IdUsuario,
                        AccionAuditoria = a.AccionAuditoria,
                        DetalleAuditoria = a.DetalleAuditoria,
                        FechaAuditoria = a.FechaAuditoria,  
                        IdUsuarioNavigation = a.IdUsuarioNavigation
                    })
                    .ToList();
            }
            return _auditoriaRepo.GetAll();
        }

        // Obtener una auditoría por ID
        public Auditoria GetById(int id)
        {
            var auditoria = _auditoriaRepo.GetById(id);
            if (auditoria == null)
                throw new KeyNotFoundException($"No se encontró la auditoría con ID {id}");
            return auditoria;
        }

        // Crear una nueva auditoría
        public void Create(Auditoria entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (entity.IdUsuario == 0)
                throw new ArgumentException("Debe asociarse un usuario válido.");
            if (string.IsNullOrWhiteSpace(entity.AccionAuditoria))
                throw new ArgumentException("La acción de auditoría es obligatoria.");
            if (entity.FechaAuditoria == default)
                entity.FechaAuditoria = DateTime.UtcNow;
            _auditoriaRepo.Add(entity);
        }

    }
}
