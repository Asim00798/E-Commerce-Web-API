using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Entities;
using E_Commerce.Infrastructure.Persistence.Common.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Orders.Configurations;

public sealed class OrderItemConfiguration : BaseEntityConfiguration<OrderItem>
{
    public override void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        base.Configure(builder);

        builder.ToTable("OrderItems", "ordering");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.OrderId)
               .IsRequired();

        builder.Property(oi => oi.ProductId)
               .IsRequired();

        builder.Property(oi => oi.ProductVariantId)
               .IsRequired();

        builder.Property(oi => oi.Sku)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(oi => oi.ProductName)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(oi => oi.VariantName)
               .IsRequired()
               .HasMaxLength(200);

        builder.OwnsOne(oi => oi.UnitPrice, money =>
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

        builder.OwnsOne(oi => oi.LineTotal, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("LineTotalAmount")
                 .HasPrecision(18, 2)
                 .IsRequired();
            money.Property(m => m.Currency)
                 .HasColumnName("LineTotalCurrency")
                 .HasMaxLength(3)
                 .IsRequired();
        });

        builder.Property(oi => oi.Quantity)
               .IsRequired();
    }
}