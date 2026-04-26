namespace E_Commerce.Api.Configuration;

public class CorsOptions
{
    public string PolicyName { get; set; } = "DefaultCorsPolicy";
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}
