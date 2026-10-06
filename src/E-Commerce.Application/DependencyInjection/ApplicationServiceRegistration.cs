using E_Commerce.Application.Extensions;
using E_Commerce.Application.Shared.Security.Authorization.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Application.DependencyInjection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // MediatR pipeline & behaviors
        services.AddApplicationMediatR();

        // Application options
        ConfigurationOptionsExtension.AddApplicationOptions(services, configuration);
        
        // Authorization sources
        services.AddAuthorizationSources();

        // Context-specific validators
        services.AddContextValidators();

        // Handler registrations
        services.AddDomainEventHandlers();
        services.AddIntegrationEventHandlers();
        services.AddJobHandlers();
        services.AddRecurringTriggers();

        return services;
    }
}