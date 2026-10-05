namespace ServiceDeskNg.Server.Repositories.Interfaces
{
    /// OBSOLETO: contrato síncrono que implementaban las catorce clases de repositorio
    /// (una por tabla). Lo sustituye IRepositorio&lt;T&gt; + EfRepository&lt;T&gt;.
    /// Se conserva solo para que los archivos antiguos sigan compilando hasta que se borren.
    [Obsolete("Use IRepositorio<T> con EfRepository<T>.")]
    public interface ICrudRepository<T>
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(int id);
    }
}
