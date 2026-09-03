using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Entities;
using E_Commerce.Infrastructure.Persistence.Common.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Orders.Configurations;

public sealed class CartItemConfiguration : BaseEntityConfiguration<CartItem>
{
    public override void Configure(EntityTypeBuilder<CartItem> builder)
    {
        base.Configure(builder);

        builder.ToTable("CartItems", "ordering");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.CartId)
               .IsRequired();

        builder.Property(ci => ci.ProductId)
               .IsRequired();

        builder.Property(ci => ci.ProductVariantId)
               .IsRequired();

        builder.Property(ci => ci.Sku)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(ci => ci.ProductName)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(ci => ci.VariantName)
               .IsRequired()
               .HasMaxLength(200);

        // Money value object as owned type
        builder.OwnsOne(ci => ci.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("UnitPriceAmount")
                 .HasPrecision(18, 2)
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("UnitPriceCurrency")
                 .HasMaxLength(3)
                 .IsRequired();
        });

        builder.Property(ci => ci.Quantity)
               .IsRequired();
    }
}