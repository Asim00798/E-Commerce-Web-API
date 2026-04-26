using MediatR;
using Microsoft.Extensions.Logging;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Events;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.EventHandlers;

public class CategoryCreatedDomainEventHandler(ILogger<CategoryCreatedDomainEventHandler> logger) : INotificationHandler<CategoryCreatedDomainEvent>
{
    public async Task Handle(CategoryCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Category {CategoryId} ({Name}) was created. Invalidating navigation menus...", 
            notification.CategoryId, notification.Name);
            
        // Example: await _cacheService.InvalidateAsync("navigation_menu");
        
        await Task.CompletedTask;
    }
}
