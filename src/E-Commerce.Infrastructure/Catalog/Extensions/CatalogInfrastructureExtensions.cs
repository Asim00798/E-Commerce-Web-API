using E_Commerce.Application.Shared.Stock;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Infrastructure.Catalog.Services;
using E_Commerce.Infrastructure.Persistence.Modules.Catalog.Brand.Repository;
using E_Commerce.Infrastructure.Persistence.Modules.Catalog.Category.Repository;
using E_Commerce.Infrastructure.Persistence.Modules.Catalog.Product.Repository;
namespace E_Commerce.Infrastructure.Catalog.Extensions;

public static class CatalogInfrastructureExtensions
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockService, StockService>();

        return services;
    }
}