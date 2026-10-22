# HTTP Caching – Integration Guide

**E-Commerce Modular Monolith**

**How to Apply Cache-Control Headers to Safe Catalog GET Endpoints**

**Version: 1.0 – Final**

---

## 1. Purpose

This guide explains how to integrate the HTTP Caching capability into API controllers. It focuses on applying the `[CacheControl]` attribute to safe, public, relatively stable Catalog GET endpoints.

The target audience is developers working on the API layer who need to add or modify HTTP caching policies for responses.

This guide does not cover CDN or reverse-proxy configuration, ETags, or advanced conditional requests.

---

## 2. Prerequisites

Before applying HTTP caching, ensure that:

* `CacheControlAttribute` and `CacheControlFilter` are implemented in the API layer.
* The `AddHttpCaching()` extension method is called in `Program.cs` after `AddControllers()`. It configures `MvcOptions` without re-registering MVC.
* The endpoint uses the **GET** method.
* The response data is **public and relatively stable**, making it safe for browsers and shared caches to store.
* You understand the caching directives you intend to use.

---

## 3. Applying the Attribute

Use the `[CacheControl]` attribute on an action method or controller class.

### Basic Example

```csharp
[HttpGet("{id:guid}")]
[CacheControl(Public = true, MaxAge = 600)]
public async Task<IActionResult> GetProduct(Guid id, CancellationToken ct)
{
    // ...
}
```

This adds the following header to the response:

```http
Cache-Control: public, max-age=600
```

### Controller-Level Example

```csharp
[ApiController]
[Route("api/catalog/categories")]
[CacheControl(Public = true, MaxAge = 1800)]
public sealed class CategoriesController : BaseApiController
{
    // All actions inherit this policy unless overridden.
}
```

### Precedence Rule

The filter checks for the attribute first on the action method, then on the controller class.

If an action-level attribute is present, it completely overrides the controller-level attribute for that action. This allows you to define a general controller policy and override it for individual actions when necessary.

---

## 4. Choosing Directives

| Scenario                                                      | Recommended Attribute                                                |
| ------------------------------------------------------------- | -------------------------------------------------------------------- |
| Public, stable product details                                | `[CacheControl(Public = true, MaxAge = 600)]`                        |
| Public categories or brands                                   | `[CacheControl(Public = true, MaxAge = 1800)]`                       |
| Public response that must be revalidated after becoming stale | `[CacheControl(Public = true, MaxAge = 300, MustRevalidate = true)]` |
| Shared caches need a different TTL than browsers              | `[CacheControl(Public = true, MaxAge = 300, SharedMaxAge = 600)]`    |
| Revalidate before reuse                                       | `[CacheControl(NoCache = true)]`                                     |
| Never store the response                                      | `[CacheControl(NoStore = true)]`                                     |

### Invalid Combinations

The filter validates the attribute before generating the header. The following combinations are not allowed and will throw an `InvalidOperationException` if used:

* `Public` and `Private` together.
* `NoStore` combined with any other directive.
* `NoCache` combined with `MaxAge`, `SharedMaxAge`, or `MustRevalidate`.
* Negative values for `MaxAge` or `SharedMaxAge`, except the sentinel value `-1`, which means "not set."

These rules keep the policy unambiguous and prevent silent misinterpretations.

### Important: Handling Sensitive Responses

**Do not** use `private` as the general solution for sensitive endpoints. The `private` directive only prevents shared caches from storing the response; a browser may still store it.

For truly sensitive data, use `no-store` or, where appropriate, `no-cache` to require revalidation before reuse.

Reserve `private` for responses that are user-specific but not highly sensitive. If unsure, consider `no-store`.

---

## 5. Integration Steps

1. Choose an existing Catalog GET endpoint that returns public, stable data.
2. Add `using E_Commerce.Api.Attributes;` if it is not already present.
3. Decorate the action with `[CacheControl(...)]` and set the appropriate properties.
4. Run the application and test the response headers using a GET request, as described in the Testing section.
5. Verify that the header appears only on the intended endpoint and not on others.
6. Ensure that non-GET methods do not receive the header.

---

## 6. Example: Product Details Endpoint

```csharp
[ApiController]
[Route("api/catalog/products")]
public sealed class ProductsController : BaseApiController
{
    [HttpGet("{id:guid}")]
    [CacheControl(Public = true, MaxAge = 600)]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken ct)
    {
        var query = new GetProductByIdQuery(id);
        var result = await Mediator.Send(query, ct);

        if (!result.Succeeded)
            return NotFound(result.Errors);

        return Ok(result.Data);
    }
}
```

---

## 7. Example: Categories List Endpoint

```csharp
[HttpGet]
[CacheControl(Public = true, MaxAge = 1800)]
public async Task<IActionResult> ListCategories(CancellationToken ct)
{
    // Handler logic.
}
```

Response header:

```http
Cache-Control: public, max-age=1800
```

---

## 8. Combining with Redis Cache

HTTP caching operates independently from the Redis application cache. Both can be used together:

* **Redis** caches the query result inside the API, reducing database load.
* **Cache-Control** instructs clients and shared caches when they can reuse an HTTP response without calling the API.

These strategies complement each other.

### Request Flow

```text
Client
  |
  |-- 1. Browser cache checks Cache-Control.
  |      It may reuse a fresh response.
  |
  |-- 2. If the response is stale or absent,
  |      the request goes to the API.
  |
  |-- 3. The API uses Redis through ICache
  |      to retrieve the query result.
  |
  |-- 4. On a Redis cache miss,
  |      the query accesses SQL Server.
```

A cacheable query may use application caching like this:

```csharp
[AuthorizePermission(CatalogPermissions.ViewProducts)]
public sealed record GetProductByIdQuery(Guid ProductId)
    : IRequest<Result<ProductDto>>, ICacheableQuery
{
    public string CacheKey => $"catalog:product:{ProductId}";

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}
```

Its corresponding controller action can also be decorated with `[CacheControl]`.

**Important:** HTTP caching and Redis caching have independent expiration and invalidation behavior. Changing or invalidating a Redis entry does not automatically invalidate a response already stored by a browser or shared cache.

---

## 9. Important Rules

* **Only GET endpoints** should use this attribute.
* **Do not mark sensitive data as `public`.** Use `no-store` or `no-cache` as appropriate. Avoid `private` unless you understand its browser-caching implications.
* **Choose TTLs based on data volatility.** Catalog categories may tolerate longer caching periods than product prices.
* **Do not set `public`** on endpoints that require authentication and return personalized data.
* **No automatic HTTP cache invalidation occurs** when the underlying data changes. Cached responses can remain available until they expire or are revalidated. Keep TTLs moderate and require revalidation when necessary.
* **Avoid caching volatile data**, such as inventory counts, payment status, or shipment state.
* **Invalid policy combinations throw an exception** during request processing. Ensure attribute usage follows the rules in Section 4.

---

## 10. Testing

Use a **GET** request to inspect the response headers.

### Option 1: Display the Full Response

```bash
curl -i -X GET http://localhost:5000/api/catalog/products/123
```

### Option 2: Display Only the Response Headers

```bash
curl -s -D - -o /dev/null http://localhost:5000/api/catalog/products/123
```

### Verification Checklist

* Verify that the expected `Cache-Control` header is present on decorated endpoints.
* Confirm that the header does not appear on undecorated endpoints.
* Confirm that `POST`, `PUT`, and `DELETE` methods do not receive the header, even if the controller has `[CacheControl]`.
* Ensure that sensitive endpoints are not mistakenly marked `public`.
* Test invalid combinations in development to confirm that they throw `InvalidOperationException`, helping prevent ambiguous policies from reaching deployment.

**Avoid `curl -I`.** It sends a HEAD request, and this filter intentionally processes only GET requests.

---

## 11. Summary

Integrating HTTP caching is straightforward: apply the `[CacheControl]` attribute to stable, public GET actions or controllers, choose appropriate TTLs and directives, and test the resulting headers using actual GET requests.

This capability works alongside Redis application caching to reduce both client and server load. Keep policies conservative and avoid caching sensitive or rapidly changing data. Built-in validation ensures that only clear, unambiguous caching policies are applied.
