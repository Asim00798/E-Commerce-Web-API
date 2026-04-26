using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Extensions;

/// <summary>
/// Extension methods for applying EF Core migrations at application startup.
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Applies pending EF Core migrations for all registered DbContexts.
    /// Call during application startup (not in production without a strategy).
    /// </summary>
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        // TODO: Resolve and migrate CatalogDbContext, FileManagementDbContext, OutboxDbContext
        await Task.CompletedTask;
    }
}
