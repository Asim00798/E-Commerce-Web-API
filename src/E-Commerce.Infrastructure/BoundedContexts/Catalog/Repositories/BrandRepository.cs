namespace E_Commerce.Infrastructure.BoundedContexts.Catalog.Repositories;

/// <summary>
/// EF Core implementation of the Brand repository for the Catalog bounded context.
/// </summary>
public sealed class BrandRepository
{
    private readonly DbContext _context;

    public BrandRepository(CatalogDbContexts.CatalogDbContext context)
    {
        _context = context;
    }

    // TODO: Implement IBrandRepository members
}
