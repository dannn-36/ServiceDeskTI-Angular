using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories;

namespace ServiceDeskNg.Server.Services
{
    public class NivelesAccesoService
    {

        private readonly ServiceDeskContext _context;
        private readonly NivelesAccesoRepository _nivelesAccesoRepo;

        public NivelesAccesoService( NivelesAccesoRepository nivelesAccesoRepo, ServiceDeskContext context)
        {
            _nivelesAccesoRepo = nivelesAccesoRepo;
            _context = context;
        }
        // Obtener todos los niveles de acceso
        public IEnumerable<NivelesAcceso> GetAll(bool includeRelations = false)
        {
            if (includeRelations)
            {
                // Si en el futuro NivelesAcceso tiene relaciones, se pueden incluir aquí
                return _context.NivelesAccesos
                    .Select(n => new NivelesAcceso
                    {
                        IdNivel = n.IdNivel,
                        // Incluir otras propiedades o relaciones si es necesario
                    })
                    .ToList();
            }
            return _nivelesAccesoRepo.GetAll();
        }

        // Obtener un nivel de acceso por ID
        public NivelesAcceso GetById(int id)
        {
            var nivel = _nivelesAccesoRepo.GetById(id);
            if (nivel == null)
                throw new KeyNotFoundException($"No se encontró el nivel de acceso con ID {id}");
            return nivel;
        }

        // Crear un nuevo nivel de acceso
        public void Create(NivelesAcceso entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (entity.Nivel != 0)
                throw new ArgumentException("El nivel debe ser diferente a 0.");
            if (string.IsNullOrWhiteSpace(entity.Nombre))
                throw new ArgumentException("El nombre del nivel es obligatoria.");
            var existing = _context.NivelesAccesos.FirstOrDefault(n => n.Nombre == entity.Nombre);
            if (existing != null)
                throw new InvalidOperationException("Ya existe un nivel de acceso con ese nombre.");
            _nivelesAccesoRepo.Add(entity);
        }

        // Actualizar un nivel de acceso existente
        public void Update(NivelesAcceso entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            var existing = _nivelesAccesoRepo.GetById(entity.IdNivel);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró el nivel de acceso con ID {entity.IdNivel}");

            var duplicate = _context.NivelesAccesos.FirstOrDefault(n => n.Nombre == entity.Nombre && n.IdNivel != entity.IdNivel);
            if (duplicate != null)
                throw new InvalidOperationException("Ya existe otro nivel de acceso con ese nombre.");
            _nivelesAccesoRepo.Update(entity);
        }

        // Eliminar un nivel de acceso por ID
        public void Delete(int id)
        {
            var existing = _nivelesAccesoRepo.GetById(id);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró el nivel de acceso con ID {id}");
            _nivelesAccesoRepo.Delete(id);
        }
    }
}
