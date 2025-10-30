using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories;

namespace ServiceDeskNg.Server.Services
{
    public class UsuarioService
    {
        private readonly UsuarioRepository _usuarioRepo;
        private readonly ServiceDeskContext _context;
        private readonly SesionRepository _sesionRepo;
        private readonly EndUserRepository _endUserRepo;

        public UsuarioService(UsuarioRepository usuarioRepo, ServiceDeskContext context, SesionRepository sesionRepo, EndUserRepository endUserRepo)
        {
            _usuarioRepo = usuarioRepo;
            _context = context;
            _sesionRepo = sesionRepo;
            _endUserRepo = endUserRepo;
        }

        // ✅ Obtener todos los usuarios
        public IEnumerable<Usuario> GetAll(bool includeRelations = false)
        {
            if (includeRelations)
            {
                return _context.Usuarios
                    .Include(u => u.Administradores)
                    .Include(u => u.Supervisores)
                    .Include(u => u.Agentes)
                    .Include(u => u.Clientes) // Asegura que siempre se incluye la relación Clientes
                    .Select(u => new Usuario
                    {
                        IdUsuario = u.IdUsuario,
                        NombreUsuario = u.NombreUsuario,
                        CorreoUsuario = u.CorreoUsuario,
                        EstadoUsuario = u.EstadoUsuario,
                        DepartamentoUsuario = u.DepartamentoUsuario,
                        UbicacionUsuario = u.UbicacionUsuario,
                        FechaHoraCreacionUsuario = u.FechaHoraCreacionUsuario,
                        Administradores = u.Administradores,
                        Agentes = u.Agentes,
                        Clientes = u.Clientes,
                        Supervisores = u.Supervisores,
                        Rol = u.Administradores.Any() ? "Administrador" :
                              u.Supervisores.Any() ? "Supervisor" :
                              u.Agentes.Any() ? "Agente" :
                              u.Clientes.Any() ? "EndUser" : "Sin Rol"
                    })
                    .ToList();
            }
            var usuarios = _usuarioRepo.GetAll().Select(u => {
                u.Rol = u.Administradores.Any() ? "Administrador" :
                        u.Supervisores.Any() ? "Supervisor" :
                        u.Agentes.Any() ? "Agente" :
                        u.Clientes.Any() ? "EndUser" : "Sin Rol";
                return u;
            }).ToList();
            return usuarios;
        }

        // ✅ Obtener un usuario por ID
        public Usuario GetById(int id)
        {
            var usuario = _usuarioRepo.GetById(id);
            if (usuario == null)
                throw new KeyNotFoundException($"No se encontró el usuario con ID {id}");
            return usuario;
        }

        // ✅ Crear un nuevo usuario
        public void Create(Usuario entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(entity.NombreUsuario))
                throw new ArgumentException("El nombre del usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(entity.CorreoUsuario))
                throw new ArgumentException("El correo del usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(entity.ContrasenaUsuario))
                throw new ArgumentException("La contraseña es obligatoria.");

            // Verificar que no exista correo duplicado
            var existing = _context.Usuarios.FirstOrDefault(u => u.CorreoUsuario == entity.CorreoUsuario);
            if (existing != null)
                throw new InvalidOperationException("Ya existe un usuario con ese correo electrónico.");

            // Estado por defecto
            entity.EstadoUsuario ??= "activo";
            entity.FechaHoraCreacionUsuario = DateTime.UtcNow;

            entity.ContrasenaUsuario = BCrypt.Net.BCrypt.HashPassword(entity.ContrasenaUsuario);

            _usuarioRepo.Add(entity);
        }

        // ✅ Actualizar un usuario existente
        public void Update(Usuario entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var existing = _usuarioRepo.GetById(entity.IdUsuario);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró el usuario con ID {entity.IdUsuario}");

            if (string.IsNullOrWhiteSpace(entity.NombreUsuario))
                throw new ArgumentException("El nombre del usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(entity.CorreoUsuario))
                throw new ArgumentException("El correo del usuario es obligatorio.");

            // Verificar duplicados (correo)
            var duplicate = _context.Usuarios
                .FirstOrDefault(u => u.CorreoUsuario == entity.CorreoUsuario && u.IdUsuario != entity.IdUsuario);

            if (duplicate != null)
                throw new InvalidOperationException("Ya existe otro usuario con ese correo electrónico.");

            // Mantener datos no actualizables
            entity.FechaHoraCreacionUsuario = existing.FechaHoraCreacionUsuario;

            _usuarioRepo.Update(entity);
        }

        // ✅ Eliminar un usuario por ID
        public void Delete(int id)
        {
            var existing = _usuarioRepo.GetById(id);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró el usuario con ID {id}");

            _usuarioRepo.Delete(id);
        }

        // ✅ Autenticar usuario (login básico, preparado para futuro)
        public Usuario Authenticate(string correo, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
                throw new ArgumentException("El correo y la contraseña son obligatorios.");

            var usuario = _context.Usuarios
                .Include(u => u.Administradores)
                .Include(u => u.Supervisores)
                .Include(u => u.Agentes)
                .Include(u => u.Clientes)
                .FirstOrDefault(u => u.CorreoUsuario == correo);

            if (usuario == null)
                throw new UnauthorizedAccessException("Usuario no encontrado.");

            if (!BCrypt.Net.BCrypt.Verify(contrasena, usuario.ContrasenaUsuario))
                throw new UnauthorizedAccessException("Contraseña incorrecta.");

            if (usuario.EstadoUsuario?.ToLower() != "activo")
                throw new UnauthorizedAccessException("La cuenta del usuario está inactiva.");

            return usuario;
        }


        public void Logout(Sesion entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            var existing = _sesionRepo.GetById(entity.IdSesion);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró la sesión con ID {entity.IdSesion}");
            existing.FechaHoraFinSesion = DateTime.UtcNow;
            existing.SesionActiva = false;
            _sesionRepo.Update(existing);

        }

        public bool IsUserSessionActive(int userId)
        {
            return _context.Sesiones.Any(s => s.IdUsuario == userId && s.SesionActiva == true);

        }

        // Nuevo: Crear EndUser desde UsuarioService
        public void CrearEndUser(EndUser endUser)
        {
            if (endUser == null)
                throw new ArgumentNullException(nameof(endUser));
            _endUserRepo.Add(endUser);
            // Validación extra: lanzar excepción si no se insertó
            var existe = _endUserRepo.GetAll().Any(e => e.IdUsuario == endUser.IdUsuario);
            if (!existe)
                throw new Exception($"No se pudo crear el registro EndUser para el usuario {endUser.IdUsuario}");
        }

        // Nuevo: Crear Agente desde UsuarioService
        public void CrearAgente(Agente agente)
        {
            if (agente == null)
                throw new ArgumentNullException(nameof(agente));
            _context.Agentes.Add(agente);
            _context.SaveChanges();
        }

        // Nuevo: Crear Supervisor desde UsuarioService
        public void CrearSupervisor(Supervisor supervisor)
        {
            if (supervisor == null)
                throw new ArgumentNullException(nameof(supervisor));
            _context.Supervisores.Add(supervisor);
            _context.SaveChanges();
        }

        // Nuevo: Obtener nivel de acceso por nombre
        public NivelesAcceso? GetNivelAccesoPorNombre(string nombre)
        {
            return _context.NivelesAccesos.FirstOrDefault(n => n.Nombre == nombre);
        }

        // Nuevo: Obtener todos los usuarios como DTOs
        public IEnumerable<UsuarioDto> GetAllUsuariosDto()
        {
            var usuarios = _context.Usuarios
                .Include(u => u.Administradores)
                .Include(u => u.Supervisores)
                .Include(u => u.Agentes)
                .Include(u => u.Clientes)
                .ToList();

            return usuarios.Select(u => new UsuarioDto
            {
                IdUsuario = u.IdUsuario,
                NombreUsuario = u.NombreUsuario,
                CorreoUsuario = u.CorreoUsuario,
                EstadoUsuario = u.EstadoUsuario,
                DepartamentoUsuario = u.DepartamentoUsuario,
                UbicacionUsuario = u.UbicacionUsuario,
                FechaHoraCreacionUsuario = u.FechaHoraCreacionUsuario,
                TipoUsuario = u.Administradores.Any() ? "Administrador" :
                              u.Supervisores.Any() ? "Supervisor" :
                              u.Agentes.Any() ? "Agente" :
                              u.Clientes.Any() ? "Cliente" : "Sin Rol"
            });
        }

        public Usuario? GetByCorreo(string correo)
        {
            return _context.Usuarios.FirstOrDefault(u => u.CorreoUsuario == correo);
        }

        // Nuevo: Crear Administrador desde UsuarioService
        public void CrearAdministrador(Administrador admin)
        {
            if (admin == null)
                throw new ArgumentNullException(nameof(admin));
            _context.Administradores.Add(admin);
            _context.SaveChanges();
            var existe = _context.Administradores.Any(a => a.IdUsuario == admin.IdUsuario);
            if (!existe)
                throw new Exception($"No se pudo crear el registro Administrador para el usuario {admin.IdUsuario}");
        }
    }
}
