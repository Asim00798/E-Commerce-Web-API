using E_Commerce.Infrastructure.Communication.Notifications.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Notifications.Configurations;

public class PushDeviceConfiguration : IEntityTypeConfiguration<PushDevice>
{
    public void Configure(EntityTypeBuilder<PushDevice> builder)
    {
        builder.ToTable("PushDevices");
        builder.HasKey(d => d.Id);
        builder.HasIndex(d => d.UserId);
        builder.Property(d => d.FirebaseInstallationId).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Platform)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();
        builder.Property(d => d.IsActive).IsRequired();
    }
}