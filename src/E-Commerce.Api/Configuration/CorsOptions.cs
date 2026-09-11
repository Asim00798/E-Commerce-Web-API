using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Api.Configuration;

public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    [Required, MinLength(1)]
    public string[] AllowedOrigins { get; init; } = [];

    public bool AllowCredentials { get; init; }

    [Required, MinLength(1)]
    public string[] AllowedHeaders { get; init; } =
    [
        "Content-Type",
        "Authorization",
        "X-Correlation-ID"
    ];

    [Required, MinLength(1)]
    public string[] AllowedMethods { get; init; } =
    [
        "GET",
        "POST",
        "PUT",
        "PATCH",
        "DELETE"
    ];
}