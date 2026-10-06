using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Entities;
using E_Commerce.Infrastructure.Persistence.Common.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Finance.Configurations;

public sealed class PaymentTransactionConfiguration : BaseEntityConfiguration<PaymentTransaction>
{
    public override void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        base.Configure(builder);

        builder.ToTable("PaymentTransactions", "finance");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.OwnsOne(x => x.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.Property(x => x.OccurredAtUtc)
            .IsRequired();

        builder.Property(x => x.ProviderTransactionId)
            .HasMaxLength(255);
    }
}