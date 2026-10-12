# Caching

**E-Commerce Modular Monolith — Technical Capability**

## 1. Overview

The Caching capability provides reusable mechanisms for improving application performance by reducing redundant data retrieval, repeated computation, unnecessary HTTP requests, and repeated response generation.

It covers two distinct caching concerns:

* **Application Caching** — Provides an application-level cache for reusable query results and application data. Redis is the configured cache provider, reducing repeated database access and supporting cache reuse across application instances.
* **HTTP Caching** — Applies declarative HTTP `Cache-Control` response headers to eligible API endpoints, allowing browsers and shared HTTP caches to reuse responses according to defined caching policies.

These concerns are documented separately because they operate at different architectural boundaries and have different implementation mechanisms, configuration requirements, security considerations, and invalidation behavior.

The capability is designed to complement the existing architecture rather than introduce unnecessary infrastructure or make caching a mandatory part of every application operation.

## 2. Classification and Responsibility

Caching is a **technical capability**, not a business feature or Domain bounded context.

It provides technical mechanisms that business features and application components can use without transferring ownership of business rules, business entities, or business lifecycle decisions to the caching subsystem.

The capability is responsible for providing caching abstractions, implementation mechanisms, and policy enforcement within its defined scope. It does not determine the business meaning of cached data or independently decide whether a particular business operation is safe to cache.

Responsibilities remain separated:

* **Business features** determine which data is suitable for caching based on business requirements and data volatility.
* **Application components** integrate application caching into eligible operations through the defined abstractions.
* **The API layer** applies HTTP caching policies to eligible HTTP responses.
* **The persistence layer** remains authoritative for persisted business state.

Caching may improve performance, but it must not become a substitute for persistence, authorization, or business correctness.

## 3. Architectural Principles

The Caching capability follows these principles.

### 3.1 Separation of Concerns

Application caching and HTTP caching have distinct responsibilities and implementation boundaries.

Application caching concerns reusable application data and query results. HTTP caching concerns the reuse of HTTP responses according to HTTP caching semantics.

The two mechanisms must remain independently configurable and understandable.

### 3.2 Explicit and Selective Usage

Caching is opt-in rather than mandatory.

Application components should cache only data for which caching provides a meaningful performance benefit. HTTP caching policies should be applied explicitly to eligible endpoints rather than globally to all API responses.

### 3.3 Correctness Before Performance

Caching must not compromise business invariants, authorization, data isolation, or operation correctness.

Cache keys, expiration policies, invalidation behavior, and response directives must reflect the characteristics of the data being cached.

### 3.4 Controlled Freshness and Invalidation

Cached data can become stale when its authoritative source changes.

Application caching must account for expiration and invalidation after relevant data changes. HTTP caching must account for response freshness, expiration, and revalidation requirements.

Invalidating an application-cache entry does not automatically invalidate a response already stored by a browser or shared HTTP cache. These mechanisms have independent lifecycles and must be managed accordingly.

### 3.5 Provider and Framework Isolation

Application components should depend on appropriate caching abstractions rather than unnecessary direct dependencies on Redis or its client library.

HTTP caching policies are implemented at the API boundary through the dedicated attribute and action-filter mechanism.

Provider-specific implementation details must remain within the appropriate technical layer.

### 3.6 Configuration-Driven Behavior

Cache providers and policies should use the application's configuration mechanisms.

Redis connection settings and other environment-specific values must not be hardcoded. Deployment-specific configuration and secrets must be supplied through appropriate configuration providers or environment variables.

### 3.7 Authoritative Data Ownership

Caching does not become the source of truth for business state.

SQL Server remains the authoritative persistence store for the persisted business data covered by these caching mechanisms. Redis and client-side HTTP caches provide performance optimizations rather than replacing authoritative storage.

### 3.8 Security and Data Isolation

Public caching must be restricted to responses that are genuinely safe for public reuse.

Sensitive or user-specific data must not be exposed through shared caches. Authentication, authorization, and user-specific response handling must remain correct regardless of whether application or HTTP caching is enabled.

### 3.9 Proportionate Infrastructure

The capability should use the simplest implementation that satisfies the application's requirements.

Redis provides the application-cache implementation where configured. HTTP caching uses standard response directives and does not require a separate server-side HTTP cache store, CDN, or reverse-proxy caching infrastructure as part of its current scope.

Additional infrastructure should be introduced only when justified by concrete requirements.

## 4. Documentation Structure

```text
caching/
├── README.md
├── application-caching/
│   ├── integration-guide.md
│   ├── reference.md
│   └── requirements.md
└── http-caching/
    ├── integration-guide.md
    ├── reference.md
    └── requirements.md
```

### 4.1 Application Caching

**Location:** `application-caching/`

Application Caching documents the application-level cache-aside mechanism used to reduce repeated data retrieval and computation.

It covers the caching abstractions, query integration, Redis implementation, configuration, expiration, invalidation, failure handling, and relevant architectural constraints.

The implementation uses an application-level caching abstraction and an opt-in caching behavior for eligible queries. Redis provides the configured cache provider, while SQL Server remains authoritative for persisted business data.

| Document               | Purpose                                                                                                                           |
| ---------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| `integration-guide.md` | Explains how application components integrate with the caching capability, including cacheable queries and invalidation.          |
| `reference.md`         | Defines the architecture, abstractions, implementation, configuration, failure behavior, and technical constraints.               |
| `requirements.md`      | Records the functional and non-functional requirements, scope, design decisions, and acceptance criteria for application caching. |

### 4.2 HTTP Caching

**Location:** `http-caching/`

HTTP Caching documents the API-level mechanism for applying `Cache-Control` response headers to eligible HTTP endpoints.

The current implementation uses a custom `[CacheControl]` attribute and an associated action filter. Developers can define policies at the controller or action level without introducing HTTP caching concerns into the Domain or Application layers.

The initial integration targets safe, public, relatively stable Catalog GET endpoints, including eligible product, category, and brand read operations.

Supported directives include:

* `public`
* `private`
* `max-age`
* `s-maxage`
* `must-revalidate`
* `no-cache`
* `no-store`

Policies must account for response freshness, public versus private cacheability, and the sensitivity and volatility of the returned data.

| Document               | Purpose                                                                                                                                               |
| ---------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| `integration-guide.md` | Explains how to apply `[CacheControl]` to eligible API endpoints, select directives, and verify response headers.                                     |
| `reference.md`         | Defines the attribute, action filter, supported directives, validation rules, registration, and implementation behavior.                              |
| `requirements.md`      | Records the functional and non-functional requirements, architectural constraints, scope, design decisions, and acceptance criteria for HTTP caching. |

## 5. Relationship Between Application and HTTP Caching

Application caching and HTTP caching both improve performance, but they operate at different boundaries and are not interchangeable.

| Concern                | Application Caching                                                              | HTTP Caching                                                                   |
| ---------------------- | -------------------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| Primary boundary       | Application data retrieval and computation                                       | HTTP request and response processing                                           |
| Typical cached content | Query results and reusable application data                                      | HTTP responses or response representations                                     |
| Main mechanism         | Application caching abstraction and cache-aside behavior                         | `Cache-Control` response headers                                               |
| Current implementation | Redis-backed application cache                                                   | API-level attribute and action filter                                          |
| Primary consumers      | Application components                                                           | Browsers and eligible shared HTTP caches                                       |
| Freshness management   | Cache expiration and explicit invalidation                                       | HTTP freshness directives and revalidation policies                            |
| Failure considerations | Cache failures must be handled according to application reliability requirements | Cache reuse depends on HTTP policy, freshness, and cache behavior              |
| Authoritative data     | SQL Server remains authoritative for persisted business data                     | The origin application and its authoritative data sources remain authoritative |

### 5.1 Application Caching Request Path

For an eligible cacheable query, the application checks the cache before retrieving the result from the underlying data source.

```text
Incoming API Request
        |
        v
Application Query
        |
        v
Application Caching Behavior
        |
        v
    Redis Cache
      /     \
    Hit     Miss
     |        |
     v        v
 Return    Query SQL Server
 Result        |
               v
          Populate Cache
               |
               v
          Return Result
```

This reduces repeated database access for cacheable queries when a valid entry is available.

### 5.2 HTTP Caching Request Path

For an eligible HTTP response, a browser or shared cache may reuse a fresh cached response without contacting the API.

```text
Client Request
      |
      v
Client or Shared HTTP Cache
      |
      +---- Fresh cached response ----> Reuse Response
      |
      +---- No usable response -------> Request API
                                           |
                                           v
                                      API Endpoint
                                           |
                                           v
                                  Application Query
                                           |
                                           v
                                  Application Cache
                                           |
                                           v
                                      SQL Server
                                  (if cache miss)
```

The HTTP caching mechanism defines response-cacheability and freshness directives. It does not itself cache application query results or provide a server-side response store.

### 5.3 Combining Both Mechanisms

The two mechanisms can be used together when justified.

For example, a public product-details endpoint may use Redis-backed application caching to reduce database queries while also returning a suitable `Cache-Control` header so clients can reuse the HTTP response.

Their policies must be coordinated:

* Redis invalidation does not automatically purge browser or shared HTTP caches.
* HTTP response freshness may outlast an underlying data change unless expiration or revalidation policies account for it.
* A short HTTP `max-age` does not guarantee that the origin returns the latest database state if its application-cache entry is stale.
* Public HTTP caching must not expose user-specific information or bypass required authorization and data-isolation guarantees.

Combining these mechanisms should be an intentional performance decision rather than a default applied to every endpoint.

## 6. Integration Boundaries

The Caching capability supports business features and application components without taking ownership of their business behavior.

Integration must preserve the following boundaries:

* **Business ownership:** Business features remain responsible for business rules, state transitions, and authoritative data.
* **Application caching:** Application components use the defined caching abstractions where caching is appropriate.
* **HTTP caching:** The API layer applies HTTP response policies to explicitly selected endpoints.
* **Persistence:** Cached data must remain recoverable from its authoritative source when required.
* **Invalidation:** Application-cache invalidation and HTTP response freshness must be considered separately whenever underlying data changes.
* **Authorization:** Cached data and responses must respect authentication, authorization, and user-specific data isolation.
* **Failure handling:** Cache failures must be handled according to the reliability requirements of the affected operation. A cache must not silently become the authoritative source of business data.
* **Selective adoption:** Not every query or endpoint requires caching. Introduce caching based on demonstrated or anticipated performance needs and a clear understanding of its correctness implications.

## 7. Maintenance

Changes to the Caching capability must preserve the distinction between application-level and HTTP-level caching.

When modifying the implementation or its documentation:

1. Update the relevant area's `reference.md` when its architecture, abstractions, configuration, or implementation details change.
2. Update `requirements.md` when functional or non-functional expectations, scope, or acceptance criteria change.
3. Update `integration-guide.md` when application components or API endpoints must integrate differently.
4. Update this README when the capability's scope, documentation structure, responsibilities, or architectural boundaries change.

Documentation should describe the implemented system and its intended behavior accurately. It must not imply support for unimplemented mechanisms, infrastructure, or guarantees.

---

**Capability:** Caching
**Classification:** Technical Capability
**Scope:** Application Caching and HTTP Caching
**Project:** E-Commerce Modular Monolith
