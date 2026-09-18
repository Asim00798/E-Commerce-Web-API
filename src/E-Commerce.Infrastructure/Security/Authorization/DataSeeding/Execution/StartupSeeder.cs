using E_Commerce.Infrastructure.Security.Authorization.DataSeeding.Seeders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Infrastructure.Security.Authorization.DataSeeding.Execution;

/// <summary>
/// Runs the four startup seeders in explicit order within a single startup
/// scope. Each seeder owns its own transaction, so a failure in one seeder
/// does not roll back the seeders that already completed — the next startup
/// reconciles the remaining state.
///
/// A seeder exception aborts host startup. The application never serves
/// requests with incomplete authorization data.
///
/// Order is significant:
///   1. Permissions must exist before role-permission mappings.
///   2. Roles must exist before role-permission mappings and before the seed
///      administrator is assigned the Administrator role.
///   3. Role-permission mappings must exist before the administrator is
///      useful in production, but the identity seeder does not depend on them
///      at the persistence level.
/// </summary>
public sealed class StartupSeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StartupSeeder> _logger;

    public StartupSeeder(
        IServiceScopeFactory scopeFactory,
        ILogger<StartupSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Data seeding started");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            await RunAllSeedersAsync(scope.ServiceProvider, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data seeding failed — host will not start");
            throw;
        }

        _logger.LogInformation("Data seeding completed");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Executes every seeder in its defined order against the given scope.
    /// All four share the scope; each owns its own transaction.
    /// </summary>
    private static async Task RunAllSeedersAsync(
        IServiceProvider sp,
        CancellationToken cancellationToken)
    {
        await sp.GetRequiredService<PermissionSeed>().SeedAsync(cancellationToken);
        await sp.GetRequiredService<RoleSeed>().SeedAsync(cancellationToken);
        await sp.GetRequiredService<RolePermissionSeed>().SeedAsync(cancellationToken);
        await sp.GetRequiredService<IdentitySeed>().SeedAsync(cancellationToken);
    }
}