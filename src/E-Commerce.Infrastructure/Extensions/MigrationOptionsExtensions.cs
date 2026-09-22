using E_Commerce.Infrastructure.Persistence.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Infrastructure.Extensions;

public static class MigrationOptionsExtensions
{
    public static IServiceCollection AddMigrationOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<MigrationOptions>()
            .Bind(configuration.GetSection(MigrationOptions.SectionName))
            .Validate(o => o.RetryCount >= 0,
                "Database:Migrations:RetryCount must be >= 0.")
            .Validate(o => o.RetryDelaySeconds >= 0,
                "Database:Migrations:RetryDelaySeconds must be >= 0.")
            .ValidateOnStart();

        return services;
    }
}