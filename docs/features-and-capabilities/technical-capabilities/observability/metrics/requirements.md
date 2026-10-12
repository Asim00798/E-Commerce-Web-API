# Metrics — Technical Requirements Document

**Project:** E-Commerce Modular Monolith
**Capability:** Cross-Cutting Observability — OpenTelemetry Metrics
**Version:** 1.0 — Final

---

## 1. Introduction

Metrics provide aggregated numerical measurements of system behavior and business activity. They answer operational questions such as:

* How many orders were created?
* How long do application operations take?
* How many operations succeed or fail?
* How many background jobs succeed or fail?
* How long do background jobs take to execute?

This document defines the functional, non-functional, and architectural requirements for the Metrics capability.

The capability uses the **OpenTelemetry Metrics API**, including `System.Diagnostics.Metrics.Meter`, as the standard telemetry interface. It combines typed metric classes for business measurements with automatic instrumentation for common application operations.

The Domain layer remains completely metrics-agnostic.

## 2. Business Goals

The Metrics capability must:

* Provide operational insight into application throughput, latency, and failure rates.
* Establish consistent, predefined business metrics without scattering metric definitions throughout application handlers.
* Capture common application-operation telemetry automatically with minimal developer effort.
* Support standard observability backends through the OpenTelemetry Protocol (OTLP).
* Prevent uncontrolled metric-dimension cardinality that could degrade performance or increase storage and processing costs.
* Measure background-job execution centrally rather than requiring individual job handlers to implement duplicate instrumentation.

## 3. Scope

### 3.1 In Scope

The Metrics capability includes:

* A shared application `Meter` as the source of application-defined instruments.
* Typed metric classes for predefined business and application measurements, including:

  * `OrderMetrics` — order creation and processing duration.
* A MediatR pipeline behavior, `TelemetryBehavior`, that records:

  * Operation duration.
  * Operation count.
  * Operation outcome.
* Automatic HTTP request metrics through ASP.NET Core and OpenTelemetry instrumentation.
* Background-job execution metrics recorded through the existing `JobOrchestrator`.
* OpenTelemetry SDK configuration and OTLP exporter registration.
* Infrastructure registration extensions for dependency injection.

### 3.2 Out of Scope

The following are explicitly excluded from the current implementation:

* A generic `IMetricRecorder` abstraction; typed metric classes are preferred.
* Metric-based alerting rules and dashboards.
* Distributed tracing, which is a separate implementation phase.
* Health checks, which are a separate capability.
* Business notifications.
* High-cardinality metric dimensions, such as customer IDs, order IDs, and email addresses.
* A custom metrics collection system outside OpenTelemetry.

## 4. Functional Requirements

### 4.1 Shared OpenTelemetry Meter

**FR-01 — Shared Meter**

The application must use a single named `Meter` instance as the source for application-defined instruments.

**FR-02 — Singleton Lifetime**

The shared meter must be created once and remain available for the lifetime of the application process.

**FR-03 — Instrument Reuse**

Typed metric classes and application-operation instrumentation must use the shared meter rather than creating unrelated meter instances for the same application instrumentation source.

**FR-04 — Stable Identity**

The meter must have a stable name and instrumentation version so the OpenTelemetry SDK can subscribe to the correct instrumentation source.

### 4.2 Typed Metric Classes

**FR-05 — Typed Business Metrics**

Business-specific measurements must be encapsulated in typed metric classes, such as `OrderMetrics`.

**FR-06 — Instrument Initialization**

Each typed metric class must create its instruments once during construction using the shared meter.

**FR-07 — Predefined Instruments**

Instrument names and types must be predefined. Instruments must not be created dynamically from arbitrary runtime values.

**FR-08 — Singleton Registration**

Typed metric classes that retain reusable instruments must be registered as singletons, provided they contain no request-specific mutable state.

**FR-09 — Initial Order Instruments**

The initial implementation must include the following instruments:

| Metric                       | Instrument          | Requirement                                                     |
| ---------------------------- | ------------------- | --------------------------------------------------------------- |
| `orders.created`             | `Counter<long>`     | Count successfully created orders.                              |
| `orders.processing.duration` | `Histogram<double>` | Measure the duration of the defined order-processing operation. |

**FR-10 — Business Measurement Accuracy**

Business metrics must be recorded at the point where the event they represent has succeeded according to the application's defined business and persistence boundaries.

Metrics are observational measurements, not authoritative business records. The implementation must not imply that an in-memory metric is durably recorded or guaranteed to be exported.

### 4.3 Application Operation Metrics

**FR-11 — MediatR Instrumentation**

A MediatR pipeline behavior, `TelemetryBehavior`, must automatically record metrics for MediatR requests passing through the registered pipeline.

**FR-12 — Required Measurements**

The behavior must record:

* Operation count.
* Operation duration.
* Operation outcome.

**FR-13 — Required Dimensions**

The behavior must include:

* `operation` — the stable name of the MediatR request type.
* `outcome` — a bounded value identifying the operation's outcome.

**FR-14 — Duration Unit**

Operation duration must be measured consistently. If the implementation uses milliseconds, the metric documentation and instrument configuration must identify that unit.

**FR-15 — Outcome Semantics**

An operation is considered successful when the measured pipeline completes normally. An operation is considered failed when the measured pipeline terminates with an exception.

A business-level failure represented by a returned `Result` object without an exception is therefore counted as a successful pipeline execution under this requirement.

If business-level failures must also be measured as operational failures, that behavior must be explicitly introduced as a separate requirement rather than silently changing the meaning of the existing metric.

**FR-16 — No Duplicate HTTP Instrumentation**

`TelemetryBehavior` must measure MediatR operation execution without manually recording HTTP request metrics already provided by ASP.NET Core instrumentation.

**FR-17 — No Tracing Responsibilities**

`TelemetryBehavior` must not create or manage tracing spans as part of this phase.

**FR-18 — Bounded Operation Names**

The `operation` dimension must represent a bounded set of known request types. It must not contain arbitrary user-controlled values or entity identifiers.

### 4.4 Background-Job Metrics

**FR-19 — Centralized Job Instrumentation**

The existing `JobOrchestrator` must record metrics for background-job executions.

**FR-20 — Required Measurements**

The job instrumentation must record:

* Job execution count.
* Job execution duration.
* Job execution outcome.

**FR-21 — Required Dimensions**

Job metrics may include a stable job type or name and a bounded outcome value, such as `success` or `failure`.

Job names must come from the application's known job definitions rather than arbitrary input.

**FR-22 — Centralized Responsibility**

Job handlers must not manually duplicate generic execution metrics already recorded by the `JobOrchestrator`. They may record additional business-specific measurements through appropriate typed metric classes.

**FR-23 — Consistent Execution Semantics**

The implementation must define what constitutes a job execution and how success and failure are determined. The measured boundary must be consistent with the existing job execution pipeline and must not introduce a second retry mechanism.

### 4.5 HTTP Request Metrics

**FR-24 — Automatic HTTP Instrumentation**

HTTP request metrics must be collected through ASP.NET Core and OpenTelemetry instrumentation.

**FR-25 — No Manual HTTP Metrics**

Application developers must not manually instrument ordinary HTTP requests when the required measurements are already provided by the configured HTTP instrumentation.

**FR-26 — Relevant HTTP Measurements**

The configured instrumentation must expose the supported HTTP measurements needed for operational monitoring, including request duration and request activity or counts, together with applicable route and response information.

The exact metric names and available attributes depend on the installed instrumentation package versions and semantic conventions. The implementation must verify the actual emitted measurements rather than assume a particular set of names.

### 4.6 OTLP Export

**FR-27 — OTLP Export**

The Metrics capability must configure an OTLP exporter to send collected metrics to a compatible observability backend or collector.

**FR-28 — Externalized Configuration**

Exporter endpoints and environment-specific settings must be configurable outside application source code, using application configuration, environment variables, or an appropriate secret-management mechanism.

**FR-29 — Replaceable Backend**

Changing the observability backend must not require changes to business logic or typed metric definitions, provided the replacement supports the configured export protocol and measurement requirements.

**FR-30 — Explicit Configuration Binding**

Any custom application configuration used for exporter settings must be explicitly read and applied by the OpenTelemetry registration code. Configuration values must not be assumed to take effect merely because they exist in `appsettings.json`.

## 5. Non-Functional Requirements

### 5.1 Performance

**NFR-01 — Low Recording Overhead**

Metric recording must have minimal overhead and must not perform blocking network calls in application request or job execution paths.

**NFR-02 — Bounded Resource Usage**

The metrics implementation must avoid unnecessary instrument creation, excessive cardinality, and unbounded in-memory telemetry accumulation.

**NFR-03 — No Material Latency Impact**

Counter and histogram recording should not materially increase request or job execution latency under expected operating conditions.

### 5.2 Reliability

**NFR-04 — Business-Operation Isolation**

An ordinary metrics collection or export failure must not cause a business operation to fail.

**NFR-05 — Backend Unavailability**

The application must continue processing business operations when the observability backend is unavailable, subject to the application's overall resource and infrastructure health.

**NFR-06 — Telemetry Loss Expectations**

The implementation must not treat in-memory metrics as durable records. Temporary exporter failures, process termination, or buffering limits may result in delayed or lost telemetry.

If a business requirement needs durable evidence of an event, it must use an appropriate persistent business record or durable event mechanism rather than relying solely on metrics.

### 5.3 Maintainability

**NFR-07 — Centralized Definitions**

Metric instruments must be organized into typed metric classes and centralized cross-cutting instrumentation.

**NFR-08 — Minimal Change Surface**

Adding a new business metric should normally require creating or updating an appropriate typed metric class and invoking its recording method at the relevant application boundary, without modifying unrelated handlers.

**NFR-09 — Consistent Naming**

Metric names, units, instrument types, and dimensions must follow documented conventions.

### 5.4 Cardinality

**NFR-10 — Bounded Dimensions**

Metric dimensions must use bounded, controlled value sets.

**NFR-11 — Prohibited Identifiers**

The following must not be used as metric dimensions:

* `CustomerId`
* `OrderId`
* `ProductId`
* Email addresses.
* Phone numbers.
* Correlation IDs or trace IDs.
* Other unbounded identifiers or sensitive values.

**NFR-12 — Preferred Dimensions**

Where useful, prefer controlled categories such as:

* `outcome`
* `operation`
* `status`
* A small, predefined set of customer tiers.

Dimensions must be appropriate to the individual metric; not every metric needs every dimension.

### 5.5 Security

**NFR-13 — Sensitive Data Protection**

Metrics must not expose passwords, authentication tokens, payment credentials, personal information, or other confidential data.

**NFR-14 — Controlled Tag Values**

Metric dimensions must not contain raw user input, exception messages, or other values that could expose confidential information or cause unbounded cardinality.

## 6. Architectural Constraints

**AC-01 — Layer Ownership**

Application owns typed metric definitions and measurement components where business or application metrics are required. These components may use the .NET metrics API directly.

Infrastructure owns the shared meter registration, OpenTelemetry SDK configuration, instrumentation setup, and exporter configuration.

**AC-02 — Domain Independence**

The Domain layer must not depend on OpenTelemetry, `Meter`, metric instruments, or metrics-specific abstractions.

**AC-03 — Automatic Instrumentation Preference**

Automatic instrumentation must be preferred for common framework-level measurements, including HTTP request metrics.

**AC-04 — No Unnecessary Generic Abstraction**

A generic `IMetricRecorder` abstraction must not be introduced in the current implementation without a demonstrated architectural need.

**AC-05 — Existing Job Pipeline**

Background-job instrumentation must integrate with the existing `JobOrchestrator` rather than introducing a separate execution pipeline or duplicating retry responsibility.

**AC-06 — Existing Modular Monolith**

The implementation must remain within the existing modular monolith architecture and must not require a separate metrics service or message broker.

## 7. Acceptance Criteria

The Metrics capability is acceptable when the following conditions are verified:

1. A single application `Meter` instance is created and reused as the source for application-defined instruments.
2. At least one typed metric class, `OrderMetrics`, exists and is registered with an appropriate singleton lifetime.
3. The required order counter and processing-duration histogram are created once and use documented units and names.
4. `TelemetryBehavior` records operation count, duration, and outcome for MediatR requests.
5. Success and failure measurements follow the defined exception-based outcome semantics.
6. Operation names and metric dimensions remain bounded and do not contain entity identifiers or sensitive information.
7. The `JobOrchestrator` records job execution count, duration, and outcome without requiring duplicate instrumentation in job handlers.
8. ASP.NET Core instrumentation provides the required HTTP measurements without manual duplication.
9. The OpenTelemetry SDK is configured to collect the intended application meter and HTTP instrumentation.
10. The OTLP exporter is configured and can export metrics to a compatible backend.
11. Exporter endpoint configuration is externalized and correctly applied by the registration code.
12. An unavailable observability backend does not cause ordinary business operations to fail under the application's tested operating conditions.
13. The Domain layer remains free of metrics dependencies.
14. The implementation is limited to the Application and Infrastructure layers as specified, with no unnecessary generic metrics abstraction.

## 8. Summary

The Metrics capability establishes a consistent approach to collecting application and business measurements through OpenTelemetry.

Typed metric classes encapsulate business measurements, the MediatR `TelemetryBehavior` records application-operation metrics, the existing `JobOrchestrator` measures background-job execution, and ASP.NET Core instrumentation provides HTTP request metrics.

Infrastructure owns the telemetry pipeline and OTLP exporter configuration. The design preserves Domain-layer independence, controls metric cardinality, and avoids introducing unnecessary infrastructure.

**Core principle:** Metrics provide operational visibility without becoming part of business invariants, introducing uncontrolled cardinality, or serving as a substitute for durable business records.
