using E_Commerce.ReadModel.BoundedContexts.Catalog.Projections;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Services;
using E_Commerce.ReadModel.DbContext;
using E_Commerce.ReadModel.Infrastructure.Caching;
using E_Commerce.ReadModel.Infrastructure.HealthChecks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace E_Commerce.ReadModel.Infrastructure;

/// <summary>
/// Extension methods for registering all ReadModel services into the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReadModel(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AppReadDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
                    sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        services.AddScoped<ProductProjectionService>();
        services.AddScoped<BrandProjectionService>();
        services.AddScoped<CategoryProjectionService>();
        services.AddScoped<IProductQueryService, ProductQueryService>();
        services.AddScoped<IBrandQueryService, BrandQueryService>();
        services.AddScoped<ICategoryQueryService, CategoryQueryService>();

        services.AddMemoryCache();
        services.AddScoped<ICacheService, MemoryCacheService>();

        services.AddHealthChecks().AddCheck<ReadDbContextHealthCheck>("read_db_health");

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        return services;
    }
}
