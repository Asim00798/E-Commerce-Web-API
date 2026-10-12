\# Caching Capability — Technical Reference Document



\*\*E-Commerce Modular Monolith\*\*

\*\*Cross-Cutting Infrastructure Capability: Cache-Aside for Read-Heavy Queries\*\*

\*\*Version 2.0 — Final\*\*



\---



\## 1. Introduction



The Caching capability provides a lightweight, opt-in cache-aside mechanism for read-heavy queries. It is implemented as a cross-cutting infrastructure concern using Redis as the first-class provider, while SQL Server remains the authoritative data source. The capability reduces database load and improves response times for stable, frequently accessed read models.



The design follows Clean Architecture principles: the Application layer defines a small `ICache` abstraction, and Infrastructure implements it using Redis. Caching is applied centrally through a MediatR pipeline behavior, enabling consistent mechanics without polluting query handlers.



Redis is treated as \*\*disposable acceleration\*\*. Its unavailability is designed not to fail a request: reads fall through to the database, and invalidation failures are logged and swallowed.



\## 2. Purpose



This document describes the technical design, components, folder structure, workflows, and integration points for the Caching capability. It complements the Caching Technical Requirements Document and guides developers implementing or extending caching within the modular monolith.



\## 3. Architecture Overview



The caching architecture is built around the cache-aside pattern:



\* Application queries opt in to caching by implementing `ICacheableQuery`.

\* A MediatR `CachingBehavior` checks the cache before executing the query.

\* On a cache hit, the cached result is returned without querying the database.

\* On a cache miss, the query handler executes, and the result is stored in Redis.

\* Redis is never the system of record; SQL Server remains authoritative.

\* Cache invalidation occurs explicitly after successful database mutations.



\### Cache-Aside Flow



```text

&#x20;                Query (implements ICacheableQuery)

&#x20;                             |

&#x20;                             v

&#x20;                    CachingBehavior

&#x20;                             |

&#x20;                  +----------+----------+

&#x20;                  |                     |

&#x20;                  v                     v

&#x20;               Cache Hit             Cache Miss

&#x20;                  |                     |

&#x20;                  v                     v

&#x20;             Redis (Get)           Query Handler

&#x20;                  |                     |

&#x20;                  v                     v

&#x20;               Response             SQL Server

&#x20;                                        |

&#x20;                                        v

&#x20;                                    Redis (Set)

&#x20;                                        |

&#x20;                                        v

&#x20;                                    Response

```



\## 4. Folder Structure



\### 4.1 Application Layer



```text

Application/

├── Shared/

│   ├── Caching/

│   │   ├── ICache.cs

│   │   ├── ICacheableQuery.cs

│   │   └── CacheException.cs

│   └── Behaviors/

│       └── CachingBehavior.cs

```



Per-context cache key factories live with the contexts they serve:



```text

Application/

└── BoundedContexts/

&#x20;   └── Catalog/

&#x20;       └── Caching/

&#x20;           └── CatalogCacheKeys.cs

```



`CatalogCacheKeys.cs` provides the key factory for Catalog cache keys.



\### 4.2 Infrastructure Layer



```text

Infrastructure/

└── Caching/

&#x20;   ├── Configuration/

&#x20;   │   └── RedisOptions.cs

&#x20;   ├── Extensions/

&#x20;   │   └── RedisCachingExtensions.cs

&#x20;   └── Services/

&#x20;       └── RedisCache.cs

```



\### 4.3 Configuration



Example `appsettings.json`:



```json

{

&#x20; "Redis": {

&#x20;   "Configuration": "localhost:6379"

&#x20; }

}

```



In production, `Redis:Configuration` is supplied through an environment variable (`Redis\_\_Configuration`) or a secret manager. Configuration containing credentials must never be committed to source control.



\## 5. Application Layer Components



\### 5.1 ICache Abstraction



Defines the core caching operations. Infrastructure provides the implementation.



```csharp

public interface ICache

{

&#x20;   Task<T?> GetAsync<T>(

&#x20;       string key,

&#x20;       CancellationToken ct = default);



&#x20;   Task SetAsync<T>(

&#x20;       string key,

&#x20;       T value,

&#x20;       TimeSpan expiration,

&#x20;       CancellationToken ct = default);



&#x20;   Task RemoveAsync(

&#x20;       string key,

&#x20;       CancellationToken ct = default);

}

```



\### 5.2 ICacheableQuery Contract



Marks a query as cacheable. Implementing queries provide a unique cache key and a time-to-live (TTL).



```csharp

public interface ICacheableQuery

{

&#x20;   string CacheKey { get; }



&#x20;   TimeSpan CacheDuration { get; }

}

```



\*\*Cache key:\*\* Built through the context's cache key factory, such as `CatalogCacheKeys.Product(id)`, never through a raw string literal. This helps prevent read-side and invalidation-side keys from drifting.



\*\*TTL:\*\* Declared on the query. It is a property of the data's volatility, not of the infrastructure. Defining it on the query makes the caching policy self-describing at the query site.



\### 5.3 CacheException



An Application-owned exception raised by Infrastructure when a Redis operation fails. It allows `CachingBehavior` and command handlers to distinguish cache failures from genuine business failures.



```csharp

public sealed class CacheException : Exception

{

&#x20;   public CacheException(

&#x20;       string message,

&#x20;       Exception innerException)

&#x20;       : base(message, innerException)

&#x20;   {

&#x20;   }

}

```



\### 5.4 CachingBehavior (MediatR Pipeline Behavior)



Implements cache-aside logic by intercepting requests that implement `ICacheableQuery`. On cache failure, it logs the error and falls through to the handler.



```csharp

public sealed class CachingBehavior<TRequest, TResponse>

&#x20;   : IPipelineBehavior<TRequest, TResponse>

&#x20;   where TRequest : notnull

{

&#x20;   private readonly ICache \_cache;

&#x20;   private readonly ILogger<CachingBehavior<TRequest, TResponse>> \_logger;



&#x20;   public CachingBehavior(

&#x20;       ICache cache,

&#x20;       ILogger<CachingBehavior<TRequest, TResponse>> logger)

&#x20;   {

&#x20;       \_cache = cache;

&#x20;       \_logger = logger;

&#x20;   }



&#x20;   public async Task<TResponse> Handle(

&#x20;       TRequest request,

&#x20;       RequestHandlerDelegate<TResponse> next,

&#x20;       CancellationToken cancellationToken)

&#x20;   {

&#x20;       if (request is not ICacheableQuery cacheable)

&#x20;           return await next();



&#x20;       var cached = await TryGetFromCacheAsync(

&#x20;           cacheable,

&#x20;           cancellationToken);



&#x20;       if (cached is not null)

&#x20;       {

&#x20;           \_logger.LogDebug(

&#x20;               "Cache hit for key {CacheKey}",

&#x20;               cacheable.CacheKey);



&#x20;           return cached;

&#x20;       }



&#x20;       \_logger.LogDebug(

&#x20;           "Cache miss for key {CacheKey}",

&#x20;           cacheable.CacheKey);



&#x20;       var response = await next();



&#x20;       if (response is null)

&#x20;       {

&#x20;           \_logger.LogDebug(

&#x20;               "Handler returned null for key {CacheKey}; not caching.",

&#x20;               cacheable.CacheKey);



&#x20;           return response;

&#x20;       }



&#x20;       await TrySetInCacheAsync(

&#x20;           cacheable,

&#x20;           response,

&#x20;           cancellationToken);



&#x20;       return response;

&#x20;   }



&#x20;   private async Task<TResponse?> TryGetFromCacheAsync(

&#x20;       ICacheableQuery cacheable,

&#x20;       CancellationToken cancellationToken)

&#x20;   {

&#x20;       try

&#x20;       {

&#x20;           return await \_cache.GetAsync<TResponse>(

&#x20;               cacheable.CacheKey,

&#x20;               cancellationToken);

&#x20;       }

&#x20;       catch (CacheException ex)

&#x20;       {

&#x20;           \_logger.LogWarning(

&#x20;               ex,

&#x20;               "Cache get failed for key {CacheKey}. Falling back to database.",

&#x20;               cacheable.CacheKey);



&#x20;           return default;

&#x20;       }

&#x20;   }



&#x20;   private async Task TrySetInCacheAsync(

&#x20;       ICacheableQuery cacheable,

&#x20;       TResponse response,

&#x20;       CancellationToken cancellationToken)

&#x20;   {

&#x20;       try

&#x20;       {

&#x20;           await \_cache.SetAsync(

&#x20;               cacheable.CacheKey,

&#x20;               response,

&#x20;               cacheable.CacheDuration,

&#x20;               cancellationToken);

&#x20;       }

&#x20;       catch (CacheException ex)

&#x20;       {

&#x20;           \_logger.LogWarning(

&#x20;               ex,

&#x20;               "Cache set failed for key {CacheKey}.",

&#x20;               cacheable.CacheKey);

&#x20;       }

&#x20;   }

}

```



\*\*Key properties:\*\*



\* \*\*Opt-in:\*\* Non-cacheable requests pass through untouched.

\* \*\*Fault-tolerant:\*\* `CacheException` from `GetAsync` or `SetAsync` is caught and logged at Warning level. Cache failures do not prevent the handler response from being returned.

\* \*\*Null-safe:\*\* If the handler returns `null`, the behavior skips caching. Otherwise, a cache miss and a cached-null result could be indistinguishable.

\* \*\*Pipeline position:\*\* Intended to execute after authorization and before the query handler, according to the configured MediatR behavior order.



\## 6. Infrastructure Layer Components



\### 6.1 RedisOptions



```csharp

public sealed class RedisOptions

{

&#x20;   public const string SectionName = "Redis";



&#x20;   public string Configuration { get; set; } = string.Empty;

}

```



There is no default connection value. The connection configuration must be explicitly supplied through `appsettings.json`, an environment variable, or a secret manager.



`ValidateOnStart` rejects an empty configuration so that a missing section produces a clear startup error rather than silently defaulting to localhost.



\### 6.2 RedisCache



Implements `ICache` using StackExchange.Redis. It receives the singleton `IConnectionMultiplexer` through constructor injection.



```csharp

public sealed class RedisCache : ICache

{

&#x20;   private readonly IDatabase \_db;

&#x20;   private readonly ILogger<RedisCache> \_logger;



&#x20;   public RedisCache(

&#x20;       IConnectionMultiplexer redis,

&#x20;       ILogger<RedisCache> logger)

&#x20;   {

&#x20;       \_db = redis.GetDatabase();

&#x20;       \_logger = logger;

&#x20;   }



&#x20;   public async Task<T?> GetAsync<T>(

&#x20;       string key,

&#x20;       CancellationToken ct = default)

&#x20;   {

&#x20;       try

&#x20;       {

&#x20;           var value = await \_db.StringGetAsync(key);



&#x20;           if (value.IsNullOrEmpty)

&#x20;               return default;



&#x20;           return JsonSerializer.Deserialize<T>(value!);

&#x20;       }

&#x20;       catch (Exception ex) when (

&#x20;           ex is RedisException

&#x20;               or RedisTimeoutException

&#x20;               or RedisConnectionException)

&#x20;       {

&#x20;           throw new CacheException(

&#x20;               $"Failed to get cache key {key}",

&#x20;               ex);

&#x20;       }

&#x20;   }



&#x20;   public async Task SetAsync<T>(

&#x20;       string key,

&#x20;       T value,

&#x20;       TimeSpan expiration,

&#x20;       CancellationToken ct = default)

&#x20;   {

&#x20;       try

&#x20;       {

&#x20;           var serialized = JsonSerializer.Serialize(value);



&#x20;           await \_db.StringSetAsync(

&#x20;               key,

&#x20;               serialized,

&#x20;               expiration);

&#x20;       }

&#x20;       catch (Exception ex) when (

&#x20;           ex is RedisException

&#x20;               or RedisTimeoutException

&#x20;               or RedisConnectionException)

&#x20;       {

&#x20;           throw new CacheException(

&#x20;               $"Failed to set cache key {key}",

&#x20;               ex);

&#x20;       }

&#x20;   }



&#x20;   public async Task RemoveAsync(

&#x20;       string key,

&#x20;       CancellationToken ct = default)

&#x20;   {

&#x20;       try

&#x20;       {

&#x20;           await \_db.KeyDeleteAsync(key);

&#x20;       }

&#x20;       catch (Exception ex) when (

&#x20;           ex is RedisException

&#x20;               or RedisTimeoutException

&#x20;               or RedisConnectionException)

&#x20;       {

&#x20;           \_logger.LogWarning(

&#x20;               ex,

&#x20;               "Cache removal failed for key {Key}",

&#x20;               key);



&#x20;           // Swallowed by design: invalidation runs post-commit

&#x20;           // and must not fail the caller.

&#x20;       }

&#x20;   }

}

```



\*\*Failure semantics differ by operation deliberately:\*\*



| Operation     | Behavior on Redis Failure        | Reason                                                                                                       |

| ------------- | -------------------------------- | ------------------------------------------------------------------------------------------------------------ |

| `GetAsync`    | Throws `CacheException`.         | `CachingBehavior` catches the exception and falls through to the database.                                   |

| `SetAsync`    | Throws `CacheException`.         | `CachingBehavior` catches the exception and continues; the response has already been produced.               |

| `RemoveAsync` | Logs and swallows the exception. | Called after commit; the write has already succeeded, so failing the request would misrepresent the outcome. |



\### 6.3 RedisCachingExtensions



```csharp

public static class RedisCachingExtensions

{

&#x20;   public static IServiceCollection AddRedisCaching(

&#x20;       this IServiceCollection services,

&#x20;       IConfiguration configuration)

&#x20;   {

&#x20;       services.AddOptions<RedisOptions>()

&#x20;           .Bind(configuration.GetSection(RedisOptions.SectionName))

&#x20;           .Validate(

&#x20;               o => !string.IsNullOrWhiteSpace(o.Configuration),

&#x20;               "Redis:Configuration must be provided.")

&#x20;           .ValidateOnStart();



&#x20;       services.AddSingleton<IConnectionMultiplexer>(

&#x20;           CreateMultiplexer);



&#x20;       services.AddSingleton<ICache, RedisCache>();



&#x20;       return services;

&#x20;   }



&#x20;   private static IConnectionMultiplexer CreateMultiplexer(

&#x20;       IServiceProvider sp)

&#x20;   {

&#x20;       var options = sp

&#x20;           .GetRequiredService<IOptions<RedisOptions>>()

&#x20;           .Value;



&#x20;       var configuration =

&#x20;           ConfigurationOptions.Parse(options.Configuration);



&#x20;       configuration.AbortOnConnectFail = false;



&#x20;       return ConnectionMultiplexer.Connect(configuration);

&#x20;   }

}

```



\*\*Key decisions:\*\*



\* \*\*`ValidateOnStart`:\*\* Missing configuration fails startup rather than the first cache call, surfacing deployment configuration errors immediately.

\* \*\*`AbortOnConnectFail = false`:\*\* An unreachable Redis instance at host startup does not, by itself, prevent the application from starting. The multiplexer can retry in the background, and cache operations may fail until connectivity is restored.



\## 7. Cacheable Query Example



`GetProductByIdQuery` implements `ICacheableQuery`:



```csharp

using E\_Commerce.Application.BoundedContexts.Catalog.Caching;



\[AuthorizePermission(CatalogPermissions.ViewProducts)]

public sealed record GetProductByIdQuery(Guid ProductId)

&#x20;   : IRequest<Result<ProductDto>>, ICacheableQuery

{

&#x20;   public string CacheKey =>

&#x20;       CatalogCacheKeys.Product(ProductId);



&#x20;   public TimeSpan CacheDuration =>

&#x20;       TimeSpan.FromMinutes(10);

}

```



The handler remains unchanged; caching is applied transparently by the pipeline behavior.



\## 8. Cache Key Management



Cache keys are constructed through per-context factories rather than raw string literals.



\*\*Example file:\*\* `Application/BoundedContexts/Catalog/Caching/CatalogCacheKeys.cs`



```csharp

public static class CatalogCacheKeys

{

&#x20;   public static string Product(Guid productId) =>

&#x20;       $"catalog:product:{productId}";

}

```



\*\*Why:\*\* The same key must be produced on both the read side (`GetProductByIdQuery.CacheKey`) and every invalidation side (each command handler that mutates a product). A repeated literal can drift silently when one occurrence is missed.



\*\*Rule:\*\* Maintain one cache key factory per bounded context. Adding a new cacheable resource adds a method to that context's factory.



\## 9. Cache-Aside Flow



1\. The client sends a request.

2\. The MediatR pipeline reaches `CachingBehavior`.

3\. The behavior checks whether the query implements `ICacheableQuery`.



&#x20;  \* \*\*No:\*\* Pass the request to the next handler.

&#x20;  \* \*\*Yes:\*\* Continue with the cache lookup.

4\. The behavior attempts to retrieve the result from Redis using the query's `CacheKey`.



&#x20;  \* \*\*Cache hit:\*\* Return the cached response without invoking the query handler.

&#x20;  \* \*\*Cache miss:\*\* Invoke the query handler.

5\. After the handler returns a response, store it in Redis using the query's `CacheDuration`, unless the response is `null`.

6\. Return the response to the caller.



\## 10. Serialization



\* `RedisCache` uses `System.Text.Json` for serialization and deserialization.

\* The Application layer is unaware of serialization details.

\* Cached types must round-trip correctly through `System.Text.Json`. `Result<T>` and `Result` carry `\[JsonConstructor]` for this purpose.

\* Types with private constructors, enums serialized as integers, or polymorphic shapes require verification before caching.



\## 11. Invalidation Strategy



Invalidation is explicit and manual. There is no automatic dependency detection.



\### 11.1 Placement Rule



Cache invalidation must run \*\*after the database transaction commits\*\*.



\*\*Auto-mode handlers\*\* (the default): `SaveChangesAsync` commits before returning, so a direct call to `ICache.RemoveAsync` immediately afterward satisfies the rule.



```csharp

await \_unitOfWork.SaveChangesAsync(ct);



await \_cache.RemoveAsync(

&#x20;   CatalogCacheKeys.Product(command.ProductId),

&#x20;   CancellationToken.None);

```



\*\*Manual-mode handlers\*\* (handlers that call `BeginTransactionAsync` first): `SaveChangesAsync` does not commit. A direct invalidation call would execute before the commit. These handlers must invalidate through `IPostCommitProcessor.Enqueue(...)` instead.



\### 11.2 Cancellation



Post-commit invalidation is a consequence of a committed write, not of the HTTP request. Pass `CancellationToken.None` so a client disconnect does not skip invalidation.



\### 11.3 Failure



Invalidation failure is logged and swallowed. The write has already succeeded; surfacing a cache failure as an HTTP 500 would misrepresent the outcome.



\### 11.4 Scope



Only the exact keys known to be cached are invalidated. Broader invalidation of lists and collections is deferred until those queries are actually cacheable.



\## 12. Failure Handling and Resilience



Redis is disposable acceleration. The capability defines the following failure semantics:



| Scenario                          | Behavior                                                                                                                                                              |

| --------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |

| Redis unavailable at host startup | The host starts if the remaining startup process succeeds; the multiplexer is configured to retry connections in the background through `AbortOnConnectFail = false`. |

| `Redis:Configuration` missing     | The host fails to start because of `ValidateOnStart`.                                                                                                                 |

| `GetAsync` fails at runtime       | `CacheException` is thrown; `CachingBehavior` catches it, logs at Warning level, and falls through to the handler.                                                    |

| `SetAsync` fails at runtime       | `CacheException` is thrown; `CachingBehavior` catches it, logs at Warning level, and returns the handler response.                                                    |

| `RemoveAsync` fails at runtime    | The failure is logged at Warning level and swallowed. The command has already succeeded.                                                                              |



\*\*Important:\*\* Cache failures are designed not to fail requests. This is the load-bearing resilience property of the capability, subject to the application's other dependencies and failure modes.



\## 13. Concurrency and Cache Stampede



The current version does not implement stampede protection such as locking, request coalescing, or stale-while-revalidate. This is considered acceptable for the initial scope because:



\* Cached data is read-heavy.

\* TTLs are short.

\* SQL Server remains authoritative and is expected to handle cold-start bursts.



Stampede mitigation can be introduced later without changing the `ICache` abstraction; it could be implemented within `CachingBehavior` or another appropriate caching component.



\## 14. Integration with Other Capabilities



\* \*\*HTTP Caching:\*\* Independent. `\[CacheControl]` sets response headers for client or proxy caching. The two mechanisms serve different consumers: Redis serves the API's application-level data access, while HTTP caching serves clients and intermediaries. Both may apply to the same endpoint when their policies are compatible.

\* \*\*ReadModel:\*\* Independent. ReadModel query results can be cached through `ICacheableQuery` when the projection is sufficiently stable. Currently, no ReadModel query implements `ICacheableQuery`.

\* \*\*Outbox:\*\* Independent. The Outbox handles durable cross-context communication; caching handles repeated reads.



\## 15. Testing



\### 15.1 Unit Tests



\*\*`CachingBehavior` with mocked `ICache`:\*\*



\* Cache hit returns the cached value without invoking the handler.

\* Cache miss invokes the handler and stores the result.

\* A non-cacheable query passes through without cache access.

\* Cache failure during `GetAsync` falls through to the handler.

\* Cache failure during `SetAsync` still returns the handler response.

\* A `null` handler response is not cached.



\*\*`RedisCache` (optional, using a mocked `IDatabase`):\*\*



\* Serialization and deserialization round-trip.

\* `RemoveAsync` swallows Redis exceptions.

\* `GetAsync` and `SetAsync` wrap Redis exceptions in `CacheException`.



\### 15.2 Integration Tests



\* Redis connection and basic operations, if Redis is available in the test environment.

\* Cache invalidation after a command succeeds.

\* Graceful fallback when the Redis connection fails.



\### 15.3 API Tests



\* Verify that cacheable endpoints return correct data.

\* Verify that cache invalidation occurs after mutations, indirectly by confirming that a subsequent GET returns fresh data.



\## 16. Acceptance Criteria



1\. `ICache` exists in Application and is independent of Infrastructure.

2\. `RedisCache` implements `ICache` using `IConnectionMultiplexer`.

3\. `CachingBehavior` correctly handles cache hits and misses for cacheable queries.

4\. Only queries implementing `ICacheableQuery` are cached.

5\. Each cacheable query provides a unique cache key and TTL.

6\. Redis failure does not prevent query execution.

7\. Cache keys are centralized per context; raw key strings do not appear in handlers.

8\. Invalidation occurs after database commit for relevant mutations, using `CancellationToken.None`.

9\. `RedisCache.RemoveAsync` swallows Redis failures, while `GetAsync` and `SetAsync` throw `CacheException`.

10\. Redis configuration is externalized, and `ValidateOnStart` rejects empty configuration.

11\. `AbortOnConnectFail = false` allows the host to start when Redis is unavailable, provided no other startup failure occurs.

12\. No Domain model or repository depends on caching.



\## 17. Summary



The Caching capability provides a centralized, opt-in cache-aside mechanism for read-heavy queries. It uses Redis as an Infrastructure implementation while keeping the Application layer free of Redis-specific dependencies. The MediatR pipeline behavior provides consistent caching mechanics, per-context cache key factories prevent key drift, and per-query TTLs keep policies declarative.



Invalidation is explicit and post-commit, with cancellation-independent execution and swallowed failures. Redis is treated as non-critical throughout: reads fall through, invalidation logs and continues, and the host can start even when Redis is unreachable.



This design improves performance for stable read models without compromising architectural boundaries or data integrity.



\---



\*\*Document version:\*\* 2.0 — Final

\*\*Capability:\*\* Caching

\*\*Scope:\*\* Cross-Cutting Infrastructure Capability



