namespace E_Commerce.Infrastructure.Messaging.Outbox;

/// <summary>
/// Service responsible for reading unprocessed outbox messages and publishing them
/// to the event bus. Invoked by <see cref="Jobs.PublishOutboxMessagesJob"/>.
/// </summary>
public sealed class OutboxProcessor
{
    private readonly OutboxDbContext _dbContext;
    private readonly EventBus.IEventBus _eventBus;

    public OutboxProcessor(OutboxDbContext dbContext, EventBus.IEventBus eventBus)
    {
        _dbContext = dbContext;
        _eventBus = eventBus;
    }

    public Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Fetch pending messages, deserialise, publish, mark as processed
        throw new NotImplementedException();
    }
}
