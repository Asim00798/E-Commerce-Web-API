# Outbox Feature — Workflow Technical Document

**Project:** E-Commerce Modular Monolith
**Scope:** End-to-end workflows for the transactional Outbox pipeline: write path, delivery path, idempotency, retry, dead letter, and reprocessing
**Version:** 1.0 — Final

**Related Documents:**

* Outbox Technical Requirements
* Outbox Technical Reference
* Outbox Integration Guide
* Event Dispatching + Outbox Architecture
* End-to-End Outbox Dispatching + Per-Handler Idempotency
* Dead Letter Queue for the Outbox Pipeline

---

## 1. Overview

This document defines the end-to-end workflows for the Outbox pipeline. It covers:

* Writing an integration event from a business fact atomically with business state.
* Delivering persisted integration events to registered handlers.
* Per-handler idempotency on retries and concurrent execution.
* Poison message handling and dead-lettering.
* Safe reprocessing of dead-lettered messages.
* Correlation ID propagation across the asynchronous boundary.
* Cancellation and concurrent-execution safety.

The workflows describe what happens between two endpoints: a business operation that produces a fact and the eventual side effects of that fact on other parts of the system.

Every state transition, transaction boundary, and failure mode is identified.

---

## 2. Actors and Components

### 2.1 Actors

| Actor                         | Description                                                                  |
| ----------------------------- | ---------------------------------------------------------------------------- |
| **Command Handler**           | Application-layer handler executing a business use case.                     |
| **Aggregate**                 | Domain entity raising a domain event.                                        |
| **Unit of Work**              | Transaction manager orchestrating persistence and domain-event dispatch.     |
| **Domain Event Handler**      | Application-layer handler reacting to a domain event inside the transaction. |
| **Outbox Writer**             | Infrastructure bridge that serializes and persists integration events.       |
| **Outbox Processing Job**     | Hangfire recurring job that dispatches persisted messages.                   |
| **Integration Event Handler** | Subscriber-owned handler in a bounded context.                               |
| **Idempotency Repository**    | Persistent marker store for per-handler deduplication.                       |
| **Dead Letter Repository**    | Persistent quarantine store for poison messages.                             |
| **Dead Letter Monitor Job**   | Hangfire recurring job that scans the dead-letter table.                     |
| **Operator**                  | Human responsible for manually reprocessing dead-lettered messages.          |

### 2.2 Key Components

| Component                                    | Layer          | Responsibility                                                                             |
| -------------------------------------------- | -------------- | ------------------------------------------------------------------------------------------ |
| `UnitOfWork`                                 | Infrastructure | Two-phase save; atomicity across business state, domain-event dispatch, and Outbox writes. |
| `DomainEventDispatcher`                      | Infrastructure | Resolves and invokes `IDomainEventHandler<T>` inside the transaction.                      |
| `IOutboxMessageWriter`                       | Application    | Sanctioned entry point for persisting integration events.                                  |
| `OutboxMessageWriter`                        | Infrastructure | Serializes an integration event and persists it as an `OutboxMessage`.                     |
| `EnrichedOutboxMessageWriter`                | Infrastructure | Decorator that logs publication with the correlation ID.                                   |
| `OutboxProcessingJob`                        | Infrastructure | Recurring job that fetches, dispatches, retries, and dead-letters Outbox messages.         |
| `OutboxDispatchService`                      | Infrastructure | Resolves event types, deserializes payloads, and delegates to the dispatcher.              |
| `IntegrationEventDispatcher`                 | Infrastructure | Resolves and invokes `IIntegrationEventHandler<T>`.                                        |
| `IdempotentIntegrationEventHandler<T>`       | Application    | Per-handler idempotency wrapper.                                                           |
| `CorrelationScopeIntegrationEventHandler<T>` | Application    | Restores the correlation ID into the logging scope.                                        |
| `ProcessedEventRepository`                   | Infrastructure | Persistent marker store for per-handler idempotency.                                       |
| `DeadLetterRepository`                       | Infrastructure | Persists dead-letter messages.                                                             |
| `OutboxSerializer`                           | Infrastructure | Serializes and deserializes integration-event payloads using JSON.                         |

---

## 3. Workflow 1 — Writing an Integration Event

### 3.1 Trigger

A command handler executes a business operation that mutates an aggregate. The aggregate raises a domain event, and the command handler then calls `UnitOfWork.SaveChangesAsync`.

### 3.2 Flow Diagram

```text
Command Handler
    |
    | aggregate.MarkPaid()
    v
Aggregate
    ├── Status = Paid
    ├── PaidAtUtc = DateTime.UtcNow
    └── AddDomainEvent(new OrderPaidDomainEvent(...))
    |
    | await UnitOfWork.SaveChangesAsync(ct)
    v
UnitOfWork
    ├── Open transaction (if not already active)
    |
    ├── Save #1 — persist aggregate state
    |
    ├── CollectDomainEvents() from ChangeTracker
    |
    ├── DomainEventDispatcher.DispatchAsync(domainEvents)
    |       |
    |       └── For each domain event, for each handler:
    |               IDomainEventHandler<T>.Handle(...)
    |                   ├── (Optional) Internal work — mutates other aggregates
    |                   └── (Optional) IOutboxMessageWriter.WriteAsync(integrationEvent)
    |                           |
    |                           v
    |                       OutboxMessageWriter
    |                           ├── Serialize payload
    |                           ├── Build OutboxMessage row
    |                           └── _outboxRepository.AddAsync(message)
    |                               (Queued in ChangeTracker; not yet saved)
    |
    ├── ClearDomainEvents()
    |
    ├── Save #2 — persist handler changes and Outbox rows
    |
    ├── Commit transaction
    |
    └── Run post-commit callbacks
        (Best-effort; exceptions swallowed)
```

### 3.3 Transaction Boundary

The entire write path executes inside a single database transaction. The following changes are committed together:

* The aggregate state change from Save #1.
* Any aggregate state changes performed by domain-event handlers and persisted during Save #2.
* Any `OutboxMessage` rows added by domain-event handlers and persisted during Save #2.

If any step between opening the transaction and committing it throws, the transaction rolls back.

There is no partial commit: the business fact and its durable integration events succeed or fail together.

### 3.4 Important Rules

* A domain-event handler may produce zero, one, or many integration events. It is not required to produce any.
* `IOutboxMessageWriter.WriteAsync` stages the row in the `DbContext` change tracker. It does **not** commit independently. Committing is the Unit of Work's responsibility.
* Domain events are cleared after dispatch. They are never persisted.
* The Outbox table stores only integration events. Domain events never appear in it.
* The correlation ID is obtained from `IAppContext` when a request context exists and becomes part of the persisted payload.

### 3.5 Failure Behavior

| Failure Point                                                                          | Result                                                                                                                                                           |
| -------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Aggregate mutation throws                                                              | Transaction rolls back; no state change or Outbox row is committed.                                                                                              |
| Domain-event handler throws                                                            | Transaction rolls back; aggregate state changes are discarded.                                                                                                   |
| Outbox write throws, such as during serialization or because of a constraint violation | Transaction rolls back; aggregate state changes are discarded.                                                                                                   |
| Save #2 throws                                                                         | Transaction rolls back; all changes are discarded.                                                                                                               |
| Commit throws                                                                          | The transaction is treated as failed; rollback is attempted where applicable. The final database outcome may be uncertain if the connection fails during commit. |
| Post-commit callback throws                                                            | Exception is swallowed; the transaction has already committed, and the callback is best-effort.                                                                  |

---

## 4. Workflow 2 — Delivering an Integration Event

### 4.1 Trigger

Hangfire fires the recurring `outbox-processor` job every minute.

### 4.2 Flow Diagram

```text
Hangfire Scheduler
    |
    v
OutboxProcessingJob.ExecuteAsync(IJobCancellationToken)
    |
    ├── ct = IJobCancellationToken.ShutdownToken
    |
    ├── GetPendingMessagesAsync(batchSize: 20, ct)
    |       |
    |       v
    |   OutboxMessages
    |       WHERE Status IN (Pending, Failed)
    |       ORDER BY OccurredAt
    |
    ├── For each message (sequentially):
    |       |
    |       ├── ct.ThrowIfCancellationRequested()
    |       |
    |       ├── If message.RetryCount >= MaxRetryCount:
    |       |       └── Workflow 5 — Dead Letter
    |       |
    |       └── Otherwise:
    |               |
    |               ├── Try:
    |               |     OutboxDispatchService.DispatchMessageAsync(message, ct)
    |               |         ├── Resolve type from message.EventType
    |               |         ├── Deserialize payload
    |               |         └── IntegrationEventDispatcher.DispatchAsync(integrationEvent, ct)
    |               |                 └── For each registered handler:
    |               |                         CorrelationScope
    |               |                             → Idempotency
    |               |                                 → Concrete Handler
    |               |
    |               ├── MarkAsProcessedAsync(message.Id, ct)
    |               └── Log success
    |
    |               Catch OperationCanceledException
    |                   when ct.IsCancellationRequested:
    |                       └── Rethrow; leave message in its current state
    |
    |               Catch Exception ex:
    |                   ├── MarkAsFailedAsync(message.Id, ex.ToString(), ct)
    |                   └── Log failure
    |
    └── Log batch summary:
        processed / failed / dead-lettered
```

### 4.3 Batch Semantics

* **Batch size:** 20 messages per cycle.
* **Ordering:** Messages are selected chronologically by `OccurredAt`.
* **Sequential processing:** One message is processed at a time within the batch.
* **Failure isolation:** A failure in one message does not abort the batch; subsequent messages continue.
* **Cancellation:** Cancellation is checked between messages. Cancelled in-flight work remains retryable if it has not been marked as processed.

### 4.4 Processing Outcomes

| Outcome                       | Message Status                                            | Next-Cycle Behavior                                                        |
| ----------------------------- | --------------------------------------------------------- | -------------------------------------------------------------------------- |
| All handlers succeeded        | `Processed`                                               | Message is not fetched again.                                              |
| One or more handlers failed   | `Failed` (`RetryCount` incremented)                       | Message is fetched again on the next cycle.                                |
| Retry cap reached             | `DeadLettered`                                            | Message is excluded from normal fetching.                                  |
| Cancellation during the cycle | Unchanged, provided no state update has already committed | Message remains eligible for retry if its status is `Pending` or `Failed`. |

### 4.5 Important Rules

* The job uses `[DisableConcurrentExecution(timeoutInSeconds: 120)]`. This mitigates overlapping executions under the same Hangfire storage coordination, but it should not be treated as a universal guarantee against concurrent processing across all deployments or failure scenarios.
* The job accepts `IJobCancellationToken` and respects cancellation without treating requested cancellation as an ordinary handler failure.
* A message is marked `Processed` only after **every** registered handler succeeds or is skipped because its processed marker already exists. See Workflow 3.
* If a message cannot be moved to the dead-letter table because of an insert failure or transaction rollback, it remains recoverable in its prior state and can be retried on a subsequent cycle.

---

## 5. Workflow 3 — Per-Handler Idempotent Execution

### 5.1 Trigger

The `IntegrationEventDispatcher` resolves all handlers for an event type. Each handler is wrapped by two decorators:

```text
CorrelationScopeIntegrationEventHandler<T>
    → IdempotentIntegrationEventHandler<T>
        → ConcreteHandler
```

### 5.2 Flow Diagram — Single Handler

```text
IntegrationEventDispatcher
    |
    v
CorrelationScopeIntegrationEventHandler<T>.HandleAsync(event, ct)
    |
    ├── BeginCorrelationScope(event)
    |       ├── If event.CorrelationId is null or empty:
    |       |       └── Do not create a correlation scope
    |       └── Otherwise:
    |               └── Push CorrelationId onto the logging scope
    |
    ├── Log debug:
    |       "Handling integration event {EventType} (Handler: {HandlerType})"
    |
    v
IdempotentIntegrationEventHandler<T>.HandleAsync(event, ct)
    |
    ├── handlerId = inner.GetType().FullName
    |
    ├── await _processedEvents.IsProcessedAsync(event.EventId, handlerId, ct)
    |       └── If true:
    |               └── Return; skip execution
    |
    ├── await _inner.HandleAsync(event, ct)
    |       └── Execute concrete business logic
    |
    └── await _processedEvents.MarkAsProcessedAsync(event.EventId, handlerId, ct)
            └── If a concurrent insert race occurs:
                    └── Catch the recognized unique-key violation,
                        detach the failed entity, and treat the marker
                        as already recorded
```

### 5.3 Idempotency Rules

* The idempotency marker is keyed by `(EventId, HandlerIdentifier)`.
* The handler identifier is the fully qualified CLR type name of the concrete handler class.
* The initial marker check is a fast path. The database enforces uniqueness through a composite primary key.
* A recognized concurrent insert conflict on the same key is treated as success because the marker already exists or has been inserted by the competing execution.
* Only the marker is deduplicated. Concurrent execution can still duplicate a handler's side effect.
* The marker is recorded **after** the concrete handler returns successfully.

### 5.4 Sequential Retry Behavior

Consider a message that fails on the first attempt and succeeds on the second.

**Attempt 1**

```text
Handler A
    → Succeeds
    → Marker (EventId, "HandlerA") inserted

Handler B
    → Throws
    → Exception propagates

OutboxProcessingJob
    → MarkAsFailedAsync
    → RetryCount = 1
```

**Attempt 2**

```text
Handler A
    → Marker exists
    → Skipped

Handler B
    → No marker exists
    → Executes
    → Succeeds
    → Marker inserted

OutboxProcessingJob
    → MarkAsProcessedAsync
```

Handlers that succeeded on the first attempt are not re-executed during a sequential retry. Only handlers that failed or were never reached run again, assuming their markers remain available.

### 5.5 Concurrent Execution Behavior

Two workers processing the same event simultaneously are possible in multi-server deployments or other concurrency scenarios, even when overlapping Hangfire executions are mitigated.

```text
Worker A                              Worker B
    |                                     |
    | IsProcessed? → false                |
    |                                     | IsProcessed? → false
    |                                     |
    | Handler runs                        | Handler runs
    |                                     |
    | MarkAsProcessed → INSERT succeeds   |
    |                                     | MarkAsProcessed → INSERT fails
    |                                     |     └── Recognized unique-key conflict
    |                                     |         → Detach entity
    |                                     |         → Treat marker as recorded
```

The handler's side effect may execute twice, even though the marker is written only once.

This is a fundamental limitation of non-transactional idempotency: the marker and an external side effect cannot be committed atomically across independent systems.

### 5.6 Mark-Then-Crash Window

If the process crashes after the handler succeeds but before `MarkAsProcessedAsync` commits, the marker does not exist. On retry, the handler executes again.

This window cannot be closed by the pipeline alone.

Handlers whose external side effects are critical should implement their own idempotency mechanism, such as passing a unique event or operation ID to a downstream system that supports deduplication.

---

## 6. Workflow 4 — Correlation ID Propagation

### 6.1 Trigger

An integration event carries a `CorrelationId` field. The domain-event handler that produces the event sets this field using `IAppContext.CorrelationId`, captured from the current HTTP request.

### 6.2 Flow Diagram

```text
HTTP Request
    |
    v
CorrelationIdMiddleware
    ├── Read or generate correlation ID
    ├── Store in HttpContext.Items
    ├── Echo in response header
    └── Push onto logging scope
            |
            v
Command Handler
    → Aggregate
        → Domain Event
            |
            v
Domain Event Handler
    └── IntegrationEvent.CorrelationId = IAppContext.CorrelationId
            |
            v
OutboxMessageWriter
    └── Persist payload JSON containing CorrelationId
            |
            v
        [Transaction commits]
        [Later, in a different process or scope]
            |
            v
OutboxProcessingJob
    → OutboxDispatchService
        → IntegrationEventDispatcher
            |
            v
CorrelationScopeIntegrationEventHandler
    ├── Read integrationEvent.CorrelationId
    └── Push it onto the logging scope
            |
            v
Idempotency Decorator
    → Concrete Handler
        → Downstream Calls
            └── Log entries carry the same CorrelationId
```

### 6.3 Continuity Guarantees

* The correlation ID from the original request is available to domain-event-handler logs when those handlers execute within the request's logging scope.
* It is present on the log entry produced by `CorrelationScopeIntegrationEventHandler`, provided the event contains a correlation ID.
* It is available to the concrete handler and downstream infrastructure that participates in the same logging scope.
* A single request can be traced across the asynchronous pipeline using one correlation ID, provided it is captured and propagated correctly.

### 6.4 Important Rules

* The correlation ID must be set on the integration event **before** `IOutboxMessageWriter.WriteAsync` is called.
* Setting it in the handler body after `WriteAsync` has executed has no effect on the already-serialized payload.
* For events produced outside an HTTP request, such as scheduled jobs, the correlation ID may be `null`. In that case, the correlation-scope decorator does not create a correlation scope for the event.

---

## 7. Workflow 5 — Poison Message Handling

### 7.1 Trigger

During a processing cycle, `OutboxProcessingJob` fetches a message whose:

```csharp
message.RetryCount >= MaxRetryCount
```

### 7.2 Flow Diagram

```text
OutboxProcessingJob.ProcessMessageAsync
    |
    ├── IsPoisonMessage(message) → true
    |
    v
TryMoveToDeadLetterAsync
    |
    ├── Log warning:
    |       "Moving outbox message {Id} to dead-letter after {RetryCount} retries"
    |
    ├── Build DeadLetterMessage
    |       ├── Id = Guid.NewGuid()
    |       ├── OriginalMessageId = message.Id
    |       ├── EventType = message.EventType
    |       ├── Payload = message.Payload
    |       ├── Error = message.Error
    |       ├── RetryCount = message.RetryCount
    |       ├── DeadLetteredAt = DateTime.UtcNow
    |       └── Status = DeadLettered
    |
    ├── BEGIN TRANSACTION
    |
    ├── _deadLetterRepo.AddAsync(deadLetter, ct)
    |
    ├── _outboxRepo.MarkAsDeadLetteredAsync(message.Id, ct)
    |
    ├── COMMIT
    |
    └── On any failure:
            ├── ROLLBACK
            ├── Log error
            └── Message remains in its prior state
```

### 7.3 Transaction Boundary

Both writes—the dead-letter insert and the Outbox status update—are committed in a single transaction.

There is no committed state in which the dead-letter record exists without the corresponding Outbox status update, or vice versa.

If the transaction fails before committing, both operations are rolled back. The message remains in its prior state and can be retried during a subsequent cycle.

### 7.4 Failure Behavior

| Failure Point               | Result                                                                                                                                                 |
| --------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Dead-letter insert throws   | Rollback; log error; message remains in its prior state and can be retried.                                                                            |
| Outbox status update throws | Rollback; log error; message remains in its prior state and can be retried.                                                                            |
| Commit fails                | Rollback is attempted where applicable; the message remains recoverable, subject to the actual database outcome if the connection fails during commit. |

**Recovery principle:** A failure during dead-lettering must not silently discard the message. The message remains recoverable through the Outbox record, and the next processing cycle can attempt the transition again.

### 7.5 Dead-Letter Exclusions

* `GetPendingMessagesAsync` filters on `Status IN (Pending, Failed)`. Messages marked `DeadLettered` are not fetched again.
* The `Processing` state exists in the enum but is not set by any current code path. It is reserved for a future claim-based multi-instance deployment.

### 7.6 Dead-Letter Monitoring

A separate recurring job, `DeadLetterMonitorJob`, runs every hour.

```text
Hangfire Scheduler
    |
    v
DeadLetterMonitorJob.ExecuteAsync(IJobCancellationToken)
    |
    ├── GetDeadLetteredAsync(ct)
    |
    └── If count > 0:
            └── Log warning:
                "Dead lettered messages detected: {Count}. Oldest: {Timestamp}"
```

Alert routing through email, Slack, PagerDuty, or similar systems is external to the current implementation.

The warning log is the operational signal used to identify the presence of dead-lettered messages.

---

## 8. Workflow 6 — Dead-Letter Reprocessing

### 8.1 Trigger

An operator has identified the root cause of a poison message and wants to requeue it for delivery.

### 8.2 Flow Diagram — Manual Reprocessing

```text
Operator
    |
    ├── Investigate the DeadLetterMessages row
    |       ├── EventType
    |       ├── Payload
    |       ├── Error (last error)
    |       └── RetryCount
    |
    ├── Fix the root cause
    |       ├── Deploy a code fix
    |       ├── Correct invalid data
    |       └── Restore an unavailable service
    |
    ├── Requeue
    |       |
    |       ├── INSERT a new OutboxMessage with:
    |       |       ├── Id = Guid.NewGuid()
    |       |       ├── EventType = deadLetter.EventType
    |       |       ├── Payload = deadLetter.Payload
    |       |       ├── OccurredAt = DateTime.UtcNow
    |       |       ├── RetryCount = 0
    |       |       └── Status = Pending
    |       |
    |       └── Optionally update the DeadLetterMessage:
    |               └── Status = Reprocessing
    |                   (or retain it for audit)
    |
    v
Next OutboxProcessingJob cycle
    |
    ├── Fetch the new message (Status = Pending)
    |
    ├── Dispatch to handlers
    |       |
    |       └── Idempotency decorator skips handlers whose
    |           (EventId, HandlerIdentifier) markers already exist
    |
    └── Mark Processed
```

### 8.3 Idempotency on Reprocessing

The requeued message preserves the original `EventId`, which is part of the serialized payload.

Handlers that already succeeded for that event can be skipped through the `ProcessedEvents` table. Handlers that never succeeded or whose execution was not recorded may run again.

This prevents previously recorded successful handlers from being executed again during **sequential reprocessing**, provided their markers remain available. It does not eliminate duplicate side effects caused by concurrent execution or a crash before a marker is committed.

### 8.4 Important Rules

* **Preserve the `EventId`:** Do not regenerate it, because doing so would break continuity with existing idempotency markers.
* **Preserve the payload:** Reuse the payload stored in the dead-letter row without modifying it.
* **Reset the retry count:** Set `RetryCount = 0` on the new Outbox message. The original retry count remains available in the dead-letter record for auditing.
* **Retain the dead-letter record:** It may be marked `Reprocessing` or retained for audit. Deleting it should be a deliberate retention decision, not an automatic assumption.

### 8.5 Failure During Reprocessing

If the requeued message fails again, it follows the normal retry cycle. If it reaches the retry cap, another dead-letter record may be created.

There is no automatic deduplication of dead-letter records. Operators should be aware that repeated failures can produce multiple dead-letter entries associated with the same `EventId`.

### 8.6 Current Mechanism

Reprocessing is **manual** today.

There is currently:

* No dedicated administrative endpoint.
* No automated reprocessing job.
* No reprocessing UI.

Operators execute SQL directly to requeue messages.

---

## 9. Workflow 7 — Batch Cycle Summary

### 9.1 Trigger

Every execution of `OutboxProcessingJob`.

### 9.2 Flow Diagram

```text
ExecuteAsync
    |
    ├── GetPendingMessagesAsync(batchSize, ct)
    |
    ├── If messages.Count == 0:
    |       └── Return without logging a batch summary
    |
    ├── For each message:
    |       └── Increment the appropriate counter:
    |           processed / failed / dead-lettered
    |
    └── Log:
        "Outbox batch complete: {Processed} processed,
         {Failed} failed, {DeadLettered} dead-lettered"
```

### 9.3 Purpose

A single Information-level log entry per non-empty cycle summarizes the outcome. It provides an operational signal for the Outbox pipeline's health.

| Observed Counts                  | Interpretation                                                |
| -------------------------------- | ------------------------------------------------------------- |
| `0 / 0 / 0`                      | The queue is empty; no summary log is emitted.                |
| `N / 0 / 0`                      | Healthy processing, with `N` messages processed successfully. |
| `N / M / 0`, where `M > 0`       | Handler or dispatch failures occurred.                        |
| Any non-zero dead-lettered count | Poison messages are being moved out of the normal retry loop. |

Operators can monitor these patterns to detect:

* Rising failure rates.
* New dead-lettered messages.
* Batch starvation or persistently underfilled batches.
* Changes in processing throughput.

The summary should be interpreted alongside queue depth, oldest-message age, and dead-letter monitoring for a more complete operational picture.

---

## 10. Cross-Workflow Concerns

### 10.1 Transaction Boundaries Summary

| Workflow                     | Transaction                                                                       | Participants                                                                                                                          |
| ---------------------------- | --------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| Write an integration event   | Single transaction                                                                | Business state, domain-event-handler changes, and Outbox rows.                                                                        |
| Deliver an integration event | No single outer transaction spanning all handlers                                 | Handler side effects and `ProcessedEvents` markers may use separate transactions, depending on handler and repository implementation. |
| Dead-letter a message        | Single explicit transaction                                                       | `DeadLetterMessage` insert and `OutboxMessage` status update.                                                                         |
| Requeue a dead letter        | Single transaction per requeue operation, if implemented through the Unit of Work | New `OutboxMessage` row; any dead-letter status update included in the same Unit of Work.                                             |

### 10.2 Concurrency Summary

| Concern                                      | Mechanism                                                                                           | Scope                                                                                       |
| -------------------------------------------- | --------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------- |
| Overlapping `OutboxProcessingJob` executions | `[DisableConcurrentExecution(timeoutInSeconds: 120)]`                                               | Hangfire storage coordination; not an unconditional guarantee across all failure scenarios. |
| Concurrent idempotency-marker insertion      | Composite primary key and handling of recognized SQL Server unique-key violations (`2601` / `2627`) | Database level.                                                                             |
| Concurrent dead-letter transition            | Explicit transaction wrapping the dead-letter insert and Outbox status update                       | Database transaction.                                                                       |
| Concurrent domain-event dispatch             | Transaction per `SaveChangesAsync` operation, according to the Unit of Work design                  | Scoped Unit of Work.                                                                        |

### 10.3 Correlation ID Propagation

The correlation ID propagates across the pipeline:

```text
HTTP Request
    → CorrelationIdMiddleware
    → HttpContext + LogContext
    → Command Handler
    → Aggregate.AddDomainEvent(...)
    → Domain Event Handler
    → IntegrationEvent.CorrelationId
    → OutboxMessageWriter
    → Persisted JSON Payload
    → OutboxProcessingJob
    → OutboxDispatchService
    → CorrelationScopeIntegrationEventHandler
    → LogContext
    → Concrete Handler
    → Downstream Calls
```

### 10.4 Idempotency Guarantees

| Scenario                                        | Guarantee                                                                                      |
| ----------------------------------------------- | ---------------------------------------------------------------------------------------------- |
| Sequential retry after a handler failure        | Previously successful handlers with recorded markers are skipped.                              |
| Concurrent execution of the same message        | The marker is written once, but the side effect may execute more than once.                    |
| Crash between handler success and marker commit | The handler may execute again on retry.                                                        |
| Reprocessed dead letter                         | Previously successful handlers with preserved markers are skipped during sequential execution. |

The pipeline provides **at-least-once delivery with sequential per-handler deduplication**. It does not provide exactly-once execution across arbitrary external systems.

---

## 11. Testing Workflows

### 11.1 Domain-Event Write Flow

* Publish a domain event that produces an integration event.
* Assert that the aggregate state is committed.
* Assert that a single row exists in `OutboxMessages` with the expected `EventType` and `Payload`.
* Assert that the row has `Status = Pending` and `RetryCount = 0`.
* Verify that a failure during the write transaction does not leave the business state committed without its corresponding Outbox row.

### 11.2 Delivery Flow

* Insert an `OutboxMessage` manually or through a business flow.
* Trigger the job's `ExecuteAsync` with an appropriate cancellation token.
* Assert that the handler ran.
* Assert that the corresponding `ProcessedEvents` marker exists.
* Assert that the Outbox row has `Status = Processed` and `ProcessedAt` populated.

### 11.3 Idempotent Retry Flow

* Configure a handler to fail on its first invocation and succeed on its second.
* Run the job twice.
* Assert that the first run marks the message `Failed` and increments `RetryCount` to `1`.
* Assert that the second run marks the message `Processed`.
* Assert that previously successful handlers are not re-executed during the sequential retry.
* Assert that the failing handler executes again and eventually succeeds.

### 11.4 Poison-Message Flow

* Configure a handler to always fail.
* Run the job until the configured retry cap is reached and the subsequent processing cycle performs the dead-letter transition, according to the implementation's retry-count convention.
* Assert that a `DeadLetterMessage` row was created.
* Assert that the Outbox row is marked `DeadLettered`.
* Assert that subsequent processing cycles do not fetch the message.

### 11.5 Correlation Propagation

* Send an HTTP request with a known `X-Correlation-ID`.
* Trigger the full pipeline.
* Assert that the correlation ID is present in the concrete handler's logs.
* Verify that the persisted event payload contains the expected correlation ID.

### 11.6 Cancellation Flow

* Start a batch containing multiple messages.
* Cancel the `IJobCancellationToken` during batch processing.
* Assert that cancellation is not recorded as an ordinary handler failure.
* Assert that an uncompleted message remains eligible for retry.
* Assert that subsequent runs process the message normally.

---

## 12. Conclusion

The Outbox pipeline is defined by seven workflows that operate together:

1. **Write:** Atomically persist integration events with business state.
2. **Deliver:** Dispatch persisted integration events to registered handlers in the background.
3. **Idempotent execution:** Deduplicate successful handler executions during sequential retries.
4. **Correlation propagation:** Trace a request across the asynchronous boundary using a shared correlation ID.
5. **Poison handling:** Move messages that exhaust the retry policy to the dead-letter table atomically.
6. **Reprocessing:** Requeue dead-lettered messages while preserving event identity and existing idempotency markers.
7. **Batch summary:** Report processing outcomes through structured operational logs.

The pipeline provides **at-least-once delivery with sequential per-handler deduplication**. Once a business transaction commits, its integration events remain durably recorded for processing. Subject to infrastructure recovery and the configured retry policy, messages are delivered to their registered handlers or quarantined for operator investigation.

The pipeline does not guarantee exactly-once execution across independent systems. Duplicate side effects remain possible when handlers execute concurrently or a process fails before its idempotency marker is committed.

---

**Document Version:** 1.0 — Final
**Document Type:** Outbox Workflow Technical Document

This document defines the seven end-to-end workflows of the transactional Outbox pipeline: the write path, delivery path, per-handler idempotency, correlation propagation, poison-message handling, dead-letter reprocessing, and batch summary. Each workflow describes its trigger, execution flow, transaction boundaries, failure modes, and operational rules.
