using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Api.Configuration;

public sealed class SwaggerConfiguration
{
    public const string SectionName = "Swagger";

    [Required]
    public string Title { get; set; } = "E-Commerce API";

    [Required]
    public string Version { get; set; } = "v1";

    [Required]
    public string Description { get; set; } = "E-Commerce Modular Monolith API";

    [Required]
    public string ContactName { get; set; } = "API Team";

    [Required, EmailAddress]
    public string ContactEmail { get; set; } = "api@ecommerce.local";
}