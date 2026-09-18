using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Infrastructure.Security.Authorization.DataSeeding.Configuration;

/// <summary>
/// Configuration for the startup data seeding pipeline.
/// Bound from the "DataSeeding" configuration section.
/// </summary>
public sealed class DataSeedingOptions
{
    public const string SectionName = "DataSeeding";

    /// <summary>
    /// When true, a seed administrator user is created if none exists.
    /// Disable in production; provision the first administrator out-of-band.
    /// </summary>
    public bool CreateSeedAdmin { get; set; }

    /// <summary>
    /// Email used for the seed administrator. Required when
    /// <see cref="CreateSeedAdmin"/> is true.
    /// </summary>
    [EmailAddress]
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>
    /// Password for the seed administrator. Required when
    /// <see cref="CreateSeedAdmin"/> is true. Never logged.
    /// </summary>
    public string AdminPassword { get; set; } = string.Empty;
}