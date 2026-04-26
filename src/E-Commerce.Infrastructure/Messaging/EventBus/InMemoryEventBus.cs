namespace E_Commerce.Infrastructure.Messaging.EventBus;

/// <summary>
/// In-process event bus implementation backed by MediatR.
/// Suitable for a monolith / modular-monolith; swap for a real broker in production.
/// </summary>
public sealed class InMemoryEventBus : IEventBus
{
    // TODO: Inject IMediator from MediatR

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        // TODO: Publish via IMediator.Publish
        throw new NotImplementedException();
    }
}
