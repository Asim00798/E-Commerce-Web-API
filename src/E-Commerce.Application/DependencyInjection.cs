using E_Commerce.Application.BoundedContexts.Catalog.Brands.Validation;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Validation;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Validation;
using E_Commerce.Application.BoundedContexts.Orders.Models;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Shared.Behaviors;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Decorators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scrutor;// Install-Package Scrutor
using System.Net.Http.Headers;
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
            cfg.AddOpenBehavior(typeof(RoleAuthorizationBehavior<,>));
            cfg.AddOpenBehavior(typeof(PermissionAuthorizationBehavior<,>));
        });

        //services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        //services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // ---------------------------------------------------------------
        // Contexts Specific Validators
        // ---------------------------------------------------------------
        services.AddScoped<BrandLogoFileValidator>();
        services.AddScoped<CategoryImageFileValidator>();
        services.AddScoped<ProductImageFileValidator>();

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

        // 2. First, wrap every handler with the idempotency decorator (innermost)
        services.Decorate(
            typeof(IIntegrationEventHandler<>),
            typeof(IdempotentIntegrationEventHandler<>));

        // 3. Then, wrap the result with the correlation‑scope decorator (outermost)
        services.Decorate(
            typeof(IIntegrationEventHandler<>),
            typeof(CorrelationScopeIntegrationEventHandler<>));

        // ---------------------------------------------------------------
        // Background Job Handlers – automatic registration
        // ---------------------------------------------------------------
        services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.AssignableTo(typeof(IJobHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // ---------------------------------------------------------------
        // Trigger Jobs – automatic registration via marker interface
        // ---------------------------------------------------------------        
        services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.AssignableTo(typeof(IRecurringJobTrigger)))
            .AsSelf()
            .WithScopedLifetime());

        return services;
    }
}