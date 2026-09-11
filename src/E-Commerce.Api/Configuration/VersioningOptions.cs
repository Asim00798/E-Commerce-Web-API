using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Api.Configuration;

public sealed class VersioningOptions
{
    public const string SectionName = "Versioning";

    [Range(1, 100)]
    public int DefaultMajorVersion { get; set; } = 1;

    [Range(0, 100)]
    public int DefaultMinorVersion { get; set; } = 0;
}