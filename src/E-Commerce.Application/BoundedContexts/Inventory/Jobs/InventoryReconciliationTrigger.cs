using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Modules.Scheduling.Attributes;

namespace E_Commerce.Application.BoundedContexts.Inventory.Jobs;

/// <summary>
/// Hangfire recurring job adapter that enqueues the reconciliation job.
/// This keeps Hangfire decoupled from the job handler.
/// </summary>
[RecurringJob("monthly-inventory-reconciliation", "0 2 1 * *")]   // 1st day of every month at 02:00
public class InventoryReconciliationTrigger : IRecurringJobTrigger
{
    private readonly IJobScheduler _scheduler;

    public InventoryReconciliationTrigger(IJobScheduler scheduler) => _scheduler = scheduler;

    public void Trigger() => _scheduler.Enqueue(new ReconcileInventoryJob());
}