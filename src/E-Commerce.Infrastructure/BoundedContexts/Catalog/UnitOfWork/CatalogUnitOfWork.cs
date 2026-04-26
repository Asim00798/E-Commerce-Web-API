namespace E_Commerce.Infrastructure.BoundedContexts.Catalog.UnitOfWork;

/// <summary>
/// Unit of Work scoped to the Catalog bounded context.
/// Coordinates commits across Catalog repositories within a single transaction.
/// </summary>
public sealed class CatalogUnitOfWork
{
    private readonly CatalogDbContexts.CatalogDbContext _context;

    public CatalogUnitOfWork(CatalogDbContexts.CatalogDbContext context)
    {
        _context = context;
    }

    /// <summary>Commits all pending changes to the database.</summary>
    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
