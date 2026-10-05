using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Repositories.Interfaces;
using ServiceDeskNg.Server.Security;

namespace ServiceDeskNg.Server.Services
{
    /// Resultado de dar de baja a un usuario: se elimina si no tiene historial,
    /// y si lo tiene se desactiva para no romper la trazabilidad de tickets y auditoría.
    public sealed record ResultadoBaja(bool Eliminado, string Mensaje);

    public class UsuarioService
    {
        private readonly ServiceDeskContext _context;
        private readonly IRepositorio<Usuario> _usuarios;
        private readonly IRepositorio<NivelesAcceso> _niveles;
        private readonly SesionService _sesiones;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(
            ServiceDeskContext context,
            IRepositorio<Usuario> usuarios,
            IRepositorio<NivelesAcceso> niveles,
            SesionService sesiones,
            ILogger<UsuarioService> logger)
        {
            _context = context;
            _usuarios = usuarios;
            _niveles = niveles;
            _sesiones = sesiones;
            _logger = logger;
        }

        // ======================================================
        // Consultas
        // ======================================================

        public Task<List<UsuarioDto>> ListarAsync(CancellationToken ct = default) =>
            _usuarios.Query()
                .OrderBy(u => u.NombreUsuario)
                .Select(ProyeccionDto)
                .ToListAsync(ct);

        public async Task<UsuarioDto> ObtenerDtoAsync(int id, CancellationToken ct = default)
        {
            var usuario = await _usuarios.Query()
                .Where(u => u.IdUsuario == id)
                .Select(ProyeccionDto)
                .FirstOrDefaultAsync(ct);

            return usuario
                ?? throw new KeyNotFoundException($"No se encontró el usuario con ID {id}");
        }

        /// Carga el usuario con sus roles. Se usa en el login y para resolver permisos.
        public async Task<IdentidadUsuario> ObtenerIdentidadAsync(int id, CancellationToken ct = default)
        {
            var usuario = await ConsultaConRoles()
                .FirstOrDefaultAsync(u => u.IdUsuario == id, ct);

            return usuario is null
                ? throw new KeyNotFoundException($"No se encontró el usuario con ID {id}")
                : ResolucionRol.Resolver(usuario);
        }

        // ======================================================
        // Autenticación
        // ======================================================

        /// Verifica credenciales. El mensaje de error es siempre el mismo para
        /// correo inexistente y contraseña incorrecta: así la API no permite
        /// averiguar qué correos están registrados.
        public async Task<IdentidadUsuario> AutenticarAsync(
            string correo,
            string contrasena,
            CancellationToken ct = default)
        {
            const string credencialesInvalidas = "Correo o contraseña incorrectos.";

            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
                throw new ArgumentException("El correo y la contraseña son obligatorios.");

            var usuario = await ConsultaConRoles()
                .FirstOrDefaultAsync(u => u.CorreoUsuario == correo, ct);

            if (usuario is null)
            {
                // Se verifica un hash ficticio para que la respuesta tarde lo mismo
                // que con un correo existente (evita distinguirlos por tiempos).
                BCrypt.Net.BCrypt.Verify(contrasena, HashSeñuelo);
                throw new UnauthorizedAccessException(credencialesInvalidas);
            }

            if (!VerificarContrasena(contrasena, usuario.ContrasenaUsuario))
                throw new UnauthorizedAccessException(credencialesInvalidas);

            if (!string.Equals(usuario.EstadoUsuario, "activo", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("La cuenta del usuario está inactiva.");

            var identidad = ResolucionRol.Resolver(usuario);

            if (identidad.Rol == ResolucionRol.SinRol)
                throw new UnauthorizedAccessException(
                    "La cuenta no tiene un rol asignado. Contacte al administrador.");

            return identidad;
        }

        /// Para auditar un intento fallido hace falta el id del usuario, si existe.
        public Task<int?> BuscarIdPorCorreoAsync(string correo, CancellationToken ct = default) =>
            _usuarios.Query()
                .Where(u => u.CorreoUsuario == correo)
                .Select(u => (int?)u.IdUsuario)
                .FirstOrDefaultAsync(ct);

        // ======================================================
        // Altas, cambios y bajas
        // ======================================================

        /// Crea el usuario y su fila de rol en una sola transacción:
        /// antes eran dos SaveChanges independientes y un fallo en el segundo
        /// dejaba usuarios huérfanos sin rol (y por tanto sin poder entrar).
        public async Task<UsuarioDto> CrearConRolAsync(UsuarioCreateDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var rol = NormalizarRol(dto.TipoUsuario);
            var correo = dto.CorreoUsuario.Trim();

            if (await _usuarios.Query().AnyAsync(u => u.CorreoUsuario == correo, ct))
                throw new ConflictoNegocioException("Ya existe un usuario con ese correo electrónico.");

            var numeroNivel = RolesApp.NivelDeRol(rol);
            var nivel = await _niveles.Query().FirstOrDefaultAsync(n => n.Nivel == numeroNivel, ct)
                ?? throw new ConflictoNegocioException(
                    $"No existe el nivel de acceso {numeroNivel} para el rol {rol}. Cargue los datos iniciales de la base de datos.");

            var hash = BCrypt.Net.BCrypt.HashPassword(dto.ContrasenaUsuario);

            // Con reintentos activados, EF exige que la transacción completa se ejecute
            // dentro de la estrategia, para poder repetirla entera si la conexión se corta.
            var usuario = await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();
                await using var transaccion = await _context.Database.BeginTransactionAsync(ct);

                var nuevo = new Usuario
                {
                    NombreUsuario = dto.NombreUsuario.Trim(),
                    CorreoUsuario = correo,
                    ContrasenaUsuario = hash,
                    DepartamentoUsuario = dto.DepartamentoUsuario,
                    UbicacionUsuario = dto.UbicacionUsuario,
                    EstadoUsuario = string.IsNullOrWhiteSpace(dto.EstadoUsuario) ? "activo" : dto.EstadoUsuario,
                    FechaHoraCreacionUsuario = DateTime.UtcNow
                };

                _context.Usuarios.Add(nuevo);
                await _context.SaveChangesAsync(ct);

                AgregarFilaDeRol(rol, nuevo.IdUsuario, nivel.IdNivel);
                await _context.SaveChangesAsync(ct);

                await transaccion.CommitAsync(ct);
                return nuevo;
            });

            _logger.LogInformation(
                "Usuario {IdUsuario} creado con rol {Rol}", usuario.IdUsuario, rol);

            return new UsuarioDto
            {
                IdUsuario = usuario.IdUsuario,
                NombreUsuario = usuario.NombreUsuario,
                CorreoUsuario = usuario.CorreoUsuario,
                EstadoUsuario = usuario.EstadoUsuario,
                DepartamentoUsuario = usuario.DepartamentoUsuario,
                UbicacionUsuario = usuario.UbicacionUsuario,
                FechaHoraCreacionUsuario = usuario.FechaHoraCreacionUsuario,
                TipoUsuario = rol
            };
        }

        /// `puedeAdministrar` distingue al administrador del usuario editando su propio perfil:
        /// este último no puede cambiar el estado de la cuenta.
        public async Task ActualizarAsync(
            int id,
            UsuarioUpdateDto dto,
            bool puedeAdministrar,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var usuario = await _usuarios.QueryParaEscritura()
                .FirstOrDefaultAsync(u => u.IdUsuario == id, ct)
                ?? throw new KeyNotFoundException($"No se encontró el usuario con ID {id}");

            var correo = dto.CorreoUsuario.Trim();

            if (await _usuarios.Query().AnyAsync(u => u.CorreoUsuario == correo && u.IdUsuario != id, ct))
                throw new ConflictoNegocioException("Ya existe otro usuario con ese correo electrónico.");

            usuario.NombreUsuario = dto.NombreUsuario.Trim();
            usuario.CorreoUsuario = correo;
            usuario.DepartamentoUsuario = dto.DepartamentoUsuario;
            usuario.UbicacionUsuario = dto.UbicacionUsuario;

            if (!string.IsNullOrWhiteSpace(dto.ContrasenaUsuario))
                usuario.ContrasenaUsuario = BCrypt.Net.BCrypt.HashPassword(dto.ContrasenaUsuario);

            if (puedeAdministrar && !string.IsNullOrWhiteSpace(dto.EstadoUsuario))
            {
                var nuevoEstado = dto.EstadoUsuario.ToLowerInvariant();

                if (nuevoEstado == "inactivo")
                {
                    await AsegurarQueNoEsElUltimoAdministradorAsync(id, ct);
                    await _sesiones.CerrarTodasAsync(id, ct);
                }

                usuario.EstadoUsuario = nuevoEstado;
            }

            await _usuarios.GuardarCambiosAsync(ct);
        }

        /// Da de baja al usuario. Si tiene historial (tickets, mensajes o auditoría)
        /// se desactiva en lugar de borrarse: eliminarlo rompería esas referencias.
        public async Task<ResultadoBaja> DarDeBajaAsync(
            int id,
            int idUsuarioQueEjecuta,
            CancellationToken ct = default)
        {
            if (id == idUsuarioQueEjecuta)
                throw new ConflictoNegocioException("No puede eliminar su propia cuenta.");

            var usuario = await ConsultaConRoles().FirstOrDefaultAsync(u => u.IdUsuario == id, ct)
                ?? throw new KeyNotFoundException($"No se encontró el usuario con ID {id}");

            await AsegurarQueNoEsElUltimoAdministradorAsync(id, ct);

            var identidad = ResolucionRol.Resolver(usuario);
            var tieneHistorial = await TieneHistorialAsync(identidad, ct);

            await _sesiones.CerrarTodasAsync(id, ct);

            if (tieneHistorial)
            {
                var seguimiento = await _usuarios.QueryParaEscritura()
                    .FirstAsync(u => u.IdUsuario == id, ct);
                seguimiento.EstadoUsuario = "inactivo";
                await _usuarios.GuardarCambiosAsync(ct);

                _logger.LogInformation("Usuario {IdUsuario} desactivado (tenía historial)", id);

                return new ResultadoBaja(
                    false,
                    "El usuario tiene tickets o actividad registrada, así que se desactivó en lugar de eliminarse.");
            }

            await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();
                await using var transaccion = await _context.Database.BeginTransactionAsync(ct);

                await EliminarFilasDeRolAsync(id, ct);
                _context.Sesiones.RemoveRange(
                    await _context.Sesiones.Where(s => s.IdUsuario == id).ToListAsync(ct));
                await _context.SaveChangesAsync(ct);

                var entidad = await _context.Usuarios.FirstAsync(u => u.IdUsuario == id, ct);
                _context.Usuarios.Remove(entidad);
                await _context.SaveChangesAsync(ct);

                await transaccion.CommitAsync(ct);
            });

            _logger.LogInformation("Usuario {IdUsuario} eliminado", id);
            return new ResultadoBaja(true, "Usuario eliminado correctamente.");
        }

        // ======================================================
        // Apoyo
        // ======================================================

        /// Hash BCrypt de una cadena arbitraria, usado solo para igualar tiempos de respuesta.
        private const string HashSeñuelo = "$2a$11$p3Y1vJ0wG5cQ0bqgQ1oHSuDqkX3Qe0iUqWr0hQ8s6o1C1Lq4Qm7Dm";

        private static bool VerificarContrasena(string contrasena, string? hashGuardado)
        {
            if (string.IsNullOrWhiteSpace(hashGuardado))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(contrasena, hashGuardado);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Contraseña guardada en texto plano o con otro formato: no es válida.
                return false;
            }
        }

        private IQueryable<Usuario> ConsultaConRoles() =>
            _usuarios.Query()
                .Include(u => u.Administradores)
                .Include(u => u.Supervisores)
                .Include(u => u.Agentes)
                .Include(u => u.Clientes);

        private static string NormalizarRol(string? tipoUsuario)
        {
            var rol = (tipoUsuario ?? string.Empty).Trim();

            // El frontend ha usado históricamente "EndUser" y "Cliente" para lo mismo.
            if (rol.Equals("EndUser", StringComparison.OrdinalIgnoreCase))
                rol = RolesApp.Cliente;

            rol = rol.ToLowerInvariant() switch
            {
                "administrador" => RolesApp.Administrador,
                "supervisor" => RolesApp.Supervisor,
                "agente" => RolesApp.Agente,
                "cliente" => RolesApp.Cliente,
                _ => rol
            };

            return RolesApp.EsRolValido(rol)
                ? rol
                : throw new ArgumentException(
                    "El tipo de usuario debe ser Administrador, Supervisor, Agente o Cliente.");
        }

        private void AgregarFilaDeRol(string rol, int idUsuario, int idNivel)
        {
            switch (rol)
            {
                case RolesApp.Administrador:
                    _context.Administradores.Add(new Administrador { IdUsuario = idUsuario, IdNivel = idNivel });
                    break;
                case RolesApp.Supervisor:
                    _context.Supervisores.Add(new Supervisor { IdUsuario = idUsuario, IdNivel = idNivel });
                    break;
                case RolesApp.Agente:
                    _context.Agentes.Add(new Agente
                    {
                        IdUsuario = idUsuario,
                        IdNivel = idNivel,
                        DisponibilidadAgente = true
                    });
                    break;
                case RolesApp.Cliente:
                    _context.Clientes.Add(new EndUser { IdUsuario = idUsuario, IdNivel = idNivel });
                    break;
                default:
                    throw new ArgumentException($"Rol no soportado: {rol}");
            }
        }

        private async Task EliminarFilasDeRolAsync(int idUsuario, CancellationToken ct)
        {
            _context.Administradores.RemoveRange(
                await _context.Administradores.Where(a => a.IdUsuario == idUsuario).ToListAsync(ct));
            _context.Supervisores.RemoveRange(
                await _context.Supervisores.Where(s => s.IdUsuario == idUsuario).ToListAsync(ct));
            _context.Agentes.RemoveRange(
                await _context.Agentes.Where(a => a.IdUsuario == idUsuario).ToListAsync(ct));
            _context.Clientes.RemoveRange(
                await _context.Clientes.Where(c => c.IdUsuario == idUsuario).ToListAsync(ct));
        }

        private async Task<bool> TieneHistorialAsync(IdentidadUsuario identidad, CancellationToken ct)
        {
            var idUsuario = identidad.Usuario.IdUsuario;

            if (await _context.Auditoria.AnyAsync(a => a.IdUsuario == idUsuario, ct))
                return true;

            if (await _context.TicketMensajes.AnyAsync(m => m.IdUsuario == idUsuario, ct))
                return true;

            if (await _context.TicketArchivos.AnyAsync(a => a.IdUsuario == idUsuario, ct))
                return true;

            if (identidad.IdCliente is int idCliente
                && await _context.Tickets.AnyAsync(t => t.IdCliente == idCliente, ct))
                return true;

            if (identidad.IdAgente is int idAgente
                && await _context.Tickets.AnyAsync(t => t.IdAgenteAsignado == idAgente, ct))
                return true;

            return false;
        }

        /// Impide quedarse sin ningún administrador activo con el que entrar al sistema.
        private async Task AsegurarQueNoEsElUltimoAdministradorAsync(int idUsuario, CancellationToken ct)
        {
            var esAdministrador = await _context.Administradores
                .AnyAsync(a => a.IdUsuario == idUsuario, ct);

            if (!esAdministrador)
                return;

            var otrosAdministradoresActivos = await _context.Administradores
                .Where(a => a.IdUsuario != idUsuario)
                .Join(_context.Usuarios, a => a.IdUsuario, u => u.IdUsuario, (a, u) => u)
                .CountAsync(u => u.EstadoUsuario == "activo", ct);

            if (otrosAdministradoresActivos == 0)
                throw new ConflictoNegocioException(
                    "No se puede dejar el sistema sin administradores activos.");
        }

        /// Proyección compartida: garantiza que la contraseña nunca salga en una respuesta.
        private static readonly System.Linq.Expressions.Expression<Func<Usuario, UsuarioDto>> ProyeccionDto =
            u => new UsuarioDto
            {
                IdUsuario = u.IdUsuario,
                NombreUsuario = u.NombreUsuario,
                CorreoUsuario = u.CorreoUsuario,
                EstadoUsuario = u.EstadoUsuario,
                DepartamentoUsuario = u.DepartamentoUsuario,
                UbicacionUsuario = u.UbicacionUsuario,
                FechaHoraCreacionUsuario = u.FechaHoraCreacionUsuario,
                TipoUsuario =
                    u.Administradores.Any() ? RolesApp.Administrador :
                    u.Supervisores.Any() ? RolesApp.Supervisor :
                    u.Agentes.Any() ? RolesApp.Agente :
                    u.Clientes.Any() ? RolesApp.Cliente :
                    ResolucionRol.SinRol
            };
    }
}
