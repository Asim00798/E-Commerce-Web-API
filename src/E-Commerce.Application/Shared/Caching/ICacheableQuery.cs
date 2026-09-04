namespace E_Commerce.Application.Shared.Caching;

public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan CacheDuration { get; }
}