using E_Commerce.Application.BoundedContexts.Orders.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Extensions;

/// <summary>
/// Centralises registration of strongly‑typed configuration options from appsettings.json.
/// </summary>
public static class ConfigurationOptionsExtension
{
    /// <summary>
    /// Registers all application options bound to their configuration sections.
    /// </summary>
    public static IServiceCollection AddApplicationOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Ordering
        services.RegisterOptions<OrderingOptions>(configuration, "Ordering");

        // Add other options here as the system grows.
        // services.RegisterOptions<ShippingOptions>(configuration, "Shipping");
        // services.RegisterOptions<PaymentOptions>(configuration, "Payment");

        return services;
    }

    /// <summary>
    /// Generic helper that binds an options class to a configuration section
    /// and enables validation on startup if the class has data annotations.
    /// </summary>
    private static IServiceCollection RegisterOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TOptions : class
    {
        var section = configuration.GetSection(sectionName);
        services.AddOptions<TOptions>()
                .Bind(section)
                .ValidateDataAnnotations()
                .ValidateOnStart();

        return services;
    }
}