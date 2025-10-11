using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Repositories;

namespace ServiceDeskNg.Server.Services
{
    public class TicketArchivoService
    {

        private readonly TicketArchivoRepository _ticketArchivoRepo;
        private readonly ServiceDeskContext _context;
        public TicketArchivoService(TicketArchivoRepository ticketArchivoRepo, ServiceDeskContext context)
        {
            _ticketArchivoRepo = ticketArchivoRepo;
            _context = context;

        }
        // Obtener todos los archivos de tickets
        public IEnumerable<Models.TicketArchivo> GetAll()
        {
            return _ticketArchivoRepo.GetAll();
        }


        // Obtener un archivo de ticket por ID
        public Models.TicketArchivo GetById(int id)
        {
            var archivo = _ticketArchivoRepo.GetById(id);
            if (archivo == null)
                throw new KeyNotFoundException($"No se encontró el archivo de ticket con ID {id}");
            return archivo;
        }

        // Crear un nuevo archivo de ticket
        public void Create(Models.TicketArchivo entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (entity.IdTicket == 0)
                throw new ArgumentException("Debe asociarse a un ticket válido.");
            if (string.IsNullOrWhiteSpace(entity.RutaArchivoTicket))
                throw new ArgumentException("La ruta del archivo es obligatoria.");
            _ticketArchivoRepo.Add(entity);
        }

        //eliminar un archivo de ticket por ID
        public void Delete(int id)
        {
            var archivo = _ticketArchivoRepo.GetById(id);
            if (archivo == null)
                throw new KeyNotFoundException($"No se encontró el archivo de ticket con ID {id}");
            _ticketArchivoRepo.Delete(id);
        }

    }
}
