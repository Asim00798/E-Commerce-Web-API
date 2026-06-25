using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.ValueObjects;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Configurations;

/// <summary>
/// EF Core owned-type configuration for the BrandDescription value object.
/// </summary>
public sealed class BrandDescriptionConfiguration : IEntityTypeConfiguration<BrandDescription>
{
    public void Configure(EntityTypeBuilder<BrandDescription> builder)
    {
        // TODO: Configure BrandDescription owned-type columns
    }
}
