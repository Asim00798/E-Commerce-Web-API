using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.BoundedContexts.Catalog.Configurations.AggregateConfigurations;

/// <summary>
/// EF Core fluent configuration for the Product aggregate root.
/// </summary>
public sealed class ProductConfiguration : IEntityTypeConfiguration<object>
{
    public void Configure(EntityTypeBuilder<object> builder)
    {
        // TODO: Configure Product table, keys, indexes, and relationships
    }
}
