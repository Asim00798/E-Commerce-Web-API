using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Repositories;

/// <summary>
/// EF Core implementation of the Category repository for the Catalog bounded context.
/// </summary>
public sealed class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext dbContext) : base(dbContext) { }
    // TODO: Implement ICategoryRepository members
}
