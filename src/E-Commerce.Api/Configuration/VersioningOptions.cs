using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Api.Configuration;

/// <summary>
/// Configuration for API versioning.
/// Bound from the "Versioning" section. Defines the default API version
/// used when a request does not specify one.
/// </summary>
public sealed class VersioningOptions
{
    public const string SectionName = "Versioning";

    /// <summary>
    /// Major component of the default API version. Combined with
    /// <see cref="DefaultMinorVersion"/> to form the full version
    /// (e.g., 1 and 0 ? "1.0").
    /// </summary>
    [Range(1, 100)]
    public int DefaultMajorVersion { get; set; } = 1;

    /// <summary>
    /// Minor component of the default API version.
    /// </summary>
    [Range(0, 100)]
    public int DefaultMinorVersion { get; set; } = 0;
}