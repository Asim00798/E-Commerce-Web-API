using PaymentAggregate = E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Behaviors.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Finance.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<PaymentAggregate>
{
    public void Configure(EntityTypeBuilder<PaymentAggregate> builder)
    {
        builder.ToTable("Payments", "finance");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId)
            .IsRequired();

        builder.HasIndex(x => x.OrderId)
            .IsUnique();

        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.OwnsOne(x => x.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.OwnsOne(x => x.Method, method =>
        {
            method.Property(m => m.Type)
                .HasColumnName("PaymentMethod")
                .HasConversion<string>()
                .HasMaxLength(50);
        });

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Provider)
            .HasMaxLength(100);

        builder.Property(x => x.ProviderIntentionId)
            .HasMaxLength(255);

        builder.Property(x => x.ProviderTransactionId)
            .HasMaxLength(255);

        builder.Property(x => x.CompletedAtUtc);

        builder.OwnsOne(x => x.RefundedAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("RefundedAmount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("RefundedCurrency").HasMaxLength(3);
        });

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasMany(x => x.Transactions)
            .WithOne()
            .HasForeignKey("PaymentId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Transactions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ProviderIntentionId)
            .IsUnique()
            .HasFilter("[ProviderIntentionId] IS NOT NULL");

        builder.HasIndex(x => x.ProviderTransactionId)
            .IsUnique()
            .HasFilter("[ProviderTransactionId] IS NOT NULL");
    }
}