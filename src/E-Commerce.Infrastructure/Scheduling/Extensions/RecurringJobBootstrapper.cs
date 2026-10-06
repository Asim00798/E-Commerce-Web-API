using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Modules.Scheduling.Attributes;
using Hangfire;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            ProcessTriggerType(serviceProvider, type, logger);
        }
    }

    #region Private Helper Methods

    private static ILogger CreateLogger(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        return loggerFactory.CreateLogger(nameof(RecurringJobBootstrapper));
    }

    private static List<Type> DiscoverTriggerTypes(Assembly[] assemblies)
    {
        return assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IRecurringJobTrigger).IsAssignableFrom(t)
                        && t is { IsAbstract: false, IsInterface: false })
            .ToList();
    }

    private static void ProcessTriggerType(
        IServiceProvider serviceProvider,
        Type triggerType,
        ILogger logger)
    {
        var attribute = triggerType.GetCustomAttribute<RecurringJobAttribute>();

        if (attribute is null)
        {
            var message = $"IRecurringJobTrigger '{triggerType.Name}' is missing [RecurringJob] attribute. " +
                          "All recurring triggers must carry this attribute to be scheduled.";
            logger.LogError(message);
            throw new InvalidOperationException(message);
        }

        var method = typeof(RecurringJobBootstrapper)
            .GetMethod(nameof(ScheduleTrigger), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(triggerType);

        method.Invoke(null, new object[]
        {
            serviceProvider,
            attribute.JobId,
            attribute.CronExpression
        });
    }

    /// <summary>
    /// Registers a trigger as a Hangfire recurring job using the DI-resolved
    /// <see cref="IRecurringJobManager"/>. Static <c>RecurringJob</c> is not used because
    /// <c>JobStorage.Current</c> is not initialised before the host starts.
    /// </summary>
    private static void ScheduleTrigger<TTrigger>(
        IServiceProvider serviceProvider,
        string jobId,
        string cron)
        where TTrigger : IRecurringJobTrigger
    {
        var manager = serviceProvider.GetRequiredService<IRecurringJobManager>();

        manager.AddOrUpdate<TTrigger>(
            jobId,
            trigger => trigger.Trigger(),
            cron);
    }

    #endregion
}