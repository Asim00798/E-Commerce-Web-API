
namespace E_Commerce.Domain.SharedKernel.Interfaces
{
    public interface IRepository<TAggregate>
    where TAggregate : IAggregateRoot
    {
        Task<TAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
        void Remove(TAggregate aggregate);
    }
}
