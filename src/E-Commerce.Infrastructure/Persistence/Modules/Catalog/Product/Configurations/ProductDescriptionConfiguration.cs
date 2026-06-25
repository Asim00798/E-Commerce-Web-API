using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.ValueObjects;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Configurations;

/// <summary>
/// EF Core owned-type configuration for the ProductDescription value object.
/// </summary>
public sealed class ProductDescriptionConfiguration : IEntityTypeConfiguration<ProductDescription>
{
    public void Configure(EntityTypeBuilder<ProductDescription> builder)
    {
        // TODO: Configure ProductDescription owned-type columns
    }
}
