using Domain.SharedKernel.Events;
using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Constants;
using E_Commerce.Application.Shared.Communication.Notifications.Models;
using E_Commerce.Application.Shared.Communication.Notifications.Persistence;
using E_Commerce.Application.Shared.Communication.PostCommit;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Events;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Application.Modules.Notifications.Handlers;

/// <summary>
/// Creates the in‑app user notification for the “OrderPlaced” event and
/// enqueues a best‑effort SignalR hint that will be delivered after
/// the database transaction commits.
/// </summary>
public class PersistUserNotificationOnOrderPlacedHandler
    : IDomainEventHandler<OrderPlacedDomainEvent>
{
    private readonly IUserNotificationRepository _userNotificationRepo;
    private readonly IPostCommitProcessor _postCommitProcessor;

    public PersistUserNotificationOnOrderPlacedHandler(
        IUserNotificationRepository userNotificationRepo,
        IPostCommitProcessor postCommitProcessor)
    {
        _userNotificationRepo = userNotificationRepo;
        _postCommitProcessor = postCommitProcessor;
    }

    public async Task Handle(OrderPlacedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var dto = new UserNotificationDto
        {
            UserId = domainEvent.CustomerId,
            Type = "OrderPlaced",
            SourceEventId = domainEvent.OrderId,
            Message = $"Your order #{domainEvent.OrderId} has been placed.",
            CreatedAtUtc = DateTime.UtcNow   
        };

        await _userNotificationRepo.AddAsync(dto, cancellationToken);

        // Enqueue a post‑commit action to send a real‑time notification hint to the user.
        // This is a best‑effort notification; if the transaction fails, the user will not receive it.
        _postCommitProcessor.Enqueue(async (serviceProvider, ct) =>
        {
            var publisher = serviceProvider.GetRequiredService<IRealtimeEventPublisher>();
            await publisher.PublishAsync(new RealTimeMessage
            {
                UserId = domainEvent.CustomerId,
                Method = RealTimeEvents.NewNotification,
                Payload = new { type = "OrderPlaced" }
            }, ct);
        });
    }
}