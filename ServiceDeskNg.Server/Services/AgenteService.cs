using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Services
{
    public class AgenteService
    {
        private readonly IRepositorio<Agente> _agentes;
        private readonly IRepositorio<Ticket> _tickets;
        private readonly IRepositorio<Usuario> _usuarios;

        public AgenteService(
            IRepositorio<Agente> agentes,
            IRepositorio<Ticket> tickets,
            IRepositorio<Usuario> usuarios)
        {
            _agentes = agentes;
            _tickets = tickets;
            _usuarios = usuarios;
        }

        public Task<List<AgenteDto>> ListarAsync(CancellationToken ct = default) =>
            Consulta().OrderBy(a => a.NombreUsuario).ToListAsync(ct);

        public async Task<AgenteDto> ObtenerAsync(int id, CancellationToken ct = default)
        {
            var agente = await Consulta().FirstOrDefaultAsync(a => a.IdAgente == id, ct);
            return agente ?? throw new KeyNotFoundException($"No se encontró el agente con ID {id}");
        }

        public async Task<AgenteDto> ObtenerPorUsuarioAsync(int idUsuario, CancellationToken ct = default)
        {
            var agente = await Consulta().FirstOrDefaultAsync(a => a.IdUsuario == idUsuario, ct);
            return agente
                ?? throw new KeyNotFoundException($"El usuario {idUsuario} no está registrado como agente.");
        }

        public async Task<AgenteDto> CrearAsync(AgenteCreateDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (!await _usuarios.Query().AnyAsync(u => u.IdUsuario == dto.IdUsuario, ct))
                throw new KeyNotFoundException($"No se encontró el usuario con ID {dto.IdUsuario}");

            if (await _agentes.Query().AnyAsync(a => a.IdUsuario == dto.IdUsuario, ct))
                throw new ConflictoNegocioException("Ya existe un agente vinculado a ese usuario.");

            var agente = new Agente
            {
                IdUsuario = dto.IdUsuario,
                IdNivel = dto.IdNivel,
                EspecialidadAgente = dto.EspecialidadAgente,
                DisponibilidadAgente = dto.DisponibilidadAgente ?? true
            };

            await _agentes.AddAsync(agente, ct);
            return await ObtenerAsync(agente.IdAgente, ct);
        }

        public async Task ActualizarAsync(int id, AgenteCreateDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var agente = await _agentes.QueryParaEscritura()
                .FirstOrDefaultAsync(a => a.IdAgente == id, ct)
                ?? throw new KeyNotFoundException($"No se encontró el agente con ID {id}");

            agente.IdNivel = dto.IdNivel;
            agente.EspecialidadAgente = dto.EspecialidadAgente;
            agente.DisponibilidadAgente = dto.DisponibilidadAgente ?? agente.DisponibilidadAgente ?? true;

            await _agentes.GuardarCambiosAsync(ct);
        }

        /// Marca al agente como disponible o no disponible para el reparto automático.
        public async Task CambiarDisponibilidadAsync(
            int id,
            bool disponible,
            CancellationToken ct = default)
        {
            var agente = await _agentes.QueryParaEscritura()
                .FirstOrDefaultAsync(a => a.IdAgente == id, ct)
                ?? throw new KeyNotFoundException($"No se encontró el agente con ID {id}");

            agente.DisponibilidadAgente = disponible;
            await _agentes.GuardarCambiosAsync(ct);
        }

        public async Task EliminarAsync(int id, CancellationToken ct = default)
        {
            if (!await _agentes.Query().AnyAsync(a => a.IdAgente == id, ct))
                throw new KeyNotFoundException($"No se encontró el agente con ID {id}");

            if (await _tickets.Query().AnyAsync(t => t.IdAgenteAsignado == id, ct))
                throw new ConflictoNegocioException(
                    "El agente tiene tickets asignados. Reasigne sus tickets antes de eliminarlo.");

            await _agentes.DeleteAsync(id, ct);
        }

        private IQueryable<AgenteDto> Consulta() =>
            _agentes.Query().Select(a => new AgenteDto
            {
                IdAgente = a.IdAgente,
                IdUsuario = a.IdUsuario,
                IdNivel = a.IdNivel,
                NombreUsuario = a.IdUsuarioNavigation.NombreUsuario,
                CorreoUsuario = a.IdUsuarioNavigation.CorreoUsuario,
                EspecialidadAgente = a.EspecialidadAgente,
                DisponibilidadAgente = a.DisponibilidadAgente ?? true
            });
    }
}
