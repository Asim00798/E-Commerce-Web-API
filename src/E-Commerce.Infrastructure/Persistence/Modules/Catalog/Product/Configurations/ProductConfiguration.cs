using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Configurations;

/// <summary>
/// EF Core fluent configuration for the Product aggregate root.
/// </summary>
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // TODO: Configure Product table, keys, and relationships
    }
}
