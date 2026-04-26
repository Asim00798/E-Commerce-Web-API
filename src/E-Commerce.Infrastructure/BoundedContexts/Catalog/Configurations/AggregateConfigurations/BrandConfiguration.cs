using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.BoundedContexts.Catalog.Configurations.AggregateConfigurations;

/// <summary>
/// EF Core fluent configuration for the Brand aggregate root.
/// </summary>
public sealed class BrandConfiguration : IEntityTypeConfiguration<object>
{
    public void Configure(EntityTypeBuilder<object> builder)
    {
        // TODO: Configure Brand table, keys, and relationships
    }
}
