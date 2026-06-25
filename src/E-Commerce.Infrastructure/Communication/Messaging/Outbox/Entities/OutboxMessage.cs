
namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;

/// <summary>
/// Represents a durable integration event stored in the Outbox table.
/// 
/// This ensures that any domain-triggered integration event is not lost
/// even if the application crashes after the transaction commits.
/// </summary>
public class OutboxMessage
{
    /// <summary>
    /// Unique identifier for the outbox message.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Full CLR type name of the integration event.
    /// Used by the OutboxProcessor to deserialize the payload.
    /// </summary>
    public string EventType { get; set; } = default!;

    /// <summary>
    /// Serialized JSON payload of the integration event.
    /// This is what gets dispatched later by the processor.
    /// </summary>
    public string Payload { get; set; } = default!;

    /// <summary>
    /// When the event was created inside the same transaction.
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// When the event was successfully processed.
    /// Null = not processed yet.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Number of retry attempts.
    /// Used for exponential backoff or failure tracking.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Last error message if processing failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Marks whether this message has been successfully processed.
    /// </summary>
    public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;
}

public enum OutboxMessageStatus
{
    Pending,
    Processing,
    Processed,
    Failed,
    DeadLettered
}