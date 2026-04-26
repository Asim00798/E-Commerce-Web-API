using MediatR;
using Microsoft.Extensions.Logging;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Events;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.EventHandlers;

public class ProductPublishedDomainEventHandler(ILogger<ProductPublishedDomainEventHandler> logger) : INotificationHandler<ProductPublishedDomainEvent>
{
    public async Task Handle(ProductPublishedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Product {ProductId} was published. Updating search index...", notification.ProductId);
        
        // Example: await _searchService.UpdateIndexAsync(notification.ProductId, cancellationToken);
        
        await Task.CompletedTask;
    }
}
