namespace E_Commerce.Application.Modules.Scheduling.Abstractions;

/// <summary>
/// Abstraction over the background job scheduling infrastructure.
/// Allows the application layer to enqueue and schedule jobs
/// without knowing the concrete scheduler (Hangfire, Quartz, etc.).
/// </summary>
public interface IJobScheduler
{
    void Enqueue<TJob>(TJob job) where TJob : IJob;
    void Schedule<TJob>(TJob job, DateTimeOffset enqueueAt) where TJob : IJob;
    void AddOrUpdateRecurring<TJob>(string recurringJobId, TJob job, string cronExpression)
        where TJob : IJob;
}