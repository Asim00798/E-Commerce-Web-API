using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Builder;

namespace E_Commerce.Infrastructure.Extensions;

/// <summary>
/// Extension methods for applying EF Core migrations at application startup.
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Applies pending EF Core migrations for the unified AppDbContext.
    /// Call during application startup (not in production without a strategy).
    /// </summary>
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
