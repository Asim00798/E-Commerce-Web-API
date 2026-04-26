using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.BoundedContexts.Catalog.Configurations.ValueObjectConfigurations;

/// <summary>
/// EF Core owned-type configuration for the CategoryName value object.
/// </summary>
public sealed class CategoryNameConfiguration : IEntityTypeConfiguration<object>
{
    public void Configure(EntityTypeBuilder<object> builder)
    {
        // TODO: Configure CategoryName owned-type columns
    }
}
