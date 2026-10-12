# Tracing — Technical Reference Document

**Project:** E-Commerce Modular Monolith
**Capability:** Cross-Cutting Observability — OpenTelemetry Tracing
**Version:** 1.0 — Final

---

## 1. Overview

Tracing provides execution visibility across HTTP requests, application operations, background jobs, and instrumented dependencies. It helps identify where time is spent, which operations participate in a request, and where failures occur.

This capability uses **OpenTelemetry** as the standard telemetry layer, combining automatic instrumentation with optional Application-level abstractions for custom spans.

The architecture follows these principles:

* **Infrastructure** owns OpenTelemetry SDK configuration, the shared `ActivitySource`, and telemetry export.
* **Application** defines tracing abstractions and uses them where additional diagnostic context is valuable.
* **Domain** remains completely tracing-agnostic.
* Automatic instrumentation handles supported execution paths without requiring custom spans everywhere.
* Tracing failures must not interfere with business operations under normal exporter or collector failure conditions.
* Sensitive information and detailed exception messages must not be exposed through span attributes or descriptions.

Tracing complements logging and metrics; it does not replace either.

---

## 2. Architecture

### 2.1 HTTP Request Execution

```text
HTTP Request
     |
     v
ASP.NET Core Instrumentation
     |
     v
MediatR Pipeline
     |
     v
TracingBehavior
     |
     v
Application Handler / Service
     |
     +---- Optional ITraceContext child spans
     |
     v
Infrastructure Dependencies
     |
     v
OpenTelemetry SDK
     |
     v
OTLP Exporter
     |
     v
Observability Backend
```

ASP.NET Core instrumentation creates HTTP server spans. `TracingBehavior` creates application-operation spans for MediatR requests. Custom spans can provide additional visibility into diagnostically significant operations.

Database and external-dependency spans are available when the corresponding instrumentation is configured.

### 2.2 Background Job Execution

Background jobs are traced through the existing `JobOrchestrator`, which creates a span around each job execution.

```text
Hangfire Job Execution
     |
     v
JobOrchestrator
     |
     v
Create Job Span
     |
     v
Execution Policies
     |
     v
Job Handler
     |
     v
Set Outcome and Dispose Span
```

A background job does not automatically continue an originating HTTP trace merely because both use the same `ActivitySource`. Continuing the original trace requires explicit trace-context propagation through the job scheduling and dispatching flow. Otherwise, the job starts a new trace.

---

## 3. Folder Structure

```text
Application/
├── Shared/
│   ├── Observability/
│   │   └── Tracing/
│   │       ├── ITraceContext.cs
│   │       └── ITraceSpan.cs
│   │
│   └── Behaviors/
│       └── TracingBehavior.cs

Infrastructure/
└── Observability/
    └── Tracing/
        ├── ECommerceActivitySource.cs
        ├── OpenTelemetryTraceContext.cs
        ├── OpenTelemetrySpan.cs
        ├── NullSpan.cs
        └── TracingRegistrationExtensions.cs
```

### 3.1 Component Responsibilities

| Component                          | Layer          | Responsibility                                                                               |
| ---------------------------------- | -------------- | -------------------------------------------------------------------------------------------- |
| `ITraceContext.cs`                 | Application    | Defines the abstraction for starting spans and accessing current trace and span identifiers. |
| `ITraceSpan.cs`                    | Application    | Defines the abstraction for adding attributes, setting span status, and disposing spans.     |
| `TracingBehavior.cs`               | Application    | Creates spans for MediatR requests and records execution outcomes.                           |
| `ECommerceActivitySource.cs`       | Infrastructure | Defines the shared application `ActivitySource`.                                             |
| `OpenTelemetryTraceContext.cs`     | Infrastructure | Implements `ITraceContext` using `System.Diagnostics.Activity`.                              |
| `OpenTelemetrySpan.cs`             | Infrastructure | Wraps an `Activity` behind `ITraceSpan`.                                                     |
| `NullSpan.cs`                      | Infrastructure | Provides a no-op span implementation when no activity is created.                            |
| `TracingRegistrationExtensions.cs` | Infrastructure | Registers OpenTelemetry tracing, ASP.NET Core instrumentation, and the OTLP exporter.        |

---

## 4. Component Details

### 4.1 ECommerceActivitySource

**Layer:** Infrastructure

**Purpose:** Provides a named `ActivitySource` for custom application tracing.

**Implementation:**

* Exposes a static `ActivitySource` named `ECommerce.ModularMonolith`.
* Uses instrumentation version `1.0.0`.
* Provides the shared source used by custom spans created through `ITraceContext`.

**Lifetime:** Static, application-wide instance.

Registering this source with OpenTelemetry allows the SDK to collect activities created through it. Merely registering the source does not create spans; application code must start activities when needed.

### 4.2 ITraceContext and ITraceSpan

**Layer:** Application

**Purpose:** Allow application code to create custom spans without depending directly on the OpenTelemetry SDK.

#### ITraceContext

```csharp
ITraceSpan StartSpan(
    string name,
    IDictionary<string, string?>? attributes = null);

string? CurrentTraceId { get; }

string? CurrentSpanId { get; }
```

Responsibilities:

* Start a custom span.
* Apply optional initial attributes.
* Expose the current trace and span identifiers when available.

#### ITraceSpan

```csharp
void AddAttribute(string key, string? value);

void SetStatus(bool isError, string? description = null);

void Dispose();
```

Responsibilities:

* Add diagnostic attributes.
* Record the operation's outcome.
* End the span through disposal.

These abstractions are optional. Automatic instrumentation and the MediatR behavior cover common execution paths; custom spans should be added only when they provide meaningful diagnostic value.

The `SetStatus` contract must remain consistent across implementations:

* `isError: false` represents a non-error outcome.
* `isError: true` represents an error outcome.
* `description` is optional and must not contain sensitive information.

Successful spans generally do not need an explicit success status because the OpenTelemetry status can remain unset.

### 4.3 OpenTelemetryTraceContext

**Layer:** Infrastructure

**Purpose:** Implements `ITraceContext` using `System.Diagnostics.Activity`.

**Behavior:**

1. Calls `ECommerceActivitySource.Instance.StartActivity(name, ActivityKind.Internal)`.
2. Applies supplied attributes when an activity is created.
3. Returns an `OpenTelemetrySpan` when `StartActivity` returns an activity.
4. Returns `NullSpan.Instance` when no activity is created.
5. Reads `CurrentTraceId` and `CurrentSpanId` from `Activity.Current`.

`ActivitySource.StartActivity` may return `null` when no listener is interested in the activity. The no-op implementation allows callers to use the abstraction without requiring special handling.

The implementation should document how it handles the absence of an active parent activity and whether it relies on the default `ActivitySource` parent-selection behavior.

### 4.4 OpenTelemetrySpan

**Layer:** Infrastructure

**Purpose:** Wraps an `Activity` behind `ITraceSpan`.

**Behavior:**

* `AddAttribute` calls `Activity.SetTag`.
* `SetStatus` maps the abstraction's error flag to the appropriate `ActivityStatusCode`.
* `Dispose` disposes the underlying activity.

Disposing the activity ends the span's lifetime. Callers should use `using` or an equivalent `try/finally` pattern to ensure spans are disposed on both success and failure.

### 4.5 NullSpan

**Layer:** Infrastructure

**Purpose:** Provides a no-op implementation of `ITraceSpan` when no activity is created.

**Behavior:**

* `AddAttribute` does nothing.
* `SetStatus` does nothing.
* `Dispose` does nothing.

This keeps tracing optional for application operations and avoids unnecessary conditional logic in callers.

### 4.6 TracingBehavior

**Layer:** Application — MediatR pipeline

**Purpose:** Automatically create an application-operation span for each MediatR request passing through the behavior.

**Behavior:**

1. Starts a span named `mediatr.{RequestName}`.
2. Adds a `request.type` attribute.
3. Executes the next MediatR pipeline delegate.
4. Leaves the span without an explicit success status when execution completes normally, unless the implementation has a specific reason to set one.
5. Records an error status when an exception terminates the pipeline.
6. Rethrows the exception without suppressing or replacing it.
7. Disposes the span regardless of the outcome.

**Security requirement:** Failure descriptions must be generic. Detailed exception messages remain in logs and must not be copied into span status descriptions.

The behavior's exception-based outcome handling should be understood explicitly: a request that returns a failure `Result` without throwing an exception is still considered a normally completed pipeline unless the implementation separately interprets that result.

### 4.7 TracingRegistrationExtensions

**Layer:** Infrastructure

**Purpose:** Configure and register the OpenTelemetry tracing pipeline.

**Responsibilities:**

* Register `ITraceContext` with the appropriate dependency-injection lifetime, such as scoped.
* Configure OpenTelemetry tracing through `AddOpenTelemetry().WithTracing(...)`.
* Register `ECommerceActivitySource.Instance.Name` using `AddSource`.
* Enable ASP.NET Core instrumentation.
* Register the OTLP exporter.

The configuration must use the same source name as `ECommerceActivitySource`. If database, HTTP-client, or other dependency instrumentation is required, the relevant instrumentation must also be configured.

OTLP exporter configuration should be supplied through environment variables or application configuration rather than hardcoded endpoint values.

### 4.8 JobOrchestrator Integration

**Layer:** Application — Scheduling module

**Purpose:** Trace background job execution through the shared job execution pipeline.

**Behavior:**

1. Injects `ITraceContext`.
2. Starts a span at the beginning of `ExecuteAsync`, named `job.{JobType}`.
3. Adds diagnostic attributes such as `job.type` and `job.id`.
4. Executes the existing job pipeline, including applicable policies and the job handler.
5. Records the outcome as success, failure, or cancellation, according to the implementation's outcome semantics.
6. Disposes the span after execution completes.

**Security requirement:** Failure descriptions must be generic. Detailed exception information remains in logging.

The span should cover the execution attempt handled by the orchestrator. If Hangfire retries the job, each execution attempt may produce a separate span. A logical job's final outcome should not be confused with an individual attempt's outcome.

Cancellation should be recorded consistently with the chosen telemetry convention. Cancellation does not automatically mean the span should be marked as an error; the decision should reflect whether the operation failed or was intentionally cancelled.

---

## 5. Data Flow

### 5.1 HTTP Request

```text
Client Request
     |
     v
ASP.NET Core Instrumentation
     |
     v
HTTP Server Span
     |
     v
MediatR Request
     |
     v
TracingBehavior
     |
     v
Application-Operation Span
     |
     v
Application Handler
     |
     +---- Optional custom spans
     |
     v
Instrumented Database / External Dependencies
```

Correct parent-child relationships depend on the active `Activity` context and the instrumentation involved.

### 5.2 Background Job

```text
Hangfire Executes Job
     |
     v
JobOrchestrator
     |
     v
Create Job Span
     |
     v
Execution Policies and Handler
     |
     v
Record Outcome
     |
     v
Dispose Span
```

If the job must continue a trace originating from another operation, the trace context must be explicitly captured, persisted, and restored when the job executes. Without this propagation, the job can still be traced, but it will start a separate trace.

### 5.3 Custom Application Span

```text
Application Handler
     |
     v
Inject ITraceContext
     |
     v
Start Custom Span
     |
     v
Add Relevant Attributes
     |
     v
Execute Operation
     |
     v
Record Error Status When Appropriate
     |
     v
Dispose Span
```

Custom spans should represent meaningful operations rather than duplicating spans already provided by automatic instrumentation or the MediatR behavior.

---

## 6. Correlation with Logs and Metrics

### 6.1 Logs

Trace and span identifiers can be included in structured logs through Serilog or other logging enrichment mechanisms when an activity is active.

This enables operators to correlate a log entry with the corresponding trace.

The logging configuration must ensure that the identifiers are populated from the active activity context. Tracing and log correlation are related capabilities, but registering tracing alone does not guarantee that every log entry includes trace and span identifiers.

### 6.2 Metrics

Metrics and traces can be correlated through standard telemetry context and exemplars when the instrumentation and backend support them.

The tracing capability does not itself create business or operational metrics. Metric collection remains the responsibility of the metrics capability.

Trace identifiers should not be used as metric labels because their high cardinality can significantly increase metric storage and processing costs.

---

## 7. Export Configuration

OTLP is the primary telemetry export mechanism.

The exporter endpoint can be configured using environment variables or application configuration. For example:

```text
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
```

Common defaults are:

* **4317:** OTLP over gRPC.
* **4318:** OTLP over HTTP using Protocol Buffers.

The actual endpoint and protocol must match the configured exporter and the receiving collector or backend. These values are examples, not mandatory production settings.

The exporter is registered through `TracingRegistrationExtensions`.

**Operational requirements:**

* Do not hardcode production collector endpoints or credentials.
* Configure exporter settings through the deployment environment.
* Verify that the configured endpoint is reachable and accepts the selected protocol.
* Ensure that exporter or collector failures do not unnecessarily fail business operations.
* Use the observability backend's retention and access controls to protect diagnostic data.

---

## 8. Security and Cardinality

Tracing must provide useful diagnostics without exposing sensitive information or creating unnecessary telemetry volume.

### 8.1 Attribute Rules

* Avoid unnecessary high-cardinality attributes.
* Include identifiers such as `CustomerId`, `OrderId`, or `ProductId` only when they provide meaningful diagnostic value.
* Never include passwords, authentication tokens, OTPs, payment credentials, or other secrets.
* Avoid personal data unless its inclusion is justified, permitted, and appropriately protected.
* Prefer stable operation names over dynamically generated names containing entity identifiers.

High-cardinality identifiers can sometimes be useful in traces, but they still have storage, privacy, and access-control implications.

### 8.2 Failure Descriptions

Failure descriptions in spans must remain generic.

For example:

```csharp
span.SetStatus(true, "Operation failed");
```

For background jobs:

```csharp
span.SetStatus(true, "Job execution failed");
```

Detailed exception information should remain in logs, subject to the application's logging and redaction policies.

Generic span descriptions must not replace structured exception logging. Operators should be able to investigate failures using the appropriate logs and correlation identifiers.

### 8.3 Failure Isolation

Tracing is an observability capability, not a prerequisite for executing business operations.

Under normal telemetry-export failure conditions, the application should continue processing business operations rather than failing solely because the observability backend is unavailable. This behavior should be verified in integration testing.

---

## 9. Testing and Verification

The tracing implementation should be verified at the boundaries where it creates, records, and exports telemetry.

Recommended checks include:

* Confirm that the configured `ActivitySource` produces activities when an OpenTelemetry listener is registered.
* Confirm that the MediatR behavior creates an application-operation span.
* Verify that exceptions produce an error status and are rethrown.
* Verify that span disposal occurs on success, failure, and cancellation.
* Verify that custom spans are no-ops when no activity is created.
* Confirm that background job execution produces the expected span and outcome.
* Verify parent-child relationships for HTTP and MediatR spans.
* Test background-job trace propagation separately if continuation of an originating trace is required.
* Confirm that trace attributes and failure descriptions contain no prohibited sensitive information.
* Verify that exporter unavailability does not cause business operations to fail under the tested conditions.

Testing should focus on observable behavior and the configured instrumentation rather than assuming that registration alone guarantees correct trace creation or propagation.

---

## 10. Summary

The tracing capability uses OpenTelemetry to provide execution visibility across HTTP requests, application operations, background jobs, and instrumented dependencies.

ASP.NET Core instrumentation creates HTTP server spans, while the MediatR `TracingBehavior` creates application-operation spans. The existing `JobOrchestrator` traces background-job execution, and optional Application abstractions allow custom spans for diagnostically significant operations.

Infrastructure owns the `ActivitySource`, OpenTelemetry SDK configuration, instrumentation registration, and OTLP export. Application code depends only on the tracing abstractions where custom tracing is required, while the Domain layer remains tracing-agnostic.

The design supports integration with OTLP-compatible observability backends while maintaining clear architectural boundaries, generic span failure descriptions, and separation between tracing and detailed exception logging.

### Required Code Adjustments

Update the exception-handling paths in `TracingBehavior` and `JobOrchestrator` to avoid placing raw exception messages in span status descriptions.

In `TracingBehavior`, replace:

```csharp
span.SetStatus(true, ex.Message);
```

with:

```csharp
span.SetStatus(true, "Operation failed");
```

In `JobOrchestrator`, replace:

```csharp
span.SetStatus(true, ex.Message);
```

with:

```csharp
span.SetStatus(true, "Job execution failed");
```

These changes align the implementation with the security rule: **failure descriptions in spans must be generic; detailed error information remains in logs.**
