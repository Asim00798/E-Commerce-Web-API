namespace E_Commerce.ReadModel.Configurations;

/// <summary>
/// Strongly-typed settings for read-model database connections,
/// including read-replica connection strings and retry policies.
/// </summary>
public sealed class ReadModelDatabaseSettings
{
    public const string SectionName = "ReadModelDatabase";

    /// <summary>Primary read-replica connection string.</summary>
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>Optional secondary read-replica for load balancing.</summary>
    public string? SecondaryConnectionString { get; init; }

    /// <summary>Maximum number of retry attempts on transient failures.</summary>
    public int MaxRetryCount { get; init; } = 3;

    /// <summary>Delay in seconds between retry attempts.</summary>
    public int RetryDelaySeconds { get; init; } = 5;
}
