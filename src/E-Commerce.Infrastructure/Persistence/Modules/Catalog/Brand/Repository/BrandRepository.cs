using BrandAggregate = E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors.Brand;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Brand.Repository;

public sealed class BrandRepository : Repository<BrandAggregate>, IBrandRepository
{
    public BrandRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<BrandAggregate?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Name == name, ct);
    }
}