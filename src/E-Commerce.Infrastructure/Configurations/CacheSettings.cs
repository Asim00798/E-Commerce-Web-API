namespace E_Commerce.Infrastructure.Configurations;

/// <summary>
/// Strongly-typed cache settings.
/// Bind from <c>appsettings.json</c> section <c>"Cache"</c>.
/// </summary>
public sealed class CacheSettings
{
    public const string SectionName = "Cache";

    /// <summary>Redis connection string. Empty means use in-memory cache.</summary>
    public string? RedisConnectionString { get; init; }

    /// <summary>Default item expiry in minutes.</summary>
    public int DefaultExpiryMinutes { get; init; } = 30;

    /// <summary>Key prefix applied to all cache entries.</summary>
    public string KeyPrefix { get; init; } = "ecommerce";
}
