using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.DbContext;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.DbContext;

/// <summary>
/// Extension methods for AppReadDbContext to provide domain-specific queries.
/// </summary>
public static class CatalogReadDbContextExtensions
{
    public static async Task<ProductReadModel?> GetProductBySkuAsync(
        this AppReadDbContext dbContext,
        string sku,
        CancellationToken ct = default)
    {
        return await dbContext.Set<ProductReadModel>()
            .FirstOrDefaultAsync(p => EF.Property<string>(p, "Sku") == sku, ct);
    }
}
