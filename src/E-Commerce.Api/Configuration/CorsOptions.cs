namespace E_Commerce.Api.Configuration;

public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; init; } = [];

    public bool AllowCredentials { get; init; }

    public string[] AllowedHeaders { get; init; } =
    [
        "Content-Type",
        "Authorization",
        "X-Correlation-ID"
    ];

    public string[] AllowedMethods { get; init; } =
    [
        "GET",
        "POST",
        "PUT",
        "PATCH",
        "DELETE"
    ];
}
