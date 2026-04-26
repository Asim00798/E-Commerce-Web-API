using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using E_Commerce.Application.Common.Abstractions;

namespace E_Commerce.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse>(
    IMemoryCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(request.CacheKey, out TResponse? cachedResponse))
        {
            logger.LogInformation("Cache hit for {CacheKey}", request.CacheKey);
            return cachedResponse!;
        }

        logger.LogInformation("Cache miss for {CacheKey}. Fetching from source.", request.CacheKey);
        var response = await next();

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = request.Expiration ?? TimeSpan.FromMinutes(5)
        };

        cache.Set(request.CacheKey, response, cacheOptions);

        return response;
    }
}
