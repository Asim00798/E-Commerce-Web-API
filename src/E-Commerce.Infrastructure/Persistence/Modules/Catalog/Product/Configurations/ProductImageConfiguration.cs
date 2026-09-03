using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Entities;
using E_Commerce.Infrastructure.Persistence.Common.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Product.Configurations;

public sealed class ProductImageConfiguration : BaseEntityConfiguration<ProductImage>
{
    public override void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        base.Configure(builder);

        builder.ToTable("ProductImages", "catalog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.FileId)
            .IsRequired();

        builder.Property(x => x.AltText)
            .HasMaxLength(250);

        builder.Property(x => x.IsMain)
            .IsRequired();

        builder.HasIndex(x => new { x.ProductId, x.FileId })
            .IsUnique();
    }
}