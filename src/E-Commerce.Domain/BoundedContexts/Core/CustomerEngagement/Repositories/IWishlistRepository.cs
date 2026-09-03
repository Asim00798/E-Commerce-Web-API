using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Wishlist.Behaviors;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.Repositories;

public interface IWishlistRepository : IRepository<Wishlist>
{
    /// <summary>
    /// Returns the wishlist for a specific customer, if any.
    /// </summary>
    Task<Wishlist?> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken ct = default);
}