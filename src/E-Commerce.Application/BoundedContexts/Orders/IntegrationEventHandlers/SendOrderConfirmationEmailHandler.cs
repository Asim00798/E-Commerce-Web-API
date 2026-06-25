using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;
using E_Commerce.Application.Shared.Abstractions.Persistence;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEventHandlers
{
    public class SendOrderConfirmationEmailHandler : IIntegrationEventHandler<OrderPlacedIntegrationEvent>
    {
        private readonly IEmailService _emailService;   

        public SendOrderConfirmationEmailHandler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task HandleAsync(OrderPlacedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
        {
            // Execute the side effect
            await _emailService.SendOrderConfirmationAsync(integrationEvent.OrderId, integrationEvent.CustomerId);         
        }
    }
}
