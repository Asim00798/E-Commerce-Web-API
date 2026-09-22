using E_Commerce.Application.Shared.Caching;
using MediatR;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.Shared.Behaviors;

/// <summary>
/// Applies cache-aside to MediatR requests that implement ICacheableQuery.
/// On cache hit, returns cached result; on miss, executes handler and stores result.
/// Cache infrastructure failures are caught and logged, then the request falls back to the handler.
/// </summary>
public sealed class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        ICache cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheable)
            return await next();

        var cached = await TryGetFromCacheAsync(cacheable, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for key {CacheKey}", cacheable.CacheKey);
            return cached;
        }

        _logger.LogDebug("Cache miss for key {CacheKey}", cacheable.CacheKey);
        var response = await next();

        if (response is null)
        {
            _logger.LogDebug("Handler returned null for key {CacheKey}; not caching.", cacheable.CacheKey);
            return response;
        }

        await TrySetInCacheAsync(cacheable, response, cancellationToken);

        return response;
    }

    // ------------------------------------------------------------------
    // Cache operations
    // ------------------------------------------------------------------

    /// <summary>
    /// Attempts to read the cached response. Returns <c>default</c> if the
    /// cache misses or the cache infrastructure fails — the caller falls
    /// through to the handler.
    /// </summary>
    private async Task<TResponse?> TryGetFromCacheAsync(
        ICacheableQuery cacheable,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _cache.GetAsync<TResponse>(cacheable.CacheKey, cancellationToken);
        }
        catch (CacheException ex)
        {
            _logger.LogWarning(
                ex,
                "Cache get failed for key {CacheKey}. Falling back to database.",
                cacheable.CacheKey);

            return default;
        }
    }

    /// <summary>
    /// Attempts to store the handler's response. Failures are logged and
    /// swallowed — the request has already succeeded and caching is
    /// best-effort.
    /// </summary>
    private async Task TrySetInCacheAsync(
        ICacheableQuery cacheable,
        TResponse response,
        CancellationToken cancellationToken)
    {
        try
        {
            await _cache.SetAsync(
                cacheable.CacheKey,
                response,
                cacheable.CacheDuration,
                cancellationToken);
        }
        catch (CacheException ex)
        {
            _logger.LogWarning(ex, "Cache set failed for key {CacheKey}.", cacheable.CacheKey);
        }
    }
}