using E_Commerce.ReadModel.DbContext;
using E_Commerce.ReadModel.Infrastructure.Caching;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Projections;

/// <summary>
/// Service that listens to Catalog domain events and updates <c>CategoryReadModel</c> projections.
/// </summary>
public sealed class CategoryProjectionService(AppReadDbContext dbContext, ICacheService cacheService)
{
    /* 
     * TODO: Implement event handlers for CategoryCreated, CategoryUpdated, CategoryDeleted, etc.
     * 
     * Example implementation pattern:
     * 
     * public async Task HandleAsync(CategoryUpdatedEvent @event, CancellationToken ct)
     * {
     *     // 1. Update the Read Model in the database via dbContext
     *     // ...
     *     
     *     // 2. Invalidate Cache
     *     await cacheService.RemoveAsync($"Category_{@event.CategoryId}", ct);
     *     await cacheService.RemoveAsync("CategoryList_*", ct);
     * }
     */
}
