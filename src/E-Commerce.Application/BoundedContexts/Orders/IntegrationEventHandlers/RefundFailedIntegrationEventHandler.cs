using E_Commerce.Application.BoundedContexts.Finance.IntegrationEvents;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEventHandlers;

public sealed class RefundFailedIntegrationEventHandler
    : IIntegrationEventHandler<RefundFailedIntegrationEvent>
{
    private readonly ILogger<RefundFailedIntegrationEventHandler> _logger;

    public RefundFailedIntegrationEventHandler(
        ILogger<RefundFailedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(
        RefundFailedIntegrationEvent integrationEvent,
        CancellationToken ct)
    {
        // No state change; log only.
        _logger.LogWarning(
            "Refund failed for payment {PaymentId}. Reason: {Reason}",
            integrationEvent.PaymentId,
            integrationEvent.Reason);

        return Task.CompletedTask;
    }
}