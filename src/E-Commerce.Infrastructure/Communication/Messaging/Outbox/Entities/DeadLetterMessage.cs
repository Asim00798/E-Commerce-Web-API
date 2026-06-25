namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities
{
    public class DeadLetterMessage
    {
        public Guid Id { get; set; }
        public Guid OriginalMessageId { get; set; }       // FK to OutboxMessage (not enforced as constraint, kept for traceability)
        public string EventType { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public string? Error { get; set; }
        public int RetryCount { get; set; }
        public DateTime DeadLetteredAt { get; set; }
        public DeadLetterStatus Status { get; set; } = DeadLetterStatus.DeadLettered;
    }

    public enum DeadLetterStatus
    {
        DeadLettered,
        Reprocessing
    }
}