# Tracing — Integration Guide

**Project:** E-Commerce Modular Monolith
**Capability:** Cross-Cutting Observability — OpenTelemetry Tracing
**Version:** 1.0 — Final

---

## 1. Purpose

This guide explains how to integrate OpenTelemetry tracing into the E-Commerce modular monolith.

It covers:

* Registering the tracing infrastructure.
* Enabling automatic tracing for HTTP requests, MediatR operations, and background jobs.
* Creating optional custom spans for diagnostically significant operations.
* Configuring the OTLP exporter.
* Applying tracing best practices and security constraints.

The guide is intended for developers working across the application's modules and bounded contexts.

Tracing helps identify where time is spent during an operation, understand execution paths, and investigate failures across application components.

## 2. Prerequisites

Before integrating tracing, verify that the following components exist.

### 2.1 Infrastructure Components

The Infrastructure layer must contain:

* `ECommerceActivitySource.cs`
* `OpenTelemetryTraceContext.cs`
* `OpenTelemetrySpan.cs`
* `NullSpan.cs`
* `TracingRegistrationExtensions.cs`

### 2.2 Application Components

The Application layer must contain:

* `ITraceContext`
* `ITraceSpan`
* `TracingBehavior` under `Application/Shared/Behaviors`

The tracing abstractions allow Application components to create spans without depending directly on the Infrastructure implementation.

### 2.3 Required NuGet Packages

Install the packages required by the implementation:

* `OpenTelemetry.Extensions.Hosting`
* `OpenTelemetry.Exporter.OpenTelemetryProtocol`
* `OpenTelemetry.Instrumentation.AspNetCore`

Ensure that package versions are compatible with the application's target framework and with each other.

Additional packages may be necessary if the implementation adds instrumentation for other libraries or dependencies.

## 3. Step-by-Step Integration

### 3.1 Register Tracing Infrastructure

In `Program.cs`, register the tracing infrastructure through the existing Infrastructure extension:

```csharp
builder.Services.AddApplicationTracing();
```

The registration extension is responsible for configuring the tracing pipeline.

It should:

* Register `ITraceContext` with the lifetime required by its implementation, typically scoped when it depends on scoped services.
* Configure the OpenTelemetry tracing provider.
* Register `ECommerceActivitySource.Instance.Name` as an application activity source.
* Add ASP.NET Core instrumentation.
* Configure the OTLP exporter.

The exact registrations must match the implementation of `TracingRegistrationExtensions`.

**Important:** Registering an `ActivitySource` with the OpenTelemetry provider does not automatically create spans from that source. Application code must start activities, such as through `ITraceContext`, and the provider must subscribe to the corresponding source.

### 3.2 Register the MediatR Tracing Behavior

Register `TracingBehavior<,>` as an open generic MediatR pipeline behavior.

For example:

```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(/* application assemblies */);
    cfg.AddOpenBehavior(typeof(TracingBehavior<,>));
});
```

Replace the assembly placeholder with the application's actual MediatR registration configuration.

The behavior creates a span for each applicable MediatR request that passes through the registered pipeline.

This allows application operations to appear as spans within the current trace when a valid parent activity exists.

The behavior should:

* Use a consistent span naming convention.
* Record relevant operation attributes using bounded, non-sensitive values.
* Set span status consistently with the operation outcome.
* Preserve normal exception propagation.
* Avoid creating duplicate spans for the same operation.

### 3.3 Enable Background-Job Tracing

The existing `JobOrchestrator` is the central execution boundary for background jobs.

When the tracing integration is implemented, the orchestrator should create a span around each job execution, covering the appropriate execution policies and handler invocation.

The span should record:

* A stable job name or type.
* Execution outcome.
* Relevant execution metadata.
* Failure status when execution fails.

No separate tracing registration is needed for the job itself if the existing tracing abstractions and dependency injection configuration are already registered.

Verify that `ITraceContext` is injected into the `JobOrchestrator` and that its implementation correctly creates and disposes spans.

**Important distinction:** Creating a job span does not automatically propagate the original trace context into an independently scheduled background job. If a job must continue the trace of the operation that scheduled it, the scheduling and dispatching pipeline must explicitly preserve and restore the relevant trace context. Otherwise, the job may start a new trace.

### 3.4 Configure the OTLP Endpoint

Configure the OTLP endpoint through environment variables or another supported configuration mechanism.

For example:

```text
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
```

The endpoint must match the protocol and listener configuration of the receiving collector or backend.

Common OTLP defaults are:

* **gRPC:** Port `4317`.
* **HTTP/protobuf:** Port `4318`.

The actual protocol and endpoint depend on the exporter configuration.

If application configuration is used instead of environment variables, the registration code must explicitly bind those settings to the exporter.

For example, placing an endpoint in an `OpenTelemetry` section of `appsettings.json` does not automatically configure `.AddOtlpExporter()` unless the implementation reads and applies that configuration.

**Security requirements:**

* Do not hardcode production endpoints or credentials.
* Use environment variables or an appropriate secret-management mechanism for sensitive configuration.
* Verify that the configured endpoint is reachable from the deployment environment.

### 3.5 Optional: Create Custom Spans in Application Code

Create a custom span when an operation has diagnostic significance and is not already adequately represented by automatic instrumentation or an existing application span.

Inject `ITraceContext` into the relevant application service or handler.

The following example assumes that `ITraceContext.StartSpan` returns an `ITraceSpan` implementing `IDisposable`, and that `SetStatus(true, description)` means the span has failed.

```csharp
using E_Commerce.Application.Shared.Observability.Tracing;

public sealed class SomeService
{
    private readonly ITraceContext _trace;

    public SomeService(ITraceContext trace)
    {
        _trace = trace;
    }

    public async Task DoImportantWorkAsync(CancellationToken ct)
    {
        using var span = _trace.StartSpan(
            "some.important.work",
            new Dictionary<string, string?>
            {
                ["operation.name"] = "DoImportantWork"
            });

        try
        {
            // Perform the actual operation.
            await Task.Delay(100, ct);

            // Mark the span successful only if the abstraction
            // requires an explicit success status.
        }
        catch (Exception)
        {
            span.SetStatus(true, "Operation failed");
            throw;
        }
    }
}
```

**Implementation note:** The example follows the assumed custom abstraction. Verify its actual method signatures and status semantics before using it. In OpenTelemetry, span status is normally left unset for successful operations; an error status is used when an operation fails. If the wrapper requires explicit success marking, follow its documented contract.

Do not create spans for trivial methods. Prefer spans around operations that help diagnose execution paths, latency, dependency calls, or failures.

## 4. Automatic Tracing Provided by Infrastructure

The following tracing paths are handled by the corresponding instrumentation or application pipeline components.

| Operation                    | Mechanism                    | Responsibility                                                              |
| ---------------------------- | ---------------------------- | --------------------------------------------------------------------------- |
| HTTP request                 | ASP.NET Core instrumentation | Creates server spans for incoming HTTP requests.                            |
| MediatR request              | `TracingBehavior`            | Creates spans for requests passing through the registered MediatR pipeline. |
| Background job               | `JobOrchestrator`            | Creates spans around background-job execution when integrated.              |
| Custom application operation | `ITraceContext`              | Creates optional spans for explicitly instrumented operations.              |

### 4.1 Trace Relationships

These spans can participate in the same distributed trace when parent-child relationships and context propagation are correctly established.

For example:

```text
HTTP Server Span
    |
    +--> MediatR Request Span
              |
              +--> Custom Application Span
```

A background job may continue the originating trace when trace context is explicitly propagated through the scheduling and dispatching workflow. Otherwise, it may begin a separate trace.

Application and job spans use the application's configured `ActivitySource`. HTTP spans are created by ASP.NET Core instrumentation.

The OpenTelemetry provider collects spans from subscribed sources and sends them through the configured exporter.

## 5. Best Practices

### 5.1 Avoid Duplicate Spans

Do not create additional spans for operations already adequately covered by HTTP instrumentation, `TracingBehavior`, or `JobOrchestrator`.

Custom spans should provide useful diagnostic information rather than simply repeat existing execution boundaries.

### 5.2 Use Meaningful Names

Use stable, descriptive names that identify the operation.

Examples:

* `PlaceOrder`
* `ProcessPayment`
* `CreateShipment`
* `ProcessOutboxBatch`

Avoid names derived from arbitrary user input or individual entity identifiers.

### 5.3 Control Span Attributes

Prefer attributes with stable diagnostic meaning, such as:

* `operation.name`
* `request.type`
* `job.type`

Use controlled values where practical.

Identifiers such as `CustomerId`, `OrderId`, or `ProductId` can sometimes be useful during targeted diagnostics, but they should not automatically become standard attributes on every span. Their use must account for privacy, data-retention, and telemetry-volume requirements.

### 5.4 Protect Sensitive Information

Never attach the following to spans:

* Passwords or password hashes.
* Authentication tokens or session secrets.
* OTPs or verification codes.
* Payment credentials.
* Unnecessary personal information.
* Request or response bodies containing sensitive data.

Review span attributes before adding them to shared instrumentation.

### 5.5 Record Failures Safely

Set span status appropriately when an operation fails.

Prefer a generic description such as `Operation failed` over exception messages that might contain sensitive information. Detailed exception diagnostics belong in the logging system, subject to its own data-protection rules.

Preserve normal exception propagation; tracing must not suppress application failures.

### 5.6 Keep Tracing Independent of Business Correctness

Tracing is an observability capability, not part of business invariants.

Ordinary telemetry collection or export failures should not cause business operations to fail. Verify this behavior with the actual SDK, exporter, and hosting configuration.

## 6. Security and Cardinality

Tracing attributes require deliberate review.

Apply the following rules:

* Do not expose secrets or sensitive personal data.
* Prefer stable operation names and controlled attribute values.
* Avoid adding unbounded identifiers to every span by default.
* Do not confuse metric cardinality with trace attribute cardinality: they have different performance and storage implications.
* Use identifiers in traces only when there is a clear diagnostic benefit and an acceptable privacy and retention policy.
* Avoid recording exception messages or arbitrary user-controlled strings as routine attributes.

Unlike metrics, traces can sometimes tolerate higher-cardinality attributes for diagnostic purposes, but high-cardinality and sensitive data still carry storage, cost, and privacy risks.

## 7. Testing and Verification

Verify the tracing capability in a development or test environment.

### 7.1 Span Collection

Confirm that spans appear in the configured observability backend for:

* Incoming HTTP requests.
* MediatR operations.
* Background-job executions.
* Custom application spans that are explicitly created.

### 7.2 Trace Relationships

Verify that spans have the expected parent-child relationships when operations occur within the same execution flow.

For background jobs, test context propagation separately if a job is expected to continue the trace of the operation that scheduled it.

### 7.3 Failure Handling

Verify that:

* Failed operations are marked appropriately.
* Exceptions propagate according to normal application behavior.
* Span disposal occurs on both success and failure.
* Tracing does not alter business results.
* The application continues to operate when the OTLP backend is unreachable.

### 7.4 Security

Inspect exported spans and confirm that they do not contain passwords, tokens, OTPs, payment credentials, or unnecessary personal information.

## 8. Summary

Integrating tracing involves:

1. Registering the tracing infrastructure through `AddApplicationTracing()`.
2. Registering `TracingBehavior<,>` in the MediatR pipeline.
3. Ensuring `JobOrchestrator` creates spans for background-job execution.
4. Creating custom spans through `ITraceContext` only when diagnostically useful.
5. Configuring the OTLP endpoint through environment variables or explicitly bound application configuration.
6. Verifying trace relationships, failure handling, and sensitive-data protection.

This design centralizes tracing infrastructure, keeps Application independent of the Infrastructure implementation, and aligns with the existing modular monolith architecture.

**Core principle:** Trace meaningful execution boundaries, preserve context where appropriate, and ensure observability never becomes a dependency of business correctness.
