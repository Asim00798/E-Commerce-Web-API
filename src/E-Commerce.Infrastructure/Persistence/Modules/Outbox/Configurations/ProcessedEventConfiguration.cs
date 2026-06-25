using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Outbox.Configurations
{
    public class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
    {
        public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
        {
            builder.HasKey(e => e.EventId);
            builder.Property(e => e.ProcessedAt).IsRequired();
        }
    }
}
