using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Modules.Scheduling.Attributes;

namespace E_Commerce.Application.BoundedContexts.Onboarding.Jobs;

/// <summary>
/// Hangfire recurring job adapter that enqueues the expired registration cleanup job.
/// Runs daily at 03:00 UTC.
/// </summary>
[RecurringJob("expired-registration-cleanup", "0 3 * * *")]
public sealed class ExpiredRegistrationCleanupTrigger : IRecurringJobTrigger
{
    private readonly IJobScheduler _scheduler;

    public ExpiredRegistrationCleanupTrigger(IJobScheduler scheduler) => _scheduler = scheduler;

    /// <summary>
    /// Called by the Hangfire scheduler. Enqueues a new <see cref="ExpiredRegistrationCleanupJob"/>.
    /// </summary>
    public void Trigger() => _scheduler.Enqueue(new ExpiredRegistrationCleanupJob());
}