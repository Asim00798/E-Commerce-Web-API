# Logging — Technical Requirements Document

**Project:** E-Commerce Modular Monolith
**Capability:** Cross-Cutting Observability — Structured Logging
**Version:** 1.0 — Final

---

## 1. Introduction

The Logging capability provides structured, correlated, and secure log output across all layers of the E-Commerce modular monolith. It is part of the broader observability stack but focuses exclusively on the **log signal**.

This document defines the business goals, functional requirements, non-functional requirements, and architectural constraints that guide the implementation of logging.

Logging is a cross-cutting infrastructure concern. The Domain layer remains independent of logging frameworks and infrastructure-specific concerns. The Application layer uses the standard `ILogger<T>` abstraction for logging and a minimal `IAppContext` abstraction when execution metadata is required for application behavior.

Serilog is the underlying logging implementation, configured and managed by Infrastructure.

---

## 2. Business Goals

The Logging capability must:

* Enable rapid diagnosis and troubleshooting of application issues.
* Provide consistent, machine-readable log events for analysis and operational alerting.
* Preserve relevant execution context, including user, request, and trace identifiers when available.
* Prevent sensitive information from entering application logs.
* Support operational monitoring without coupling business logic to a specific logging vendor.
* Maintain clear architectural boundaries between business logic and logging infrastructure.

---

## 3. Scope

### 3.1 In Scope

* Structured logging using Serilog.
* Startup configuration through an Infrastructure extension method.
* Request-level correlation ID middleware.
* Application-operation logging through a MediatR pipeline behavior.
* Performance monitoring through a MediatR pipeline behavior, including slow-operation warnings.
* Global log enrichment with application name, environment, version, and machine name.
* Correlation scopes for integration-event handlers.
* An Application-layer `IAppContext` abstraction and its Infrastructure implementation.
* Console and rolling-file sinks.
* Sensitive-data protection requirements.
* Configuration that permits additional logging sinks without changing Application-layer code.

### 3.2 Out of Scope

* Metrics collection, including OpenTelemetry metrics.
* Distributed tracing implementation, including OpenTelemetry tracing.
* Health checks.
* Alerting and notification delivery.
* Configuration and operation of a centralized log aggregation platform.
* Deployment-specific log retention, access control, archival, and compliance policies.

The design may integrate with these capabilities in the future without expanding the responsibility of the Logging capability itself.

---

## 4. Functional Requirements

### 4.1 Serilog Integration

**FR-LOG-01 — Logging Provider**

The application must use Serilog as its logging provider.

**FR-LOG-02 — Startup Configuration**

Serilog must be configured during application startup through an Infrastructure extension method, such as `AddInfrastructureLogging()`.

**FR-LOG-03 — Configuration Sources**

The logging configuration must support `IConfiguration`, programmatic enrichers, and sink configuration.

**FR-LOG-04 — Standard Logging Abstraction**

The standard `ILogger<T>` abstraction must route log events through Serilog without requiring Application-layer changes when the underlying logging provider or sinks are changed.

### 4.2 Structured Logging

**FR-LOG-05 — Structured Message Templates**

Application log entries must use structured message templates with named properties where applicable.

Example:

```csharp
_logger.LogInformation(
    "Order {OrderId} was placed",
    orderId);
```

**FR-LOG-06 — Structured Properties**

Log properties must remain available as structured key/value pairs rather than existing only as formatted message text.

**FR-LOG-07 — Global Metadata**

Log events must include the following global metadata:

* `AppName`
* `Environment`
* `Version`
* `MachineName`

These properties must be populated through the configured enrichment mechanism.

### 4.3 Correlation ID Middleware

**FR-LOG-08 — Correlation ID Resolution**

The middleware must read `X-Correlation-ID` from the incoming request header. If the header is absent or its value is rejected by validation, the middleware must generate a new GUID.

**FR-LOG-09 — Request Context Storage**

The middleware must store the resolved correlation identifier in `HttpContext.Items` using `ContextKeys.CorrelationId`.

**FR-LOG-10 — Response Header**

The middleware must include the resolved identifier in the outgoing `X-Correlation-ID` response header.

**FR-LOG-11 — Logging Scope**

The middleware must establish a logging scope containing:

* `CorrelationId`
* `UserId`

The scope must remain active for the relevant request-processing pipeline and be disposed of when the request completes.

### 4.4 User ID Enrichment

**FR-LOG-12 — User Identification**

The middleware must obtain `UserId` from the authenticated claims principal when available.

**FR-LOG-13 — Anonymous Requests**

When no authenticated user identifier is available, the middleware must use `"anonymous"` as the logging-scope value.

**FR-LOG-14 — Authentication Ordering**

If the middleware reads the authenticated principal, it must execute after authentication has populated `HttpContext.User`.

### 4.5 Application-Operation Logging — LoggingBehavior

**FR-LOG-15 — Operation Identification**

`LoggingBehavior` must log the MediatR request name at the start of each request.

**FR-LOG-16 — User Context**

The behavior must include the user identifier when available.

**FR-LOG-17 — Request Data Protection**

The behavior must not serialize or automatically log the complete command or query object.

**FR-LOG-18 — Responsibility Separation**

`LoggingBehavior` must not measure execution duration or independently log normal completion or failure. Performance monitoring belongs to `PerformanceBehavior`, while exception handling and failure logging belong to their designated components.

### 4.6 Performance Monitoring — PerformanceBehavior

**FR-LOG-19 — Execution Measurement**

`PerformanceBehavior` must measure the execution duration of each MediatR request.

**FR-LOG-20 — Slow-Operation Detection**

When execution exceeds the configured threshold, the behavior must emit a warning containing:

* Request name.
* Elapsed duration in milliseconds.
* User identifier when available.

**FR-LOG-21 — Configurable Threshold**

The slow-operation threshold must be configurable. A default value of 500 milliseconds may be used unless the implementation specifies another justified default.

**FR-LOG-22 — Responsibility Separation**

The behavior must not emit routine success or failure logs solely to duplicate other pipeline responsibilities.

Exceptions must propagate according to normal application behavior. Performance measurement must not suppress or replace the original exception.

### 4.7 Integration-Event Correlation

**FR-LOG-23 — Correlation Restoration**

An integration-event handler decorator must restore correlation context into a logging scope when a correlation identifier is available.

**FR-LOG-24 — Event Metadata**

The decorator must obtain the correlation identifier from the integration event or its associated envelope.

**FR-LOG-25 — Missing Correlation Identifier**

If the incoming event contains no correlation identifier, the decorator must not create an additional correlation scope solely for that missing value.

**FR-LOG-26 — Publisher Propagation**

The integration-event publishing flow must propagate the originating correlation identifier when one exists.

### 4.8 IAppContext Abstraction

**FR-LOG-27 — Execution Metadata Access**

Application code must be able to access execution metadata through the `IAppContext` abstraction when that information is required by application behavior.

**FR-LOG-28 — Required Properties**

`IAppContext` must expose the following properties:

| Property        | Purpose                                               |
| --------------- | ----------------------------------------------------- |
| `AppName`       | Application identity.                                 |
| `Environment`   | Deployment environment.                               |
| `Version`       | Application version.                                  |
| `MachineName`   | Host or machine identity.                             |
| `CorrelationId` | Current correlation identifier when available.        |
| `UserId`        | Current authenticated user identifier when available. |

**FR-LOG-29 — Infrastructure Implementation**

The concrete implementation must reside in Infrastructure and adapt the relevant framework services, including:

* `IHttpContextAccessor`
* `ICurrentUser`
* `IConfiguration`
* `IHostEnvironment`

**FR-LOG-30 — Background Execution**

The implementation must handle execution without an active HTTP request, including background jobs and asynchronous event processing.

Application code must not assume that HTTP-specific context is always available.

### 4.9 Sink Configuration

**FR-LOG-31 — Console Sink**

A console sink must be available for development and environments that collect application output through container or host logging.

**FR-LOG-32 — Rolling-File Sink**

A rolling-file sink must support daily rollover, retention of seven files, and a configured file-size limit.

The intended configuration is:

| Setting          | Requirement           |
| ---------------- | --------------------- |
| File path        | `logs/ecommerce-.log` |
| Rolling interval | Daily                 |
| Retained files   | 7                     |
| File-size limit  | 10 MB                 |

The implementation must use the appropriate Serilog sink options to enforce the intended behavior.

**FR-LOG-33 — Centralized Configuration**

Sink configuration must have a clearly defined source of truth. The implementation must avoid unintentionally registering the same sink through both configuration and programmatic setup.

---

## 5. Non-Functional Requirements

### 5.1 Performance

**NFR-LOG-01 — Low Overhead**

Logging overhead must be minimized. Middleware, logging scopes, and MediatR behaviors must not introduce unnecessary request-processing costs.

**NFR-LOG-02 — File I/O**

The file sink should use an appropriate asynchronous or buffered strategy where supported and configured, so routine file writes do not unnecessarily block request processing.

**NFR-LOG-03 — Efficient Event Creation**

The implementation must avoid unnecessary serialization, excessive logging, and expensive processing for events that will not be emitted.

### 5.2 Reliability

**NFR-LOG-04 — Logging Failure Isolation**

A logging-sink failure should not ordinarily cause a business operation to fail solely because diagnostic output could not be written.

**NFR-LOG-05 — Sink Failure Diagnostics**

Sink failures must be diagnosable through suitable mechanisms, such as Serilog's self-logging facilities, without creating recursive logging failures.

**NFR-LOG-06 — Original Exception Preservation**

Logging and performance instrumentation must not suppress or replace application exceptions.

### 5.3 Maintainability

**NFR-LOG-07 — Layer Isolation**

Logging components must remain separated according to their architectural responsibilities.

**NFR-LOG-08 — Provider Independence**

Changing sink configuration or introducing a compatible logging destination must not require changes to Application or Domain code.

**NFR-LOG-09 — Focused Responsibilities**

Operation logging, performance measurement, request correlation, and sink configuration must remain separate concerns.

### 5.4 Security

**NFR-LOG-10 — Secret Exclusion**

Logs must never contain passwords, OTPs, authentication tokens, API keys, payment credentials, or other secrets.

**NFR-LOG-11 — Request and Response Protection**

Full HTTP request and response bodies must not be logged automatically.

**NFR-LOG-12 — Personal Data Minimization**

Only necessary and approved personal information may be included in logs. User and correlation identifiers must be handled as potentially sensitive operational metadata and protected through appropriate access controls.

**NFR-LOG-13 — Correlation Header Validation**

Client-provided correlation identifiers must be validated before use to prevent oversized or malformed values from contaminating log records or response headers.

**NFR-LOG-14 — Safe Exception Logging**

Exception details and external-provider responses must be reviewed for potential secret or personal-data exposure before being written to logs.

### 5.5 Scalability and Extensibility

**NFR-LOG-15 — Additional Logging Destinations**

The design must support adding centralized logging destinations, such as Seq, Elasticsearch, or Loki, without requiring changes to Application-layer code.

**NFR-LOG-16 — Deployment Flexibility**

Logging destinations and relevant sink settings must be configurable for the deployment environment.

---

## 6. Architectural Constraints

The following constraints are mandatory.

### 6.1 Domain Layer

* The Domain layer must not depend on Serilog, ASP.NET Core, or infrastructure-specific logging implementations.
* Domain entities and value objects must not require logging services.
* Domain invariants and business decisions must remain independent of logging infrastructure.

### 6.2 Application Layer

* Application code must use the standard `ILogger<T>` abstraction for logging.
* Application code may depend on `IAppContext` when execution metadata is needed for application behavior.
* Application code must not reference Serilog-specific APIs.
* Application code must not depend directly on `HttpContext` or other ASP.NET Core infrastructure types for execution metadata.
* Logging behaviors must not serialize full request objects by default.

### 6.3 Infrastructure Layer

* Infrastructure owns Serilog configuration, sink registration, enrichment setup, and the concrete `IAppContext` implementation.
* Infrastructure adapts framework-specific services to the abstractions exposed by Application.
* Logging configuration must not introduce business rules into Infrastructure.

### 6.4 API Layer

* `CorrelationIdMiddleware` establishes HTTP request correlation.
* Middleware ordering must ensure that authenticated user claims are available before the middleware reads them.
* `UseSerilogRequestLogging()` must execute after correlation middleware if request-completion logs are expected to include the correlation scope.

### 6.5 Pipeline Responsibilities

* `LoggingBehavior` records application-operation identity.
* `PerformanceBehavior` measures execution duration and emits slow-operation warnings.
* The integration-event handler decorator restores correlation metadata for asynchronous processing.
* Each component must remain focused on its designated responsibility.

---

## 7. Acceptance Criteria

The implementation is accepted when the following criteria are verified.

| ID    | Acceptance criterion                                                                                   | Verification                                                                         |
| ----- | ------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------ |
| AC-01 | Serilog is configured and registered as the application's logging provider.                            | Confirm startup registration and provider integration.                               |
| AC-02 | Log events contain `AppName`, `Environment`, `Version`, and `MachineName`.                             | Inspect emitted structured log properties.                                           |
| AC-03 | Correlation middleware resolves a correlation identifier and returns it in `X-Correlation-ID`.         | Test requests with and without valid incoming identifiers.                           |
| AC-04 | Request logs contain `CorrelationId` and the resolved `UserId` when the relevant context is available. | Inspect logs for authenticated and anonymous requests.                               |
| AC-05 | `LoggingBehavior` records request identity without serializing the full request object.                | Inspect emitted log events.                                                          |
| AC-06 | `PerformanceBehavior` emits warnings when execution exceeds the configured threshold.                  | Exercise operations below and above the threshold.                                   |
| AC-07 | Integration-event handlers restore correlation identifiers when the metadata is present.               | Process an event containing a known correlation identifier and inspect handler logs. |
| AC-08 | `IAppContext` is implemented in Infrastructure and is injectable into Application consumers.           | Verify dependency-injection registration and property behavior.                      |
| AC-09 | Console and rolling-file sinks are configured with the intended settings.                              | Verify emitted output, rollover, retention, and file-size behavior.                  |
| AC-10 | Sensitive data is not exposed through automatic logging.                                               | Review logging code and inspect representative log output.                           |
| AC-11 | Logging and performance instrumentation preserve normal exception propagation.                         | Exercise a failing handler and verify the original exception propagates.             |
| AC-12 | Logging-sink failures are handled according to the configured failure policy.                          | Exercise or simulate a sink failure and verify application behavior.                 |
| AC-13 | Execution metadata access remains safe when no HTTP request is active.                                 | Exercise the relevant `IAppContext` properties from a background execution path.     |

---

## 8. Summary

The Logging capability is a foundational cross-cutting concern that enables structured, correlated, and security-conscious logging throughout the E-Commerce modular monolith.

It uses Serilog as the logging implementation, MediatR behaviors for operation identification and slow-operation monitoring, API middleware for request correlation, and an `IAppContext` abstraction for execution metadata.

Infrastructure owns provider configuration and sink integration, while Application remains independent of Serilog-specific APIs and Domain remains independent of logging infrastructure.

The requirements establish a focused logging capability that supports troubleshooting, operational visibility, and future log aggregation while preserving architectural boundaries and avoiding unnecessary coupling.

**Implementation principle:** Logging should improve the ability to understand system behavior without becoming a dependency that compromises business correctness or application reliability.
