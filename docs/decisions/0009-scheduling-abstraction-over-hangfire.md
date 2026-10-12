# ADR-0009: Scheduling Abstraction over Hangfire

* **Status:** Accepted
* **Date:** 2026-10-13
* **Decision Type:** Architecture / Background Processing

## Context

The E-Commerce modular monolith requires background processing for operations that should execute outside the immediate HTTP request or business transaction.

Examples include:

* Processing transactional Outbox records.
* Expiring pending orders.
* Monitoring failed or dead-lettered operations.
* Cleaning up files scheduled for deletion.
* Executing recurring maintenance tasks.

The application uses Hangfire to provide persistent job scheduling, background execution, retry handling, and recurring-job infrastructure.

However, allowing application use cases and business modules to depend directly on Hangfire would couple business orchestration to a specific infrastructure library. It would also encourage inconsistent job registration, execution, retry, and error-handling patterns across the solution.

A centralized Scheduling capability provides a consistent execution pipeline while keeping business modules responsible for their own job payloads and behavior.

The architectural decision is how to integrate Hangfire without allowing its implementation details to spread through the Application and Domain layers.

## Decision

**Use Hangfire as the Infrastructure implementation behind a centralized Scheduling capability, exposed to Application through application-owned scheduling contracts.**

Application code must not reference Hangfire types or invoke Hangfire APIs directly. Infrastructure owns Hangfire configuration, storage, workers, dispatching, recurring-job registration, and integration with the scheduling contracts.

The Scheduling capability provides a controlled execution gateway for background jobs. It centralizes execution concerns without taking ownership of business workflows.

### 1. Application-Owned Contracts

The Application layer defines the contracts required to describe and schedule background work.

The established scheduling abstractions include:

* `IJob` — identifies a job payload contract.
* `IJobContext` — exposes execution metadata, such as job ID, correlation ID, attempt number, and enqueue time.
* `IJobHandler<TJob>` — defines the handler responsible for executing a job.
* `IJobScheduler` — provides operations for enqueueing, delayed scheduling, and recurring-job registration.
* `IRecurringJobTrigger` — identifies recurring-job configuration through application-owned metadata.
* `IJobPolicy` — represents reusable execution policies applied by the job pipeline.

These contracts describe application requirements without exposing Hangfire-specific types.

Job payloads should be serializable, stable, and suitable for persistence between scheduling and execution. They should contain the data required to identify or perform the work rather than depend on retaining an in-memory object graph.

The Domain layer does not depend on the Scheduling capability or Hangfire. Domain behavior may produce application-level consequences that lead to background work, but the Domain layer itself does not schedule infrastructure jobs.

### 2. Centralized Execution Pipeline

All jobs execute through the centralized Scheduling pipeline.

The pipeline is responsible for resolving the appropriate handler, establishing execution context, applying configured policies, and invoking the handler.

The intended execution flow is:

1. A business use case or infrastructure component requests that a job be scheduled through `IJobScheduler`.
2. The Hangfire-backed scheduler persists or registers the job through Hangfire.
3. Hangfire invokes the centralized dispatcher when execution is due.
4. The dispatcher establishes the application job context and resolves the corresponding handler.
5. The job orchestration component applies the configured execution policies.
6. The handler performs the requested work.
7. Failures propagate through the pipeline so the configured retry and operational mechanisms can respond appropriately.

The centralized pipeline prevents every handler from having to implement its own Hangfire integration, execution-context construction, or policy orchestration.

### 3. Infrastructure-Owned Hangfire Integration

Hangfire-specific implementation details remain in Infrastructure.

These include:

* Hangfire storage and server configuration.
* The concrete `IJobScheduler` implementation.
* The Hangfire dispatcher and execution-context adapter.
* Integration with Hangfire's execution metadata and cancellation mechanisms.
* Automatic discovery and registration of recurring jobs.
* Job serialization and dependency-injection integration.
* Hangfire retry configuration and operational integration.

The Infrastructure composition root may reference Hangfire directly to configure these components.

Application and Domain code must not reference `Hangfire`, `Hangfire.Core`, `PerformContext`, `RecurringJob`, `IRecurringJobManager`, or other Hangfire-specific APIs.

The application contracts must remain usable independently of the selected scheduling implementation.

### 4. Business Modules Own Job Behavior

The Scheduling capability owns the execution pipeline, not the business logic executed by jobs.

Each business feature or technical capability defines its own job payloads, handlers, and recurring-job declarations where required.

Examples include:

* Ordering owns the business rules for expiring pending orders.
* Outbox owns the logic for processing pending integration-event records.
* File Storage owns the rules for cleaning up files eligible for deletion.

Scheduling provides the mechanism for invoking those operations. It must not absorb their business rules into a centralized collection of job handlers.

This arrangement keeps job behavior close to the module that owns the relevant data and responsibilities.

### 5. Recurring-Job Discovery and Registration

Recurring jobs are declared through application-owned contracts and metadata.

Infrastructure discovers the relevant declarations and registers them with Hangfire during application startup.

The bootstrap process is responsible for translating application-owned recurring-job definitions into Hangfire registrations.

Business handlers and recurring-job declarations must not directly call Hangfire's static `RecurringJob` API or resolve Hangfire's concrete recurring-job manager.

Recurring-job identifiers must be stable. Registration must be deterministic and must update existing recurring definitions when their configuration changes, rather than creating unintended duplicates.

The application must also ensure that recurring-job registration is performed by the appropriate startup infrastructure and that multiple application instances do not create conflicting registrations.

### 6. Retry Ownership and Failure Handling

Hangfire owns the scheduling system's job retry mechanism.

The Scheduling pipeline may apply execution policies such as timeouts, circuit breakers, or other controls where they provide a clear benefit. These policies must not accidentally introduce a second independent retry loop that multiplies Hangfire's retries.

A failure should propagate through the execution pipeline unless it has been deliberately handled according to the job's documented semantics.

Retryable failures should remain visible to Hangfire so its configured retry policy can act on them.

The system must distinguish between:

* **Retry:** Execute a failed job again according to the configured policy.
* **Idempotency:** Prevent repeated execution from applying duplicate effects.
* **Timeout:** Limit the duration of an execution where the underlying operation supports cancellation or another effective timeout mechanism.
* **Circuit breaking:** Temporarily limit calls to a failing dependency when the policy's state and lifetime provide meaningful protection.

These mechanisms are complementary, not interchangeable.

In particular, a circuit breaker whose state is recreated for every job execution cannot reliably accumulate failures across executions. Any cross-execution protection must use appropriately shared state and a suitable concurrency model.

Timeouts and cancellation are also cooperative in many .NET operations; they do not guarantee that every operation can be forcibly terminated.

### 7. Durable Scheduling Does Not Replace Transactional Outbox

The Scheduling capability does not replace the Transactional Outbox.

For integration-event publication, the business transaction must persist the business changes and the corresponding Outbox records atomically. A background job subsequently processes those records.

The application must not rely on scheduling a Hangfire job inside a business transaction as a substitute for atomically persisting the publication intent.

A transaction could commit while a separately scheduled job fails to register, or scheduling could succeed while the business transaction later rolls back.

The Outbox provides the durable record of required publication work; Hangfire provides the mechanism for executing the processing job.

Other business jobs must use an appropriate durability strategy for their requirements. Where losing a scheduling request would violate a business guarantee, the request must be persisted atomically with the relevant business changes or recovered through another explicitly designed mechanism.

### 8. Dependency Injection and Handler Discovery

Infrastructure configures the Scheduling pipeline through the application's dependency-injection composition root.

Handlers and their required dependencies are registered through the established discovery and registration mechanism.

The dispatcher must resolve handlers with appropriate dependency-injection lifetimes and ensure scoped services are used within a valid execution scope.

Automatic discovery reduces repetitive registration, but it must not silently register ambiguous handlers, create duplicate recurring-job identities, or conceal missing dependencies.

Startup registration failures should be visible rather than allowing the application to operate with silently missing background jobs.

## Consequences

### Positive Consequences

* Application code remains independent of Hangfire.
* Background work follows one consistent execution pipeline.
* Job context, handler resolution, and execution policies are managed centrally.
* Business modules retain ownership of their job behavior and business rules.
* Hangfire can be replaced or supplemented without requiring its APIs to be removed from every application module.
* Retry behavior has a clear owner.
* Recurring jobs are registered through a consistent and maintainable mechanism.
* The design supports a growing number of background jobs without distributing scheduling infrastructure throughout the solution.

### Trade-Offs

* The scheduling abstraction, dispatcher, discovery mechanism, and policy pipeline introduce additional implementation complexity.
* The centralized pipeline must be maintained as a platform-level capability.
* Job payload contracts require compatibility considerations because persisted jobs may execute after a deployment.
* Automatic discovery can make registration less explicit and therefore requires validation and observability.
* Abstracting scheduling may hide provider-specific features unless those features are deliberately exposed through suitable application-owned contracts.
* Durable scheduling alone does not make the business effects of a job exactly once; handlers still require appropriate idempotency and failure-handling strategies.

These trade-offs are accepted because the application has multiple background-processing responsibilities and benefits from consistent execution and dependency boundaries.

## Alternatives Considered

### 1. Use Hangfire Directly Throughout the Application

Business modules and application handlers would call Hangfire APIs directly to enqueue and register jobs.

**Rejected:** This would spread infrastructure dependencies, complicate testing and maintenance, and encourage inconsistent job execution and retry behavior.

### 2. Allow Each Feature to Implement Its Own Scheduling Infrastructure

Each module would configure its own dispatching, retries, recurring-job registration, and execution context.

**Rejected:** Business modules should own job behavior, but duplicated scheduling infrastructure would increase maintenance costs and weaken consistency.

### 3. Build a Custom Background Job Engine

The application would implement its own persistence, scheduling, workers, retry policies, recurring execution, and operational tooling.

**Rejected:** The existing requirements do not justify replacing a mature scheduling framework with a custom execution engine.

### 4. Introduce a Message Broker for All Background Work

All background jobs would be represented as messages processed through a broker.

**Not adopted as the default:** A broker introduces additional infrastructure and messaging semantics that are not required for the current single-process modular monolith. It may be reconsidered if independent consumers, cross-process communication, or other concrete requirements justify it.

## Reconsideration Criteria

Revisit this decision if:

* Background processing moves to independently deployed worker services.
* The application requires scheduling capabilities that cannot be represented cleanly by the existing contracts.
* Hangfire no longer satisfies durability, operational, throughput, or scheduling requirements.
* The system introduces a broker for independently justified cross-process messaging needs.
* The current centralized pipeline becomes a bottleneck or adds unnecessary complexity for the actual job workload.

Any change should preserve explicit business ownership, appropriate durability guarantees, and the separation between application contracts and infrastructure implementation.

## Related Decisions

* [ADR-0001: Modular Monolith](0001-modular-monolith.md)
* [ADR-0002: Clean Architecture Dependency Direction](0002-clean-architecture-dependency-direction.md)
* [ADR-0004: Domain Events vs. Integration Events](0004-domain-events-vs-integration-events.md)
* [ADR-0005: Transactional Outbox](0005-transactional-outbox.md)
* [ADR-0006: Two-Phase Save in Unit of Work](0006-two-phase-save-in-unit-of-work.md)
* [ADR-0007: Per-Handler Idempotency for Integration Events](0007-per-handler-idempotency.md)
* [ADR-0012: Automatic Handler and Trigger Discovery](0012-automatic-handler-and-trigger-discovery.md)
* [ADR-0013: Business Process vs. Housekeeping Jobs](0013-business-process-vs-housekeeping-jobs.md)
* [ADR-0014: Single Deployable Unit](0014-single-deployable-unit.md)

## Architectural Principle

**Application defines the scheduling contracts, Infrastructure implements them through Hangfire, and business modules own the work being executed.**

The centralized Scheduling capability provides a controlled execution gateway without allowing Hangfire-specific concerns to leak into business logic or replacing the durability guarantees provided by the Transactional Outbox.
