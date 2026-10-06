using E_Commerce.Application.Modules.Scheduling.Abstractions;

namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Jobs.OutboxProcessing;

/// <summary>
/// Background job that processes pending outbox messages in batches and
/// moves poison messages to the dead-letter table.
/// </summary>
public sealed record OutboxProcessingJobPayload : IJob;