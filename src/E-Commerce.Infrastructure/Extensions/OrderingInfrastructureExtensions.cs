using E_Commerce.Application.BoundedContexts.Orders.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Infrastructure.Persistence.Modules.Orders.Repositories;
using E_Commerce.Infrastructure.Persistence.Modules.Orders.Repository;
using E_Commerce.Infrastructure.Persistence.Modules.Orders.Services;

namespace E_Commerce.Infrastructure.Extensions;

public static class OrderingInfrastructureExtensions
{
    /// <summary>
    /// Registers Ordering-specific infrastructure services and repositories.
    /// </summary>
    public static IServiceCollection AddOrderingInfrastructure(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Application services implemented in Infrastructure
        services.AddScoped<IPendingOrderCleanupService, PendingOrderCleanupService>();

        return services;
    }
}