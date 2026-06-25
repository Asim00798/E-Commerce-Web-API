using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Repositories;

/// <summary>
/// EF Core implementation of the Brand repository for the Catalog bounded context.
/// </summary>
public sealed class BrandRepository : Repository<Brand>, IBrandRepository
{
    public BrandRepository(AppDbContext dbContext):base(dbContext){}

    // TODO: Implement IBrandRepository members
}
