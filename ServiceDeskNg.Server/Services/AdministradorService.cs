using System.Collections.Generic;
using System.Linq;
using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories;

namespace ServiceDeskNg.Server.Services
{
    public class AdministradorService
    {
        private readonly AdministradorRepository _adminRepo;
        private readonly ServiceDeskContext _context;

        public AdministradorService(AdministradorRepository adminRepo, ServiceDeskContext context)
        {
            _adminRepo = adminRepo;
            _context = context;
        }

        // ✅ Obtener todos los administradores (con relaciones opcionales)
        public IEnumerable<Administrador> GetAll(bool includeRelations = false)
        {
            if (includeRelations)
            {
                // Si quieres incluir las relaciones (Usuario y Nivel)
                return _context.Administradores
                    .Select(a => new Administrador
                    {
                        IdAdmin = a.IdAdmin,
                        AreaResponsabilidadAdmin = a.AreaResponsabilidadAdmin,
                        IdUsuario = a.IdUsuario,
                        IdNivel = a.IdNivel,
                        IdUsuarioNavigation = a.IdUsuarioNavigation,
                        IdNivelNavigation = a.IdNivelNavigation
                    })
                    .ToList();
            }

            return _adminRepo.GetAll();
        }

        // ✅ Obtener un administrador por ID
        public Administrador GetById(int id)
        {
            var admin = _adminRepo.GetById(id);
            if (admin == null)
                throw new KeyNotFoundException($"No se encontró el administrador con ID {id}");

            return admin;
        }

        // ✅ Crear un nuevo administrador
        public void Create(Administrador entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.IdUsuario == 0)
                throw new ArgumentException("Debe asociarse un usuario válido.");

            if (string.IsNullOrWhiteSpace(entity.AreaResponsabilidadAdmin))
                throw new ArgumentException("El área de responsabilidad es obligatoria.");

            // Verificar si ya existe un admin con el mismo usuario
            var existing = _context.Administradores.FirstOrDefault(a => a.IdUsuario == entity.IdUsuario);
            if (existing != null)
                throw new InvalidOperationException("Ya existe un administrador vinculado a ese usuario.");

            _adminRepo.Add(entity);
        }

        // ✅ Actualizar un administrador existente
        public void Update(Administrador entity)
        {
            var existing = _adminRepo.GetById(entity.IdAdmin);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró el administrador con ID {entity.IdAdmin}");

            // Reglas de negocio básicas
            if (string.IsNullOrWhiteSpace(entity.AreaResponsabilidadAdmin))
                throw new ArgumentException("El área de responsabilidad no puede estar vacía.");

            _adminRepo.Update(entity);
        }

        // ✅ Eliminar un administrador
        public void Delete(int id)
        {
            var admin = _adminRepo.GetById(id);
            if (admin == null)
                throw new KeyNotFoundException($"No existe el administrador con ID {id}");

            _adminRepo.Delete(id);
        }
    }
}
