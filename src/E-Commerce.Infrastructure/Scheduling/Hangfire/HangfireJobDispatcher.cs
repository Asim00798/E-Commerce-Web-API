using E_Commerce.Application.Modules.Scheduling.Abstractions;
using Hangfire.Server;
using E_Commerce.Infrastructure.Scheduling.Execution;

namespace E_Commerce.Infrastructure.Scheduling.Hangfire;

/// <summary>
/// Hangfire‑facing dispatcher that bridges Hangfire's runtime to the
/// Application‑layer execution engine. Hangfire calls <c>Dispatch</c>
/// when a job is dequeued.
/// </summary>
public class HangfireJobDispatcher
{
    private readonly IJobOrchestrator _orchestrator;

    public HangfireJobDispatcher(IJobOrchestrator Orchestrator)
    {
        _orchestrator = Orchestrator;
    }

    /// <summary>
    /// Entry point called by Hangfire.
    /// </summary>
    /// <typeparam name="TJob">The job type.</typeparam>
    /// <param name="job">Deserialized job instance.</param>
    /// <param name="context">Hangfire's <see cref="PerformContext"/> (supplied at runtime).</param>
    public async Task Dispatch<TJob>(TJob job, PerformContext? context) where TJob : IJob
    {
        // Hangfire replaces the null placeholder with the real PerformContext at runtime
        var ctx = context ?? throw new InvalidOperationException("PerformContext is not available.");

        var jobContext = new JobExecutionContext(
            jobId: ctx.BackgroundJob.Id,
            correlationId: ctx.BackgroundJob.Id,
            attempt: ctx.GetJobParameter<int>("RetryCount") + 1,
            queuedAt: ctx.BackgroundJob.CreatedAt);

        await _orchestrator.ExecuteAsync(job, jobContext, ctx.CancellationToken.ShutdownToken);
    }
}