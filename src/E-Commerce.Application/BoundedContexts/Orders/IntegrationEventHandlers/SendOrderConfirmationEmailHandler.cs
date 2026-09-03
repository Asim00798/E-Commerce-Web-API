using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;
using E_Commerce.Application.BoundedContexts.Orders.Models;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Channels;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEventHandlers;

public class SendOrderConfirmationEmailHandler : IIntegrationEventHandler<OrderPlacedIntegrationEvent>
{
    private readonly IEmailChannel _emailChannel;

    public SendOrderConfirmationEmailHandler(IEmailChannel emailChannel)
    {
        _emailChannel = emailChannel;
    }

    public async Task HandleAsync(OrderPlacedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        var model = new OrderConfirmationEmail
        {
            RecipientEmail = integrationEvent.CustomerEmail,
            CustomerName = integrationEvent.CustomerName,
            OrderId = integrationEvent.OrderId,
            Total = integrationEvent.TotalAmount
        };

        await _emailChannel.SendAsync(new NotificationRequest<OrderConfirmationEmail>
        {
            UserId = integrationEvent.CustomerId,
            Model = model
        }, cancellationToken);
    }
}