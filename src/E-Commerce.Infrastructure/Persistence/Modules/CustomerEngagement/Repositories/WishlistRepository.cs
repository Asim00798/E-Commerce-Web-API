using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Wishlist.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.Repositories;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Modules.CustomerEngagement.Repositories;

public sealed class WishlistRepository : Repository<Wishlist>, IWishlistRepository
{
    public WishlistRepository(AppDbContext dbContext) : base(dbContext)
    {}

    public async Task<Wishlist?> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.CustomerId == customerId, ct);
    }

    // For read operations we can also provide a no-tracking version if needed.
}