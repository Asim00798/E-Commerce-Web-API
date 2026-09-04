using E_Commerce.Api.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Api.Extensions;

public static class HttpCachingExtensions
{
    /// <summary>
    /// Registers the global CacheControlFilter by configuring MVC options.
    /// Does not re-register MVC controllers.
    /// </summary>
    public static IServiceCollection AddHttpCaching(this IServiceCollection services)
    {
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<CacheControlFilter>();
        });

        return services;
    }
}