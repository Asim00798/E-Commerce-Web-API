using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Configuration;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Contracts;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Processing;
using E_Commerce.Infrastructure.Persistence.Context;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Scheduling.Hangfire;

/// <summary>
/// Hangfire recurring job that processes pending outbox messages.
/// Moves messages that exceed the configured retry limit to the dead-letter table.
///
/// Concurrency: the job is protected by <see cref="DisableConcurrentExecutionAttribute"/>
/// so only one instance runs at a time per Hangfire server. Multi-server deployments
/// require the fetch/claim semantics on the outbox table to prevent two servers
/// from picking up the same message.
///
/// Cancellation: the job respects Hangfire's shutdown token. Cancellation is not
/// treated as a handler failure — the in-flight message is left in its current
/// state and will be retried on the next cycle.
/// </summary>
public sealed class OutboxProcessingJob
{
    private const int BatchSize = 20;

    private readonly IOutboxMessageRepository _outboxRepo;
    private readonly OutboxDispatchService _dispatchService;
    private readonly IDeadLetterRepository _deadLetterRepo;
    private readonly AppDbContext _dbContext;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxProcessingJob> _logger;

    public OutboxProcessingJob(
        IOutboxMessageRepository outboxRepo,
        OutboxDispatchService dispatchService,
        IDeadLetterRepository deadLetterRepo,
        AppDbContext dbContext,
        IOptions<OutboxOptions> options,
        ILogger<OutboxProcessingJob> logger)
    {
        _outboxRepo = outboxRepo;
        _dispatchService = dispatchService;
        _deadLetterRepo = deadLetterRepo;
        _dbContext = dbContext;
        _options = options.Value;
        _logger = logger;
    }

    [DisableConcurrentExecution(timeoutInSeconds: 120)]
    public async Task ExecuteAsync(IJobCancellationToken cancellationToken)
    {
        var ct = cancellationToken.ShutdownToken;

        var messages = await _outboxRepo.GetPendingMessagesAsync(BatchSize, ct);
        if (messages.Count == 0)
            return;

        var processed = 0;
        var failed = 0;
        var deadLettered = 0;

        foreach (var message in messages)
        {
            ct.ThrowIfCancellationRequested();

            var outcome = await ProcessMessageAsync(message, ct);

            switch (outcome)
            {
                case ProcessOutcome.Processed: processed++; break;
                case ProcessOutcome.Failed: failed++; break;
                case ProcessOutcome.DeadLettered: deadLettered++; break;
            }
        }

        _logger.LogInformation(
            "Outbox batch complete: {Processed} processed, {Failed} failed, {DeadLettered} dead-lettered",
            processed,
            failed,
            deadLettered);
    }

    // ------------------------------------------------------------------
    // Single message processing
    // ------------------------------------------------------------------

    private async Task<ProcessOutcome> ProcessMessageAsync(
        OutboxMessage message,
        CancellationToken ct)
    {
        if (IsPoisonMessage(message))
        {
            await TryMoveToDeadLetterAsync(message, ct);
            return ProcessOutcome.DeadLettered;
        }

        try
        {
            await _dispatchService.DispatchMessageAsync(message, ct);
            await _outboxRepo.MarkAsProcessedAsync(message.Id, ct);
            return ProcessOutcome.Processed;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Shutdown requested — let Hangfire handle cancellation.
            // The message stays in its current state and will be retried.
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);

            try
            {
                await _outboxRepo.MarkAsFailedAsync(message.Id, ex.ToString(), ct);
            }
            catch (Exception markEx)
            {
                // A failure to record the failure must not abort the batch.
                _logger.LogError(
                    markEx,
                    "Failed to mark outbox message {MessageId} as failed",
                    message.Id);
            }

            return ProcessOutcome.Failed;
        }
    }

    private bool IsPoisonMessage(OutboxMessage message)
        => message.RetryCount >= _options.MaxRetryCount;

    // ------------------------------------------------------------------
    // Dead letter handling
    // ------------------------------------------------------------------

    /// <summary>
    /// Moves a poison message to the dead-letter table and marks the original
    /// outbox row as dead-lettered.
    ///
    /// The two writes are wrapped in an explicit transaction so that a crash
    /// between them cannot leave an orphaned dead-letter row (which would
    /// cause duplicates on the next cycle) or a status update without a
    /// corresponding dead-letter record.
    /// </summary>
    private async Task TryMoveToDeadLetterAsync(OutboxMessage message, CancellationToken ct)
    {
        _logger.LogWarning(
            "Moving outbox message {MessageId} to dead-letter after {RetryCount} retries",
            message.Id,
            message.RetryCount);

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

        await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            await _deadLetterRepo.AddAsync(deadLetter, ct);
            await _outboxRepo.MarkAsDeadLetteredAsync(message.Id, ct);
            await tx.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);

            _logger.LogError(
                ex,
                "Failed to move outbox message {MessageId} to dead-letter; " +
                "the message will be retried on the next cycle",
                message.Id);
        }
    }

    private enum ProcessOutcome
    {
        Processed,
        Failed,
        DeadLettered
    }
}