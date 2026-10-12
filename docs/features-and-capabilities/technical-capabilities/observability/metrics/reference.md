# Metrics — Technical Reference Document

**Project:** E-Commerce Modular Monolith
**Capability:** Cross-Cutting Observability — OpenTelemetry Metrics
**Version:** 1.0 — Final

---

## 1. Overview

Metrics provide aggregated numerical measurements of system behavior and business activity. They help answer operational questions about throughput, latency, failure rates, and resource utilization.

The Metrics capability uses **OpenTelemetry Metrics API** as the standard telemetry interface. Typed metric classes encapsulate business and application measurements, while automatic instrumentation captures common operations such as HTTP requests.

The architecture separates responsibilities as follows:

* **Domain:** Remains completely metrics-agnostic.
* **Application:** Defines typed metric classes for business and application measurements and uses metrics where appropriate.
* **Infrastructure:** Owns the shared application meter instance, OpenTelemetry SDK configuration, instrumentation, and exporter setup.

The capability is designed for the existing modular monolith. It does not introduce a separate telemetry service or require distributed infrastructure.

## 2. Architecture

### 2.1 Metrics Pipeline

```text
Business / Application Code
        |
        v
Typed Metric Classes
(OrderMetrics, etc.)
        |
        v
Shared Application Meter
(ECommerceMeter)
        |
        +-----------------------------+
        |                             |
        v                             v
  Counter / Histogram          ObservableGauge
        |
        v
OpenTelemetry SDK
        |
        +-----------------------------+
        |                             |
        v                             v
ASP.NET Core Instrumentation     OTLP Exporter
(HTTP Request Metrics)                 |
                                       v
                              Observability Backend
```

Application-operation metrics are recorded by the MediatR `TelemetryBehavior`. HTTP request metrics are collected through ASP.NET Core instrumentation.

Background-job metrics may be added later through the existing `JobOrchestrator`, allowing job execution to be measured without introducing a separate metrics pipeline.

### 2.2 Dependency Boundaries

The Metrics capability follows the existing architecture:

* The Domain layer does not reference OpenTelemetry or metrics infrastructure.
* Application components use typed metric abstractions or classes appropriate to the chosen dependency direction.
* Infrastructure configures the SDK, registers the shared meter, and configures exporters.
* Application code does not configure exporters or depend on the observability backend.

**Dependency-direction note:** If `OrderMetrics` and `TelemetryBehavior` directly reference `System.Diagnostics.Metrics.Meter` and instrument types, Application has a dependency on the .NET metrics API. This does not introduce an Infrastructure dependency, but it is an explicit Application-layer design choice. If strict telemetry-provider independence is required, define Application-owned metric contracts and implement them in Infrastructure instead.

## 3. Folder Structure

```text
Application/
└── Shared/
    ├── Metrics/
    │   └── OrderMetrics.cs
    │
    └── Behaviors/
        └── TelemetryBehavior.cs

Infrastructure/
└── Observability/
    └── Metrics/
        ├── ECommerceMeter.cs
        └── MetricsRegistrationExtensions.cs
```

### Component Responsibilities

| Component                          | Layer          | Responsibility                                                                                                   |
| ---------------------------------- | -------------- | ---------------------------------------------------------------------------------------------------------------- |
| `OrderMetrics.cs`                  | Application    | Encapsulates order-related business measurements using typed metric instruments.                                 |
| `TelemetryBehavior.cs`             | Application    | Records MediatR request counts, outcomes, and execution durations.                                               |
| `ECommerceMeter.cs`                | Infrastructure | Exposes the shared application `Meter` instance.                                                                 |
| `MetricsRegistrationExtensions.cs` | Infrastructure | Registers the meter and metric classes and configures the OpenTelemetry SDK, instrumentation, and OTLP exporter. |

## 4. Component Details

### 4.1 ECommerceMeter

**Layer:** Infrastructure

**Purpose:** Provide a shared application-wide `Meter` instance.

**Implementation characteristics:**

* Exposes a static `Meter` instance.
* Uses the meter name `ECommerce.ModularMonolith`.
* Uses instrumentation version `1.0.0`.

**Lifetime:** The static meter instance is shared for the lifetime of the process.

The meter name identifies the instrumentation source. The OpenTelemetry SDK subscribes to that source by registering the corresponding meter name in its metrics configuration.

The meter should not be recreated for individual requests or operations.

### 4.2 Typed Metric Classes

Example: `OrderMetrics`

**Layer:** Application

**Purpose:** Encapsulate business measurements and expose controlled methods for recording them.

**Responsibilities:**

* Receive the shared `Meter` through constructor injection.
* Create instruments once during construction.
* Expose methods that record measurements with predefined, bounded dimensions.
* Keep metric names and tag conventions consistent.
* Avoid exposing raw instrument manipulation to unrelated application components.

**Instruments:**

| Metric                       | Instrument          | Purpose                                                          |
| ---------------------------- | ------------------- | ---------------------------------------------------------------- |
| `orders.created`             | `Counter<long>`     | Counts successfully created orders.                              |
| `orders.processing.duration` | `Histogram<double>` | Measures the duration of the defined order-processing operation. |

**Potential dimensions:**

* `customer.tier`: A controlled set of customer-tier values.
* `status`: A controlled set of status values, where meaningful for the measurement.

The exact dimensions should be selected for each instrument. A tag should be included only when it provides useful analytical value.

**Recording rule:** Business metrics should reflect the event they claim to measure. For example, `orders.created` should be incremented only when order creation has succeeded according to the application's defined success boundary.

Recording a metric after a successful database save reflects the application flow, but it does not guarantee that the measurement will be exported if the process terminates immediately afterward. If a business measurement must be durably recoverable, an in-memory metric alone is insufficient; a durable source of business events or records may be needed.

### 4.3 TelemetryBehavior

**Layer:** Application

**Purpose:** Automatically measure the execution of MediatR requests through a pipeline behavior.

**Instruments:**

| Metric                        | Instrument          | Purpose                                        |
| ----------------------------- | ------------------- | ---------------------------------------------- |
| `mediatr.operations`          | `Counter<long>`     | Counts completed MediatR operations.           |
| `mediatr.operations.duration` | `Histogram<double>` | Measures MediatR operation execution duration. |

**Dimensions:**

* `operation`: The request or operation name.
* `outcome`: A bounded result such as `success` or `failure`.

**Behavior:**

1. Starts a timer before invoking the next MediatR pipeline component.
2. Executes the next component.
3. Records the operation outcome and duration.
4. Records failure when an exception escapes the measured execution.
5. Propagates exceptions according to the application's normal exception-handling behavior.

**Cardinality control:** The behavior must not attach exception messages, user identifiers, order identifiers, correlation identifiers, or other unbounded values to metric dimensions.

The `operation` dimension should use a stable set of known request names. If the application uses dynamically generated request names, normalize them to a bounded set.

**Implementation note:** Define precisely what constitutes a completed operation. For example, a request that returns a failure `Result` without throwing an exception is not automatically classified as a failure by exception-based instrumentation. If business-level failures should count as failures, the behavior must inspect the application's result conventions.

The duration unit should be documented and implemented consistently. If milliseconds are intended, record milliseconds explicitly and configure the histogram accordingly.

### 4.4 MetricsRegistrationExtensions

**Layer:** Infrastructure

**Purpose:** Register the Metrics capability and configure its collection and export pipeline.

**Responsibilities:**

* Register `ECommerceMeter.Instance` as a singleton, if the implementation exposes it through dependency injection.
* Register typed metric classes such as `OrderMetrics` as singletons, provided they are stateless apart from their instruments.
* Configure OpenTelemetry metrics collection.
* Subscribe to the application meter.
* Add ASP.NET Core instrumentation.
* Configure the OTLP exporter.

Conceptually, the configuration performs the following steps:

```csharp
services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter(ECommerceMeter.Instance.Name)
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter();
    });
```

This is an illustrative configuration fragment; it assumes the required OpenTelemetry packages are installed and the surrounding registration code matches the project's implementation.

**Important distinction:** Registering a `Meter` with dependency injection and creating a static meter instance are different implementation approaches. The registration must match how the application and typed metric classes access the shared instance. Avoid creating multiple unrelated meters with the same intended role.

## 5. Automatic Instrumentation

### 5.1 HTTP Request Metrics

ASP.NET Core instrumentation collects HTTP server metrics without requiring application developers to instrument each endpoint manually.

Depending on the installed OpenTelemetry package versions and semantic-convention support, relevant measurements may include:

* `http.server.request.duration`: HTTP server request duration.
* `http.server.active_requests`: Number of requests currently being processed.
* Additional HTTP request measurements supported by the configured instrumentation version.

**Version note:** Metric names and available measurements depend on the OpenTelemetry instrumentation package version and semantic conventions. Verify the actual emitted metrics against the versions used by the application rather than assuming every HTTP metric name is available.

HTTP instrumentation is distinct from subscribing to a meter by name. Calling `AddAspNetCoreInstrumentation()` configures the instrumentation component; calling `AddMeter(...)` subscribes the SDK to a named `Meter` source.

### 5.2 Application Operation Metrics

The MediatR `TelemetryBehavior` records:

* `mediatr.operations`: Operation count.
* `mediatr.operations.duration`: Operation execution duration.

The metrics use the bounded dimensions `operation` and `outcome`.

These measurements complement HTTP metrics. HTTP instrumentation describes incoming requests, while MediatR instrumentation describes application requests handled through the MediatR pipeline. A single HTTP request may invoke multiple MediatR operations, and some MediatR operations may execute outside HTTP request processing.

## 6. Metric Naming and Dimensions

### 6.1 Naming Convention

Metric names use a consistent, lowercase, dot-separated convention.

Examples:

* `orders.created`
* `orders.processing.duration`
* `mediatr.operations`
* `mediatr.operations.duration`

Metric names should communicate what is measured, while the instrument type and unit should reflect the measurement's meaning.

For duration metrics, use a consistent unit, preferably seconds when aligning with OpenTelemetry conventions. If the application records milliseconds instead, document and configure that choice consistently.

### 6.2 Dimensions and Cardinality

Dimensions provide context for interpreting measurements. Their values should come from controlled, bounded sets.

| Dimension       | Example                  | Guidance                                                         |
| --------------- | ------------------------ | ---------------------------------------------------------------- |
| `operation`     | `PlaceOrderCommand`      | Use stable request names from the known application request set. |
| `outcome`       | `success`, `failure`     | Use a small, predefined set of values.                           |
| `status`        | `completed`, `cancelled` | Use only controlled business statuses appropriate to the metric. |
| `customer.tier` | `standard`, `premium`    | Use only a bounded set of tier values.                           |

Avoid high-cardinality dimensions, including:

* User, customer, order, or payment identifiers.
* Email addresses and phone numbers.
* Correlation and trace identifiers.
* Exception messages or stack traces.
* Arbitrary URL paths containing resource identifiers.
* Unrestricted values supplied by users.

High cardinality can increase storage and processing costs and make metrics less useful for aggregate analysis. Sensitive information must not be placed in metric dimensions.

## 7. Export and Configuration

### 7.1 OTLP Export

OTLP is the primary export mechanism for the Metrics capability.

The exporter sends measurements to a compatible observability backend or telemetry collector. The backend can be replaced without changing business logic, provided the exporter and endpoint configuration are updated appropriately.

### 7.2 Endpoint Configuration

The OTLP endpoint can be configured using standard OpenTelemetry environment variables or explicit application configuration wired into the SDK setup.

For example:

```text
OTEL_EXPORTER_OTLP_ENDPOINT=<configured-collector-endpoint>
```

If a custom application setting is used instead, the registration code must explicitly read and apply that setting. Merely defining a value in `appsettings.json` does not automatically configure the exporter.

The default OTLP protocol and endpoint behavior depend on the exporter configuration. Common defaults are gRPC on port `4317` and HTTP/protobuf on port `4318`; verify the actual protocol and endpoint used by the deployment.

**Configuration and security requirements:**

* Do not hardcode environment-specific endpoints or credentials.
* Use environment variables or an appropriate secret-management mechanism for sensitive configuration.
* Ensure the collector or backend is reachable from the application environment.
* Confirm that exporter failures do not unexpectedly disrupt business operations.

## 8. Operational Considerations

### 8.1 Telemetry Export Failures

Metrics are observability data, not the authoritative record of business transactions.

The application should ordinarily continue processing business operations when the observability backend is unavailable. However, this behavior depends on the SDK, exporter, and hosting configuration. Validate the application's behavior under exporter failures rather than assuming all telemetry failures are harmless.

Metrics may be delayed or lost during process termination, buffer exhaustion, or prolonged exporter outages. This is acceptable for many operational measurements but may be unacceptable for audit-grade or financially authoritative records.

### 8.2 Instrument Lifecycle

Create instruments once and reuse them. Register typed metric classes with lifetimes consistent with that requirement.

Do not create counters or histograms per request. Repeated instrument creation can complicate measurement collection and resource usage.

### 8.3 Testing and Verification

Verification should cover:

* Correct registration of the application meter.
* Collection of the expected instruments.
* Accurate success and failure classification.
* Consistent duration units.
* Bounded metric dimensions.
* HTTP metrics emitted by the configured instrumentation version.
* Exporter configuration and endpoint connectivity.
* Expected behavior when telemetry export is unavailable.

Use tools such as `MeterListener` or a test exporter to inspect measurements without requiring a production observability backend for every test.

## 9. Future Extensions

The existing pipeline can be extended with additional typed metric classes or instrumentation for:

* Inventory and stock adjustments.
* Outbox processing and backlog behavior.
* Background-job execution through the existing `JobOrchestrator`.
* Payment and refund processing.
* File-storage operations.

Each extension should define its measurements, units, and dimensions explicitly. Business metrics should be recorded at the point where their stated business meaning becomes true.

## 10. Summary

The Metrics capability provides standardized telemetry through the OpenTelemetry Metrics API.

Typed metric classes encapsulate business measurements, the MediatR `TelemetryBehavior` records application-operation metrics, and ASP.NET Core instrumentation collects HTTP measurements. Infrastructure configures the shared meter, SDK, instrumentation, and OTLP exporter.

The design preserves Domain-layer independence, maintains controlled metric cardinality, and integrates with the existing modular monolith without introducing unnecessary distributed infrastructure.

**Core principle:** Metrics should provide reliable, low-cardinality operational insight without becoming a dependency of business invariants or a substitute for durable business records.
