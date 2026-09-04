namespace E_Commerce.Application.Shared.Caching;

/// <summary>
/// Abstraction for cache-aside storage.
/// Infrastructure provides implementations (currently Redis).
/// </summary>
public interface ICache
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
}