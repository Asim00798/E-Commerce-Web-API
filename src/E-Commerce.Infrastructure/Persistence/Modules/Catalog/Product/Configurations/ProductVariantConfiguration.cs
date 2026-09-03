using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Entities;
using E_Commerce.Infrastructure.Persistence.Common.Configurations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Product.Configurations;

public sealed class ProductVariantConfiguration : BaseEntityConfiguration<ProductVariant>
{
    public override void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        base.Configure(builder);

        builder.ToTable("ProductVariants", "catalog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.SKU)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.OwnsOne(x => x.Price, price =>
        {
            price.Property(p => p.Amount).HasColumnName("PriceAmount").HasPrecision(18, 2);
            price.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.Property(x => x.StockQuantity)
            .IsRequired();

        builder.HasIndex(x => new { x.ProductId, x.SKU })
            .IsUnique()
            .HasFilter("[SKU] IS NOT NULL");
    }
}