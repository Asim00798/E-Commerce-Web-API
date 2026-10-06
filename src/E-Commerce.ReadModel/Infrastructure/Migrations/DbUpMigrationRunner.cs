using DbUp;
using DbUp.Engine;
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
        var upgrader = BuildUpgrader(migrationConnectionString);

        var result = ExecuteMigrations(upgrader);

        HandleMigrationResult(result, logger);

        LogMigrationSuccess(logger);
    }

    private static UpgradeEngine BuildUpgrader(
        string migrationConnectionString)
    {
        var assembly = typeof(DbUpMigrationRunner).Assembly;

        return DeployChanges.To
            .SqlDatabase(migrationConnectionString)
            .WithScriptsEmbeddedInAssembly(
                assembly,
                scriptName => scriptName.Contains(
                    ".Sql.Migrations.",
                    StringComparison.OrdinalIgnoreCase))
            .WithTransactionPerScript()
            .Build();
    }

    private static DatabaseUpgradeResult ExecuteMigrations(
        UpgradeEngine upgrader)
    {
        return upgrader.PerformUpgrade();
    }

    private static void HandleMigrationResult(
        DatabaseUpgradeResult result,
        ILogger logger)
    {
        if (result.Successful)
        {
            return;
        }

        logger.LogError(
            result.Error,
            "ReadModel database migration failed.");

        throw result.Error;
    }

    private static void LogMigrationSuccess(ILogger logger)
    {
        logger.LogInformation(
            "ReadModel database migrations applied successfully.");
    }
}