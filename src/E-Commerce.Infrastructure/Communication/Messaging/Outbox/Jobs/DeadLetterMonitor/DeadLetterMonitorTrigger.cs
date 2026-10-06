using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Modules.Scheduling.Attributes;

namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Jobs.DeadLetterMonitor;

[RecurringJob("dead-letter-monitor", "0 * * * *")] // hourly
public sealed class DeadLetterMonitorTrigger : IRecurringJobTrigger
{
    private readonly IJobScheduler _scheduler;

    public DeadLetterMonitorTrigger(IJobScheduler scheduler) => _scheduler = scheduler;

    public void Trigger() => _scheduler.Enqueue(new DeadLetterMonitorJobPayload());
}