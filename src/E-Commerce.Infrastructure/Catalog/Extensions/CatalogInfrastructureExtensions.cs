using E_Commerce.Application.Shared.Stock;
using E_Commerce.Infrastructure.Catalog.Services;
using E_Commerce.Infrastructure.Extensions;

namespace E_Commerce.Infrastructure.Catalog.Extensions;

public static class CatalogInfrastructureExtensions
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IStockService, StockService>();

        /// <summary>
        /// Repositories auto registration handled by <see cref="RepositoryRegistrationExtensions"/>
        /// </summary>
        
        return services;
    }
}