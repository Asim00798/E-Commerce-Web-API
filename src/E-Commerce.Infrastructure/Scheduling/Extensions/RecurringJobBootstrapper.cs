using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Modules.Scheduling.Attributes;
using Hangfire;
using System.Reflection;

namespace E_Commerce.Infrastructure.Scheduling.Extensions;

public static class RecurringJobBootstrapper
{
    /// <summary>
    /// Scans all <see cref="IRecurringJobTrigger"/> implementations for the required
    /// <see cref="RecurringJobAttribute"/> and registers them with Hangfire as recurring jobs.
    /// </summary>
    /// <remarks>
    /// This method MUST be called once at startup, after the DI container is built
    /// (e.g. in Program.cs).
    /// Any trigger that is missing the attribute will cause the application to fail fast
    /// with an <see cref="InvalidOperationException"/>.
    /// </remarks>
    public static void ScheduleRecurringJobs(this IServiceProvider serviceProvider, params Assembly[] assemblies)
    {
        var logger = CreateLogger(serviceProvider);
        var triggerTypes = DiscoverTriggerTypes(assemblies);

        foreach (var type in triggerTypes)
        {
            ProcessTriggerType(type, logger);
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Creates a logger for the bootstrapper using a category name.
    /// </summary>
    private static ILogger CreateLogger(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        return loggerFactory.CreateLogger(nameof(RecurringJobBootstrapper));
    }

    /// <summary>
    /// Returns all concrete types that implement <see cref="IRecurringJobTrigger"/>
    /// from the provided assemblies.
    /// </summary>
    private static List<Type> DiscoverTriggerTypes(Assembly[] assemblies)
    {
        return assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IRecurringJobTrigger).IsAssignableFrom(t)
                        && t is { IsAbstract: false, IsInterface: false })
            .ToList();
    }

    /// <summary>
    /// Validates that the trigger type carries the required <see cref="RecurringJobAttribute"/>,
    /// then registers it with Hangfire. Fails fast with an <see cref="InvalidOperationException"/>
    /// if the attribute is missing.
    /// </summary>
    private static void ProcessTriggerType(Type triggerType, ILogger logger)
    {
        var attribute = triggerType.GetCustomAttribute<RecurringJobAttribute>();

        if (attribute is null)
        {
            var message = $"IRecurringJobTrigger '{triggerType.Name}' is missing [RecurringJob] attribute. " +
                          "All recurring triggers must carry this attribute to be scheduled.";
            logger.LogError(message);
            throw new InvalidOperationException(message);
        }

        // Call the generic scheduling helper via reflection
        var method = typeof(RecurringJobBootstrapper)
            .GetMethod(nameof(ScheduleTrigger), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(triggerType);

        method.Invoke(null, new object[] { attribute.JobId, attribute.CronExpression });
    }

    /// <summary>
    /// Generic helper that registers a trigger as a Hangfire recurring job.
    /// </summary>
    private static void ScheduleTrigger<TTrigger>(string jobId, string cron)
        where TTrigger : IRecurringJobTrigger
    {
        RecurringJob.AddOrUpdate<TTrigger>(
            jobId,
            trigger => trigger.Trigger(),
            cron);
    }

    #endregion
}