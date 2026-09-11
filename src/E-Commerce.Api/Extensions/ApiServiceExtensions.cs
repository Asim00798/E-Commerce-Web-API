using E_Commerce.Api.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Api.Extensions;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApiConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<SwaggerConfiguration>()
            .Bind(configuration.GetSection(SwaggerConfiguration.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(CorsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<VersioningOptions>()
            .Bind(configuration.GetSection(VersioningOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}