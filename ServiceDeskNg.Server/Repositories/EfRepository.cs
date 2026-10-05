using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Repositories.Interfaces;

namespace ServiceDeskNg.Server.Repositories
{
    /// Implementación única del repositorio para cualquier entidad del contexto.
    /// Sustituye a las catorce clases repetidas que existían antes (una por tabla).
    public class EfRepository<T> : IRepositorio<T> where T : class
    {
        private readonly ServiceDeskContext _context;

        public EfRepository(ServiceDeskContext context)
        {
            _context = context;
        }

        public IQueryable<T> Query() => _context.Set<T>().AsNoTracking();

        public IQueryable<T> QueryParaEscritura() => _context.Set<T>();

        public Task<List<T>> GetAllAsync(CancellationToken ct = default) =>
            _context.Set<T>().AsNoTracking().ToListAsync(ct);

        public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
            await _context.Set<T>().FindAsync([id], ct);

        public async Task AddAsync(T entity, CancellationToken ct = default)
        {
            await _context.Set<T>().AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(T entity, CancellationToken ct = default)
        {
            // Si la instancia ya viene del contexto, Update() no hace daño:
            // EF conserva el estado de las propiedades realmente modificadas.
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entidad = await _context.Set<T>().FindAsync([id], ct);
            if (entidad is null)
                return false;

            _context.Set<T>().Remove(entidad);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public Task<int> GuardarCambiosAsync(CancellationToken ct = default) =>
            _context.SaveChangesAsync(ct);
    }
}
