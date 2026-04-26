namespace E_Commerce.Infrastructure.Jobs;

/// <summary>
/// Background job that reads pending outbox messages and publishes them to the event bus.
/// Guarantees at-least-once delivery of integration events.
/// </summary>
public sealed class PublishOutboxMessagesJob
{
    // TODO: Inject OutboxDbContext and IEventBus
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Fetch unprocessed outbox messages, publish, mark as sent
        throw new NotImplementedException();
    }
}
