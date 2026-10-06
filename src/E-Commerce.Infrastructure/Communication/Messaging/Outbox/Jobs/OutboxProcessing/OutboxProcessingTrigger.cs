using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Modules.Scheduling.Attributes;

namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Jobs.OutboxProcessing;

[RecurringJob("outbox-processor", "* * * * *")] // every minute
public sealed class OutboxProcessingTrigger : IRecurringJobTrigger
{
    private readonly IJobScheduler _scheduler;

    public OutboxProcessingTrigger(IJobScheduler scheduler) => _scheduler = scheduler;

    public void Trigger() => _scheduler.Enqueue(new OutboxProcessingJobPayload());
}