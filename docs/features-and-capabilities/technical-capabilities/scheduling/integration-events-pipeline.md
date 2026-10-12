\# E-Commerce Integration Events Pipeline



\## Durable Asynchronous Communication — Technical Reference Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Platform:\*\* .NET 8 / ASP.NET Core / EF Core / SQL Server

\*\*Version:\*\* 1.0 — Final



\---



\## 1. Overview



The Integration Events Pipeline provides \*\*durable, eventually consistent communication between modules\*\* within the E-Commerce modular monolith.



It handles integration events representing business facts that have already occurred and need to be communicated to other modules or trigger reliable asynchronous side effects.



The pipeline is built around three core mechanisms:



\* \*\*Transactional Outbox:\*\* Persists integration events atomically with the business transaction that produces them.

\* \*\*Outbox Processing:\*\* Uses a background job to retrieve persisted events, dispatch them to registered handlers, and manage delivery outcomes.

\* \*\*Idempotent Handling:\*\* Uses durable processed-event records and appropriate transaction boundaries to prevent duplicate business effects when events are delivered more than once.



The pipeline is architecturally separate from the Independent Background Jobs Pipeline. It uses the same Hangfire infrastructure but does not execute integration-event handlers through the Scheduling module's `JobOrchestrator`.



\### 1.1 Architectural Goals



The pipeline is designed to provide:



\* Durable event recording.

\* Atomic persistence of business changes and corresponding outbox messages.

\* Eventual delivery after the original business transaction commits.

\* Retryable processing following transient failures.

\* Duplicate-tolerant delivery.

\* Clear ownership of integration-event contracts and handlers.

\* Isolation between business modules.

\* Observability of pending, processed, and failed messages.



\### 1.2 Delivery Guarantees



The pipeline provides \*\*at-least-once delivery with idempotent processing where correctly implemented\*\*.



It does not automatically guarantee exactly-once execution of arbitrary side effects, especially when those effects involve external systems such as email providers, search services, or payment APIs.



The distinction matters:



| Guarantee                    | Meaning                                                                                                                                         |

| ---------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |

| Durable event recording      | The event is persisted in the database transaction with the business changes that produce it.                                                   |

| At-least-once delivery       | An event may be dispatched again after a failure or uncertain outcome.                                                                          |

| Idempotent handling          | Reprocessing the same event does not repeat the same logical business effect.                                                                   |

| Eventual processing          | A persisted event can eventually be processed if the system recovers and processing continues.                                                  |

| Exactly-once external effect | Requires additional guarantees from the destination or an appropriate coordination mechanism; the outbox and decorator alone do not provide it. |



\*\*Important:\*\* These guarantees depend on transaction boundaries, retry behavior, durable storage, and correct failure recovery.



\---



\## 2. Architecture Overview



\### 2.1 End-to-End Pipeline



```text

┌──────────────────────────────────────────┐

│             Business Transaction        │

│                                          │

│  Command Handler                         │

│       │                                  │

│       ▼                                  │

│  Aggregate State Change                  │

│       │                                  │

│       ▼                                  │

│  Domain Event                             │

│       │                                  │

│       ▼                                  │

│  Domain Event Handler / Event Translator │

│       │                                  │

│       ▼                                  │

│  IOutboxMessageWriter.WriteAsync()       │

│       │                                  │

│       ▼                                  │

│  Outbox Message Persisted                │

│                                          │

│  Business State + Outbox Message         │

│             Commit Atomically             │

└────────────────────┬─────────────────────┘

&#x20;                    │

&#x20;                    ▼

┌──────────────────────────────────────────┐

│        Asynchronous Outbox Processing    │

│                                          │

│  OutboxProcessingJob                     │

│       │                                  │

│       ▼                                  │

│  Retrieve Pending Messages               │

│       │                                  │

│       ▼                                  │

│  OutboxDispatchService                   │

│       │                                  │

│       ▼                                  │

│  Deserialize Integration Event           │

│       │                                  │

│       ▼                                  │

│  IIntegrationEventDispatcher              │

│       │                                  │

│       ▼                                  │

│  Registered Integration Event Handlers   │

│       │                                  │

│       ▼                                  │

│  Durable Idempotency Check                │

│       │                                  │

│       ▼                                  │

│  Execute Handler                         │

│       │                                  │

│       ▼                                  │

│  Persist Handler Outcome                 │

│                                          │

│  Mark Outbox Message Processed            │

└──────────────────────────────────────────┘

```



\### 2.2 Architectural Boundaries



The pipeline has three logical responsibilities:



\*\*Application\*\*



\* Defines integration-event contracts.

\* Defines handler and dispatcher abstractions.

\* Contains integration-event handlers owned by the relevant feature or module.

\* Defines the idempotency abstraction or decorator when that behavior belongs to the Application layer.



\*\*Infrastructure\*\*



\* Persists outbox messages.

\* Serializes and deserializes event payloads.

\* Implements event dispatching.

\* Implements processed-event persistence.

\* Coordinates message retrieval, dispatch, and delivery-state updates.

\* Integrates the outbox processor with Hangfire.



\*\*Scheduling\*\*



\* Provides the shared Hangfire execution infrastructure for independent background jobs.

\* Does not own integration-event contracts, business event handling, or outbox delivery semantics.



The outbox processor is a specialized background job that drives the messaging pipeline. It is not an integration-event handler itself.



\---



\## 3. Folder and Class Structure



The following is a logical structure. Exact namespaces and directories may vary with the existing solution conventions.



\### 3.1 Application Layer



```text

Application/

├── Shared/

│   ├── Abstractions/

│   │   └── Messaging/

│   │       ├── IIntegrationEvent.cs

│   │       ├── IIntegrationEventHandler.cs

│   │       ├── IIntegrationEventDispatcher.cs

│   │       ├── IOutboxMessageWriter.cs

│   │       └── IProcessedEventRepository.cs

│   │

│   └── Messaging/

│       └── Decorators/

│           └── IdempotentIntegrationEventHandler.cs

│

└── Modules/

&#x20;   ├── Orders/

&#x20;   │   ├── IntegrationEvents/

&#x20;   │   │   └── OrderPlacedIntegrationEvent.cs

&#x20;   │   └── IntegrationEventHandlers/

&#x20;   │       └── SendOrderConfirmationEmailHandler.cs

&#x20;   │

&#x20;   └── Catalog/

&#x20;       ├── IntegrationEvents/

&#x20;       │   └── ProductCreatedIntegrationEvent.cs

&#x20;       └── IntegrationEventHandlers/

&#x20;           └── UpdateSearchIndexHandler.cs

```



\*\*Ownership rule:\*\* The module that owns the business fact owns its integration-event contract. A consuming module implements the handler for that contract.



For example, Ordering owns `OrderPlacedIntegrationEvent`. A Notifications or Customer Engagement module may implement the handler that sends the confirmation email.



The exact module ownership of a handler depends on where the relevant behavior belongs. A handler should not be placed in another module merely because that module owns the event contract.



\### 3.2 Infrastructure Layer



```text

Infrastructure/

├── Communication/

│   └── Messaging/

│       ├── Outbox/

│       │   ├── Contracts/

│       │   │   └── IOutboxMessageRepository.cs

│       │   ├── Entities/

│       │   │   └── OutboxMessage.cs

│       │   ├── Implementation/

│       │   │   └── OutboxMessageWriter.cs

│       │   ├── Processing/

│       │   │   └── OutboxDispatchService.cs

│       │   ├── Serialization/

│       │   │   └── OutboxSerializer.cs

│       │   └── Extensions/

│       │       └── OutboxInfrastructureExtensions.cs

│       │

│       └── Dispatching/

│           ├── IntegrationEventDispatcher.cs

│           └── DomainEventDispatcher.cs

│

├── Persistence/

│   ├── Modules/

│   │   └── Outbox/

│   │       ├── Configurations/

│   │       │   └── OutboxConfiguration.cs

│   │       └── Repository/

│   │           └── OutboxMessageRepository.cs

│   │

│   └── ProcessedEvents/

│       ├── ProcessedEvent.cs

│       ├── ProcessedEventConfiguration.cs

│       └── ProcessedEventRepository.cs

│

└── Scheduling/

&#x20;   └── Hangfire/

&#x20;       └── OutboxProcessingJob.cs

```



The placement of `OutboxProcessingJob` under `Scheduling/Hangfire` is an implementation choice. The important architectural distinction is that this class is an adapter that invokes the outbox-processing mechanism, not the owner of messaging logic.



The Application layer must not depend on Hangfire.



\---



\## 4. Detailed Class Responsibilities



\### 4.1 Application Layer



| Component                              | Responsibility                                                                                                                     |

| -------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |

| `IIntegrationEvent`                    | Defines the contract for an integration event, including a stable event identifier and occurrence timestamp.                       |

| `IIntegrationEventHandler<T>`          | Defines the operation performed when a handler receives an integration event.                                                      |

| `IIntegrationEventDispatcher`          | Resolves and invokes the registered handlers for a particular event type.                                                          |

| `IOutboxMessageWriter`                 | Provides an Application-facing abstraction for recording integration events within the current business transaction.               |

| `IProcessedEventRepository`            | Defines the operations needed to coordinate durable duplicate detection and processing records.                                    |

| `IdempotentIntegrationEventHandler<T>` | Wraps a handler with durable idempotency behavior, provided that the processing and deduplication state are coordinated correctly. |

| `OrderPlacedIntegrationEvent`          | Represents an order-placement fact owned by the Ordering module.                                                                   |

| `SendOrderConfirmationEmailHandler`    | Requests or performs the confirmation-email operation according to the Notifications module's design.                              |

| `ProductCreatedIntegrationEvent`       | Represents a product-creation fact owned by the Catalog module.                                                                    |

| `UpdateSearchIndexHandler`             | Coordinates the corresponding search-index update.                                                                                 |



An integration event should contain the information consumers need to understand the fact being communicated. Prefer stable identifiers over unnecessarily large object graphs or sensitive data.



\### 4.2 Infrastructure Layer



| Component                    | Responsibility                                                                             |

| ---------------------------- | ------------------------------------------------------------------------------------------ |

| `OutboxMessage`              | Represents the durable database record for an integration event.                           |

| `IOutboxMessageRepository`   | Defines persistence and retrieval operations for outbox records.                           |

| `OutboxMessageRepository`    | Implements the repository using the configured persistence technology.                     |

| `OutboxMessageWriter`        | Serializes an event and creates its outbox record within the current business transaction. |

| `OutboxSerializer`           | Converts between integration-event instances and persisted payloads.                       |

| `OutboxDispatchService`      | Coordinates deserialization, dispatch, and processing outcomes for an outbox message.      |

| `IntegrationEventDispatcher` | Resolves and invokes the applicable handler or handlers for a deserialized event.          |

| `ProcessedEventRepository`   | Persists durable event-processing records used for duplicate detection.                    |

| `OutboxProcessingJob`        | Invokes the outbox-processing service on the configured background schedule.               |



The dispatcher should use an explicit and deterministic mapping between event types and handler registrations. Reflection may be used for discovery, but event-type resolution, service lifetimes, and error handling should remain predictable.



\---



\## 5. Transactional Outbox



\### 5.1 Purpose



The Transactional Outbox pattern prevents the application from committing a business change while losing the corresponding integration event because a separate message-publishing operation failed.



Without an outbox, the application might:



1\. Commit an order to the database.

2\. Attempt to publish `OrderPlacedIntegrationEvent`.

3\. Encounter a process crash or network failure before the event is delivered.



The order would exist, but consumers might never learn about it.



The outbox addresses this by recording the event in the same database transaction as the business change.



\### 5.2 Transactional Flow



```text

Begin Database Transaction

&#x20;         │

&#x20;         ├── Persist Business State

&#x20;         │

&#x20;         ├── Persist Outbox Message

&#x20;         │

&#x20;         ▼

Commit Transaction

&#x20;         │

&#x20;         ├── Both changes committed

&#x20;         │

&#x20;         └── Or neither change committed

```



The business changes and outbox record must use the same transactional database context and transaction.



The event is not dispatched as a side effect of the uncommitted business transaction. The background processor reads it after it becomes eligible for processing.



\### 5.3 Event Creation



A typical flow is:



1\. An Application command invokes the relevant domain operation.

2\. The aggregate raises a domain event.

3\. The domain-event dispatch mechanism invokes an appropriate handler or event translator.

4\. The handler creates an integration event when cross-module communication is required.

5\. `IOutboxMessageWriter.WriteAsync()` records the event.

6\. The business state and outbox record are committed atomically.



Not every domain event needs to become an integration event. Domain events represent facts within a domain model; integration events communicate selected facts across module boundaries.



\### 5.4 Outbox Message State



A practical outbox record may contain:



\* `Id`

\* `EventId`

\* `EventType`

\* `Payload`

\* `OccurredAtUtc`

\* `CreatedAtUtc`

\* `ProcessedAtUtc`

\* `Status`

\* `AttemptCount`

\* `NextAttemptAtUtc`

\* `LastError`

\* `LockedUntilUtc` or equivalent lease metadata, if concurrent workers require it



A suitable lifecycle is:



```text

Pending

&#x20;  │

&#x20;  ▼

Processing

&#x20;  │

&#x20;  ├── Success ─────────► Processed

&#x20;  │

&#x20;  ├── Retryable Failure ► Pending / Scheduled Retry

&#x20;  │

&#x20;  └── Terminal Failure ─► DeadLetter / Failed

```



The exact states depend on the implementation. If the system does not persist a separate `Processing` state, it still needs a safe strategy for claiming messages and coordinating concurrent workers.



\### 5.5 Atomicity Boundary



The outbox record must be inserted using the same database transaction as the business state that produced the event.



The later dispatch operation is a separate transaction or set of operations. Its success cannot retroactively alter the original business transaction.



This is the central distinction between transactional event recording and asynchronous event delivery.



\---



\## 6. Idempotency and Duplicate Processing



\### 6.1 Why Idempotency Is Required



Outbox processing is designed to retry messages when delivery outcomes are uncertain.



For example:



1\. A handler performs an operation.

2\. The process fails before the system records the event as processed.

3\. The outbox message is retried.

4\. The handler receives the same event again.



The handler must be designed so that the retry does not unintentionally repeat the same logical effect.



\### 6.2 Idempotency Decorator



A decorator can centralize duplicate detection and keep handler implementations free from repeated idempotency boilerplate.



The following is \*\*illustrative pseudocode\*\*, not a complete concurrency-safe implementation:



```csharp

public sealed class IdempotentIntegrationEventHandler<T>

&#x20;   : IIntegrationEventHandler<T>

&#x20;   where T : IIntegrationEvent

{

&#x20;   private readonly IIntegrationEventHandler<T> \_inner;

&#x20;   private readonly IProcessedEventRepository \_processedEvents;



&#x20;   public IdempotentIntegrationEventHandler(

&#x20;       IIntegrationEventHandler<T> inner,

&#x20;       IProcessedEventRepository processedEvents)

&#x20;   {

&#x20;       \_inner = inner;

&#x20;       \_processedEvents = processedEvents;

&#x20;   }



&#x20;   public async Task HandleAsync(

&#x20;       T integrationEvent,

&#x20;       CancellationToken cancellationToken)

&#x20;   {

&#x20;       if (await \_processedEvents.IsProcessedAsync(

&#x20;               integrationEvent.EventId,

&#x20;               cancellationToken))

&#x20;       {

&#x20;           return;

&#x20;       }



&#x20;       await \_inner.HandleAsync(

&#x20;           integrationEvent,

&#x20;           cancellationToken);



&#x20;       await \_processedEvents.MarkAsProcessedAsync(

&#x20;           integrationEvent.EventId,

&#x20;           cancellationToken);

&#x20;   }

}

```



This demonstrates the basic check/execute/mark sequence, but it does \*\*not\*\* by itself guarantee exactly-once processing.



\### 6.3 Critical Transaction Boundary



Consider this sequence:



```text

Check ProcessedEvents

&#x20;       │

&#x20;       ▼

Execute Handler

&#x20;       │

&#x20;       ▼

Handler Effect Succeeds

&#x20;       │

&#x20;       ▼

Process Crashes

&#x20;       │

&#x20;       ▼

ProcessedEvents Record Was Not Saved

&#x20;       │

&#x20;       ▼

Event Is Retried

&#x20;       │

&#x20;       ▼

Handler Effect May Be Repeated

```



A second problem arises if two workers process the same event concurrently: both can observe that it has not been processed and execute the handler before either writes the processed-event record.



A reliable implementation therefore needs more than a repository check followed by a separate insert.



\### 6.4 Recommended Idempotency Strategy



\*\*For database-only handler effects:\*\*



Where possible, persist the handler's business changes and the processed-event record within the same database transaction.



Use a unique database constraint on the relevant event-processing identity, and handle concurrent attempts safely. A unique constraint is an important final guard, but it must be combined with transaction and conflict-handling logic.



The processing identity should distinguish the intended scope. If each event has one handler, `EventId` may be sufficient. If one event can have multiple independent handlers, the deduplication key may need to include both `EventId` and a stable handler identifier.



\*\*For external effects:\*\*



A database transaction cannot atomically commit an arbitrary remote API call, email delivery, or search-index update.



Use the destination's idempotency support when available. Otherwise, use a durable operation record, retry-safe semantics, reconciliation, or a further outbox owned by the subsystem responsible for the external effect.



For example, a handler may durably request an email delivery rather than attempting to guarantee that an email provider sends an email exactly once.



\### 6.5 What the Decorator Guarantees



The decorator provides a central place for idempotency logic. Its actual guarantee depends on its implementation.



| Implementation                                                    | Guarantee                                                                           |

| ----------------------------------------------------------------- | ----------------------------------------------------------------------------------- |

| Separate check, handler execution, and mark operations            | Best-effort duplicate suppression; vulnerable to crashes and concurrent processing. |

| Atomic processed-event record plus database-only business changes | Strong transactional idempotency for those database effects.                        |

| Destination-supported idempotency key                             | Duplicate suppression according to the destination's documented guarantees.         |

| Decorator alone around an arbitrary external side effect          | No general exactly-once guarantee.                                                  |



The architectural rule is therefore:



\*\*Use the decorator to centralize idempotency, but enforce correctness at the durable effect boundary.\*\*



\### 6.6 Handler Registration



A typical Scrutor registration may look like this:



```csharp

services.Scan(scan => scan

&#x20;   .FromAssemblies(applicationAssemblies)

&#x20;   .AddClasses(classes =>

&#x20;       classes.AssignableTo(typeof(IIntegrationEventHandler<>)))

&#x20;   .AsImplementedInterfaces()

&#x20;   .WithScopedLifetime());



services.Decorate(

&#x20;   typeof(IIntegrationEventHandler<>),

&#x20;   typeof(IdempotentIntegrationEventHandler<>));

```



This is illustrative. The actual registration must ensure that:



\* All relevant handler assemblies are scanned.

\* The decorator is compatible with the installed Scrutor version and service registrations.

\* Each handler receives the expected scoped dependencies.

\* The dispatcher resolves the decorated registrations.

\* Multiple handlers for the same event type are handled correctly.

\* The deduplication identity reflects whether processing is tracked per event or per handler.



The dispatcher and registration strategy must agree on whether an event has one handler or multiple handlers.



\---



\## 7. End-to-End Processing Flow



\### Step 1 — Business Operation



An Application command executes a business operation, and the relevant aggregate changes state.



\### Step 2 — Domain Event



The aggregate raises a domain event representing a fact that occurred within the domain model.



\### Step 3 — Integration Event Translation



An appropriate domain-event handler or translator creates an integration event when another module needs to be informed.



For example:



```text

OrderPlacedDomainEvent

&#x20;         │

&#x20;         ▼

OrderPlacedIntegrationEvent

```



\### Step 4 — Outbox Persistence



`OutboxMessageWriter` serializes the integration event and creates a pending outbox record.



The write participates in the same transaction as the business state change.



\### Step 5 — Atomic Commit



The transaction commits both the business state and the outbox record.



If the transaction rolls back, neither should be committed.



\### Step 6 — Background Trigger



Hangfire invokes `OutboxProcessingJob` according to the configured schedule.



The recurring interval is an operational setting, not a guarantee that each event will be processed at that exact interval. Queue delays, failures, and backlog can increase delivery latency.



\### Step 7 — Claim Pending Messages



The processing service retrieves an appropriate batch of eligible messages.



When multiple workers or application instances may process messages concurrently, the retrieval strategy must prevent unsafe simultaneous claims. Depending on the design, this can involve database locking, atomic claiming, leases, or an equivalent concurrency-control mechanism.



\### Step 8 — Deserialize the Event



`OutboxDispatchService` uses `OutboxSerializer` to reconstruct the integration event.



Unknown event types, invalid payloads, and incompatible event versions must be handled explicitly rather than silently discarded.



\### Step 9 — Dispatch to Handlers



`IntegrationEventDispatcher` resolves the applicable handler registrations and invokes them using the configured dependency-injection scope.



The dispatcher should not treat successful resolution as successful processing; it must observe handler completion and propagate failures to the outbox-processing layer.



\### Step 10 — Apply Idempotency



The handler's idempotency mechanism determines whether the event has already been processed for the relevant handler or processing scope.



If it has, the handler's business operation can be skipped according to the deduplication design.



\### Step 11 — Execute the Handler



The handler performs its intended operation.



Examples include:



\* Requesting order-confirmation email delivery.

\* Updating a search index.

\* Initiating a downstream module operation.

\* Recording a durable notification request.



\### Step 12 — Persist the Processing Outcome



On success, the system records the appropriate processed-event state and updates the outbox message to `Processed`.



These updates should be coordinated transactionally wherever they share a database and consistency boundary.



If a handler fails, the outbox message must remain eligible for retry or transition to a defined terminal-failure state.



\*\*A message must not be marked as successfully processed merely because it was fetched, deserialized, or passed to the dispatcher.\*\*



\---



\## 8. Retry and Failure Handling



\### 8.1 Failure Categories



| Failure                               | Recommended behavior                                                                               |

| ------------------------------------- | -------------------------------------------------------------------------------------------------- |

| Temporary database or network failure | Retry with controlled backoff.                                                                     |

| Temporary downstream service failure  | Retry when the operation is safe to repeat.                                                        |

| Handler exception                     | Preserve the failure and retry according to the delivery policy.                                   |

| Invalid or unsupported event payload  | Record diagnostics and route to a terminal-failure workflow when retry cannot resolve the problem. |

| Missing handler registration          | Treat as a dispatch failure; alert or quarantine according to policy.                              |

| Duplicate delivery                    | Suppress duplicate effects using the applicable idempotency strategy.                              |

| Repeated permanent failure            | Move the message to a dead-letter or equivalent terminal state and make it observable.             |



\### 8.2 Hangfire Retries Versus Outbox Retries



Hangfire may retry a failed execution of `OutboxProcessingJob`. The outbox implementation may also maintain per-message attempt counts and retry schedules.



These mechanisms operate at different levels:



\* \*\*Hangfire retry:\*\* Retries execution of the background job.

\* \*\*Outbox retry:\*\* Retries an individual persisted message.



Both can be used, but their interaction must be deliberate. Otherwise, a single failing message may repeatedly fail the whole batch and delay unrelated messages.



A robust processor should isolate failures at the message level when appropriate, allowing independent messages to progress while retaining the failed message for retry or investigation.



The system should define which layer owns the retry schedule for each failure type and avoid uncontrolled nested retry loops.



\### 8.3 Dead-Letter Handling



A terminally failed message should retain enough information for diagnosis and recovery, such as:



\* Event ID and type.

\* Creation and last-attempt timestamps.

\* Attempt count.

\* Last error details, with sensitive information redacted.

\* Terminal-failure reason.

\* Any required operator-recovery metadata.



A dead-letter state should not silently delete the event. Operational procedures should explain how to inspect, correct, and safely replay eligible messages.



\---



\## 9. Concurrent Processing and Message Ordering



\### 9.1 Concurrent Workers



If more than one worker or application instance can process the same outbox table, the system must coordinate message claiming.



Fetching pending records and then separately marking them as processing can allow two workers to select the same records.



The implementation should use a concurrency-safe claiming strategy and define lease expiration and recovery behavior if a worker crashes while holding a claim.



\### 9.2 Message Ordering



The outbox does not automatically guarantee global ordering.



If events for the same aggregate or business entity must be processed in order, the implementation must explicitly preserve the required ordering, for example by using an aggregate identifier and sequence number with suitable claim and dispatch rules.



Do not impose global ordering unless the business semantics require it. Unnecessary serialization can reduce throughput.



\### 9.3 Batch Isolation



A failure in one message should not automatically prevent all other messages in the batch from progressing unless the batch's transactional semantics require that behavior.



The processing service should document whether it uses:



\* One transaction per batch.

\* One transaction per message.

\* A transaction for claiming followed by separate processing transactions.



For handlers with independent business effects, per-message isolation is often easier to recover and reason about.



\---



\## 10. Event Contracts and Serialization



Integration-event contracts are durable data contracts. Their compatibility matters because messages may remain in the outbox longer than the deployment that created them.



\### 10.1 Event Identity



Each event should have a stable identifier, such as `EventId`, that remains unchanged across retries.



A retry must not generate a new event identity for the same persisted event.



\### 10.2 Event Type



The serialized record should identify the event type through an explicit, controlled mapping.



Avoid depending on arbitrary runtime type names or unrestricted reflection-based deserialization of untrusted payloads.



\### 10.3 Schema Evolution



When event contracts evolve, consider:



\* Adding optional fields with safe defaults.

\* Maintaining backward-compatible deserialization where feasible.

\* Introducing explicit schema versions when necessary.

\* Supporting old pending messages during deployments.

\* Testing compatibility between serialized payloads and current handlers.



Removing or renaming a property can break events persisted by an earlier application version.



\### 10.4 Payload Contents



Integration events should carry the data needed by consumers without unnecessarily duplicating entire aggregates.



Avoid placing credentials, plaintext secrets, or unnecessary personal information in event payloads. Persisted messages, logs, and dead-letter records must be treated as data that may require access controls and retention policies.



\---



\## 11. Relationship with the Scheduling Module



The Integration Events Pipeline and Independent Background Jobs Pipeline share Hangfire infrastructure but have different responsibilities.



| Concern                       | Integration Events Pipeline                                     | Independent Background Jobs Pipeline                                                     |

| ----------------------------- | --------------------------------------------------------------- | ---------------------------------------------------------------------------------------- |

| Primary purpose               | Deliver durable integration events                              | Execute scheduled or queued work                                                         |

| Work identity                 | Persisted integration event                                     | Job payload                                                                              |

| Durable source of work        | Outbox table                                                    | Hangfire job storage and the application's job-enqueueing design                         |

| Business transaction coupling | Outbox record is committed with the originating business state  | Depends on how the job is requested and whether an outbox-backed enqueue request is used |

| Execution entry point         | `OutboxProcessingJob`                                           | Job dispatcher and `JobOrchestrator`                                                     |

| Idempotency                   | Required at the event-handler/effect boundary                   | Required when retries or repeated execution can duplicate effects                        |

| Retry responsibility          | Outbox message policy and, where configured, Hangfire job retry | Hangfire retry policy and applicable job-level behavior                                  |

| Typical work                  | Communicate business facts to consuming modules                 | Expire orders, clean files, reconcile records, run maintenance                           |



\### 11.1 No `JobOrchestrator` Dependency



The outbox processor does not need to route every integration event through the independent-job `JobOrchestrator`.



Its execution semantics are owned by the messaging pipeline:



\* Retrieve durable messages.

\* Claim messages safely.

\* Deserialize event contracts.

\* Dispatch to registered handlers.

\* Coordinate idempotency and delivery outcomes.

\* Retry or quarantine failures.



The two pipelines may share infrastructure such as Hangfire storage, the server, logging, dependency injection, and monitoring without sharing their orchestration abstractions.



\### 11.2 Hangfire Is an Adapter



The use of Hangfire to trigger outbox processing is an infrastructure choice.



If Hangfire is replaced, the trigger mechanism can change while the Application contracts, outbox persistence, event dispatching, and handler responsibilities remain largely intact.



The outbox processing implementation may require adjustments if the replacement changes scheduling, concurrency, cancellation, or execution semantics.



\### 11.3 Important Enqueueing Distinction



The outbox guarantees atomic recording of the integration event. It does not automatically guarantee atomic creation of an unrelated Hangfire job.



For example, if a business transaction commits and the application then separately enqueues an independent Hangfire job, a process crash between those operations can leave the business change committed without the job being enqueued.



If an independent job request must be guaranteed whenever a business transaction commits, that request needs its own durable coordination mechanism, such as an outbox-backed job request.



\---



\## 12. Observability and Operations



Durable asynchronous processing needs operational visibility. A message that remains pending or repeatedly fails should not require someone to inspect database rows manually before discovering the problem.



\### 12.1 Recommended Metrics



Monitor at least:



\* Number of pending outbox messages.

\* Age of the oldest pending message.

\* Successful and failed processing attempts.

\* Retry counts and dead-letter counts.

\* Processing duration.

\* Dispatch throughput.

\* Handler failures grouped by event type.

\* Number of messages currently claimed or processing, if applicable.



\### 12.2 Logging



Logs should include relevant diagnostic identifiers:



\* `EventId`.

\* Outbox message ID.

\* Event type.

\* Handler identifier.

\* Hangfire job ID, where available.

\* Correlation ID, when propagated.

\* Attempt count.

\* Processing outcome.



Avoid logging entire payloads by default, especially when they contain personal or commercially sensitive information.



\### 12.3 Alerts



Alert on conditions that indicate a persistent delivery problem, such as:



\* The oldest pending message exceeds the defined processing-latency threshold.

\* The outbox backlog grows continuously.

\* Dead-letter counts increase unexpectedly.

\* A handler repeatedly fails.

\* Message processing stops while new messages continue to arrive.



Thresholds should reflect the application's actual delivery expectations and workload rather than arbitrary universal values.



\### 12.4 Operational Recovery



The operational guide should define how to:



1\. Identify a failing event or handler.

2\. Inspect safe diagnostic information.

3\. Correct the underlying issue.

4\. Determine whether replay is safe.

5\. Retry or requeue the message.

6\. Verify the resulting business effect.

7\. Prevent a manual replay from bypassing idempotency controls.



Manual replay must use the same event identity and appropriate deduplication rules unless an explicit recovery procedure intentionally creates a new logical operation.



\---



\## 13. Security and Reliability Requirements



The implementation should enforce the following requirements:



\* Integration events must be persisted transactionally with the business state that produces them.

\* The Application layer must not depend on Hangfire.

\* Event contracts must have stable identities and controlled serialization.

\* Message claiming must be safe under the configured worker concurrency.

\* Handler failures must be observable and must not result in false success acknowledgments.

\* Duplicate delivery must be expected.

\* Database-only effects should coordinate business changes and processed-event records transactionally when feasible.

\* External effects must use destination-supported idempotency or another appropriate recovery strategy when duplicate execution is unsafe.

\* Retry behavior must distinguish transient failures from terminal failures.

\* Permanently failing messages must remain diagnosable and recoverable.

\* Event payloads and operational logs must not unnecessarily expose secrets or sensitive personal data.

\* Outbox and processed-event retention must be explicitly managed.

\* Database indexes and query patterns must support pending-message retrieval and processed-event lookup.



The implementation should also account for graceful shutdown and cancellation. Cancellation must not cause a message to be acknowledged as successfully processed when its handler did not complete successfully.



\---



\## 14. Architectural Invariants



The following rules define the intended design:



1\. \*\*Business state and its outbox event are committed atomically.\*\*

2\. \*\*Domain events and integration events have distinct responsibilities.\*\*

3\. \*\*The module that owns a business fact owns its integration-event contract.\*\*

4\. \*\*Consuming modules own their respective integration-event handlers.\*\*

5\. \*\*Integration-event handlers are invoked through the dispatcher, not directly by command handlers.\*\*

6\. \*\*The outbox processor owns delivery coordination and message outcomes.\*\*

7\. \*\*Hangfire triggers the processor but does not own the business meaning of integration events.\*\*

8\. \*\*Integration-event delivery is treated as at-least-once.\*\*

9\. \*\*Idempotency must be enforced at the durable business-effect boundary.\*\*

10\. \*\*A processed-event record must not be treated as an exactly-once guarantee unless its transaction and concurrency semantics support that conclusion.\*\*

11\. \*\*Concurrent workers must not be allowed to claim the same message unsafely.\*\*

12\. \*\*Retry and dead-letter behavior must be explicit and observable.\*\*

13\. \*\*The pipeline remains separate from the Independent Background Jobs Pipeline's `JobOrchestrator`.\*\*

14\. \*\*Shared infrastructure does not imply shared orchestration responsibilities.\*\*

15\. \*\*External side effects must be designed for uncertain outcomes and retries.\*\*



\---



\## 15. Final Summary



The Integration Events Pipeline provides the modular monolith with a durable mechanism for communicating business facts asynchronously.



Its core responsibilities are deliberately focused:



\* The originating business transaction records the integration event in the transactional outbox.

\* The background processor retrieves persisted messages and coordinates delivery.

\* The dispatcher invokes the handlers responsible for consuming each event.

\* Durable idempotency mechanisms protect against duplicate effects where their transaction and concurrency guarantees are sufficient.

\* Retry, dead-letter handling, observability, and operational recovery support reliable processing over time.



The pipeline uses Hangfire as an infrastructure-level trigger while remaining architecturally independent of the Scheduling module's `JobOrchestrator`.



\*\*The central guarantee is durable event recording with retryable, at-least-once delivery—not automatic exactly-once execution.\*\* Correctness depends on atomic outbox writes, safe message claiming, appropriate transaction boundaries, idempotent effects, and explicit failure recovery.



Together with the Independent Background Jobs Pipeline, this design provides two distinct asynchronous execution mechanisms within the same modular monolith, each with a clear purpose and responsibility boundary.



