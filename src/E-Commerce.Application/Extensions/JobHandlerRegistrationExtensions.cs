using E_Commerce.Application.Modules.Scheduling.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace E_Commerce.Application.Extensions;

public static class JobHandlerRegistrationExtensions
{
    public static IServiceCollection AddJobHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var targets = assemblies.Length > 0
            ? assemblies
            : new[] { Assembly.GetExecutingAssembly() };

        services.Scan(scan => scan
            .FromAssemblies(targets)
            .AddClasses(classes => classes.AssignableTo(typeof(IJobHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}