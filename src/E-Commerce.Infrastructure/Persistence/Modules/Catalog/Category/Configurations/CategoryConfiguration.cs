using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Behaviors;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Configurations;

/// <summary>
/// EF Core fluent configuration for the Category aggregate root.
/// </summary>
public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // TODO: Configure Category table, keys, and relationships
    }
}
