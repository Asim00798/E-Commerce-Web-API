using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Outbox.Configurations;

public class OutboxConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.EventType)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Payload)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(m => m.Error)
            .HasMaxLength(4000);

        builder.Property(m => m.Status)
            .HasConversion<string>()
            .IsRequired();

        // Supports the batch fetch:
        //   WHERE Status IN (Pending, Failed) ORDER BY OccurredAt
        // Without this index, every outbox cycle performs a full table scan,
        // which degrades as processed rows accumulate.
        builder.HasIndex(m => new { m.Status, m.OccurredAt })
            .HasDatabaseName("IX_OutboxMessages_Status_OccurredAt");
    }
}