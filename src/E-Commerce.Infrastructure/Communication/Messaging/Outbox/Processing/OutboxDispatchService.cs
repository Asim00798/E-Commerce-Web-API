using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;
using E_Commerce.Infrastructure.Communication.Messaging.Serialization;

namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Processing;

/// <summary>
/// Coordinates the deserialization and dispatching of a single
/// <see cref="OutboxMessage"/> to its corresponding integration event
/// handlers.
/// </summary>
/// <remarks>
/// <para>
/// This service is used by the <see cref="HangfireJobBackgroundWorker"/> for each
/// message that is ready to be delivered. It bridges the gap between
/// the raw outbox record and the strongly‑typed integration event
/// handlers.
/// </para>
/// <para>
/// <b>Workflow:</b>
/// <list type="number">
///   <item>Resolve the concrete <see cref="Type"/> of the integration
///         event from the message's <c>EventType</c> string.</item>
///   <item>Deserialize the JSON payload into an
///         <see cref="IIntegrationEvent"/> instance.</item>
///   <item>Forward the event to the
///         <see cref="IIntegrationEventDispatcher"/> so that all
///         registered handlers can react.</item>
/// </list>
/// </para>
/// </remarks>
public class OutboxDispatchService
{
    private readonly IIntegrationEventDispatcher _dispatcher;
    private readonly OutboxSerializer _serializer;

    /// <summary>
    /// Initialises a new instance of the <see cref="OutboxDispatchService"/>.
    /// </summary>
    /// <param name="dispatcher">
    /// The dispatcher that will fan‑out the integration event to all handlers.
    /// </param>
    /// <param name="serializer">
    /// The JSON serializer for converting the outbox payload back into an
    /// <see cref="IIntegrationEvent"/> object.
    /// </param>
    public OutboxDispatchService(
        IIntegrationEventDispatcher dispatcher,
        OutboxSerializer serializer)
    {
        _dispatcher = dispatcher;
        _serializer = serializer;
    }

    /// <summary>
    /// Deserializes the given outbox message and dispatches the resulting
    /// integration event to all registered handlers.
    /// </summary>
    /// <param name="message">
    /// The outbox message containing the <c>EventType</c> and serialized
    /// payload.
    /// </param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the event type stored in the message cannot be resolved.
    /// </exception>
    public async Task DispatchMessageAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        // Resolve the runtime Type from the fully qualified type name
        var eventType = Type.GetType(message.EventType);
        if (eventType is null)
            throw new InvalidOperationException($"Unknown event type: {message.EventType}");

        // Deserialize the payload to the concrete integration event
        var integrationEvent = (IIntegrationEvent)_serializer.Deserialize(message.Payload, eventType)!;

        // Fan out to all handlers
        await _dispatcher.DispatchAsync(integrationEvent, cancellationToken);
    }
}