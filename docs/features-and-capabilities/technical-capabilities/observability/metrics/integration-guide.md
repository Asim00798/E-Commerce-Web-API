# Metrics — Integration Guide

**Project:** E-Commerce Modular Monolith
**Capability:** Cross-Cutting Observability — OpenTelemetry Metrics
**Version:** 1.0 — Final

---

## 1. Purpose

This guide explains how to integrate the Metrics capability into the E-Commerce modular monolith.

It covers:

* Registering the shared `ECommerceMeter` and metric exporter.
* Using typed metric classes to record business measurements.
* Enabling automatic application-operation metrics through MediatR.
* Configuring automatic HTTP request instrumentation.
* Adding new metrics and controlling metric cardinality.
* Verifying that metrics are emitted correctly.

The guide is intended for developers working within any bounded context who need to record or consume application and business metrics.

---

## 2. Prerequisites

Before integrating metrics, ensure the following components are available.

### 2.1 Infrastructure

* `ECommerceMeter`
* `MetricsRegistrationExtensions`
* OpenTelemetry metrics provider and exporter configuration.

### 2.2 Application

* `TelemetryBehavior`
* Typed metric classes, such as `OrderMetrics`.
* A shared Application metrics namespace or folder consistent with the existing project structure.

### 2.3 Required NuGet Packages

| Package                                        | Purpose                                                                         |
| ---------------------------------------------- | ------------------------------------------------------------------------------- |
| `OpenTelemetry.Extensions.Hosting`             | Integrates OpenTelemetry with the .NET hosting lifecycle.                       |
| `OpenTelemetry.Exporter.OpenTelemetryProtocol` | Exports metrics through OTLP.                                                   |
| `OpenTelemetry.Instrumentation.AspNetCore`     | Provides ASP.NET Core instrumentation through `AddAspNetCoreInstrumentation()`. |

An OTLP-compatible collector or observability backend should be available for end-to-end verification. It may be configured later during local development.

---

## 3. Step-by-Step Integration

### 3.1 Register the Metrics Infrastructure

In `Program.cs`, or the application's composition root, register the Metrics capability:

```csharp
builder.Services.AddApplicationMetrics();
```

The extension method is responsible for configuring the metrics infrastructure, including:

* Registering the shared `ECommerceMeter`.
* Registering typed metric classes, such as `OrderMetrics`.
* Configuring ASP.NET Core instrumentation.
* Configuring the OTLP exporter.
* Registering the metrics provider with the application's dependency-injection and hosting infrastructure.

The exact registrations must match the implementation of `MetricsRegistrationExtensions`.

**Important:** Register the metrics provider and its instruments through a consistent configuration path. Avoid creating separate providers or meters unnecessarily, because doing so can cause instruments to be omitted from the intended export pipeline.

### 3.2 Configure the OTLP Endpoint

The OTLP exporter must be configured to reach the intended collector or observability backend.

For example, the application may use this configuration:

```json
{
  "OpenTelemetry": {
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

Alternatively, the standard environment variable can be used:

```bash
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
```

The application's configuration code must explicitly bind the custom `OpenTelemetry:OtlpEndpoint` setting if that setting is used. The environment variable is a standard OpenTelemetry configuration option, subject to the selected exporter and protocol configuration.

The example endpoint assumes a local OTLP receiver listening on the specified address and port. The actual endpoint and protocol depend on the deployment environment.

**Operational considerations:**

* Ensure the endpoint is reachable from the application process.
* Configure the protocol consistently with the receiver. OTLP commonly uses gRPC on port `4317` and HTTP/protobuf on port `4318`.
* Do not assume that an exporter successfully registered during startup can necessarily reach its destination.
* Configure credentials and transport security where required by the target environment.

### 3.3 Register the MediatR Telemetry Behavior

`TelemetryBehavior` must be registered in the MediatR pipeline to capture application-operation metrics.

Example:

```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        /* Application assemblies */);

    cfg.AddOpenBehavior(typeof(TelemetryBehavior<,>));
});
```

Use the actual Application assemblies in the production registration.

If MediatR is already registered elsewhere, add the behavior to the existing registration rather than creating a conflicting or redundant registration.

#### Lifetime

The behavior can use a transient lifetime, which is the conventional lifetime for MediatR pipeline behaviors.

Metric instruments should be created once and reused through the shared meter rather than recreated for every request. Typed metric classes should normally be registered as singletons when they hold long-lived instruments created from that meter.

### 3.4 Use Typed Metrics in Handlers

Business metrics should be recorded through typed metric classes rather than by creating instruments directly inside handlers.

For example, a handler that creates an order can inject `OrderMetrics` and record the appropriate business measurement.

```csharp
using E_Commerce.Application.Shared.Metrics;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands.PlaceOrder;

public sealed class PlaceOrderCommandHandler
    : IRequestHandler<PlaceOrderCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly OrderMetrics _metrics;

    public PlaceOrderCommandHandler(
        IUnitOfWork unitOfWork,
        OrderMetrics metrics)
    {
        _unitOfWork = unitOfWork;
        _metrics = metrics;
    }

    public async Task<Result<Guid>> Handle(
        PlaceOrderCommand command,
        CancellationToken ct)
    {
        // Execute business logic and determine the order ID.

        await _unitOfWork.SaveChangesAsync(ct);

        _metrics.RecordCreated(customerTier: "standard");

        return Result<Guid>.Success(orderId);
    }
}
```

The example assumes `orderId` is produced by the omitted business logic.

**Important:** Record a metric such as `orders.created` only when the corresponding business event has actually occurred. If the database transaction fails, the metric should not misleadingly indicate that the order was successfully persisted.

A metric recorded immediately after `SaveChangesAsync` succeeds reflects successful persistence within the current application flow. If the metric must represent a committed business event with stronger delivery guarantees, consider how it interacts with the transaction boundary and the application's outbox or event-processing workflow.

### 3.5 Automatic Application-Operation Metrics

`TelemetryBehavior` records application-operation measurements for MediatR requests.

The intended instruments are:

| Metric                        | Instrument | Dimensions             | Purpose                                                       |
| ----------------------------- | ---------- | ---------------------- | ------------------------------------------------------------- |
| `mediatr.operations`          | Counter    | `operation`, `outcome` | Counts completed application-operation executions by outcome. |
| `mediatr.operations.duration` | Histogram  | `operation`, `outcome` | Measures application-operation execution duration.            |

The exact metric names, instrument types, tags, and timing boundaries must match the implemented `TelemetryBehavior`.

Typical outcomes include `success` and `failure`.

No additional handler code is required for these automatic measurements, provided the behavior is registered correctly and the instruments are included in the configured meter.

**Important:** Automatic application-operation metrics and business metrics serve different purposes:

* `mediatr.operations` measures application execution.
* `orders.created` measures a business event.

Neither replaces the other.

### 3.6 HTTP Request Metrics

ASP.NET Core instrumentation provides HTTP server metrics through:

```csharp
.AddAspNetCoreInstrumentation()
```

This instrumentation can collect HTTP request measurements, including request duration and request counts, depending on the instrumentation version and configured instruments.

No manual metric-recording code is normally required for standard HTTP server metrics.

**Do not replace `AddAspNetCoreInstrumentation()` with `.AddMeter("Microsoft.AspNetCore.Hosting")` as though the two registrations were equivalent.** Instrumentation registers the relevant instrumentation behavior; meter registration subscribes the provider to instruments emitted by a particular meter. They serve different purposes.

The exact metrics exposed should be verified against the installed OpenTelemetry instrumentation version.

---

## 4. Adding a New Typed Metric

When a bounded context needs a new business metric, use a dedicated typed metric class.

### 4.1 Implementation Steps

1. Create a metric class in the established Application metrics namespace, such as `Application/Shared/Metrics/InventoryMetrics.cs`.
2. Inject the shared `Meter` through the constructor.
3. Define the required instruments, such as counters, histograms, or observable gauges.
4. Use descriptive metric names and appropriate units.
5. Expose methods that record measurements with controlled, bounded tags.
6. Register the metric class as a singleton through `MetricsRegistrationExtensions`.
7. Verify that the shared meter is included in the configured OpenTelemetry metrics provider.

### 4.2 Example: InventoryMetrics

```csharp
using System.Diagnostics.Metrics;

namespace E_Commerce.Application.Shared.Metrics;

public sealed class InventoryMetrics
{
    private readonly Counter<long> _stockAdjusted;

    public InventoryMetrics(Meter meter)
    {
        _stockAdjusted = meter.CreateCounter<long>(
            "inventory.stock.adjusted",
            unit: "units",
            description: "Number of stock adjustments.");
    }

    public void RecordStockAdjustment(string direction)
    {
        _stockAdjusted.Add(
            1,
            new TagList
            {
                { "direction", direction }
            });
    }
}
```

Register the metric class:

```csharp
services.AddSingleton<InventoryMetrics>();
```

The example assumes the shared `Meter` is already registered and managed by the Metrics infrastructure.

### 4.3 Metric Design Guidelines

* Use a counter for cumulative events or quantities that increase over time.
* Use a histogram for distributions such as processing duration or order value.
* Use an observable gauge when a current value should be sampled from an observation callback.
* Select units that accurately describe the measurement.
* Use a stable naming convention across bounded contexts.
* Document what each measurement represents and when it is recorded.

For `inventory.stock.adjusted`, the counter measures adjustment quantities only if the implementation adds the adjusted quantity. As currently written, `Add(1, ...)` counts adjustment events rather than the number of units adjusted. If the intended measurement is the quantity of stock changed, the method should accept and record that quantity instead.

---

## 5. Cardinality Rules

Metric cardinality must be controlled to prevent excessive time-series growth and unnecessary resource consumption.

### 5.1 Approved Dimensions

Use bounded, controlled dimensions such as:

* `operation`
* `outcome`
* `status`
* `customer.tier`
* `direction`

Values should come from defined sets wherever practical.

For example, an `outcome` dimension might use `success` and `failure`, rather than arbitrary exception messages.

### 5.2 Prohibited Dimensions

Do not use high-cardinality or sensitive values as metric dimensions, including:

* `CustomerId`
* `OrderId`
* `ProductId`
* Email addresses.
* Request identifiers or correlation IDs.
* Arbitrary exception messages.
* Raw URLs containing identifiers or other variable path values.

Exception type names should also be avoided as metric dimensions unless a carefully bounded classification is deliberately designed. Detailed exception diagnosis belongs in logs and, where configured, traces.

### 5.3 Review Requirements

Before adding a new metric or tag, verify:

1. The metric answers a meaningful operational or business question.
2. The instrument type matches the measurement.
3. Tag values are bounded and predictable.
4. No secrets or unnecessary personal information are recorded.
5. The metric is registered with the intended meter and export pipeline.

---

## 6. Testing and Verification

### 6.1 Local Verification

1. Start the application with the Metrics capability enabled.
2. Configure the OTLP exporter to target a local collector or another compatible metrics backend.
3. Trigger representative HTTP requests.
4. Execute successful and unsuccessful MediatR operations.
5. Trigger business events that record typed metrics.
6. Inspect the exported measurements and their dimensions.

Verify that the backend receives the expected metrics:

* `mediatr.operations`
* `mediatr.operations.duration`
* `orders.created`
* ASP.NET Core HTTP server metrics.

The metric names above represent the intended application-level names. Confirm the actual exported names and units against the implementation and backend, because instrumentation versions and exporters can affect metric representation.

### 6.2 Optional Console Exporter

A console exporter can be useful for local development and troubleshooting.

For versions of the OpenTelemetry .NET SDK that provide the corresponding extension, it can be configured as follows:

```csharp
using OpenTelemetry.Metrics;

metrics.AddConsoleExporter();
```

Add the exporter to the existing metrics-provider configuration rather than creating a second, independent provider.

The console exporter is optional and is not part of the required production telemetry path. Verify package availability and compatibility with the installed OpenTelemetry version before using it.

### 6.3 Optional Automated Testing

Automated metric verification can use `MeterListener` to observe measurements emitted by .NET metric instruments.

This can help verify:

* Instrument names and measurement values.
* Expected tags and tag values.
* Success and failure outcome classification.
* Recording behavior when operations succeed or fail.

Mocking `Meter` is not generally the preferred approach because `Meter` is a concrete instrumentation API. `MeterListener` or a test exporter provides a more direct way to inspect emitted measurements.

Automated testing is optional for initial integration but useful for verifying metric semantics and preventing accidental cardinality growth.

### 6.4 Manual Verification Checklist

* [ ] `AddApplicationMetrics()` is registered.
* [ ] The shared `ECommerceMeter` is included in the metrics provider.
* [ ] `TelemetryBehavior` is registered in MediatR.
* [ ] Typed metric classes are registered with appropriate lifetimes.
* [ ] HTTP instrumentation is enabled.
* [ ] The OTLP endpoint and protocol match the receiving collector.
* [ ] Expected metrics are emitted with the intended instruments and tags.
* [ ] No high-cardinality or sensitive dimensions are present.
* [ ] Business metrics reflect the intended business event or measurement.
* [ ] Application behavior remains correct if the telemetry backend is unreachable.

### 6.5 Backend Unavailability

An unavailable telemetry backend should not ordinarily cause business operations to fail solely because metrics cannot be exported.

However, the actual behavior depends on the exporter, SDK, buffering, and hosting configuration. Verify the behavior under the intended deployment conditions rather than assuming exporter failures are always harmless.

---

## 7. Summary

Integrating the Metrics capability follows a consistent workflow:

1. Register `AddApplicationMetrics()` in the composition root.
2. Configure the shared meter, OpenTelemetry provider, instrumentation, and OTLP exporter.
3. Register `TelemetryBehavior` in the MediatR pipeline.
4. Inject typed metric classes into handlers that need to record business measurements.
5. Use ASP.NET Core instrumentation for standard HTTP server metrics.
6. Keep metric dimensions bounded and avoid sensitive or high-cardinality values.
7. Verify exported measurements and ensure telemetry failures do not compromise business correctness.

The implementation uses `AddAspNetCoreInstrumentation()` for HTTP instrumentation and `AddOtlpExporter()` for OTLP export. Meter registration, instrumentation registration, and exporter configuration must work together as part of one coherent metrics pipeline.

**Core principle:** Metrics should provide useful operational and business measurements without introducing unnecessary coupling, uncontrolled cardinality, or dependencies that compromise application reliability.
