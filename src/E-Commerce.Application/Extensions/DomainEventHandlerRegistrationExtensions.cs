using E_Commerce.Domain.SharedKernel.Events;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace E_Commerce.Application.Extensions;

public static class DomainEventHandlerRegistrationExtensions
{
    public static IServiceCollection AddDomainEventHandlers(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}