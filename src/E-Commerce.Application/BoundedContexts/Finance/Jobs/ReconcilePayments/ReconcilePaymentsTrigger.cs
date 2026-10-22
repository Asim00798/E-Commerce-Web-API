using E_Commerce.Application.Shared.Scheduling.Attributes;
using E_Commerce.Application.Shared.Scheduling.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Finance.Jobs.ReconcilePayments;

[RecurringJob("finance-payment-reconciliation", "*/30 * * * *")]
public sealed class ReconcilePaymentsTrigger : IRecurringJobTrigger
{
    private readonly IJobScheduler _scheduler;

    public ReconcilePaymentsTrigger(IJobScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public void Trigger()
    {
        _scheduler.Enqueue(new ReconcilePaymentsJob());
    }
}