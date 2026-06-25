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

    public async Task ExecuteAsync()
    {
        const int batchSize = 20;
        var messages = await _outboxRepo.GetPendingMessagesAsync(batchSize, CancellationToken.None);

        foreach (var message in messages)
        {
            // Poison message detection
            if (message.RetryCount >= _options.MaxRetryCount)
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
                continue;
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
    }
    public class OutboxOptions
    {
        public int MaxRetryCount { get; set; } = 5;
    }
}