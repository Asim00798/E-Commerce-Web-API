using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Modules.Scheduling.Coordination;
using E_Commerce.Application.Modules.Scheduling.Policies;
using E_Commerce.Infrastructure.Scheduling.Hangfire;
using Hangfire;

namespace E_Commerce.Infrastructure.Scheduling.Extensions;

/// <summary>
/// Registers the core scheduling infrastructure services.
/// 
/// NOTE: Recurring triggers are automatically scheduled by <see cref="RecurringJobBootstrapper"/>
/// based on their [RecurringJob] attribute. No manual <c>RecurringJob.AddOrUpdate</c> calls
/// are needed for triggers implementing <see cref="IRecurringJobTrigger"/>.
/// The bootstrapper must be called once at startup (e.g. in Program.cs) after the DI container is built.
/// </summary>
public static class SchedulingInfrastructureExtensions
{
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
        services.AddScoped<IJobOrchestrator, JobOrchestrator>();
        services.AddScoped<IJobScheduler, HangfireJobScheduler>();

        // -----------------------------------------------------------------
        // 3. Resilience policies
        // -----------------------------------------------------------------
        services.AddScoped<IJobPolicy>(sp =>
            new TimeoutPolicy(TimeSpan.FromMinutes(5)));
        services.AddScoped<IJobPolicy>(sp =>
            new CircuitBreakerPolicy(
                failureThreshold: 5,
                logger: sp.GetRequiredService<ILogger<CircuitBreakerPolicy>>()));

        // -----------------------------------------------------------------
        // 4. Hangfire adapters
        // -----------------------------------------------------------------
        services.AddScoped<HangfireJobDispatcher>();

        // -----------------------------------------------------------------
        // 5. Infrastructure recurring jobs (not ITrigger-based)
        // -----------------------------------------------------------------
        services.AddScoped<OutboxProcessingJob>();
        RecurringJob.AddOrUpdate<OutboxProcessingJob>(
            "outbox-processor",
            job => job.ExecuteAsync(),
            Cron.Minutely);

        services.AddScoped<DeadLetterMonitorJob>();
        RecurringJob.AddOrUpdate<DeadLetterMonitorJob>(
            "dead-letter-monitor",
            job => job.ExecuteAsync(),
            Cron.Hourly);

        // -----------------------------------------------------------------
        // 6. Automatic trigger scheduling is handled by RecurringJobBootstrapper.
        //    All triggers implementing ITrigger with [RecurringJob] attribute
        //    are discovered and scheduled automatically at startup.
        // -----------------------------------------------------------------

        return services;
    }
}