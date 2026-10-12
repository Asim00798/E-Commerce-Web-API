# E-Commerce.Scheduling — Technical Requirements Document

**Project:** E-Commerce Modular Monolith
**Feature:** Scheduling / Background Job Execution
**Platform:** .NET 8 / ASP.NET Core
**Scheduler:** Hangfire with SQL Server Storage
**Version:** 1.0 — Final Implementation Specification
**Status:** Final

---

## 1. Purpose

The Scheduling feature provides a **scheduler-agnostic abstraction** and a **controlled execution gateway** for background work across the application.

It allows business features to define background jobs without depending on the underlying scheduler. It also centralizes cross-cutting execution concerns, including policy composition, structured logging, correlation metadata, and cancellation handling.

The feature provides three primary capabilities:

1. **Scheduler abstraction:** Enqueue immediate jobs, schedule delayed jobs, and register recurring work without exposing Hangfire to the Application layer.
2. **Controlled execution gateway:** Execute job handlers through a common orchestration pipeline that applies configured policies and emits structured telemetry.
3. **Automatic discovery:** Discover job handlers and recurring triggers through assembly scanning, minimizing per-job registration and startup wiring.

The Scheduling feature does not own business logic, define which jobs individual features require, or maintain a separate business-level job state machine. Job persistence and scheduler lifecycle management belong to the underlying scheduling provider.

The Scheduling module provides the execution mechanism; consuming features define the work.

### Version Note

Version 1.0 formalizes Scheduling as a first-class technical capability with the following design:

* `JobOrchestrator` as the execution gateway.
* `IJobPolicy` as the policy-composition contract.
* `RecurringJobBootstrapper` using `IRecurringJobManager` instead of relying on the static `RecurringJob` API during startup.
* Feature-local job payloads, handlers, and triggers.
* Hangfire with SQL Server storage as the current scheduling provider.

---

## 2. Architectural Decision

### 2.1 Scheduling Is Not a Business Bounded Context

Scheduling is a **technical execution subsystem**, not a business bounded context.

It has no:

* Business aggregates.
* Business domain events.
* Business invariants.
* Business workflows.
* Feature-specific business state.

Jobs remain owned by the features they serve. Scheduling supplies the shared execution pipeline through which those jobs run.

### 2.2 Cross-Cutting Execution Capability

The application-facing Scheduling abstractions reside under:

```text
Application/
└── Modules/
    └── Scheduling/
```

The subsystem is consumed by multiple features.

| Feature    | Example Background Work                                        |
| ---------- | -------------------------------------------------------------- |
| Finance    | Reconcile payments and refunds                                 |
| Ordering   | Expire pending orders and send feedback requests               |
| Onboarding | Clean up expired registrations                                 |
| Files      | Process pending deletions and remove orphaned physical files   |
| Outbox     | Dispatch integration events and monitor dead-lettered messages |
| Identity   | Initiate verification and password-reset notification delivery |

These examples describe consuming features, not responsibilities owned by Scheduling.

### Application-Facing Abstractions

* `IJob`
* `IJobContext`
* `IJobHandler<TJob>`
* `IJobScheduler`
* `IJobOrchestrator`
* `IRecurringJobTrigger`
* `IJobPolicy`

### Infrastructure Implementations

* `HangfireJobScheduler`
* `HangfireJobDispatcher`
* `JobExecutionContext`
* `RecurringJobBootstrapper`

The dependency direction is:

* The Application layer never references Hangfire.
* Infrastructure implements the scheduler-facing adapters.
* Infrastructure must not take ownership of business logic merely because that logic runs in the background.
* Feature-local handlers use application contracts and services rather than scheduler-specific APIs.

These boundaries are enforced through project references and dependency registration.

---

## 3. Architectural Goal

The primary architectural goal is:

> **Business features define what work should happen in the background; Scheduling provides a scheduler-agnostic, policy-governed execution path with automatic discovery and minimal manual wiring.**

The intended dependency structure is:

```text
Application Features
        |
        | depend on
        v
Scheduling Abstractions
        |
        v
Application Layer
        ^
        | implements / adapts
        |
Infrastructure Layer
(Hangfire Adapter)
        |
        v
Hangfire Runtime
        |
        v
SQL Server Storage
```

The Application layer owns the scheduling contracts and execution orchestration. Infrastructure integrates those contracts with Hangfire.

Application code and business handlers must never depend directly on the concrete scheduler.

**Provider replacement boundary:** Replacing Hangfire should not require changes to job payloads or business handlers. The infrastructure adapter and any provider-specific bootstrap or configuration code would need to change. Provider-specific behavior must not be assumed to be identical across implementations.

---

## 4. Cross-Cutting Usage

Every feature that schedules background work consumes Scheduling through its application-facing abstractions.

| Feature    | Scheduling Responsibility                                                                  |
| ---------- | ------------------------------------------------------------------------------------------ |
| Finance    | Schedule payment and refund reconciliation                                                 |
| Ordering   | Expire pending orders, send feedback requests, and process shipment-related follow-up work |
| Onboarding | Clean up expired registration records                                                      |
| Files      | Process pending deletions and remove eligible orphaned files                               |
| Outbox     | Dispatch integration events and monitor dead-lettered messages                             |
| Identity   | Initiate verification and password-reset notification delivery                             |

The division of responsibility is explicit:

* **Scheduling** provides the mechanism for scheduling and executing work.
* **Consuming features** define the job payloads, execution behavior, and business meaning.
* **Infrastructure** adapts the abstractions to Hangfire.
* **Hangfire** manages persisted scheduling state, worker execution, retries, and scheduler lifecycle.

No consuming feature interacts directly with Hangfire.

Integration-event delivery continues to use the Outbox capability. Scheduling executes the Outbox processing job; it does not replace the Outbox's durability or consistency guarantees.

---

## 5. Payload vs. Handler vs. Trigger

Job payloads, handlers, and recurring triggers have separate responsibilities.

| Concept                                    | Layer                                                                                     | Purpose                                                                      |
| ------------------------------------------ | ----------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------- |
| Job payload (`IJob`)                       | Application                                                                               | Serializable data describing a job execution                                 |
| Job handler (`IJobHandler<TJob>`)          | Application, or an appropriate infrastructure implementation for infrastructure-only work | Performs the operation represented by the payload                            |
| Recurring trigger (`IRecurringJobTrigger`) | Application or Infrastructure, depending on ownership                                     | Defines the recurring schedule entry point and enqueues the relevant payload |

The normal execution relationship is:

```text
Recurring Trigger or Application Caller
                 |
                 | submits payload
                 v
            IJobScheduler
                 |
                 v
          Hangfire Storage
                 |
                 v
          Hangfire Worker
                 |
                 v
       HangfireJobDispatcher
                 |
                 v
        IJobOrchestrator
                 |
                 v
       IJobHandler<TJob>
                 |
                 v
            Job Work
```

A recurring trigger determines **when work is submitted**. A handler determines **what happens when the payload is executed**.

A single handler may be invoked by multiple triggers or by direct calls to `IJobScheduler`. The mapping between triggers, payloads, and handlers is therefore not necessarily one-to-one.

Recurring trigger callbacks are scheduler entry points. They should remain thin and delegate actual work to the normal job execution pipeline. The trigger callback itself does not automatically receive the same `JobOrchestrator` policies unless it is explicitly routed through that gateway.

---

## 6. Job Metadata

The runtime execution context describes the scheduler metadata associated with a running job.

```csharp
public interface IJobContext
{
    string JobId { get; }

    string CorrelationId { get; }

    int Attempt { get; }

    DateTime QueuedAt { get; }
}
```

| Field           | Meaning                                                                               |
| --------------- | ------------------------------------------------------------------------------------- |
| `JobId`         | Identifier assigned to the scheduler's background job                                 |
| `CorrelationId` | Identifier used to correlate execution logs and related operations                    |
| `Attempt`       | Current execution attempt, with the first attempt represented as `1`                  |
| `QueuedAt`      | Scheduler-provided job creation or enqueue timestamp, normalized to UTC by convention |

### Context Rules

* The dispatcher constructs the runtime context from the metadata available from Hangfire.
* The current implementation may use the Hangfire job ID as the correlation ID.
* A scheduler job ID is not automatically the same as the correlation ID of the original HTTP request or business operation. Propagating the originating correlation ID requires carrying it explicitly in the job payload or associated metadata.
* Attempt numbering must follow the retry metadata exposed by the configured Hangfire version and be normalized consistently.
* `QueuedAt` must reflect the timestamp actually available from the scheduler. If the runtime exposes creation time rather than the precise enqueue time, the implementation must not describe it as an exact enqueue timestamp.

The context is primarily for execution diagnostics. Handlers may use it for logging, but business decisions must not depend on scheduler-specific identifiers or attempt numbers.

---

## 7. Responsibilities

### 7.1 Application Layer

The Application layer owns the scheduler-independent contracts and execution orchestration.

Its responsibilities include:

* Declaring the scheduling abstraction consumed by application code.
* Declaring the job-handler contract.
* Declaring the recurring-trigger contract.
* Declaring the execution-gateway contract.
* Declaring the resilience-policy contract.
* Implementing `JobOrchestrator`.
* Composing configured policies around job-handler execution.
* Emitting structured execution telemetry through standard application logging abstractions.

The Application layer must not reference Hangfire types.

### 7.2 Infrastructure Layer

Infrastructure is responsible for adapting the application contracts to the selected scheduling provider.

Its responsibilities include:

* Translating scheduling requests into Hangfire client API operations.
* Dispatching Hangfire executions to `IJobOrchestrator`.
* Constructing runtime execution context from scheduler metadata.
* Discovering and registering recurring triggers at startup.
* Registering infrastructure adapters and provider services in dependency injection.
* Configuring Hangfire SQL Server storage and worker execution.
* Handling provider-specific integration details.

Infrastructure must not duplicate business logic that belongs in application handlers.

### 7.3 Feature-Local Jobs

Job payloads, handlers, and triggers belong with the feature they serve.

They must:

* Reside under the owning feature's `Jobs` folder.
* Keep payload data separate from execution logic.
* Use `IJobHandler<TJob>` for execution.
* Use `IRecurringJobTrigger` when a recurring schedule entry point is required.
* Be discovered and registered through the configured assembly-scanning process.
* Be schedulable without individual manual DI registration.

Infrastructure-only maintenance jobs may be implemented in Infrastructure when their responsibilities are genuinely infrastructure-specific. Their ownership must remain explicit, and they must still use the shared execution pipeline for the actual job payload.

---

## 8. Application Abstractions

### 8.1 `IJob`

Marker interface for job payloads.

```csharp
public interface IJob
{
}
```

Payloads are data-transfer objects. They carry parameters for an execution and contain no execution logic.

Payloads must be compatible with the configured Hangfire serializer and remain suitable for durable storage.

Serialization behavior depends on the configured serializer and its version. Payload design must therefore account for the serializer's requirements, including supported types, constructors, converters, and compatibility with persisted jobs across deployments.

### 8.2 `IJobContext`

Defines runtime execution metadata.

See [Section 6 — Job Metadata](#6-job-metadata).

### 8.3 `IJobHandler<TJob>`

Defines the handler contract.

```csharp
public interface IJobHandler<in TJob>
    where TJob : IJob
{
    Task HandleAsync(
        TJob job,
        CancellationToken cancellationToken);
}
```

Handlers are discovered through assembly scanning and registered as scoped services.

Handlers must accept cancellation and propagate it to supported downstream operations, including database and external-service calls where appropriate.

### 8.4 `IJobScheduler`

Defines the scheduling abstraction consumed by application code.

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

| Operation              | Purpose                                                 |
| ---------------------- | ------------------------------------------------------- |
| `Enqueue`              | Submit a job for immediate processing                   |
| `Schedule`             | Submit a one-shot job for execution at a specified time |
| `AddOrUpdateRecurring` | Register or update a cron-based recurring schedule      |

The implementation must define consistent UTC handling for scheduled timestamps.

`AddOrUpdateRecurring` is available as an abstraction, but the standard recurring-job workflow uses a feature-local trigger decorated with `RecurringJobAttribute`. Individual features should not manually register recurring jobs.

The provider adapter must also account for Hangfire API differences between versions. The contract above describes application-facing intent rather than a guarantee that every provider uses the same method signatures.

### 8.5 `IJobOrchestrator`

Defines the controlled execution gateway.

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

The dispatcher invokes the orchestrator after the scheduler has loaded the payload and started the execution.

The orchestrator resolves the corresponding handler, composes the configured policies, and executes the handler through the resulting pipeline.

### 8.6 `IRecurringJobTrigger`

Defines the entry point for recurring schedules.

```csharp
public interface IRecurringJobTrigger
{
    void Trigger();
}
```

A trigger typically submits a job payload through `IJobScheduler`.

Triggers are discovered at startup and registered according to their `RecurringJobAttribute`.

The trigger should not contain the business operation itself. It is a scheduling entry point, not a substitute for the job handler.

### 8.7 `RecurringJobAttribute`

Declares the recurring schedule identifier and cron expression without exposing Hangfire types to the Application layer.

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

The cron expression is represented as a string. The Application layer does not reference Hangfire's `Cron` helper.

The bootstrapper must validate the attribute values and ensure that each recurring identifier is unique within the configured scheduling scope.

### 8.8 `IJobPolicy`

Defines a policy composed around handler execution.

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

Policies are registered as `IJobPolicy` implementations and composed by `JobOrchestrator`.

The composition order must be deterministic and explicitly documented. For example:

```text
Timeout Policy
      |
      v
Circuit-Breaker Policy
      |
      v
Job Handler
```

Policy registration order must not be left to incidental DI enumeration behavior if the order affects execution semantics.

### 8.9 `JobExecutionException`

Represents an execution failure reported by the orchestration pipeline.

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

The orchestrator must preserve the original exception when wrapping a failure.

Failures must propagate to Hangfire when retry behavior is expected. The exception type alone does not determine whether Hangfire retries a job; the configured Hangfire retry policy and any exception filters determine the final behavior.

---

## 9. Provider Implementation Requirements

The current scheduling provider is **Hangfire with SQL Server storage**.

The implementation must support:

* Durable job persistence in SQL Server.
* Immediate, fire-and-forget job submission.
* Delayed one-shot job scheduling.
* Cron-based recurring schedules.
* Worker-based job execution.
* Retry and backoff behavior according to Hangfire configuration.
* Recurring schedule registration through the provider's manager API.
* Execution dispatch through `IJobOrchestrator`.

The implementation must not leak Hangfire dependencies into the Application layer.

Application code must never directly call:

```text
BackgroundJob
RecurringJob
JobStorage.Current
```

or use any other Hangfire type.

### Provider Replacement

The scheduler provider is replaceable at the infrastructure boundary.

A replacement with Quartz, Azure Functions, or another worker platform may require changes to:

* Scheduling adapters.
* Runtime dispatch.
* Recurring-trigger registration.
* Provider-specific configuration.
* Cancellation and retry integration.
* Serialization and persisted-job migration, where applicable.

The goal is to preserve job payloads, handlers, and trigger contracts wherever the replacement provider can support their semantics. A replacement must not be assumed to be a drop-in substitution if its scheduling, retry, or persistence behavior differs.

---

## 10. Configuration

### 10.1 Required Configuration

Hangfire SQL Server storage requires a connection string, currently expected from:

```text
ConnectionStrings:DefaultConnection
```

The connection string is read through Infrastructure configuration and passed to `AddSchedulingInfrastructure`.

The Hangfire storage schema must be initialized through the configured storage integration. Whether schema installation occurs automatically depends on the Hangfire version, storage configuration, and database permissions.

The application must not assume that schema creation will succeed without appropriate database access.

**Security requirement:** Connection strings and other secrets must not be hardcoded in source files. Use environment variables, development user secrets, or an appropriate production secret store.

### 10.2 Optional Configuration

The current implementation does not customize the following SQL Server storage options:

* Queue polling interval.
* Job expiration.
* Invisibility timeout.

Their effective values depend on the installed Hangfire version and storage configuration.

Policy parameters, including timeout duration and circuit-breaker threshold, are currently defined in `SchedulingInfrastructureExtensions`.

They may be moved into validated configuration if operational tuning becomes necessary.

### 10.3 Configuration Ownership

Scheduling reads the storage connection string and configures its technical execution behavior.

It does not define application-level configuration sections for business features.

Recurring schedules are declared in code through `RecurringJobAttribute`, rather than requiring a separate configuration entry for every trigger.

---

## 11. Dependency Injection Registration

Registration is centralized through:

```text
SchedulingInfrastructureExtensions.AddSchedulingInfrastructure
```

Responsibilities include registering:

* Hangfire storage and server services.
* `IJobScheduler` → `HangfireJobScheduler`.
* `IJobOrchestrator` → `JobOrchestrator`, using the appropriate application registration.
* `IJobPolicy` implementations, including `TimeoutPolicy` and `CircuitBreakerPolicy`.
* `HangfireJobDispatcher`.
* Job-handler scanning for the assemblies that contain handlers.
* Recurring-trigger discovery for the assemblies that contain triggers.
* `RecurringJobBootstrapper` and its required provider dependencies.

Application services and feature-local handlers are registered through `AddApplication` or the application's equivalent registration mechanism.

### Startup Sequence

1. Register Scheduling infrastructure during host configuration.
2. Register application services and feature-local handlers.
3. Build the host and initialize the required Hangfire storage and services.
4. Run `ScheduleRecurringJobs` before the host begins serving requests.
5. Supply every assembly containing recurring triggers to the bootstrapper.
6. Validate and register each discovered recurring schedule through `IRecurringJobManager`.

The startup process must fail clearly when a discovered trigger is invalid, missing its required attribute, or conflicts with another recurring schedule identifier.

The implementation must not rely on the static `RecurringJob` API being usable before Hangfire storage has been initialized.

---

## 12. Execution Flow

### 12.1 One-Shot and Scheduled Jobs

The normal job-payload execution path is:

```text
Application Code
       |
       v
IJobScheduler.Enqueue / Schedule
       |
       v
HangfireJobScheduler
       |
       v
Hangfire SQL Server Storage
       |
       v
Hangfire Worker
       |
       v
HangfireJobDispatcher.Dispatch
       |
       | Build execution context
       v
IJobOrchestrator.ExecuteAsync
       |
       v
JobOrchestrator
       |
       ├── Resolve IJobHandler<TJob>
       ├── Compose configured policies
       ├── Emit structured telemetry
       |
       v
IJobHandler<TJob>.HandleAsync
       |
       v
Business or Infrastructure Work
```

The handler performs the operation represented by the payload. The dispatcher and orchestrator remain responsible for the common execution path.

### 12.2 Recurring Jobs

The recurring execution path is:

```text
Hangfire Recurring Scheduler
       |
       v
IRecurringJobTrigger.Trigger()
       |
       v
IJobScheduler.Enqueue(payload)
       |
       v
Hangfire Storage
       |
       v
Hangfire Worker
       |
       v
HangfireJobDispatcher
       |
       v
JobOrchestrator
       |
       v
IJobHandler<TJob>.HandleAsync
```

The recurring trigger is a scheduler callback that submits the actual job payload. The payload's execution then follows the standard dispatch and orchestration pipeline.

**Important distinction:** The trigger callback itself is not automatically executed through `JobOrchestrator`. The single-gateway guarantee applies to the actual job-payload execution path unless trigger execution is explicitly wrapped as well.

---

## 13. No Direct Scheduler Access from Application Code

The following patterns are prohibited:

```text
Application → BackgroundJob.Enqueue(...)
Application → RecurringJob.AddOrUpdate(...)
Application → JobStorage.Current
Application → Any Hangfire Type
```

The required pattern is:

```text
Application
     |
     v
IJobScheduler
     |
     v
HangfireJobScheduler
     |
     v
Hangfire
```

This keeps the Application layer independent of the scheduling provider and centralizes provider-specific behavior in Infrastructure.

It also prevents feature code from bypassing the approved scheduling abstraction.

---

## 14. No Direct Scheduler Dependency in Handlers

Job handlers must not reference Hangfire types.

A handler receives:

* A deserialized job payload (`TJob`).
* A `CancellationToken`.
* Required services through dependency injection.

Depending on the feature, injected services may include:

* `DbContext`.
* Repositories and unit-of-work abstractions.
* `IClock`.
* `ILogger<T>`.
* `IOutboxMessageWriter`.
* Other required application services.

Handlers must not:

* Call `BackgroundJob`.
* Call `RecurringJob`.
* Access `JobStorage`.
* Resolve `PerformContext`.
* Depend on Hangfire's `IJobCancellationToken`.

Handlers contain the execution logic and remain independent of the scheduler.

### Cancellation Requirements

The dispatcher must translate the provider's cancellation mechanism into the `CancellationToken` passed to the orchestrator and handler.

The exact Hangfire cancellation API is version-dependent and must be implemented against the installed package version.

Cancellation should propagate through the handler to supported downstream operations. A cancellation request does not guarantee that a non-cancellable external operation or already-committed side effect can be undone.

---

## 15. Trigger Ownership

Recurring triggers are colocated with the feature they serve.

Illustrative locations include:

| Feature Location                                                         | Trigger                             |
| ------------------------------------------------------------------------ | ----------------------------------- |
| `Application/BoundedContexts/Orders/Jobs/ExpirePendingOrders`            | `ExpirePendingOrdersTrigger`        |
| `Application/BoundedContexts/Finance/Jobs/ReconcilePayments`             | `ReconcilePaymentsTrigger`          |
| `Application/BoundedContexts/Finance/Jobs/ReconcileRefunds`              | `ReconcileRefundsTrigger`           |
| `Application/BoundedContexts/Onboarding/Jobs/ExpiredRegistrationCleanup` | `ExpiredRegistrationCleanupTrigger` |
| `Infrastructure/Communication/Messaging/Outbox/Jobs/OutboxProcessing`    | `OutboxProcessingTrigger`           |
| `Infrastructure/Communication/Messaging/Outbox/Jobs/DeadLetterMonitor`   | `DeadLetterMonitorTrigger`          |
| `Infrastructure/Files/Jobs`                                              | `FileStorageCleanupTrigger`         |

These paths are illustrative and should follow the repository's established module naming conventions.

The Scheduling module owns the execution pipeline, not the feature-specific jobs.

A trigger should live next to the feature's job definition or in the appropriate infrastructure location when the job is infrastructure-owned.

---

## 16. Failed and Stuck Jobs

### 16.1 Retry Exhaustion

Jobs that fail are handled according to Hangfire's configured retry policy.

After automatic retries are exhausted, a job may remain in Hangfire's failed state for inspection and possible manual reprocessing.

Scheduling does not maintain a separate dead-letter table.

The scheduler owns its persisted execution state and failed-job storage. Application-level dead-letter monitoring, such as monitoring failed Outbox deliveries, remains a separate feature responsibility.

### 16.2 Worker Crashes and Stuck Jobs

Hangfire SQL Server storage uses configured storage and worker settings to manage jobs whose workers stop processing them.

The effective recovery behavior and invisibility timeout depend on the installed Hangfire version and configuration. The current implementation does not customize the relevant timeout.

The system must not assume that a worker crash means the handler's side effects were rolled back. A handler may have performed a side effect before the worker stopped.

### 16.3 Reprocessing

The Scheduling feature does not provide a separate manual-reprocessing interface.

A job may be reprocessed through:

* The Hangfire dashboard, if enabled and appropriately authorized.
* A new submission through `IJobScheduler`.
* An explicit application-level recovery process where required by the owning feature.

Re-enqueueing a job can repeat its effects. Handlers must therefore be idempotent where necessary or use suitable concurrency controls and uniqueness constraints.

---

## 17. Consistency Model

### 17.1 Job Durability

Once a job has been successfully persisted by Hangfire SQL Server storage, it can survive an application-process restart.

This guarantee assumes the database remains available and durable, the storage transaction succeeds, and the job has not expired or been removed according to storage policies.

Durability does not automatically make a job submission atomic with unrelated business changes in a separate transaction.

Where business-state changes and the obligation to perform background work must be committed atomically, use an appropriate transactional design. For integration-event delivery, use the Outbox so the business change and outbox record are persisted within the same unit of work and database transaction.

### 17.2 At-Least-Once Execution

The intended processing model is **at-least-once**, not exactly-once.

A job may execute again because of:

* Automatic retries.
* Worker interruption and recovery.
* Manual reprocessing.
* Duplicate submissions.
* Application-level recovery procedures.

The handler must not assume that a job executes exactly once.

Handlers with non-idempotent side effects must implement suitable idempotency protection, such as durable operation identifiers, unique constraints, concurrency controls, or provider-supported idempotency keys.

Exactly-once business effects cannot be guaranteed merely by using a durable scheduler.

### 17.3 Recurring Schedule Persistence

Hangfire persists recurring-job definitions in its storage. With the standard SQL Server storage schema, recurring definitions are represented using Hangfire's internal storage structures, including the `Hash` table.

The precise schema and internal representation are provider implementation details and should not be accessed directly by application code.

At startup, `RecurringJobBootstrapper` registers discovered triggers through `IRecurringJobManager`.

For a stable recurring job identifier, registration is intended to be idempotent:

* A missing schedule is created.
* An existing schedule is updated to match the declared configuration.
* A changed cron expression updates the registered schedule.

### 17.4 Policy Failure Modes

| Failure                           | Expected Behavior                                                                                                                           |
| --------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| Timeout deadline expires          | The operation is reported as timed out; Hangfire retry behavior depends on the exception propagated and configured retry policy             |
| Circuit breaker rejects execution | The policy throws an exception and the handler is not executed                                                                              |
| Handler cannot be resolved        | The orchestrator reports an execution failure                                                                                               |
| Handler throws                    | The exception propagates through the policy chain; the orchestrator logs and rethrows so Hangfire can apply its configured failure handling |
| Cancellation is requested         | Cancellation propagates where supported; final job-state handling depends on the provider's cancellation and shutdown behavior              |

**Timeout implementation requirement:** A timeout that only uses `Task.WaitAsync` can stop waiting for a task without cancelling the underlying operation. To cancel handler work cooperatively, the pipeline must pass a timeout-linked `CancellationToken` into the handler and its downstream operations. The implementation must not claim that the underlying work has stopped merely because the timeout expired.

**Circuit-breaker implementation limitation:** A scoped circuit-breaker instance resets its failure counter when its scope is disposed. If a new scope is created for each job execution, that registration does not provide a persistent, cross-execution circuit breaker. The current implementation's logging or per-execution behavior must not be described as effective cross-execution circuit breaking.

Hangfire retries and backoff can limit the frequency of repeated job attempts according to configuration, but they are not a substitute for a true circuit breaker.

---

## 18. Job Execution States

Job lifecycle states are managed by Hangfire, not by the Scheduling feature.

The orchestrator observes execution and records telemetry. It does not maintain a separate scheduler-level state machine.

A simplified lifecycle is:

```text
Enqueued
    |
    v
Processing
    |
    ├── Succeeded
    |
    ├── Failed
    |     |
    |     v
    |  Retry Scheduled
    |     |
    |     v
    |  Processing Again
    |
    └── Deleted
          (manual or administrative action)
```

Delayed jobs may pass through a scheduled state before becoming eligible for processing. Exact state transitions depend on Hangfire's lifecycle and configured filters.

When retries are exhausted, the job may remain in the failed state for inspection. The orchestrator logs the outcomes it observes but does not manage the scheduler's persisted job state.

---

## 19. Automatic Trigger Discovery

The system must discover and register recurring triggers without requiring individual manual registration.

### Discovery Mechanism

1. At startup, `ScheduleRecurringJobs` receives the assemblies to scan.
2. The bootstrapper enumerates concrete types implementing `IRecurringJobTrigger`.
3. Each discovered trigger is checked for `RecurringJobAttribute`.
4. A trigger missing the required attribute causes startup validation to fail with a clear `InvalidOperationException`.
5. The bootstrapper validates the recurring identifier and cron expression.
6. The bootstrapper resolves `IRecurringJobManager` through dependency injection.
7. It registers or updates the trigger using its recurring identifier and cron expression.

The bootstrapper uses `IRecurringJobManager` instead of the static `RecurringJob` class so that registration is performed through the configured provider services after the host and storage dependencies have been initialized.

The assemblies supplied to discovery must include every assembly containing recurring triggers. Scanning the Application assembly alone is insufficient when triggers also exist in Infrastructure or other feature assemblies.

### Developer Workflow

A developer creates the required job components:

```text
IJob Payload
     |
     v
IJobHandler<TJob>
     |
     v
IRecurringJobTrigger
     +
[RecurringJob("id", "cron")]
```

The expected workflow requires:

* No individual job-handler DI registration.
* No individual trigger DI registration.
* No manual `RecurringJob.AddOrUpdate` call for each trigger.
* No separate configuration entry for each code-defined recurring schedule.

These guarantees depend on the relevant assemblies being included in scanning and the application's registration conventions being followed.

---

## 20. Authorization

The Scheduling feature does not expose its own HTTP endpoints.

Jobs execute in-process and may be initiated by:

* Hangfire's recurring scheduler.
* Application code calling `IJobScheduler`.
* An explicitly authorized administrative or recovery operation.

If the Hangfire dashboard is enabled, it must be protected by appropriate authentication and authorization.

Dashboard security is a host-level operational concern rather than business logic owned by Scheduling.

### Background Principal

Background jobs do not execute within the original authenticated HTTP request context.

Handlers must not rely on:

* `ICurrentUser` being populated from the original request.
* `HttpContext` being available.
* Request-scoped authentication state being present.

If a job must act on behalf of a user, the required identity or actor information must be captured explicitly at submission time and validated or applied by the handler according to the use case.

User identity data should be minimized in payloads and must not be treated as a substitute for authorization checks.

---

## 21. Security Requirements

The Scheduling subsystem must:

* Avoid exposing its own HTTP endpoints.
* Avoid storing secrets, passwords, OTP values, or unnecessary raw personal data in job payloads.
* Avoid relying on an authenticated principal during background execution.
* Restrict access to job payloads and scheduler metadata.
* Protect Hangfire storage using appropriate database access controls.
* Prevent untrusted callers from submitting arbitrary jobs.
* Propagate cancellation during graceful shutdown where supported.
* Log execution failures without exposing secrets or unnecessary sensitive information.
* Keep the Hangfire dashboard disabled or appropriately protected when enabled.

### Payload Principle

Payloads should carry **identifiers rather than full domain objects or sensitive values** whenever practical.

Examples:

```text
OrderId
PaymentId
UserId
FileId
```

The handler retrieves the required current state through the owning repository or application service.

This reduces payload size, limits unnecessary personal-data persistence, and avoids treating a serialized snapshot as authoritative business state.

Where a job genuinely requires immutable data captured at submission time, the payload should contain only the minimum necessary information.

---

## 22. Final Folder Structure

The following structure illustrates the ownership boundaries.

### 22.1 Application Layer

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
        │
        ├── Attributes/
        │   └── RecurringJobAttribute.cs
        │
        ├── Coordination/
        │   └── JobOrchestrator.cs
        │
        ├── Exceptions/
        │   └── JobExecutionException.cs
        │
        └── Policies/
            ├── IJobPolicy.cs
            ├── CircuitBreakerPolicy.cs
            └── TimeoutPolicy.cs
```

### 22.2 Infrastructure Layer

```text
Infrastructure/
└── Scheduling/
    ├── Execution/
    │   └── JobExecutionContext.cs
    │
    ├── Hangfire/
    │   ├── HangfireJobDispatcher.cs
    │   └── HangfireJobScheduler.cs
    │
    └── Extensions/
        ├── SchedulingInfrastructureExtensions.cs
        └── RecurringJobBootstrapper.cs
```

### 22.3 Feature-Local Application Jobs

```text
Application/
└── BoundedContexts/
    └── <Context>/
        └── Jobs/
            └── <FeatureName>/
                ├── <FeatureName>Job.cs
                ├── <FeatureName>JobHandler.cs
                └── <FeatureName>Trigger.cs
```

Responsibilities:

| File                         | Responsibility                                                          |
| ---------------------------- | ----------------------------------------------------------------------- |
| `<FeatureName>Job.cs`        | Defines the `IJob` payload                                              |
| `<FeatureName>JobHandler.cs` | Implements `IJobHandler<TJob>`                                          |
| `<FeatureName>Trigger.cs`    | Implements `IRecurringJobTrigger` when recurring scheduling is required |

### 22.4 Infrastructure-Owned Jobs

```text
Infrastructure/
└── <Feature>/
    └── Jobs/
        └── <FeatureName>/
            ├── <FeatureName>JobPayload.cs
            ├── <FeatureName>JobHandler.cs
            └── <FeatureName>Trigger.cs
```

This layout is appropriate for genuinely infrastructure-specific operations. Business workflows should remain in the owning feature's Application layer.

The precise folder names may follow the repository's established module conventions. The architectural requirements are clear ownership, correct dependency direction, and discovery of every required handler and trigger.

---

## 23. What This Feature Is Not

### 23.1 Not a Business Bounded Context

Scheduling contains no business aggregates, invariants, or domain model.

### 23.2 Not a Message Broker

Scheduling does not provide:

* Publish/subscribe semantics.
* Topic routing.
* Message fan-out.

Integration events use the Outbox pipeline.

### 23.3 Not a Workflow Engine

Scheduling does not coordinate complex multi-step workflows with conditional branching and business-level workflow state.

Each job represents a unit of background execution. Multi-step business processes remain the responsibility of the owning application feature.

### 23.4 Not a Replacement for the Outbox

Scheduling executes background work.

The Outbox provides durable integration-event delivery by persisting event records atomically with the associated business transaction.

### 23.5 Not an Application-Level Scheduling Product

Scheduling does not provide user-defined calendars or business-managed schedules.

### 23.6 Not a Separate Dead-Letter Store

Hangfire owns its failed-job state and retry lifecycle. Feature-specific dead-letter monitoring and recovery remain separate concerns.

The Scheduling feature's responsibility is intentionally narrow:

> **Provide a scheduler-agnostic execution path with centralized orchestration, configured policy composition, structured telemetry, and automatic recurring-trigger discovery.**

---

## 24. Design Principles

The Scheduling feature follows these principles:

1. **Scheduler Independence**
   The Application layer depends on Scheduling abstractions, never on Hangfire.

2. **Controlled Execution Gateway**
   Job-payload execution passes through `JobOrchestrator`.

3. **Consistent Policy Composition**
   Configured policies are applied consistently to job-handler execution.

4. **Automatic Discovery**
   Handlers and triggers are discovered through assembly scanning when their assemblies are included in registration.

5. **Payload-Handler Separation**
   Payloads contain serializable data; handlers perform the operation.

6. **Trigger-Handler Separation**
   Triggers determine when recurring work is submitted; handlers execute the submitted job.

7. **Code-Defined Recurring Schedules**
   Recurring schedules are declared through `RecurringJobAttribute` and registered by the bootstrapper.

8. **Centralized Execution Telemetry**
   Job execution logs include structured job identifiers, attempt numbers, and correlation metadata.

9. **Deliberate Scope**
   Scheduling does not own business logic, business persistence, or the semantics of individual jobs.

10. **Explicit Dependency Boundaries**
    Handlers do not depend on Hangfire, and the Application layer does not depend on the scheduler implementation.

11. **At-Least-Once Safety**
    Handlers account for retries, duplicate submissions, and worker interruptions.

12. **Explicit Policy Limitations**
    Timeout and circuit-breaker behavior must reflect the actual cancellation and state-sharing capabilities of their implementations.

---

## 25. Final Architecture

```text
┌───────────────────────────────────────────────────────────┐
│                      APPLICATION                          │
│                                                           │
│  Feature Business Logic                                   │
│  Feature-Local Job Payloads and Handlers                  │
│                                                           │
│  IJobScheduler                                            │
│  IJobHandler<TJob>                                        │
│  IRecurringJobTrigger                                     │
│  IJobOrchestrator                                         │
│  IJobPolicy                                               │
│                                                           │
│  JobOrchestrator                                          │
│  TimeoutPolicy / CircuitBreakerPolicy                     │
└────────────────────────────┬──────────────────────────────┘
                             │
                             ▼
┌───────────────────────────────────────────────────────────┐
│                     INFRASTRUCTURE                        │
│                                                           │
│  HangfireJobScheduler                                     │
│  HangfireJobDispatcher                                    │
│  JobExecutionContext                                      │
│  RecurringJobBootstrapper                                 │
│  Hangfire SQL Server Storage Configuration               │
└────────────────────────────┬──────────────────────────────┘
                             │
                             ▼
┌───────────────────────────────────────────────────────────┐
│                    HANGFIRE RUNTIME                       │
│                                                           │
│  Client API                                               │
│  Worker Pool                                              │
│  Retry / Backoff                                          │
│  Recurring Scheduler                                      │
│  Job Lifecycle Management                                 │
└────────────────────────────┬──────────────────────────────┘
                             │
                             ▼
┌───────────────────────────────────────────────────────────┐
│                  SQL SERVER STORAGE                       │
│                                                           │
│  Hangfire Job and State Data                              │
│  Recurring Schedule Data                                   │
│  Queue and Worker Coordination Data                       │
└───────────────────────────────────────────────────────────┘
```

### Ownership Model

**Application layer**

* Owns scheduler-independent contracts.
* Owns the job execution gateway.
* Owns policy contracts and application-level policy implementations.
* Owns feature-local business job payloads and handlers.
* Depends on abstractions rather than the concrete scheduler.

**Infrastructure layer**

* Owns the Hangfire adapter and provider-specific integration.
* Owns recurring-trigger discovery and registration.
* Owns provider-specific storage and worker configuration.
* Implements infrastructure-only job handlers where appropriate.

**Hangfire runtime**

* Owns scheduler execution and job lifecycle management.
* Owns configured retry and backoff behavior.
* Owns failed-job state and recurring-schedule processing.

**SQL Server**

* Persists Hangfire's scheduler state according to the configured storage provider.

---

## 26. Final Architectural Conclusion

The Scheduling feature is a **technical subsystem that decouples the Application layer from the concrete scheduler**.

It provides:

* A scheduler-agnostic `IJobScheduler` abstraction.
* A controlled job-payload execution gateway through `JobOrchestrator`.
* Composable execution policies.
* Centralized structured execution telemetry.
* Automatic discovery and registration of handlers and recurring triggers.
* Durable scheduling through Hangfire and SQL Server.
* A clear provider boundary that limits scheduler-specific code to Infrastructure.

The feature does not own business logic. Job payloads, handlers, and triggers remain with the features they serve, while Scheduling owns the shared execution mechanism.

Individual jobs do not require manual DI registration when their handlers are included in the configured assembly-scanning process. Recurring triggers do not require individual manual `RecurringJob.AddOrUpdate` calls when they are discovered and registered by `RecurringJobBootstrapper`.

### Current Implementation Limitations

The architecture distinguishes the intended capabilities from the behavior currently guaranteed by the implementation:

* **Timeout:** A timeout is only able to cancel underlying handler work when a linked cancellation token is propagated through the execution pipeline and honored by the handler and downstream operations.
* **Circuit breaker:** A scoped policy instance does not accumulate failures across independent job executions. The current implementation must not be described as a functioning cross-execution circuit breaker.
* **Recurring triggers:** The trigger callback enqueues the actual payload; the payload's execution follows the orchestrator pipeline. The trigger callback itself is not automatically subject to the same policies.
* **Retries:** Hangfire owns retry behavior. Job handlers must tolerate repeated execution, and the configured exception-handling behavior must be verified against the actual provider configuration.
* **Discovery:** Automatic wiring works only when all assemblies containing relevant handlers and triggers are included in scanning.

The target architecture allows the scheduling capability to grow with the number of jobs without proportional growth in manual registration or duplicated infrastructure code.

It preserves a clear boundary between business execution and scheduler infrastructure while acknowledging that durability, cancellation, retry behavior, and resilience depend on the concrete implementation and its configuration.

---

## Appendix A. Summary of Formalization Changes

| Area                     | Documented Requirement                                                                  |
| ------------------------ | --------------------------------------------------------------------------------------- |
| Purpose and scope        | Formalizes Scheduling as a shared technical capability                                  |
| Architectural boundaries | Establishes scheduler independence and feature ownership                                |
| Job components           | Separates payloads, handlers, and recurring triggers                                    |
| Execution context        | Defines job ID, correlation ID, attempt number, and timestamp semantics                 |
| Application abstractions | Documents the scheduling, handler, orchestration, trigger, and policy contracts         |
| Provider integration     | Defines Hangfire and SQL Server responsibilities                                        |
| Configuration            | Defines connection-string ownership and secret-management requirements                  |
| DI and startup           | Documents registration, assembly scanning, and recurring-schedule bootstrap             |
| Execution flow           | Separates recurring trigger execution from job-payload execution                        |
| Failure handling         | Documents retry exhaustion, worker interruption, and reprocessing                       |
| Consistency              | Establishes durable persistence and at-least-once execution expectations                |
| Resilience policies      | Clarifies timeout cancellation and circuit-breaker limitations                          |
| Security                 | Defines payload minimization, background-principal boundaries, and dashboard protection |
| Folder structure         | Establishes feature-local ownership and infrastructure-specific locations               |
| Provider replacement     | Defines the infrastructure boundary while acknowledging provider-specific differences   |

---

**Document Version:** 1.0 — Final Implementation Specification
**Feature:** Scheduling / Background Job Execution
**Status:** Final
**Architecture:** E-Commerce Modular Monolith
