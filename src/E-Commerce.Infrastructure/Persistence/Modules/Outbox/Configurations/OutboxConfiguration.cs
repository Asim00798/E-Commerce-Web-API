using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace E_Commerce.Infrastructure.Persistence.Modules.Outbox.Configurations
{
    public class OutboxConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.EventType).IsRequired();
            builder.Property(m => m.Payload).IsRequired();
            builder.Property(m => m.Status).HasConversion<string>();
            builder.HasKey(m => m.Id);
            builder.Property(m => m.EventType).IsRequired();
            builder.Property(m => m.Payload).IsRequired();
            builder.Property(m => m.Status).HasConversion<string>();
        }
    }
}
