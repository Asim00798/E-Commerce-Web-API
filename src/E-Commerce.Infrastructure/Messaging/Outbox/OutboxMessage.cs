namespace E_Commerce.Infrastructure.Messaging.Outbox;

/// <summary>
/// Persistent outbox message stored in the database before publication.
/// Guarantees at-least-once delivery of integration events.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset OccurredOnUtc { get; set; }
    public DateTimeOffset? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
}
