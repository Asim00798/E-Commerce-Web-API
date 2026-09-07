namespace E_Commerce.ReadModel.Infrastructure.Configuration;

/// <summary>
/// Strongly typed configuration options for the ReadModel subsystem.
/// Validation is performed by ReadModelOptionsValidator.
/// </summary>
public sealed class ReadModelOptions
{
    /// <summary>
    /// Connection string used by DbUp migrations to create or alter
    /// ReadModel database objects and maintain the migration journal.
    /// </summary>
    public string MigrationConnectionString { get; set; } = string.Empty;
}