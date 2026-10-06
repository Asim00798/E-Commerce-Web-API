using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Behaviors;
using E_Commerce.Infrastructure.Persistence.Common.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Finance.Configurations;

public sealed class RefundConfiguration : BaseEntityConfiguration<Refund>
{
    public override void Configure(EntityTypeBuilder<Refund> builder)
    {
        base.Configure(builder);

        builder.ToTable("Refunds", "finance");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PaymentId)
            .IsRequired();

        builder.Property(x => x.OrderId)
            .IsRequired();

        builder.OwnsOne(x => x.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3);
        });

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Reason)
            .HasMaxLength(1000);

        builder.Property(x => x.RequestedAtUtc)
            .IsRequired();

        builder.Property(x => x.CompletedAtUtc);

        builder.Property(x => x.ProviderTransactionId)
            .HasMaxLength(255);

        // RowVersion is a shadow property — Domain does not carry it.
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion()
            .HasColumnName("RowVersion")
            .IsRequired();

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.PaymentId);

        // NOTE: The unique index on (PaymentId, Amount, Currency) filtered to
        // non-Failed refunds cannot be declared here. EF Core does not support
        // composite indexes that mix owner properties with owned-type
        // properties. The index is created in the migration's Up() method
        // instead. See FinanceRefundOrderIdAndUniqueIndex migration.
    }
}