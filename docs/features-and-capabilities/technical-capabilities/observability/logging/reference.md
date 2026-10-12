# Logging — Technical Reference Document

**Project:** E-Commerce Modular Monolith
**Capability:** Cross-Cutting Observability — Structured Logging
**Version:** 1.0 — Final

---

## 1. Overview

The Logging capability provides structured, correlated, and traceable log output across all layers of the E-Commerce modular monolith.

It uses **Serilog** for structured logging, with request correlation, application-operation logging, performance monitoring, and sensitive-data protection.

Logging is part of the broader observability stack but focuses exclusively on the **log signal**. Metrics, distributed tracing, and health checks remain separate capabilities that may integrate with logging through shared contextual metadata.

### Core responsibilities

* Structured application and infrastructure logging.
* HTTP request correlation.
* Application-operation identification.
* Slow-operation detection.
* Correlation propagation into asynchronous integration-event handlers.
* Global and request-scoped log enrichment.
* Protection against sensitive-data exposure.
* Support for console, rolling-file, and optional centralized logging sinks.

---

## 2. Architecture

The logging pipeline consists of the following components:

```text
HTTP Request
     |
     v
CorrelationIdMiddleware
(CorrelationId + UserId logging scope)
     |
     v
SerilogRequestLogging
(HTTP request completion logging)
     |
     v
MediatR Pipeline
     |
     +--> LoggingBehavior
     |    (Application operation name + UserId)
     |
     +--> PerformanceBehavior
     |    (Execution duration + slow-operation warning)
     |
     v
Command / Query Handler
(ILogger<T> for specific business and technical events)
     |
     v
Serilog
(Structured log event processing and enrichment)
     |
     v
Configured Sinks
     |
     +--> Console
     |
     +--> Rolling File
     |
     +--> Optional Centralized Logging Platform
```

Global metadata and request-scoped properties are attached to log events through Serilog enrichment and logging scopes.

Background processing uses a separate execution path. Integration-event handlers restore the original correlation identifier when it is available in the event envelope or payload.

---

## 3. Folder Structure

```text
E-Commerce.Api/
└── Middleware/
    └── CorrelationIdMiddleware.cs

E-Commerce.Application/
└── Shared/
    ├── Abstractions/
    │   └── IAppContext.cs
    ├── Behaviors/
    │   ├── LoggingBehavior.cs
    │   └── PerformanceBehavior.cs
    ├── Constants/
    │   └── ContextKeys.cs
    └── Communication/
        └── Messaging/
            └── Decorators/
                └── CorrelationScopeIntegrationEventHandler.cs

E-Commerce.Infrastructure/
└── Observability/
    └── Logging/
        ├── ExecutionContext.cs
        └── LoggingExtensions.cs
```

The structure separates responsibilities by architectural layer:

* **API:** Establishes HTTP request correlation.
* **Application:** Defines execution-context abstractions and application-operation logging behaviors.
* **Infrastructure:** Implements execution-context access and configures Serilog.

---

## 4. Component Details

### 4.1 CorrelationIdMiddleware

**Layer:** API

**Responsibility:** Establishes and propagates an HTTP request correlation identifier.

#### Behavior

1. Reads `X-Correlation-ID` from the incoming request header.
2. Generates a new identifier when an acceptable identifier is not supplied.
3. Stores the identifier in `HttpContext.Items` using `ContextKeys.CorrelationId`.
4. Adds the identifier to the outgoing response through the `X-Correlation-ID` header.
5. Obtains the authenticated user's identifier from the claims principal when available, falling back to `"anonymous"` for unauthenticated requests.
6. Begins a logging scope containing `CorrelationId` and `UserId`.

#### Important considerations

* Authentication must run before this middleware if the middleware needs to read the authenticated user's claims.
* Correlation identifiers supplied by clients should be validated for acceptable length and format before being accepted.
* A correlation identifier is for diagnostic association, not authentication or authorization.
* Correlation identifiers should be propagated to downstream operations where appropriate.

The implementation should use structured scope properties so that the identifiers become searchable log fields rather than being embedded only in message text.

### 4.2 IAppContext

**Layer:** Application

**Responsibility:** Provides decoupled access to execution metadata when application logic requires it.

The abstraction exposes relevant contextual information, including:

| Property        | Purpose                                                           |
| --------------- | ----------------------------------------------------------------- |
| `AppName`       | Application identity.                                             |
| `Environment`   | Deployment environment.                                           |
| `Version`       | Application version.                                              |
| `MachineName`   | Host or machine identity.                                         |
| `CorrelationId` | Identifier associated with the current execution, when available. |
| `UserId`        | Authenticated user identifier, when available.                    |

The exact interface should expose only the properties required by Application consumers.

**Design rule:** Application services should not depend on `IAppContext` solely to produce log entries. Logging should normally use `ILogger<T>`, while `IAppContext` should be used when execution metadata is genuinely needed for application behavior.

### 4.3 LoggingBehavior

**Layer:** Application — MediatR pipeline

**Responsibility:** Records the identity of an application operation.

#### Behavior

* Logs the MediatR request type or operation name at the start of execution.
* Includes the user identifier when available.
* Does not measure execution duration.
* Does not log normal completion or failure.
* Does not serialize the complete command or query object.

This behavior provides consistent operation-level visibility without duplicating performance monitoring or handler-specific logging.

### 4.4 PerformanceBehavior

**Layer:** Application — MediatR pipeline

**Responsibility:** Measures application-operation duration and identifies slow-running operations.

#### Behavior

1. Starts a `Stopwatch` before invoking the next MediatR pipeline component.
2. Measures elapsed time after execution completes.
3. Compares the elapsed duration against a configured threshold, such as 500 milliseconds.
4. Logs a warning when the threshold is exceeded.
5. Includes the operation name, elapsed milliseconds, and user identifier when available.

Normal completion does not need to produce an additional log entry.

#### Important considerations

* The threshold should be configurable where different workloads require different performance expectations.
* The implementation should define whether duration includes inner pipeline behaviors. The exact measurement depends on behavior registration order.
* Exceptions should propagate normally. If the behavior uses `finally` to measure elapsed time, it should avoid accidentally suppressing the original exception.
* Slow-operation warnings are diagnostic signals, not substitutes for metrics or profiling.

### 4.5 ContextKeys

**Layer:** Application

**Responsibility:** Defines shared constants for execution-context keys.

Example:

```csharp
public static class ContextKeys
{
    public const string CorrelationId = "CorrelationId";
}
```

The exact implementation may contain additional keys as required by the application.

Using shared constants avoids duplicated string literals when components access contextual values.

### 4.6 CorrelationScopeIntegrationEventHandler

**Layer:** Application

**Responsibility:** Restores correlation context when processing asynchronous integration events.

#### Behavior

1. Reads the correlation identifier from the incoming integration event or its execution envelope.
2. Begins a logging scope containing that identifier.
3. Invokes the decorated integration-event handler within the scope.
4. Disposes the scope after processing completes.

This allows logs produced by background event handlers to be associated with the original operation when the publisher propagated its correlation identifier.

#### Important considerations

* The publisher must include the correlation identifier in the event or envelope.
* The decorator must be registered with the dependency-injection container and match the application's actual integration-event handler abstraction.
* Background jobs that do not originate from an HTTP request should establish their own correlation context.
* Correlation identifiers support diagnostic association; they do not guarantee end-to-end distributed tracing.

### 4.7 ExecutionContext

**Layer:** Infrastructure

**Responsibility:** Implements `IAppContext` using the application's available execution metadata.

#### Dependencies

* `IHttpContextAccessor` for HTTP request context.
* `ICurrentUser` for authenticated-user information.
* `IConfiguration` for configured application metadata.
* `IHostEnvironment` for environment information.

The implementation should handle the absence of an active HTTP request, as can occur in background jobs.

**Design rule:** Infrastructure adapts framework-specific context sources to the Application abstraction. Application code should not depend directly on `HttpContext`, ASP.NET Core hosting APIs, or configuration infrastructure to retrieve execution metadata.

### 4.8 LoggingExtensions

**Layer:** Infrastructure

**Responsibility:** Configures Serilog and registers logging infrastructure during application startup.

#### Behavior

* Reads Serilog configuration through `ReadFrom.Configuration`.
* Adds global enrichers for `AppName`, `Environment`, `Version`, and `MachineName`.
* Configures console and rolling-file sinks.
* Integrates Serilog with the ASP.NET Core host through `builder.Host.UseSerilog()`.

The registration should establish one deliberate configuration path.

**Important:** Avoid configuring the same sink both programmatically and through configuration unless duplicate output is intentional. If configuration is split between code and `appsettings.json`, document which component owns each setting.

---

## 5. Data Flow

The standard HTTP request flow is:

1. **Application startup:** `AddInfrastructureLogging()` configures Serilog and registers the logging infrastructure.
2. **HTTP request arrival:** `CorrelationIdMiddleware` establishes the correlation identifier and logging scope.
3. **HTTP request completion:** `UseSerilogRequestLogging()` records the HTTP request summary.
4. **Application operation:** `LoggingBehavior` records the operation identity and user identifier when available.
5. **Performance monitoring:** `PerformanceBehavior` measures execution duration and logs slow operations.
6. **Handler execution:** Handlers and services use `ILogger<T>` to record meaningful business or technical events.
7. **Log processing:** Serilog enriches and routes structured log events to the configured sinks.

For asynchronous integration-event processing:

1. The originating operation establishes a correlation identifier.
2. The publisher propagates that identifier with the integration event or its envelope.
3. The receiving handler restores the identifier into a logging scope.
4. Logs emitted during event processing retain the correlation property.

This provides diagnostic continuity across HTTP requests and asynchronous processing, provided the correlation metadata is propagated correctly.

---

## 6. Log Enrichment

The logging pipeline supports three categories of metadata.

| Category         | Properties                                         | Purpose                                                                                                         |
| ---------------- | -------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| Global metadata  | `AppName`, `Environment`, `Version`, `MachineName` | Identifies the application and its execution environment.                                                       |
| Request metadata | `CorrelationId`, `UserId`                          | Associates log entries with a request and, where applicable, a user.                                            |
| Trace metadata   | `TraceId`, `SpanId`                                | Associates logs with distributed tracing activities when tracing is configured and the properties are enriched. |

These properties improve searchability and allow related events to be analyzed together.

### Correlation versus tracing

A correlation identifier and a trace identifier are related but distinct concepts:

* **Correlation ID:** An application-level identifier used to associate related operations.
* **Trace ID:** Identifies a distributed trace.
* **Span ID:** Identifies an individual operation within a trace.

The logging capability can support both correlation and tracing, but adding `CorrelationId` does not automatically configure distributed tracing or populate `TraceId` and `SpanId`.

---

## 7. Sensitive Data Handling

The logging implementation must avoid exposing secrets, credentials, and unnecessary personal information.

### Logging rules

* Never serialize complete command or query objects by default.
* Log operation names, entity identifiers, and explicitly approved metadata.
* Never log passwords, password-reset tokens, authentication tokens, OTPs, API keys, or other secrets.
* Avoid logging payment credentials or sensitive payment-provider payloads.
* Avoid logging full personal-data records or unnecessary personally identifiable information.
* Review exception messages and third-party provider responses before logging them, as they may contain sensitive values.

Use structured message templates rather than string interpolation.

Example:

```csharp
_logger.LogInformation(
    "Order {OrderId} was submitted by user {UserId}",
    orderId,
    userId);
```

Structured logging makes individual fields searchable and filterable without embedding them exclusively in the message text.

Sensitive-data protection should be enforced through implementation discipline, safe logging contracts, and appropriate review. It should not rely solely on developers remembering to redact values manually.

---

## 8. Sink Configuration

The current configuration uses two sinks.

| Sink         | Configuration             | Purpose                                 |
| ------------ | ------------------------- | --------------------------------------- |
| Console      | Structured console output | Development and container environments. |
| Rolling file | `logs/ecommerce-.log`     | Local or host-based log persistence.    |

### Rolling-file settings

| Setting          | Value                 |
| ---------------- | --------------------- |
| File path        | `logs/ecommerce-.log` |
| Rolling interval | Daily                 |
| Retained files   | 7                     |
| File-size limit  | 10 MB                 |

These settings describe the intended configuration; the actual behavior depends on the sink options used in the implementation.

### Operational considerations

* Ensure the application has permission to write to the configured directory.
* Define an appropriate retention and storage policy for the deployment environment.
* Confirm whether container logs are collected externally before relying on local file persistence.
* Avoid duplicate sinks when combining `ReadFrom.Configuration` with programmatic configuration.
* Do not assume rolling files alone provide centralized search, durable archival, or alerting.

An optional centralized logging platform can be introduced without changing the Application-layer logging abstractions, provided the platform is integrated through the logging configuration and compatible sinks or exporters.

---

## 9. Middleware and Pipeline Ordering

Middleware order affects which contextual properties are available when log events are created.

### HTTP middleware

When `CorrelationIdMiddleware` reads `HttpContext.User`, authentication must run first. Serilog request logging should run after correlation middleware so the request-completion event can include the correlation scope.

A representative pipeline is:

```csharp
app.UseRouting();

app.UseAuthentication();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSerilogRequestLogging();

app.UseAuthorization();
```

The final order must remain consistent with the application's actual routing, authentication, authorization, and endpoint configuration.

If correlation middleware does not depend on authenticated claims, it may be placed earlier, but user-specific enrichment will only be available after authentication has populated the principal.

### MediatR behaviors

`LoggingBehavior` and `PerformanceBehavior` serve separate purposes:

* `LoggingBehavior` records operation identity.
* `PerformanceBehavior` records execution duration and warns about slow operations.

Their registration order can be chosen according to the intended pipeline semantics. Because one behavior may execute inside another, registration order can affect which work is included in a duration measurement.

The selected order should be documented and verified against the actual MediatR registration.

---

## 10. Integration with Observability

Logging is one component of the broader observability capability.

| Capability          | Primary responsibility                        | Relationship with logging                                         |
| ------------------- | --------------------------------------------- | ----------------------------------------------------------------- |
| Logging             | Records structured events                     | Provides detailed diagnostic context.                             |
| Metrics             | Measures numerical values over time           | Supports dashboards, thresholds, and alerting.                    |
| Distributed tracing | Tracks operations across execution boundaries | Provides trace and span identifiers that can be attached to logs. |
| Health checks       | Reports application and dependency health     | Helps identify availability and readiness problems.               |
| Alerting            | Notifies operators of actionable conditions   | Can use signals derived from logs, metrics, or health checks.     |

The logging capability should remain focused on producing structured, useful, and secure log events rather than absorbing the responsibilities of these other capabilities.

For asynchronous processing, correlation metadata should be propagated explicitly. When distributed tracing is configured, trace context should also be propagated through the appropriate tracing mechanisms rather than relying on a correlation ID alone.

---

## 11. Operational and Reliability Considerations

A production-oriented logging implementation should account for the following:

* **Logging failures:** Logging should not ordinarily cause business operations to fail merely because a diagnostic sink is unavailable. The behavior depends on the configured logging pipeline and sink.
* **Storage limits:** Rolling-file retention and size limits should be reviewed against actual log volume and disk capacity.
* **Configuration validation:** Required application metadata and sink settings should be validated at startup where practical.
* **Correlation propagation:** Missing correlation identifiers in background processing should be handled deliberately rather than assumed to be impossible.
* **Sensitive data:** Logs should be treated as operational data that may require access controls, retention limits, and protection at rest.
* **Performance:** Avoid unnecessarily expensive serialization, excessive logging, and high-volume logs at inappropriate levels.
* **Centralized collection:** If logs are shipped to an external platform, configure the appropriate transport, access controls, retention, and failure behavior separately.

These considerations help keep logging useful during incidents without introducing unnecessary dependencies into the business layers.

---

## 12. Summary

The Logging capability provides a centralized, structured, and security-conscious logging pipeline for the E-Commerce modular monolith.

It uses Serilog in Infrastructure, exposes a minimal `IAppContext` abstraction in Application, establishes HTTP correlation through API middleware, and uses MediatR behaviors for operation identification and slow-operation detection. Integration-event handlers restore correlation metadata for asynchronous processing when the publisher propagates it.

The design separates operation logging from performance monitoring, keeps framework-specific logging configuration in Infrastructure, and supports console, rolling-file, and optional centralized log aggregation.

**The result is a focused logging capability that improves diagnostic visibility while preserving architectural boundaries and avoiding unnecessary coupling between business logic and logging infrastructure.**
