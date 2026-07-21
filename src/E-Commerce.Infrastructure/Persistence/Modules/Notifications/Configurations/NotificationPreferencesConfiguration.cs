using E_Commerce.Infrastructure.Communication.Notifications.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Notifications.Configurations;

internal class NotificationPreferencesConfiguration : IEntityTypeConfiguration<NotificationPreferences>
{
    public void Configure(EntityTypeBuilder<NotificationPreferences> builder)
    {
        builder.ToTable("NotificationPreferences");

        builder.HasKey(p => p.UserId);
        builder.Property(p => p.UserId)
               .ValueGeneratedNever();            // UserId is assigned, not auto‑generated

        builder.Property(p => p.AllowEmail)
               .IsRequired()
               .HasDefaultValue(true);

        builder.Property(p => p.AllowSms)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(p => p.AllowPush)
               .IsRequired()
               .HasDefaultValue(false);
    }
}