using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Modules.Scheduling.Attributes;

namespace E_Commerce.Application.BoundedContexts.Finance.Jobs.ReconcileRefunds;

[RecurringJob("finance-refund-reconciliation", "*/15 * * * *")]
public sealed class ReconcileRefundsTrigger : IRecurringJobTrigger
{
    private readonly IJobScheduler _scheduler;

    public ReconcileRefundsTrigger(IJobScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public void Trigger()
    {
        _scheduler.Enqueue(new ReconcileRefundsJob());
    }
}