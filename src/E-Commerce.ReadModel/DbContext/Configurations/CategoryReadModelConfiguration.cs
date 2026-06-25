using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.ReadModel.DbContext.Configurations;

/// <summary>
/// EF Core fluent mapping configuration for <see cref="CategoryReadModel"/>.
/// </summary>
public sealed class CategoryReadModelConfiguration : IEntityTypeConfiguration<CategoryReadModel>
{
    public void Configure(EntityTypeBuilder<CategoryReadModel> builder)
    {
        // TODO: Configure table name, keys, columns, indexes
    }
}
