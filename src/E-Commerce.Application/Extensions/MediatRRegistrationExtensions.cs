using E_Commerce.Application.Shared.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace E_Commerce.Application.Extensions;

public static class MediatRRegistrationExtensions
{
    public static IServiceCollection AddApplicationMediatR(this IServiceCollection services)
    {
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

        return services;
    }
}