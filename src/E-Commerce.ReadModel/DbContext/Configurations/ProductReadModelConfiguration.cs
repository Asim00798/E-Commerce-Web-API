using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.ReadModel.DbContext.Configurations;

/// <summary>
/// EF Core fluent mapping configuration for <see cref="ProductReadModel"/>.
/// </summary>
public sealed class ProductReadModelConfiguration : IEntityTypeConfiguration<ProductReadModel>
{
    public void Configure(EntityTypeBuilder<ProductReadModel> builder)
    {
        // TODO: Configure table name, keys, columns, indexes
    }
}
