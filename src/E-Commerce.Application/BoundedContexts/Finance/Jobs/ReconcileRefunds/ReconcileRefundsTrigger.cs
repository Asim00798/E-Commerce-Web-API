using E_Commerce.Application.Shared.Scheduling.Attributes;
using E_Commerce.Application.Shared.Scheduling.Abstractions;

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