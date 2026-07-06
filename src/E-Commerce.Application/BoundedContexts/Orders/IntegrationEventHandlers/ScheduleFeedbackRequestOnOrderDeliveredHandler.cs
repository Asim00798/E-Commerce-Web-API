using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;
using E_Commerce.Application.BoundedContexts.Orders.Jobs;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEventHandlers;

/// <summary>
/// Schedules a feedback request email 7 days after an order is delivered.
/// Idempotency is handled by the global IdempotentIntegrationEventHandler decorator,
/// so the schedule call will be executed at most once.
/// </summary>
public class ScheduleFeedbackRequestOnOrderDeliveredHandler
    : IIntegrationEventHandler<OrderDeliveredIntegrationEvent>
{
    private readonly IJobScheduler _scheduler;

    public ScheduleFeedbackRequestOnOrderDeliveredHandler(IJobScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public Task HandleAsync(OrderDeliveredIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        var job = new SendFeedbackRequestJob
        {
            OrderId = integrationEvent.OrderId,
        };

        // Schedule exactly once, 7 days from now
        _scheduler.Schedule(job, DateTimeOffset.UtcNow.AddDays(7));

        return Task.CompletedTask;
    }
}