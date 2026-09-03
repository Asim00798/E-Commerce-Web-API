using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Modules.Orders.Repositories;

public sealed class CartRepository : Repository<Cart>, ICartRepository
{
    public CartRepository(AppDbContext dbContext) : base(dbContext)
    {}

    /// <summary>
    /// Retrieves a Cart by its unique ID, including all cart items.
    /// Used by command handlers that need to mutate the cart.
    /// </summary>
    public override async Task<Cart?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        return await _dbSet
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    /// <summary>
    /// Retrieves the active cart for a specific customer, including items.
    /// </summary>
    public async Task<Cart?> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);
    }
}