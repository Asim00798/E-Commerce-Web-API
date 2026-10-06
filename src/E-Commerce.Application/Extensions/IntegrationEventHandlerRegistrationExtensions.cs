using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Decorators;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace E_Commerce.Application.Extensions;

public static class IntegrationEventHandlerRegistrationExtensions
{
    public static IServiceCollection AddIntegrationEventHandlers(this IServiceCollection services)
    {
        // 1. Register all IIntegrationEventHandler<T> implementations.
        services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.AssignableTo(typeof(IIntegrationEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // 2. Idempotency decorator (innermost).
        services.Decorate(
            typeof(IIntegrationEventHandler<>),
            typeof(IdempotentIntegrationEventHandler<>));

        // 3. Correlation-scope decorator (outermost).
        services.Decorate(
            typeof(IIntegrationEventHandler<>),
            typeof(CorrelationScopeIntegrationEventHandler<>));

        return services;
    }
}