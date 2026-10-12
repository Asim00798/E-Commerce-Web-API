# ADR-0005: Transactional Outbox

* **Status:** Accepted
* **Decision Type:** Architecture
* **Date:** 2026-10-13

## Context

The E-Commerce modular monolith uses integration events to communicate business facts between modules.

When a business operation changes persisted state and requires another module to react, the system must preserve the relationship between the committed business change and the intention to publish the corresponding integration event.

Publishing an event directly before committing the database transaction can expose a business change that is subsequently rolled back. Publishing directly after commit creates a different failure window: the database transaction may succeed, but the application may fail before the event is recorded or delivered.

Neither approach reliably coordinates database persistence with asynchronous event delivery.

A distributed message broker could participate in a broader messaging architecture, but introducing one solely to coordinate communication inside a single deployable application would add unnecessary operational complexity.

The system therefore needs a mechanism that persists business changes and the corresponding integration-event publication intent atomically, while allowing event delivery to occur asynchronously.

## Decision

The system adopts the **Transactional Outbox Pattern** for integration events that require reliable publication alongside business-state changes.

Business changes and their corresponding outbox records are persisted in the same database transaction.

A background-processing mechanism dispatches pending outbox records after the transaction commits.

The implementation uses the existing database, Unit of Work, outbox infrastructure, and centralized Scheduling capability. Hangfire provides background execution, while the outbox remains the durable record of pending publication work.

The pattern separates three responsibilities:

1. **Business transaction:** Persists authoritative business changes and outbox records atomically.
2. **Outbox processing:** Finds pending records and coordinates dispatch.
3. **Consumer processing:** Handles integration events according to the receiving module's responsibilities and idempotency requirements.

### 1. Atomic Persistence

When a use case produces an integration event, the event must be converted into an outbox record before the relevant database transaction commits.

The transaction includes:

* The business-state changes required by the operation.
* The outbox records representing the integration events to be published.

If the transaction rolls back, neither the corresponding business changes nor the associated outbox records should remain committed.

If the transaction commits, both the business changes and publication intent are durable.

This guarantee depends on the business changes and outbox records participating in the same database transaction and transactional resource.

### 2. Event Creation and Ownership

The module that owns a business fact owns its integration-event contract.

Application orchestration coordinates the creation of integration events from relevant domain events or business workflows.

The outbox infrastructure is responsible for persisting and dispatching the resulting integration-event records. It does not own the business meaning of those events.

For example, Finance owns the contract for a payment-completed integration event. Ordering owns the handler that reacts to that event and updates its own order state.

The event contract should expose the information required by consumers without exposing internal aggregate instances, repository implementations, or other persistence-specific objects.

### 3. Outbox Persistence

Outbox records represent integration events awaiting or undergoing processing.

Depending on the implementation, a record may contain:

* A unique event or message identifier.
* The integration-event type.
* A serialized event payload.
* Creation and processing timestamps.
* Delivery status.
* Attempt or failure information.
* Correlation or tracing metadata.

The exact schema is an implementation detail and must remain consistent with the actual outbox reference documentation.

The outbox is durable application state, not merely an in-memory queue. Pending records must survive process restarts.

The design should also define retention and cleanup policies for successfully processed records, failed records, and records requiring operational investigation.

### 4. Asynchronous Dispatch

Outbox processing runs outside the original business transaction.

The intended lifecycle is:

1. A use case performs its business operation.
2. Domain events are collected and processed according to the Unit of Work workflow.
3. Integration-event records are added to the outbox.
4. The business changes and outbox records commit atomically.
5. A background job retrieves pending outbox records.
6. The dispatcher routes each event to its appropriate handler or configured transport.
7. Successful processing is recorded according to the dispatch and consumer-processing design.
8. Failed processing is retried or retained for further investigation according to the established failure policy.

The background job must not be responsible for reconstructing business decisions that should have been made in the original transaction. Its responsibility is to process already-recorded integration-event work.

### 5. Scheduling Through the Centralized Abstraction

The Outbox capability uses the centralized Scheduling abstraction to execute background processing.

Application code must not depend directly on Hangfire APIs.

The Scheduling capability owns the Hangfire integration and provides the established job-execution pipeline, while the Outbox capability owns the outbox processing behavior.

This separation allows background execution infrastructure to evolve without moving business or outbox responsibilities into the scheduler.

The outbox must remain correct even if a scheduled execution is delayed or a background job fails. A scheduling trigger is not the durable source of truth for pending integration events; the database records are.

### 6. Delivery Guarantees

The transactional outbox guarantees atomic persistence of business changes and publication intent. It does not, by itself, guarantee exactly-once processing.

A dispatcher may successfully invoke a consumer and then fail before recording the successful outcome. The same event may consequently be delivered again.

The system therefore treats delivery as potentially repeated and requires appropriate idempotency protections.

Where multiple consumers handle an event, the processing state and idempotency key must be scoped appropriately to the consumer or handler. One consumer's successful processing must not incorrectly suppress another consumer's work.

A successful dispatch attempt must be distinguished from successful completion of every downstream business process. Those are separate outcomes with potentially separate transaction boundaries.

### 7. Failure Handling

The outbox processing mechanism must account for transient failures and failures requiring investigation.

Examples include:

* Database connectivity failures.
* Temporary failures during handler execution.
* Process termination after dispatch but before success is recorded.
* Malformed or unsupported event payloads.
* Repeated failures that prevent an event from completing.
* A growing backlog of pending events.

Retry policy must be coordinated with the centralized Scheduling mechanism to avoid unintended nested retries.

The system should provide operational visibility into pending work, processing failures, retry behavior, and events requiring intervention.

If dead-letter or terminal-failure handling is implemented, its lifecycle must be documented separately. Moving an event to a terminal state must not silently discard business-critical work.

### 8. Concurrency and Multiple Workers

If multiple workers can process outbox records concurrently, the implementation must prevent conflicting processing from corrupting outbox state.

Possible mechanisms include atomic claims, conditional status updates, row-level locking, or other database-supported concurrency controls.

The specific mechanism must match the implemented persistence design.

A simple query that selects pending records does not, by itself, prevent two workers from selecting the same records.

Even with atomic claiming, duplicate delivery remains possible if a worker completes a consumer operation but fails before acknowledging the event. Consumer idempotency remains necessary.

### 9. No Mandatory Message Broker

The outbox pattern does not require Kafka, RabbitMQ, or another external message broker.

Within the current modular monolith, the dispatcher may deliver events through the application's in-process integration mechanism.

If the system later needs external message transport, the dispatcher can be extended to publish through an appropriate transport while preserving the transactional outbox as the durable record of publication intent.

Such a change would require explicit decisions about transport delivery guarantees, consumer acknowledgements, ordering, security, and operational recovery.

## Rationale

The transactional outbox addresses the gap between database commit and reliable asynchronous event publication.

It provides the following benefits:

* **Atomicity:** Business changes and publication intent are committed together.
* **Durability:** Pending integration events survive application restarts.
* **Failure resilience:** Temporary delivery failures do not require repeating the original business transaction.
* **Separation of concerns:** Business workflows, durable event storage, and background execution remain distinct responsibilities.
* **Idempotent processing:** Consumers can safely handle repeated deliveries using explicit processing records and database constraints.
* **Operational simplicity:** The current modular monolith can use its existing database and scheduling infrastructure without requiring a distributed broker.
* **Extensibility:** The dispatch mechanism can evolve if future requirements justify external messaging.

The outbox does not eliminate failures. It makes the intention to perform asynchronous work durable and provides a foundation for recovering from failures.

## Consequences

### Positive Consequences

* A committed business change cannot be separated from its corresponding persisted outbox record by a failure between independent database writes.
* Pending events can be retried without repeating the original command.
* Background processing can be monitored independently from request execution.
* Integration-event contracts remain separate from infrastructure delivery mechanisms.
* The application can support reliable cross-module communication within a single deployable process.
* A future transport change does not inherently require changing the original business transaction.

### Trade-offs and Constraints

* Outbox records require database storage and lifecycle management.
* Event dispatch introduces asynchronous latency between the original transaction and downstream processing.
* Duplicate delivery remains possible.
* Outbox processing requires retry policies, concurrency controls, monitoring, and operational recovery procedures.
* A persistent backlog can delay downstream workflows even when the original business transaction succeeded.
* Serialization formats and event contracts require compatibility management.
* The outbox alone does not guarantee event ordering, exactly-once delivery, or atomic completion across modules.
* Database transactions must include the relevant business changes and outbox records for the atomicity guarantee to hold.

## Alternatives Considered

### 1. Direct Publication Before Database Commit

**Description:** Publish the integration event before committing the business transaction.

**Reason for Rejection:** A consumer may react to an event for a business change that subsequently rolls back.

### 2. Direct Publication After Database Commit

**Description:** Commit the business transaction and then publish the integration event directly.

**Reason for Rejection:** The process may fail after the database commit but before the event is durably recorded or delivered. The business change would remain committed without a recoverable publication record.

### 3. In-Memory Queue

**Description:** Store pending integration events only in memory and process them asynchronously.

**Reason for Rejection:** In-memory work can be lost if the process terminates before processing completes. It does not provide the required durable relationship with the business transaction.

### 4. Distributed Message Broker Without an Outbox

**Description:** Publish directly to an external broker as part of the business workflow.

**Reason for Rejection:** A database transaction and a broker publication are separate operations. Without a distributed transaction or another reliable coordination mechanism, the original consistency problem remains.

A broker may be useful in the future, but it does not automatically eliminate the need for an outbox.

### 5. Distributed Transactions

**Description:** Coordinate the database and messaging infrastructure through a distributed transaction.

**Reason for Rejection:** Distributed transaction support would introduce additional operational and infrastructure complexity that is not justified by the current single-deployable modular monolith.

## Conditions for Reconsideration

This decision should be revisited if:

* The application introduces independently deployed services.
* Integration-event volume requires a different dispatch or transport architecture.
* Multiple consumers require independently managed delivery pipelines.
* Event ordering or partitioning requirements become explicit.
* Outbox storage or processing becomes a demonstrated operational bottleneck.
* A new infrastructure capability offers a materially better reliability model.

Any replacement or extension must preserve a reliable relationship between committed business changes and the corresponding publication intent.

## Related Decisions

* [ADR-0001: Modular Monolith Architecture](0001-modular-monolith.md)
* [ADR-0004: Domain Events vs. Integration Events](0004-domain-events-vs-integration-events.md)
* [ADR-0006: Two-Phase Save in Unit of Work](0006-two-phase-save-in-unit-of-work.md)
* [ADR-0007: Per-Handler Idempotency](0007-per-handler-idempotency.md)
* [ADR-0009: Scheduling Abstraction over Hangfire](0009-scheduling-abstraction-over-hangfire.md)
* [ADR-0012: Automatic Handler and Trigger Discovery](0012-automatic-handler-and-trigger-discovery.md)
* [ADR-0013: Business Process vs. Housekeeping Jobs](0013-business-process-vs-housekeeping-jobs.md)

## Summary

The E-Commerce system uses a transactional outbox to persist integration-event publication intent atomically with the corresponding business changes.

Background processing dispatches the durable records independently of the original request, while retry-safe processing and idempotency address failures and duplicate delivery.

**The governing principle is that business changes and publication intent commit together; delivery happens asynchronously and must tolerate retries and duplicates.**
