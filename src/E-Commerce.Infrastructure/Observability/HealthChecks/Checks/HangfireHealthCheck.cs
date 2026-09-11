using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Data.SqlClient;

namespace E_Commerce.Infrastructure.Observability.HealthChecks.Checks;

/// <summary>
/// Verifies connectivity to the database used by Hangfire storage.
/// This checks database connectivity only, not job processing status.
/// </summary>
public sealed class HangfireHealthCheck : IHealthCheck
{
    private readonly string? _connectionString;

    public HangfireHealthCheck(IConfiguration configuration)
    {
        // Use the primary database connection string.
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            return HealthCheckResult.Unhealthy(
                "Hangfire connection string is not configured.");
        }

        try
        {
            await using var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync(ct);

            await using var command = new SqlCommand(
                "SELECT 1",
                connection);

            await command.ExecuteScalarAsync(ct);

            return HealthCheckResult.Healthy(
                "Hangfire storage database is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Hangfire storage database is unreachable.",
                ex);
        }
    }
}

