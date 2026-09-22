namespace E_Commerce.Infrastructure.Caching.Configuration;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string Configuration { get; set; } = string.Empty;
}