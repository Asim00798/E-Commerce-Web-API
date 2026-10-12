\# Logging — Workflow Technical Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Capability:\*\* Cross-Cutting Observability — Structured Logging

\*\*Version:\*\* 1.0 — Final



\---



\## 1. Overview



This document describes the end-to-end workflows of the Logging capability within the E-Commerce modular monolith.



It covers:



\* HTTP request correlation and request logging.

\* Application-operation logging through the MediatR pipeline.

\* Performance monitoring and slow-operation warnings.

\* Integration-event correlation during asynchronous processing.

\* Business and technical logging from application handlers and services.

\* Log enrichment and output to configured sinks.



The Logging capability operates independently of metrics, distributed tracing, and health checks. However, its correlation identifiers and structured metadata can help associate related diagnostic information across these capabilities.



\---



\## 2. Actors and Components



\### 2.1 Actors



| Actor               | Description                                                                        |

| ------------------- | ---------------------------------------------------------------------------------- |

| Client              | Sends HTTP requests to the API.                                                    |

| Application Handler | Executes application use cases through commands and queries.                       |

| Background Job      | Executes asynchronous work, including Hangfire jobs.                               |

| Integration Event   | Represents a cross-context notification processed by an integration-event handler. |

| Operations Team     | Uses application logs for troubleshooting, diagnosis, and operational monitoring.  |



\### 2.2 Key Components



| Component                                 | Layer          | Responsibility                                                  |

| ----------------------------------------- | -------------- | --------------------------------------------------------------- |

| `CorrelationIdMiddleware`                 | API            | Establishes request correlation and logging scope.              |

| `SerilogRequestLogging`                   | API            | Automatically logs HTTP request completion details.             |

| `LoggingBehavior`                         | Application    | Logs application-operation identity and user context.           |

| `PerformanceBehavior`                     | Application    | Measures operation duration and warns about slow operations.    |

| `ILogger<T>`                              | Application    | Provides structured logging for handlers and services.          |

| `CorrelationScopeIntegrationEventHandler` | Application    | Restores correlation context for integration-event handlers.    |

| `ExecutionContext` (`IAppContext`)        | Infrastructure | Provides execution metadata through an Application abstraction. |

| `LoggingExtensions`                       | Infrastructure | Configures Serilog and registers logging infrastructure.        |

| Serilog sinks                             | Infrastructure | Write structured log events to configured destinations.         |



\---



\## 3. Workflow 1: HTTP Request Logging



\### 3.1 Description



Every incoming HTTP request receives a correlation identifier. That identifier is added to the request's logging scope so that log events generated during request processing can be associated with the same execution.



Serilog's request-logging middleware records the HTTP request summary when processing completes.



\### 3.2 Flow



```text

Client

&#x20; |

&#x20; | HTTP Request

&#x20; v

CorrelationIdMiddleware

&#x20; |

&#x20; +--> Read and validate X-Correlation-ID

&#x20; |       |

&#x20; |       +--> Generate a GUID if absent or invalid

&#x20; |

&#x20; +--> Store CorrelationId in HttpContext.Items

&#x20; |

&#x20; +--> Set X-Correlation-ID response header

&#x20; |

&#x20; +--> Resolve UserId or use "anonymous"

&#x20; |

&#x20; +--> Begin logging scope

&#x20;         |

&#x20;         | CorrelationId + UserId

&#x20;         v

SerilogRequestLogging

&#x20; |

&#x20; +--> Records HTTP request completion

&#x20; |

&#x20; v

Downstream Pipeline

&#x20; |

&#x20; +--> MediatR Behaviors

&#x20; |

&#x20; +--> Application Handlers and Services

```



\### 3.3 Important Rules



\* If `CorrelationIdMiddleware` extracts `UserId` from `HttpContext.User`, authentication must run before the middleware.

\* `UseSerilogRequestLogging()` must execute after the correlation middleware so that the request-completion log can inherit the correlation scope.

\* The middleware must validate client-provided correlation identifiers before accepting them.

\* The response header must contain the identifier resolved for the current request.

\* Handlers must not duplicate the automatic HTTP request-completion log with unnecessary manual request logging.



\---



\## 4. Workflow 2: Application-Operation Logging



\### 4.1 Description



When a MediatR command or query is executed, `LoggingBehavior` records the operation name and the current user identifier when available.



This provides consistent application-level visibility into which use cases are being invoked without requiring every handler to implement the same operation-start logging.



\### 4.2 Flow



```text

MediatR Request

&#x20;     |

&#x20;     v

LoggingBehavior

&#x20;     |

&#x20;     +--> Determine request name

&#x20;     |       typeof(TRequest).Name

&#x20;     |

&#x20;     +--> Resolve UserId when available

&#x20;     |

&#x20;     +--> Log operation identity

&#x20;     |       RequestName + UserId

&#x20;     |

&#x20;     v

Next Pipeline Behavior / Handler

```



Example structured log:



```csharp

\_logger.LogInformation(

&#x20;   "Request {RequestName} initiated by user {UserId}",

&#x20;   typeof(TRequest).Name,

&#x20;   userId);

```



\### 4.3 Important Rules



\* `LoggingBehavior` does not measure execution duration.

\* It does not independently log normal completion or failure.

\* It must not serialize the complete request object.

\* The log event should contain the operation name and approved contextual metadata only.

\* The user identifier should reflect the current execution context and must not be inferred from untrusted request data.



\---



\## 5. Workflow 3: Performance Monitoring



\### 5.1 Description



`PerformanceBehavior` measures the execution duration of each MediatR request and emits a warning when the elapsed time exceeds the configured threshold.



This provides a consistent mechanism for identifying potentially slow application operations.



\### 5.2 Flow



```text

MediatR Request

&#x20;     |

&#x20;     v

PerformanceBehavior

&#x20;     |

&#x20;     +--> Start Stopwatch

&#x20;     |

&#x20;     +--> Execute Next Pipeline Component

&#x20;     |       |

&#x20;     |       +--> Handler Execution

&#x20;     |

&#x20;     +--> Measure Elapsed Duration

&#x20;     |

&#x20;     +--> Compare Against Threshold

&#x20;             |

&#x20;             +--> Within Threshold

&#x20;             |       |

&#x20;             |       +--> No Performance Log

&#x20;             |

&#x20;             +--> Exceeds Threshold

&#x20;                     |

&#x20;                     +--> Log Warning

&#x20;                             RequestName

&#x20;                             ElapsedMilliseconds

&#x20;                             UserId

```



\### 5.3 Important Rules



\* Only slow-operation warnings are emitted by this behavior.

\* Normal completion and failure are not logged by this behavior solely for performance-monitoring purposes.

\* The current threshold is \*\*500 milliseconds\*\*. It may be made configurable.

\* The warning must include the request name, elapsed duration, and user identifier when available.

\* Exceptions must propagate normally. Timing instrumentation must not suppress the original exception.

\* The measured duration depends on MediatR behavior ordering and should be interpreted accordingly.



\### 5.4 Relationship with LoggingBehavior



The two behaviors have separate responsibilities:



| Behavior              | Responsibility                                                          |

| --------------------- | ----------------------------------------------------------------------- |

| `LoggingBehavior`     | Records which application operation started.                            |

| `PerformanceBehavior` | Identifies operations whose execution exceeds the configured threshold. |



Neither behavior replaces meaningful business or technical logging within application handlers and services.



\---



\## 6. Workflow 4: Integration-Event Correlation



\### 6.1 Description



When an integration event is processed asynchronously, the correlation identifier associated with the originating operation is restored into the logging scope.



This allows logs generated during event handling to be associated with the originating request when the correlation metadata has been propagated correctly.



For example, an order-related operation may produce an outbox message that is later processed by a background job. The event handler can restore the original correlation identifier before executing its business logic.



\### 6.2 Flow



```text

Originating Request or Operation

&#x20;     |

&#x20;     v

Integration Event Created

&#x20;     |

&#x20;     +--> CorrelationId Propagated

&#x20;     |

&#x20;     v

Outbox Persistence

&#x20;     |

&#x20;     v

Background Processing

&#x20;     |

&#x20;     v

Integration Event Dispatch

&#x20;     |

&#x20;     v

CorrelationScopeIntegrationEventHandler

&#x20;     |

&#x20;     +--> Read CorrelationId from Event or Envelope

&#x20;     |

&#x20;     +--> Begin Logging Scope if Available

&#x20;     |

&#x20;     v

Inner Integration-Event Handler

&#x20;     |

&#x20;     +--> Execute Business Logic

&#x20;     |

&#x20;     +--> Emit Structured Logs

&#x20;     |

&#x20;     v

Logging Scope Disposed

```



\### 6.3 Important Rules



\* The publisher must propagate the correlation identifier when one exists.

\* The decorator must read the identifier from the integration event or its associated envelope.

\* If no correlation identifier exists, the decorator must not create an additional scope solely for that missing value.

\* The decorator must not change business behavior or event-processing semantics.

\* The logging scope must be disposed after the handler finishes, including when execution fails.

\* Correlation IDs associate related log events but do not, by themselves, establish distributed tracing.



\---



\## 7. Workflow 5: Business-Specific Logs



\### 7.1 Description



Application handlers and services can inject `ILogger<T>` to record meaningful business and technical events.



These logs supplement automatic request and operation logging by capturing information specific to the activity being performed.



\### 7.2 Flow



```text

Application Handler / Service

&#x20;     |

&#x20;     v

ILogger<T>

&#x20;     |

&#x20;     +--> Information

&#x20;     |       Business milestone or state transition

&#x20;     |

&#x20;     +--> Warning

&#x20;     |       Unexpected but recoverable condition

&#x20;     |

&#x20;     +--> Error

&#x20;             Failed operation requiring diagnosis

&#x20;     |

&#x20;     v

Serilog Pipeline

&#x20;     |

&#x20;     v

Configured Sinks

```



Examples:



```csharp

\_logger.LogInformation(

&#x20;   "Order {OrderId} was placed",

&#x20;   orderId);



\_logger.LogWarning(

&#x20;   "Product {ProductId} has low stock",

&#x20;   productId);



\_logger.LogError(

&#x20;   exception,

&#x20;   "Payment processing failed for order {OrderId}",

&#x20;   orderId);

```



These examples illustrate structured logging. The actual events and their severity should reflect the meaning of the operation and the application's error-handling policy.



\### 7.3 Important Rules



\* Use structured message templates with named properties.

\* Log meaningful business milestones, state transitions, and technical conditions.

\* Avoid excessive, repetitive, or trivial log entries.

\* Never log passwords, OTPs, authentication tokens, API keys, payment credentials, or other secrets.

\* Avoid logging complete request objects, unnecessary personal data, or sensitive third-party response payloads.

\* Include relevant identifiers where doing so is useful and permitted.



\---



\## 8. Workflow 6: Log Enrichment and Output



\### 8.1 Description



Log events pass through the configured Serilog pipeline, where global enrichment and available execution-context properties are attached before the events are written to their configured sinks.



\### 8.2 Flow



```text

Application or Infrastructure Log Event

&#x20;     |

&#x20;     v

Serilog Pipeline

&#x20;     |

&#x20;     +--> Global Enrichment

&#x20;     |       AppName

&#x20;     |       Environment

&#x20;     |       Version

&#x20;     |       MachineName

&#x20;     |

&#x20;     +--> Request / Execution Scope

&#x20;     |       CorrelationId

&#x20;     |       UserId

&#x20;     |

&#x20;     +--> Trace Metadata, When Configured

&#x20;     |       TraceId

&#x20;     |       SpanId

&#x20;     |

&#x20;     v

Configured Sinks

&#x20;     |

&#x20;     +--> Console

&#x20;     |

&#x20;     +--> Rolling File

&#x20;     |

&#x20;     +--> Optional Centralized Logging Platform

```



\### 8.3 Important Rules



\* Log output must remain structured and machine-readable.

\* Sink configuration must be deliberate to avoid duplicate log output.

\* Global metadata should be populated through the configured enrichment mechanism.

\* Request-scoped properties should be included only when their context is available.

\* `TraceId` and `SpanId` require appropriate tracing and enrichment configuration; correlation middleware alone does not provide them.

\* Sink configuration must not introduce Serilog-specific dependencies into the Application or Domain layers.



\---



\## 9. Workflow 7: Background Jobs Without an HTTP Request



\### 9.1 Description



Background jobs may execute independently of an HTTP request. In these cases, no active `HttpContext` or authenticated user may be available.



The logging pipeline must still provide useful execution context without assuming that every job originates from an API request.



\### 9.2 Flow



```text

Hangfire Background Job

&#x20;     |

&#x20;     v

Job Execution

&#x20;     |

&#x20;     +--> Establish or Restore Available Correlation Context

&#x20;     |

&#x20;     +--> Execute Job Handler

&#x20;     |

&#x20;     +--> Log Meaningful Events through ILogger<T>

&#x20;     |

&#x20;     v

Serilog Pipeline

&#x20;     |

&#x20;     +--> Global Metadata

&#x20;     |

&#x20;     +--> Available Job / Correlation Properties

&#x20;     |

&#x20;     v

Configured Sinks

```



\### 9.3 Important Rules



\* `IAppContext` must handle the absence of an active HTTP request.

\* A background job must not fail merely because HTTP-specific execution metadata is unavailable.

\* When a job processes an integration event, the original correlation identifier should be restored if it was propagated.

\* Independently scheduled jobs may establish their own execution correlation identifier where useful.

\* Correlation context and job identifiers serve different purposes and should not be treated as interchangeable.



\---



\## 10. Summary



The Logging capability consists of complementary workflows that provide consistent diagnostic visibility throughout the modular monolith.



\* \*\*HTTP request logging\*\* establishes request correlation and records request-completion details.

\* \*\*Application-operation logging\*\* records the identity of commands and queries.

\* \*\*Performance monitoring\*\* identifies operations that exceed the configured execution threshold.

\* \*\*Integration-event correlation\*\* carries diagnostic context into asynchronous processing.

\* \*\*Business-specific logging\*\* captures meaningful events from handlers and services.

\* \*\*Log enrichment and sinks\*\* add contextual metadata and deliver structured events to configured destinations.

\* \*\*Background-job logging\*\* supports useful diagnostic context even when no HTTP request exists.



Together, these workflows provide structured and security-conscious logging while preserving architectural boundaries and keeping the logging capability focused on operational diagnostics.



\*\*Core principle:\*\* Every workflow should contribute useful diagnostic information without duplicating responsibilities, exposing sensitive data, or making business execution unnecessarily dependent on logging infrastructure.



