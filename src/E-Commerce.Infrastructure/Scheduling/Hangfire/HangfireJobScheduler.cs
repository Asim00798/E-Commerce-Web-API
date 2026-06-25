using E_Commerce.Application.Modules.Scheduling.Abstractions;
using Hangfire;

namespace E_Commerce.Infrastructure.Scheduling.Hangfire;

/// <summary>
/// Implements <see cref="IJobScheduler"/> using Hangfire.
/// Translates application‑level scheduling calls to Hangfire's API.
/// </summary>
public class HangfireJobScheduler : IJobScheduler
{
    public void Enqueue<TJob>(TJob job) where TJob : IJob
        => BackgroundJob.Enqueue<HangfireJobDispatcher>(d => d.Dispatch(job, null));

    public void Schedule<TJob>(TJob job, DateTimeOffset enqueueAt) where TJob : IJob
        => BackgroundJob.Schedule<HangfireJobDispatcher>(d => d.Dispatch(job, null), enqueueAt);

    public void AddOrUpdateRecurring<TJob>(string recurringJobId, TJob job, string cronExpression)
        where TJob : IJob
    {
        RecurringJob.AddOrUpdate<HangfireJobDispatcher>(
            recurringJobId,
            d => d.Dispatch(job, null),
            cronExpression);
    }
}