using E_Commerce.Api.Configuration;

namespace E_Commerce.Api.Extensions;

public static class CorsExtensions
{
    private const string PolicyName = "DefaultCorsPolicy";

    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CorsOptions>(
            configuration.GetSection(CorsOptions.SectionName));

        var corsOptions =
            configuration
                .GetSection(CorsOptions.SectionName)
                .Get<CorsOptions>()
            ?? throw new InvalidOperationException(
                "Cors configuration is missing.");

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy
                    .WithOrigins(corsOptions.AllowedOrigins)
                    .WithHeaders(corsOptions.AllowedHeaders)
                    .WithMethods(corsOptions.AllowedMethods);

                if (corsOptions.AllowCredentials)
                {
                    policy.AllowCredentials();
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCorsConfiguration(
        this IApplicationBuilder app)
    {
        return app.UseCors(PolicyName);
    }
}