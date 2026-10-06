using E_Commerce.Application.Modules.Scheduling.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace E_Commerce.Application.Extensions;

public static class RecurringTriggerRegistrationExtensions
{
    public static IServiceCollection AddRecurringTriggers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var targets = assemblies.Length > 0
            ? assemblies
            : new[] { Assembly.GetExecutingAssembly() };

        services.Scan(scan => scan
            .FromAssemblies(targets)
            .AddClasses(classes => classes.AssignableTo(typeof(IRecurringJobTrigger)))
            .AsSelf()
            .WithScopedLifetime());

        return services;
    }
}