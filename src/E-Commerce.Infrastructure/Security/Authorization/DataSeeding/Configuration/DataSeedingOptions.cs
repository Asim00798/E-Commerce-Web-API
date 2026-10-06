using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Infrastructure.Security.Authorization.DataSeeding.Configuration;

/// <summary>
/// Configuration for the startup data seeding pipeline.
/// Bound from the "DataSeeding" configuration section.
/// </summary>
public sealed class DataSeedingOptions : IValidatableObject
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
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>
    /// Password for the seed administrator. Required when
    /// <see cref="CreateSeedAdmin"/> is true. Never logged.
    /// </summary>
    public string AdminPassword { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!CreateSeedAdmin)
            yield break;

        if (string.IsNullOrWhiteSpace(AdminEmail))
        {
            yield return new ValidationResult(
                "DataSeeding:AdminEmail is required when CreateSeedAdmin is true.",
                new[] { nameof(AdminEmail) });
        }
        else if (!new EmailAddressAttribute().IsValid(AdminEmail))
        {
            yield return new ValidationResult(
                "DataSeeding:AdminEmail must be a valid email address.",
                new[] { nameof(AdminEmail) });
        }

        if (string.IsNullOrWhiteSpace(AdminPassword))
        {
            yield return new ValidationResult(
                "DataSeeding:AdminPassword is required when CreateSeedAdmin is true.",
                new[] { nameof(AdminPassword) });
        }
    }
}