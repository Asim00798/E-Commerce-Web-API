namespace E_Commerce.Application.Modules.Scheduling.Abstractions;

/// <summary>
/// Abstraction over the background job scheduling infrastructure.
/// Allows the application layer to enqueue and schedule jobs
/// without knowing the concrete scheduler (Hangfire, Quartz, etc.).
/// </summary>
/// <remarks>
/// All three methods eventually cause a job to be executed through the
/// same execution pipeline:
/// <code>
///   Hangfire → HangfireJobDispatcher → IJobOrchestrator → IJobHandler&lt;TJob&gt;
/// </code>
/// The difference is purely in <b>when</b> the job enters the pipeline.
/// </remarks>
public interface IJobScheduler
{
    // -----------------------------------------------------------------
    // 1. Immediate (Fire‑and‑Forget)
    // -----------------------------------------------------------------

    /// <summary>
    /// Enqueues a job for <b>immediate</b> execution (fire‑and‑forget).
    /// The job runs once, as soon as a worker is available.
    /// </summary>
    /// <typeparam name="TJob">Concrete job type implementing <see cref="IJob"/>.</typeparam>
    /// <param name="job">The job payload.</param>
    /// <remarks>
    /// <b>Example usage:</b>
    /// <code>
    ///   _jobScheduler.Enqueue(new SendEmailJob { To = "…", Subject = "…" });
    /// </code>
    /// <para>
    /// This is the primitive used by <b>all</b> other scheduling methods:
    /// both <see cref="Schedule{TJob}"/> and triggers for recurring jobs
    /// ultimately call <see cref="Enqueue{TJob}"/> to place the work in the queue.
    /// </para>
    /// </remarks>
    void Enqueue<TJob>(TJob job) where TJob : IJob;

    // -----------------------------------------------------------------
    // 2. One‑Shot Delayed
    // -----------------------------------------------------------------

    /// <summary>
    /// Schedules a job to run <b>exactly once</b> at the specified future time.
    /// The job does not execute until that moment.
    /// </summary>
    /// <typeparam name="TJob">Concrete job type implementing <see cref="IJob"/>.</typeparam>
    /// <param name="job">The job payload.</param>
    /// <param name="enqueueAt">The UTC time at which the job should be enqueued.</param>
    /// <remarks>
    /// <b>Example usage:</b>
    /// <code>
    ///   _jobScheduler.Schedule(
    ///       new SendReminderJob { OrderId = order.Id },
    ///       DateTimeOffset.UtcNow.AddMinutes(15));
    /// </code>
    /// <para>
    /// Internally, Hangfire will wait until <paramref name="enqueueAt"/> and then
    /// call <see cref="Enqueue{TJob}"/> with the given payload. The execution
    /// pipeline is identical to an immediate enqueue; only the timing differs.
    /// </para>
    /// </remarks>
    void Schedule<TJob>(TJob job, DateTimeOffset enqueueAt) where TJob : IJob;

    // -----------------------------------------------------------------
    // 3. Recurring (Cron‑Based)
    // -----------------------------------------------------------------

    /// <summary>
    /// Registers or updates a <b>recurring</b> job that executes on a cron schedule.
    /// Each execution is independent and goes through the same pipeline as a
    /// fire‑and‑forget enqueue.
    /// </summary>
    /// <typeparam name="TJob">Concrete job type implementing <see cref="IJob"/>.</typeparam>
    /// <param name="recurringJobId">Unique identifier for the recurring job (used by Hangfire).</param>
    /// <param name="job">The job payload.</param>
    /// <param name="cronExpression">
    /// A valid cron expression that defines the schedule (e.g. <c>"0 2 1 * *"</c> for
    /// the 1st day of every month at 02:00). Cron helpers like <c>Cron.Monthly</c> are
    /// not used in the Application layer to avoid an infrastructure dependency.
    /// </param>
    /// <remarks>
    /// <b>Example usage (rarely used directly – prefer the <c>[RecurringJob]</c> attribute on a trigger):</b>
    /// <code>
    ///   _jobScheduler.AddOrUpdateRecurring(
    ///       "monthly-report",
    ///       new GenerateReportJob { Format = "CSV" },
    ///       "0 0 1 * *");
    /// </code>
    /// <para>
    /// In the current architecture, recurring jobs are normally defined via a
    /// <see cref="IRecurringJobTrigger"/> with the <c>[RecurringJob]</c> attribute.
    /// Each time the schedule fires, Hangfire calls the trigger, which in turn
    /// calls <see cref="Enqueue{TJob}"/> – so the execution path is identical
    /// to a fire‑and‑forget enqueue.
    /// </para>
    /// </remarks>
    void AddOrUpdateRecurring<TJob>(string recurringJobId, TJob job, string cronExpression)
        where TJob : IJob;
}