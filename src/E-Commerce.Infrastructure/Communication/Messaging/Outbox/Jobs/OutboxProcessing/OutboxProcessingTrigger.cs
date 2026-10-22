using E_Commerce.Application.Shared.Scheduling.Attributes;
using E_Commerce.Application.Shared.Scheduling.Abstractions;

namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Jobs.OutboxProcessing;

[RecurringJob("outbox-processor", "* * * * *")] // every minute
public sealed class OutboxProcessingTrigger : IRecurringJobTrigger
{
    private readonly IJobScheduler _scheduler;

    public OutboxProcessingTrigger(IJobScheduler scheduler) => _scheduler = scheduler;

    public void Trigger() => _scheduler.Enqueue(new OutboxProcessingJobPayload());
}