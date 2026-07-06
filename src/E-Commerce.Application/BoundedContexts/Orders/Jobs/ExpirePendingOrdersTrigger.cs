using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Modules.Scheduling.Attributes;

namespace E_Commerce.Application.BoundedContexts.Orders.Jobs;

[RecurringJob("expire-pending-orders", "*/10 * * * *")]   // every 10 minutes
public class ExpirePendingOrdersTrigger : IRecurringJobTrigger
{
    private readonly IJobScheduler _scheduler;

    public ExpirePendingOrdersTrigger(IJobScheduler scheduler) => _scheduler = scheduler;

    public void Trigger() => _scheduler.Enqueue(new ExpirePendingOrdersJob());
}