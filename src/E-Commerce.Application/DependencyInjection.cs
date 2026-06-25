using E_Commerce.Application.BoundedContexts.Catalog.Services;
using E_Commerce.Application.Common.Behaviors;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Decorators;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;                                                           // Install-Package Scrutor
using System.Reflection;

namespace E_Commerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR & behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
        });

        //services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        //services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Application services
        services.AddScoped<CatalogSearchService>();

        // ---------------------------------------------------------------
        // Integration Event Handlers – automatic registration & decoration
        // ---------------------------------------------------------------

        // 1. Register all IIntegrationEventHandler<T> implementations from the Application assembly.
        //    This picks up every handler (e.g., SendOrderConfirmationEmailHandler, UpdateInventoryHandler, etc.)
        services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.AssignableTo(typeof(IIntegrationEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // 2. Decorate every IIntegrationEventHandler<T> with the idempotency decorator.
        //    This is a single line – no per-handler registration overhead.
        services.Decorate(
            typeof(IIntegrationEventHandler<>),
            typeof(IdempotentIntegrationEventHandler<>));

        return services;
    }
}