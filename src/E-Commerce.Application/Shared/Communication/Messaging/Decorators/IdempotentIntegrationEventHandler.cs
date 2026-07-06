using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.Shared.Communication.Messaging.Decorators;
/// <summary>
/// Decorates any <see cref="IIntegrationEventHandler{T}"/> with idempotency
/// using <see cref="IProcessedEventRepository"/>.  Checks whether the event
/// has already been handled before invoking the inner handler, and marks it
/// as processed afterwards.
/// </summary>
/// <typeparam name="TIntegrationEvent">Concrete integration event type.</typeparam>
public sealed class IdempotentIntegrationEventHandler<TIntegrationEvent>
    : IIntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    private readonly IIntegrationEventHandler<TIntegrationEvent> _inner;
    private readonly IProcessedEventRepository _processedEvents;
    private readonly string _handlerId;

    public IdempotentIntegrationEventHandler(
        IIntegrationEventHandler<TIntegrationEvent> inner,
        IProcessedEventRepository processedEvents)
    {
        _inner = inner;
        _processedEvents = processedEvents;
        _handlerId = inner.GetType().FullName!;   // unique per handler class
    }

    public async Task HandleAsync(TIntegrationEvent integrationEvent, CancellationToken ct)
    {
        // Check if the event has already been processed by this handler
        if (await _processedEvents.IsProcessedAsync(integrationEvent.EventId, _handlerId, ct))
            return;
        // Invoke the inner handler
        await _inner.HandleAsync(integrationEvent, ct);
        // Mark the evnt as processed by this handler
        await _processedEvents.MarkAsProcessedAsync(integrationEvent.EventId, _handlerId, ct);
    }
}