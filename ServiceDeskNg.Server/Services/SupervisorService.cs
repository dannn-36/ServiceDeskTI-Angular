using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Services
{
    public class SupervisorService
    {
        private readonly IRepositorio<Supervisor> _supervisores;
        private readonly IRepositorio<Usuario> _usuarios;

        public SupervisorService(IRepositorio<Supervisor> supervisores, IRepositorio<Usuario> usuarios)
        {
            _supervisores = supervisores;
            _usuarios = usuarios;
        }

        public Task<List<SupervisorDto>> ListarAsync(CancellationToken ct = default) =>
            Consulta().OrderBy(s => s.NombreUsuario).ToListAsync(ct);

        public async Task<SupervisorDto> ObtenerAsync(int id, CancellationToken ct = default)
        {
            var supervisor = await Consulta().FirstOrDefaultAsync(s => s.IdSupervisor == id, ct);
            return supervisor
                ?? throw new KeyNotFoundException($"No se encontró el supervisor con ID {id}");
        }

        public async Task<SupervisorDto> CrearAsync(SupervisorCreateDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (!await _usuarios.Query().AnyAsync(u => u.IdUsuario == dto.IdUsuario, ct))
                throw new KeyNotFoundException($"No se encontró el usuario con ID {dto.IdUsuario}");

            if (await _supervisores.Query().AnyAsync(s => s.IdUsuario == dto.IdUsuario, ct))
                throw new ConflictoNegocioException("Ya existe un supervisor vinculado a ese usuario.");

            var supervisor = new Supervisor
            {
                IdUsuario = dto.IdUsuario,
                IdNivel = dto.IdNivel,
                AreaResponsabilidadSupervisor = dto.AreaResponsabilidadSupervisor
            };

            await _supervisores.AddAsync(supervisor, ct);
            return await ObtenerAsync(supervisor.IdSupervisor, ct);
        }

        public async Task ActualizarAsync(int id, SupervisorCreateDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var supervisor = await _supervisores.QueryParaEscritura()
                .FirstOrDefaultAsync(s => s.IdSupervisor == id, ct)
                ?? throw new KeyNotFoundException($"No se encontró el supervisor con ID {id}");

            supervisor.IdNivel = dto.IdNivel;
            supervisor.AreaResponsabilidadSupervisor = dto.AreaResponsabilidadSupervisor;

            await _supervisores.GuardarCambiosAsync(ct);
        }

        public async Task EliminarAsync(int id, CancellationToken ct = default)
        {
            if (!await _supervisores.Query().AnyAsync(s => s.IdSupervisor == id, ct))
                throw new KeyNotFoundException($"No se encontró el supervisor con ID {id}");

            await _supervisores.DeleteAsync(id, ct);
        }

        private IQueryable<SupervisorDto> Consulta() =>
            _supervisores.Query().Select(s => new SupervisorDto
            {
                IdSupervisor = s.IdSupervisor,
                IdUsuario = s.IdUsuario,
                IdNivel = s.IdNivel,
                NombreUsuario = s.IdUsuarioNavigation.NombreUsuario,
                CorreoUsuario = s.IdUsuarioNavigation.CorreoUsuario,
                AreaResponsabilidadSupervisor = s.AreaResponsabilidadSupervisor
            });
    }
}
