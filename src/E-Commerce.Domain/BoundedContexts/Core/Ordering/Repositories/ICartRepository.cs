using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Behaviors;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
}