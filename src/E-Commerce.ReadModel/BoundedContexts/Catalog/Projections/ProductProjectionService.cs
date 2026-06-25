using E_Commerce.ReadModel.DbContext;
using E_Commerce.ReadModel.Infrastructure.Caching;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Projections;

/// <summary>
/// Service that listens to Catalog domain events and updates <c>ProductReadModel</c> projections.
/// </summary>
public sealed class ProductProjectionService(AppReadDbContext dbContext, ICacheService cacheService)
{
    /* 
     * TODO: Implement event handlers for ProductCreated, ProductUpdated, ProductDeleted, etc.
     * 
     * Example implementation pattern:
     * 
     * public async Task HandleAsync(ProductUpdatedEvent @event, CancellationToken ct)
     * {
     *     // 1. Update the Read Model in the database via dbContext
     *     // ...
     *     
     *     // 2. Invalidate Cache
     *     await cacheService.RemoveAsync($"Product_{@event.ProductId}", ct);
     *     await cacheService.RemoveAsync("ProductList_*", ct);
     * }
     */
}
