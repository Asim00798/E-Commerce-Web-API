using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;
using ProductAggregate =  E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors.Product;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Product.Repository;

public sealed class ProductRepository : Repository<ProductAggregate>, IProductRepository
{
    public ProductRepository(AppDbContext dbContext) : base(dbContext)
    {}

    public override async Task<ProductAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Images)
            .Include(x => x.Variants)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<ProductAggregate>> GetPublishedProductsAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Images)
            .Include(x => x.Variants)
            .Where(x => x.Status == E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Enums.ProductStatus.Published)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductAggregate>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Images)
            .Include(x => x.Variants)
            .Where(x => x.CategoryId == categoryId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductAggregate>> GetProductsByBrandAsync(Guid brandId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Images)
            .Include(x => x.Variants)
            .Where(x => x.BrandId == brandId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductAggregate>> SearchProductsAsync(
        string searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(x => x.Images)
            .Include(x => x.Variants)
            .Where(x => x.Status == ProductStatus.Published);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{searchTerm.Trim()}%";

            query = query.Where(x =>
                EF.Functions.Like(x.Description.Name, pattern) ||
                (x.Description.ShortDescription != null &&
                 EF.Functions.Like(x.Description.ShortDescription, pattern)));
        }

        return await query
            .OrderBy(x => x.Description.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    // <summary>
    //returns how many products match the current search filters
    //without loading the actual products.
    // </summary>
    public async Task<int> GetSearchTotalCountAsync(
        string searchTerm,
        CancellationToken ct = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(x => x.Status == ProductStatus.Published);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{searchTerm.Trim()}%";

            query = query.Where(x =>
                EF.Functions.Like(x.Description.Name, pattern) ||
                (x.Description.ShortDescription != null &&
                 EF.Functions.Like(x.Description.ShortDescription, pattern)));
        }

        return await query.CountAsync(ct);
    }
}