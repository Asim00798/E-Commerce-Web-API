# Caching Capability — Integration Guide

**E-Commerce Modular Monolith**

**How to Integrate Cache-Aside into Queries and Commands**

**Version 2.0 — Final**

---

## 1. Purpose

This guide explains how to integrate the Caching capability into application use cases—primarily read-heavy queries—and how to manage cache invalidation after writes. It is intended for developers working within any bounded context who need to opt in to caching or maintain cache consistency.

This guide does not repeat the technical design; it focuses on practical integration steps and rules.

## 2. Prerequisites

Before integrating caching, ensure that:

* The `ICache` abstraction and `ICacheableQuery` interface are available in the Application layer.
* `CachingBehavior` is registered in the MediatR pipeline.
* Redis caching is registered via `AddRedisCaching` in the composition root.
* `Redis:Configuration` is present in the configuration, whether supplied through JSON, environment variables, or a secret manager.
* The target query returns a serializable DTO or `Result<T>`.
* The data is relatively stable and tolerant of short-lived staleness.

## 3. Making a Query Cacheable

To enable caching for a query, implement `ICacheableQuery` on the query record or class.

### Steps

1. Add the relevant namespaces, including `E_Commerce.Application.Shared.Caching` and the context's cache-key namespace.
2. Implement `ICacheableQuery`.
3. Provide a deterministic `CacheKey` through the context's key factory and define a `CacheDuration`.

### Example

```csharp
using E_Commerce.Application.BoundedContexts.Catalog.Caching;

public sealed record GetProductByIdQuery(Guid ProductId)
    : IRequest<Result<ProductDto>>, ICacheableQuery
{
    public string CacheKey => CatalogCacheKeys.Product(ProductId);

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}
```

No handler changes are needed. `CachingBehavior` automatically applies the cache-aside pattern.

## 4. Cache Key Guidelines

### 4.1 Use a Per-Context Key Factory

Cache keys are built through a static factory for each bounded context, rather than through raw string literals.

**Example file:** `Application/BoundedContexts/Catalog/Caching/CatalogCacheKeys.cs`

```csharp
public static class CatalogCacheKeys
{
    public static string Product(Guid productId)
        => $"catalog:product:{productId}";
}
```

**Why:** The same key is produced on the read side (`CacheKey`) and every invalidation side (command handlers). Repeating literals across files allows them to drift silently when one location is missed. The factory is the single source of truth.

**Rule:** Maintain one key factory per context. When adding a new cacheable resource, add a method to that context's factory.

### 4.2 Key Format Guidelines

* **Prefix by context:** Use prefixes such as `catalog:`, `ordering:`, and `shipping:` to prevent collisions across contexts sharing a Redis instance.
* **Include all query parameters that affect the result.** For example, search queries with `page=1` and `page=2` must produce different keys.
* **Do not include user-specific data under a shared key.** If the result is user-specific, include the user identifier or otherwise ensure the cache is isolated per user.
* **Keep keys deterministic and unique.**

### 4.3 Examples

```text
catalog:product:123
catalog:products:search:phone:page:1
catalog:categories
```

## 5. Cache Duration (TTL) Guidelines

* Choose the TTL based on how frequently the data changes.
* **Short TTLs** reduce staleness but lower the cache hit rate.
* **Long TTLs** improve the cache hit rate but increase the risk of serving stale data.
* Define the TTL per query. It reflects the data's volatility, not a global infrastructure setting.

### Suggested Defaults

| Resource               | Suggested TTL                         |
| ---------------------- | ------------------------------------- |
| Product details        | 10–15 minutes                         |
| Categories             | 30 minutes                            |
| Brands                 | 30 minutes                            |
| Product lists / search | 5–10 minutes, depending on volatility |

If the same TTL is repeated across several queries in a context, consider introducing a per-context duration registry, such as `CatalogCacheDurations`. It can follow the same general approach as the key factory. This is not required until the repetition is real.

## 6. Invalidation After Writes

When a command modifies data that is cached, invalidate the affected cache entries **after the database transaction succeeds**.

### 6.1 Auto-Mode Handlers (Default)

The command handler does not call `BeginTransactionAsync`. `SaveChangesAsync` opens, saves, and commits the transaction before returning. A direct invalidation call immediately afterward therefore runs post-commit.

```csharp
await _unitOfWork.SaveChangesAsync(ct);

await _cache.RemoveAsync(
    CatalogCacheKeys.Product(command.ProductId),
    CancellationToken.None);
```

### 6.2 Manual-Mode Handlers

The command handler calls `BeginTransactionAsync` first. `SaveChangesAsync` persists changes but does **not** commit; the handler commits later through `CommitTransactionAsync`.

A direct invalidation call after `SaveChangesAsync` would therefore execute **before the transaction commits**.

These handlers must schedule invalidation through `IPostCommitProcessor.Enqueue(...)`:

```csharp
await _unitOfWork.BeginTransactionAsync(ct);

// ... mutations ...

await _unitOfWork.SaveChangesAsync(ct);

_postCommitProcessor.Enqueue(async (sp, ct) =>
{
    var cache = sp.GetRequiredService<ICache>();

    await cache.RemoveAsync(
        CatalogCacheKeys.Product(command.ProductId),
        CancellationToken.None);
});

await _unitOfWork.CommitTransactionAsync(ct);
```

### 6.3 Rules

* **Never invalidate before the transaction commits.** If the write fails, the existing cache entry remains valid and can continue serving consistent data.
* **Pass `CancellationToken.None`.** Post-commit invalidation is a consequence of a committed write, not of the HTTP request. A client disconnect must not cancel the invalidation operation.
* **Invalidate only the exact keys known to be cached.** Broader invalidation of lists or collections can be added when those queries become cacheable.
* **Determine the transaction mode by checking for `BeginTransactionAsync`.** If it is present, the handler is manual-mode and must use `IPostCommitProcessor`.

### 6.4 Invalidation Failure

Invalidation failure is logged by `RedisCache.RemoveAsync` and swallowed. The database write has already succeeded, so a cache failure must not surface as an HTTP 500 response.

## 7. Handling Cache Failure

Redis is not authoritative. The system is designed to degrade gracefully when Redis is unavailable.

### 7.1 Failure Semantics

| Operation     | Caller            | Behavior on Redis Failure                                                                                   |
| ------------- | ----------------- | ----------------------------------------------------------------------------------------------------------- |
| `GetAsync`    | `CachingBehavior` | Throws `CacheException`; the behavior catches it, logs at Warning level, and falls through to the database. |
| `SetAsync`    | `CachingBehavior` | Throws `CacheException`; the behavior catches it, logs at Warning level, and returns the handler response.  |
| `RemoveAsync` | Command handler   | Logs at Warning level and swallows the exception. The command has already succeeded.                        |

### 7.2 Startup Behavior

* If `Redis:Configuration` is missing, the host fails to start with a clear error because configuration is validated through `ValidateOnStart`.
* If Redis is unreachable at startup, the host **still starts**. The multiplexer is created with `AbortOnConnectFail = false` and retries in the background.
* Cache operations may fail with `CacheException` until the connection is established. The caching behavior and invalidation path handle these failures.

Redis outages are therefore designed not to take down the application.

## 8. Integration Checklist

### Read Side (Query)

* [ ] The query implements `ICacheableQuery`.
* [ ] `CacheKey` is built through the context's key factory, not a raw string.
* [ ] `CacheDuration` reflects the data's volatility.
* [ ] The query result is serializable, such as `Result<T>` or a plain DTO.
* [ ] The operation is a read operation; commands are never cached.

### Write Side (Command)

* [ ] Every command that mutates cached data invalidates the affected keys.
* [ ] Invalidation runs **after commit**: a direct call in auto-mode handlers or `IPostCommitProcessor.Enqueue` in manual-mode handlers.
* [ ] `CancellationToken.None` is passed to invalidation.
* [ ] Cache keys are built through the same key factory used by the read side.

### Infrastructure

* [ ] `AddRedisCaching` is called from the composition root.
* [ ] `Redis:Configuration` is provided in all environments.
* [ ] `CachingBehavior` is registered in the MediatR pipeline.

## 9. Common Pitfalls

| Pitfall                                      | Why It's Wrong                                                                                   | Fix                                                                                                |
| -------------------------------------------- | ------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------- |
| **Caching commands**                         | Commands mutate state; caching them can return stale results and bypass expected execution.      | Only queries opt in through `ICacheableQuery`.                                                     |
| **Raw cache-key strings**                    | Read-side and invalidation-side keys can drift, causing invalidation to miss entries.            | Use the per-context key factory.                                                                   |
| **Invalidating before commit**               | If the transaction rolls back, a valid cache entry may have been evicted unnecessarily.          | Invalidate after `SaveChangesAsync` in auto mode or through `IPostCommitProcessor` in manual mode. |
| **Passing `ct` to invalidation**             | A client disconnect can skip invalidation, allowing stale data to persist until the TTL expires. | Pass `CancellationToken.None`.                                                                     |
| **Caching volatile data**                    | Cart, payment status, and shipment status can change frequently.                                 | Do not cache them, or use a very short TTL when justified.                                         |
| **Caching per-user data under a shared key** | Users may see each other's data.                                                                 | Include the user ID in the key or do not cache the result.                                         |
| **Ambiguous null versus cache miss**         | A cached null and a cache miss can become indistinguishable.                                     | The behavior refuses to cache null responses; do not work around this.                             |
| **Forgetting to invalidate**                 | Stale data persists until the TTL expires.                                                       | Review every mutating command for affected cache keys.                                             |

## 10. Common Integration Scenarios

### 10.1 Adding Caching to an Existing Query

1. Add the context's namespace for the key factory.
2. Implement `ICacheableQuery` on the query record.
3. Add a method to the context's key factory if the required key does not exist.
4. Set `CacheKey` to the factory call and `CacheDuration` to the appropriate TTL.
5. Add invalidation to every command that mutates the query's underlying data.

### 10.2 Adding a New Cacheable Resource

Add a method to the context's key factory:

```csharp
public static string Category(Guid categoryId)
    => $"catalog:category:{categoryId}";
```

Then:

1. Implement `ICacheableQuery` on the query.
2. Invalidate the affected key in every command that mutates the resource.

### 10.3 Invalidating Multiple Keys

If a command affects more than one cached resource—for example, a product update also invalidates a product-list cache—invalidate all affected keys after commit.

```csharp
await _unitOfWork.SaveChangesAsync(ct);

await _cache.RemoveAsync(
    CatalogCacheKeys.Product(command.ProductId),
    CancellationToken.None);

await _cache.RemoveAsync(
    CatalogCacheKeys.ProductList(),
    CancellationToken.None);
```

Do not wrap invalidation in a database transaction. Each `RemoveAsync` operation is independent and best-effort.

### 10.4 Migrating to a New Key Format

Because keys are produced by the factory, changing a key format can be performed in one place:

1. Update the relevant factory method.
2. Deploy the change.
3. Allow old entries to expire naturally according to their TTL; no cache migration is required.

Without the factory, this change would require finding and updating every occurrence of the old key literal across the codebase.

## 11. Summary

Integrating caching into a query involves implementing `ICacheableQuery`, using a key from the context's factory, and selecting a TTL that reflects the data's volatility. `CachingBehavior` applies the cache-aside pattern transparently.

Invalidation must run after the database transaction commits: use a direct call in auto-mode handlers and `IPostCommitProcessor.Enqueue` in manual-mode handlers. Pass `CancellationToken.None` and use the same key factory as the read side.

Redis failures are non-critical by design: reads fall through to the database, invalidation failures are logged and swallowed, and the host starts even when Redis is unreachable.

This opt-in approach keeps caching maintainable and aligned with the modular monolith architecture.

---

**Document version:** 2.0 — Final

**Capability:** Caching

**Scope:** Application Caching / Cross-Cutting Integration Guide
