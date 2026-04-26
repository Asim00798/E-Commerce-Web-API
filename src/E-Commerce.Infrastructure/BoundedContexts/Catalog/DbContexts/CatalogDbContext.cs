namespace E_Commerce.Infrastructure.BoundedContexts.Catalog.DbContexts;

/// <summary>
/// Write-side EF Core DbContext for the Catalog bounded context.
/// Handles all command / write operations for Products, Brands, and Categories.
/// </summary>
public sealed class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    // TODO: Add DbSet<T> write-side aggregate sets

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}
