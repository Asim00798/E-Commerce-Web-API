using MediatR;
using Microsoft.Extensions.Logging;
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Application.Common.EventHandlers;

public class LoggingDomainEventHandler(ILogger<LoggingDomainEventHandler> logger) : INotificationHandler<DomainEvent>
{
    public Task Handle(DomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain Event: {EventName} occurred at {OccurredOn}", 
            notification.GetType().Name, notification.OccurredOn);
            
        return Task.CompletedTask;
    }
}
