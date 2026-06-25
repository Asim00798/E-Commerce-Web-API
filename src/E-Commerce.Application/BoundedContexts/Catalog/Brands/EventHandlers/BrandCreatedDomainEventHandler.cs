#if false

using MediatR;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.EventHandlers;

public class BrandCreatedDomainEventHandler(ILogger<BrandCreatedDomainEventHandler> logger) : INotificationHandler<BrandCreatedDomainEvent>
{
    public async Task Handle(BrandCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Brand {BrandId} ({Name}) was created. Updating brand cache...", 
            notification.BrandId, notification.Name);
            
        // Example: await _cacheService.InvalidateAsync("brands_list");
        
        await Task.CompletedTask;
    }
}

#endif