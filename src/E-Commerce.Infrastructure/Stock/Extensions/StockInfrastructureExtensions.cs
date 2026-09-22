using E_Commerce.Application.Shared.Stock;
using E_Commerce.Infrastructure.Stock.Services;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Infrastructure.Stock.Extensions;

public static class StockInfrastructureExtensions
{
    public static IServiceCollection AddStockInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IStockService, StockService>();
        
        return services;
    }
}