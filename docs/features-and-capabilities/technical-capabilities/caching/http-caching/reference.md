# HTTP Caching – Technical Reference Document

**E-Commerce Modular Monolith**

**Cross-Cutting API Capability: Cache-Control for Safe Catalog GET Endpoints**

**Version: 1.0 – Final**

---

## 1. Introduction

The HTTP Caching capability introduces declarative `Cache-Control` headers for safe, public, relatively stable Catalog GET endpoints. It is implemented entirely within the API layer as a custom attribute and an associated action filter.

The purpose is to reduce redundant client-server round trips and improve perceived performance by instructing browsers and shared HTTP caches how long a response may be reused.

This document describes the technical design, components, folder structure, and integration points for this capability. It complements the HTTP Caching Technical Requirements Document.

---

## 2. Architecture Overview

The capability follows a simple, attribute-based model:

* Developers annotate controller actions or entire controllers with `[CacheControl]`.
* The attribute carries cache policy information, such as `public` and `max-age`.
* An action filter inspects the attribute, validates the policy, and appends the appropriate `Cache-Control` header to the HTTP response.
* No other parts of the system—Domain, Application, or Infrastructure—are involved.

This keeps the concern isolated to the API presentation layer, preserving Clean Architecture boundaries.

### Processing Flow

```text
Controller Action
       |
       | [CacheControl(Public = true, MaxAge = 600)]
       v
CacheControlFilter (on result execution)
       |
       v
Validate policy
       |
       v
Build header
       |
       v
Set Cache-Control on HTTP response
```

---

## 3. Folder Structure

```text
API/
├── Attributes/
│   └── CacheControlAttribute.cs
│
├── Filters/
│   └── CacheControlFilter.cs
│
└── Extensions/
    └── HttpCachingExtensions.cs
```

No changes are required in the `Application`, `Domain`, or `Infrastructure` layers, nor in configuration files.

---

## 4. Component Details

### 4.1 `CacheControlAttribute`

**Purpose:**

Marks a controller or action method with an HTTP caching policy. It is a metadata attribute that does not contain behavior itself; the associated filter reads and validates its values.

**Location:** `API/Attributes/CacheControlAttribute.cs`

#### Properties

| Property         | Type   | Description                                                                                    |
| ---------------- | ------ | ---------------------------------------------------------------------------------------------- |
| `Public`         | `bool` | If `true`, the response may be stored by shared caches, such as a CDN or proxy.                |
| `Private`        | `bool` | If `true`, the response is intended for a single user and must not be stored by shared caches. |
| `MaxAge`         | `int`  | Duration in seconds that the response is considered fresh in any cache. `-1` means "not set."  |
| `SharedMaxAge`   | `int`  | Overrides `MaxAge` for shared caches only. `-1` means "not set."                               |
| `MustRevalidate` | `bool` | Once stale, caches must revalidate with the origin before reuse.                               |
| `NoCache`        | `bool` | Caches must revalidate with the origin before using a cached copy.                             |
| `NoStore`        | `bool` | The response must not be stored in any cache.                                                  |

**Sentinel value:** Attribute parameters cannot be nullable (`int?`). Therefore, `-1` is used to indicate that `MaxAge` or `SharedMaxAge` is not set. The filter emits the corresponding directive only if the value is non-negative.

#### Validation Rules

The attribute has a `Validate()` method that is called by the filter before header generation. It throws `InvalidOperationException` for invalid combinations:

* `Public` and `Private` are both enabled.
* `NoStore` is combined with any other directive.
* `NoCache` is combined with `MaxAge` or `SharedMaxAge`.
* `NoCache` is combined with `MustRevalidate`.
* `MaxAge` or `SharedMaxAge` is less than `-1`.

#### Usage Notes

* At least one property should be set to produce a meaningful header.
* The attribute can be applied to a controller class, where it applies to its actions, or to an individual action method for a more specific policy.

### 4.2 `CacheControlFilter`

**Purpose:**

Executes after an action has produced a result, reads any applied `CacheControlAttribute`, validates it, and sets the corresponding `Cache-Control` response header.

**Location:** `API/Filters/CacheControlFilter.cs`

#### Behavior

* Runs only for `GET` requests. If the request method is not `GET`, it does nothing.
* Looks for the attribute in this order:

  1. The action method.
  2. The controller class, if no action-level attribute is found.
* If no attribute is found, no header is added by this filter.
* Calls `Validate()` to ensure the policy is valid.
* Constructs a `Cache-Control` header based on the attribute's properties and applies it to the response.

#### Header Composition Logic

* If `NoStore` is `true`, the header is exactly `no-store`; no other directives are added.
* Otherwise, if `NoCache` is `true`, the header is exactly `no-cache`; no `max-age`, `s-maxage`, or `must-revalidate` directives are added.
* Otherwise:

  * Add `public` if `Public` is `true`.
  * Add `private` if `Private` is `true`. It is mutually exclusive with `public`.
  * If `MaxAge >= 0`, add `max-age={MaxAge}`.
  * If `SharedMaxAge >= 0`, add `s-maxage={SharedMaxAge}`.
  * If `MustRevalidate` is `true`, add `must-revalidate`.

**Example:**

```csharp
[CacheControl(Public = true, MaxAge = 600)]
```

Produces:

```http
Cache-Control: public, max-age=600
```

---

## 5. Integration Example

A stable product endpoint:

```csharp
[ApiController]
[Route("api/catalog/products")]
public sealed class ProductsController : BaseApiController
{
    [HttpGet("{id:guid}")]
    [CacheControl(Public = true, MaxAge = 600)]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken ct)
    {
        // Handler logic.
    }
}
```

The filter automatically adds the header after the action executes.

### Registration in `Program.cs`

```csharp
builder.Services.AddControllers();
builder.Services.AddHttpCaching();
```

`AddHttpCaching()` uses `services.Configure<MvcOptions>` and does **not** re-register MVC controllers.

---

## 6. Workflow

1. The client sends `GET /api/catalog/products/{id}`.
2. ASP.NET Core invokes the action after authorization and model binding.
3. The action returns an `IActionResult`.
4. The `CacheControlFilter` executes during result processing:

   * Confirms that the request method is `GET`.
   * Finds the `[CacheControl]` attribute on the action or controller.
   * Calls `Validate()` to ensure the policy is valid.
   * Builds and applies the `Cache-Control` header.
5. The response is sent to the client with caching instructions.

---

## 7. Supported Directives

| Directive         | Attribute Property | Purpose                                                                                                 |
| ----------------- | ------------------ | ------------------------------------------------------------------------------------------------------- |
| `public`          | `Public`           | Allows shared caches to store the response.                                                             |
| `private`         | `Private`          | Allows private caches, such as a browser cache, to store the response; shared caches must not store it. |
| `max-age`         | `MaxAge`           | Freshness lifetime for all caches.                                                                      |
| `s-maxage`        | `SharedMaxAge`     | Freshness lifetime for shared caches only.                                                              |
| `must-revalidate` | `MustRevalidate`   | Requires stale responses to be revalidated before reuse.                                                |
| `no-cache`        | `NoCache`          | Requires revalidation before a cached response can be reused.                                           |
| `no-store`        | `NoStore`          | Instructs caches not to store the response.                                                             |

---

## 8. Non-Functional Considerations

* **Performance:** Overhead is negligible; header construction involves simple string operations.
* **Maintainability:** Centralizes HTTP caching policy in one attribute/filter pair rather than scattering logic across controllers.
* **Correctness:** Validation enforces clear, unambiguous policies.
* **Security:** Developers must ensure `public` is not used on sensitive endpoints. Use `no-store` or `no-cache` where appropriate.

---

## 9. Testing

### Unit Tests — Filter Logic

* `CacheControlFilter` with a GET request and no attribute adds no header.
* An attribute on an action produces the correct header.
* An attribute on a controller produces the correct header.
* A non-GET request adds no header, even if an attribute exists.
* Combinations of directives produce the expected header string.
* Invalid combinations, such as `Public` + `Private` or `NoCache` + `MaxAge`, throw `InvalidOperationException`.

### Integration Tests — API

* An endpoint with `[CacheControl(Public = true, MaxAge = 600)]` returns `Cache-Control: public, max-age=600`.
* An endpoint without the attribute receives no `Cache-Control` header from this filter.
* Sensitive endpoints remain without the `public` directive.
* `POST`, `PUT`, and `DELETE` methods do not receive the header, even if the controller has `[CacheControl]`.

---

## 10. Acceptance Criteria

1. `CacheControlAttribute` and `CacheControlFilter` exist and are functional.
2. Header generation supports all listed directives.
3. The attribute works on both controllers and action methods.
4. Non-GET requests are unaffected.
5. By default, the filter leaves responses without cache headers unless the attribute is applied.
6. At least one Catalog GET endpoint uses the attribute and returns the correct header.
7. The implementation is confined to the API layer.
8. Invalid policies are rejected with a clear exception.

---

## 11. Summary

The HTTP Caching capability provides a lightweight, declarative way to add `Cache-Control` headers to stable, public GET endpoints. It uses a custom attribute and action filter entirely within the API layer. The design is simple, maintainable, and aligned with the modular monolith architecture, complementing the existing Redis application-level caching without introducing cross-layer dependencies.

Validation ensures that only clear, unambiguous caching policies are applied. This capability can be used independently or alongside Redis, depending on the system's performance needs.

---

**Document version:** 1.0 – Final

**Feature:** HTTP Caching
