# ADR-0007: Per-Handler Idempotency for Integration Events

* **Status:** Accepted
* **Date:** 2026-10-13
* **Decision Type:** Reliability / Integration Architecture

## Context

Integration events enable business modules to communicate through explicit contracts without directly depending on one another's internal implementation.

In the E-Commerce modular monolith, a publishing module persists integration events to the Outbox within the same database transaction as its business changes. A background job subsequently dispatches those events to their registered integration event handlers.

Event delivery and handler execution can be repeated because of retries, process interruptions, concurrent workers, or failures occurring around transaction commits.

For example, an `OrderReadyForShippingIntegrationEvent` may be dispatched more than once. Its Shipping handler must not create multiple shipments for the same event merely because the dispatcher invokes it repeatedly.

The Outbox guarantees that the intent to publish an integration event is persisted atomically with the originating business changes. It does not guarantee that each handler executes exactly once.

Therefore, integration event handlers require a durable mechanism for recognizing events they have already processed successfully.

## Decision

**Each integration event handler must maintain its own idempotency boundary using a durable processed-event record.**

The system uses a `ProcessedEvents` persistence model to record successful processing by individual integration event handlers.

A processed-event record identifies both:

* The integration event being processed.
* The specific handler responsible for processing it.

The record must be protected by a database uniqueness constraint so concurrent executions cannot independently complete the same handler-event pair.

For database effects owned by the handler, the handler's business changes and its processed-event record must be committed within the same database transaction.

This decision applies to integration event handlers. It does not establish a general requirement for every application handler, domain event handler, or background job.

### 1. Per-Handler Idempotency

Idempotency is scoped to the combination of an event and a handler, rather than to the event alone.

One integration event may have several independent subscribers. Each subscriber must be able to process the event once, regardless of whether another subscriber has already processed it.

For example:

* Finance publishes `PaymentCompletedIntegrationEvent`.
* Ordering handles the event to update the order's payment state.
* Notifications handles the event to initiate a customer notification.

Ordering and Notifications must maintain independent processing records. The successful processing of the event by Ordering must not prevent Notifications from processing it.

The logical idempotency key is:

`(IntegrationEventId, HandlerIdentity)`

`IntegrationEventId` identifies the individual event occurrence, while `HandlerIdentity` identifies the logical subscriber.

The handler identity must remain stable across deployments and retries. Renaming a handler must not inadvertently make previously processed events appear unprocessed.

Event type alone is insufficient because multiple occurrences of the same event type must be processed independently. Event ID alone is also insufficient because different handlers must process the same event independently.

### 2. Processed-Event Persistence

The `ProcessedEvents` persistence model records successful processing by integration event handlers.

A record should contain, at minimum:

* **IntegrationEventId** — the unique identifier of the integration event.
* **HandlerIdentity** — the stable identifier of the handler that processed it.
* **ProcessedAtUtc** — the UTC timestamp of successful processing.

Additional fields, such as the event type or correlation identifier, may be included when useful for diagnostics and operational support.

The database must enforce uniqueness on:

`(IntegrationEventId, HandlerIdentity)`

The uniqueness constraint is the final safeguard against concurrent duplicate processing. An application-level check alone is insufficient because two workers may both observe that no processing record exists before either inserts one.

The processed-event record represents successful completion, not merely an attempt to execute the handler.

### 3. Handler Processing Workflow

For an integration event handler whose business effects and processed-event record use the same database transaction, the workflow is:

1. Receive the integration event and identify the handler-event pair.
2. Begin or participate in the handler's database transaction.
3. Determine whether a successful processing record already exists.
4. If the pair has already been processed, skip the business operation.
5. Otherwise, execute the handler's business logic and apply its database changes.
6. Persist the processed-event record.
7. Commit the transaction.

The existing-record check is an optimization that avoids unnecessary work. The database uniqueness constraint remains authoritative when concurrent executions race.

**The business changes and processed-event record must commit atomically.** They must not be independently committed operations.

If the handler's business changes fail, the transaction must roll back both those changes and the processing record. The event can then be retried according to the established dispatch and retry policies.

If a commit succeeds but the worker loses confirmation, a subsequent invocation can discover the committed processing record and avoid applying the same database effects again.

The implementation must ensure that the check, business changes, and record insertion participate in the intended transaction. If multiple workers may process the same event concurrently, the transaction and uniqueness-conflict handling must be designed to preserve correctness under that race.

### 4. Duplicate Detection and Concurrency

When concurrent executions attempt to process the same handler-event pair, the unique constraint ensures that no more than one corresponding processed-event record can be committed.

The implementation must handle this race deliberately.

If an insertion fails because the expected processed-event uniqueness constraint was violated, the handler may treat the invocation as a duplicate after confirming that the corresponding processing record exists.

It must not treat every database uniqueness violation as successful duplicate detection. A different constraint violation may indicate a genuine data-integrity defect and must be handled accordingly.

If a concurrent execution is still in progress, the losing execution must not assume that the other execution succeeded merely because it encountered a conflict. It must allow the relevant transaction to resolve and verify the committed processing record before considering the event successfully handled.

Other database failures, transaction failures, and unexpected exceptions must follow the normal error-handling and retry policies.

### 5. Relationship to the Transactional Outbox

The Outbox and `ProcessedEvents` address different reliability concerns.

| Mechanism                            | Responsibility                                                                                         |
| ------------------------------------ | ------------------------------------------------------------------------------------------------------ |
| Transactional Outbox                 | Atomically persists the publisher's business changes and intent to publish an integration event.       |
| Integration event dispatcher         | Delivers persisted integration events to their registered handlers.                                    |
| Per-handler `ProcessedEvents` record | Prevents a handler from repeating database effects for an event it has already processed successfully. |
| Database transaction                 | Atomically commits a handler's database changes and its processing record.                             |

The Outbox belongs to the publishing side. Processed-event records belong to the consuming side.

The dispatcher may invoke a handler more than once. A successful handler invocation does not require the dispatcher to guarantee that delivery will never be repeated.

The system therefore relies on durable event publication and independently idempotent consumption rather than assuming exactly-once delivery.

### 6. External Side Effects

A database transaction cannot atomically include an arbitrary external operation, such as sending an email, calling a payment provider, or delivering a push notification.

Consequently, a processed-event record alone cannot guarantee that an external side effect occurs exactly once.

For example, if a handler sends an email and then fails before committing its processed-event record, a retry may send the email again.

External effects must use an appropriate reliability mechanism, such as:

* Persisting a notification or delivery request in the same database transaction and processing it asynchronously.
* Using an Outbox for subsequent external delivery.
* Passing a stable idempotency key to an external provider that supports idempotent requests.
* Applying reconciliation or compensation where the external system does not provide sufficient idempotency guarantees.

The handler must not mark an event as successfully processed before required work has either completed or been durably recorded for reliable continuation.

The exact strategy depends on the external operation and its failure semantics.

## Consequences

### Positive Consequences

* Repeated integration event delivery does not repeat successfully committed database effects for the same handler-event pair.
* Different handlers can process the same integration event independently.
* Concurrent processing is protected by a database-enforced uniqueness constraint.
* Failed transactions can be retried without leaving a processing record that incorrectly indicates success.
* The design works within the existing modular monolith and shared database.
* The system does not require a message broker or a full generic Inbox implementation solely to establish handler idempotency.

### Trade-Offs

* Each handler must participate in the established transaction and processed-event workflow.
* The `ProcessedEvents` table grows over time and requires an explicit retention policy.
* Unique-constraint conflicts and concurrent execution must be handled carefully.
* Changes to handler identity or event identity can undermine deduplication if compatibility is not maintained.
* External side effects require additional reliability measures beyond the processed-event record.

### Operational Considerations

Processed-event records must not be deleted while their removal could allow an event to be replayed and its effects applied again.

Any retention or cleanup policy must account for the maximum supported replay window, operational recovery procedures, and the consequences of reprocessing historical events.

For critical handlers, monitoring should distinguish between successful processing, recognized duplicates, transient failures, and persistent failures.

## Alternatives Considered

### 1. Rely on the Outbox Alone

The Outbox provides durable publication intent but does not prevent a handler from executing multiple times.

**Rejected:** It does not address duplicate effects on the consuming side.

### 2. Use an In-Memory Set of Processed Event IDs

An in-memory collection can suppress duplicates within a running process but does not survive restarts and cannot reliably coordinate multiple workers.

**Rejected:** It is not a durable or concurrency-safe idempotency mechanism.

### 3. Use One Global Processed-Event Record per Event

A single record for an event would make it difficult to represent independent processing by multiple handlers. One handler could incorrectly suppress another handler's execution.

**Rejected:** It does not preserve independent subscriber responsibilities.

### 4. Implement a Full Generic Inbox

A full Inbox architecture can provide additional message-receipt, leasing, retry, and processing-state capabilities.

**Not adopted as the default:** The current modular monolith uses a focused `ProcessedEvents` mechanism for durable per-handler idempotency. A more comprehensive Inbox may be introduced if concrete operational or messaging requirements justify it.

### 5. Assume Exactly-Once Delivery

A dispatcher cannot generally guarantee that a handler is invoked exactly once across process failures and uncertain transaction outcomes.

**Rejected:** The design instead tolerates repeated delivery and protects database effects through transactional idempotency.

## Reconsideration Criteria

Revisit this decision if:

* Integration events move to an external broker with materially different delivery and acknowledgment semantics.
* Handler processing requires durable leases, independently managed receipt states, or more advanced recovery workflows.
* Event replay requirements change the necessary retention period or deduplication strategy.
* Handlers routinely perform external effects that cannot be made idempotent using the existing delivery mechanisms.
* Multiple independently owned databases require a different transaction and idempotency design.

## Related Decisions

* [ADR-0001: Modular Monolith](0001-modular-monolith.md)
* [ADR-0004: Domain Events vs. Integration Events](0004-domain-events-vs-integration-events.md)
* [ADR-0005: Transactional Outbox](0005-transactional-outbox.md)
* [ADR-0006: Two-Phase Save in Unit of Work](0006-two-phase-save-in-unit-of-work.md)
* [ADR-0009: Scheduling Abstraction over Hangfire](0009-scheduling-abstraction-over-hangfire.md)
* [ADR-0012: Automatic Handler and Trigger Discovery](0012-automatic-handler-and-trigger-discovery.md)
* [ADR-0014: Single Deployable Unit](0014-single-deployable-unit.md)

## Architectural Principle

**Integration event delivery may be repeated; each handler must durably recognize successful processing of its own events.**

The system achieves reliable, repeatable processing by combining transactional publication, per-handler idempotency, database-enforced uniqueness, and explicit handling of external side effects. It does not claim universal exactly-once execution.
