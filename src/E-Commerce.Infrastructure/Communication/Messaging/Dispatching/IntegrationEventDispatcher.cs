using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Infrastructure.Abstractions;

namespace E_Commerce.Infrastructure.Communication.Messaging.Dispatching;

/// <summary>
/// Implements the integration event dispatcher, responsible for delivering
/// durable integration events (retrieved from the Outbox) to all registered
/// handlers in a background, asynchronous context.
/// </summary>
/// <remarks>
/// <para>
/// This class is used exclusively by the <see cref="OutboxDispatchService"/>
/// (or equivalent background processing component) after an
/// <see cref="OutboxMessage"/> has been deserialized back into an
/// <see cref="IIntegrationEvent"/>.
/// </para>
/// <para>
/// <b>Key characteristics:</b>
/// <list type="bullet">
///   <item>Operates outside the original business transaction – it is
///         part of the background execution world.</item>
///   <item>Resolves all <c>IIntegrationEventHandler&lt;T&gt;</c>
///         implementations for the concrete event type via reflection.</item>
///   <item>Supports multiple handlers per integration event (fan‑out).</item>
///   <item>Handlers must be idempotent; this dispatcher provides
///         at‑least‑once delivery semantics.</item>
/// </list>
/// </para>
/// </remarks>
public class IntegrationEventDispatcher : IIntegrationEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initialises the dispatcher with the root service provider.
    /// </summary>
    /// <param name="serviceProvider">
    /// The application's DI container, used to dynamically resolve
    /// <c>IIntegrationEventHandler&lt;T&gt;</c> implementations.
    /// </param>
    public IntegrationEventDispatcher(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    /// <inheritdoc />
    public async Task DispatchAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        // Build the closed generic handler type, e.g.
        // IIntegrationEventHandler<OrderPlacedIntegrationEvent>
        var handlerType = typeof(IIntegrationEventHandler<>)
            .MakeGenericType(integrationEvent.GetType());

        // Retrieve all handler implementations registered in DI
        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            // Invoke the handler's HandleAsync method via reflection
            // and await the resulting Task
            await (Task)handlerType
                .GetMethod(nameof(IIntegrationEventHandler<IIntegrationEvent>.HandleAsync))!
                .Invoke(handler, new object[] { integrationEvent, cancellationToken })!;
        }
    }
}