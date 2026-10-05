namespace ServiceDeskNg.Server.Repositories.Interfaces
{
    /// Acceso a datos genérico para una entidad. Lo implementa EfRepository&lt;T&gt;,
    /// de modo que no hace falta una clase de repositorio por tabla.
    /// Para consultas con filtros, proyecciones o Include, los servicios usan Query().
    public interface IRepositorio<T> where T : class
    {
        /// Consulta sin seguimiento de cambios, para componer filtros y proyecciones.
        IQueryable<T> Query();

        /// Consulta con seguimiento, para leer una entidad que luego se va a modificar.
        IQueryable<T> QueryParaEscritura();

        Task<List<T>> GetAllAsync(CancellationToken ct = default);

        Task<T?> GetByIdAsync(int id, CancellationToken ct = default);

        Task AddAsync(T entity, CancellationToken ct = default);

        Task UpdateAsync(T entity, CancellationToken ct = default);

        /// Devuelve false si la entidad no existía.
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);

        Task<int> GuardarCambiosAsync(CancellationToken ct = default);
    }
}
