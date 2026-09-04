using E_Commerce.Application.Shared.Caching;
using StackExchange.Redis;
using System.Text.Json;

namespace E_Commerce.Infrastructure.Caching;

/// <summary>
/// Redis implementation of ICache. Uses a shared IConnectionMultiplexer singleton.
/// Serializes values as JSON. Throws CacheException for Redis failures.
/// </summary>
public sealed class RedisCache : ICache
{
    private readonly IDatabase _db;

    public RedisCache(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex) when (ex is RedisException or RedisTimeoutException or RedisConnectionException)
        {
            throw new CacheException($"Failed to get cache key {key}", ex);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct = default)
    {
        try
        {
            var serialized = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, serialized, expiration);
        }
        catch (Exception ex) when (ex is RedisException or RedisTimeoutException or RedisConnectionException)
        {
            throw new CacheException($"Failed to set cache key {key}", ex);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _db.KeyDeleteAsync(key);
        }
        catch (Exception ex) when (ex is RedisException or RedisTimeoutException or RedisConnectionException)
        {
            throw new CacheException($"Failed to remove cache key {key}", ex);
        }
    }
}