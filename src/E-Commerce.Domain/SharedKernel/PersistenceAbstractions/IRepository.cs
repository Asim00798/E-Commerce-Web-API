
namespace E_Commerce.Domain.SharedKernel.PersistenceAbstractions
{
    public interface IRepository<T>
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(T aggregate, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(T aggregate, CancellationToken cancellationToken = default);
        void Remove(T aggregate);
    }
}
