using E_Commerce.Api.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace E_Commerce.Api.Extensions;

public static class CorsExtensions
{
    private const string PolicyName = "DefaultCorsPolicy";

    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var corsOptions = configuration
            .GetSection(CorsOptions.SectionName)
            .Get<CorsOptions>()
            ?? throw new InvalidOperationException("CORS configuration is missing.");

        if (corsOptions.AllowedOrigins is null || corsOptions.AllowedOrigins.Length == 0)
            throw new InvalidOperationException("CORS configuration requires at least one allowed origin.");

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                if (corsOptions.AllowedOrigins.Contains("*"))
                {
                    // Wildcard origin cannot be combined with credentials.
                    policy.AllowAnyOrigin();
                }
                else
                {
                    policy.WithOrigins(corsOptions.AllowedOrigins);
                    if (corsOptions.AllowCredentials)
                        policy.AllowCredentials();
                }

                policy
                    .WithHeaders(corsOptions.AllowedHeaders.Length > 0 ? corsOptions.AllowedHeaders : new[] { "*" })
                    .WithMethods(corsOptions.AllowedMethods.Length > 0 ? corsOptions.AllowedMethods : new[] { "*" });
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCorsConfiguration(this IApplicationBuilder app)
    {
        return app.UseCors(PolicyName);
    }
}