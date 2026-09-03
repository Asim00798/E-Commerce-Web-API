using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Enums;
using E_Commerce.Infrastructure.Persistence.Common.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Orders.Configurations;

public sealed class OrderConfiguration : BaseEntityConfiguration<Order>
{
    public override void Configure(EntityTypeBuilder<Order> builder)
    {
        base.Configure(builder);

        builder.ToTable("Orders", "ordering");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerId)
               .IsRequired();

        builder.Property(o => o.Status)
               .HasConversion<string>()
               .IsRequired();

        builder.OwnsOne(o => o.Subtotal, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("SubtotalAmount")
                 .HasPrecision(18, 2)
                 .IsRequired();
            money.Property(m => m.Currency)
                 .HasColumnName("SubtotalCurrency")
                 .HasMaxLength(3)
                 .IsRequired();
        });

        builder.OwnsOne(o => o.ShippingFee, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("ShippingFeeAmount")
                 .HasPrecision(18, 2)
                 .IsRequired();
            money.Property(m => m.Currency)
                 .HasColumnName("ShippingFeeCurrency")
                 .HasMaxLength(3)
                 .IsRequired();
        });

        builder.OwnsOne(o => o.Total, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("TotalAmount")
                 .HasPrecision(18, 2)
                 .IsRequired();
            money.Property(m => m.Currency)
                 .HasColumnName("TotalCurrency")
                 .HasMaxLength(3)
                 .IsRequired();
        });

        builder.Property(o => o.PlacedAtUtc)
               .IsRequired();

        builder.Property(o => o.CancelledAtUtc)
               .IsRequired(false);

        builder.Property(o => o.DeliveredAtUtc)
               .IsRequired(false);

        builder.Property(o => o.RefundedAtUtc)
               .IsRequired(false);

        // One-to-many relationship with OrderItem
        builder.HasMany(o => o.Items)
               .WithOne()
               .HasForeignKey(oi => oi.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        // Optimistic concurrency token (shadow property)
        builder.Property<byte[]>("RowVersion")
               .IsRowVersion()
               .HasColumnName("RowVersion");
    }
}