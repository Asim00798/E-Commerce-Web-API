using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Shipping.Configurations;

public sealed class DeliveryAttemptConfiguration : IEntityTypeConfiguration<DeliveryAttempt>
{
    public void Configure(EntityTypeBuilder<DeliveryAttempt> builder)
    {
        builder.ToTable("DeliveryAttempts", "shipping");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ShipmentId)
            .IsRequired();

        builder.Property(x => x.AttemptNumber)
            .IsRequired();

        builder.Property(x => x.AttemptedAtUtc)
            .IsRequired();

        builder.Property(x => x.Result)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.FailureReason)
            .HasMaxLength(250);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.ShipmentId);
    }
}