using Domain.SharedKernel.Events;
using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Persistence;
using E_Commerce.Application.Shared.Communication.PostCommit;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Events.Order;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.Modules.Notifications.Handlers;

/// <summary>
/// Listens to <see cref="OrderPlacedDomainEvent"/> and creates a durable in‑app notification
/// for the user, then enqueues a post‑commit SignalR callback to instantly notify connected clients.
/// </summary>
public sealed class CreateUserNotificationHandler : IDomainEventHandler<OrderPlacedDomainEvent>
{
    private readonly IUserNotificationRepository _notificationRepo;
    private readonly IPostCommitProcessor _postCommitProcessor;
    private readonly ILogger<CreateUserNotificationHandler> _logger;

    public CreateUserNotificationHandler(
        IUserNotificationRepository notificationRepo,
        IPostCommitProcessor postCommitProcessor,
        ILogger<CreateUserNotificationHandler> logger)
    {
        _notificationRepo = notificationRepo;
        _postCommitProcessor = postCommitProcessor;
        _logger = logger;
    }

    public async Task Handle(OrderPlacedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var dto = BuildNotificationDto(domainEvent);
        await _notificationRepo.AddAsync(dto, cancellationToken);
        EnqueueSignalRHint(domainEvent, dto, cancellationToken);
        LogNotificationCreated(dto);
    }

    /// <summary>
    /// Builds the application‑layer DTO from the domain event data.
    /// </summary>
    private static UserNotificationDto BuildNotificationDto(OrderPlacedDomainEvent domainEvent)
    {
        return new UserNotificationDto
        {
            Id = Guid.NewGuid(),
            UserId = domainEvent.CustomerId,
            Type = "OrderPlaced",
            Title = "Order Confirmed",
            Message = $"Your order #{domainEvent.OrderId} has been placed successfully.",
            SourceEventId = domainEvent.EventId,
            CreatedAtUtc = domainEvent.OccurredAt,
            IsRead = false
        };
    }

    /// <summary>
    /// Enqueues a post‑commit callback that publishes a real‑time hint via SignalR.
    /// </summary>
    private void EnqueueSignalRHint(
        OrderPlacedDomainEvent domainEvent,
        UserNotificationDto dto,
        CancellationToken cancellationToken)
    {
        _postCommitProcessor.Enqueue(async serviceProvider =>
        {
            var publisher = serviceProvider.GetRequiredService<IRealtimeEventPublisher>();
            await publisher.PublishAsync(new RealTimeMessage
            {
                UserId = domainEvent.CustomerId,
                Method = "NotificationAvailable",
                Payload = new
                {
                    notificationId = dto.Id,
                    type = dto.Type,
                    title = dto.Title
                }
            }, cancellationToken);
        });
    }

    /// <summary>
    /// Logs the creation of the in‑app notification.
    /// </summary>
    private void LogNotificationCreated(UserNotificationDto dto)
    {
        _logger.LogDebug("In‑app notification {NotificationId} for order {OrderId} enqueued",
            dto.Id, dto.SourceEventId);
    }
}