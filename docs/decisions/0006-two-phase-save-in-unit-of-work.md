# ADR-0006: Two-Phase Save in Unit of Work

* **Status:** Accepted
* **Decision Type:** Architecture
* **Date:** 2026-10-13

## Context

The E-Commerce modular monolith uses Domain-Driven Design, a shared Unit of Work, and a transactional outbox to coordinate business-state changes and reliable integration-event publication.

During a use case, domain aggregates may raise domain events as they undergo business state transitions. Processing those events may produce additional domain changes or integration events that must be persisted with the original business operation.

The system must ensure that:

* Domain events are processed within a controlled application workflow.
* Integration events are recorded in the outbox before the relevant transaction commits.
* Business changes and outbox records are committed atomically.
* Domain events are not accidentally processed repeatedly because of multiple persistence operations.
* A failed transaction does not leave partially committed business changes or outbox records.
* Application handlers do not need to manage the low-level persistence sequence themselves.

A conventional save operation that simply persists all currently tracked changes and then publishes integration events would not, by itself, establish these guarantees.

The Unit of Work therefore requires an explicit persistence workflow that coordinates domain-event processing and outbox creation before the final commit.

## Decision

The system adopts a **two-phase save workflow within the Unit of Work** to coordinate domain-event processing, integration-event creation, and transactional persistence.

The workflow separates preparation of the transaction from its final commit.

The intended sequence is:

1. Persist the initial tracked business changes within the active database transaction.
2. Collect and process the domain events raised by the operation.
3. Apply any additional domain changes resulting from event processing.
4. Convert relevant domain events into integration events where cross-module communication is required.
5. Add the resulting integration-event records to the outbox.
6. Persist the additional changes and outbox records within the same transaction.
7. Commit the transaction.

The two persistence phases are part of one logical business operation. They do not represent two independent committed transactions.

The exact implementation must preserve the established `IUnitOfWork` contract and ensure that both phases participate in the same database transaction.

### 1. Phase One: Initial Persistence

The first phase persists the changes made by the original use case.

These changes may include modified aggregates, newly created entities, and other tracked persistence operations.

The initial save occurs within an active transaction that remains uncommitted.

This phase makes the initial changes available to subsequent processing within the transaction without making them durable as a committed business operation.

A failure before the final commit must still allow the transaction to roll back.

### 2. Domain-Event Processing

After the initial persistence phase, the Unit of Work coordinates processing of the domain events collected from the relevant aggregates.

Domain events represent business occurrences, not infrastructure commands. Their handlers must follow the established event-processing responsibilities and dependency boundaries.

Processing may result in:

* Additional changes to tracked entities.
* Creation of other domain objects.
* Additional domain events, subject to the event-processing policy.
* Integration events that need to be recorded in the outbox.

The implementation must define when events are collected and removed from the pending-event collection to avoid unintended repeated processing.

It must also define how nested or newly raised domain events are handled. If handlers can raise additional domain events, the processing strategy must either process them deliberately or reject unsupported behavior rather than silently losing them.

Domain-event handlers must not independently commit the business transaction or bypass the Unit of Work's transaction coordination.

### 3. Phase Two: Persisting Event-Related Changes and Outbox Records

The second phase persists changes introduced during domain-event processing.

It also persists the outbox records corresponding to integration events produced by the operation.

The relevant records must participate in the same database transaction as the original business changes.

This is the key relationship between the two-phase save workflow and the transactional outbox:

**The first save does not commit the business operation; the final transaction commit makes both the business changes and the outbox records durable together.**

If the second persistence phase fails, the transaction must be rolled back so that the initial business changes do not remain committed without the required event-related changes.

### 4. Final Commit

After both persistence phases succeed, the Unit of Work commits the active transaction.

A successful commit makes the business changes and their corresponding outbox records durable.

The outbox dispatcher can then process the persisted integration events asynchronously.

The Unit of Work must not treat a successful intermediate save as proof that the entire business operation has committed.

If commit outcome is uncertain because the database connection fails during commit, the application must not assume either success or failure without an appropriate recovery strategy.

## Transaction Boundaries

The two-phase save workflow operates within a single logical database transaction.

| Operation                    | Transaction behavior                              |
| ---------------------------- | ------------------------------------------------- |
| Initial business changes     | Persisted within the active transaction           |
| Domain-event processing      | Coordinated before final commit                   |
| Event-related entity changes | Persisted within the same transaction             |
| Outbox record creation       | Persisted within the same transaction             |
| Final commit                 | Makes the transaction's changes durable           |
| Outbox dispatch              | Occurs after commit through background processing |

The intermediate saves must not independently commit the transaction.

The guarantee depends on the implementation using the same transaction for both persistence phases. Separate `DbContext` instances or independent transactions must not be introduced into this workflow unless they are explicitly coordinated to preserve the required consistency guarantees.

## Failure Handling

### Initial Persistence Failure

If the initial save fails, the Unit of Work must not proceed to final commit.

The transaction must be rolled back or disposed in a way that ensures it cannot commit partially prepared business changes.

### Domain-Event Processing Failure

If domain-event processing fails, the Unit of Work must not commit the business operation.

The transaction must be rolled back, including any changes persisted during the initial phase.

The implementation must also define how pending domain events are handled after failure. Clearing events before successful completion can cause them to be lost from the in-memory processing workflow, while retaining them on tracked entities may cause unintended reprocessing if the same context is reused.

A failed Unit of Work should not be reused for another business operation without a deliberate recovery or cleanup strategy.

### Second Persistence Phase Failure

If persistence of event-related changes or outbox records fails, the transaction must be rolled back.

The initial business changes must not remain committed independently of the outbox records required by the operation.

### Commit Failure

A failure during transaction commit may leave the caller uncertain whether the database committed the transaction.

The application must not blindly replay a non-idempotent business operation solely because the commit call returned an exception. Recovery may require reloading authoritative state or using an appropriate operation-level idempotency mechanism.

## Relationship to the Transactional Outbox

The two-phase save workflow and transactional outbox solve related but distinct problems.

* **Two-phase save** defines how the Unit of Work coordinates persistence and domain-event processing before commit.
* **Transactional outbox** ensures that integration-event publication intent is persisted atomically with the business changes.
* **Background scheduling** executes the process that dispatches persisted outbox records.
* **Per-handler idempotency** protects consumers against repeated delivery or processing.

The two-phase save workflow does not guarantee successful downstream event processing. It guarantees that the original transaction can persist the business changes and corresponding outbox records together.

Likewise, the outbox does not require every business operation to use exactly two calls to `SaveChangesAsync`. The essential requirement is the transaction and event-processing guarantees, not the number of EF Core save calls.

## Rationale

This decision makes the Unit of Work responsible for coordinating a multi-step persistence workflow that must remain atomic.

It provides the following benefits:

* **Transactional consistency:** Initial changes, event-related changes, and outbox records commit together.
* **Controlled event processing:** Domain-event handling occurs at a defined point in the persistence lifecycle.
* **Outbox reliability:** Integration-event records are not committed independently of the business changes they represent.
* **Centralized coordination:** Application handlers do not need to reproduce the persistence sequence themselves.
* **Failure containment:** Failures before final commit can roll back the entire database transaction.
* **Separation of concerns:** Domain events express business occurrences, while the Unit of Work coordinates persistence and the outbox supports reliable delivery.

The approach is appropriate when domain-event processing can introduce additional tracked changes that must participate in the original transaction.

## Consequences

### Positive Consequences

* Business changes and outbox records share a clear transaction boundary.
* Persistence sequencing is centralized instead of duplicated across handlers.
* Event-related persistence failures can prevent the business operation from committing.
* Domain-event processing has a defined place in the Unit of Work lifecycle.
* The outbox and background-processing mechanisms remain separate from the original business transaction.

### Trade-offs and Constraints

* The Unit of Work implementation is more complex than a single-save workflow.
* Event-processing order and nested-event behavior must be explicitly defined.
* Intermediate saves can execute database constraints and persistence interceptors before final commit, even though the transaction remains uncommitted.
* EF Core change tracking and pending-event collections must be managed carefully.
* A long-running domain-event handler can extend transaction duration and increase lock contention.
* External side effects must not be executed directly during the transaction if they cannot participate in its atomicity guarantees.
* Database rollback cannot reverse an external operation that has already succeeded.
* Transaction retries must account for operations that may have executed before a transient failure.

## Alternatives Considered

### 1. Single Save Followed by Event Publication

**Description:** Persist business changes in one save operation, commit, and then publish integration events.

**Reason for Rejection:** A failure between commit and publication can leave committed business state without a durable record of the required event. The transactional outbox addresses this gap.

### 2. Publish Domain or Integration Events Before Commit

**Description:** Dispatch events before the business transaction commits.

**Reason for Rejection:** Consumers may react to business changes that subsequently roll back. External side effects also cannot generally be undone by rolling back the database transaction.

### 3. Let Each Handler Manage Its Own Saves

**Description:** Allow command handlers and domain-event handlers to perform persistence operations and commits independently.

**Reason for Rejection:** This distributes transaction coordination across application code, increases the risk of inconsistent behavior, and makes it harder to guarantee that business changes and outbox records share one transaction.

### 4. Use a Distributed Transaction

**Description:** Coordinate business persistence and event publication through a distributed transaction.

**Reason for Rejection:** The current architecture can achieve the required atomic database writes using a shared transaction and outbox. Distributed transaction infrastructure would introduce unnecessary complexity.

## Conditions for Reconsideration

This decision should be revisited if:

* Domain-event processing no longer requires multiple persistence phases.
* The Unit of Work introduces additional database contexts or transaction boundaries.
* Event-processing requirements change substantially.
* Transaction duration or contention becomes a demonstrated performance problem.
* A persistence strategy is introduced that changes the semantics of intermediate saves.
* A future distributed architecture requires different transaction coordination.

Any alternative must preserve the required atomicity between business changes and outbox records and explicitly define domain-event failure and recovery behavior.

## Related Decisions

* [ADR-0002: Clean Architecture Dependency Direction](0002-clean-architecture-dependency-direction.md)
* [ADR-0004: Domain Events vs. Integration Events](0004-domain-events-vs-integration-events.md)
* [ADR-0005: Transactional Outbox](0005-transactional-outbox.md)
* [ADR-0007: Per-Handler Idempotency](0007-per-handler-idempotency.md)

## Summary

The E-Commerce system uses a two-phase save workflow to coordinate initial persistence, domain-event processing, and outbox creation within one database transaction.

**The governing principle is that intermediate persistence does not commit the business operation; business changes and required outbox records become durable together at the final transaction commit.**
