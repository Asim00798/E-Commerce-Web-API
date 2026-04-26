using E_Commerce.Api.Configuration;

namespace E_Commerce.Api.Extensions;

public static class ConfigurationExtensions
{
    public static void AddConfigurationExtension(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));
        services.Configure<SwaggerConfiguration>(configuration.GetSection("SwaggerConfiguration"));
        services.Configure<CorsOptions>(configuration.GetSection("CorsOptions"));
        services.Configure<RateLimitingOptions>(configuration.GetSection("RateLimitingOptions"));
        services.Configure<VersioningOptions>(configuration.GetSection("VersioningOptions"));
    }
}
