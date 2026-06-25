using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Outbox.Configurations
{
    public class DeadLetterMessageConfiguration : IEntityTypeConfiguration<DeadLetterMessage>
    {
        public void Configure(EntityTypeBuilder<DeadLetterMessage> builder)
        {
            builder.ToTable("DeadLetterMessages");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.EventType).IsRequired().HasMaxLength(500);
            builder.Property(e => e.Payload).IsRequired();
            builder.Property(e => e.Error).HasMaxLength(4000);
        }
    }
}