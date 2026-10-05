using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Services
{
    /// Sesiones persistidas en la tabla `sesiones`.
    /// La cookie de autenticación lleva el id de sesión, y cada petición comprueba
    /// contra esta tabla que la sesión siga abierta: así un "cerrar sesión" invalida
    /// la credencial del lado del servidor y no solo en el navegador.
    public class SesionService
    {
        private readonly IRepositorio<Sesion> _sesiones;
        private readonly IRepositorio<Usuario> _usuarios;

        public SesionService(IRepositorio<Sesion> sesiones, IRepositorio<Usuario> usuarios)
        {
            _sesiones = sesiones;
            _usuarios = usuarios;
        }

        public async Task<Sesion> AbrirAsync(int idUsuario, CancellationToken ct = default)
        {
            var sesion = new Sesion
            {
                IdUsuario = idUsuario,
                FechaHoraInicioSesion = DateTime.UtcNow,
                SesionActiva = true
            };

            await _sesiones.AddAsync(sesion, ct);
            return sesion;
        }

        public async Task CerrarAsync(int idSesion, CancellationToken ct = default)
        {
            var sesion = await _sesiones.GetByIdAsync(idSesion, ct)
                ?? throw new KeyNotFoundException($"No se encontró la sesión con ID {idSesion}");

            if (sesion.SesionActiva == false)
                return;

            sesion.SesionActiva = false;
            sesion.FechaHoraFinSesion = DateTime.UtcNow;
            await _sesiones.UpdateAsync(sesion, ct);
        }

        /// Cierra todas las sesiones abiertas de un usuario (al desactivarlo o eliminarlo).
        public async Task CerrarTodasAsync(int idUsuario, CancellationToken ct = default)
        {
            var abiertas = await _sesiones.QueryParaEscritura()
                .Where(s => s.IdUsuario == idUsuario && s.SesionActiva == true)
                .ToListAsync(ct);

            if (abiertas.Count == 0)
                return;

            foreach (var sesion in abiertas)
            {
                sesion.SesionActiva = false;
                sesion.FechaHoraFinSesion = DateTime.UtcNow;
            }

            await _sesiones.GuardarCambiosAsync(ct);
        }

        /// La sesión vale si sigue abierta, pertenece al usuario indicado
        /// y la cuenta de ese usuario continúa activa.
        public async Task<bool> EsValidaAsync(int idSesion, int idUsuario, CancellationToken ct = default)
        {
            var sesionAbierta = await _sesiones.Query()
                .AnyAsync(s => s.IdSesion == idSesion
                               && s.IdUsuario == idUsuario
                               && s.SesionActiva == true, ct);

            if (!sesionAbierta)
                return false;

            return await _usuarios.Query()
                .AnyAsync(u => u.IdUsuario == idUsuario && u.EstadoUsuario == "activo", ct);
        }

        public Task<bool> TieneSesionActivaAsync(int idUsuario, CancellationToken ct = default) =>
            _sesiones.Query().AnyAsync(s => s.IdUsuario == idUsuario && s.SesionActiva == true, ct);
    }
}
