using MediatR;
using Microsoft.Extensions.Logging;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Events;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.EventHandlers;

public class ProductCreatedDomainEventHandler(ILogger<ProductCreatedDomainEventHandler> logger) : INotificationHandler<ProductCreatedDomainEvent>
{
    public async Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Product {ProductId} ({Name}) was created. Triggering initial processing...", 
            notification.ProductId, notification.Name);
            
        // Example: Send welcome/notification email to the owner
        // await _emailService.SendAsync(notification.OwnerEmail, "New Product Created", ...);
        
        await Task.CompletedTask;
    }
}
