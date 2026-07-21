using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Services;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEventHandlers
{
    public class SendOrderConfirmationEmailHandler : IIntegrationEventHandler<OrderPlacedIntegrationEvent>
    {
        private readonly IEmailChannel _emailService;   

        public SendOrderConfirmationEmailHandler(IEmailChannel emailService)
        {
            _emailService = emailService;
        }

        public async Task HandleAsync(OrderPlacedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
        {
            // Execute the side effect
            await _emailService.SendAsync(integrationEvent.OrderId, integrationEvent.CustomerId);         
        }
    }
}
