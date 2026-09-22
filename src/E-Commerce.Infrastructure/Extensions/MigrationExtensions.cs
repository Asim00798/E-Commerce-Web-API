using E_Commerce.Infrastructure.Persistence.Configuration;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Extensions;

/// <summary>
/// Applies pending EF Core migrations at application startup.
///
/// Current deployment shape: single-instance. Migrations run directly with
/// retry on transient SQL Server failures. No distributed coordination is
/// performed.
///
/// When multi-instance deployment is introduced, extend this class with a
/// distributed-lock strategy. See the Migration Subsystem technical reference.
/// </summary>
public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<MigrationOptions>>().Value;
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("ECommerce.Migrations");

        if (!options.Enabled)
        {
            logger.LogInformation("EF Core migrations are disabled by configuration.");
            return;
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await ApplyWithRetryAsync(dbContext, options, logger);
    }

    // ------------------------------------------------------------------
    // Migration with retry
    // ------------------------------------------------------------------

    /// <summary>
    /// Applies migrations with retry on transient SQL Server failures.
    /// The connection is closed between attempts so each retry starts from
    /// a fresh session, avoiding stale pooled connections.
    /// </summary>
    private static async Task ApplyWithRetryAsync(
        AppDbContext dbContext,
        MigrationOptions options,
        ILogger logger)
    {
        var maxAttempts = options.RetryCount + 1;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                logger.LogInformation(
                    "Applying EF Core migrations (attempt {Attempt}/{Max})...",
                    attempt,
                    maxAttempts);

                await dbContext.Database.MigrateAsync();

                logger.LogInformation("EF Core migrations applied successfully.");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts && IsTransient(ex))
            {
                logger.LogWarning(
                    ex,
                    "Migration attempt {Attempt}/{Max} failed (transient). Retrying in {Delay}s...",
                    attempt,
                    maxAttempts,
                    options.RetryDelaySeconds);

                try { await dbContext.Database.CloseConnectionAsync(); }
                catch { /* best effort */ }

                await Task.Delay(TimeSpan.FromSeconds(options.RetryDelaySeconds));
            }
        }
    }

    // ------------------------------------------------------------------
    // Transient failure detection
    // ------------------------------------------------------------------

    /// <summary>
    /// Walks the exception chain looking for a recognized transient SQL Server
    /// condition. EF Core sometimes wraps <see cref="SqlException"/> in a
    /// higher-level exception, so inner exceptions are inspected too.
    /// </summary>
    private static bool IsTransient(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (current is SqlException sql && IsTransientSqlError(sql.Number))
                return true;
        }

        return false;
    }

    private static bool IsTransientSqlError(int number) =>
        number is
            -2 or         // timeout
            1205 or       // deadlock victim
            4060 or       // cannot open database (cold start)
            10928 or      // resource limit
            10929 or      // resource limit
            10053 or      // connection aborted
            10054 or      // connection reset
            10060 or      // network timeout
            40197 or      // service error
            40501 or      // service busy
            40613 or      // database unavailable
            49918 or      // insufficient resources
            49919 or      // too many operations
            49920;        // too many operations
}