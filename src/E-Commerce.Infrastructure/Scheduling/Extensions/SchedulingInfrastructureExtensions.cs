using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Modules.Scheduling.Coordination;
using E_Commerce.Application.Modules.Scheduling.Pipelines;
using E_Commerce.Application.Modules.Scheduling.Policies;
using E_Commerce.Infrastructure.Scheduling.Hangfire;
using Hangfire;

namespace E_Commerce.Infrastructure.Scheduling.Extensions;

/// <summary>
/// Extension methods for registering the entire Scheduling subsystem
/// (Application + Infrastructure) into the DI container.
/// </summary>
public static class SchedulingInfrastructureExtensions
{
    /// <summary>
    /// Adds all scheduling services: Hangfire, job execution gateway,
    /// pipeline, resilience policies, and the outbox processing recurring job.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="hangfireConnectionString">Connection string for Hangfire SQL storage.</param>
    public static IServiceCollection AddSchedulingInfrastructure(
        this IServiceCollection services,
        string hangfireConnectionString)
    {
        // -----------------------------------------------------------------
        // 1. Hangfire core
        // -----------------------------------------------------------------
        services.AddHangfire(config =>
            config.UseSqlServerStorage(hangfireConnectionString));
        services.AddHangfireServer();

        // -----------------------------------------------------------------
        // 2. Application layer abstractions
        // -----------------------------------------------------------------
        // The execution gateway – resolves handlers, applies policies, runs pipeline
        services.AddScoped<IJobExecutionEngine, JobOrchestrator>();

        // The scheduler abstraction – decouples Application code from Hangfire
        services.AddScoped<IJobScheduler, HangfireJobScheduler>();

        // -----------------------------------------------------------------
        // 3. Execution pipeline
        // -----------------------------------------------------------------
        // Composes optional JobExecutionStep instances (currently none registered;
        // add them as scoped implementations of JobExecutionStep here when needed)
        services.AddScoped<JobExecutionPipeline>();

        // -----------------------------------------------------------------
        // 4. Resilience policies
        // Registered as IJobPolicy so they are injected as a collection
        // into JobOrchestrator. Hangfire itself handles retries.
        // -----------------------------------------------------------------
        services.AddScoped<IJobPolicy>(sp =>
            new TimeoutPolicy(TimeSpan.FromMinutes(5)));

        services.AddScoped<IJobPolicy>(sp =>
            new CircuitBreakerPolicy(
                failureThreshold: 5,
                logger: sp.GetRequiredService<ILogger<CircuitBreakerPolicy>>()));

        // -----------------------------------------------------------------
        // 5. Infrastructure services (Hangfire adapters)
        // -----------------------------------------------------------------
        services.AddScoped<HangfireJobDispatcher>();

        // -----------------------------------------------------------------
        // 6. Outbox processing recurring job
        // Replaces the continuous BackgroundService loop.
        // -----------------------------------------------------------------
        services.AddScoped<OutboxProcessingJob>();

        RecurringJob.AddOrUpdate<OutboxProcessingJob>(
            "outbox-processor",
            job => job.ExecuteAsync(),
            Cron.Minutely);
        // -----------------------------------------------------------------
        // 7. Dead‑letter monitoring recurring job
        // -----------------------------------------------------------------
        services.AddScoped<DeadLetterMonitorJob>();

        RecurringJob.AddOrUpdate<DeadLetterMonitorJob>(
            "dead-letter-monitor",
            job => job.ExecuteAsync(),
            Cron.Hourly); // or Cron.Minutely, etc.

        return services;
    }
}