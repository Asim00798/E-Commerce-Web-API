using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Behaviors;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<List<Guid>> GetPendingOrderIdsOlderThanAsync(DateTime expirationTime, CancellationToken ct = default);
}