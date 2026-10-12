# ADR-0013: Business Process Jobs vs. Housekeeping Jobs

* **Status:** Accepted
* **Date:** 2026-10-13
* **Decision Type:** Architecture / Background Processing

## Context

The E-Commerce modular monolith uses background jobs for both business workflows and technical maintenance.

Examples of business-related background work include expiring pending orders and processing integration events from the Transactional Outbox.

Examples of housekeeping work include cleaning up expired temporary data, deleting eligible files, and monitoring operational records that require attention.

Although both categories execute through the same scheduling infrastructure, they have different responsibilities, failure consequences, and ownership boundaries.

Business process jobs enforce or advance business workflows. Housekeeping jobs maintain the technical health, consistency, and operational condition of the application.

Without an explicit distinction, business rules may drift into infrastructure jobs, technical maintenance may become entangled with business workflows, and operational requirements may be handled inconsistently.

The architecture therefore needs a classification that preserves business ownership while allowing both categories to share the centralized Scheduling capability.

## Decision

**Classify background jobs according to their primary responsibility as either business process jobs or housekeeping jobs. Both categories use the centralized Scheduling capability, but each remains owned by the module responsible for the work.**

The classification is based on what the job does, not on whether it is recurring, delayed, or triggered by an event.

A recurring job can be a business process job, and a one-time job can be housekeeping. Similarly, a job may be technically implemented in Infrastructure while executing a business-owned operation.

### 1. Business Process Jobs

A business process job performs or advances behavior governed by business requirements.

Examples include:

* Expiring pending orders according to Ordering's rules.
* Advancing a business workflow after a defined delay.
* Processing durable integration events that cause business modules to react.
* Executing a business operation that must occur independently of an HTTP request.
* Performing a scheduled business action when its eligibility conditions are satisfied.

Business process jobs must remain associated with the module that owns the relevant business rules.

For example, Ordering owns the rules determining when a pending order expires. The job handler executes those rules, while Scheduling provides the execution mechanism.

The fact that a business process runs in the background does not transfer ownership of its behavior to the Scheduling capability.

#### Responsibilities

The owning business feature is responsible for:

* Defining the business conditions under which the job should act.
* Enforcing relevant business invariants.
* Validating the current state before applying a transition.
* Handling conflicts with concurrent business operations.
* Defining the consequences of success or failure.
* Ensuring repeated execution does not produce unintended duplicate effects.
* Defining any required business audit or traceability behavior.

Scheduling is responsible for invoking the handler through the established execution pipeline.

A job must not assume that the business state remains unchanged between scheduling and execution. The handler should re-evaluate the conditions required for the operation when it runs.

### 2. Housekeeping Jobs

A housekeeping job performs technical maintenance or operational support work that does not itself define a business workflow.

Examples include:

* Cleaning up expired temporary records.
* Removing files whose deletion has been authorized and whose retention period has elapsed.
* Monitoring failed or dead-lettered Outbox records.
* Performing technical cleanup of obsolete operational data.
* Maintaining infrastructure-related records according to established retention policies.
* Executing routine maintenance needed to keep the application operational.

Housekeeping jobs are owned by the technical capability responsible for the resource or operational concern.

For example, File Storage owns the technical process for deleting eligible physical files. The Scheduling capability provides the execution pipeline, while the owning feature or workflow determines whether the file is eligible for deletion.

Similarly, the Outbox capability owns the technical mechanisms for processing and monitoring Outbox records.

#### Responsibilities

The owning technical capability is responsible for:

* Defining the maintenance operation.
* Establishing eligibility and safety conditions.
* Handling partial failures and retry behavior.
* Avoiding unintended deletion or corruption.
* Providing appropriate logging and operational visibility.
* Applying retention and cleanup rules.
* Ensuring concurrent executions do not compromise data integrity.

Housekeeping jobs must not silently implement business rules that belong to a business feature.

### 3. Classification Examples

| Job                                             | Classification                                        | Owning module or capability               | Reason                                                                            |
| ----------------------------------------------- | ----------------------------------------------------- | ----------------------------------------- | --------------------------------------------------------------------------------- |
| Expire pending orders                           | Business process                                      | Ordering                                  | Enforces order-lifecycle rules                                                    |
| Process Outbox integration events               | Technical processing of business integration work     | Outbox                                    | Implements durable event-dispatch infrastructure                                  |
| Monitor failed or dead-lettered Outbox records  | Housekeeping / operational maintenance                | Outbox                                    | Monitors and supports recovery of technical processing records                    |
| Clean up eligible stored files                  | Housekeeping                                          | File Storage                              | Maintains the physical storage lifecycle                                          |
| Clean up expired temporary registration records | Housekeeping                                          | Identity capability                       | Removes expired temporary registration data                                       |
| Send a business-triggered order notification    | Business-triggered operation using technical delivery | Owning business feature and Notifications | The feature determines the notification's meaning; Notifications handles delivery |
| Execute a recurring database-maintenance task   | Housekeeping                                          | Relevant technical capability             | Maintains technical infrastructure                                                |

Some jobs combine business and technical responsibilities. In those cases, classify the job according to its primary purpose and keep distinct responsibilities separated where doing so improves ownership and maintainability.

In particular, processing an Outbox record is not automatically a housekeeping operation merely because it is performed in the background. It is durable technical processing that enables business modules to react to committed business events. The handler invoked by the integration event may perform business behavior in its own owning module.

### 4. Scheduling Is Shared Infrastructure

Both business process jobs and housekeeping jobs use the same centralized Scheduling capability.

The Scheduling capability provides:

* Application-owned job contracts.
* A centralized execution pipeline.
* Handler resolution and execution context.
* Recurring-job registration.
* Integration with Hangfire.
* Configured execution policies.
* Consistent retry and failure propagation.

It does not own the business logic or maintenance rules executed by the handlers.

This shared infrastructure avoids creating separate job engines for business processing and technical maintenance without a concrete need.

The distinction affects ownership, requirements, observability, and failure consequences—not the basic scheduling mechanism.

### 5. Durability Requirements Depend on the Job's Purpose

Not every job requires the same persistence strategy.

A recurring housekeeping job may be reconstructible from its recurring-job declaration and current database state. Losing an individual execution may be acceptable if the next execution safely retries the work.

By contrast, a business operation that must occur because a transaction committed cannot rely solely on an in-memory scheduling request.

Where a job represents required work arising from a business transaction, the application must ensure that the work is durably recorded or can be reliably recovered.

For integration-event publication, the existing Transactional Outbox persists the publication intent in the same database transaction as the originating business changes. A background job processes that durable work afterward.

The application must not treat calling `IJobScheduler.Enqueue` inside a business transaction as proof that the scheduling request and business changes are atomic.

Each job's durability requirements must be derived from the consequences of losing, delaying, or repeating its work.

### 6. Retry and Idempotency

Both categories must account for repeated execution.

Hangfire owns the configured scheduling retry mechanism. Handlers must propagate failures appropriately so the retry policy can respond.

Business process jobs must protect business invariants and avoid duplicate effects when retried.

Housekeeping jobs must avoid destructive or inconsistent behavior when executions overlap or resume after a failure.

For integration event handlers, durable per-handler idempotency uses the `ProcessedEvents` mechanism described in ADR-0007. This is distinct from generic job idempotency: the specific reliability mechanism depends on the operation being performed.

Idempotency may be achieved through database constraints, conditional state transitions, transactional processing records, provider-supported idempotency keys, or other suitable mechanisms.

The architecture does not assume that every job can execute exactly once.

### 7. Concurrency and Overlapping Executions

Recurring scheduling does not guarantee that a previous execution has finished before another execution becomes eligible.

A job must therefore account for overlapping executions when concurrency could compromise correctness.

Depending on the operation, appropriate safeguards may include:

* Conditional database updates.
* Unique constraints.
* Atomic work claiming.
* Transactional state transitions.
* Distributed or database-backed coordination when genuinely required.
* Safe, repeatable cleanup operations.

The safeguard must match the failure and concurrency characteristics of the operation.

A process-local lock is not sufficient to coordinate multiple application instances. Conversely, introducing distributed locking is not automatically justified in a single-process deployment when simpler database safeguards are sufficient.

### 8. Observability and Recovery

Both categories require enough observability to diagnose failures, but the operational indicators may differ.

Business process jobs may need visibility into:

* Business records affected.
* State transitions attempted.
* Eligibility conditions that were not satisfied.
* Retry outcomes and persistent failures.
* Correlation with the originating business operation.

Housekeeping jobs may need visibility into:

* Records or resources examined.
* Items successfully processed or removed.
* Items skipped because safety conditions were not met.
* Failed cleanup attempts.
* Backlog size and repeated failures.

The Scheduling capability provides common execution metadata and infrastructure-level observability. The owning module provides operation-specific context.

Monitoring a job's execution is not a substitute for verifying the business or technical outcome it was intended to achieve.

## Consequences

### Positive Consequences

* Business behavior remains owned by business features.
* Technical maintenance remains owned by the relevant technical capabilities.
* Both job categories use a consistent scheduling and execution pipeline.
* Durability requirements can be evaluated according to the consequences of losing work.
* Retry and idempotency requirements are explicit rather than assumed.
* Operational monitoring can distinguish business failures from infrastructure-maintenance failures.
* The design supports a growing number of background jobs without introducing separate execution frameworks unnecessarily.

### Trade-Offs

* Some jobs require careful classification when business and technical responsibilities overlap.
* Each owning module must define appropriate failure and recovery behavior.
* The shared Scheduling capability must support different operational needs without absorbing job-specific rules.
* Durable processing may require additional database records and transactional coordination.
* Reliable recurring execution still requires safeguards against concurrency and repeated effects.

These trade-offs are accepted because the distinction improves ownership and reliability without adding unnecessary infrastructure.

## Alternatives Considered

### 1. Treat All Background Jobs as Infrastructure Operations

Every job would be owned by Scheduling or Infrastructure, regardless of its behavior.

**Rejected:** This would move business rules away from the modules that own the relevant concepts and make the scheduling layer responsible for unrelated business decisions.

### 2. Build Separate Scheduling Systems for Business and Housekeeping Jobs

Each category would use a different execution engine and registration mechanism.

**Rejected:** The categories have different ownership and reliability requirements, but those differences do not currently justify separate scheduling infrastructure.

### 3. Classify Jobs Solely by Their Trigger

Recurring jobs would be classified as housekeeping, while event-triggered jobs would be classified as business processes.

**Rejected:** The trigger does not determine the job's responsibility. A recurring job may enforce a business rule, and an event-triggered job may perform technical maintenance.

### 4. Assume Every Job Can Be Retried Indefinitely

All failures would be handled through repeated execution without job-specific safeguards.

**Rejected:** Retries cannot replace idempotency, failure classification, operational monitoring, or recovery procedures. Some failures require intervention, and some external effects cannot be repeated safely without additional protection.

## Reconsideration Criteria

Revisit this decision if:

* Business process execution requires an independently managed workflow engine.
* Housekeeping workloads require isolation that the current Scheduling capability cannot provide.
* A job's ownership becomes ambiguous because multiple modules share responsibility for its outcome.
* Durability requirements change substantially.
* The application introduces independently deployed workers or external messaging infrastructure for separately justified reasons.

Any change should preserve clear ownership of business rules and technical maintenance, along with explicit durability and failure-handling guarantees.

## Related Decisions

* [ADR-0001: Modular Monolith](0001-modular-monolith.md)
* [ADR-0004: Domain Events vs. Integration Events](0004-domain-events-vs-integration-events.md)
* [ADR-0005: Transactional Outbox](0005-transactional-outbox.md)
* [ADR-0007: Per-Handler Idempotency for Integration Events](0007-per-handler-idempotency.md)
* [ADR-0009: Scheduling Abstraction over Hangfire](0009-scheduling-abstraction-over-hangfire.md)
* [ADR-0010: File Storage Is a Technical Capability, Not a Business Bounded Context](0010-file-storage-not-a-bounded-context.md)
* [ADR-0011: Notification Categories and Delivery Responsibilities](0011-notification-categories.md)
* [ADR-0012: Automatic Handler and Trigger Discovery](0012-automatic-handler-and-trigger-discovery.md)

## Architectural Principle

**Business features own business processes; technical capabilities own housekeeping; Scheduling provides the shared execution mechanism.**

Job classification determines ownership and operational requirements, while durability, retry, and idempotency strategies are selected according to the consequences of failure and repeated execution.
