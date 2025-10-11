using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories;

namespace ServiceDeskNg.Server.Services
{
    public class IntegracionService
    {


        private readonly IntegracionRepository _integracionRepo;
        private readonly ServiceDeskContext _context;
        // Implementar métodos y lógica de negocio para Integracion
        public IntegracionService(IntegracionRepository integracioRepo, ServiceDeskContext context) 
        {
            _integracionRepo = integracioRepo;
            _context = context;
        }

        // Obtener todas las integraciones
        public IEnumerable<Integracion> GetAll()
        {
            return _integracionRepo.GetAll();
        }
        // Obtener una integración por ID
        public Integracion GetById(int id)
        {
            var integracion = _integracionRepo.GetById(id);
            if (integracion == null)
                throw new KeyNotFoundException($"No se encontró la integración con ID {id}");
            return integracion;
        }

        // Eliminar una integración por ID
        public void Delete(int id)
        {
            var integracion = _integracionRepo.GetById(id);
            if (integracion == null)
                throw new KeyNotFoundException($"No se encontró la integración con ID {id}");
            _integracionRepo.Delete(id);
        }
    }
}
