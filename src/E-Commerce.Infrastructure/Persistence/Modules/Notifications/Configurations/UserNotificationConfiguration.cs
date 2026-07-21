using E_Commerce.Infrastructure.Communication.Notifications.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Notifications.Configurations;

public class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.ToTable("UserNotifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Message).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.PayloadJson).HasMaxLength(4000);

        // Prevent duplicate notifications for the same user, event, and type
        builder.HasIndex(x => new { x.UserId, x.SourceEventId, x.Type }).IsUnique();

        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.ReadAtUtc).IsRequired(false);
    }
}
