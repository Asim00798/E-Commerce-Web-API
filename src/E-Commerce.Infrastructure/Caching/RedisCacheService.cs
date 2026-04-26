namespace E_Commerce.Infrastructure.Caching;

/// <summary>
/// Redis-backed distributed cache implementation of <see cref="ICacheService"/>.
/// Suitable for multi-instance / cloud deployments.
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    // TODO: Inject IConnectionMultiplexer (StackExchange.Redis)

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        // TODO: Implement
        throw new NotImplementedException();
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        // TODO: Implement
        throw new NotImplementedException();
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        // TODO: Implement
        throw new NotImplementedException();
    }
}
