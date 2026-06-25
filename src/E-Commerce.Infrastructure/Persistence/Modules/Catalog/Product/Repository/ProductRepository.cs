using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Repositories;

/// <summary>
/// EF Core implementation of the Product repository for the Catalog bounded context.
/// </summary>
public sealed class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext dbContext) : base(dbContext) { }
    // TODO: Implement IProductRepository members
}
