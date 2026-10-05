using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Services
{
    /// Clientes (usuarios finales) que abren tickets.
    public class EndUserService
    {
        private readonly IRepositorio<EndUser> _clientes;
        private readonly IRepositorio<Ticket> _tickets;
        private readonly IRepositorio<Usuario> _usuarios;

        public EndUserService(
            IRepositorio<EndUser> clientes,
            IRepositorio<Ticket> tickets,
            IRepositorio<Usuario> usuarios)
        {
            _clientes = clientes;
            _tickets = tickets;
            _usuarios = usuarios;
        }

        public Task<List<EndUserDto>> ListarAsync(CancellationToken ct = default) =>
            Consulta().OrderBy(c => c.NombreUsuario).ToListAsync(ct);

        public async Task<EndUserDto> ObtenerAsync(int id, CancellationToken ct = default)
        {
            var cliente = await Consulta().FirstOrDefaultAsync(c => c.IdCliente == id, ct);
            return cliente ?? throw new KeyNotFoundException($"No se encontró el cliente con ID {id}");
        }

        public async Task<EndUserDto> ObtenerPorUsuarioAsync(int idUsuario, CancellationToken ct = default)
        {
            var cliente = await Consulta().FirstOrDefaultAsync(c => c.IdUsuario == idUsuario, ct);
            return cliente
                ?? throw new KeyNotFoundException($"El usuario {idUsuario} no está registrado como cliente.");
        }

        public async Task<EndUserDto> CrearAsync(EndUserCreateDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (!await _usuarios.Query().AnyAsync(u => u.IdUsuario == dto.IdUsuario, ct))
                throw new KeyNotFoundException($"No se encontró el usuario con ID {dto.IdUsuario}");

            if (await _clientes.Query().AnyAsync(c => c.IdUsuario == dto.IdUsuario, ct))
                throw new ConflictoNegocioException("Ya existe un cliente vinculado a ese usuario.");

            var cliente = new EndUser { IdUsuario = dto.IdUsuario, IdNivel = dto.IdNivel };
            await _clientes.AddAsync(cliente, ct);

            return await ObtenerAsync(cliente.IdCliente, ct);
        }

        public async Task ActualizarAsync(int id, EndUserCreateDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var cliente = await _clientes.QueryParaEscritura()
                .FirstOrDefaultAsync(c => c.IdCliente == id, ct)
                ?? throw new KeyNotFoundException($"No se encontró el cliente con ID {id}");

            cliente.IdNivel = dto.IdNivel;
            await _clientes.GuardarCambiosAsync(ct);
        }

        public async Task EliminarAsync(int id, CancellationToken ct = default)
        {
            if (!await _clientes.Query().AnyAsync(c => c.IdCliente == id, ct))
                throw new KeyNotFoundException($"No se encontró el cliente con ID {id}");

            if (await _tickets.Query().AnyAsync(t => t.IdCliente == id, ct))
                throw new ConflictoNegocioException(
                    "El cliente tiene tickets registrados y no puede eliminarse.");

            await _clientes.DeleteAsync(id, ct);
        }

        private IQueryable<EndUserDto> Consulta() =>
            _clientes.Query().Select(c => new EndUserDto
            {
                IdCliente = c.IdCliente,
                IdUsuario = c.IdUsuario,
                IdNivel = c.IdNivel,
                NombreUsuario = c.IdUsuarioNavigation.NombreUsuario,
                CorreoUsuario = c.IdUsuarioNavigation.CorreoUsuario,
                DepartamentoUsuario = c.IdUsuarioNavigation.DepartamentoUsuario
            });
    }
}
