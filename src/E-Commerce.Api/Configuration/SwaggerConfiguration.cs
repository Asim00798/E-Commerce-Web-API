using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Api.Configuration;

/// <summary>
/// Configuration for the Swagger / OpenAPI document.
/// Bound from the "Swagger" section. Populates the OpenAPI <c>info</c> block.
/// </summary>
public sealed class SwaggerConfiguration
{
    public const string SectionName = "Swagger";

    /// <summary>
    /// Display title of the API.
    /// </summary>
    [Required]
    public string Title { get; set; } = "E-Commerce API";

    /// <summary>
    /// Version of the API document. Also used in the JSON endpoint path
    /// (<c>/swagger/{Version}/swagger.json</c>). Keep aligned with
    /// <see cref="VersioningOptions"/>.
    /// </summary>
    [Required]
    public string Version { get; set; } = "v1";

    /// <summary>
    /// Short description of the API.
    /// </summary>
    [Required]
    public string Description { get; set; } = "E-Commerce Modular Monolith API";

    /// <summary>
    /// Name of the team or individual responsible for the API.
    /// </summary>
    [Required]
    public string ContactName { get; set; } = "API Team";

    /// <summary>
    /// Contact email for API-related inquiries.
    /// </summary>
    [Required, EmailAddress]
    public string ContactEmail { get; set; } = "api@ecommerce.local";
}