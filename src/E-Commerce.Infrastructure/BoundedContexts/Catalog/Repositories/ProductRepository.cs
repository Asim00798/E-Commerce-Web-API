namespace E_Commerce.Infrastructure.BoundedContexts.Catalog.Repositories;

/// <summary>
/// EF Core implementation of the Product repository for the Catalog bounded context.
/// </summary>
public sealed class ProductRepository
{
    private readonly DbContext _context;

    public ProductRepository(CatalogDbContexts.CatalogDbContext context)
    {
        _context = context;
    }

    // TODO: Implement IProductRepository members
}
