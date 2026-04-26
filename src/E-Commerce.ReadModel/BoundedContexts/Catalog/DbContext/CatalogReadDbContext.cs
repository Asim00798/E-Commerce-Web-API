using E_Commerce.ReadModel.Abstractions;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.DbContext;

/// <summary>
/// Read-optimised EF Core DbContext for the Catalog bounded context.
/// All queries run with <c>AsNoTracking</c> by default.
/// </summary>
public sealed class CatalogReadDbContext : Microsoft.EntityFrameworkCore.DbContext, IReadDbContext
{
    public CatalogReadDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<CatalogReadDbContext> options)
        : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
    }

    // TODO: Add DbSet<T> read model sets
}
