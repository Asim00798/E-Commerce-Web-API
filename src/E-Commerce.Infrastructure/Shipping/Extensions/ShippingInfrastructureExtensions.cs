using E_Commerce.Application.BoundedContexts.Shipping.Abstractions;
using E_Commerce.Application.Shared.Shipping.Services;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.Policies;
using E_Commerce.Infrastructure.Shipping.Location;
using E_Commerce.Infrastructure.Shipping.Services;

namespace E_Commerce.Infrastructure.Shipping.Extensions;

public static class ShippingInfrastructureExtensions
{
    public static IServiceCollection AddShippingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Location/Distance calculation
        services.Configure<LocationOptions>(
            configuration.GetSection(LocationOptions.SectionName));

        services.AddScoped<ILocationService, MapLocationService>();

        // Policies
        services.AddScoped<LocalShippingFeePolicy>();

        // Services
        services.AddScoped<IShippingFeeCalculator, ShippingFeeCalculatorService>();

        // Persistence
        /// <summary>
        /// Repositories registration are handled by the automatic registration <see cref="RepositoryRegistrationExtensions"/>
        /// </summary>

        return services;
    }
}