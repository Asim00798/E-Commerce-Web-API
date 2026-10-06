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
        // Ordering does not change state on refund failure.
        // Log with order context for operational visibility.
        _logger.LogWarning(
            "Refund {RefundId} failed for order {OrderId}, payment {PaymentId}: {Reason}",
            integrationEvent.RefundId,
            integrationEvent.OrderId,
            integrationEvent.PaymentId,
            integrationEvent.Reason);

        return Task.CompletedTask;
    }
}