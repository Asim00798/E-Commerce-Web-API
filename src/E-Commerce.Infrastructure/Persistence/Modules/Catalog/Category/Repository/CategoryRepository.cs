using CategoryAggregate = E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Behaviors.Category;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Category.Repository;

public sealed class CategoryRepository : Repository<CategoryAggregate>, ICategoryRepository
{
    public CategoryRepository(AppDbContext dbContext) : base(dbContext)
    {}

    public async Task<CategoryAggregate?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Name == name, ct);
    }

    public override async Task<CategoryAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}