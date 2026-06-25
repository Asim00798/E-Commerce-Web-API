namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities
{
    public class ProcessedEvent
    {
        public Guid EventId { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.Now;
    }
}
