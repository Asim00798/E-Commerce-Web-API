namespace E_Commerce.Api.Extensions;

public static class RoutingExtensions
{
    public static void AddRoutingExtension(this IServiceCollection services)
    {
        services.AddRouting(options => options.LowercaseUrls = true);
    }
}
