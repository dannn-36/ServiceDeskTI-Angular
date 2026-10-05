using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Common;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Models.Dtos;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Services
{
    /// Catálogos de tickets (estados y categorías) y las consultas derivadas que
    /// necesitan el resto de servicios: "¿qué id tiene el estado Abierto?",
    /// "¿este estado cuenta como activo?", etc.
    public class CatalogoTicketsService
    {
        private readonly IRepositorio<TicketsEstado> _estados;
        private readonly IRepositorio<TicketsCategoria> _categorias;

        public CatalogoTicketsService(
            IRepositorio<TicketsEstado> estados,
            IRepositorio<TicketsCategoria> categorias)
        {
            _estados = estados;
            _categorias = categorias;
        }

        public Task<List<EstadoTicketDto>> ListarEstadosAsync(CancellationToken ct = default) =>
            _estados.Query()
                .OrderBy(e => e.IdEstado)
                .Select(e => new EstadoTicketDto { IdEstado = e.IdEstado, NombreEstado = e.NombreEstado })
                .ToListAsync(ct);

        public Task<List<CategoriaTicketDto>> ListarCategoriasAsync(CancellationToken ct = default) =>
            _categorias.Query()
                .OrderBy(c => c.NombreCategoria)
                .Select(c => new CategoriaTicketDto { IdCategoria = c.IdCategoria, NombreCategoria = c.NombreCategoria })
                .ToListAsync(ct);

        /// Diccionario id -> nombre normalizado en minúsculas.
        public async Task<Dictionary<int, string>> MapaEstadosAsync(CancellationToken ct = default) =>
            (await _estados.Query().ToListAsync(ct))
                .ToDictionary(e => e.IdEstado, e => e.NombreEstado.Trim().ToLowerInvariant());

        public async Task<Dictionary<int, string>> MapaCategoriasAsync(CancellationToken ct = default) =>
            (await _categorias.Query().ToListAsync(ct))
                .ToDictionary(c => c.IdCategoria, c => c.NombreCategoria);

        /// Ids de los estados en los que un ticket sigue ocupando al agente.
        public async Task<List<int>> IdsEstadosActivosAsync(CancellationToken ct = default)
        {
            var mapa = await MapaEstadosAsync(ct);
            return mapa.Where(par => EsEstadoActivo(par.Value)).Select(par => par.Key).ToList();
        }

        /// Ids de los estados que cuentan como trabajo terminado.
        public async Task<List<int>> IdsEstadosFinalizadosAsync(CancellationToken ct = default)
        {
            var mapa = await MapaEstadosAsync(ct);
            return mapa.Where(par => EsEstadoFinalizado(par.Value)).Select(par => par.Key).ToList();
        }

        public async Task<int> IdEstadoAbiertoAsync(CancellationToken ct = default)
        {
            var mapa = await MapaEstadosAsync(ct);

            foreach (var par in mapa)
            {
                if (par.Value == EstadosTicket.Abierto)
                    return par.Key;
            }

            throw new ConflictoNegocioException(
                "No existe el estado 'Abierto' en la tabla tickets_estados. Cargue los datos iniciales.");
        }

        public async Task<int> IdEstadoPorNombreAsync(string nombre, CancellationToken ct = default)
        {
            var buscado = nombre.Trim().ToLowerInvariant();
            var mapa = await MapaEstadosAsync(ct);

            foreach (var par in mapa)
            {
                if (par.Value == buscado)
                    return par.Key;
            }

            throw new KeyNotFoundException($"No existe el estado de ticket '{nombre}'.");
        }

        public async Task<TicketsCategoria> CategoriaPorNombreAsync(string nombre, CancellationToken ct = default)
        {
            var buscado = nombre.Trim();

            var categoria = await _categorias.Query()
                .FirstOrDefaultAsync(c => c.NombreCategoria.ToLower() == buscado.ToLower(), ct);

            return categoria
                ?? throw new KeyNotFoundException($"No existe la categoría '{nombre}'.");
        }

        public Task<bool> ExisteEstadoAsync(int idEstado, CancellationToken ct = default) =>
            _estados.Query().AnyAsync(e => e.IdEstado == idEstado, ct);

        public Task<bool> ExisteCategoriaAsync(int idCategoria, CancellationToken ct = default) =>
            _categorias.Query().AnyAsync(c => c.IdCategoria == idCategoria, ct);

        /// Tolera las variantes que conviven en la base de datos ("En Progreso", "en-progreso").
        public static bool EsEstadoActivo(string nombreEstadoNormalizado) =>
            EstadosTicket.Activos.Contains(Normalizar(nombreEstadoNormalizado));

        public static bool EsEstadoFinalizado(string nombreEstadoNormalizado) =>
            EstadosTicket.Finalizados.Contains(Normalizar(nombreEstadoNormalizado));

        private static string Normalizar(string nombre) =>
            nombre.Trim().ToLowerInvariant().Replace(' ', '-');
    }
}
