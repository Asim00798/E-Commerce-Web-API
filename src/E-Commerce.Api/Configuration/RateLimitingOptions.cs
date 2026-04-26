namespace E_Commerce.Api.Configuration;

public class RateLimitingOptions
{
    public int PermitLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
}
