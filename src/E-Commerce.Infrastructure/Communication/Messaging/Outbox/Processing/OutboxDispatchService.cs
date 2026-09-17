using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Serialization;

namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Processing;

public class OutboxDispatchService
{
    private readonly IIntegrationEventDispatcher _dispatcher;
    private readonly OutboxSerializer _serializer;

    public OutboxDispatchService(
        IIntegrationEventDispatcher dispatcher,
        OutboxSerializer serializer)
    {
        _dispatcher = dispatcher;
        _serializer = serializer;
    }

    public async Task DispatchMessageAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var eventType = ResolveEventType(message.EventType);

        if (eventType is null)
            throw new InvalidOperationException(
                $"Unknown event type: {message.EventType}");

        var integrationEvent = (IIntegrationEvent)_serializer
            .Deserialize(message.Payload, eventType)!;

        await _dispatcher.DispatchAsync(integrationEvent, cancellationToken);
    }

    /// <summary>
    /// Resolves the CLR type of an integration event from its stored name.
    ///
    /// 1. Type.GetType — fast path, works for assembly-qualified names.
    /// 2. Assembly scan — fallback for older rows written with a plain
    ///    full name, and for types that moved assemblies.
    /// </summary>
    private static Type? ResolveEventType(string typeName)
    {
        var direct = Type.GetType(typeName);
        if (direct is not null)
            return direct;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var fromAssembly = assembly.GetType(typeName);
            if (fromAssembly is not null)
                return fromAssembly;
        }

        return null;
    }
}