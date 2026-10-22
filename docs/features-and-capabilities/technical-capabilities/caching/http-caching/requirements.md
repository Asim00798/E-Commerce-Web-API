# HTTP Caching – Technical Requirements Document

**E-Commerce Modular Monolith**

**Cross-Cutting API Capability: Cache-Control for Safe Catalog GET Endpoints**

**Version: 1.0 – Final**

---

## 1. Introduction

HTTP caching is a performance optimization at the API boundary. It uses standard `Cache-Control` response headers to instruct browsers, shared caches, and intermediate proxies how long a response may be reused without revalidating with the origin server.

This capability is deliberately narrow. It targets **safe, public, relatively stable Catalog GET responses** where short-lived caching is appropriate. It complements the existing Redis application-level cache but operates at a different layer: HTTP response semantics rather than data retrieval.

---

## 2. Business Goals

* Reduce redundant network traffic for stable catalog data.
* Improve client-perceived performance by allowing browser cache reuse.
* Decrease API and database load for frequently accessed public data.
* Provide a simple, declarative way to apply HTTP caching policies to endpoints.
* Keep the implementation aligned with Clean Architecture by placing this concern entirely in the API layer.

---

## 3. Scope

### 3.1 In Scope

* `Cache-Control` response headers.
* Support for the following directives:

  * `public`
  * `private`
  * `max-age`
  * `s-maxage`
  * `no-cache`
  * `no-store`
  * `must-revalidate`
* Applying caching only to **safe GET endpoints** that return **public, relatively stable** data.
* Initial integration limited to **Catalog** read endpoints:

  * Product details.
  * Category details and lists.
  * Brand details and lists.
* A declarative attribute-based mechanism for applying caching policies at the controller or action level.

### 3.2 Out of Scope

* ETags, `If-None-Match`, and `304 Not Modified`.
* CDN configuration and edge caching.
* Reverse-proxy or load-balancer caching configuration.
* Advanced HTTP cache invalidation and consistency mechanisms.
* Caching user-specific, sensitive, or frequently changing data.
* Server-side HTTP cache storage.

---

## 4. Functional Requirements

### 4.1 Cache-Control Header Generation

* The system must support generating `Cache-Control` headers with the following directives:

  * `public`
  * `private`
  * `max-age` in seconds.
  * `s-maxage` in seconds.
  * `must-revalidate`
  * `no-cache`
  * `no-store`
* The generated `Cache-Control` header must conform to applicable HTTP caching semantics.

### 4.2 Attribute-Based Policy

* Provide a custom attribute, such as `[CacheControl]`, that can be applied to controller classes or action methods.
* The attribute must allow configuration of:

  * `Public` or `Private`.
  * `MaxAge` in seconds.
  * `SharedMaxAge` in seconds, optionally.
  * `MustRevalidate`.
  * `NoCache`.
  * `NoStore`.
* When the attribute is present, the response must include the corresponding `Cache-Control` header.

### 4.3 Default Behavior

* Endpoints without the attribute must not receive automatically generated cache headers from this capability.
* The application must not globally apply `Cache-Control` headers to all endpoints.

### 4.4 Safe Methods Only

* This capability applies only to HTTP `GET` requests.
* `POST`, `PUT`, `DELETE`, and other methods must not be affected by these cache policies.

### 4.5 Integration with Existing Caching

* This HTTP caching capability is independent of the Redis application cache.
* Redis caches query results at the application layer.
* `Cache-Control` instructs browsers and shared HTTP caches how responses may be cached and reused.

---

## 5. Non-Functional Requirements

### 5.1 Performance

* Adding `Cache-Control` headers must not introduce noticeable processing overhead.
* Cache policies should reduce repeated requests for stable data.

### 5.2 Maintainability

* Policies must be declarative and easy to apply to new endpoints.
* The implementation must be centralized in one attribute/filter pair to avoid scattered header-generation logic.

### 5.3 Correctness

* Headers must accurately reflect the intended caching semantics.
* Data that is sensitive to staleness must not be marked as publicly cacheable.

### 5.4 Security

* Avoid caching sensitive or user-specific data with `public`.
* Use `private` or `no-store` for endpoints involving authentication or personal data, as appropriate to the sensitivity and caching requirements.

---

## 6. Architectural Constraints

* Implementation must reside in the **API layer only**.
* Domain and Application layers must not be aware of HTTP caching.
* The capability must use ASP.NET Core action filters without external dependencies.
* The implementation must not conflict with existing authorization filters.

---

## 7. Key Design Decisions

| Concern                               | Decision                                                                                     |
| ------------------------------------- | -------------------------------------------------------------------------------------------- |
| Layer                                 | API layer                                                                                    |
| Mechanism                             | Custom `[CacheControl]` attribute and action filter                                          |
| Scope                                 | Safe GET endpoints only                                                                      |
| Initial targets                       | Catalog products, categories, and brands                                                     |
| Directives                            | `public`, `private`, `max-age`, `s-maxage`, `must-revalidate`, `no-cache`, `no-store`        |
| Default behavior                      | No cache headers from this capability unless the attribute is applied                        |
| ETags                                 | Out of scope                                                                                 |
| CDN and proxy configuration           | Out of scope                                                                                 |
| Integration with Redis                | Independent; both mechanisms can be combined                                                 |
| `private`, `no-cache`, and `no-store` | Supported by the header-generation mechanism but not part of the initial Catalog integration |

---

## 8. Acceptance Criteria

1. A custom `CacheControlAttribute` exists and can be applied to controllers and action methods.
2. The attribute correctly generates `Cache-Control` headers based on its properties.
3. The following directives are supported and produce valid headers:

   * `public`
   * `private`
   * `max-age`
   * `s-maxage`
   * `must-revalidate`
   * `no-cache`
   * `no-store`
4. At least one stable Catalog GET endpoint is decorated with `[CacheControl]` and returns the expected header.
5. Non-GET requests are unaffected.
6. Endpoints without the attribute do not receive cache headers from this capability.
7. No sensitive or user-specific data is marked `public`.
8. The implementation is contained within the API layer.

---

## 9. Out of Scope

* ETags and conditional requests.
* CDN or edge-cache configuration.
* Reverse-proxy caching.
* Advanced HTTP cache invalidation.
* HTTP caching for user-specific or sensitive data.

---

## 10. Summary

This HTTP caching capability introduces declarative `Cache-Control` headers for safe, public, relatively stable Catalog GET responses. It uses a simple attribute-based approach in the API layer, supporting core directives without building ETag, CDN, or proxy infrastructure.

The implementation complements existing Redis caching by addressing client-side and shared-cache reuse while preserving architectural boundaries.

---

**Document version:** 1.0 – Final

**Feature:** HTTP Caching
