using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Contracts;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Processing;
using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Scheduling.Hangfire;

/// <summary>
/// Hangfire recurring job that processes pending outbox messages.
/// Moves messages that exceed the configured retry limit to a dead‑letter table.
/// Replaces the continuous <see cref="OutboxProcessor"/> loop.
/// </summary>
public class OutboxProcessingJob
{
    private readonly IOutboxMessageRepository _outboxRepo;
    private readonly OutboxDispatchService _dispatchService;
    private readonly IDeadLetterRepository _deadLetterRepo;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxProcessingJob> _logger;

    public OutboxProcessingJob(
        IOutboxMessageRepository outboxRepo,
        OutboxDispatchService dispatchService,
        IDeadLetterRepository deadLetterRepo,
        IOptions<OutboxOptions> options,
        ILogger<OutboxProcessingJob> logger)
    {
        _outboxRepo = outboxRepo;
        _dispatchService = dispatchService;
        _deadLetterRepo = deadLetterRepo;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Executes the outbox processing logic:
    /// fetches pending messages, dispatches them, and moves poison messages
    /// to the dead‑letter table.
    /// </summary>
    public async Task ExecuteAsync()
    {
        const int batchSize = 20;
        var messages = await _outboxRepo.GetPendingMessagesAsync(batchSize, CancellationToken.None);

        foreach (var message in messages)
        {
            await ProcessMessageAsync(message);
        }
    }

    /// <summary>
    /// Processes a single outbox message:
    /// - Moves to dead‑letter if retry limit exceeded.
    /// - Otherwise dispatches and updates status accordingly.
    /// </summary>
    private async Task ProcessMessageAsync(OutboxMessage message)
    {
        if (IsPoisonMessage(message))
        {
            await MoveToDeadLetterAsync(message);
            return;
        }

        try
        {
            await _dispatchService.DispatchMessageAsync(message, CancellationToken.None);
            await _outboxRepo.MarkAsProcessedAsync(message.Id, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);
            await _outboxRepo.MarkAsFailedAsync(message.Id, ex.ToString(), CancellationToken.None);
        }
    }

    /// <summary>
    /// Determines whether a message has reached the maximum retry count and should be dead‑lettered.
    /// </summary>
    private bool IsPoisonMessage(OutboxMessage message)
    {
        return message.RetryCount >= _options.MaxRetryCount;
    }

    /// <summary>
    /// Creates a dead‑letter record for the given message and marks the original
    /// outbox message as dead‑lettered so it won't be processed again.
    /// </summary>
    private async Task MoveToDeadLetterAsync(OutboxMessage message)
    {
        _logger.LogWarning(
            "Moving outbox message {MessageId} to dead‑letter after {RetryCount} retries",
            message.Id, message.RetryCount);

        var deadLetter = new DeadLetterMessage
        {
            Id = Guid.NewGuid(),
            OriginalMessageId = message.Id,
            EventType = message.EventType,
            Payload = message.Payload,
            Error = message.Error,
            RetryCount = message.RetryCount,
            DeadLetteredAt = DateTime.UtcNow,
            Status = DeadLetterStatus.DeadLettered
        };

        await _deadLetterRepo.AddAsync(deadLetter, CancellationToken.None);
        await _outboxRepo.MarkAsDeadLetteredAsync(message.Id, CancellationToken.None);
    }

    public class OutboxOptions
    {
        public int MaxRetryCount { get; set; } = 5;
    }
}