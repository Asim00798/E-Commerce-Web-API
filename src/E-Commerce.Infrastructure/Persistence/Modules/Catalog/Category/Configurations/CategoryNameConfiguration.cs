using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.ValueObjects;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Configurations;

/// <summary>
/// EF Core owned-type configuration for the CategoryName value object.
/// </summary>
public sealed class CategoryNameConfiguration : IEntityTypeConfiguration<CategoryName>
{
    public void Configure(EntityTypeBuilder<CategoryName> builder)
    {
        // TODO: Configure CategoryName owned-type columns
    }
}
