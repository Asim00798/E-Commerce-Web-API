using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Configurations;

/// <summary>
/// Example of applying a global query filter to ProductReadModel.
/// Uncomment the code block if you add an `IsDeleted` flag to the read model.
/// </summary>
public static class ProductReadModelGlobalFilters
{
    public static void ApplySoftDeleteFilter(ModelBuilder modelBuilder)
    {
        // modelBuilder.Entity<ProductReadModel>().HasQueryFilter(p => !p.IsDeleted);
    }
}
