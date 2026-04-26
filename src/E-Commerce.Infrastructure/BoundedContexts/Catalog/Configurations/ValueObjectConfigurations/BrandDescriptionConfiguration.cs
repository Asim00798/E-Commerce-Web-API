using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.BoundedContexts.Catalog.Configurations.ValueObjectConfigurations;

/// <summary>
/// EF Core owned-type configuration for the BrandDescription value object.
/// </summary>
public sealed class BrandDescriptionConfiguration : IEntityTypeConfiguration<object>
{
    public void Configure(EntityTypeBuilder<object> builder)
    {
        // TODO: Configure BrandDescription owned-type columns
    }
}
