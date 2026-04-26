using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.SharedKernel.PersistenceAbstractions
{
    public interface IRepository<T>
    where T : IAggregateRoot
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(T aggregate, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
        void Remove(T aggregate);
    }
}
