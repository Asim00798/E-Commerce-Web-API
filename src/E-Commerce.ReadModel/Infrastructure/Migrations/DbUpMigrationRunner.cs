using DbUp;
using Microsoft.Extensions.Logging;

namespace E_Commerce.ReadModel.Infrastructure.Migrations;

public static class DbUpMigrationRunner
{
    /// <summary>
    /// Executes embedded SQL migrations using the migration connection string.
    /// </summary>
    public static void RunMigrations(
        string migrationConnectionString,
        ILogger logger)
    {
        var assembly = typeof(DbUpMigrationRunner).Assembly;

        var upgrader = DeployChanges.To
            .SqlDatabase(migrationConnectionString)
            .WithScriptsEmbeddedInAssembly(
                assembly,
                scriptName => scriptName.Contains(
                    ".Sql.Migrations.",
                    StringComparison.OrdinalIgnoreCase))
            .WithTransactionPerScript()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            logger.LogError(
                result.Error,
                "ReadModel database migration failed.");

            throw result.Error;
        }

        logger.LogInformation(
            "ReadModel database migrations applied successfully.");
    }
}