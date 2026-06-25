using E_Commerce.ReadModel.DbContext;
using E_Commerce.ReadModel.Infrastructure.Caching;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Projections;

/// <summary>
/// Service that listens to Catalog domain events and updates <c>BrandReadModel</c> projections.
/// </summary>
public sealed class BrandProjectionService(AppReadDbContext dbContext, ICacheService cacheService)
{
    /* 
     * TODO: Implement event handlers for BrandCreated, BrandUpdated, BrandDeleted, etc.
     * 
     * Example implementation pattern:
     * 
     * public async Task HandleAsync(BrandUpdatedEvent @event, CancellationToken ct)
     * {
     *     // 1. Update the Read Model in the database via dbContext
     *     // ...
     *     
     *     // 2. Invalidate Cache
     *     await cacheService.RemoveAsync($"Brand_{@event.BrandId}", ct);
     *     await cacheService.RemoveAsync("BrandList_*", ct);
     * }
     */
}
