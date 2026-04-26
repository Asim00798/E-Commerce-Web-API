namespace E_Commerce.Infrastructure.BoundedContexts.Catalog.Repositories;

/// <summary>
/// EF Core implementation of the Category repository for the Catalog bounded context.
/// </summary>
public sealed class CategoryRepository
{
    private readonly DbContext _context;

    public CategoryRepository(CatalogDbContexts.CatalogDbContext context)
    {
        _context = context;
    }

    // TODO: Implement ICategoryRepository members
}
