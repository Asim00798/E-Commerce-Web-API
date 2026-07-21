using E_Commerce.Infrastructure.Communication.Notifications.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Notifications.Configurations;

public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("NotificationLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventId).IsRequired();
        builder.Property(x => x.Channel).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Provider).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ProviderMessageId).HasMaxLength(200);
        builder.Property(x => x.Recipient).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.FailureReason).HasMaxLength(2000);
        builder.Property(x => x.OccurredAt).IsRequired();
        builder.Property(x => x.CompletedAt).IsRequired(false);
    }
}
