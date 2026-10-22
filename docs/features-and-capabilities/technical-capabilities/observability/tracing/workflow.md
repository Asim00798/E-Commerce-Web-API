# Tracing — Workflow Technical Document

**Project:** E-Commerce Modular Monolith
**Capability:** Cross-Cutting Observability — OpenTelemetry Tracing
**Version:** 1.0 — Final

---

## 1. Overview

This document describes the end-to-end workflows for tracing within the E-Commerce modular monolith.

It covers:

* HTTP request tracing through ASP.NET Core instrumentation.
* Application-operation tracing through the MediatR `TracingBehavior`.
* Background-job tracing through the existing `JobOrchestrator`.
* Optional custom spans through `ITraceContext` and `ITraceSpan`.
* Export of trace data through the OpenTelemetry Protocol (OTLP).
* Correlation between traces and logs through standard trace context.

Tracing complements logging and metrics. It uses standard `TraceId` and `SpanId` context to support correlation across observability signals without taking ownership of logging or metric collection.

The Domain layer remains tracing-agnostic, and Application code does not depend on the OpenTelemetry SDK.

## 2. Actors and Components

### 2.1 Actors

| Actor               | Description                                                                               |
| ------------------- | ----------------------------------------------------------------------------------------- |
| Client              | Sends HTTP requests to the API.                                                           |
| Application Handler | Executes application operations, including commands and queries.                          |
| Background Job      | Executes asynchronous work through the scheduling infrastructure.                         |
| Operations Team     | Uses trace data to investigate failures, analyze latency, and understand execution paths. |

### 2.2 Key Components

| Component                       | Layer                                                    | Responsibility                                                             |
| ------------------------------- | -------------------------------------------------------- | -------------------------------------------------------------------------- |
| ASP.NET Core Instrumentation    | Infrastructure configuration / framework instrumentation | Automatically creates HTTP server spans for incoming requests.             |
| `TracingBehavior`               | Application                                              | Creates spans for MediatR operations and records execution outcomes.       |
| `JobOrchestrator`               | Application                                              | Creates spans around background-job execution attempts.                    |
| `ITraceContext` / `ITraceSpan`  | Application                                              | Optional abstractions for creating and managing custom spans.              |
| `ECommerceActivitySource`       | Infrastructure                                           | Provides the shared `ActivitySource` for custom application and job spans. |
| `OpenTelemetryTraceContext`     | Infrastructure                                           | Implements `ITraceContext` using `System.Diagnostics.Activity`.            |
| `TracingRegistrationExtensions` | Infrastructure                                           | Registers OpenTelemetry tracing, instrumentation, and the OTLP exporter.   |
| OTLP Exporter                   | Infrastructure configuration                             | Exports collected trace data to a compatible observability backend.        |

## 3. Workflow 1: HTTP Request Tracing

### 3.1 Description

Incoming HTTP requests are traced automatically through ASP.NET Core instrumentation when the tracing pipeline is configured.

The HTTP server span represents the incoming request. Application-operation spans created during request processing become its descendants when they execute within the same active trace context.

### 3.2 Flow

```text
Client Request
    |
    v
ASP.NET Core Instrumentation
    |
    +---- Creates HTTP server span
    |
    +---- Captures supported HTTP attributes
    |     (method, route, status, duration)
    |
    +---- Establishes the current activity context
    |
    v
Application Pipeline
    |
    v
Controller / Endpoint
    |
    v
MediatR Request
```

The exact attributes captured depend on the instrumentation version and configuration.

### 3.3 Important Rules

* HTTP tracing is automatic; application code does not need to create a duplicate HTTP span.
* HTTP server spans are created by ASP.NET Core instrumentation, not by the application's custom `ActivitySource`.
* The HTTP span acts as the parent of downstream MediatR and custom spans when they are created within the active request trace.
* Correct parent-child relationships depend on activity context being preserved throughout asynchronous execution.
* Additional dependency spans require the corresponding instrumentation to be configured.

## 4. Workflow 2: MediatR Operation Tracing

### 4.1 Description

For each MediatR command or query that passes through the `TracingBehavior`, the behavior creates an application-operation span.

The span surrounds execution of the remaining MediatR pipeline, including the handler and any downstream behaviors.

### 4.2 Flow

```text
MediatR Request
    |
    v
TracingBehavior
    |
    +---- Start span: mediatr.{RequestName}
    |
    +---- Add attribute: request.type
    |
    +---- Execute remaining pipeline and handler
    |
    +---- Normal completion
    |          |
    |          v
    |     Leave status unset or apply the
    |     explicitly defined success convention
    |
    +---- Exception
               |
               v
          Set Error status with
          generic description
               |
               v
          Rethrow exception
    |
    v
Dispose span
    |
    v
OpenTelemetry SDK processes the completed span
```

The span is exported according to the configured OpenTelemetry processing and exporter pipeline; disposal alone does not guarantee immediate delivery to the backend.

### 4.3 Important Rules

* Span names should use stable request type names, such as `mediatr.GetProductQuery`.
* Request names must not include customer-specific values, entity identifiers, or other unbounded data.
* The `request.type` attribute should identify the request type without including request payload data.
* Failure status must use a generic description, such as `Operation failed`, rather than the exception message.
* Exceptions must continue through the existing exception-handling pipeline.
* The behavior must dispose of the span on both normal and exceptional execution paths.
* The behavior must not duplicate HTTP tracing; it provides application-level detail within the request trace.

**Outcome semantics:** Unless the behavior explicitly inspects a returned `Result`, a request that returns a failure result without throwing is considered a normally completed pipeline execution.

## 5. Workflow 3: Background Job Tracing

### 5.1 Description

Each background-job execution attempt passing through the `JobOrchestrator` is traced through a span created by the shared Application-level execution pipeline.

The span should begin before handler resolution and policy execution so that it covers the intended job execution lifecycle.

### 5.2 Flow

```text
Hangfire Executes Job
    |
    v
JobOrchestrator
    |
    +---- Start span: job.{JobType}
    |
    +---- Add attributes:
    |       job.type
    |       job.id
    |
    +---- Resolve handler
    |
    +---- Apply execution policies
    |
    +---- Execute handler
    |
    +---- Record outcome
    |       |
    |       +---- Success
    |       |
    |       +---- Failure
    |       |
    |       +---- Cancellation
    |
    v
Dispose span
    |
    v
OpenTelemetry SDK processes the completed span
```

### 5.3 Important Rules

* The span covers the execution pipeline managed by `JobOrchestrator`, including handler resolution and applicable policies when those operations occur inside the span's lifetime.
* Individual job handlers must not duplicate the common job-execution span.
* Handlers may create custom child spans for meaningful internal operations.
* Failure descriptions must be generic, such as `Job execution failed`.
* Detailed exception information remains in logs, subject to the application's logging and redaction rules.
* Each retry may produce a separate span because each retry represents another execution attempt.
* An attempt's outcome must not be confused with the final outcome of the logical job.

**Cancellation semantics:** Cancellation must be represented consistently with the implementation's telemetry convention. An intentional cancellation does not necessarily constitute an error. If cancellation is recorded as an error, use a generic description such as `Job cancelled` and ensure this reflects the intended operational meaning.

### 5.4 Trace Context Propagation

A background job does not automatically become a child of the HTTP request that scheduled it.

If the system needs to continue the originating trace, it must capture and propagate the appropriate trace context through the scheduling and dispatching flow.

Otherwise, the job can start an independent trace. This still provides useful visibility into job execution, but it does not establish an end-to-end parent-child relationship with the originating request.

## 6. Workflow 4: Optional Custom Spans

### 6.1 Description

Application code may create custom spans when a specific operation has meaningful diagnostic value, such as a significant shipping-fee calculation or an external service interaction.

Custom spans should supplement automatic instrumentation and centralized pipeline tracing rather than duplicate them.

### 6.2 Flow

```text
Application Handler
    |
    v
Inject ITraceContext
    |
    v
Start Custom Span
    |
    +---- Add relevant attributes
    |
    v
Execute Operation
    |
    +---- Normal completion
    |          |
    |          v
    |     Leave status unset or apply
    |     the defined success convention
    |
    +---- Exception
               |
               v
          Set Error status with
          generic description
               |
               v
          Rethrow exception
    |
    v
Dispose span
```

### 6.3 Implementation Rules

* Use `ITraceContext` to start custom spans.
* Use `ITraceSpan` to add attributes and set status.
* Use `using` or an equivalent `try/finally` pattern to ensure disposal.
* Preserve the original exception and existing error-handling behavior.
* Avoid tracing trivial methods or creating redundant nested spans.
* Do not include secrets, personal information, or unnecessary high-cardinality identifiers.
* Follow the actual namespace and dependency-injection registration for the tracing abstractions.

Custom spans should represent operations that help diagnose performance, dependencies, or failures.

## 7. Trace Context and Log Correlation

### 7.1 Current Activity

`System.Diagnostics.Activity.Current` exposes the currently active activity when one exists.

```text
Activity.Current
    |
    +---- TraceId
    |
    +---- SpanId
             |
             v
     Optional Log Enrichment
             |
             v
         Log Events
```

The trace identifier identifies the overall trace, while the span identifier identifies a specific operation within it.

### 7.2 Logging Integration

Serilog or another logging implementation can enrich log events with trace and span identifiers when the appropriate enrichment is configured.

Requirements:

* Correlation must use the active trace context.
* Logging must remain functional when no activity is active.
* Tracing must not replace structured exception logging.
* Sensitive information must not be exposed merely because a log event is correlated with a trace.

Trace-log correlation is configurable; enabling OpenTelemetry tracing alone does not guarantee that every log entry contains trace and span identifiers.

### 7.3 Metrics Integration

Metrics can be correlated with trace context when the relevant instrumentation and observability backend support it.

The tracing capability does not create metrics. Metric instruments and export remain the responsibility of the separate metrics capability.

Trace and span identifiers must not be used as metric labels because doing so introduces excessive metric cardinality.

## 8. Important Rules Summary

* **HTTP requests:** Traced automatically through ASP.NET Core instrumentation.
* **MediatR operations:** Traced through `TracingBehavior`.
* **Background jobs:** Traced through `JobOrchestrator`.
* **Custom spans:** Optional; created through `ITraceContext` and managed through `ITraceSpan`.
* **Failure descriptions:** Generic; detailed exception information remains in logs.
* **Span lifecycle:** Spans must be disposed on success, failure, and cancellation.
* **Trace propagation:** Background jobs require explicit propagation if they must continue an originating trace.
* **Attributes:** Must not expose sensitive data or unnecessary high-cardinality values.
* **Failure isolation:** Tracing or exporter failures must not cause business operations to fail under the tested conditions.
* **Instrumentation:** Database and other dependency spans require the corresponding instrumentation.
* **Observability separation:** Tracing complements logging and metrics without replacing them.

## 9. Summary

The tracing workflows provide execution visibility across HTTP requests, application operations, background jobs, and optional custom spans.

ASP.NET Core instrumentation handles incoming HTTP spans, `TracingBehavior` provides application-operation spans, and `JobOrchestrator` traces background-job execution attempts. Optional Application abstractions support targeted custom spans where additional diagnostic detail is valuable.

Infrastructure owns the shared `ActivitySource`, OpenTelemetry configuration, concrete span implementations, and OTLP export. Application remains decoupled from the OpenTelemetry SDK, and Domain remains tracing-agnostic.

This workflow supports performance analysis, failure investigation, and trace-log correlation while preserving architectural boundaries, controlling telemetry data, and isolating business execution from observability infrastructure failures.
