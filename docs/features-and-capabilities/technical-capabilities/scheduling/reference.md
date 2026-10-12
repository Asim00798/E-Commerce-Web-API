# Scheduling — Technical Reference Document

**Project:** E-Commerce Modular Monolith
**Feature:** Scheduling / Background Job Execution Gateway
**Version:** 1.0 — Final

---

## 1. Overview

The Scheduling feature provides the abstraction through which the application schedules, dispatches, and executes background work. It is the integration point between background execution requirements in the Application layer and the concrete scheduler in Infrastructure.

The feature has three primary responsibilities:

* **Job abstraction:** Contracts for job payloads, handlers, execution context, and scheduling.
* **Execution gateway:** `JobOrchestrator`, which resolves handlers and composes execution policies.
* **Scheduler adapter:** The Hangfire implementation of the scheduling and dispatch contracts.

Scheduling does not own business logic. Job payloads, handlers, and recurring triggers are colocated with the features they serve, while the Scheduling module owns the shared execution pipeline.

### 1.1 Architectural Position

```text
APPLICATION
    |
    +-- IJob
    +-- IJobHandler<TJob>
    +-- IJobContext
    +-- IJobScheduler
    +-- IJobOrchestrator
    +-- IRecurringJobTrigger
    +-- RecurringJobAttribute
    |
    +-- JobOrchestrator
    +-- IJobPolicy
    +-- TimeoutPolicy
    +-- CircuitBreakerPolicy
    |
    v
INFRASTRUCTURE
    |
    +-- HangfireJobScheduler
    +-- HangfireJobDispatcher
    +-- JobExecutionContext
    +-- RecurringJobBootstrapper
    |
    v
HANGFIRE RUNTIME
    |
    v
SQL SERVER STORAGE
```

The Application layer does not reference Hangfire. Infrastructure implements the scheduling integration and does not own the business meaning of individual jobs.

These boundaries are enforced through project references and dependency-injection configuration.

### 1.2 Design Principles

The Scheduling feature makes background execution a first-class architectural concern.

The intended design ensures that:

* Jobs are dispatched through a consistent Application-facing contract.
* Executions pass through a common orchestration gateway.
* Configured execution policies are applied centrally.
* Logging and tracing are handled consistently.
* Handlers and recurring triggers are discovered automatically from configured assemblies.
* Business logic remains within the feature that owns it.
* Hangfire-specific APIs remain within Infrastructure.

The architecture centralizes execution behavior without requiring every job to implement its own scheduler integration.

---

## 2. Architectural Decisions

### 2.1 Why a Scheduling Abstraction?

Hangfire provides persistent job storage, scheduling, retry behavior, and background worker execution.

The application uses these capabilities through an abstraction rather than allowing Application code to depend directly on Hangfire.

Without this boundary:

* Handlers could become coupled to Hangfire runtime APIs.
* Execution policies could be duplicated across jobs.
* Logging and correlation could become inconsistent.
* Replacing or adapting the scheduling implementation would affect application code.

The Scheduling feature defines the Application-facing contracts, while Infrastructure supplies their Hangfire-backed implementation.

This reduces coupling, although replacing Hangfire may still require changes to Infrastructure configuration, persisted job formats, scheduling semantics, and runtime-specific behavior.

### 2.2 Why an Execution Gateway?

A direct scheduler-to-handler model would leave cross-cutting execution responsibilities distributed across individual handlers.

With many background jobs, that duplication becomes a maintenance concern.

`JobOrchestrator` centralizes:

* Handler resolution.
* Policy composition.
* Execution logging.
* Execution context propagation.
* Consistent failure propagation.
* Trace integration, when configured.

Handlers focus on the work owned by their feature.

### 2.3 Why Automatic Trigger Discovery?

Manual registration of recurring jobs can cause registration drift:

* A trigger is implemented but never registered.
* A recurring schedule is registered separately from its implementation.
* A renamed or removed trigger leaves an outdated schedule behind.

The Scheduling feature reduces these risks by discovering recurring triggers at startup and reading their `[RecurringJob]` attributes.

The bootstrapper registers the discovered schedules with Hangfire.

**Important:** Automatic discovery removes routine manual registration, but it does not eliminate the need to configure scanning assemblies, validate attributes, manage obsolete recurring IDs, and ensure startup registration is safe across multiple application instances.

---

## 3. Architecture at a Glance

```text
APPLICATION
    |
    +-- IJob
    +-- IJobHandler<TJob>
    +-- IJobContext
    +-- IJobScheduler
    +-- IJobOrchestrator
    +-- IRecurringJobTrigger
    |
    +-- JobOrchestrator
    |       |
    |       +-- Resolve handler
    |       +-- Compose policies
    |       +-- Execute handler
    |       +-- Log outcome
    |
    +-- IJobPolicy
    |       |
    |       +-- TimeoutPolicy
    |       +-- CircuitBreakerPolicy
    |
    v
INFRASTRUCTURE
    |
    +-- HangfireJobScheduler
    |       +-- Enqueue
    |       +-- Schedule
    |       +-- AddOrUpdateRecurring
    |
    +-- HangfireJobDispatcher
    |       +-- Obtain PerformContext
    |       +-- Build execution context
    |       +-- Call IJobOrchestrator
    |
    +-- RecurringJobBootstrapper
            +-- Scan trigger implementations
            +-- Read [RecurringJob]
            +-- Register recurring schedules
    |
    v
HANGFIRE RUNTIME
    |
    v
SQL SERVER STORAGE
```

The diagram describes the logical responsibilities. The actual method-call order and registration details are determined by the implementation.

---

## 4. Concept Separation

The Scheduling feature separates responsibilities that are frequently conflated in background execution systems.

| Concept           | Responsibility                                              | Architectural location                                        |
| ----------------- | ----------------------------------------------------------- | ------------------------------------------------------------- |
| Job payload       | Carries the data required for execution                     | Application                                                   |
| Job handler       | Executes the feature-owned operation                        | Application or owning feature layer                           |
| Job context       | Provides runtime metadata such as job ID and attempt number | Application contract; Infrastructure implementation           |
| Job policy        | Wraps execution with cross-cutting behavior                 | Application                                                   |
| Recurring trigger | Enqueues a payload when a recurring schedule fires          | Application or Infrastructure, according to feature ownership |
| Scheduler         | Enqueues and schedules jobs                                 | Infrastructure adapter                                        |
| Dispatcher        | Bridges Hangfire execution to the Application gateway       | Infrastructure                                                |
| Orchestrator      | Resolves the handler and executes the policy chain          | Application                                                   |

### 4.1 Payload vs. Handler

A job is represented by two distinct concepts.

**Payload — `IJob`**

* A data-only object containing execution parameters.
* Serialized by the configured Hangfire serializer.
* Persisted as part of the scheduled job representation.
* Deserialized when the job executes.

**Handler — `IJobHandler<TJob>`**

* Contains the operation's execution logic.
* Receives the deserialized payload and a `CancellationToken`.
* Uses injected Application and Infrastructure abstractions as appropriate.

Keeping these concerns separate allows the payload to be persisted without serializing the handler's dependency graph.

Persisted payloads must remain compatible with the application's deployment and serializer configuration.

### 4.2 Trigger vs. Handler

An `IRecurringJobTrigger` connects a recurring schedule to a job payload.

Its responsibility is to enqueue the corresponding job through `IJobScheduler`. It does not execute the business operation itself.

The same handler can therefore be invoked through:

* A recurring schedule.
* Immediate enqueueing.
* Delayed scheduling.
* An authorized administrative operation.

This keeps scheduling initiation separate from business execution.

### 4.3 Policy vs. Handler

An `IJobPolicy` wraps execution with cross-cutting behavior.

It receives the job, execution context, cancellation token, and a delegate representing the next step in the pipeline.

Policies can perform work before and after invoking the next delegate, subject to their implementation.

`JobOrchestrator` composes the registered policies. Handlers do not need to reference policy implementations.

---

## 5. Folder Structure

The following structure describes the intended organization. Exact directories should match the repository's current implementation.

### 5.1 Application Layer

```text
Application/
└── Modules/
    └── Scheduling/
        ├── Abstractions/
        │   ├── IJob.cs
        │   ├── IJobContext.cs
        │   ├── IJobOrchestrator.cs
        │   ├── IJobHandler.cs
        │   ├── IJobScheduler.cs
        │   └── IRecurringJobTrigger.cs
        ├── Attributes/
        │   └── RecurringJobAttribute.cs
        ├── Coordination/
        │   └── JobOrchestrator.cs
        ├── Exceptions/
        │   └── JobExecutionException.cs
        └── Policies/
            ├── IJobPolicy.cs
            ├── CircuitBreakerPolicy.cs
            └── TimeoutPolicy.cs
```

### 5.2 Infrastructure Layer

```text
Infrastructure/
└── Scheduling/
    ├── Execution/
    │   └── JobExecutionContext.cs
    ├── Hangfire/
    │   ├── HangfireJobDispatcher.cs
    │   └── HangfireJobScheduler.cs
    └── Extensions/
        ├── SchedulingInfrastructureExtensions.cs
        └── RecurringJobBootstrapper.cs
```

### 5.3 Feature-Local Jobs

Jobs should be colocated with the features that own their operations.

Example:

```text
Application/
└── BoundedContexts/
    └── Orders/
        └── Jobs/
            └── ExpirePendingOrders/
                ├── ExpirePendingOrdersJob.cs
                ├── ExpirePendingOrdersJobHandler.cs
                └── ExpirePendingOrdersTrigger.cs
```

Infrastructure-owned operational jobs may be located alongside their Infrastructure capability:

```text
Infrastructure/
└── Communication/
    └── Messaging/
        └── Outbox/
            └── Jobs/
                └── OutboxProcessing/
                    ├── OutboxProcessingJobPayload.cs
                    ├── OutboxProcessingJobHandler.cs
                    └── OutboxProcessingTrigger.cs
```

Additional examples include:

```text
Infrastructure/
├── Communication/
│   └── Messaging/
│       └── Outbox/
│           └── Jobs/
│               └── DeadLetterMonitor/
│                   ├── DeadLetterMonitorJobPayload.cs
│                   ├── DeadLetterMonitorJobHandler.cs
│                   └── DeadLetterMonitorTrigger.cs
└── Files/
    └── Jobs/
        ├── FileStorageCleanupJobPayload.cs
        ├── FileStorageCleanupJobHandler.cs
        └── FileStorageCleanupTrigger.cs
```

The Scheduling module owns the shared execution pipeline, not the business-specific jobs.

**Layering rule:** An Infrastructure-owned job may use Infrastructure services directly. A job that performs a business operation should delegate to the owning feature's Application or Domain logic rather than moving business rules into Infrastructure.

---

## 6. Application Layer Contracts

The Application layer owns the scheduling contracts, execution gateway, and policy abstractions.

### 6.1 `IJob`

`IJob` is a marker interface for job payloads.

```csharp
public interface IJob
{
}
```

Implementations should be plain data objects.

```csharp
public sealed class ExampleJob : IJob
{
    public Guid EntityId { get; init; }
}
```

Payloads must be compatible with the configured Hangfire serializer.

Changes to persisted job payload types should account for jobs that were created by earlier application versions and may execute after a deployment.

### 6.2 `IJobContext`

`IJobContext` exposes runtime metadata for the current execution.

```csharp
public interface IJobContext
{
    string JobId { get; }

    string CorrelationId { get; }

    int Attempt { get; }

    DateTime QueuedAt { get; }
}
```

The intended meanings are:

| Property        | Meaning                                                                          |
| --------------- | -------------------------------------------------------------------------------- |
| `JobId`         | Identifier assigned to the Hangfire background job                               |
| `CorrelationId` | Identifier used to correlate the execution with related activity                 |
| `Attempt`       | Current execution attempt number                                                 |
| `QueuedAt`      | Time associated with job creation or enqueueing, according to the implementation |

The current implementation uses the Hangfire job ID as the correlation ID and derives the attempt from Hangfire's retry metadata.

These are implementation choices, not universal meanings of correlation IDs or queue timestamps.

If cross-request or cross-system correlation is required, the correlation ID should be propagated explicitly rather than assuming the Hangfire job ID represents the original request.

Likewise, `QueuedAt` should only be described as the actual enqueue time if the implementation retrieves and stores that timestamp accurately. Hangfire's `CreatedAt` value represents job creation time and should not automatically be treated as the time the job entered a queue.

### 6.3 `IJobHandler<TJob>`

The handler contract is:

```csharp
public interface IJobHandler<in TJob>
    where TJob : IJob
{
    Task HandleAsync(
        TJob job,
        CancellationToken cancellationToken);
}
```

Handlers are discovered through configured assembly scanning and registered in dependency injection, typically as scoped services.

The actual lifetime and scanning behavior depend on the registration implementation.

### 6.4 `IJobScheduler`

The scheduling abstraction is:

```csharp
public interface IJobScheduler
{
    void Enqueue<TJob>(TJob job)
        where TJob : IJob;

    void Schedule<TJob>(
        TJob job,
        DateTimeOffset enqueueAt)
        where TJob : IJob;

    void AddOrUpdateRecurring<TJob>(
        string recurringJobId,
        TJob job,
        string cronExpression)
        where TJob : IJob;
}
```

The methods represent three scheduling operations:

* `Enqueue`: Submit a one-shot job for execution as soon as it can be processed.
* `Schedule`: Submit a one-shot job for future execution.
* `AddOrUpdateRecurring`: Register or update a recurring schedule.

The standard recurring-job convention uses `IRecurringJobTrigger` and `[RecurringJob]`, allowing the bootstrapper to own recurring registration.

Because these methods return `void`, scheduling failures must be communicated through the implementation's exception behavior or another explicitly defined mechanism.

The abstraction does not itself guarantee that dispatch is atomic with an Application database transaction.

### 6.5 `IJobOrchestrator`

The execution gateway contract is:

```csharp
public interface IJobOrchestrator
{
    Task ExecuteAsync<TJob>(
        TJob job,
        IJobContext context,
        CancellationToken cancellationToken)
        where TJob : IJob;
}
```

The dispatcher calls this contract after Hangfire invokes the job execution method.

The orchestrator is the common entry point for execution, regardless of whether the job was initiated by immediate enqueueing, delayed scheduling, or a recurring trigger.

### 6.6 `IRecurringJobTrigger`

The recurring-trigger contract is:

```csharp
public interface IRecurringJobTrigger
{
    void Trigger();
}
```

A trigger enqueues the corresponding payload through `IJobScheduler`.

The bootstrapper discovers trigger implementations and registers those with valid `[RecurringJob]` attributes.

### 6.7 `RecurringJobAttribute`

The attribute declares recurring schedule metadata.

```csharp
[AttributeUsage(
    AttributeTargets.Class,
    AllowMultiple = false,
    Inherited = false)]
public sealed class RecurringJobAttribute : Attribute
{
    public string JobId { get; }

    public string CronExpression { get; }

    public RecurringJobAttribute(
        string jobId,
        string cronExpression)
    {
        JobId = jobId;
        CronExpression = cronExpression;
    }
}
```

Usage:

```csharp
[RecurringJob("expire-pending-orders", "0 */6 * * *")]
public sealed class ExpirePendingOrdersTrigger
    : IRecurringJobTrigger
{
    public void Trigger()
    {
        // Enqueue the feature-owned job payload.
    }
}
```

The cron expression is a plain string. The Application layer does not need to reference Hangfire's `Cron` helper.

The actual attribute implementation should validate its inputs, or the bootstrapper should validate the recurring ID and cron expression during startup.

### 6.8 `JobOrchestrator`

`JobOrchestrator` resolves the handler, composes policies, and centralizes execution behavior.

The logical flow is:

```text
JobOrchestrator.ExecuteAsync(job, context, ct)
        |
        +-- Start trace span, when configured
        |
        +-- Log execution start
        |
        +-- Resolve IJobHandler<TJob>
        |
        +-- Compose the policy chain
        |
        +-- Execute the composed pipeline
        |
        +-- Log success or failure
        |
        +-- Propagate failure
```

The configured policy order is:

```text
TimeoutPolicy
    |
    v
CircuitBreakerPolicy
    |
    v
Job Handler
```

This represents the intended outermost-to-innermost composition. The actual order must match the orchestrator's policy-composition implementation.

On failure, the orchestrator propagates the exception so Hangfire can apply its configured retry behavior.

Cancellation should be classified according to its cause. Shutdown cancellation, a policy timeout, and a business operation that independently throws `OperationCanceledException` are not necessarily the same event. The implementation should distinguish them where possible.

### 6.9 `IJobPolicy`

The policy contract is:

```csharp
public interface IJobPolicy
{
    Task ExecuteAsync<TJob>(
        TJob job,
        IJobContext context,
        Func<Task> next,
        CancellationToken cancellationToken)
        where TJob : IJob;
}
```

Policies wrap the next stage of execution.

They may:

* Perform pre-execution checks.
* Establish execution boundaries.
* Observe execution outcomes.
* Perform post-execution actions.
* Propagate failures.

Policies are registered through dependency injection and composed by the orchestrator.

The policy registration order and composition direction must be documented and tested because they determine which policies wrap the others.

### 6.10 `TimeoutPolicy`

The timeout policy bounds how long the gateway waits for execution.

A representative implementation is:

```csharp
public sealed class TimeoutPolicy : IJobPolicy
{
    private readonly TimeSpan _timeout;

    public TimeoutPolicy(TimeSpan timeout)
    {
        _timeout = timeout;
    }

    public async Task ExecuteAsync<TJob>(
        TJob job,
        IJobContext context,
        Func<Task> next,
        CancellationToken cancellationToken)
        where TJob : IJob
    {
        using var timeoutCts =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        timeoutCts.CancelAfter(_timeout);

        await next().WaitAsync(timeoutCts.Token);
    }
}
```

**Important implementation detail:** This example does not pass `timeoutCts.Token` to `next`, because the shown policy contract does not accept a token for the downstream handler. As a result, the token can cancel the wait without necessarily cancelling the handler's underlying work.

A production implementation should make the cancellation contract explicit if handlers must observe the timeout token. For example, the execution pipeline could propagate an effective linked token to the handler.

A timeout therefore does not automatically guarantee that the underlying operation has stopped.

If the handler continues after the orchestrator reports a timeout, a retry may overlap with the original operation. Handlers must be cancellation-aware where possible and safe under duplicate execution.

### 6.11 `CircuitBreakerPolicy`

The circuit-breaker policy is intended to reject executions after a configured failure threshold until recovery conditions are met.

The current registration is scoped, meaning a new policy instance is typically created for each job execution scope.

If the failure counter is held only in that instance, failures do not accumulate across executions. The policy therefore does not provide a true cross-execution circuit breaker in that configuration.

Its effective behavior is limited to what the implementation actually tracks and enforces.

If the operational requirement is to stop repeatedly calling a failing dependency across executions, a real circuit breaker needs state that persists across the relevant executions. That state may be held in a correctly managed singleton for a single process or in a shared/distributed mechanism when multiple application instances must coordinate.

Hangfire retry limits and backoff can reduce repeated attempts, but they are not equivalent to a dependency-level circuit breaker.

### 6.12 `JobExecutionException`

`JobExecutionException` provides a scheduling-specific exception type.

```csharp
public class JobExecutionException : Exception
{
    public string? JobId { get; }

    public JobExecutionException(
        string message,
        string? jobId = null,
        Exception? inner = null)
        : base(message, inner)
    {
        JobId = jobId;
    }
}
```

It may represent failures such as a missing handler or an execution-pipeline failure.

The implementation should preserve the original exception as the inner exception when wrapping an existing failure.

Throwing this exception does not itself configure retries. Hangfire's retry behavior depends on its configured filters and exception handling.

---

## 7. Infrastructure Layer

Infrastructure supplies the Hangfire-backed implementations of the Application contracts.

### 7.1 `JobExecutionContext`

`JobExecutionContext` implements `IJobContext` using metadata available from Hangfire's execution context.

```csharp
public sealed class JobExecutionContext : IJobContext
{
    public string JobId { get; }

    public string CorrelationId { get; }

    public int Attempt { get; }

    public DateTime QueuedAt { get; }

    public JobExecutionContext(
        string jobId,
        string correlationId,
        int attempt,
        DateTime queuedAt)
    {
        JobId = jobId;
        CorrelationId = correlationId;
        Attempt = attempt;
        QueuedAt = queuedAt;
    }
}
```

The constructor is illustrative; the production implementation must populate these values accurately.

Current behavior:

* `JobId` comes from Hangfire's background job identifier.
* `CorrelationId` currently defaults to the Hangfire job ID.
* `Attempt` is derived from Hangfire's retry metadata.
* `QueuedAt` is populated from the timestamp selected by the implementation.

If a job must retain the originating HTTP request's correlation ID, that ID must be explicitly propagated through the payload or another supported context mechanism.

### 7.2 `HangfireJobScheduler`

The scheduler adapter implements `IJobScheduler` using Hangfire APIs.

Representative structure:

```csharp
public sealed class HangfireJobScheduler : IJobScheduler
{
    public void Enqueue<TJob>(TJob job)
        where TJob : IJob
    {
        // Enqueue a HangfireJobDispatcher invocation.
    }

    public void Schedule<TJob>(
        TJob job,
        DateTimeOffset enqueueAt)
        where TJob : IJob
    {
        // Schedule a HangfireJobDispatcher invocation.
    }

    public void AddOrUpdateRecurring<TJob>(
        string recurringJobId,
        TJob job,
        string cronExpression)
        where TJob : IJob
    {
        // Register or update a recurring dispatcher invocation.
    }
}
```

All scheduling methods must route execution through the dispatcher so that every job uses the common orchestration gateway.

The actual implementation may use Hangfire client APIs to enqueue or schedule dispatcher expressions. Those expressions must be compatible with Hangfire's expression serialization and dependency-injection activation behavior.

### 7.3 `HangfireJobDispatcher`

The dispatcher bridges Hangfire execution to the Application orchestrator.

Its responsibilities are to:

1. Receive the persisted job payload.
2. Obtain Hangfire's runtime execution context.
3. Construct the Application-facing job context.
4. Obtain the supported shutdown cancellation token.
5. Invoke `IJobOrchestrator.ExecuteAsync(...)`.

A representative structure is:

```csharp
public sealed class HangfireJobDispatcher
{
    private readonly IJobOrchestrator _orchestrator;

    public HangfireJobDispatcher(
        IJobOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public async Task Dispatch<TJob>(
        TJob job,
        PerformContext? context)
        where TJob : IJob
    {
        var ctx = context
            ?? throw new InvalidOperationException(
                "Hangfire execution context is unavailable.");

        var jobContext = new JobExecutionContext(
            jobId: ctx.BackgroundJob.Id,
            correlationId: ctx.BackgroundJob.Id,
            attempt: ctx.GetJobParameter<int>("RetryCount") + 1,
            queuedAt: ctx.BackgroundJob.CreatedAt);

        await _orchestrator.ExecuteAsync(
            job,
            jobContext,
            ctx.CancellationToken.ShutdownToken);
    }
}
```

This example illustrates the intended flow. Verify the exact context and cancellation APIs against the Hangfire package version in use.

In particular, `CreatedAt` should not be described as the precise enqueue timestamp unless the implementation guarantees that interpretation.

**Runtime substitution:** Hangfire supports supplying execution context through its runtime parameter-injection mechanisms. If the scheduler passes `null` for the `PerformContext` parameter, the implementation must be verified to ensure Hangfire replaces that value as intended. The dispatcher should fail clearly if runtime context is unavailable rather than proceeding with fabricated metadata.

### 7.4 `RecurringJobBootstrapper`

The bootstrapper discovers recurring triggers and registers their schedules with Hangfire.

Logical flow:

```text
ScheduleRecurringJobs(serviceProvider, assemblies)
        |
        +-- Discover IRecurringJobTrigger implementations
        |
        +-- Read each trigger's [RecurringJob] attribute
        |
        +-- Validate the recurring ID and cron expression
        |
        +-- Resolve IRecurringJobManager
        |
        +-- Register or update the recurring schedule
```

The implementation uses the DI-resolved `IRecurringJobManager` rather than relying on the static `RecurringJob` API during startup registration.

A startup invocation may look like:

```csharp
using var scope = app.Services.CreateScope();

scope.ServiceProvider.ScheduleRecurringJobs(
    typeof(IRecurringJobTrigger).Assembly,
    typeof(SchedulingInfrastructureExtensions).Assembly);
```

This is an example, not a universal assembly list. The assemblies passed must include all locations that contain recurring triggers.

If triggers exist in feature-specific Application or Infrastructure assemblies, those assemblies must also be included.

The bootstrapper should:

* Fail startup when a discovered trigger is missing required schedule metadata, if that is the selected configuration policy.
* Validate recurring IDs and cron expressions.
* Avoid registering duplicate IDs for different trigger types.
* Register schedules idempotently.
* Account for obsolete recurring IDs when triggers are renamed or removed.
* Be safe when multiple application instances start concurrently.

The actual behavior of `ScheduleRecurringJobs` depends on its implementation.

### 7.5 `SchedulingInfrastructureExtensions`

The Infrastructure extension method configures Hangfire and registers the Scheduling services.

A representative registration structure is:

```csharp
public static IServiceCollection AddSchedulingInfrastructure(
    this IServiceCollection services,
    string hangfireConnectionString)
{
    services.AddHangfire(configuration =>
        configuration.UseSqlServerStorage(
            hangfireConnectionString));

    services.AddHangfireServer();

    services.AddScoped<
        IJobScheduler,
        HangfireJobScheduler>();

    services.AddScoped<
        HangfireJobDispatcher>();

    // Register IJobOrchestrator, policy implementations,
    // handlers, and other required services through the
    // appropriate Application and Infrastructure registrations.

    return services;
}
```

The complete implementation must ensure that:

* `IJobOrchestrator` resolves to the Application orchestrator.
* Every required `IJobPolicy` is registered.
* Handler scanning includes all relevant assemblies.
* Trigger scanning includes all relevant assemblies.
* Hangfire storage and worker configuration are applied once.
* Connection strings are loaded from configuration or a secret provider, not hardcoded.

Avoid duplicate registrations of the orchestrator or scanning services that cause handlers and triggers to be registered inconsistently.

---

## 8. Automatic Wiring

The zero-manual-registration convention relies on two mechanisms.

### 8.1 Handler and Trigger Discovery

Configured assembly scanning discovers:

* Implementations of `IJobHandler<TJob>`.
* Implementations of `IRecurringJobTrigger`.

Application registration should scan Application-owned jobs and handlers. Infrastructure registration should scan Infrastructure-owned jobs and handlers where required.

The scan must include the relevant assemblies explicitly or through a well-defined assembly-discovery convention.

A fallback to the executing assembly is not sufficient when the application's job types live in other assemblies.

### 8.2 Recurring Schedule Registration

At startup, `RecurringJobBootstrapper`:

1. Scans configured assemblies.
2. Finds trigger implementations.
3. Reads `[RecurringJob]`.
4. Validates the metadata.
5. Registers each recurring schedule with Hangfire.

Automatic discovery does not mean every class in the solution is scanned automatically. Only the configured assemblies are considered.

### 8.3 Developer Workflow

**Adding a recurring job**

1. Create a payload implementing `IJob`.
2. Implement `IJobHandler<TJob>`.
3. Implement `IRecurringJobTrigger`.
4. Add `[RecurringJob("id", "cron")]`.
5. Ensure the owning assembly is included in the discovery configuration.

No manual handler registration or direct Hangfire recurring registration is required when the automatic wiring is configured correctly.

**Adding a one-shot job**

1. Create a payload implementing `IJob`.
2. Implement `IJobHandler<TJob>`.
3. Call `IJobScheduler.Enqueue(payload)` or `IJobScheduler.Schedule(payload, time)`.

No recurring trigger or attribute is required.

---

## 9. Execution Lifecycle

The complete lifecycle of a typical job is:

```text
1. Application calls IJobScheduler.Enqueue(job)
                    |
                    v
2. HangfireJobScheduler submits the dispatcher invocation
                    |
                    v
3. Hangfire persists the job in configured storage
                    |
                    v
4. A worker claims the job
                    |
                    v
5. Hangfire invokes HangfireJobDispatcher
                    |
                    v
6. Dispatcher builds IJobContext
                    |
                    v
7. Dispatcher calls IJobOrchestrator.ExecuteAsync(...)
                    |
                    v
8. Orchestrator resolves IJobHandler<TJob>
                    |
                    v
9. Orchestrator composes execution policies
                    |
                    v
10. The handler executes
                    |
                    v
11. Outcome propagates to Hangfire
                    |
           +--------+--------+
           |        |        |
           v        v        v
        Success   Failure  Cancellation
           |        |        |
           v        v        v
       Completed  Retry or  Depends on
                  failure   exception and
                  handling  cancellation
                            handling
```

The final Hangfire state depends on the exception propagated from the dispatcher and the configured Hangfire filters.

A thrown exception does not guarantee that a retry will occur indefinitely or at a particular interval. Retry counts, backoff, and final failure handling must be configured explicitly.

Cancellation does not universally imply a single Hangfire state; the result depends on how the cancellation is surfaced and handled.

---

## 10. Concurrency, Cancellation, and Runtime Behavior

### 10.1 Hangfire Concurrency

Hangfire processes jobs using its configured worker pool.

Worker count is configurable and may depend on the host's processor count and the selected Hangfire configuration.

Jobs of the same type may execute concurrently unless an applicable concurrency-control mechanism prevents it.

The `DisableConcurrentExecution` attribute is one possible Hangfire mechanism, but it must be applied to the actual Hangfire-invoked method or type in a way supported by the selected Hangfire version.

Applying it to a payload or handler that Hangfire does not invoke directly does not automatically serialize the dispatcher's executions.

Concurrency control should be selected based on the business invariant:

* Use database constraints or optimistic concurrency for durable business invariants.
* Use idempotency keys for duplicate external operations.
* Use an appropriate distributed lock when execution truly requires cross-worker mutual exclusion.
* Do not rely solely on in-process locks when multiple application instances can execute jobs.

### 10.2 Cancellation

The dispatcher passes a cancellation token derived from Hangfire's shutdown mechanism to the orchestrator.

Handlers should propagate the token to cancellable operations.

During graceful shutdown:

* In-flight handlers can observe cancellation if the token reaches them.
* Cooperative handlers may stop promptly.
* Handlers that ignore cancellation may continue until the work finishes or the process terminates.
* A timeout that only stops awaiting a handler does not force the underlying operation to stop.

The cancellation path must be tested end to end. Receiving a token in the handler signature does not guarantee that timeout or shutdown cancellation reaches every downstream operation.

### 10.3 Circuit-Breaker Runtime Behavior

The circuit-breaker policy is currently registered as a scoped service.

If its failure counter exists only within the scoped instance:

* Each job execution receives a fresh counter.
* Failures do not accumulate across executions.
* The circuit does not provide a persistent, cross-execution breaker.
* Any logging or rejection behavior is limited to the actual per-execution implementation.

If the operational requirement is to stop repeatedly calling a broken dependency, implement a circuit breaker whose state is shared across the intended scope of protection.

A singleton can provide process-local state. Multiple application instances may require shared state or an established resilience implementation with appropriate distributed behavior.

Hangfire retry backoff is useful but is not equivalent to a circuit breaker: retries govern repeated job execution, whereas a circuit breaker protects calls to a dependency based on observed outcomes.

### 10.4 Timeout Runtime Behavior

A timeout policy implemented with `Task.WaitAsync` can stop waiting for the handler's task after the timeout expires.

If the timeout token is not propagated into the handler, the handler may continue running after the orchestrator reports failure.

This creates a potential overlap:

```text
Original Execution
        |
        +-- Timeout reported
        |
        +-- Handler still running
                    |
                    v
             Hangfire retries
                    |
                    v
             Second execution
```

For this reason:

* Propagate cancellation to handlers and downstream operations.
* Ensure handlers are safe under duplicate execution.
* Avoid assuming timeout means the underlying operation stopped.
* Consider the behavior of external calls that may have succeeded even if the local caller timed out.

The timeout policy must be designed together with the cancellation contract and the job's idempotency strategy.

---

## 11. Persistence

The Scheduling feature does not own a separate business persistence model for scheduled jobs.

Hangfire manages the persistence required for its scheduling and execution lifecycle in the configured storage database.

### 11.1 Hangfire Storage

Hangfire SQL Server storage creates and manages its own schema, including tables used for job records, state transitions, queues, recurring definitions, server coordination, and statistics.

Common table names include:

| Table                        | General purpose                                                                     |
| ---------------------------- | ----------------------------------------------------------------------------------- |
| `HangFire.Job`               | Background job records and serialized invocation data                               |
| `HangFire.State`             | Job state history                                                                   |
| `HangFire.Set`               | Named sets used by Hangfire                                                         |
| `HangFire.Hash`              | Hash-based internal storage, including recurring-job metadata in supported versions |
| `HangFire.List`              | Internal lists and queues                                                           |
| `HangFire.Counter`           | Statistics counters                                                                 |
| `HangFire.AggregatedCounter` | Aggregated statistics                                                               |
| `HangFire.Server`            | Registered Hangfire servers and heartbeat information                               |

The exact schema and storage details depend on the Hangfire version and configured storage implementation. This table is a conceptual overview, not a complete or version-independent schema specification.

Hangfire storage initialization and schema installation are handled by the configured Hangfire storage implementation. Whether installation occurs automatically at startup depends on the package version and storage options.

No application-owned EF Core migration is required merely to create Hangfire's internal tables when Hangfire manages its own schema installation. Database permissions and deployment configuration must still allow the selected installation process.

### 11.2 Storage Configuration

A representative SQL Server configuration is:

```csharp
services.AddHangfire(configuration =>
    configuration.UseSqlServerStorage(connectionString));
```

The connection string should come from application configuration, environment variables, or a managed secret provider.

It must not be hardcoded in source code.

The current configuration may use default SQL Server storage options. Queue polling, job expiration, transaction behavior, and other storage settings should be documented separately if customized.

### 11.3 Business Data and Hangfire Storage

Hangfire persistence does not make a scheduled job atomic with an unrelated business database transaction.

For example, the following two operations do not automatically share a transaction:

1. Update an order in the application's database.
2. Enqueue a Hangfire job.

If both operations must be guaranteed together, use a transactional design such as the Outbox pattern and have a background processor dispatch the resulting work.

Hangfire persistence protects the job once it has been successfully stored; it does not eliminate failures between independent database operations.

---

## 12. Extensibility

The Scheduling feature is designed to allow extensions without forcing every handler to change.

### 12.1 Adding a New Policy

To add a policy:

1. Implement `IJobPolicy`.
2. Register it in dependency injection.
3. Verify the intended policy order in the orchestrator.
4. Verify that it handles exceptions, cancellation, and disposal correctly.

The orchestrator can compose the registered policies through `IEnumerable<IJobPolicy>`.

Adding a policy requires no handler changes when the policy applies uniformly to every job. A policy that only applies to specific jobs needs an explicit selection mechanism rather than an assumption that every registered policy is appropriate for every execution.

### 12.2 Replacing Hangfire

The Application layer depends on contracts rather than Hangfire types.

The main integration contracts include:

* `IJobScheduler`
* `IJobOrchestrator`
* `IJobHandler<TJob>`
* `IRecurringJobTrigger`

Replacing Hangfire would generally require:

1. Implementing `IJobScheduler` for the new runtime.
2. Adapting runtime dispatch to the Application execution gateway.
3. Replacing the recurring schedule registration mechanism.
4. Updating Infrastructure registration and configuration.
5. Migrating or retiring existing persisted Hangfire jobs and schedules where appropriate.
6. Verifying retry, cancellation, concurrency, and serialization semantics.

If the replacement runtime has a different execution model, `IJobOrchestrator` may remain unchanged while the adapter and dispatch pipeline change.

The abstraction reduces application coupling; it does not make scheduler replacement completely cost-free.

### 12.3 Extending `IJobContext`

`IJobContext` can be extended with additional runtime metadata, such as tenant or initiating-user identifiers, when the execution model requires them.

A safe extension involves:

1. Updating the Application contract.
2. Updating the Infrastructure implementation.
3. Defining where the new value originates.
4. Propagating it through the scheduling boundary.
5. Updating handlers only when they require the new value.
6. Reviewing logging and telemetry cardinality if the value is used as a tag.

Not every value should be added to the context globally. Include only metadata that has a clear execution or observability purpose.

---

## 13. Operational Considerations

The Scheduling feature is part of the application's operational infrastructure.

The following concerns should be addressed as the number and importance of jobs grow.

### 13.1 Observability

The common execution gateway should provide consistent structured logs and, when configured, tracing.

Useful metadata includes:

* Job ID.
* Job type.
* Attempt number.
* Correlation ID.
* Execution duration.
* Outcome.
* Failure classification.

Avoid using unbounded values such as job IDs or correlation IDs as metric labels. Such identifiers are appropriate for logs and traces but can create excessive metric cardinality.

Do not log sensitive payload contents, credentials, verification codes, payment secrets, or unnecessary personal data.

### 13.2 Retry Safety

A job may execute more than once.

A handler must not assume that a failure proves none of its operations succeeded. This is especially important for external HTTP calls, payment operations, notifications, and other side effects.

Use idempotency, durable state transitions, unique constraints, and explicit recovery strategies where appropriate.

### 13.3 Scheduling Reliability

Consider:

* What happens if enqueueing fails?
* What happens if the process stops after a business change but before job dispatch?
* Can a job remain queued across a deployment?
* What happens when a recurring trigger is renamed or removed?
* How are permanently failing jobs inspected and recovered?
* Can multiple workers execute the same business operation concurrently?
* Are timeout and cancellation semantics safe for the underlying work?

These concerns should be addressed by the appropriate combination of the Scheduling gateway, Hangfire configuration, the Outbox pattern, business-level idempotency, and operational monitoring.

---

## 14. Summary

The Scheduling feature provides:

* A lightweight Application-facing abstraction over Hangfire.
* A centralized `JobOrchestrator` for handler resolution, policy composition, and execution behavior.
* Automatic handler and recurring-trigger discovery from configured assemblies.
* Automatic recurring schedule registration through `[RecurringJob]` and `RecurringJobBootstrapper`.
* A consistent execution context for job identity, attempt metadata, and correlation.
* A clear separation between payloads, handlers, triggers, policies, and scheduler infrastructure.

The feature deliberately limits its scope:

* **It does not own business logic.** Jobs and handlers belong with the features or operational capabilities they serve.
* **It does not own Hangfire's persisted job state.** Hangfire manages its internal scheduling and execution storage.
* **It does not make business operations automatically idempotent.** Handlers must account for retries and duplicate execution.
* **It does not guarantee atomic dispatch with business transactions.** Use a transactional mechanism such as the Outbox when atomicity is required.
* **It does not make timeouts equivalent to cancellation.** The execution pipeline must propagate cancellation correctly.
* **It does not eliminate operational responsibilities.** Retry behavior, recurring schedule lifecycle, concurrency, monitoring, and recovery must be configured deliberately.

The result is a centralized background execution capability that can accommodate a growing number of jobs without proportionally increasing scheduler coupling or duplicated execution logic.

---

**Document version:** 1.0 — Final
**Feature:** Scheduling / Background Job Execution Gateway
