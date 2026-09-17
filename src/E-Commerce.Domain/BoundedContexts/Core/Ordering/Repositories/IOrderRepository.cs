using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Enums;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<List<Guid>> GetPendingOrderIdsOlderThanAsync(DateTime expirationTime, CancellationToken ct = default);

    /// <summary>
    /// Returns a page of orders filtered by status and/or customer, ordered by
    /// most recently created first. Complements the inherited
    /// <c>GetPagedAsync(pageNumber, pageSize, ct)</c> which does not filter.
    /// </summary>
    Task<IReadOnlyList<Order>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        OrderStatus? status,
        Guid? customerId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the total count of orders matching the same filters used by
    /// <see cref="GetPagedAsync(int,int,OrderStatus?,Guid?,CancellationToken)"/>.
    /// Complements the inherited <c>GetTotalCountAsync(ct)</c> which does not filter.
    /// </summary>
    Task<int> GetTotalCountAsync(
        OrderStatus? status,
        Guid? customerId,
        CancellationToken ct = default);
}
