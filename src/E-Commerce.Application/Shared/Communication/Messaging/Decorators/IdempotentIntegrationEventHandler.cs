using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Persistence;  // IProcessedEventRepository

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

    public IdempotentIntegrationEventHandler(
        IIntegrationEventHandler<TIntegrationEvent> inner,
        IProcessedEventRepository processedEvents)
    {
        _inner = inner;
        _processedEvents = processedEvents;
    }

    public async Task HandleAsync(
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        // 1. Skip if already processed
        if (await _processedEvents.IsProcessedAsync(integrationEvent.EventId, cancellationToken))
            return;

        // 2. Execute the actual business logic (the “side effect”)
        await _inner.HandleAsync(integrationEvent, cancellationToken);

        // 3. Mark as processed so that future retries are skipped
        await _processedEvents.MarkAsProcessedAsync(integrationEvent.EventId, cancellationToken);
    }
}