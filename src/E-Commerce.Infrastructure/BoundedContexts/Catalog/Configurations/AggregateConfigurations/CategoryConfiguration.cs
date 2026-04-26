using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.BoundedContexts.Catalog.Configurations.AggregateConfigurations;

/// <summary>
/// EF Core fluent configuration for the Category aggregate root.
/// </summary>
public sealed class CategoryConfiguration : IEntityTypeConfiguration<object>
{
    public void Configure(EntityTypeBuilder<object> builder)
    {
        // TODO: Configure Category table, keys, and relationships
    }
}
