using E_Commerce.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Extensions;

public static class HttpCachingExtensions
{
    /// <summary>
    /// Registers the global CacheControl result filter and its startup validator.
    /// Does not register MVC controllers.
    /// </summary>
    public static IServiceCollection AddHttpCaching(
        this IServiceCollection services)
    {
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<CacheControlFilter>();
        });

        services.AddHostedService<CacheControlStartupValidator>();

        return services;
    }
}