using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;

namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Contracts;

/// <summary>
/// Defines persistence operations for Outbox messages.
/// 
/// This is the ONLY abstraction allowed to interact with the Outbox table.
/// </summary>
public interface IOutboxMessageRepository
{
    /// <summary>
    /// Inserts a new outbox message into the database.
    /// Must be part of the same transaction as the business operation.
    /// </summary>
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves pending (not processed) outbox messages.
    /// Used by background processor.
    /// </summary>
    Task<List<OutboxMessage>> GetPendingMessagesAsync(int BatchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as successfully processed.
    /// </summary>
    Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as failed and stores error information.
    /// </summary>
    Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as dead‑lettered (poison message)
    /// </summary>
    Task MarkAsDeadLetteredAsync(Guid messageId, CancellationToken cancellationToken = default);
}