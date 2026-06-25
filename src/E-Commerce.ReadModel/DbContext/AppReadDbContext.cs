using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;

namespace E_Commerce.ReadModel.DbContext;

/// <summary>
/// Unified read-optimized DbContext for all bounded contexts.
/// </summary>
public sealed class AppReadDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public AppReadDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<AppReadDbContext> options)
        : base(options)
    {
    }

    public Microsoft.EntityFrameworkCore.DbSet<BrandReadModel> Brands => Set<BrandReadModel>();
    public Microsoft.EntityFrameworkCore.DbSet<CategoryReadModel> Categories => Set<CategoryReadModel>();
    public Microsoft.EntityFrameworkCore.DbSet<ProductReadModel> Products => Set<ProductReadModel>();
    public Microsoft.EntityFrameworkCore.DbSet<ProductListReadModel> ProductList => Set<ProductListReadModel>();

    protected override void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppReadDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
