namespace E_Commerce.Infrastructure.Configurations;

/// <summary>
/// Strongly-typed database connection settings.
/// Bind from <c>appsettings.json</c> section <c>"Database"</c>.
/// </summary>
public sealed class DatabaseSettings
{
    public const string SectionName = "Database";

    /// <summary>Primary write connection string.</summary>
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>Optional read-replica connection string.</summary>
    public string? ReadReplicaConnectionString { get; init; }

    /// <summary>Max retry count on transient failures.</summary>
    public int MaxRetryCount { get; init; } = 3;

    /// <summary>Delay between retries in seconds.</summary>
    public int RetryDelaySeconds { get; init; } = 5;

    /// <summary>Enable EF Core query logging in development.</summary>
    public bool EnableDetailedErrors { get; init; } = false;
}
