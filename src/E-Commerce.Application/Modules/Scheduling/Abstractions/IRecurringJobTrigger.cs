namespace E_Commerce.Application.Modules.Scheduling.Abstractions;

/// <summary>
/// Contract for a trigger that enqueues a recurring background job via IJobScheduler.
/// </summary>
public interface IRecurringJobTrigger
{
    void Trigger();
}