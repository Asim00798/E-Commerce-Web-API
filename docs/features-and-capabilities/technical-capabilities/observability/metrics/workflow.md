\# Metrics — Workflow Technical Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Capability:\*\* Cross-Cutting Observability — OpenTelemetry Metrics

\*\*Version:\*\* 1.0 — Final



\---



\## 1. Overview



This document describes the end-to-end workflows of the Metrics capability, including:



\* Registering the OpenTelemetry metrics infrastructure.

\* Recording business measurements through typed metric classes.

\* Automatically recording application-operation metrics through MediatR.

\* Collecting HTTP request metrics through ASP.NET Core instrumentation.

\* Exporting collected metrics to an OTLP-compatible observability backend.



Metrics provide aggregated measurements of system behavior and business activity. They are independent of logging, distributed tracing, and health checks, although their data can complement those capabilities during operational investigations.



The Domain layer remains completely metrics-agnostic. Business and application measurements are recorded through typed metric classes, while Infrastructure configures the collection and export pipeline.



\## 2. Actors and Components



\### 2.1 Actors



| Actor               | Description                                                                                                              |

| ------------------- | ------------------------------------------------------------------------------------------------------------------------ |

| Application Handler | Executes application use cases and records relevant business measurements through typed metric classes when appropriate. |

| MediatR Pipeline    | Intercepts registered commands and queries to collect application-operation metrics.                                     |

| Background Job      | Executes asynchronous work. Generic job execution metrics can be recorded centrally by the existing `JobOrchestrator`.   |

| Operations Team     | Uses metrics in observability dashboards and alerting systems to monitor application behavior.                           |



\### 2.2 Key Components



| Component                       | Layer                          | Responsibility                                                                                   |

| ------------------------------- | ------------------------------ | ------------------------------------------------------------------------------------------------ |

| `ECommerceMeter`                | Infrastructure                 | Provides the shared application `Meter` instance.                                                |

| `OrderMetrics`                  | Application                    | Encapsulates order-related business measurements.                                                |

| `TelemetryBehavior`             | Application                    | Records MediatR operation counts, durations, and outcomes.                                       |

| `MetricsRegistrationExtensions` | Infrastructure                 | Registers metric components and configures the OpenTelemetry SDK, instrumentation, and exporter. |

| ASP.NET Core Instrumentation    | Infrastructure configuration   | Collects HTTP server metrics automatically.                                                      |

| OTLP Exporter                   | Infrastructure configuration   | Exports collected metrics to the configured observability backend or collector.                  |

| `JobOrchestrator`               | Application execution pipeline | Provides the central execution boundary where generic background-job metrics can be recorded.    |



\## 3. Workflow 1: Register Metrics Infrastructure



\### 3.1 Description



During application startup, the application registers the shared meter, typed metric classes, and OpenTelemetry metrics pipeline.



The registration process establishes the instruments' collection source and configures how measurements are exported.



\### 3.2 Flow



```text

Application Startup (Program.cs)

&#x20;       |

&#x20;       v

AddApplicationMetrics()

&#x20;       |

&#x20;       +--> Register shared ECommerceMeter

&#x20;       |

&#x20;       +--> Register OrderMetrics

&#x20;       |

&#x20;       +--> Configure OpenTelemetry Metrics

&#x20;                |

&#x20;                +--> Subscribe to application meter

&#x20;                |

&#x20;                +--> Add ASP.NET Core instrumentation

&#x20;                |

&#x20;                +--> Configure OTLP exporter

&#x20;       |

&#x20;       v

Metrics Infrastructure Ready

```



\### 3.3 Important Rules



\* The shared meter must be created once and reused throughout the application process.

\* Typed metric classes must create their instruments once during construction.

\* The OpenTelemetry SDK must subscribe to the correct meter name.

\* Exporter endpoints and other environment-specific settings must be configured externally.

\* HTTP metrics must be collected through the configured instrumentation rather than manually duplicated in controllers or middleware.

\* Instrumentation and exporter configuration must be completed before the application begins serving requests.



\## 4. Workflow 2: Record a Business Metric



\### 4.1 Description



An application handler records a business measurement through a typed metric class, such as `OrderMetrics`.



The handler does not manage metric instruments, meter construction, or exporter configuration.



\### 4.2 Flow



```text

PlaceOrderCommandHandler

&#x20;       |

&#x20;       v

Order Creation Logic

&#x20;       |

&#x20;       v

Order Persistence Succeeds

&#x20;       |

&#x20;       v

OrderMetrics.RecordCreated(...)

&#x20;       |

&#x20;       v

orders.created Counter Increments

&#x20;       |

&#x20;       v

Measurement Collected by OpenTelemetry SDK

&#x20;       |

&#x20;       v

Measurement Exported through OTLP

```



The diagram represents the intended application flow. The exact recording point depends on the business event being measured and the application's persistence semantics.



\### 4.3 Important Rules



\* Business metrics must represent clearly defined business events.

\* `orders.created` must be incremented only when order creation has succeeded according to the application's defined success boundary.

\* A successful database save does not guarantee that the measurement will be exported if the process terminates before export.

\* Typed metric classes encapsulate instrument access and recording conventions.

\* The handler must not manage the meter, create instruments, or configure the exporter.

\* Dimensions such as `customer.tier` or `status` must use controlled, bounded values.

\* Metrics should be recorded explicitly when they provide meaningful operational or business insight.



\*\*Reliability distinction:\*\* Metrics are observational measurements, not durable business records. If a business measurement must be recoverable after process failure, an appropriate persistent record or durable event mechanism may be necessary.



\## 5. Workflow 3: Automatic Application-Operation Metrics



\### 5.1 Description



The MediatR `TelemetryBehavior` automatically records the count and execution duration of requests passing through the registered MediatR pipeline.



It determines the operation outcome according to the defined exception-based semantics.



\### 5.2 Flow



```text

MediatR Request

&#x20;       |

&#x20;       v

TelemetryBehavior

&#x20;       |

&#x20;       +--> Start Timer

&#x20;       |

&#x20;       +--> Execute Next Pipeline Component

&#x20;       |

&#x20;       +--> Stop Timer

&#x20;       |

&#x20;       +--> Determine Outcome

&#x20;       |       |

&#x20;       |       +--> Normal Completion: success

&#x20;       |

&#x20;       |       +--> Exception Escapes: failure

&#x20;       |

&#x20;       v

Record Measurements

&#x20;       |

&#x20;       +--> mediatr.operations

&#x20;       |       Dimensions: operation, outcome

&#x20;       |

&#x20;       +--> mediatr.operations.duration

&#x20;               Dimensions: operation, outcome

&#x20;       |

&#x20;       v

OpenTelemetry Metrics Pipeline

```



The behavior should use appropriate exception-handling and timing logic so that duration and outcome measurements are recorded for both successful execution and exceptions that escape the measured pipeline.



\### 5.3 Important Rules



\* The `operation` dimension must use stable names from the known set of MediatR request types.

\* The `outcome` dimension must use a bounded set of values, such as `success` and `failure`.

\* Exception types and exception messages must not be used as dimensions.

\* Entity identifiers, user identifiers, and correlation IDs must not be attached to these measurements.

\* The behavior must not duplicate HTTP request metrics provided by ASP.NET Core instrumentation.

\* The behavior must not create tracing spans; distributed tracing belongs to a separate implementation phase.

\* Duration units must be consistent with the metric definition.



\*\*Outcome semantics:\*\* A request returning a failure `Result` without throwing an exception is still a successful pipeline execution under the current exception-based rule. If business-level failures need separate measurement, that must be explicitly defined.



\## 6. Workflow 4: HTTP Request Metrics



\### 6.1 Description



ASP.NET Core instrumentation collects HTTP server measurements automatically for instrumented requests.



This avoids scattering manual HTTP metric recording across controllers and middleware.



\### 6.2 Flow



```text

Incoming HTTP Request

&#x20;       |

&#x20;       v

ASP.NET Core Request Pipeline

&#x20;       |

&#x20;       v

OpenTelemetry ASP.NET Core Instrumentation

&#x20;       |

&#x20;       v

HTTP Measurements Recorded

&#x20;       |

&#x20;       +--> Request Duration

&#x20;       |

&#x20;       +--> Request Activity / Count

&#x20;       |

&#x20;       +--> Applicable Route and Status Attributes

&#x20;       |

&#x20;       v

OpenTelemetry Metrics Pipeline

&#x20;       |

&#x20;       v

OTLP Export

```



\### 6.3 Important Rules



\* HTTP instrumentation must be enabled through the OpenTelemetry registration configuration.

\* Controllers and application handlers must not manually duplicate HTTP measurements already provided by the instrumentation.

\* The configured package version determines the available HTTP metrics and their names.

\* The implementation must verify the actual measurements emitted by the installed instrumentation version.



For example, current OpenTelemetry HTTP semantic conventions include `http.server.request.duration`. Do not assume that every proposed name, such as `http.server.request.count` or `http.server.response.status\_code`, exists as a separate instrument in the selected package version.



HTTP instrumentation and MediatR instrumentation measure different execution boundaries. One HTTP request may invoke multiple MediatR requests, and MediatR requests may also execute outside HTTP processing.



\## 7. Workflow 5: Background-Job Metrics



\### 7.1 Description



Generic background-job metrics belong at the existing `JobOrchestrator` execution boundary. This allows the application to measure job execution consistently without requiring every job handler to duplicate the same instrumentation.



\### 7.2 Flow



```text

Background Job Dispatched

&#x20;       |

&#x20;       v

JobOrchestrator

&#x20;       |

&#x20;       +--> Start Timer

&#x20;       |

&#x20;       +--> Execute Job Handler

&#x20;       |

&#x20;       +--> Determine Execution Outcome

&#x20;       |

&#x20;       +--> Record Job Metrics

&#x20;                |

&#x20;                +--> Job Execution Count

&#x20;                |

&#x20;                +--> Job Execution Duration

&#x20;                |

&#x20;                +--> Job Execution Outcome

&#x20;       |

&#x20;       v

OpenTelemetry Metrics Pipeline

&#x20;       |

&#x20;       v

OTLP Export

```



\### 7.3 Important Rules



\* Job names must come from known job definitions.

\* The outcome dimension must use bounded values.

\* Job handlers must not duplicate generic execution metrics already recorded by the orchestrator.

\* Business-specific measurements may still be recorded through feature-owned typed metric classes.

\* Instrumentation must respect the existing job execution and retry pipeline rather than introducing a second retry mechanism.

\* The implementation must define whether measurements represent individual execution attempts or final logical job outcomes. These are different concepts and must not be conflated.



\*\*Implementation status:\*\* This workflow applies when job metrics have been integrated into `JobOrchestrator`. If that integration is still planned, it should be documented as a future workflow rather than represented as an implemented capability.



\## 8. Workflow 6: Metric Export



\### 8.1 Description



The OpenTelemetry SDK collects measurements from subscribed instruments and exports them through the configured OTLP exporter.



Collection and export are separate stages: recording a measurement does not necessarily mean that the backend has already received it.



\### 8.2 Flow



```text

Metric Sources

&#x20;       |

&#x20;       +--> Typed Business Metrics

&#x20;       |

&#x20;       +--> MediatR Operation Metrics

&#x20;       |

&#x20;       +--> ASP.NET Core HTTP Metrics

&#x20;       |

&#x20;       +--> Background-Job Metrics

&#x20;       |

&#x20;       v

OpenTelemetry Metrics SDK

&#x20;       |

&#x20;       v

Configured OTLP Exporter

&#x20;       |

&#x20;       v

OTLP Endpoint / Collector

&#x20;       |

&#x20;       v

Observability Backend

&#x20;       |

&#x20;       v

Dashboards and Operational Analysis

```



\### 8.3 Important Rules



\* OTLP is the primary export mechanism.

\* The exporter endpoint must be externally configurable.

\* A backend change should not require changes to business handlers or metric definitions.

\* Exporter configuration must explicitly bind custom application settings when used.

\* The application must be tested under backend-unavailable conditions.

\* Telemetry export failures must not ordinarily cause business operations to fail.

\* Temporary export failures, process termination, and buffer limitations may cause metrics to be delayed or lost.



Metrics are not a substitute for application logs, tracing, health checks, or durable business records.



\## 9. Summary



The Metrics workflow consists of the following stages:



1\. \*\*Infrastructure registration:\*\* Register the shared meter, typed metric classes, OpenTelemetry SDK, instrumentation, and exporter.

2\. \*\*Business measurements:\*\* Record meaningful business events through typed metric classes.

3\. \*\*Application-operation measurements:\*\* Collect MediatR request counts, durations, and outcomes through `TelemetryBehavior`.

4\. \*\*HTTP measurements:\*\* Collect request metrics through ASP.NET Core instrumentation.

5\. \*\*Background-job measurements:\*\* Record generic job execution metrics through the existing `JobOrchestrator` when integrated.

6\. \*\*Metric export:\*\* Send collected measurements through OTLP to the configured observability backend.



This design provides consistent operational insight, controlled metric cardinality, and centralized instrumentation while preserving Domain-layer independence and the existing modular monolith architecture.



\*\*Core principle:\*\* Collect each measurement at the correct execution boundary, define its meaning precisely, and keep observability independent of business correctness.



