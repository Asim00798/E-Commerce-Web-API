
namespace E_Commerce.Domain.SharedKernel.PersistenceAbstractions
{
    public interface IRepository<T>
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(T aggregate, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(T aggregate, CancellationToken cancellationToken = default);
        void Remove(T aggregate);

        // <summary>
        // Retrieves a paginated list of entities of type T.
        // </summary>
        Task<IReadOnlyList<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);
        
        // <summary>
        // Retrieves the total count of entities of type T.
        // </summary>
        Task<int> GetTotalCountAsync(CancellationToken ct = default);

        /// <summary>
        /// Permanently deletes an entity by its ID using a hard SQL DELETE.
        /// This bypasses the soft-delete interceptor and change tracker.
        /// </summary>
        Task<bool> HardDeleteAsync(Guid id, CancellationToken ct = default);
    }
}
