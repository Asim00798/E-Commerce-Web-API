using E_Commerce.Application.BoundedContexts.Orders.Abstractions;
using E_Commerce.Application.Shared.Orders;
using E_Commerce.Infrastructure.Persistence.Modules.Orders.Services;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Infrastructure.Extensions;

public static class OrderingInfrastructureExtensions
{
    /// <summary>
    /// Registers Ordering-specific infrastructure services and repositories.
    /// </summary>
    public static IServiceCollection AddOrderingInfrastructure(this IServiceCollection services)
    {
        /// <summary>
        /// Repositories auto registration handled by <see cref="RepositoryRegistrationExtensions"/>
        /// </summary>

        // Application services implemented in Infrastructure
        services.AddScoped<IPendingOrderCleanupService, PendingOrderCleanupService>();

        // Shared contract consumed by Finance to prevent tampered payment fields.
        services.AddScoped<IOrderPricingReader, OrderPricingReader>();

        return services;
    }
}