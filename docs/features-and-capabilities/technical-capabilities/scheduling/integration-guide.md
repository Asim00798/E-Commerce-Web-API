# Background Jobs Integration Guide

**Project:** E-Commerce Modular Monolith

**Purpose:** How Application Features Integrate with the Scheduling / Background Job Execution Gateway

**Version:** 1.0 — Final

---

## 1. Purpose

This document explains how application features integrate with the Scheduling feature to create and execute background jobs.

It covers:

* Creating fire-and-forget jobs.
* Creating recurring jobs.
* Creating delayed one-shot jobs.
* Distinguishing business-process jobs from housekeeping jobs.
* Using `IJobScheduler` to dispatch background work.
* Using `IJobHandler<TJob>` to execute jobs.
* Using `IRecurringJobTrigger` to declare recurring schedules.
* Publishing integration events from business-process jobs through the Outbox.
* Automatic handler and trigger discovery.
* Execution, resilience, cancellation, and integration boundaries.

This guide is intended for developers working on any feature that requires background execution.

---

## 2. Integration Overview

The Scheduling feature provides an Application-layer execution gateway backed by Hangfire in Infrastructure.

Application features integrate with it through three primary concepts:

1. **Job payload — `IJob`**

   Carries the data required to execute a job.

2. **Job handler — `IJobHandler<TJob>`**

   Contains the work performed when the job executes.

3. **Job scheduler — `IJobScheduler`**

   Enqueues or schedules jobs without exposing Hangfire to Application code.

Recurring jobs additionally use:

4. **Recurring trigger — `IRecurringJobTrigger`**

   Declares when a recurring job should run and enqueues the corresponding payload.

### 2.1 Architectural Boundary

```text
Application Feature
       |
       v
 IJobScheduler
       |
       v
Scheduling Execution Gateway
       |
       +-- JobOrchestrator
       |
       +-- TimeoutPolicy
       |
       +-- CircuitBreakerPolicy
       |
       +-- Centralized Logging
       |
       v
Hangfire Infrastructure
       |
       v
Background Worker
```

The Application layer does not call Hangfire directly.

The Scheduling gateway centralizes job execution concerns, while Hangfire provides the Infrastructure implementation for job persistence, scheduling, worker execution, and retry management.

---

## 3. Determine the Job Type

Before implementing a background job, determine how and when it should execute.

### 3.1 Execution Modes

| Execution mode   | Purpose                                              | Application integration                     |
| ---------------- | ---------------------------------------------------- | ------------------------------------------- |
| Fire-and-forget  | Execute once as soon as a worker can process the job | `IJobScheduler.Enqueue(...)`                |
| Recurring        | Execute repeatedly according to a cron schedule      | `IRecurringJobTrigger` and `[RecurringJob]` |
| Delayed one-shot | Execute once at a future time                        | `IJobScheduler.Schedule(...)`               |

These modes describe **when the job executes**, not its business meaning.

### 3.2 Business Classification

Jobs also fall into two broad categories:

* **Business-process jobs:** Perform business operations whose outcomes may represent facts that other bounded contexts need to know about.
* **Housekeeping jobs:** Perform operational maintenance without creating a cross-context business event.

This classification helps determine whether an integration event is required. It does not change the underlying job execution mechanism.

---

## 4. Business-Process Jobs vs. Housekeeping Jobs

Both categories use the same Scheduling gateway, handler contracts, and execution pipeline.

The distinction concerns the business meaning of the work performed.

### 4.1 Business-Process Jobs

A business-process job performs an operation that may produce a business fact relevant to another bounded context.

When the operation establishes such a fact, the owning feature records the corresponding integration event through the Outbox as part of the same database transaction as the associated business state change.

Example:

```text
Inventory Reconciliation
        |
        v
Business State Change
        |
        v
Integration Event
        |
        v
Outbox
        |
        v
Outbox Processor
        |
        v
Finance Handles the Event
```

For example, a completed inventory reconciliation might produce a fact that Finance needs to consume to update accounting records.

The event must describe an actual business outcome, not merely the fact that a background job finished executing.

The handler uses the feature's business logic and Outbox abstractions to persist the state change and event atomically.

### 4.2 Housekeeping Jobs

A housekeeping job performs operational work that does not produce a cross-context business fact requiring publication.

Examples include:

* Temporary file cleanup.
* Expired temporary-record cleanup.
* Cache maintenance.
* Operational monitoring.
* Dead-letter monitoring.

A housekeeping job normally completes without publishing a business integration event.

However, if an operational process also changes business state in a way that other bounded contexts must react to, that business outcome may require an integration event.

### 4.3 Decision Rule

Ask:

**Does this operation establish a business fact that another bounded context needs to react to?**

* **Yes:** Persist the business change and its integration event through the Outbox in the same Unit of Work transaction.
* **No:** Perform the operational work without publishing a business integration event.

Not every successful job needs an integration event. Conversely, a job should not be classified as housekeeping merely because it runs on a schedule.

---

## 5. Job Integration Contracts

Application features integrate with Scheduling through the following contracts.

### 5.1 `IJob`

`IJob` represents the job payload.

```csharp
public interface IJob
{
}
```

A job payload is a data-transfer object containing the information required to perform the work.

Example:

```csharp
public sealed class MyJob : IJob
{
    public string SomeParameter { get; init; } = string.Empty;
}
```

Job payloads should:

* Contain execution data rather than business execution logic.
* Be compatible with the configured Hangfire serializer.
* Remain stable enough for persisted jobs to be deserialized after application deployments.
* Avoid carrying unnecessary sensitive information.
* Prefer identifiers and explicit parameters over large object graphs.

Because jobs may remain persisted between deployments, changes to payload types and their properties must account for previously enqueued jobs.

### 5.2 `IJobHandler<TJob>`

The handler contains the actual work.

```csharp
public interface IJobHandler<in TJob>
    where TJob : IJob
{
    Task HandleAsync(
        TJob job,
        CancellationToken cancellationToken);
}
```

Example:

```csharp
public sealed class MyJobHandler : IJobHandler<MyJob>
{
    private readonly ILogger<MyJobHandler> _logger;

    public MyJobHandler(ILogger<MyJobHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(
        MyJob job,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing job parameter {Param}",
            job.SomeParameter);

        cancellationToken.ThrowIfCancellationRequested();

        // Perform the operation owned by this feature.

        return Task.CompletedTask;
    }
}
```

Handlers are discovered and registered through the Scheduling feature's configured assembly scanning.

Manual registration is unnecessary when the handler belongs to an assembly included in that scanning configuration.

A handler should focus on the operation it owns. It should not contain Hangfire-specific scheduling code or duplicate the gateway's centralized execution policies.

### 5.3 `IJobScheduler`

Application code uses `IJobScheduler` to dispatch background work.

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

Application features normally use:

* `Enqueue` for immediate one-shot execution.
* `Schedule` for delayed one-shot execution.

Recurring jobs normally use `IRecurringJobTrigger` and `[RecurringJob]`, allowing the bootstrapper to discover and register recurring schedules automatically.

The availability of `AddOrUpdateRecurring` in the interface does not mean feature developers should bypass the established recurring-trigger convention.

**Important:** The `void` signatures shown above do not provide an explicit application-level result indicating whether scheduling succeeded. If callers need to handle dispatch failures directly, the scheduling abstraction should expose an appropriate result or exception contract.

---

## 6. Why Recurring Triggers Exist

Recurring triggers are thin scheduling adapters.

Their responsibility is to connect a recurring schedule to the corresponding job payload. They contain no business execution logic.

This separation allows the same handler to be invoked through different scheduling mechanisms:

* A recurring schedule.
* Immediate enqueueing.
* Delayed scheduling.
* An authorized administrative operation that enqueues the same job.

The trigger determines when the recurring execution is initiated. The handler determines what the job does.

Example:

```csharp
[RecurringJob("my-recurring-job", "0 3 * * *")]
public sealed class MyJobTrigger : IRecurringJobTrigger
{
    private readonly IJobScheduler _scheduler;

    public MyJobTrigger(IJobScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public void Trigger()
    {
        _scheduler.Enqueue(new MyJob());
    }
}
```

The example assumes the Scheduling implementation defines the `RecurringJobAttribute` and `IRecurringJobTrigger` contracts as shown.

The trigger delegates dispatch to `IJobScheduler` rather than calling Hangfire directly.

---

## 7. Standard Job Execution Flow

All jobs use the same execution gateway.

```text
Job Payload
   |
   v
IJobScheduler
   |
   v
Hangfire Infrastructure
   |
   v
HangfireJobDispatcher
   |
   v
JobOrchestrator
   |
   v
Execution Policies
   |
   v
IJobHandler<TJob>
   |
   v
Job Completion or Failure
```

Recurring jobs add a trigger and schedule-registration step before execution:

```text
RecurringJobBootstrapper
   |
   v
Recurring Job Registration
   |
   v
Recurring Trigger
   |
   v
IJobScheduler.Enqueue(...)
   |
   v
Standard Job Execution Flow
```

The exact order of dispatcher, orchestrator, and handler resolution follows the Scheduling implementation. The diagram describes their responsibilities rather than requiring a particular internal method-call sequence.

---

## 8. Fire-and-Forget Jobs

Fire-and-forget jobs execute once when the job becomes available and a Hangfire worker can process it.

They require:

* An `IJob` payload.
* An `IJobHandler<TJob>` implementation.
* A call to `IJobScheduler.Enqueue(...)`.

No recurring trigger is required.

### 8.1 Create the Payload

```csharp
public sealed class MyJob : IJob
{
    public string SomeParameter { get; init; } = string.Empty;
}
```

### 8.2 Create the Handler

```csharp
public sealed class MyJobHandler : IJobHandler<MyJob>
{
    private readonly ILogger<MyJobHandler> _logger;

    public MyJobHandler(ILogger<MyJobHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(
        MyJob job,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation(
            "Processing {Param}",
            job.SomeParameter);

        // Perform the required work.

        return Task.CompletedTask;
    }
}
```

### 8.3 Enqueue the Job

From an appropriate Application component:

```csharp
_jobScheduler.Enqueue(
    new MyJob
    {
        SomeParameter = "value"
    });
```

The Scheduling integration handles the configured dispatch and execution pipeline.

The gateway is responsible for:

* Routing execution through the common gateway.
* Resolving the corresponding handler.
* Applying configured execution policies.
* Providing the job execution context.
* Centralizing execution logging.
* Propagating failures to the Hangfire execution layer.

Hangfire manages persisted job execution and its configured retry behavior.

**Reliability consideration:** If enqueueing occurs before the business transaction commits, a worker could execute the job before the related business state is durable. If job creation must be atomic with a business transaction, use a transactional mechanism such as an Outbox rather than relying on a separate scheduling call to participate in the transaction.

---

## 9. Recurring Jobs

Recurring jobs execute repeatedly according to a cron expression.

A recurring job requires:

* An `IJob` payload.
* An `IJobHandler<TJob>` implementation.
* An `IRecurringJobTrigger` implementation.
* A `[RecurringJob]` attribute containing the recurring job ID and cron expression.

### 9.1 Create the Payload

```csharp
public sealed class MyJob : IJob
{
}
```

### 9.2 Create the Handler

```csharp
public sealed class MyJobHandler : IJobHandler<MyJob>
{
    public Task HandleAsync(
        MyJob job,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Perform the required work.

        return Task.CompletedTask;
    }
}
```

### 9.3 Create the Trigger

```csharp
[RecurringJob("my-recurring-job", "0 3 * * *")]
public sealed class MyJobTrigger : IRecurringJobTrigger
{
    private readonly IJobScheduler _scheduler;

    public MyJobTrigger(IJobScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public void Trigger()
    {
        _scheduler.Enqueue(new MyJob());
    }
}
```

The example cron expression represents a daily execution at 03:00 according to the configured scheduling time zone. Verify the actual time-zone configuration in the Scheduling infrastructure.

### 9.4 Automatic Schedule Registration

The `RecurringJobBootstrapper` runs during application startup.

It:

1. Scans the configured assemblies.
2. Discovers implementations of `IRecurringJobTrigger`.
3. Reads their `[RecurringJob]` attributes.
4. Extracts each recurring job ID and cron expression.
5. Registers the recurring schedule with Hangfire.

When the trigger and handler are included in the configured scanning assemblies, developers do not need to register them manually.

Developers do not write Hangfire-specific registration code such as:

```csharp
RecurringJob.AddOrUpdate(...);
```

The bootstrapper owns recurring schedule registration.

**Operational consideration:** Recurring job IDs should be stable and unique. Changing an ID can leave the previous schedule registered unless the old entry is explicitly removed. Startup registration must also be safe when multiple application instances perform it.

### 9.5 Cron Expression Rule

Cron expressions are declared as plain strings:

```csharp
[RecurringJob("monthly-maintenance", "0 2 1 * *")]
```

This example represents execution at 02:00 on the first day of each month, subject to the configured time zone and cron interpretation.

The Application layer does not need to reference Hangfire's `Cron` helper. This keeps recurring schedule declarations independent of Hangfire-specific APIs.

---

## 10. Delayed One-Shot Jobs

A delayed job executes once at a future time.

It requires:

* An `IJob` payload.
* An `IJobHandler<TJob>` implementation.
* A call to `IJobScheduler.Schedule(...)`.

No recurring trigger is required.

Example:

```csharp
_jobScheduler.Schedule(
    new MyJob
    {
        SomeParameter = "value"
    },
    DateTimeOffset.UtcNow.AddHours(1));
```

The example requests that the job become eligible for execution approximately one hour later.

Actual execution may occur later than the requested time because of worker availability, queue backlog, application downtime, or scheduler behavior.

The handler and execution gateway are the same as those used for fire-and-forget jobs. Only the dispatch timing differs.

For business-critical delayed actions, such as order expiration, consider how the system handles scheduling failures, transaction boundaries, duplicate execution, and recovery after downtime.

---

## 11. Business-Process Job Integration

When a job performs a business operation that establishes a fact relevant to another bounded context, the handler follows the normal Outbox workflow.

```text
Background Job
     |
     v
Job Handler
     |
     v
Business State Change
     +
Integration Event
     |
     v
Same Unit of Work Transaction
     |
     v
Outbox Record
     |
     v
Outbox Processor
     |
     v
Subscriber Handles Event
```

The business state change and its Outbox message must be persisted in the same database transaction.

This prevents the business state from committing without its corresponding durable integration event, assuming both writes participate in the same transaction.

Example structure:

```csharp
public sealed class InventoryReconciliationHandler
    : IJobHandler<InventoryReconciliationJob>
{
    private readonly IOutboxMessageWriter _outbox;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryReconciliationHandler(
        IOutboxMessageWriter outbox,
        IUnitOfWork unitOfWork)
    {
        _outbox = outbox;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        InventoryReconciliationJob job,
        CancellationToken cancellationToken)
    {
        // 1. Execute the business operation.
        // 2. Update the relevant business state.
        // 3. Create the integration event if a business fact occurred.
        // 4. Add the event to the Outbox.

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
```

This is a structural example, not a complete implementation. The actual business operation, event construction, and Outbox API depend on the owning feature.

The owning feature is responsible for:

* Enforcing its business invariants.
* Performing the business state transition.
* Determining whether an integration event is warranted.
* Writing the event through the Outbox abstraction.
* Committing the changes through the appropriate Unit of Work.

Scheduling only provides the execution mechanism.

**Do not enqueue a separate Hangfire job from inside the business transaction as a substitute for the Outbox.** The scheduling call and database transaction do not automatically share an atomic commit.

### 11.1 Idempotency

Hangfire may execute a job more than once, including after a failure that occurs after some work has already succeeded.

Handlers must therefore consider duplicate execution, especially when they:

* Change business state.
* Initiate payments or refunds.
* Call external services.
* Create shipments.
* Produce integration events.

Use business-level idempotency, unique constraints, concurrency control, or other appropriate safeguards. The Outbox makes event publication durable; it does not automatically make the handler's entire business operation idempotent.

---

## 12. Housekeeping Job Integration

Housekeeping handlers perform operational work without publishing business integration events when no cross-context business fact is produced.

Example:

```text
File Cleanup Job
     |
     v
FileStorageCleanupHandler
     |
     v
Delete Eligible Files
     |
     v
Complete
```

Typical housekeeping jobs include:

* Temporary file cleanup.
* Expired registration cleanup.
* Dead-letter monitoring.
* Cache maintenance.
* Operational maintenance tasks.

Housekeeping jobs still require appropriate error handling, cancellation support, observability, and safe behavior under retries.

They do not bypass the common Scheduling execution gateway.

---

## 13. Automatic Wiring

The Scheduling feature provides two automatic discovery mechanisms.

### 13.1 Handler Discovery

Implementations of:

```csharp
IJobHandler<TJob>
```

are discovered through assembly scanning and registered in dependency injection.

For handlers in configured scanning assemblies, manual registration such as the following is unnecessary:

```csharp
services.AddScoped<MyJobHandler>();
```

The actual discovery scope depends on the assemblies configured by the Scheduling registration.

### 13.2 Trigger Discovery

Implementations of:

```csharp
IRecurringJobTrigger
```

are discovered through the configured assembly scanning.

If a discovered trigger contains a valid `[RecurringJob]` attribute, the `RecurringJobBootstrapper` registers its recurring schedule.

Developers do not manually register recurring schedules with Hangfire.

Automatic discovery does not eliminate the need to ensure that the feature's assembly is included in the scanning configuration and that the trigger's dependencies are registered.

---

## 14. Developer Workflow

### 14.1 Fire-and-Forget

1. Create an `IJob` payload.
2. Implement `IJobHandler<TJob>`.
3. Call `IJobScheduler.Enqueue(payload)` from an appropriate Application component.

No trigger, recurring attribute, or manual handler registration is required when automatic discovery is configured correctly.

### 14.2 Recurring

1. Create an `IJob` payload.
2. Implement `IJobHandler<TJob>`.
3. Create an `IRecurringJobTrigger`.
4. Add `[RecurringJob(id, cron)]`.
5. Ensure the feature assembly is included in the configured discovery process.

No manual recurring registration is required.

### 14.3 Delayed One-Shot

1. Create an `IJob` payload.
2. Implement `IJobHandler<TJob>`.
3. Call `IJobScheduler.Schedule(payload, enqueueAt)`.

No trigger or recurring attribute is required.

---

## 15. Execution and Resilience

All job types use the common execution gateway.

```text
HangfireJobDispatcher
        |
        v
JobOrchestrator
        |
        v
Resolve IJobHandler<TJob>
        |
        v
TimeoutPolicy
        |
        v
CircuitBreakerPolicy
        |
        v
Handler Execution
```

The precise policy composition and execution order are defined by the Scheduling implementation.

The gateway provides the configured combination of:

* Handler resolution.
* Timeout handling.
* Circuit-breaker policy.
* Structured execution logging.
* Job execution context.
* Failure propagation.
* Integration with Hangfire retry behavior.

Feature handlers should not duplicate these centralized scheduler policies.

### 15.1 Retry Ownership

Hangfire owns the configured job retry behavior.

Avoid implementing a second general-purpose retry loop inside handlers or the JobOrchestrator. Layering independent retries can multiply attempts and increase load on failing dependencies.

A handler may still need domain-specific recovery or compensation logic where the business operation requires it. Such logic should be distinct from duplicating Hangfire's general retry mechanism.

### 15.2 Policy Limitations

Timeout and circuit-breaker policies do not automatically guarantee that the underlying operation stops or that the job is safe to retry.

For example:

* A timeout may stop awaiting an operation without stopping the underlying work if cancellation is not honored.
* A circuit breaker only protects against failures according to its actual scope and state-management design.
* A per-execution circuit-breaker instance cannot accumulate failures across executions in the way a shared circuit breaker can.

The effective behavior depends on the configured policy implementations. Handlers must still use cancellation correctly and protect business operations against duplicate execution.

---

## 16. Cancellation

Job handlers receive a `CancellationToken`.

```csharp
public async Task HandleAsync(
    MyJob job,
    CancellationToken cancellationToken)
{
    await DoWorkAsync(cancellationToken);
}
```

Handlers should propagate the token to operations that support cancellation, for example:

```csharp
await repository.SaveChangesAsync(cancellationToken);
```

Or:

```csharp
await httpClient.SendAsync(
    request,
    cancellationToken);
```

Cancellation allows handlers to respond to supported cancellation conditions, including application shutdown where the token is wired to the relevant shutdown signal.

Cancellation is cooperative. An operation may continue if the underlying API ignores the token or cannot cancel the work.

Do not automatically treat every cancellation as a business failure. Whether a cancelled job should be retried depends on the cancellation cause and the configured Hangfire behavior.

---

## 17. Integration Rules

The following rules define how application features integrate with Scheduling.

### Contracts and Boundaries

* Do not reference Hangfire from Application business logic.
* Do not call Hangfire static APIs from job handlers.
* Use `IJobScheduler` to enqueue or schedule jobs.
* Keep `IJob` payloads focused on execution data.
* Keep business logic inside the owning feature's handler or the appropriate application/domain components.
* Keep recurring triggers thin and free of business execution logic.

### Discovery and Registration

* Use `IRecurringJobTrigger` for convention-based recurring schedules.
* Do not manually register handlers when they are already covered by automatic discovery.
* Do not manually register recurring jobs when the bootstrapper owns their registration.
* Ensure feature assemblies are included in the configured discovery process.
* Keep recurring job IDs stable and unique.

### Business Meaning and Transactions

* The owning feature determines the business meaning of its job.
* Publish an integration event through the Outbox when a business operation establishes a relevant cross-context fact.
* Persist the business state change and corresponding Outbox record in the same Unit of Work transaction.
* Do not assume a Hangfire scheduling call participates in the business database transaction.
* Design handlers for safe behavior under duplicate execution.

### Execution and Reliability

* Do not duplicate centralized timeout or retry policies inside handlers.
* Use cancellation tokens for operations that support cancellation.
* Preserve appropriate structured logging and execution context.
* Account for persisted jobs when changing payload types.
* Treat scheduling, handler execution, and external side effects as operations that can fail independently.

---

## 18. Common Integration Scenarios

### 18.1 Fire-and-Forget Operation

```text
Application Feature
     |
     v
IJobScheduler.Enqueue(...)
     |
     v
Hangfire
     |
     v
JobOrchestrator
     |
     v
JobHandler
     |
     v
Complete or Fail
```

### 18.2 Recurring Housekeeping

```text
RecurringJobBootstrapper
     |
     v
Recurring Schedule
     |
     v
MyJobTrigger
     |
     v
IJobScheduler.Enqueue(...)
     |
     v
MyJobHandler
     |
     v
Perform Operational Cleanup
     |
     v
Complete
```

No business integration event is produced unless the operation establishes a business fact that another context needs.

### 18.3 Recurring Business Process

```text
RecurringJobBootstrapper
     |
     v
BusinessProcessTrigger
     |
     v
IJobScheduler.Enqueue(...)
     |
     v
BusinessProcessHandler
     |
     v
Business State Change
     +
Integration Event
     |
     v
Unit of Work Transaction
     |
     v
Outbox
     |
     v
Other Bounded Contexts
```

### 18.4 Delayed Job

```text
Application Feature
     |
     v
IJobScheduler.Schedule(job, futureTime)
     |
     v
Hangfire
     |
     v
Wait Until Eligible for Execution
     |
     v
JobOrchestrator
     |
     v
JobHandler
     |
     v
Complete or Fail
```

### 18.5 Manual Invocation of an Existing Job

An authorized administrative operation can enqueue an existing job without implementing the business operation a second time.

```text
Authorized Admin Endpoint
     |
     v
IJobScheduler.Enqueue(existingJob)
     |
     v
Same Execution Gateway
     |
     v
Same Handler
```

The endpoint must enforce authorization and validate the request. Reusing a handler does not automatically make an administrative invocation safe or authorized.

---

## 19. Job Type Comparison

| Concern                                | Fire-and-forget                      | Recurring                                   | Delayed one-shot                     |
| -------------------------------------- | ------------------------------------ | ------------------------------------------- | ------------------------------------ |
| Payload                                | Implement `IJob`                     | Implement `IJob`                            | Implement `IJob`                     |
| Handler                                | Implement `IJobHandler<TJob>`        | Implement `IJobHandler<TJob>`               | Implement `IJobHandler<TJob>`        |
| Dispatch mechanism                     | `Enqueue(...)`                       | Trigger calls `Enqueue(...)`                | `Schedule(...)`                      |
| Trigger required                       | No                                   | Yes, `IRecurringJobTrigger`                 | No                                   |
| `[RecurringJob]` attribute             | No                                   | Yes                                         | No                                   |
| Manual handler registration            | Not when automatic discovery applies | Not when automatic discovery applies        | Not when automatic discovery applies |
| Manual Hangfire recurring registration | No                                   | No, when the bootstrapper owns registration | No                                   |
| Common execution gateway               | Yes                                  | Yes                                         | Yes                                  |

---

## 20. Summary

The Scheduling feature provides one consistent model for background execution.

* **`IJob`** defines the job payload.
* **`IJobHandler<TJob>`** defines the work performed.
* **`IJobScheduler`** provides the Application-facing dispatch contract.
* **`IRecurringJobTrigger`** connects recurring schedules to job payloads.
* **`[RecurringJob]`** declares recurring schedule metadata.
* **`JobOrchestrator`** provides the centralized execution and policy pipeline.
* **Hangfire** provides the Infrastructure implementation for persisted scheduling and worker execution.

Three execution modes are supported:

* **Fire-and-forget:** `Enqueue(...)`.
* **Recurring:** Trigger + `[RecurringJob]`.
* **Delayed one-shot:** `Schedule(...)`.

Business classification determines whether an integration event is needed:

* **Business-process job:** When the operation establishes a relevant business fact, persist the business change and Outbox event in the same transaction.
* **Housekeeping job:** Perform operational work without publishing a business integration event when no such business fact is produced.

The automatic discovery model removes the need for routine manual handler registration and recurring schedule registration, provided the relevant assemblies and dependencies are configured correctly.

By following these integration boundaries, application features remain decoupled from Hangfire, scheduling concerns remain centralized, and background jobs share a consistent execution pipeline without sacrificing business transaction integrity or safe failure handling.

---

**Document version:** 1.0 — Final

**Feature:** Scheduling / Background Job Execution Gateway
