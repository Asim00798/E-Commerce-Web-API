using E_Commerce.Infrastructure.Security.Authorization.DataSeeding.Configuration;
using E_Commerce.Infrastructure.Security.Authorization.DataSeeding.Execution;
using E_Commerce.Infrastructure.Security.Authorization.DataSeeding.Seeders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Infrastructure.Security.Authorization.DataSeeding.Extensions;

/// <summary>
/// Registers the startup data seeding pipeline.
///
/// The four seeders run as an IHostedService after the host is built and
/// before requests are served. Each seeder owns its own transaction.
/// Requires EF migrations to have completed.
/// </summary>
public static class DataSeedingExtensions
{
    public static IServiceCollection AddDataSeeding(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<DataSeedingOptions>()
            .Bind(configuration.GetSection(DataSeedingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<PermissionSeed>();
        services.AddScoped<RoleSeed>();
        services.AddScoped<RolePermissionSeed>();
        services.AddScoped<IdentitySeed>();

        services.AddHostedService<StartupSeeder>();

        return services;
    }
}