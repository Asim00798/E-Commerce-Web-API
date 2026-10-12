\# Architectural Work Dispatch Paths and Execution Channels



\*\*Architecture:\*\* DDD Modular Monolith

\*\*Topic:\*\* Execution Channels, Triggers, Business Facts, and Background Jobs



\## Purpose



This document defines the execution channels used in the application and the criteria for selecting each one.



The goal is to ensure that every piece of work is executed through the appropriate mechanism based on its required:



\* Consistency model

\* Durability requirements

\* Business ownership

\* Execution timing



The decision must \*\*never start with technology\*\* (Hangfire, Outbox, Events, etc.).



Instead, always ask:



\*\*What type of work is this?\*\*



\---



\## Core Mental Model: Two Dimensions of Background Work



To avoid confusion, background work is described using \*\*two independent dimensions\*\*:



1\. \*\*What triggered the work?\*\* — The \*trigger\*, such as a domain event, immediate call, future schedule, or recurring schedule.

2\. \*\*Does the work produce a business fact?\*\* — The \*business significance\*. If yes, publish an integration event; if no, perform the work without publishing a business event.



The \*\*publication mechanism\*\* (Outbox) and the \*\*execution pipeline\*\* (`JobOrchestrator`) are shared infrastructure. They do not fundamentally change based on the trigger or business significance.



\### Dimension 1 — Triggers: How Work Enters the System



| Trigger Type                    | Starts When                                                | Implemented By                                                                                                                          |

| ------------------------------- | ---------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |

| \*\*Domain event\*\*                | An aggregate records a domain event.                       | `IDomainEventHandler<T>` (may create an integration event).                                                                             |

| \*\*Immediate (fire-and-forget)\*\* | Application code explicitly requests background execution. | `IJobScheduler.Enqueue(job)` — standalone use is restricted to loss-tolerant work; reliable recurring jobs use their recurring trigger. |

| \*\*Scheduled one-shot\*\*          | Work is intended to execute at a specific future time.     | `IJobScheduler.Schedule(job, futureTime)`.                                                                                              |

| \*\*Recurring schedule\*\*          | Work is triggered according to a cron expression.          | `IRecurringJobTrigger` with the `\[RecurringJob]` attribute; the trigger invokes `Enqueue` internally.                                   |



All triggers except domain events produce \*\*background jobs\*\* that pass through the \*\*Controlled Execution Gateway\*\* (`JobOrchestrator`).



\*\*Important:\*\* Standalone `Enqueue` is not a durable business-work entry point and must not be used for reliable work. Recurring execution is driven by a persisted recurring-job definition, while one-shot critical tasks should use `Schedule` or the Outbox pipeline (Channel 3).



\### Dimension 2 — Business Significance: Does the Outcome Matter to Other Contexts?



| Business Significance          | Meaning                                                                                      | Action                                                                                                  |

| ------------------------------ | -------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |

| \*\*Yes — Business Process Job\*\* | Completion of the work represents a business fact that other bounded contexts must react to. | Call `IOutboxMessageWriter.WriteAsync(integrationEvent)` inside the handler's Unit of Work transaction. |

| \*\*No — Housekeeping Job\*\*      | The work is operational or technical and has no cross-context business meaning.              | Perform the work and return without publishing a business integration event.                            |



The decision is made \*\*inside the handler\*\*:



\* `IDomainEventHandler<T>` for domain-event-triggered work.

\* `IJobHandler<T>` for background jobs.



The \*\*Outbox pipeline\*\*—Outbox table → `OutboxProcessingJob` → idempotent subscribers—is the shared mechanism for publishing business facts, regardless of their origin.



\---



\## Execution Channels



With this mental model established, application work maps to one of the following five channels.



Channels 1–3 are primarily driven by domain operations and business requirements. Channels 4–5 address independently initiated background work and business-fact publication without a naturally occurring domain event.



\### 1. Direct Transactional Execution



\*\*Purpose:\*\* Execute a business operation that requires no additional reactions.



\*\*Trigger:\*\* Command initiated by a user request or another application operation.



\*\*Characteristics:\*\*



\* Synchronous

\* Transactional

\* Immediate

\* Strongly consistent within the transaction



\*\*Flow:\*\*



```text

Command

&#x20;  ↓

CommandHandler

&#x20;  ↓

Aggregate

&#x20;  ↓

SaveChanges

&#x20;  ↓

Commit

```



\*\*When to use:\*\*



\* No additional reactions are required.

\* The operation only modifies the relevant domain state.

\* No other components need to be notified.



\*\*Examples:\*\*



\* Update customer address.

\* Rename category.

\* Change product description.



\### 2. Internal Domain Event Execution



\*\*Purpose:\*\* Execute side effects that must succeed or fail together with the original business transaction.



\*\*Trigger:\*\* A domain event raised by an aggregate.



\*\*Characteristics:\*\*



\* Synchronous

\* Transactional

\* Immediate

\* Strongly consistent within the transaction



\*\*Flow:\*\*



```text

Command

&#x20;  ↓

Aggregate

&#x20;  ↓

Domain Event

&#x20;  ↓

Domain Event Handlers

&#x20;  ↓

Commit

```



\*\*When to use:\*\*



\* The reaction is part of the same business operation.

\* Consistency must be immediate.

\* Failure must roll back the entire transaction.



\*\*Examples:\*\*



\* `OrderPlaced` → Reserve inventory through an appropriate transactional handler.

\* `OrderPlaced` → Update loyalty points when the operation belongs in the same transactional consistency boundary.

\* `OrderPlaced` → Create an internal notification record when it must be committed with the original operation.



\*\*Architectural consideration:\*\* A domain event does not automatically justify synchronous execution. Use this channel only when the reaction genuinely belongs to the same transaction and consistency boundary.



\### 3. Domain Event → Integration Event (Transactional Outbox)



\*\*Purpose:\*\* Communicate business facts reliably to other bounded contexts or external systems.



\*\*Trigger:\*\* A domain event.



\*\*Characteristics:\*\*



\* Asynchronous delivery

\* Durable publication intent through the Outbox

\* Eventually consistent across boundaries

\* Resilient to process failures after the transaction commits



\*\*Flow:\*\*



```text

Command

&#x20;  ↓

Aggregate

&#x20;  ↓

Domain Event

&#x20;  ↓

Domain Event Handler

&#x20;  ↓

Create Integration Event

&#x20;  ↓

Write Outbox Message

&#x20;  ↓

Commit Transaction

&#x20;  ↓

OutboxProcessingJob

&#x20;  ↓

IntegrationEventDispatcher

&#x20;  ↓

Idempotent Integration Event Handler

&#x20;  ↓

IIntegrationEventHandler<T>.HandleAsync()

```



The business state and Outbox message are persisted in the same database transaction. Dispatch happens after the transaction commits.



\*\*When to use:\*\*



\* Another bounded context must be informed.

\* An external system must be notified.

\* The publication intent must survive process crashes.

\* Immediate consistency across the boundary is not required.



\*\*Examples:\*\*



\* `OrderPlaced` → Send a confirmation email through the notification workflow.

\* `OrderPlaced` → Update a search index.

\* `OrderPlaced` → Notify the Shipping context.



\*\*Important:\*\* The Outbox makes the publication intent durable. It does not guarantee that delivery happens exactly once. Consumers must handle duplicate deliveries through idempotent processing.



\### 4. Independent Background Job



\*\*Purpose:\*\* Execute work that is not directly triggered by a domain event.



\*\*Triggers:\*\*



\* \*\*Immediate (fire-and-forget):\*\* For non-critical, loss-tolerant tasks.

\* \*\*Scheduled one-shot:\*\* For work intended to execute at a future time.

\* \*\*Recurring schedule:\*\* For work executed according to a recurring schedule.



All jobs pass through the \*\*Controlled Execution Gateway\*\* (`JobOrchestrator`), which applies the configured execution policies, such as timeout and circuit-breaker policies, and centralized execution logging.



\*\*Characteristics:\*\*



\* Asynchronous

\* Scheduler-managed through Hangfire

\* Optional Outbox publication when the job produces a business fact that other contexts must consume



\*\*Flow:\*\*



```text

IJobScheduler / Recurring Job Trigger

&#x20;  ↓

Hangfire

&#x20;  ↓

HangfireJobDispatcher

&#x20;  ↓

JobOrchestrator

&#x20;  ├── Timeout Policy

&#x20;  ├── Circuit-Breaker Policy

&#x20;  └── Resolve IJobHandler<TJob>

&#x20;         ↓

&#x20;  IJobHandler<TJob>.HandleAsync(...)

&#x20;         ↓

&#x20;  Business or Operational Work

&#x20;         ↓

&#x20;  If a Business Fact Is Produced:

&#x20;  IOutboxMessageWriter.WriteAsync(...)

&#x20;         ↓

&#x20;  Outbox Table

&#x20;         ↓

&#x20;  OutboxProcessingJob

&#x20;         ↓

&#x20;  Integration Event Subscribers

```



\*\*When to use:\*\*



\* Work is scheduled, time-based, or manually triggered.

\* Work does not originate directly from a domain event.

\* The operation can execute independently of the initiating request.



\#### Job Types Within This Channel



| Job Type                                  | Trigger                                      | Business Fact?                                       | Example                                              |

| ----------------------------------------- | -------------------------------------------- | ---------------------------------------------------- | ---------------------------------------------------- |

| \*\*Business Process — Recurring\*\*          | `IRecurringJobTrigger` with `\[RecurringJob]` | Yes — writes to Outbox when publication is required. | Monthly inventory reconciliation.                    |

| \*\*Housekeeping — Recurring\*\*              | `IRecurringJobTrigger` with `\[RecurringJob]` | No.                                                  | Nightly temporary-file cleanup.                      |

| \*\*Business Process — Scheduled One-Shot\*\* | `IJobScheduler.Schedule`                     | Yes — writes to Outbox when publication is required. | Send a feedback request seven days after delivery.   |

| \*\*Housekeeping — Scheduled One-Shot\*\*     | `IJobScheduler.Schedule`                     | No.                                                  | Expire a password-reset record or link.              |

| \*\*Fire-and-Forget — Loss-Tolerant\*\*       | Standalone `IJobScheduler.Enqueue`           | Normally no.                                         | Best-effort cache warming or non-critical telemetry. |



\*\*Important distinctions:\*\*



\* A business process job is not automatically an integration event. It publishes an integration event only when its outcome represents a business fact that other components need to consume.

\* A housekeeping job generally performs technical maintenance without publishing a business event.

\* Standalone `Enqueue` is restricted to work whose loss is acceptable.

\* Critical one-shot jobs require a durable scheduling or dispatch mechanism appropriate to their reliability requirements.



The recurring-job definition and the one-shot `Schedule` mechanism are the intended durable entry points for this channel. Their actual persistence and recovery guarantees depend on the configured Hangfire storage and scheduling implementation.



\### 5. Direct Integration Event Publication from Application Logic



\*\*Purpose:\*\* Publish a business fact originating from application logic or a background job when no suitable domain event naturally represents it.



\*\*Pattern:\*\* Application-Initiated Integration Event.



\*\*Characteristics:\*\*



\* Asynchronous delivery

\* Durable publication intent through the Outbox

\* Eventually consistent across boundaries



\*\*Flow:\*\*



```text

Application Logic

(Job Handler / Application Service)

&#x20;  ↓

Create Integration Event

&#x20;  ↓

IOutboxMessageWriter.WriteAsync(...)

&#x20;  ↓

Commit Outbox Message

&#x20;  ↓

OutboxProcessingJob

&#x20;  ↓

IntegrationEventDispatcher

&#x20;  ↓

Integration Event Handlers

```



The Outbox message must be written within the relevant Unit of Work transaction.



\*\*When to use — only when:\*\*



\* The business fact is not naturally represented by a domain event.

\* A background process must publish a meaningful business outcome.

\* An administrative process must broadcast a completed business operation.



\*\*Examples:\*\*



\* Inventory reconciliation completed → `InventorySyncedIntegrationEvent`.

\* Large data import completed → `ImportCompletedIntegrationEvent`.



\*\*Recommendation:\*\* Prefer the domain-driven path in Channel 3 whenever the business fact naturally arises from an aggregate state change.



Channel 5 exists because not every meaningful business fact originates from an aggregate. It standardizes this less common scenario through the same `IOutboxMessageWriter` used elsewhere.



\---



\## Unified Asynchronous Infrastructure



Hangfire is the shared asynchronous execution platform for the modular monolith. It hosts both Outbox delivery and independent background jobs.



```text

Hangfire

│

├── OutboxProcessingJob

│   └── Integration Event Delivery

│       ├── Channel 3: Domain Event → Integration Event

│       └── Channel 5: Application-Initiated Integration Event

│

├── InventoryReconciliationTrigger

│   └── Business Process Job

│

├── CleanupOldTempFilesTrigger

│   └── Housekeeping Job

│

├── ExpirePendingOrdersTrigger

│   └── Business Process Job

│

├── Scheduled Feedback Requests

│   └── One-Shot Jobs

│

└── Other Operational Jobs

```



\*\*Shared infrastructure does not imply shared architectural purpose.\*\*



Integration events communicate business facts across boundaries. Background jobs execute work asynchronously. They can use the same execution platform while retaining distinct responsibilities.



\---



\## Decision Matrix



| Question                                                                                                  | Channel / Mechanism                                                                                                                 |

| --------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |

| Is this a simple business operation with no additional reactions?                                         | \*\*1. Direct Transactional Execution\*\*                                                                                               |

| Must a reaction succeed or fail with the original transaction?                                            | \*\*2. Internal Domain Event Execution\*\*                                                                                              |

| Must another bounded context or external system be informed reliably about a domain-driven business fact? | \*\*3. Domain Event → Integration Event through the Outbox\*\*                                                                          |

| Is the work scheduled, operational, or independently initiated?                                           | \*\*4. Independent Background Job\*\* — choose recurring, scheduled one-shot, or fire-and-forget according to reliability requirements. |

| Does a background job produce a business fact that other contexts must consume?                           | \*\*Channel 4 with Outbox publication\*\* — write the integration event within the job's Unit of Work transaction.                      |

| Must a business fact be published, but no suitable domain event exists?                                   | \*\*5. Application-Initiated Integration Event\*\* — use the Outbox, sparingly.                                                         |



\### A Practical Selection Rule



Choose the mechanism in this order:



1\. \*\*Determine the consistency boundary.\*\* Must the work succeed or fail with the original transaction?

2\. \*\*Determine the communication requirement.\*\* Must another context or external system learn about a business fact?

3\. \*\*Determine the execution timing.\*\* Must the work run immediately, at a future time, or repeatedly?

4\. \*\*Determine the durability requirement.\*\* Can the work be lost, or must it survive process failures?

5\. \*\*Select the mechanism.\*\* Use the simplest channel that satisfies the requirements.



\---



\## Architectural Principle



\*\*Select execution channels based on business meaning, consistency, and durability requirements—not implementation technology.\*\*



\* \*\*Domain events\*\* represent domain occurrences and enable reactions within the appropriate consistency boundary.

\* \*\*Integration events\*\* communicate business facts across boundaries through durable Outbox publication.

\* \*\*Background jobs\*\* execute independently initiated or time-based work through the controlled execution pipeline.

\* \*\*The Outbox\*\* provides a shared mechanism for persisting publication intent atomically with the relevant business transaction.

\* \*\*Hangfire\*\* provides shared scheduling and background execution infrastructure without determining the business meaning of the work.



The objective is to maintain clear business ownership, predictable failure behavior, and reliable execution without introducing unnecessary distributed infrastructure.



