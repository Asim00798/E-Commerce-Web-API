using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Api.Extensions;

public static class RoutingExtensions
{
    public static IServiceCollection AddLowercaseRouting(this IServiceCollection services)
    {
        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        });

        return services;
    }
}