using MediatR;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Catalog.IntegrationEventHandlers;

// Mocking the event structure for demonstration as per instructions
public record OrderPlacedIntegrationEvent(Guid OrderId, List<(Guid ProductId, int Quantity)> Items) : INotification;

public class OrderPlacedIntegrationEventHandler(ILogger<OrderPlacedIntegrationEventHandler> logger) 
    : INotificationHandler<OrderPlacedIntegrationEvent>
{
    public Task Handle(OrderPlacedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Integration Event: Order {OrderId} placed. Updating product popularity counters...", notification.OrderId);
        
        // In a real scenario, we might call an application service to increment popularity scores
        // or reserve stock (though stock is usually in Inventory context).
        
        return Task.CompletedTask;
    }
}
