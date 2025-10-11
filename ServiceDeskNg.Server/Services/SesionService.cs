using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories;

namespace ServiceDeskNg.Server.Services
{
    public class SesionService
    {
        private readonly SesionRepository _sesionRepo;
        private readonly ServiceDeskContext _context;

        // Implementación del servicio de sesión
        public SesionService(SesionRepository sesionRepo, ServiceDeskContext context) 
        {
            _sesionRepo = sesionRepo;
            _context = context;
        }

        public IEnumerable<Sesion> GetAll( bool includeRelations = false)
            {
            if (includeRelations)
            {
                return _context.Sesiones
                    .Select(s => new Sesion
                    {
                        IdSesion = s.IdSesion,
                        IdUsuario = s.IdUsuario,
                        FechaHoraInicioSesion = s.FechaHoraInicioSesion,
                        FechaHoraFinSesion = s.FechaHoraFinSesion,
                        SesionActiva = s.SesionActiva,
                        IdUsuarioNavigation = s.IdUsuarioNavigation
                    })
                    .ToList();
            } 
            return _sesionRepo.GetAll();
        }

        public Sesion GetById(int id)
        {
            var sesion = _sesionRepo.GetById(id);
            if (sesion == null)
                throw new KeyNotFoundException($"No se encontró la sesión con ID {id}");
            return sesion;
        }

        public void Create(Sesion entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (entity.IdUsuario == 0)
                throw new ArgumentException("Debe asociarse un usuario válido.");
            _sesionRepo.Add(entity);
        }



    }
}
