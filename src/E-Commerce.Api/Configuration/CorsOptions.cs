using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Api.Configuration;

/// <summary>
/// Configuration for Cross-Origin Resource Sharing (CORS).
/// Bound from the "Cors" section. Affects browser clients only.
/// </summary>
public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    /// <summary>
    /// Origins allowed to call the API. Full origin required
    /// (scheme + host + port). Use "*" for any origin — only when
    /// <see cref="AllowCredentials"/> is false.
    /// </summary>
    [Required, MinLength(1)]
    public string[] AllowedOrigins { get; init; } = [];

    /// <summary>
    /// Allows cookies and Authorization headers on cross-origin requests.
    /// Requires explicit origins — cannot be combined with "*".
    /// </summary>
    public bool AllowCredentials { get; init; }

    /// <summary>
    /// Request headers permitted on cross-origin requests.
    /// </summary>
    [Required, MinLength(1)]
    public string[] AllowedHeaders { get; init; } =
    [
        "Content-Type",
        "Authorization",
        "X-Correlation-ID"
    ];

    /// <summary>
    /// HTTP methods permitted on cross-origin requests.
    /// </summary>
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