using E_Commerce.Application.BoundedContexts.Orders.Jobs.SendFeedbackRequest;
using E_Commerce.Application.BoundedContexts.Shipping.IntegrationEvents;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEventHandlers;

public sealed class ScheduleFeedbackRequestOnOrderDeliveredHandler
    : IIntegrationEventHandler<ShipmentDeliveredIntegrationEvent>
{
    private readonly IJobScheduler _jobScheduler;

    public ScheduleFeedbackRequestOnOrderDeliveredHandler(IJobScheduler jobScheduler)
    {
        _jobScheduler = jobScheduler;
    }

    public Task HandleAsync(
        ShipmentDeliveredIntegrationEvent integrationEvent,
        CancellationToken ct)
    {
        _jobScheduler.Schedule(
            new SendFeedbackRequestJob(integrationEvent.OrderId),
            DateTimeOffset.UtcNow.AddDays(7));

        return Task.CompletedTask;
    }
}