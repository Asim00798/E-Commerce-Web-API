# Tracing — Technical Requirements Document

**Project:** E-Commerce Modular Monolith
**Capability:** Cross-Cutting Observability — OpenTelemetry Tracing
**Version:** 1.0 — Final

---

## 1. Introduction

Tracing provides visibility into the execution paths of operations across HTTP requests, application layers, background jobs, and instrumented dependencies. It helps answer questions such as:

* Where did a request travel?
* How long did each operation take?
* Which components participated in the execution?
* Where did a failure occur?

This document defines the technical requirements for the tracing capability, using **OpenTelemetry** as the standard telemetry framework.

The design relies primarily on automatic instrumentation and centralized pipeline integration. Optional Application-level abstractions are provided only when custom spans offer meaningful diagnostic value.

The Domain layer remains completely tracing-agnostic.

## 2. Business Goals

The tracing capability must:

* Provide execution visibility for HTTP requests and background jobs.
* Establish a standard mechanism for creating application-level spans when needed.
* Support correlation between traces and logs through `TraceId` and `SpanId`.
* Enable performance analysis and dependency investigation.
* Keep the Application layer independent of the OpenTelemetry SDK.
* Support replacement of the observability backend without changing business-layer code.

## 3. Scope

### 3.1 In Scope

* OpenTelemetry tracing using `System.Diagnostics.ActivitySource`.
* ASP.NET Core automatic instrumentation for HTTP request tracing.
* MediatR pipeline behavior for application-operation spans.
* Background job tracing through the existing `JobOrchestrator`.
* Optional Application abstractions (`ITraceContext` and `ITraceSpan`) for custom spans.
* OTLP exporter configuration.
* Trace and log correlation through standard activity context.
* Generic span failure descriptions that do not expose detailed exception messages.

### 3.2 Out of Scope

* Metrics collection and instrumentation, which belong to the separate metrics capability.
* Health checks.
* Alerting and incident-management workflows.
* Custom distributed tracing protocols outside the OpenTelemetry ecosystem.
* Manual tracing of every method or operation.
* Unnecessary high-cardinality attributes or indiscriminate capture of identifiers.

## 4. Functional Requirements

### 4.1 ActivitySource

The application must define a named `ActivitySource`, such as `ECommerce.ModularMonolith`.

Requirements:

* The shared `ActivitySource` must be owned by Infrastructure.
* Its name must be consistent with the source registered in the OpenTelemetry tracing configuration.
* Custom spans must be created through `ActivitySource.StartActivity` or the Application-facing tracing abstractions.
* The implementation must handle `StartActivity` returning `null` when no listener is interested.
* The absence of a listener must not require callers to implement special-case business logic.

The `ActivitySource` defines the source for custom activities; registering it with OpenTelemetry does not itself create spans.

### 4.2 Application Tracing Abstractions

Application-level tracing abstractions must be available when custom spans are required.

#### `ITraceContext`

Provides access to the current trace and span identifiers and allows callers to start custom spans.

#### `ITraceSpan`

Provides operations for adding attributes, setting status, and disposing of spans.

Requirements:

* The abstractions must not depend on the OpenTelemetry SDK.
* They must remain optional for common execution paths.
* Custom spans should be used only for operations with meaningful diagnostic value, such as a significant calculation or external dependency interaction.
* The concrete Infrastructure implementation must handle cases where no activity is created.
* Span disposal must occur on success, failure, and cancellation.
* Error descriptions must remain generic and must not expose raw exception messages.

Automatic instrumentation and the MediatR behavior cover common execution paths without requiring custom spans throughout the application.

### 4.3 MediatR Tracing Behavior

A MediatR pipeline behavior named `TracingBehavior` must create an application-operation span for each request passing through the behavior.

Requirements:

* Use a stable operation name, such as `mediatr.{RequestName}`.
* Start the span before invoking the next pipeline delegate.
* Stop the span after execution completes.
* Record relevant attributes, including `request.type`.
* Record an error status when an exception terminates the pipeline.
* Preserve normal exception propagation; tracing must not suppress or replace application exceptions.
* Dispose of the span regardless of the execution outcome.
* Avoid including sensitive information in span attributes or status descriptions.

Unless result-based failures are explicitly interpreted by the behavior, a request that returns a failure `Result` without throwing is considered a normally completed pipeline execution.

### 4.4 JobOrchestrator Tracing

The existing `JobOrchestrator` must create a span for each background-job execution attempt.

Requirements:

* Start a span at the beginning of the job execution pipeline.
* Use a stable name, such as `job.{JobType}`.
* Include appropriate diagnostic attributes, such as `job.type` and `job.id`.
* Record the execution outcome.
* Ensure the span covers the intended execution pipeline, including applicable policies and the handler.
* Dispose of the span on success, failure, and cancellation.
* Use generic failure descriptions rather than raw exception messages.
* Avoid duplicating common tracing logic inside individual job handlers.

Job handlers may create additional child spans when they perform diagnostically significant internal operations.

If a background job must continue an originating trace, the trace context must be explicitly propagated through scheduling and dispatch. Otherwise, the job may start a separate trace.

### 4.5 ASP.NET Core Instrumentation

HTTP request tracing must be provided through ASP.NET Core and OpenTelemetry instrumentation.

Requirements:

* HTTP server spans must be created automatically for instrumented requests.
* Application-operation spans should become children of the active HTTP span when execution occurs within that trace context.
* Standard activity context must be preserved across the normal asynchronous execution flow.
* Trace relationships must not be assumed when context propagation has not been established.

Additional instrumentation, such as database or HTTP-client tracing, must be configured separately when those dependency spans are required.

### 4.6 OTLP Export

Traces must be exportable through the OpenTelemetry Protocol (OTLP) to a compatible observability backend.

Requirements:

* Configure the OTLP exporter through Infrastructure.
* Externalize endpoint and protocol configuration through environment variables or application configuration.
* Keep backend-specific implementation details out of Application and Domain.
* Allow the observability backend to be replaced through configuration and standard OTLP integration, provided the replacement supports the configured protocol and telemetry features.
* Ensure exporter or backend unavailability does not cause business operations to fail under normal telemetry-export failure conditions.

## 5. Non-Functional Requirements

### 5.1 Performance

* Span creation must have low overhead.
* Telemetry export should not introduce unnecessary blocking into request or job execution.
* Automatic tracing must not significantly degrade application performance under expected workloads.
* Sampling and exporter configuration should be appropriate for the deployment environment.

### 5.2 Reliability

* Tracing failures must not cause business operations to fail.
* The application must continue operating if the OTLP exporter or observability backend becomes unavailable.
* Span disposal must be guaranteed across normal and exceptional execution paths.
* Tracing must not interfere with the application's exception handling or job retry policies.

### 5.3 Maintainability

* Common MediatR tracing must be centralized in `TracingBehavior`.
* Background job tracing must be centralized in `JobOrchestrator`.
* Optional custom spans must use the Application-facing abstractions.
* Adding a custom span must not require changes across unrelated architectural layers.
* Concrete telemetry implementations and exporter configuration must remain in Infrastructure.

### 5.4 Cardinality

* Trace attributes must avoid unnecessary high-cardinality data.
* Identifiers such as `CustomerId`, `OrderId`, and `ProductId` may be included only when there is a clear diagnostic purpose and the associated privacy and security implications have been considered.
* Operation names must remain stable and must not embed entity identifiers or other unbounded values.
* Trace identifiers must not be used as metric labels.

### 5.5 Security and Privacy

* Traces must not expose passwords, authentication tokens, OTPs, payment credentials, or other secrets.
* Personal information must not be included unless its use is justified, permitted, and appropriately protected.
* Span attributes must be reviewed before being introduced.
* Failure descriptions must be generic.
* Detailed exception information must remain in logs, subject to the application's logging and redaction policies.
* Access to exported telemetry and retention must follow the deployment's security requirements.

## 6. Architectural Constraints

The tracing implementation must comply with the modular-monolith architecture:

* **Domain:** Must not depend on OpenTelemetry, `ActivitySource`, or tracing abstractions.
* **Application:** May depend on `ITraceContext` and `ITraceSpan`; must not depend on the OpenTelemetry SDK.
* **Infrastructure:** Owns the shared `ActivitySource`, OpenTelemetry configuration, exporters, and concrete span implementations.
* **MediatR pipeline:** `TracingBehavior` centralizes application-operation tracing and does not replace logging or metrics behaviors.
* **Scheduling:** Background-job tracing is integrated through `JobOrchestrator`, rather than requiring individual handlers to depend on Hangfire-specific tracing APIs.
* **Observability:** Tracing complements logging and metrics without taking ownership of their responsibilities.
* **Failure handling:** Telemetry failure must not be allowed to alter business outcomes solely because observability infrastructure is unavailable.

## 7. Acceptance Criteria

The tracing capability is acceptable when the following criteria are satisfied:

1. A named `ActivitySource` is defined and owned by Infrastructure.
2. The OpenTelemetry configuration registers the correct activity source.
3. `TracingBehavior` creates and completes spans for MediatR requests.
4. `TracingBehavior` records exception-based failures without exposing raw exception messages in span descriptions.
5. `JobOrchestrator` creates and completes spans for background-job execution attempts.
6. Job spans include appropriate diagnostic attributes and execution outcomes.
7. ASP.NET Core instrumentation creates HTTP request spans automatically.
8. HTTP and application-operation spans have the expected parent-child relationships when they share an active trace context.
9. The OTLP exporter is configured and can export traces to a compatible endpoint.
10. Endpoint and protocol settings can be supplied through external configuration.
11. `ITraceContext` and `ITraceSpan` are available and implemented when Application-level custom spans are required.
12. Spans are disposed on success, failure, and cancellation.
13. No unnecessary high-cardinality attributes or prohibited sensitive information are included.
14. The Domain layer remains free of tracing dependencies.
15. Tracing failures do not cause business operations to fail under the tested exporter and backend failure conditions.
16. The implementation remains within the Application and Infrastructure layers, with no tracing-specific business logic introduced into Domain.

## 8. Summary

The tracing capability provides execution visibility using OpenTelemetry while preserving the architectural boundaries of the E-Commerce modular monolith.

Automatic ASP.NET Core instrumentation handles HTTP request spans, `TracingBehavior` centralizes MediatR operation spans, and the existing `JobOrchestrator` traces background-job execution. Optional Application abstractions support custom spans when additional diagnostic detail is justified.

Infrastructure owns the `ActivitySource`, OpenTelemetry configuration, concrete span implementations, and OTLP export. Application code remains decoupled from the OpenTelemetry SDK, and Domain remains tracing-agnostic.

The resulting capability supports performance analysis, failure investigation, and trace-log correlation while controlling telemetry overhead, protecting sensitive information, and isolating business operations from observability infrastructure failures.
