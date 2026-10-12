# Outbox Feature — Technical Requirements Document

**Project:** E-Commerce Modular Monolith
**Scope:** Transactional Outbox pipeline — durability, dispatch, idempotency, retry, and dead lettering
**Version:** 1.0 — Final
**Related Documents:**

* Event Dispatching + Outbox Architecture
* End-to-End Outbox Dispatching + Per-Handler Idempotency
* Dead Letter Queue for the Outbox Pipeline

---

## 1. Introduction

This document defines the technical requirements for the Outbox feature: the durable, transactional mechanism that prevents integration events from being lost when business state changes commit successfully.

The requirements are expressed as behavioral and structural obligations the system must satisfy. Implementation choices—including the storage engine, scheduler, libraries, and class design—are specified in the related technical reference documents. This document establishes **what must be true**, not **how it is achieved**.

The requirements cover the write path (business transaction), the delivery path (background processing), reliability guarantees (idempotency, retry, and dead lettering), and operational constraints.

## 2. Scope

### 2.1 In Scope

* Atomic persistence of integration events alongside business state.
* Conversion of domain events into durable integration events within a single transaction.
* Background dispatch of persisted integration events to registered handlers.
* Per-handler idempotency under retries and concurrent execution.
* Retry behavior for failed message processing with a configurable cap.
* Quarantine of poison messages in a dead-letter store.
* Monitoring of quarantined messages.
* A safe reprocessing path for quarantined messages after the root cause is fixed.
* Correlation ID propagation across the full pipeline.
* Structured logging of state transitions and outcomes.
* Configuration and validation of the pipeline at startup.
* Concurrency and cancellation safety during processing.

### 2.2 Out of Scope

* **Domain event dispatching mechanics:** In-memory dispatch within the transaction is covered by the Domain/Application layer requirements.
* **Individual integration event handler business logic:** Each handler is responsible for its own side-effect semantics.
* **External broker publication:** The Outbox is internal-first. External publication through Kafka, RabbitMQ, or similar infrastructure is a future extension.
* **Cross-server claim mechanisms:** Deferred until multi-instance deployments require them. The reserved `Processing` state exists for this purpose but is not currently activated.
* **Automated dead-letter reprocessing tooling:** Manual operation is the current contract.
* **Alerting integrations:** Email, Slack, PagerDuty, and similar integrations are outside the current scope. Logging is the current output; alert routing is external.
* **Retention and archival policies:** Retention of processed Outbox rows, processed-event markers, and dead-letter rows is deferred.
* **Event schema versioning:** Deferred. The current compatibility contract is to add optional fields only and never rename or remove existing fields.

---

## 3. Business Responsibility

The Outbox feature is responsible for the following guarantee:

**Once a business transaction commits, every integration event it produced is durably recorded and will be retried until its registered handlers succeed or the message is quarantined, subject to the system's operational availability and configured retry policy.**

For a handler that has already completed successfully and has a committed idempotency marker, subsequent sequential deliveries of the same event will skip that handler.

This does **not** guarantee end-to-end exactly-once execution across independent systems. A crash after an external side effect succeeds but before its idempotency marker commits can cause that side effect to be repeated unless the handler or external provider supports its own idempotency mechanism.

The Outbox answers the question:

*How does a business fact, once committed, propagate durably and reliably to other parts of the system?*

It does **not** own:

* The decision about what constitutes a business fact; that belongs to the Domain.
* The mapping between a domain event and its integration events; that belongs to the Application layer's domain event handlers.
* The side effects themselves; those belong to integration event handlers.
* The external systems those handlers call.

---

## 4. Functional Requirements

Requirements are numbered for traceability. Each is classified as:

* **Must:** Mandatory for correctness.
* **Should:** Strongly recommended.
* **May:** Optional.

### 4.1 Write Path — Atomicity and Durability

**FR-1 (Must)** — Business state changes and integration event persistence **must be atomic**. If the transaction commits, both must commit; if it rolls back, both must roll back.

**FR-2 (Must)** — The write path **must not** perform synchronous external calls as side effects of integration events, including email delivery, HTTP requests, or external queue publication. Such work must be deferred to background processing.

**FR-3 (Must)** — Domain events must be dispatched to their handlers inside the same transaction as the business state change, before the transaction commits.

**FR-4 (Must)** — Domain events must not be persisted to the Outbox or another durable store as domain events. They exist as in-memory facts and are discarded after dispatch. If a domain event is translated into an integration event, that integration event is persisted separately.

**FR-5 (Must)** — Only integration events are persisted to the Outbox. The Outbox table must not contain rows representing domain events.

**FR-6 (Must)** — Once the transaction commits, every integration event written during that transaction must survive process crashes, restarts, and transient downstream failures.

**FR-7 (Must)** — Integration events must not be stored in an in-memory queue, in-process buffer, or other volatile medium as their primary durable representation.

**FR-8 (Should)** — A domain event handler may produce zero, one, or multiple integration events. The mapping is not required to be one-to-one.

**FR-9 (Must)** — A failure to persist an integration event during the business transaction must cause the entire transaction to roll back. Partial persistence is not permitted.

**FR-10 (Must)** — An integration event must record, at minimum:

* A unique identifier (`EventId`).
* The event type.
* A serialized payload.
* The occurrence timestamp (`OccurredAt`).
* A correlation identifier when available.

### 4.2 Delivery Path — Dispatch and Processing

**FR-11 (Must)** — Persisted integration events must be delivered by a background processing mechanism that operates independently of the original HTTP request or command handler.

**FR-12 (Must)** — Processing must be idempotent with respect to the same persisted event. Reprocessing due to retries must not repeat the work of a handler whose successful processing has already been recorded. This guarantee applies to the pipeline's committed per-handler markers, not to uncoordinated external side effects.

**FR-13 (Must)** — The processor must fetch a batch of pending messages, process them in a defined order (chronologically by occurrence time), and not depend on in-memory state from a previous processing cycle.

**FR-14 (Must)** — The processor must resolve each persisted event's CLR type from its stored representation and deserialize the payload into an integration event instance.

**FR-15 (Must)** — The processor must invoke all registered handlers for a given event type. If no handlers are registered, the message must still be marked as processed; an event with no subscribers is not inherently an error.

**FR-16 (Must)** — Handlers for a given event must be invoked in a deterministic order within a single processing cycle. That order must not be treated as a business-level ordering guarantee between subscribers unless explicitly defined by the architecture.

**FR-17 (Must)** — A failure in one handler must stop processing the remaining handlers for that message in the current cycle. The message must remain eligible for retry, subject to the configured retry policy.

**FR-18 (Must)** — A failure in one message must not prevent other messages in the batch from being processed.

**FR-19 (Must)** — After all registered handlers complete successfully, the message must be marked as `Processed` and its `ProcessedAt` timestamp recorded. The absence of registered handlers also counts as successful dispatch.

**FR-20 (Must)** — The processor must be safe against overlapping executions within the same application instance.

**FR-21 (Should)** — The processor should support cancellation. When cancellation is requested, in-flight messages must remain recoverable and eligible for retry. Cancellation must not be recorded as an ordinary handler failure. A side effect that completed before cancellation may still have occurred, so retry safety must be preserved.

**FR-22 (Should)** — The processor should log a summary at the end of each cycle, reporting the number of messages processed, failed, and dead-lettered.

### 4.3 Idempotency — Per-Handler Guarantees

**FR-23 (Must)** — Idempotency must be tracked per handler, not merely per event. Different handlers for the same event must have independent progress records.

**FR-24 (Must)** — The idempotency record must be keyed by `(EventId, HandlerIdentifier)` and enforced by a database uniqueness constraint.

**FR-25 (Must)** — Before a handler executes, the pipeline must check whether that handler has already successfully processed the event. If a committed success marker exists, the handler must be skipped.

**FR-26 (Must)** — After a handler succeeds, the pipeline must record its success so future deliveries can skip it.

**FR-27 (Must)** — A failure to insert an idempotency marker must not be treated as a processing failure if the marker already exists and represents the same handler's successful processing. Concurrent-insert races must be handled safely without masking unrelated database errors.

**FR-28 (Must)** — Idempotency must be applied transparently by the pipeline, without requiring handler code to implement the pipeline's marker checks.

**FR-29 (Should)** — The handler identifier used for idempotency should remain stable across deployments. Renaming or moving a handler class, if its identity is derived from its type name, requires coordination or migration so previously recorded markers remain effective.

**FR-30 (Must)** — The system must not claim transactional idempotency across independent external systems. Handlers whose side effects are critical must implement appropriate external idempotency or deduplication. The Outbox provides **sequential per-handler deduplication**; it does not atomically commit an external side effect and its database marker.

### 4.4 Retry and Poison Message Handling

**FR-31 (Must)** — Failed messages must be retried on subsequent processing cycles until they succeed or reach the configured retry cap.

**FR-32 (Must)** — Each failed processing attempt counted as a retry must update a persistent retry counter associated with the message. The precise convention for whether the initial attempt counts toward the cap must be documented and applied consistently.

**FR-33 (Must)** — The maximum retry count must be configurable and validated at application startup.

**FR-34 (Must)** — A message whose retry count reaches or exceeds the configured maximum must be moved to the dead-letter store.

**FR-35 (Must)** — A dead-lettered message must not be processed again by the normal processing cycle.

**FR-36 (Must)** — The dead-letter transition must be atomic: insertion of the dead-letter record and the original message's status change must commit as a single transaction, or neither change must commit.

**FR-37 (Must)** — The dead-letter record must preserve, at minimum:

* A unique identifier.
* A reference to the original message.
* The event type.
* The serialized payload.
* The last error message.
* The retry count at the time of quarantine.
* The quarantine timestamp.

**FR-38 (Must)** — A failure to insert the dead-letter record must not cause the message to be lost. The message must remain recoverable in its prior state and be eligible for another processing cycle.

**FR-39 (Should)** — Moving a message to the dead-letter store should emit a warning-level log entry including the message identifier and retry count.

### 4.5 Configuration and Validation

**FR-40 (Must)** — All Outbox configuration must be supplied through the application's standard configuration mechanism, such as `appsettings.json`, environment variables, or another supported configuration provider, and bound to a strongly typed options class.

**FR-41 (Must)** — Configuration must be validated at application startup. Invalid configuration must prevent the host from starting.

**FR-42 (Must)** — The retry cap must be a positive integer within a bounded range. Zero, negative values, and values above the permitted upper bound must be rejected at startup.

**FR-43 (Should)** — The batch size for Outbox processing should be configurable per environment.

**FR-44 (May)** — A feature flag to pause Outbox processing may be supported, allowing operators to halt delivery during a known incident without redeploying.

### 4.6 Observability

**FR-45 (Must)** — Every significant Outbox state transition and processing outcome must emit a structured log entry. These include:

* Message persisted.
* Message processing started for a batch.
* Handler invocation.
* Message marked as processed.
* Message marked as failed.
* Message moved to the dead-letter store.
* Dead-letter record inserted.

**FR-46 (Must)** — A correlation identifier present on an integration event must be restored into the logging scope for each handler that processes the event, so downstream logs can carry the same correlation ID as the originating request.

**FR-47 (Must)** — Failure log entries must include the message identifier and exception details. Success log entries must include the message identifier and, where applicable, the handler identifier.

**FR-48 (Must)** — The correlation identifier stored with an integration event must be preserved across the Outbox boundary: from the originating HTTP request, through persistence, and into asynchronous handler execution.

**FR-49 (Should)** — A recurring monitor should log a warning whenever the dead-letter store contains entries awaiting operational attention.

**FR-50 (May)** — Metrics for backlog depth, age of the oldest pending message, and dead-letter count may be emitted to the application's telemetry pipeline.

### 4.7 Reprocessing

**FR-51 (Must)** — It must be possible to reprocess a dead-lettered message after the underlying failure has been addressed.

**FR-52 (Must)** — Reprocessing must preserve per-handler idempotency. Any handler whose success marker remains recorded for the original event must be skipped, preventing repetition of already-recorded successful handler processing.

**FR-53 (Should)** — The reprocessing mechanism should be documented, whether manual or automated.

**FR-54 (May)** — An administrative endpoint or scheduled job to requeue a dead-lettered message may be provided.

The reprocessing operation must preserve the original event identity and existing per-handler markers when resuming the original event. If a genuinely new logical event is required, it must be created as a new event rather than disguising it as a reprocessing attempt.

---

## 5. Non-Functional Requirements

### 5.1 Reliability

**NFR-1** — No integration event persisted to the Outbox must be silently lost. It must ultimately be marked `Processed` or moved to the dead-letter store, assuming the database and processing infrastructure eventually recover and operational intervention occurs when required.

**NFR-2** — The pipeline must tolerate process crashes at any point:

* **After business state commit but before Outbox persistence:** This state must be impossible because both writes are atomic.
* **After the Outbox write but before background processing:** Messages remain pending and can be picked up after restart.
* **During background processing but before a status update:** Messages remain recoverable and can be retried.
* **After a handler's external side effect succeeds but before its marker or message status commits:** Duplicate execution is possible. The marker prevents repeating a handler only after its successful marker has committed; external idempotency may be required to prevent duplicate external effects.

**NFR-3** — A poison message must not prevent other messages in the batch from being processed.

**NFR-4** — A transient persistence-layer failure during background processing must result in recoverable work being retried rather than silently dropped.

### 5.2 Correctness

**NFR-5** — Atomicity between business state and integration event persistence must be enforced by the underlying database transaction. The system must not rely on application-level coordination to simulate atomicity.

**NFR-6** — Per-handler idempotency must be enforced at the database level through a uniqueness constraint on `(EventId, HandlerIdentifier)`. Application-level checks alone are insufficient.

**NFR-7** — Retry counting must be monotonic: a message's retry count must never decrease except during an explicitly defined reprocessing operation.

### 5.3 Scalability

**NFR-8** — The pipeline must operate correctly with a single application instance without additional coordination.

**NFR-9** — The pipeline must support horizontal scaling when an appropriate message-claiming or concurrency-control mechanism is introduced. Until then, multiple instances may execute duplicate work. Idempotency helps prevent repeated execution after a committed handler marker exists, but it does not prevent simultaneous execution of the same handler or duplicate external side effects.

**NFR-10** — Batch sizes and scheduling frequency must be tunable without code changes.

### 5.4 Performance

**NFR-11** — Persisting an integration event during the business transaction must not add disproportionate latency. Serialization and persistence costs should scale primarily with payload size and must not scale with the number of subscribers.

**NFR-12** — Fetching pending messages must be supported by an index on `(Status, OccurredAt)`. Full table scans are not acceptable at scale.

**NFR-13** — The dead-letter monitor query must be supported by an index on `(Status, DeadLetteredAt)`.

### 5.5 Maintainability

**NFR-14** — Outbox persistence entities, repositories, processing components, and related infrastructure implementations must reside in the Infrastructure layer. Application code may depend on sanctioned abstractions such as `IOutboxMessageWriter`, but Application and Domain code must not reference Outbox tables, persistence repositories, or infrastructure entities directly.

**NFR-15** — Adding a new integration event must not require changes to the Outbox pipeline. It should require only the new event contract and, where applicable, domain-event translation and subscriber handlers.

**NFR-16** — Adding a new integration event handler must not require changes to the Outbox pipeline. Handlers must be automatically discovered and decorated according to the established registration conventions.

### 5.6 Testability

**NFR-17** — The pipeline must be testable at the following levels:

* **Unit:** Dispatcher, serializer, idempotency decorator, and retry logic.
* **Integration:** Persistence, transaction atomicity, uniqueness constraints, and indexes.
* **End-to-end:** Message written → message processed → handler invoked → marker recorded.

**NFR-18** — The pipeline must not require a running HTTP server or UI to exercise.

### 5.7 Security

**NFR-19** — Sensitive data must not appear in log entries. Payloads may be logged in aggregate—for example, counts and event types—but must not be logged in full.

**NFR-20** — Access to dead-letter inspection and reprocessing operations must be restricted to authorized operations or administrative roles. Normal business processing must not provide an unrestricted path to modify or delete dead-letter records.

**NFR-21** — The correlation identifier must be treated as untrusted input. Its length and character set must be validated before it is used in logs or persistence.

---

## 6. Domain Rules and Invariants

**DR-1** — An integration event committed to the Outbox is considered durable. It remains eligible for delivery until it is processed successfully or quarantined.

**DR-2** — A domain event that is not translated into an integration event before the business transaction commits is lost by design. Domain events are transient facts; only their explicit translation into durable integration events survives the transaction.

**DR-3** — A handler with a committed success marker for an integration event is considered complete for that event. Subsequent sequential deliveries of the same event ID must skip that handler.

**DR-4** — A message in the dead-letter store is quarantined, not actively retrying. It is excluded from normal processing until an operator or an explicitly supported reprocessing operation intervenes.

**DR-5** — The Outbox never holds domain events. A row representing anything other than an integration event violates the Outbox contract.

---

## 7. Constraints

**C-1** — The Outbox is a component of a modular monolith. It shares a database with the business contexts and does not attempt to span distributed transactions.

**C-2** — The Outbox does not replace synchronous interactions when immediate transactional semantics are required. Such interactions must use an appropriate shared contract and transaction strategy for the modular monolith's shared database. The design must not imply that separate bounded-context abstractions automatically create separate atomic transactions.

**C-3** — The Outbox assumes the underlying database supports ACID transactions. Systems without the required transactional guarantees cannot rely on this design.

**C-4** — The application process, or one of its replicas, that writes an Outbox message is expected eventually to process it. A separate worker process is not required, but the architecture permits one.

**C-5** — Idempotency is provided at the Outbox handler boundary. External effects that are not naturally idempotent—such as SMS delivery or card charging—require the handler to establish appropriate external idempotency or deduplication.

**C-6** — The Outbox is not a general-purpose messaging system. It does not provide end-to-end exactly-once execution, ordering guarantees across aggregates, or fan-out to external consumers. It provides durable internal delivery with per-handler deduplication based on committed markers.

---

## 8. Assumptions

**A-1** — Business operations that produce integration events execute within a database transaction managed by the application's unit of work.

**A-2** — Domain event handlers execute synchronously and in-process, within the transaction and before commit.

**A-3** — A scheduler, such as Hangfire, is available to drive background processing on a recurring basis.

**A-4** — Handlers are registered in the dependency injection container and resolved through that container at dispatch time.

**A-5** — Integration event payloads can be serialized to and deserialized from a supported format, such as JSON, without losing semantic content.

**A-6** — Value objects and domain-agnostic primitives used in integration event payloads are round-trippable by the configured serializer. Types that are not must provide a compatible constructor or custom converter.

**A-7** — Handlers do not carry mutable state between invocations, or any mutable state is scoped to a single invocation.

---

## 9. Acceptance Criteria

The Outbox feature is considered complete when the following criteria are satisfied:

1. **AC-1** — Business state and integration events commit atomically. Rolling back the transaction rolls back both.
2. **AC-2** — A domain event that fails to translate into an integration event causes the business transaction to fail.
3. **AC-3** — Persisted integration events survive application restarts and are picked up in a subsequent processing cycle.
4. **AC-4** — Each registered handler's successful processing is recorded independently. Under retries, handlers with committed success markers are skipped. External side effects are protected by their own idempotency mechanisms where necessary.
5. **AC-5** — A concurrent idempotency-marker insert does not cause a spurious message failure when the same successful marker already exists.
6. **AC-6** — A handler failure leaves the message eligible for retry; a subsequent successful attempt marks the message `Processed` once all registered handlers have succeeded.
7. **AC-7** — A message reaching the retry cap is moved to the dead-letter store atomically, without orphaned dead-letter records or duplicate quarantine records.
8. **AC-8** — Dead-lettered messages are excluded from normal processing and remain available for authorized reprocessing.
9. **AC-9** — Reprocessing a dead-lettered message skips handlers that already have successful markers for the original event.
10. **AC-10** — Invalid configuration, including an out-of-range `MaxRetryCount`, prevents the host from starting.
11. **AC-11** — Every significant message state transition and processing outcome emits a structured log entry.
12. **AC-12** — The originating request's correlation ID is present in logs emitted within the restored correlation scope for handlers processing the corresponding integration event.
13. **AC-13** — The pipeline operates correctly with a single application instance and a single recurring processing job.
14. **AC-14** — Cancellation of the background job does not automatically classify an in-flight message as a handler failure; the message remains recoverable.
15. **AC-15** — Fetching pending messages uses the appropriate index rather than requiring a full Outbox table scan at scale.
16. **AC-16** — Adding an integration event and its handlers does not require changes to the Outbox pipeline itself.

---

## 10. Traceability Matrix

| Requirement Area      | Description                                                                  | Related Document                                              |
| --------------------- | ---------------------------------------------------------------------------- | ------------------------------------------------------------- |
| Write-path atomicity  | Business state and integration events commit in one transaction.             | Event Dispatching + Outbox Architecture §5                    |
| Event separation      | Domain events are transient; integration events are durable.                 | Event Dispatching + Outbox Architecture §7                    |
| Type resolution       | Assembly-qualified type storage and fallback resolution.                     | Event Dispatching + Outbox Architecture §5.1                  |
| Idempotency           | Per-handler marker with a composite key.                                     | End-to-End Outbox Dispatching + Per-Handler Idempotency §§2–5 |
| Race handling         | Concurrent marker insertion is handled safely.                               | End-to-End Outbox Dispatching + Per-Handler Idempotency §4    |
| Retry                 | Failed messages are counted and retried.                                     | Dead Letter Queue for the Outbox Pipeline §6                  |
| Dead-letter atomicity | Dead-letter insertion and message status update occur in one transaction.    | Dead Letter Queue for the Outbox Pipeline §6.3                |
| Poison-message cap    | `MaxRetryCount` is enforced during processing.                               | Dead Letter Queue for the Outbox Pipeline §§2, 6              |
| Monitoring            | A recurring scan checks the dead-letter store.                               | Dead Letter Queue for the Outbox Pipeline §5.7                |
| Reprocessing          | Manual requeue preserves existing per-handler markers.                       | Dead Letter Queue for the Outbox Pipeline §10                 |
| Concurrency           | The processing job prevents overlapping execution within the intended scope. | Event Dispatching + Outbox Architecture §5.2                  |
| Cancellation          | The job respects the shutdown token.                                         | Event Dispatching + Outbox Architecture §5.2                  |
| Configuration         | Configuration is validated at startup.                                       | Dead Letter Queue for the Outbox Pipeline §5.5                |
| Indexes               | `(Status, OccurredAt)` and `(Status, DeadLetteredAt)`.                       | Dead Letter Queue for the Outbox Pipeline §8                  |
| Logging               | Structured entries capture processing transitions and outcomes.              | Event Dispatching + Outbox Architecture §5                    |
| Correlation           | Correlation IDs are restored into handler logging scopes.                    | Event Dispatching + Outbox Architecture §4                    |

---

## 11. Summary

The Outbox feature provides durable internal integration-event delivery with per-handler deduplication and bounded retry.

Its core requirements are:

1. **Atomic persistence** — Business state and integration events commit in the same database transaction.
2. **Durable storage** — Persisted events survive crashes and remain eligible for background processing.
3. **Per-handler idempotency** — Handlers are tracked independently using a database-enforced marker keyed by `(EventId, HandlerIdentifier)`.
4. **Bounded retry and quarantine** — Failed messages are retried up to a configured cap and then atomically moved to the dead-letter store.
5. **Observability and reprocessing** — State transitions are logged, correlation is preserved, and quarantined messages can be requeued safely.

The feature's persistence and processing implementation belongs in Infrastructure. Application code may use sanctioned abstractions, such as `IOutboxMessageWriter`, to write integration events without depending on Outbox persistence details. Domain code remains independent of the Outbox implementation.

Adding a new integration event or handler should require no changes to the Outbox pipeline itself. Automatic discovery and decoration are governed by the application's established registration conventions.

---

**Document Version:** 1.0 — Final
**Document Type:** Outbox Technical Requirements

This document establishes the behavioral and structural obligations for transactional atomicity, durability, dispatch, per-handler idempotency, bounded retry, poison-message quarantine, configuration validation, correlation propagation, structured logging, and safe reprocessing. Implementation details are deferred to the related technical reference documents.
