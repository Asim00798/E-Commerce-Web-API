namespace E_Commerce.Infrastructure.Messaging.EventBus;

/// <summary>
/// Abstraction for publishing integration events to the event bus.
/// </summary>
public interface IEventBus
{
    /// <summary>Publishes an integration event asynchronously.</summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class;
}
