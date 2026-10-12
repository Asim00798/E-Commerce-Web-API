\# Caching Capability — Technical Requirements Document



\*\*E-Commerce Modular Monolith\*\*

\*\*Cross-Cutting Infrastructure Capability: Cache-Aside for Read-Heavy Queries\*\*

\*\*Version 1.0 — Final\*\*



\---



\## 1. Introduction



The Caching capability introduces a lightweight, opt-in cache-aside mechanism for read-heavy queries. It is not a new bounded context or a replacement for SQL Server as the source of truth. Instead, it is a cross-cutting infrastructure concern that reduces unnecessary database load for stable, frequently accessed read models.



Redis is the intended first-class implementation, but the Application layer depends only on a small `ICache` abstraction, preserving Clean Architecture boundaries.



\## 2. Business Goals



\* Improve response times for read-dominant endpoints that tolerate short-lived staleness.

\* Reduce database load for frequently accessed Catalog and product read models.

\* Provide a consistent caching pattern across contexts without duplicating caching logic in query handlers.

\* Keep the application resilient by treating Redis as disposable acceleration rather than an authoritative data store.

\* Maintain a clear separation between caching and business logic.



\## 3. Scope



\### 3.1 In Scope



\* `ICache` abstraction in the Application layer.

\* `ICacheableQuery` marker contract for opt-in caching.

\* MediatR pipeline behavior (`CachingBehavior`) implementing cache-aside.

\* Redis implementation of `ICache` in Infrastructure.

\* Dependency injection registration for Redis caching.

\* Initial opt-in caching for stable Catalog read queries.

\* Explicit invalidation after successful mutations affecting cached data.

\* Per-query TTL and deterministic cache keys.

\* JSON serialization and deserialization of cached values.

\* Graceful degradation when Redis is unavailable, with reads falling back to the database.



\### 3.2 Out of Scope



\* Write-through or write-behind caching.

\* Redis as authoritative cart storage, which is a separate potential concern.

\* Distributed locking or advanced cache-stampede prevention.

\* Cache tagging or dependency graphs.

\* Automatic caching for all queries.

\* Caching of commands or domain events.

\* Event-driven cache invalidation through the Outbox.

\* Custom Redis protocol implementation.



\## 4. Functional Requirements



\### 4.1 Cache Abstraction



The Application layer shall define an `ICache` interface with the following operations:



```csharp

Task<T?> GetAsync<T>(

&#x20;   string key,

&#x20;   CancellationToken ct = default);



Task SetAsync<T>(

&#x20;   string key,

&#x20;   T value,

&#x20;   TimeSpan expiration,

&#x20;   CancellationToken ct = default);



Task RemoveAsync(

&#x20;   string key,

&#x20;   CancellationToken ct = default);

```



The abstraction must not reference Redis, StackExchange.Redis, or any Infrastructure-specific type.



\### 4.2 Cacheable Query Contract



A query may implement `ICacheableQuery` to indicate that it is eligible for caching.



`ICacheableQuery` shall expose:



\* \*\*`CacheKey`\*\* — A deterministic string unique to the query parameters.

\* \*\*`CacheDuration`\*\* — The TTL for the cached result.



\### 4.3 Caching Behavior (MediatR Pipeline)



A `CachingBehavior<TRequest, TResponse>` shall be registered in the MediatR pipeline.



The behavior shall:



1\. Check whether the request implements `ICacheableQuery`.

2\. Pass non-cacheable requests through without caching.

3\. For cacheable requests, attempt to retrieve the result from `ICache`.

4\. On a cache hit, return the cached value without executing the handler.

5\. On a cache miss, execute the handler and then store the result using the query's TTL.

6\. Handle cache failures so that Redis unavailability does not prevent the query from executing and returning its database-backed result.



\### 4.4 Initial Cache Candidates



Catalog read queries are the first opt-in targets, specifically:



\* Product details by ID.

\* Product lists and searches where the data is relatively stable.

\* Categories.

\* Brands.



These queries must be selected carefully. Only queries that tolerate short-lived staleness should be cached.



\### 4.5 Cache Invalidation



\* Cached entries must be removed after a successful write that affects the cached data.

\* Invalidation must occur \*\*after the database transaction commits\*\*, not before.

\* Initially, invalidation is manual and explicit; automatic dependency detection is not required.

\* Example: after updating a product, remove the corresponding `catalog:product:{id}` cache entry.



\### 4.6 Serialization



\* Cached objects shall be serialized and deserialized using JSON in Infrastructure.

\* Application code must remain unaware of serialization details.

\* The serialization format must support the DTO and `Result` types used by queries.



\## 5. Non-Functional Requirements



\### 5.1 Performance



\* Cache hits should avoid database round-trips and be significantly faster than direct SQL reads.

\* Caching overhead, including serialization and Redis calls, should be minimal and must not noticeably degrade non-cached requests.



\### 5.2 Reliability



\* Redis failure shall not cause application failure for reads.

\* The system must continue fetching data from SQL Server when the cache is unavailable.

\* Cache entries may be lost at any time; the system must tolerate cold starts.



\### 5.3 Scalability



\* Redis connection management should use a singleton `IConnectionMultiplexer` per process.

\* The design should support multiple API instances sharing the same Redis instance.



\### 5.4 Maintainability



\* Caching mechanics shall be centralized in `CachingBehavior`, while cache policy—key and TTL—remains declarative at the query level.

\* No business logic should depend on cache internals.

\* Invalidation code should be simple and located near the relevant command handlers.



\### 5.5 Security



\* Sensitive data should not be cached unless caching is necessary and appropriate safeguards, including encryption where applicable and suitable TTLs, are in place.

\* Cache keys must not expose unnecessary internal information beyond what is needed.



\## 6. Architectural Constraints



\* SQL Server remains the source of truth; Redis is disposable acceleration.

\* Caching is \*\*opt-in\*\* per query, not global.

\* Cache-aside is the only supported caching strategy in the initial implementation.

\* Commands must not be cached; only queries may opt in.

\* The Domain layer must never reference caching abstractions or Redis.

\* The Application layer defines `ICache`; Infrastructure provides the Redis implementation.

\* Redis connections are managed through a singleton `IConnectionMultiplexer`.



\## 7. Key Design Decisions



| Concern                | Decision                                  |

| ---------------------- | ----------------------------------------- |

| Cache technology       | Redis                                     |

| .NET client            | StackExchange.Redis                       |

| Cache strategy         | Cache-aside                               |

| Application access     | `ICache` abstraction                      |

| CQRS integration       | MediatR pipeline behavior                 |

| Activation             | Explicit opt-in through `ICacheableQuery` |

| Initial targets        | Catalog read queries                      |

| Expiration             | Per-query TTL                             |

| Invalidation           | Explicit, after successful persistence    |

| Source of truth        | SQL Server                                |

| Serialization          | JSON at the Infrastructure layer          |

| Redis failure behavior | Fall back to the database for reads       |



\## 8. Acceptance Criteria



1\. `ICache` exists in Application and is independent of Infrastructure.

2\. `RedisCache` implements `ICache` using `IConnectionMultiplexer`.

3\. `CachingBehavior` correctly handles cache hits and misses for cacheable queries.

4\. Only queries implementing `ICacheableQuery` are cached.

5\. Each cacheable query provides a unique cache key and TTL.

6\. Redis failure does not prevent query execution.

7\. At least one Catalog read query, such as `GetProductByIdQuery`, is cacheable.

8\. Invalidation occurs after the database transaction commits for relevant mutations.

9\. Redis configuration is externalized through options.

10\. No Domain model or repository depends on caching.



\## 9. Out of Scope for This Version



\* Distributed locking and cache-stampede prevention.

\* Write-through or write-behind caching.

\* Cache event publication through the Outbox.

\* Advanced invalidation strategies, including tags and dependency tracking.

\* Caching user-specific or highly volatile data, such as cart contents, payment status, and shipment status.



\## 10. Summary



The Caching capability is an opt-in, cache-aside read-acceleration mechanism using Redis as an Infrastructure implementation. It centralizes caching mechanics in a MediatR behavior while keeping cache policy explicit per query.



The application remains resilient because Redis is not part of the authoritative data path, and failures degrade gracefully to SQL Server. The initial scope is limited to stable Catalog read models, with simple, explicit invalidation after successful writes.



This design preserves Clean Architecture principles and modular monolith boundaries while improving performance where it matters.



\---



\*\*Document version:\*\* 1.0 — Final

\*\*Capability:\*\* Caching

\*\*Scope:\*\* Cross-Cutting Infrastructure Capability



