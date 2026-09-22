using E_Commerce.Application.BoundedContexts.Catalog.Brands.Validation;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Validation;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Validation;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Shared.Behaviors;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Decorators;
using E_Commerce.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace E_Commerce.Application.DependencyInjection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // MediatR & behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            // Pipeline order (outermost → innermost)
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));                 // 1
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));             // 2
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));              // 3
            cfg.AddOpenBehavior(typeof(RoleAuthorizationBehavior<,>));       // 4
            cfg.AddOpenBehavior(typeof(PermissionAuthorizationBehavior<,>)); // 5
            cfg.AddOpenBehavior(typeof(TelemetryBehavior<,>));               // 6
            cfg.AddOpenBehavior(typeof(TracingBehavior<,>));                 // 7
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));                 // 8 — innermost
        });

        // ---------------------------------------------------------------
        // Application Options
        // ---------------------------------------------------------------
        ConfigurationOptionsExtension.AddApplicationOptions(services, configuration);

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