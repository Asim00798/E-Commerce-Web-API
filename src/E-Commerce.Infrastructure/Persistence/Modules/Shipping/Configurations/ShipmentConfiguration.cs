using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Behaviors;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Shipping.Configurations;

public sealed class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments", "shipping");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId)
            .IsRequired();

        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.OwnsOne(x => x.DeliveryAddress, address =>
        {
            address.Property(a => a.FullName).HasColumnName("DeliveryFullName").HasMaxLength(150).IsRequired();
            address.Property(a => a.PhoneNumber).HasColumnName("DeliveryPhoneNumber").HasMaxLength(30).IsRequired();
            address.Property(a => a.Street).HasColumnName("DeliveryStreet").HasMaxLength(250).IsRequired();
            address.Property(a => a.City).HasColumnName("DeliveryCity").HasMaxLength(100).IsRequired();
            address.Property(a => a.LocationMapUrl).HasColumnName("DeliveryLocationMapUrl").HasMaxLength(500).IsRequired();
        });

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.AssignedDriverId)
            .IsRequired(false);

        builder.Property(x => x.TrackingNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.AssignedAtUtc);
        builder.Property(x => x.ReadyForPickupAtUtc);
        builder.Property(x => x.PickedUpAtUtc);
        builder.Property(x => x.OutForDeliveryAtUtc);
        builder.Property(x => x.DeliveredAtUtc);
        builder.Property(x => x.ReturnedAtUtc);
        builder.Property(x => x.CancelledAtUtc);

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasMany(x => x.DeliveryAttempts)
            .WithOne()
            .HasForeignKey(x => x.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.DeliveryAttempts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.OrderId)
            .IsUnique()
            .HasFilter("[Status] IN (1, 2, 3, 4, 5, 6)"); // one active shipment per order

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.AssignedDriverId);
    }
}