using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.ReadModel.DbContext.Configurations;

/// <summary>
/// EF Core fluent mapping configuration for <see cref="BrandReadModel"/>.
/// </summary>
public sealed class BrandReadModelConfiguration : IEntityTypeConfiguration<BrandReadModel>
{
    public void Configure(EntityTypeBuilder<BrandReadModel> builder)
    {
        // TODO: Configure table name, keys, columns, indexes
    }
}
