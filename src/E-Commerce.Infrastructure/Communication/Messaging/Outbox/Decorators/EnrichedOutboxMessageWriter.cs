using E_Commerce.Application.Shared.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Decorators;

/// <summary>
/// Decorates <see cref="IOutboxMessageWriter"/> to add consistent structured logging
/// and correlation enrichment to every integration event published through the Outbox.
/// This applies to both domain‑originated and application‑originated events.
/// </summary>
internal sealed class EnrichedOutboxMessageWriter : IOutboxMessageWriter
{
    private readonly IOutboxMessageWriter _inner;
    private readonly ILogger<EnrichedOutboxMessageWriter> _logger;
    private readonly IAppContext _appContext;

    public EnrichedOutboxMessageWriter(
        IOutboxMessageWriter inner,
        ILogger<EnrichedOutboxMessageWriter> logger,
        IAppContext appContext)
    {
        _inner = inner;
        _logger = logger;
        _appContext = appContext;
    }

    public async Task WriteAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        var correlationId = _appContext.CorrelationId ?? "N/A";

        _logger.LogInformation(
            "Publishing integration event {EventType} with CorrelationId {CorrelationId}",
            integrationEvent.GetType().Name,
            correlationId);

        await _inner.WriteAsync(integrationEvent, cancellationToken);
    }
}