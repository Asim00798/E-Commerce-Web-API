using MediatR;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Catalog.IntegrationEventHandlers;

// Mocking the event structure for demonstration as per instructions
public record OrderCancelledIntegrationEvent(Guid OrderId, List<(Guid ProductId, int Quantity)> Items) : INotification;

public class OrderCancelledIntegrationEventHandler(ILogger<OrderCancelledIntegrationEventHandler> logger) 
    : INotificationHandler<OrderCancelledIntegrationEvent>
{
    public Task Handle(OrderCancelledIntegrationEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Integration Event: Order {OrderId} cancelled. Reverting product popularity adjustments...", notification.OrderId);
        
        return Task.CompletedTask;
    }
}
