using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Services
{
    /// Acciones que se registran en el log de auditoría.
    public static class AccionesAuditoria
    {
        public const string Login = "LOGIN";
        public const string LoginFallido = "LOGIN_FALLIDO";
        public const string Logout = "LOGOUT";
        public const string UsuarioCreado = "USUARIO_CREADO";
        public const string UsuarioActualizado = "USUARIO_ACTUALIZADO";
        public const string UsuarioEliminado = "USUARIO_ELIMINADO";
        public const string TicketCreado = "TICKET_CREADO";
        public const string TicketActualizado = "TICKET_ACTUALIZADO";
        public const string TicketEliminado = "TICKET_ELIMINADO";
        public const string TicketAsignado = "TICKET_ASIGNADO";
        public const string TicketEscalado = "TICKET_ESCALADO";
        public const string TicketsRedistribuidos = "TICKETS_REDISTRIBUIDOS";
        public const string RespaldoDescargado = "RESPALDO_DESCARGADO";
        public const string RespaldoRestaurado = "RESPALDO_RESTAURADO";
    }

    /// Registro de acciones sensibles. Es la bitácora que consulta el panel de
    /// administración y la que permite saber quién hizo qué y cuándo.
    public class AuditoriaService
    {
        private readonly IRepositorio<Auditoria> _auditorias;
        private readonly ILogger<AuditoriaService> _logger;

        public AuditoriaService(IRepositorio<Auditoria> auditorias, ILogger<AuditoriaService> logger)
        {
            _auditorias = auditorias;
            _logger = logger;
        }

        /// Registra una acción. Un fallo al escribir la bitácora se registra en el log
        /// del servidor pero no interrumpe la operación del usuario.
        public async Task RegistrarAsync(
            int idUsuario,
            string accion,
            string? detalle = null,
            CancellationToken ct = default)
        {
            try
            {
                await _auditorias.AddAsync(
                    new Auditoria
                    {
                        IdUsuario = idUsuario,
                        AccionAuditoria = accion,
                        DetalleAuditoria = detalle,
                        FechaAuditoria = DateTime.UtcNow
                    },
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "No se pudo registrar la auditoría {Accion} del usuario {IdUsuario}",
                    accion,
                    idUsuario);
            }
        }

        /// Últimos registros, del más reciente al más antiguo.
        public async Task<List<AuditoriaDto>> ListarAsync(
            int limite = 200,
            string? accion = null,
            CancellationToken ct = default)
        {
            limite = Math.Clamp(limite, 1, 1000);

            var consulta = _auditorias.Query();

            if (!string.IsNullOrWhiteSpace(accion))
                consulta = consulta.Where(a => a.AccionAuditoria == accion);

            return await consulta
                .OrderByDescending(a => a.FechaAuditoria)
                .ThenByDescending(a => a.IdAuditoria)
                .Take(limite)
                .Select(a => new AuditoriaDto
                {
                    IdAuditoria = a.IdAuditoria,
                    IdUsuario = a.IdUsuario,
                    NombreUsuario = a.IdUsuarioNavigation.NombreUsuario,
                    AccionAuditoria = a.AccionAuditoria,
                    DetalleAuditoria = a.DetalleAuditoria,
                    FechaAuditoria = a.FechaAuditoria
                })
                .ToListAsync(ct);
        }

        public async Task<AuditoriaDto> ObtenerAsync(int id, CancellationToken ct = default)
        {
            var registro = await _auditorias.Query()
                .Where(a => a.IdAuditoria == id)
                .Select(a => new AuditoriaDto
                {
                    IdAuditoria = a.IdAuditoria,
                    IdUsuario = a.IdUsuario,
                    NombreUsuario = a.IdUsuarioNavigation.NombreUsuario,
                    AccionAuditoria = a.AccionAuditoria,
                    DetalleAuditoria = a.DetalleAuditoria,
                    FechaAuditoria = a.FechaAuditoria
                })
                .FirstOrDefaultAsync(ct);

            return registro
                ?? throw new KeyNotFoundException($"No se encontró la auditoría con ID {id}");
        }
    }
}
