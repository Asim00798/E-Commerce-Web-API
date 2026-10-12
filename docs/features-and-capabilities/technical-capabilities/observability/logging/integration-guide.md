# Logging — Integration Guide

**Project:** E-Commerce Modular Monolith
**Purpose:** How to Integrate Structured Logging into the Application
**Version:** 1.0 — Final

---

## 1. Purpose

This guide explains how to integrate and use the structured logging capability within the E-Commerce modular monolith.

It covers:

* Registering Serilog and the logging infrastructure.
* Configuring correlation IDs and HTTP request logging.
* Using `ILogger<T>` in Application handlers and services.
* Applying automatic application-operation logging through MediatR.
* Applying performance monitoring through MediatR.
* Correlating integration-event handlers.
* Protecting sensitive information from being written to logs.

The guide is intended for developers adding or using logging within any bounded context or module.

## 2. Prerequisites

Before integrating logging, ensure that:

* `LoggingExtensions` and the execution-context implementation exist in Infrastructure.
* `CorrelationIdMiddleware` exists in the API layer.
* `IAppContext` and `ContextKeys` are defined in Application.
* `LoggingBehavior` and `PerformanceBehavior` are implemented.
* The required NuGet packages are installed:

  * `Serilog.AspNetCore`
  * `Serilog.Sinks.Console`
  * `Serilog.Sinks.File`

Install only the packages required by the actual logging configuration.

## 3. Step-by-Step Integration

### 3.1 Register Serilog at Startup

In `Program.cs`, register the logging infrastructure before building the application:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructureLogging();
```

The `AddInfrastructureLogging()` extension method is responsible for configuring the logging infrastructure.

According to the intended implementation, it:

* Reads logging configuration from `IConfiguration`.
* Adds global enrichers such as `AppName`, `Environment`, `Version`, and `MachineName`.
* Configures the console and rolling-file sinks.
* Configures Serilog as the application's logging provider through `builder.Host.UseSerilog()`.

This centralizes logging configuration in Infrastructure rather than coupling Application code to Serilog.

### 3.2 Configure Authentication and Correlation Middleware

Add the correlation middleware to the API pipeline:

```csharp
app.UseMiddleware<CorrelationIdMiddleware>();
```

The middleware establishes the correlation context for the current HTTP request and returns the correlation ID through the `X-Correlation-ID` response header.

**Middleware ordering matters.**

If `CorrelationIdMiddleware` reads `HttpContext.User` to obtain the authenticated user ID, authentication must run before that lookup. However, the middleware should still run before request logging so the correlation ID can be included in request log events.

A suitable ordering is:

```csharp
app.UseRouting();

app.UseAuthentication();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSerilogRequestLogging();

app.UseAuthorization();
```

Place the remaining middleware and endpoint mappings according to the application's established API pipeline.

This ordering assumes that the correlation middleware requires the authenticated principal. If the middleware does not depend on authentication, it can run earlier to establish correlation for more of the request pipeline.

### 3.3 Add Serilog Request Logging

Register Serilog request logging:

```csharp
app.UseSerilogRequestLogging();
```

Place it after the correlation middleware so that request-completion log events can inherit the correlation context.

Serilog request logging provides a consolidated event for each completed HTTP request, including information such as the request method, path, response status code, and elapsed time.

Avoid adding redundant request-logging middleware unless there is a specific requirement for separate events.

### 3.4 Register MediatR Behaviors

Ensure `LoggingBehavior` and `PerformanceBehavior` are registered in the MediatR pipeline.

Example:

```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        /* Application assemblies */);

    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
});
```

Replace the assembly placeholder with the actual Application assemblies used by the project.

Register the behaviors as open generic pipeline behaviors so they can apply to the relevant MediatR requests.

Their responsibilities are distinct:

| Behavior              | Responsibility                                                                         |
| --------------------- | -------------------------------------------------------------------------------------- |
| `LoggingBehavior`     | Records application-operation identity and relevant execution metadata.                |
| `PerformanceBehavior` | Measures operation duration and warns when execution exceeds the configured threshold. |

The relative order of these behaviors and other pipeline behaviors should follow the application's established pipeline policy.

**Important:** Ensure the logging behavior does not serialize or log complete command and query objects automatically. Request names and carefully selected metadata are generally sufficient.

### 3.5 Use `ILogger<T>` in Application Code

Inject `ILogger<T>` into Application handlers and services when meaningful business or technical logs are needed.

Example:

```csharp
public sealed class PlaceOrderCommandHandler
    : IRequestHandler<PlaceOrderCommand, Result<Guid>>
{
    private readonly ILogger<PlaceOrderCommandHandler> _logger;

    public PlaceOrderCommandHandler(
        ILogger<PlaceOrderCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        PlaceOrderCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Placing order for customer {CustomerId}",
            command.CustomerId);

        // Execute the application operation.

        return Result<Guid>.Success(orderId);
    }
}
```

The example illustrates structured logging; `orderId` represents the result produced by the actual implementation.

Use structured message templates rather than string interpolation:

```csharp
_logger.LogWarning(
    "Product {ProductId} stock is below reorder threshold",
    productId);
```

The named placeholders become structured log properties, allowing supported logging platforms to filter and search events by those values.

Choose log statements carefully so that they provide useful operational context without duplicating information already recorded by the logging pipeline.

### 3.6 Access Execution Metadata Through `IAppContext`

Inject `IAppContext` when Application logic genuinely needs execution metadata, such as the correlation ID or current user ID.

Example:

```csharp
public sealed class SomeService
{
    private readonly IAppContext _appContext;

    public SomeService(IAppContext appContext)
    {
        _appContext = appContext;
    }

    public void DoSomething()
    {
        var correlationId = _appContext.CorrelationId;
        var userId = _appContext.UserId;

        // Use execution metadata when required.
    }
}
```

Do not inject `IAppContext` solely to enrich logs. Prefer the established logging scope and enrichment mechanisms for that purpose.

This keeps Application code focused on application behavior rather than logging infrastructure.

### 3.7 Correlate Integration-Event Handlers

The `CorrelationScopeIntegrationEventHandler` decorator restores the correlation ID from integration-event metadata into the logging scope.

Concrete integration-event handlers should not need to implement this behavior individually.

Register the decorator through dependency injection:

```csharp
services.Decorate(
    typeof(IIntegrationEventHandler<>),
    typeof(CorrelationScopeIntegrationEventHandler<>));
```

This example assumes the relevant integration-event handlers are registered before decoration and that the decorator's generic shape matches the registered handler abstraction.

For correlation to work across asynchronous boundaries, the publisher must propagate the correlation ID in the integration-event metadata, and the receiving handler must restore it into the execution scope.

If an event has no correlation ID, the implementation should follow the application's defined fallback policy rather than assuming a value is always present.

## 4. Adding Business Logs — Best Practices

Business logs should describe meaningful events and operational conditions, not reproduce every internal method call.

### 4.1 Log Meaningful Events

Examples include:

* An order is placed or cancelled.
* A payment attempt fails.
* A product's stock falls below a threshold.
* A background operation fails.
* An integration-event handler encounters a recoverable or unexpected condition.

Avoid logging every successful internal operation when the same information is already available through automatic operation logging or other telemetry.

### 4.2 Use Structured Message Templates

Prefer:

```csharp
_logger.LogWarning(
    "Product {ProductId} stock is below reorder threshold",
    productId);
```

Avoid:

```csharp
_logger.LogWarning(
    $"Product {productId} stock is below reorder threshold");
```

Structured templates preserve values as properties when supported by the configured logging provider and sink.

### 4.3 Include Useful Identifiers

Include identifiers such as `OrderId`, `ProductId`, or `UserId` when they help diagnose an operation.

Do not confuse structured logging with metrics: high-cardinality identifiers can be useful as log properties but should generally not be used as metric labels.

### 4.4 Choose Appropriate Log Levels

| Level         | Intended Use                                                                              |
| ------------- | ----------------------------------------------------------------------------------------- |
| `Debug`       | Detailed diagnostic information useful during investigation.                              |
| `Information` | Significant application or operational events during normal execution.                    |
| `Warning`     | Unexpected conditions that do not necessarily fail the operation.                         |
| `Error`       | Failures that prevent an operation from completing successfully or require investigation. |

Use levels consistently so operators can distinguish routine activity from actionable failures.

### 4.5 Avoid Duplicate Logs

Avoid logging the same exception independently in every layer as it propagates.

Log failures at a point where meaningful context is available and where the failure can be handled or reported appropriately. Preserve exception details when logging an exception, but never expose them indiscriminately to API consumers.

## 5. Performance Monitoring

`PerformanceBehavior` measures the execution duration of MediatR requests and logs warnings for operations that exceed the configured threshold.

The current implementation uses a **500 ms threshold**.

No additional instrumentation is required in individual handlers for this behavior to operate.

To change the threshold, modify the behavior or make it configurable through application configuration in a future improvement.

**Important:** The 500 ms threshold is an application-operation warning threshold, not a universal definition of a slow request. Some legitimate operations may take longer, while performance-sensitive operations may require a lower threshold.

The behavior should also avoid treating expected business failures represented by a result value as unhandled technical exceptions unless that is an explicit logging policy.

## 6. Sensitive Data Protection

Logging must not expose secrets or unnecessary personal information.

Follow these rules:

* Never log passwords, one-time passwords (OTPs), access tokens, refresh tokens, API keys, or payment credentials.
* Never log complete authentication requests or other objects containing sensitive fields.
* Avoid logging full request or response bodies by default.
* Do not log payment card data or other sensitive financial information.
* Avoid logging full personal-data records when a stable identifier is sufficient.
* Review log statements and automatic enrichers before committing changes.
* Ensure exception messages and external-provider responses are not forwarded blindly if they may contain secrets or personal information.
* Apply appropriate access controls and retention policies to log storage.

If a DTO must be logged, explicitly select safe fields rather than relying on the DTO's current shape remaining free of sensitive data.

## 7. Testing and Verification

After integration, run the application and verify the following behavior.

### 7.1 Correlation ID

* Send an HTTP request.
* Confirm that the response contains the `X-Correlation-ID` header.
* Confirm that the correlation ID appears in the corresponding request logs.
* Verify that a valid incoming correlation ID is handled according to the middleware's validation and trust policy.

### 7.2 User Context

* Send an authenticated request.
* Confirm that the expected `UserId` is included in logs where configured.
* Verify that anonymous requests are handled without errors.
* Ensure that authentication and middleware ordering allows the user principal to be available when required.

### 7.3 MediatR Logging

* Execute a representative command or query.
* Confirm that `LoggingBehavior` records the request name and approved execution metadata.
* Confirm that the behavior does not log the complete request object or sensitive fields.

### 7.4 Performance Monitoring

* Execute an operation that completes below the threshold.
* Confirm that no slow-operation warning is emitted solely because of duration.
* Execute a controlled operation that exceeds the threshold.
* Confirm that `PerformanceBehavior` emits a warning with useful operation identity and elapsed time.

### 7.5 Integration-Event Correlation

* Publish an integration event containing a correlation ID.
* Verify that the receiving handler's log events contain the restored correlation ID.
* Verify that missing or invalid correlation metadata follows the defined fallback policy.

### 7.6 Log Sinks

* Confirm that structured logs appear in the console.
* Confirm that rolling log files are created when file logging is enabled.
* Verify that log properties remain structured in the configured sinks.
* Confirm that no sensitive data is written to either destination.

## 8. Summary

Integrating structured logging involves six main steps:

1. Register Serilog through the Infrastructure logging extension.
2. Configure correlation middleware and HTTP request logging.
3. Register the MediatR logging and performance behaviors.
4. Use `ILogger<T>` for meaningful business and technical events.
5. Use `IAppContext` only when application logic genuinely requires execution metadata.
6. Rely on correlation-scope propagation for integration-event handlers and other execution paths where correlation metadata is propagated.

This design provides consistent, structured, and correlated logging without coupling Application code to Serilog or a particular log-storage platform. It supports operational troubleshooting while preserving the modular monolith's architectural boundaries and sensitive-data protections.
