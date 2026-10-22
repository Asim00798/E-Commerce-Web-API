# E-Commerce.API — Architecture Document

**Project:** E-Commerce Modular Monolith

**Layer:** API

**Version:** 1.0 – Final

---

## 1. Overview

The `E-Commerce.Api` project is the **HTTP entry point and composition root** of the E-Commerce modular monolith.

It exposes application capabilities through HTTP endpoints and translates HTTP requests into Application commands and queries. It then translates application results into appropriate HTTP responses.

The API layer is responsible for answering:

**How does an external client communicate with the application's use cases through HTTP?**

It owns the HTTP boundary, request/response contracts, endpoint routing, middleware pipeline, API versioning, HTTP security policies, and application startup composition.

It does not own business rules, application orchestration, persistence implementation, or external-provider integrations.

The API layer therefore acts as the **transport boundary between external clients and the Application layer**.

### Primary Responsibilities

* Expose RESTful HTTP endpoints.
* Translate requests into Application commands and queries.
* Dispatch operations through MediatR.
* Translate application results into HTTP responses.
* Apply HTTP-level validation and security controls.
* Configure middleware and endpoint routing.
* Define API-specific request and response DTOs.
* Configure API versioning, CORS, rate limiting, and HTTP caching.
* Register and compose application and infrastructure services.
* Expose health-check endpoints.
* Configure API-level logging and exception handling.

---

## 2. Architectural Philosophy

The API follows Clean Architecture and maintains a deliberately thin HTTP boundary.

The central rule is:

**API translates HTTP; Application orchestrates; Domain enforces business rules; Infrastructure implements technical capabilities.**

The API must not become a second Application layer.

A controller should not independently coordinate repositories, perform business workflows, manipulate transactions, or communicate directly with infrastructure providers.

Instead, it delegates the requested operation to Application.

### Architectural Flow

```text
External Client
     |
     | HTTP Request
     v
E-Commerce.Api
     |
     | Command / Query
     v
E-Commerce.Application
     |
     v
E-Commerce.Domain
     |
     v
Infrastructure Implementations
     |
     v
SQL Server / External Providers
```

Specialized reads follow a separate implementation path:

```text
External Client
     |
     v
API Controller
     |
     v
Application Query
     |
     v
ReadModel Query Service
     |
     v
ADO.NET
     |
     v
SQL Server
```

The API does not need to know whether a query uses EF Core repositories or the specialized ADO.NET ReadModel.

---

## 3. Project Dependencies

The API is the outermost application entry point.

Its project references include:

* `E-Commerce.Application`
* `E-Commerce.Infrastructure`
* `E-Commerce.ReadModel`

The API references Infrastructure because it acts as the composition root and must register concrete implementations of Application abstractions.

It references ReadModel because it must register specialized read implementations and, where applicable, invoke ReadModel migration startup functionality.

### Dependency Diagram

```text
                  E-Commerce.Api
                        |
            ┌───────────┼───────────┐
            |           |           |
            v           v           v
      Application  Infrastructure  ReadModel
            |           |             |
            v           |             |
          Domain        └──────┬──────┘
                               |
                               v
                          Application
```

### Essential Dependency Rules

1. API may depend on Application.
2. API may depend on Infrastructure for composition.
3. API may depend on ReadModel for composition and specialized query support.
4. Infrastructure must not depend on API.
5. Application must not depend on API.
6. Domain must not depend on API.
7. ReadModel depends on Application, not on API or Infrastructure.

These rules preserve inward dependency direction while allowing the API to assemble the running application.

---

## 4. Primary Responsibilities

### 4.1 HTTP Endpoints

The API exposes application capabilities through HTTP endpoints.

Examples include:

* Registration and verification endpoints.
* Authentication and token endpoints.
* Product and catalog endpoints.
* Order and cart endpoints.
* Payment and refund endpoints.
* Shipping endpoints.
* Customer profile endpoints.
* Notification endpoints.
* File upload and download endpoints.

Controllers define the HTTP contract and delegate actual use cases to Application.

### 4.2 Request Translation

The API receives HTTP-specific request DTOs and translates them into Application commands or queries.

For example:

```text
HTTP POST /api/v1/orders
         |
         v
PlaceOrderRequest
         |
         v
PlaceOrderCommand
         |
         v
MediatR
         |
         v
PlaceOrderCommandHandler
```

The API should not pass HTTP request objects into Domain aggregates.

### 4.3 Response Translation

Application results are translated into HTTP responses.

Typical responses include:

* `200 OK`
* `201 Created`
* `202 Accepted`
* `204 No Content`
* `400 Bad Request`
* `401 Unauthorized`
* `403 Forbidden`
* `404 Not Found`
* `409 Conflict`
* `429 Too Many Requests`
* `500 Internal Server Error`
* `503 Service Unavailable`

The appropriate response depends on the endpoint contract and the outcome of the operation.

---

## 5. Thin Controllers

Controllers should remain focused on HTTP responsibilities.

A controller generally performs these steps:

1. Receive the request.
2. Bind route, query-string, and body parameters.
3. Apply endpoint-level authorization metadata where required.
4. Dispatch an Application command or query.
5. Map the result to an HTTP response.

### Example

```csharp
[ApiController]
[Route("api/v1/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new PlaceOrderCommand(request.CartId);

        var result = await _sender.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetOrder),
            new { id = result.OrderId },
            result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetOrderQuery(id),
            cancellationToken);

        return result is null
            ? NotFound()
            : Ok(result);
    }
}
```

This is an illustrative example; the exact command and result types depend on the application's contracts.

### Controllers Should Not

* Implement business rules.
* Instantiate repositories.
* Access `DbContext` directly.
* Manage database transactions.
* Schedule Hangfire jobs directly.
* Publish integration events directly.
* Call Paymob directly.
* Call SMTP, Twilio, Firebase, Redis, or SignalR directly.
* Duplicate Application authorization logic.

The controller should communicate application intent, not implement the workflow.

---

## 6. MediatR Integration

The API uses MediatR to dispatch Application commands and queries.

Typical dependencies include:

* `ISender`
* `IMediator`

Where only request dispatch is required, `ISender` is generally the narrower dependency.

### Execution Flow

```text
Controller
   |
   v
ISender.Send(...)
   |
   v
MediatR Pipeline
   |
   v
Application Handler
```

MediatR is the dispatch mechanism; it does not define the business architecture by itself.

The Application layer owns commands, queries, handlers, and application behaviors.

The API should not contain business workflows merely because MediatR is available.

---

## 7. API DTOs

The API owns HTTP request and response DTOs.

These represent the external HTTP contract, rather than internal application behavior or domain state.

### Example Structure

```text
API
└── Contracts
    ├── Requests
    └── Responses
```

A request DTO might be:

```csharp
public sealed record PlaceOrderRequest(
    Guid CartId);
```

The API maps it to an Application command.

### DTO Separation

```text
API Request DTO
      |
      v
Application Command
      |
      v
Domain Aggregate
```

Likewise, response data may flow through:

```text
Application Result
      |
      v
API Response DTO
      |
      v
HTTP Response
```

This separation is intentional.

**API DTOs, Application DTOs, and Domain entities are different architectural concepts.**

They do not need to be identical, and the API should not expose domain aggregates directly as its public contract.

---

## 8. Request Validation

Validation is performed at the appropriate architectural boundary.

### API Validation

Responsible for HTTP and request-contract concerns, such as:

* Required request fields.
* Valid request formats.
* Route and query parameter binding.
* Basic transport-level constraints.

ASP.NET Core model binding and `[ApiController]` behavior can automatically produce a `400 Bad Request` response for invalid model state, depending on the endpoint configuration.

### Application Validation

Responsible for application-level input requirements and use-case preconditions.

### Domain Validation

Responsible for business invariants and valid aggregate state transitions.

### Validation Flow

```text
API
 |
 | Is the HTTP request structurally valid?
 v
Application
 |
 | Can this use case proceed?
 v
Domain
 |
 | Is the business operation valid?
 v
```

The API must not become the sole enforcement point for business invariants.

---

## 9. Middleware Pipeline

The middleware pipeline provides cross-cutting HTTP behavior.

The current pipeline is organized around the following responsibilities:

```text
Forwarded Headers
       |
       v
Correlation
       |
       v
Serilog Request Logging
       |
       v
Security Headers
       |
       v
Global Exception Handling
       |
       v
Development Swagger
       |
       v
HTTPS / HSTS
       |
       v
CORS
       |
       v
Production Rate Limiting
       |
       v
Authentication
       |
       v
Authorization
       |
       v
Controller / Endpoint Mapping
```

The actual registration order must remain deliberate because middleware order affects behavior.

For example:

* Forwarded headers must be processed before middleware that depends on the original scheme or client address.
* Exception handling should wrap downstream middleware whose failures it is intended to translate.
* Authentication must execute before authorization.
* CORS must be configured so browser preflight and response handling work correctly.
* Endpoint mapping must occur after the relevant middleware configuration.

The pipeline should be verified against the actual `Program.cs` implementation rather than treated as a universally valid sequence for every ASP.NET Core application.

---

## 10. Global Exception Handling

The API provides a centralized exception boundary.

Application and Infrastructure failures should not require every controller to implement its own exception-to-HTTP mapping.

The global exception handler translates known failures into appropriate HTTP responses.

### Typical Mappings

| Failure                               | HTTP Status                 |
| ------------------------------------- | --------------------------- |
| Request/model validation failure      | `400 Bad Request`           |
| Resource not found                    | `404 Not Found`             |
| Unauthenticated request               | `401 Unauthorized`          |
| Authenticated but forbidden operation | `403 Forbidden`             |
| Optimistic concurrency conflict       | `409 Conflict`              |
| Rate limit exceeded                   | `429 Too Many Requests`     |
| Temporary service unavailability      | `503 Service Unavailable`   |
| Unexpected unhandled failure          | `500 Internal Server Error` |

These mappings are conventions for the application's exception policy, not automatic mappings provided by .NET.

The exception handler must avoid exposing:

* Stack traces in production responses.
* SQL statements.
* Database connection details.
* Credentials or tokens.
* Internal provider responses containing sensitive information.
* Other implementation details that could compromise security.

Unexpected errors should be logged with a correlation identifier where available.

### Cancellation

Request cancellation must be handled carefully.

When the client disconnects or `HttpContext.RequestAborted` is signaled, cancellation may propagate through MediatR and downstream asynchronous operations.

The API must not automatically classify every `OperationCanceledException` as an internal server error.

The final behavior should distinguish expected request cancellation from unrelated timeout or cancellation failures.

---

## 11. Authentication

The API is responsible for configuring and applying HTTP authentication.

For bearer-token authentication, this generally includes:

* Registering authentication services.
* Configuring JWT bearer validation.
* Applying authentication middleware.
* Applying authorization policies.
* Returning appropriate authentication challenges.

The API does not generate tokens itself.

Token generation, refresh-token persistence, hashing, and ASP.NET Core Identity operations belong to Infrastructure behind Application-facing abstractions.

### Authentication Flow

```text
HTTP Login Request
      |
      v
API
      |
      v
Application Authentication Use Case
      |
      v
Infrastructure Identity / Token Services
      |
      v
Application Result
      |
      v
HTTP Response
```

The API may receive and return token-related data as part of its HTTP contract, but it should not own the underlying identity implementation.

---

## 12. Authorization

Authorization has both HTTP-boundary and application-use-case responsibilities.

### API Responsibility

The API may apply endpoint-level authorization metadata, including role-based requirements.

For example:

```csharp
[Authorize]
```

Or, where appropriate:

```csharp
[Authorize(Roles = "Administrator")]
```

These are boundary-level controls.

### Application Responsibility

Application use cases enforce their own required permissions and application-level authorization rules.

A controller-level authorization attribute must not be treated as a replacement for authorization requirements that belong to the use case.

### Infrastructure Responsibility

Infrastructure provides the ASP.NET Core authorization implementation, such as custom policy providers and authorization handlers.

The resulting separation is:

```text
API
 └── Endpoint access policy

Application
 └── Use-case permission requirements

Infrastructure
 └── ASP.NET Core authorization implementation
```

---

## 13. API Versioning

The API uses URL-segment versioning through Asp.Versioning.

An example endpoint is:

```text
/api/v1/products
```

The version identifies the HTTP API contract exposed to clients.

API versioning allows the external contract to evolve without forcing every client to adopt changes simultaneously.

Versioning policy should distinguish between:

* Backward-compatible changes.
* Breaking contract changes.
* Deprecated endpoints.
* Removal of older versions.

A default version of `1.0` may be configured, but whether clients must explicitly specify the version depends on the application's versioning configuration.

Versioning does not require separate Application or Domain projects for every API version.

---

## 14. CORS

CORS controls which browser origins may make cross-origin requests to the API.

The API owns CORS policy configuration.

A production CORS policy should explicitly define the required:

* Origins.
* HTTP methods.
* Request headers.
* Credential behavior, if required.

CORS is a browser-enforced access mechanism, not authentication or authorization.

It does not prevent non-browser clients from calling an endpoint.

Therefore, CORS must not be treated as a substitute for authentication, authorization, or rate limiting.

---

## 15. Rate Limiting

The API applies rate limiting at the HTTP boundary.

Its purpose is to control request volume and reduce abuse or resource exhaustion.

Different endpoints may require different policies.

For example:

* Login and registration.
* Verification-code requests.
* Password-reset requests.
* Public catalog queries.
* Expensive search endpoints.
* Sensitive administrative operations.

Rate-limit policy should reflect the endpoint's risk and expected usage.

A `429 Too Many Requests` response should be returned when a request exceeds the applicable limit.

Where appropriate, the API may include retry guidance such as `Retry-After`.

Rate limiting is an HTTP-level protection. It does not replace business-level limits, provider quotas, or infrastructure resource controls.

---

## 16. Security Headers

The API configures appropriate HTTP security headers.

Depending on the deployment and client model, these may include:

* `X-Content-Type-Options`
* `X-Frame-Options` or an appropriate Content Security Policy
* `Referrer-Policy`
* `Strict-Transport-Security` for HTTPS deployments

Headers should be selected based on the actual threat model.

For example, browser-oriented headers are useful for browser clients but are not a replacement for endpoint authorization.

The API should avoid emitting contradictory or redundant headers through multiple middleware components.

---

## 17. HTTPS and HSTS

The API enforces the intended HTTPS deployment policy.

HTTPS protects requests and responses in transit.

HSTS instructs compatible browsers to use HTTPS for subsequent access to the host.

The production configuration must account for reverse proxies and forwarded headers so the application interprets the original request scheme correctly.

HSTS should only be enabled where the deployment's HTTPS configuration is reliable.

---

## 18. Correlation and Request Logging

The API establishes request-level observability.

### Correlation

A correlation identifier helps connect logs and traces associated with a request.

The API should:

* Accept or generate a correlation identifier according to the established policy.
* Include it in relevant logging scopes.
* Return it to clients when appropriate.
* Avoid trusting arbitrary inbound values without validation.

### Request Logging

Serilog request logging provides structured information about HTTP requests and their outcomes.

Useful fields may include:

* HTTP method.
* Route or endpoint.
* Status code.
* Request duration.
* Correlation identifier.
* Trace identifier.

Sensitive information such as passwords, access tokens, verification codes, and payment credentials must not be logged.

**HTTP correlation middleware alone does not propagate correlation context into background jobs or Outbox processing.** That requires explicit context capture and propagation in the relevant Application and Infrastructure workflows.

---

## 19. HTTP Caching

The API implements HTTP response caching policy where appropriate.

The current design uses a global result filter and attributes, with startup validation of caching configuration.

The policy includes:

* Applying caching only to eligible GET responses.
* Caching successful responses where explicitly allowed.
* Applying `no-store` to unsuccessful responses.
* Supporting endpoint-specific cache configuration.
* Avoiding accidental caching of sensitive user-specific data.

The actual cache duration and cache directives must reflect the endpoint's requirements.

HTTP response caching and Redis application caching are different mechanisms.

The API controls HTTP caching behavior; Infrastructure owns Redis implementation.

---

## 20. Health Checks

The API exposes configured health-check endpoints.

Infrastructure supplies checks for dependencies such as:

* SQL Server.
* Redis.
* Hangfire.
* Outbox processing and backlog.

The API is responsible for mapping these checks to HTTP endpoints and controlling their exposure.

Health checks should distinguish between:

* **Liveness:** Whether the process is alive and able to continue running.
* **Readiness:** Whether the application is ready to receive the relevant traffic.

A dependency failure should not automatically make every health endpoint report the same result.

Readiness policy should reflect which dependencies are essential for the application to serve its intended requests.

Detailed operational information should not be exposed publicly without an explicit security decision.

---

## 21. Dependency Injection and Composition Root

The API contains the application's composition root, normally in `Program.cs` and associated startup extensions.

Its responsibility is to assemble the application by registering:

* Application services and MediatR handlers.
* Infrastructure implementations.
* ReadModel query services.
* Authentication and authorization.
* Middleware-related services.
* Health checks.
* Observability.
* API controllers and endpoint services.

### Composition Overview

```text
Program.cs
   |
   ├── Application Registration
   |
   ├── Infrastructure Registration
   |
   ├── ReadModel Registration
   |
   ├── Authentication / Authorization
   |
   ├── Observability / Health Checks
   |
   └── Middleware / Endpoint Mapping
```

The composition root is allowed to reference concrete implementations because its job is to assemble the running application.

This does not mean controllers or Application handlers should depend directly on those concrete implementations.

---

## 22. Configuration and Options

The API loads application configuration from supported configuration providers.

Typical sources include:

* `appsettings.json`.
* Environment-specific configuration files.
* Environment variables.
* Secret stores or other approved deployment configuration sources.

Configuration may cover:

* Connection strings.
* JWT settings.
* CORS policy.
* Rate limiting.
* HTTP caching.
* Health checks.
* Logging and telemetry.
* API versioning.
* Feature-specific options.

Sensitive configuration must not be committed to source control.

Options validation should fail early when required settings are invalid, rather than allowing the application to start in an unsafe or unusable state.

The API coordinates startup configuration but does not own the implementation of every subsystem whose options it registers.

---

## 23. Startup and Migration Coordination

The API composition root may coordinate startup operations, including database migration execution.

However, migration implementation belongs to the relevant persistence subsystem.

For example:

```text
API Startup
   |
   ├── Infrastructure Registration
   |
   ├── Write-Side EF Core Migration
   |
   └── ReadModel DbUp Migration
```

The write-side EF Core migrations and ReadModel DbUp migrations remain separate mechanisms.

The API should not contain SQL migration logic or duplicate migration implementation details.

Startup ordering must ensure required infrastructure is registered before its startup operations execute.

For a single-instance deployment, startup migration execution may be sufficient. A future multi-instance deployment must account for concurrent migration execution through deployment coordination or an appropriate locking strategy.

---

## 24. Swagger and OpenAPI

The API exposes OpenAPI documentation through Swagger tooling in the configured environments.

OpenAPI documents the HTTP contract, including:

* Endpoints.
* Request DTOs.
* Response DTOs.
* Status codes.
* Authentication requirements.
* API versions.

Swagger is a documentation and development tool, not a security boundary.

Its exposure should be configured deliberately in production rather than assumed to be safe or unsafe in every deployment.

---

## 25. Cancellation Propagation

The API propagates request cancellation to Application operations.

For example:

```csharp
await _sender.Send(command, cancellationToken);
```

ASP.NET Core provides a request-aborted cancellation token that can be used to stop unnecessary work after a client disconnects or cancels a request.

Application handlers and downstream services should pass the token to cancellable asynchronous operations.

Cancellation does not guarantee that every operation can be interrupted immediately. In particular, external systems and already-committed transactions may have their own cancellation semantics.

The API must not treat cancellation as proof that a state-changing operation did not commit.

---

## 26. File Upload and Download Endpoints

The API handles the HTTP-specific aspects of file operations.

These include:

* Receiving multipart uploads or other supported request formats.
* Enforcing request-level size limits.
* Validating HTTP input.
* Passing content and metadata to Application-facing file capabilities.
* Returning appropriate download responses.

Infrastructure implements file storage and file lifecycle behavior.

The API should not access storage paths directly or bypass the file service abstraction.

Transport-level size limits complement, rather than replace, the deeper content validation and storage protections implemented by the relevant service.

---

## 27. Payment Webhooks

Payment webhooks are external HTTP requests and therefore enter through the API boundary.

The API is responsible for:

* Receiving the provider callback.
* Binding the request according to the provider's contract.
* Passing the required callback data to the appropriate processing capability.
* Returning the expected HTTP response.

Infrastructure owns provider-specific technical verification, including Paymob HMAC verification and provider communication.

Application and Finance own the business workflow and payment state transitions.

### Responsibility Flow

```text
Paymob
  |
  v
API Webhook Endpoint
  |
  v
Application / Finance Workflow
  |
  v
Infrastructure Verification and Provider Adapter
  |
  v
Finance Domain Rules
  |
  v
Persistence / Outbox
```

The exact call sequence may place signature verification before dispatching the business operation, but the responsibility boundary remains the same.

The webhook endpoint must not independently mark an order as paid or bypass Finance business rules.

Webhook processing should account for duplicate deliveries and idempotency.

---

## 28. API and Background Jobs

The API may expose endpoints that request or initiate background work through Application use cases.

However, the API must not directly schedule Hangfire jobs.

### Intended Flow

```text
HTTP Request
    |
    v
API Controller
    |
    v
Application Use Case
    |
    v
Application Scheduling Abstraction
    |
    v
Infrastructure Hangfire Adapter
    |
    v
Hangfire
```

For durable business events, the transactional Outbox remains the established mechanism.

The API is not responsible for implementing job retries, job persistence, or Outbox dispatch.

---

## 29. API and Notifications

The API may expose endpoints for notification preferences, device registration, or retrieving user notifications.

It may also return the result of an Application use case that requests notification delivery.

It does not directly send email, SMS, or push notifications.

The responsibilities remain:

* **API:** HTTP endpoints.
* **Application:** Use-case orchestration and notification intent.
* **Infrastructure:** Channel implementations and delivery mechanisms.
* **Database:** Durable notification state where applicable.
* **SignalR:** Best-effort realtime notification hints.

This keeps notification delivery independent of HTTP controller implementation.

---

## 30. Error Response Contract

The API should return consistent, safe error responses.

Where appropriate, the application may use the standard ProblemDetails response model or a consistent custom error contract.

A production error response should provide enough information for clients to understand the failure without exposing internal implementation details.

For example:

```json
{
  "type": "about:blank",
  "title": "Conflict",
  "status": 409,
  "detail": "The resource was modified by another operation."
}
```

This is an illustrative response, not a required literal message.

The API should maintain consistent status codes and error formats across controllers.

Validation errors should identify invalid request fields where appropriate, while unexpected server errors should not disclose internal exception details.

---

## 31. API and Optimistic Concurrency

The API translates concurrency failures into an HTTP response appropriate to the endpoint contract.

For example:

```text
Client A reads resource
      |
      v
Client B modifies resource
      |
      v
Client A submits stale update
      |
      v
Infrastructure detects concurrency conflict
      |
      v
Application / API exception boundary
      |
      v
409 Conflict
```

The API does not implement SQL Server row-version logic.

It exposes the failure through HTTP so the client can refresh, reconcile, or retry according to the application's contract.

---

## 32. Observability

The API participates in the system's observability architecture.

It configures and uses the appropriate logging, metrics, and tracing integrations.

Typical responsibilities include:

* Structured HTTP request logging.
* Correlation identifiers.
* Distributed trace context for incoming requests.
* Request-duration and status metrics.
* Health-check endpoints.
* Consistent error reporting.

Infrastructure provides the relevant logging, metrics, tracing, and dependency health-check implementations.

The API should avoid duplicate telemetry where existing middleware or instrumentation already records the same information.

Observability should provide operational visibility without logging sensitive request bodies or credentials.

---

## 33. API Testing Strategy

The API should be tested at the HTTP boundary.

### Endpoint Tests

Verify:

* Routing.
* Request binding.
* Response status codes.
* Response contracts.
* Validation behavior.
* Cancellation handling.

### Authentication and Authorization Tests

Verify:

* Unauthenticated access.
* Forbidden operations.
* Role and policy enforcement.
* Correct authentication challenges.

### Middleware Tests

Verify:

* Exception mapping.
* CORS behavior.
* Rate limiting.
* Security headers.
* HTTP caching policy.

### Integration Tests

Use an appropriate test host and test dependencies to verify that the configured application behaves correctly across the HTTP and Application boundaries.

Application handlers and Domain behavior should also have their own tests. API tests should not attempt to replace them.

---

## 34. API Boundaries and Prohibited Responsibilities

The API must not:

* Implement business invariants.
* Reproduce aggregate state transitions.
* Directly access EF Core `DbContext` for business operations.
* Manage database transactions manually.
* Implement repository persistence.
* Depend on ASP.NET Core Identity stores directly for use-case logic.
* Call Paymob directly from ordinary controllers.
* Publish integration events directly from controllers.
* Call Hangfire scheduling APIs directly.
* Access Redis directly for business behavior.
* Send email or SMS directly.
* Send Firebase push notifications directly.
* Publish SignalR messages directly as a substitute for Application workflows.
* Access file-system paths directly for business file operations.
* Implement ReadModel SQL queries in controllers.

Instead, the API delegates operations to Application and lets the relevant implementation layer perform its responsibilities.

---

## 35. API and Modular Monolith Alignment

The API serves as the single HTTP entry point for the modular monolith.

The architecture retains:

* One running application process.
* Explicit Application and Domain module boundaries.
* Shared infrastructure where appropriate.
* A shared database where appropriate.
* A shared Unit of Work for operations that require atomicity.
* Specialized ReadModel query paths where justified.

The API does not need separate HTTP services for every module.

A module may expose multiple controllers or endpoint groups while remaining inside the same application.

The modular monolith can therefore preserve logical separation without introducing unnecessary service-to-service communication.

---

## 36. API vs Application vs Domain vs Infrastructure vs ReadModel

| Concern                                   | API | Application                         | Domain                 | Infrastructure              | ReadModel              |
| ----------------------------------------- | --- | ----------------------------------- | ---------------------- | --------------------------- | ---------------------- |
| HTTP endpoints                            | ✓   | —                                   | —                      | —                           | —                      |
| Request binding                           | ✓   | —                                   | —                      | —                           | —                      |
| API request/response DTOs                 | ✓   | —                                   | —                      | —                           | —                      |
| Commands and queries                      | —   | ✓                                   | —                      | —                           | —                      |
| Use-case orchestration                    | —   | ✓                                   | —                      | —                           | —                      |
| Application pipeline behaviors            | —   | ✓                                   | —                      | —                           | —                      |
| Business invariants                       | —   | —                                   | ✓                      | —                           | —                      |
| Aggregate state transitions               | —   | —                                   | ✓                      | —                           | —                      |
| Domain events                             | —   | —                                   | ✓                      | —                           | —                      |
| Repository abstractions                   | —   | ✓ or Domain, according to ownership | ✓ where Domain-owned   | —                           | —                      |
| Repository implementations                | —   | —                                   | —                      | ✓                           | —                      |
| Unit of Work implementation               | —   | —                                   | —                      | ✓                           | —                      |
| EF Core                                   | —   | —                                   | —                      | ✓                           | —                      |
| ASP.NET Core Identity                     | —   | —                                   | —                      | ✓                           | —                      |
| JWT implementation                        | —   | —                                   | —                      | ✓                           | —                      |
| Paymob integration                        | —   | —                                   | —                      | ✓                           | —                      |
| Hangfire implementation                   | —   | —                                   | —                      | ✓                           | —                      |
| Redis implementation                      | —   | —                                   | —                      | ✓                           | —                      |
| SignalR implementation                    | —   | —                                   | —                      | ✓                           | —                      |
| Email/SMS/push delivery                   | —   | —                                   | —                      | ✓                           | —                      |
| Specialized read contracts                | —   | ✓                                   | —                      | —                           | —                      |
| ADO.NET query implementation              | —   | —                                   | —                      | —                           | ✓                      |
| Stored procedures for specialized reads   | —   | —                                   | —                      | —                           | ✓                      |
| HTTP authentication configuration         | ✓   | —                                   | —                      | —                           | —                      |
| Application permission requirements       | —   | ✓                                   | —                      | —                           | —                      |
| ASP.NET Core authorization implementation | —   | —                                   | —                      | ✓                           | —                      |
| HTTP exception mapping                    | ✓   | —                                   | —                      | —                           | —                      |
| Application error semantics               | —   | ✓                                   | ✓ where domain-related | ✓ for technical translation | —                      |
| Middleware and endpoint routing           | ✓   | —                                   | —                      | —                           | —                      |
| Dependency injection composition root     | ✓   | Registers application services      | —                      | Supplies registrations      | Supplies registrations |

---

## 37. Final Architectural Rules

The API layer follows these rules:

1. **API is the HTTP boundary and composition root.**
2. **Controllers remain thin and delegate use cases to Application.**
3. **MediatR dispatches commands and queries; it does not replace the architectural layers.**
4. **API owns HTTP request and response DTOs.**
5. **API DTOs remain separate from Application DTOs and Domain entities.**
6. **Application owns use-case orchestration.**
7. **Domain owns business invariants and aggregate state transitions.**
8. **Infrastructure owns concrete persistence and external-provider implementations.**
9. **ReadModel owns the specialized ADO.NET read implementation.**
10. **API may reference Infrastructure and ReadModel for composition, but Application and Domain must not depend on API.**
11. **Infrastructure must not depend on API or ReadModel.**
12. **API performs HTTP-level validation without replacing Application or Domain validation.**
13. **Authentication and authorization are configured at the HTTP boundary, while use-case permission requirements remain in Application.**
14. **Global exception handling translates failures into consistent HTTP responses.**
15. **Middleware order is deliberate and must be verified against the actual startup configuration.**
16. **CORS is not authentication or authorization.**
17. **Rate limiting is an HTTP protection, not a substitute for business or provider limits.**
18. **HTTP caching must account for sensitive and user-specific data.**
19. **Correlation and tracing context must be explicitly propagated when work crosses into background processing.**
20. **The API does not schedule Hangfire jobs directly.**
21. **The API does not bypass Finance workflows when processing payment callbacks.**
22. **Concurrency conflicts are exposed through appropriate HTTP responses.**
23. **Health-check exposure must follow an explicit operational security policy.**
24. **Startup migration execution is coordinated by the composition root; migration implementation remains in its owning subsystem.**
25. **The API remains a thin transport boundary within the modular monolith.**

---

## 38. Final Responsibility Matrix

| Layer              | Primary Responsibility                                                                         |
| ------------------ | ---------------------------------------------------------------------------------------------- |
| **API**            | HTTP boundary, endpoint routing, request/response translation, middleware, composition         |
| **Application**    | Use cases, orchestration, commands, queries, application contracts                             |
| **Domain**         | Business rules, aggregates, invariants, domain events                                          |
| **Infrastructure** | EF Core, persistence, Identity, external integrations, messaging, jobs, caching, observability |
| **ReadModel**      | Specialized read implementation using ADO.NET and stored procedures                            |
| **Database**       | Durable data and transactional persistence                                                     |

### Standard Request Flow

```text
External Client
     |
     v
API
     |
     v
Application
     |
     v
Domain
     |
     v
Infrastructure
     |
     v
Database / External Systems
```

For specialized reads:

```text
External Client
     |
     v
API
     |
     v
Application
     |
     v
ReadModel
     |
     v
ADO.NET / SQL Server
```

---

## 39. Final Architecture Summary

`E-Commerce.Api` is the HTTP entry point and composition root of the E-Commerce modular monolith.

It exposes application capabilities through endpoints, translates HTTP contracts into Application commands and queries, and converts application results into consistent HTTP responses.

It owns middleware configuration, request logging, correlation, exception handling, API versioning, CORS, rate limiting, HTTP caching, authentication configuration, endpoint authorization metadata, and health-check exposure.

It composes Application, Infrastructure, and ReadModel into the running application without taking ownership of their implementations.

It does not implement business rules, persistence, payment processing, notification delivery, background scheduling, or specialized SQL queries.

The resulting architecture preserves a clear boundary:

**API translates HTTP. Application orchestrates use cases. Domain enforces business rules. Infrastructure implements technical capabilities. ReadModel optimizes specialized reads.**

This separation supports maintainability, security, testability, observability, and long-term evolution without introducing unnecessary distributed infrastructure.
