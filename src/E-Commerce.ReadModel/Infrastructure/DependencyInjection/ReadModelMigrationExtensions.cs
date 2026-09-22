using E_Commerce.ReadModel.Infrastructure.Configuration;
using E_Commerce.ReadModel.Infrastructure.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace E_Commerce.ReadModel.Infrastructure.DependencyInjection;

/// <summary>
/// Applies ReadModel database migrations at application startup.
///
/// DbUp runs embedded SQL scripts against the migration connection string.
/// Each script is recorded in the DbUp journal (<c>dbo.SchemaVersions</c>),
/// so subsequent runs are idempotent — only new scripts execute.
///
/// Must run after EF Core migrations (which create the tables the
/// read-side procedures query) and before HTTP traffic is served.
/// </summary>
public static class ReadModelMigrationExtensions
{
    public static void ApplyReadModelMigrations(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var options = scope.ServiceProvider
            .GetRequiredService<IOptions<ReadModelOptions>>().Value;

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("ECommerce.ReadModelMigrations");

        DbUpMigrationRunner.RunMigrations(
            options.MigrationConnectionString,
            logger);
    }
}