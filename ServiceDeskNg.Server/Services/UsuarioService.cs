using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories;

namespace ServiceDeskNg.Server.Services
{
    public class UsuarioService
    {
        private readonly UsuarioRepository _usuarioRepo;
        private readonly ServiceDeskContext _context;

        public UsuarioService(UsuarioRepository usuarioRepo, ServiceDeskContext context)
        {
            _usuarioRepo = usuarioRepo;
            _context = context;
        }

        // ✅ Obtener todos los usuarios
        public IEnumerable<Usuario> GetAll(bool includeRelations = false)
        {
            if (includeRelations)
            {
                return _context.Usuarios
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
                        Supervisores = u.Supervisores
                    })
                    .ToList();
            }

            return _usuarioRepo.GetAll();
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

            // Si quisieras hash de contraseñas, aquí podrías integrarlo:
            // entity.ContrasenaUsuario = BCrypt.Net.BCrypt.HashPassword(entity.ContrasenaUsuario);

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

            var usuario = _context.Usuarios.FirstOrDefault(u => u.CorreoUsuario == correo);

            if (usuario == null)
                throw new UnauthorizedAccessException("Usuario no encontrado.");

            // Si activas hash de contraseña, cambiar a:
            // if (!BCrypt.Net.BCrypt.Verify(contrasena, usuario.ContrasenaUsuario))
            if (usuario.ContrasenaUsuario != contrasena)
                throw new UnauthorizedAccessException("Contraseña incorrecta.");

            if (usuario.EstadoUsuario?.ToLower() != "activo")
                throw new UnauthorizedAccessException("La cuenta del usuario está inactiva.");

            return usuario;
        }
    }
}
