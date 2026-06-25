using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;

namespace E_Commerce.Infrastructure.Persistence.Context
{
    public partial class AppDbContext
    {
        // Messaging / Infrastructure
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();
        public DbSet<DeadLetterMessage> DeadLetterMessages { get; set; }
    }
}
