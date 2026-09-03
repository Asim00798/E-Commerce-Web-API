using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.Shared.Communication.Messaging.Decorators;

/// <summary>
/// Restores the correlation ID from an integration event into the logging scope,
/// ensuring that all downstream logs (handlers, composers, transports) automatically
/// carry the same correlation ID.
/// </summary>
public sealed class CorrelationScopeIntegrationEventHandler<TIntegrationEvent>
    : IIntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    private readonly IIntegrationEventHandler<TIntegrationEvent> _inner;
    private readonly ILogger<CorrelationScopeIntegrationEventHandler<TIntegrationEvent>> _logger;

    public CorrelationScopeIntegrationEventHandler(
        IIntegrationEventHandler<TIntegrationEvent> inner,
        ILogger<CorrelationScopeIntegrationEventHandler<TIntegrationEvent>> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task HandleAsync(TIntegrationEvent integrationEvent, CancellationToken ct)
    {
        // Restore the correlation ID into the ambient logging context
        using var scope = BeginCorrelationScope(integrationEvent);

        // Optional: log that a handler is starting – now tagged with the correlation ID
        _logger.LogDebug(
            "Handling integration event {EventType} (Handler: {HandlerType})",
            typeof(TIntegrationEvent).Name,
            _inner.GetType().Name);

        await _inner.HandleAsync(integrationEvent, ct);
    }

    private IDisposable? BeginCorrelationScope(TIntegrationEvent integrationEvent)
    {
        if (string.IsNullOrEmpty(integrationEvent.CorrelationId))
            return null;

        return _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = integrationEvent.CorrelationId
        });
    }
}