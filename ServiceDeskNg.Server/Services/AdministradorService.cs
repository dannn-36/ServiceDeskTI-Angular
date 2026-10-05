using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Services
{
    public class AdministradorService
    {
        private readonly IRepositorio<Administrador> _administradores;

        public AdministradorService(IRepositorio<Administrador> administradores)
        {
            _administradores = administradores;
        }

        public Task<List<AdministradorDto>> ListarAsync(CancellationToken ct = default) =>
            Consulta().OrderBy(a => a.NombreUsuario).ToListAsync(ct);

        public async Task<AdministradorDto> ObtenerAsync(int id, CancellationToken ct = default)
        {
            var administrador = await Consulta().FirstOrDefaultAsync(a => a.IdAdmin == id, ct);
            return administrador
                ?? throw new KeyNotFoundException($"No se encontró el administrador con ID {id}");
        }

        public async Task ActualizarAsync(
            int id,
            AdministradorUpdateDto dto,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var administrador = await _administradores.QueryParaEscritura()
                .FirstOrDefaultAsync(a => a.IdAdmin == id, ct)
                ?? throw new KeyNotFoundException($"No se encontró el administrador con ID {id}");

            administrador.IdNivel = dto.IdNivel;
            administrador.AreaResponsabilidadAdmin = dto.AreaResponsabilidadAdmin;

            await _administradores.GuardarCambiosAsync(ct);
        }

        private IQueryable<AdministradorDto> Consulta() =>
            _administradores.Query().Select(a => new AdministradorDto
            {
                IdAdmin = a.IdAdmin,
                IdUsuario = a.IdUsuario,
                IdNivel = a.IdNivel,
                NombreUsuario = a.IdUsuarioNavigation.NombreUsuario,
                CorreoUsuario = a.IdUsuarioNavigation.CorreoUsuario,
                AreaResponsabilidadAdmin = a.AreaResponsabilidadAdmin
            });
    }
}
