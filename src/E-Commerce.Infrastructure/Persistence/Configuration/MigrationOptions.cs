namespace E_Commerce.Infrastructure.Persistence.Configuration;

/// <summary>
/// Controls how EF Core migrations are applied at application startup.
///
/// Current deployment shape: single-instance. Migrations run at startup with
/// retry on transient SQL Server failures. No distributed coordination is
/// needed because only one process migrates.
///
/// Set <see cref="Enabled"/> to false when migrations move to the deployment
/// pipeline (the preferred shape for multi-instance production). The startup
/// call becomes a no-op in that environment without any code changes.
///
/// When multi-instance deployment is introduced, this class will gain
/// distributed-lock options and the extension will coordinate via
/// <c>sp_getapplock</c>. See the Migration Subsystem technical reference.
/// </summary>
public sealed class MigrationOptions
{
    public const string SectionName = "Database:Migrations";

    /// <summary>
    /// When false, the extension returns immediately without applying
    /// migrations. Use when migrations run as a separate deployment step.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Number of retry attempts on transient SQL Server failures
    /// (connection loss, timeout, deadlock). Zero disables retries.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Fixed delay between retry attempts, in seconds.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 5;
}