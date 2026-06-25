using E_Commerce.Infrastructure.Abstractions;
using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEventHandlers
{
    public class UpdateInventoryHandler : IIntegrationEventHandler<OrderPlacedIntegrationEvent>
    {
        public Task HandleAsync(OrderPlacedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
        {
            // Placeholder: integrate with inventory system
            return Task.CompletedTask;
        }
    }
}
