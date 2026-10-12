\# E-Commerce Background Jobs Pipeline



\## Workflow Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Platform:\*\* .NET 8 / ASP.NET Core / Hangfire

\*\*Version:\*\* 1.0 — Final



\---



\## 1. Purpose



This document describes the end-to-end workflows of the Background Jobs Pipeline, including job creation, automatic registration, recurring scheduling, execution through the `JobOrchestrator`, and failure handling.



The pipeline provides a centralized execution gateway for background work that does not originate from the direct dispatch of an integration event.



It supports two project-specific categories:



\* \*\*Business Process Jobs:\*\* Execute business operations that may produce business facts requiring communication to other modules.

\* \*\*Housekeeping Jobs:\*\* Perform technical or operational work that does not require cross-module business communication.



Both categories use the same execution pipeline. Their difference lies in the work performed and whether the operation produces an integration event—not in their execution infrastructure.



The Integration Events Pipeline remains a separate mechanism for durably recording and dispatching integration events.



\---



\## 2. Workflow Overview



```text

&#x20;                 JOB INITIATION

&#x20;                       |

&#x20;         +-------------+-------------+

&#x20;         |                           |

&#x20;         v                           v

&#x20;  Application Request         Recurring Trigger

&#x20;         |                           |

&#x20;         |                    Automatically Discovered

&#x20;         |                           |

&#x20;         |                    Hangfire Schedules Trigger

&#x20;         |                           |

&#x20;         |                    Trigger Invoked by Hangfire

&#x20;         |                           |

&#x20;         |                    Enqueues Job Payload

&#x20;         |                           |

&#x20;         +-------------+-------------+

&#x20;                       |

&#x20;                       v

&#x20;               IJobScheduler

&#x20;                       |

&#x20;                       v

&#x20;             HangfireJobScheduler

&#x20;                       |

&#x20;                       v

&#x20;                Hangfire Storage

&#x20;                       |

&#x20;                       v

&#x20;                Hangfire Worker

&#x20;                       |

&#x20;                       v

&#x20;            HangfireJobDispatcher

&#x20;                       |

&#x20;                       v

&#x20;               JobExecutionContext

&#x20;                       |

&#x20;                       v

&#x20;               IJobOrchestrator

&#x20;                       |

&#x20;                       v

&#x20;                 JobOrchestrator

&#x20;                       |

&#x20;                       v

&#x20;             Resolve Job Handler

&#x20;                       |

&#x20;                       v

&#x20;            Compose Execution Policies

&#x20;                       |

&#x20;                       v

&#x20;                IJobHandler<TJob>

&#x20;                       |

&#x20;              +--------+--------+

&#x20;              |                 |

&#x20;              v                 v

&#x20;      Business Process      Housekeeping

&#x20;              |                 |

&#x20;              v                 v

&#x20;      Business Operation   Technical Operation

&#x20;              |

&#x20;              v

&#x20;      Business State Change

&#x20;              |

&#x20;              v

&#x20;      Integration Event Required?

&#x20;              |

&#x20;         +----+----+

&#x20;         |         |

&#x20;        Yes        No

&#x20;         |         |

&#x20;         v         v

&#x20;    Write Outbox   Complete

&#x20;    in Same UoW

&#x20;         |

&#x20;         v

&#x20;    Commit Business

&#x20;    State + Outbox

&#x20;         |

&#x20;         v

&#x20;      Complete

```



\*\*Important:\*\* A Business Process Job does not necessarily publish an integration event simply because it belongs to that category. An event is written when the operation produces a business fact that other modules need to consume. The event and any corresponding business changes must be committed atomically when they share the same transaction boundary.



\---



\## 3. Workflow A — Adding a New Background Job



Every job follows the same structural convention.



\### Step 1 — Define the Job Payload



Create a payload class implementing `IJob`.



The payload represents the work to be performed and should contain only the information required to execute it.



Prefer identifiers over serialized domain aggregates or unnecessary personal data.



\### Step 2 — Implement the Handler



Create a handler implementing `IJobHandler<TJob>`.



The handler contains the application or maintenance operation to execute. It should not call Hangfire APIs directly.



\### Step 3 — Decide Whether a Recurring Trigger Is Required



If the job must run on a recurring schedule, create an `IRecurringJobTrigger` implementation and declare its schedule using the project's `RecurringJobAttribute`.



If the job is initiated by an application request or another workflow, a recurring trigger may not be necessary.



\### Step 4 — Automatic Discovery



The configured assembly-scanning mechanisms discover and register applicable handlers and recurring triggers.



The `RecurringJobBootstrapper` discovers recurring triggers and registers their schedules with Hangfire through `IRecurringJobManager`.



\### Step 5 — Execute Through the Gateway



When the job is enqueued or triggered, the Hangfire dispatcher passes the payload and execution context to `IJobOrchestrator`.



The orchestrator resolves the handler, applies the configured policies, and executes the operation.



\### Workflow



```text

Create Job Payload

&#x20;       |

&#x20;       v

Implement IJobHandler<TJob>

&#x20;       |

&#x20;       v

Recurring Schedule Required?

&#x20;       |

&#x20;  +----+----+

&#x20;  |         |

&#x20; Yes        No

&#x20;  |         |

&#x20;  v         |

Implement    |

IRecurring   |

JobTrigger   |

&#x20;  |         |

&#x20;  v         |

Add RecurringJobAttribute

&#x20;  |         |

&#x20;  +----+----+

&#x20;       |

&#x20;       v

Assembly Scanning

&#x20;       |

&#x20;       v

Handler and Trigger Registration

&#x20;       |

&#x20;       v

Recurring Trigger Registered at Startup

&#x20;       |

&#x20;       v

Job Ready for Execution

```



\*\*Expected outcome:\*\* Adding a job requires no per-job manual DI registration or handwritten `RecurringJob.AddOrUpdate` call, provided the relevant assemblies are included in the configured scanning and bootstrapper discovery.



\---



\## 4. Workflow B — Enqueuing an Immediate Job



This workflow applies when Application code requests background execution without waiting for a recurring schedule.



\### Step 1 — Request Background Execution



Application code calls:



`IJobScheduler.Enqueue(job)`



The Application layer depends on the scheduling abstraction rather than Hangfire.



\### Step 2 — Persist the Job Request



`HangfireJobScheduler` adapts the scheduling request to Hangfire's enqueueing mechanism.



Hangfire persists the job request in its configured storage.



\### Step 3 — Worker Retrieves the Job



A Hangfire worker retrieves the job when a worker is available and invokes `HangfireJobDispatcher`.



\### Step 4 — Build the Execution Context



The dispatcher constructs a `JobExecutionContext` containing the runtime metadata available from Hangfire and the application.



Depending on the implementation, this may include:



\* Job ID.

\* Execution attempt.

\* Correlation ID.

\* Enqueue timestamp.

\* Cancellation token or shutdown information.



These values must reflect the actual metadata available. For example, the Hangfire job ID is not automatically the original HTTP request's correlation ID.



\### Step 5 — Enter the Execution Gateway



The dispatcher calls:



`IJobOrchestrator.ExecuteAsync(job, context)`



The orchestrator resolves the handler and runs it through the configured execution pipeline.



\### Workflow



```text

Application Requests Job

&#x20;         |

&#x20;         v

IJobScheduler.Enqueue(job)

&#x20;         |

&#x20;         v

HangfireJobScheduler

&#x20;         |

&#x20;         v

Hangfire Persists Job

&#x20;         |

&#x20;         v

Hangfire Worker Retrieves Job

&#x20;         |

&#x20;         v

HangfireJobDispatcher

&#x20;         |

&#x20;         v

Build JobExecutionContext

&#x20;         |

&#x20;         v

IJobOrchestrator.ExecuteAsync(...)

&#x20;         |

&#x20;         v

JobOrchestrator

&#x20;         |

&#x20;         v

Resolve Handler + Apply Policies

&#x20;         |

&#x20;         v

Execute Job Handler

```



\*\*Expected outcome:\*\* The caller requests background work without directly depending on Hangfire's APIs or executing the handler inline.



\---



\## 5. Workflow C — Executing a Recurring Job



Recurring jobs use the same execution gateway as immediately enqueued jobs.



The recurring trigger determines when work should be requested. The job handler performs the actual operation.



\### Step 1 — Discover the Trigger



During application startup, `RecurringJobBootstrapper` scans the configured assemblies for `IRecurringJobTrigger` implementations decorated with `RecurringJobAttribute`.



\### Step 2 — Register the Schedule



The bootstrapper reads the recurring job ID and cron expression and registers the trigger with Hangfire through `IRecurringJobManager`.



This avoids relying on the static `RecurringJob` API during service registration, when Hangfire's global storage may not yet be initialized.



\### Step 3 — Hangfire Invokes the Trigger



When the schedule becomes due, Hangfire invokes the registered trigger.



The trigger should remain thin: its responsibility is to request execution of the corresponding job payload.



\### Step 4 — Enqueue the Actual Job



The trigger calls the scheduling abstraction to enqueue the job payload.



The payload then follows the normal dispatcher and orchestrator workflow.



\### Step 5 — Execute the Handler



The orchestrator resolves the handler and applies the configured execution policies.



\### Workflow



```text

Application Startup

&#x20;       |

&#x20;       v

RecurringJobBootstrapper

&#x20;       |

&#x20;       v

Discover IRecurringJobTrigger

&#x20;       |

&#x20;       v

Read RecurringJobAttribute

&#x20;       |

&#x20;       v

Register Schedule with Hangfire

&#x20;       |

&#x20;       v

Schedule Becomes Due

&#x20;       |

&#x20;       v

Hangfire Invokes Trigger

&#x20;       |

&#x20;       v

Trigger Enqueues Job Payload

&#x20;       |

&#x20;       v

Hangfire Stores Job

&#x20;       |

&#x20;       v

Hangfire Worker Executes Dispatcher

&#x20;       |

&#x20;       v

JobOrchestrator

&#x20;       |

&#x20;       v

Handler Executes Through Policies

```



\*\*Important distinction:\*\* The recurring trigger is itself a Hangfire-invoked callback. It does not automatically pass through `JobOrchestrator` merely because the payload it enqueues does. The actual job payload follows the gateway.



\---



\## 6. Workflow D — Job Execution Through the Gateway



The `JobOrchestrator` is the common execution entry point for job payloads.



\### Step 1 — Resolve the Handler



The orchestrator determines the handler corresponding to the job's payload type and resolves it from dependency injection.



\### Step 2 — Build the Execution Pipeline



The orchestrator composes the registered `IJobPolicy` implementations around the handler.



With the described policy registration and composition, the intended order is:



```text

TimeoutPolicy

&#x20;     |

&#x20;     v

CircuitBreakerPolicy

&#x20;     |

&#x20;     v

IJobHandler<TJob>

```



The outer policy controls the execution of the inner policy chain.



\### Step 3 — Execute the Handler



The handler receives the payload and cancellation token and performs the relevant operation.



The orchestrator provides a consistent place for execution logging and any configured metrics, tracing, or correlation behavior.



\### Step 4 — Handle the Outcome



If the handler completes successfully, the orchestrator records the successful outcome.



If the handler throws an exception, the orchestrator logs the failure and propagates it to Hangfire so the configured retry behavior can apply.



\### Workflow



```text

HangfireJobDispatcher

&#x20;         |

&#x20;         v

IJobOrchestrator.ExecuteAsync(...)

&#x20;         |

&#x20;         v

Resolve IJobHandler<TJob>

&#x20;         |

&#x20;         v

Build Policy Chain

&#x20;         |

&#x20;         v

TimeoutPolicy

&#x20;         |

&#x20;         v

CircuitBreakerPolicy

&#x20;         |

&#x20;         v

Handler.HandleAsync(job, ct)

&#x20;         |

&#x20;    +----+----+

&#x20;    |         |

&#x20; Success    Exception

&#x20;    |         |

&#x20;    v         v

Log Success  Log Failure

&#x20;    |         |

&#x20;    v         v

&#x20;Return     Propagate Failure

&#x20;              |

&#x20;              v

&#x20;         Hangfire Retry

&#x20;         if configured

```



Hangfire owns the configured job retry behavior. The handler should not independently reproduce the same retry loop unless a separate, explicitly justified retry policy is required.



\---



\## 7. Workflow E — Business Process Job



A Business Process Job performs a business operation that may produce a fact other modules need to know about.



Examples may include expiring eligible orders, reconciling inventory, or completing a catalog import.



The operation uses the shared execution gateway. If it produces a cross-module business fact, the integration event must be persisted through the Transactional Outbox.



\### Step 1 — Execute the Business Operation



The handler loads the necessary data and performs the business operation using the owning module's application services and domain model.



\### Step 2 — Apply Business Rules



The operation validates its preconditions and applies the relevant business rules.



For example, an order-expiration operation must verify that the order is still eligible for expiration rather than blindly changing every order that was previously selected.



\### Step 3 — Determine Whether a Business Fact Occurred



The operation determines whether it produced a business fact that other modules must consume.



A business job may complete without producing a new integration event—for example, if no eligible orders were found.



\### Step 4 — Write the Integration Event When Required



When a business fact must be communicated, the operation or appropriate event-handling workflow creates the integration event and calls `IOutboxMessageWriter.WriteAsync()`.



The outbox record must be written within the same Unit of Work transaction as the corresponding business state change.



\### Step 5 — Commit the Transaction



The business state and outbox message are committed atomically.



If the transaction fails, neither should be committed.



\### Step 6 — Complete the Job



Once the business transaction has completed successfully, the handler returns and the orchestrator records successful job execution.



The event is delivered later by the separate Integration Events Pipeline.



\### Workflow



```text

Business Process Job Starts

&#x20;         |

&#x20;         v

Load Business Data

&#x20;         |

&#x20;         v

Validate Business Rules

&#x20;         |

&#x20;         v

Execute Business Operation

&#x20;         |

&#x20;         v

Did a Relevant Business Fact Occur?

&#x20;         |

&#x20;    +----+----+

&#x20;    |         |

&#x20;   Yes        No

&#x20;    |         |

&#x20;    v         v

Create        Commit Business

Integration   Changes, If Any

Event         |

&#x20;    |         |

&#x20;    v         |

Write Outbox   |

Message        |

&#x20;    |         |

&#x20;    +----+----+

&#x20;         |

&#x20;         v

Commit Unit of Work

(Business State + Outbox, When Applicable)

&#x20;         |

&#x20;         v

Handler Completes Successfully

&#x20;         |

&#x20;         v

JobOrchestrator Records Success

&#x20;         |

&#x20;         v

Integration Events Pipeline

Processes Outbox Message Later

```



\### Transaction Boundary Requirement



Do not assume that writing an outbox message is sufficient to make it atomic with the business operation. Both writes must participate in the same database transaction.



Also, do not enqueue an unrelated Hangfire job after committing the business transaction if that enqueue must be guaranteed. A crash between the commit and enqueue can lose the job request. Use durable coordination, such as an outbox-backed request, when atomicity is required.



\---



\## 8. Workflow F — Housekeeping Job



A Housekeeping Job performs maintenance or technical work that does not require publishing a cross-module business fact.



Examples include:



\* Cleaning up expired files.

\* Removing obsolete temporary records.

\* Warming a cache.

\* Rotating or managing application logs.

\* Performing scheduled technical maintenance.



Health checks should be classified according to how they are executed: request-time or host-lifecycle health checks are not necessarily background jobs.



\### Step 1 — Start the Job



The job is enqueued directly or through a recurring trigger.



\### Step 2 — Resolve the Handler



The dispatcher invokes the orchestrator, which resolves the corresponding handler.



\### Step 3 — Execute Maintenance



The handler performs the maintenance operation using the appropriate infrastructure or application abstractions.



\### Step 4 — Handle the Outcome



If the operation succeeds, the handler returns normally.



If it fails, the exception is propagated so the configured retry policy can handle the failure.



Housekeeping work does not publish an integration event merely because it ran. An event should be emitted only if the operation produces a business fact that other modules need to consume.



\### Workflow



```text

Housekeeping Job Starts

&#x20;         |

&#x20;         v

Load Required Resources

&#x20;         |

&#x20;         v

Execute Maintenance Operation

&#x20;         |

&#x20;         v

Operation Successful?

&#x20;         |

&#x20;    +----+----+

&#x20;    |         |

&#x20;   Yes        No

&#x20;    |         |

&#x20;    v         v

Return       Throw Exception

&#x20;    |         |

&#x20;    v         v

Log Success  Log Failure

&#x20;    |         |

&#x20;    v         v

Complete     Hangfire Retry

&#x20;            if configured

```



\*\*Expected outcome:\*\* Maintenance operations use the same execution and observability infrastructure without unnecessarily involving the Integration Events Pipeline.



\---



\## 9. Workflow G — Timeout and Failure Handling



\### 9.1 Timeout Behavior



The `TimeoutPolicy` bounds how long the gateway waits for the inner execution pipeline.



However, if the policy uses `Task.WaitAsync` without propagating a linked cancellation token to the handler and its downstream operations, timing out does not forcibly stop the underlying work.



Consequently:



1\. The timeout expires.

2\. The policy reports a timeout failure.

3\. The failure propagates to Hangfire.

4\. Hangfire may retry the job.

5\. The original handler may still be running.



This creates a risk of overlapping executions if the handler is not cancellation-aware and idempotent.



The handler and downstream operations should accept and honor cancellation tokens where supported. Even then, cancellation is cooperative rather than a universal guarantee that all external work has stopped.



\### 9.2 Failure and Retry Workflow



```text

Job Handler Executes

&#x20;       |

&#x20;       v

Execution Exceeds Timeout?

&#x20;       |

&#x20;  +----+----+

&#x20;  |         |

&#x20;  No        Yes

&#x20;  |         |

&#x20;  v         v

Continue   Timeout Failure

&#x20;  |         |

&#x20;  |         v

&#x20;  |      Propagate Exception

&#x20;  |         |

&#x20;  +----+----+

&#x20;       |

&#x20;       v

Handler or Policy Throws?

&#x20;       |

&#x20;  +----+----+

&#x20;  |         |

&#x20;  No        Yes

&#x20;  |         |

&#x20;  v         v

Log Success  Log Failure

&#x20;  |         |

&#x20;  v         v

Complete    Hangfire Retry

&#x20;           if configured

```



\### 9.3 Circuit Breaker Behavior



The intended circuit-breaker workflow is:



```text

Job Execution

&#x20;     |

&#x20;     v

Circuit Breaker Evaluates State

&#x20;     |

&#x20;+----+----+

&#x20;|         |

Closed    Open

&#x20;|         |

&#x20;v         v

Execute   Reject Execution

Handler   Until Recovery

&#x20;|

&#x20;v

Observe Outcome

&#x20;|

&#x20;v

Update Failure State

```



\*\*Current implementation caveat:\*\* If `CircuitBreakerPolicy` is registered as a scoped service, each job execution receives a fresh instance. Its failure counter therefore does not accumulate across separate executions.



Under that configuration, it does not provide a functioning cross-execution circuit breaker. It may still produce useful failure logs, but it cannot reliably stop repeated calls to an unhealthy dependency.



A real cross-execution circuit breaker requires state that survives individual job scopes, with suitable concurrency control and, where applicable, shared state across application instances.



Until that behavior is implemented, Hangfire's retry limits and backoff are the actual configured protection against repeated job failures—not a substitute for every circuit-breaker use case.



\---



\## 10. Workflow H — Outbox Jobs Within the Shared Gateway



The Outbox processing and dead-letter monitoring jobs are feature-local jobs that participate in the Background Jobs Pipeline.



Their payloads, handlers, and recurring triggers remain colocated with the Outbox feature, while execution uses the shared Scheduling infrastructure.



\### 10.1 Outbox Processing Job



The Outbox processing job invokes the Outbox subsystem to retrieve and dispatch persisted integration events.



Its handler does not need to implement the integration-event delivery mechanism itself; it coordinates the existing outbox-processing service.



```text

RecurringJobBootstrapper

&#x20;         |

&#x20;         v

OutboxProcessingTrigger Scheduled

&#x20;         |

&#x20;         v

Hangfire Invokes Trigger

&#x20;         |

&#x20;         v

Trigger Enqueues Outbox Job Payload

&#x20;         |

&#x20;         v

HangfireJobDispatcher

&#x20;         |

&#x20;         v

JobOrchestrator

&#x20;         |

&#x20;         v

OutboxProcessingJobHandler

&#x20;         |

&#x20;         v

Outbox Processing Service

&#x20;         |

&#x20;         v

Retrieve Pending Outbox Messages

&#x20;         |

&#x20;         v

Dispatch Each Message

&#x20;         |

&#x20;         v

Update Processing Outcomes

```



The Outbox processing job itself does not replace the Transactional Outbox or the Integration Events Pipeline. It is the scheduled mechanism that initiates processing of persisted messages.



\### 10.2 Dead-Letter Monitor Job



The dead-letter monitor periodically checks for messages that require operational attention.



```text

DeadLetterMonitorTrigger

&#x20;         |

&#x20;         v

Enqueue Job Payload

&#x20;         |

&#x20;         v

JobOrchestrator

&#x20;         |

&#x20;         v

DeadLetterMonitorJobHandler

&#x20;         |

&#x20;         v

Inspect Dead-Letter State

&#x20;         |

&#x20;         v

Evaluate Alerting Conditions

&#x20;         |

&#x20;         v

Emit Operational Diagnostics

```



The monitor should not silently discard failed messages or automatically replay every dead-lettered event without considering the cause and duplicate-effect risks.



\---



\## 11. Workflow I — Automatic Registration and Scheduling



The pipeline uses assembly scanning and recurring-trigger discovery to minimize manual registration.



\### Application Startup



1\. Application services are registered.

2\. Configured Application assemblies are scanned for job handlers and other applicable contracts.

3\. Scheduling Infrastructure registers its core services and Infrastructure-side implementations.

4\. Configured assemblies are scanned for Infrastructure-side handlers and recurring triggers.

5\. The host starts.

6\. `RecurringJobBootstrapper` discovers recurring triggers.

7\. The bootstrapper reads each trigger's `RecurringJobAttribute`.

8\. Each schedule is registered with Hangfire through `IRecurringJobManager`.



\### Workflow



```text

Configure Service Collection

&#x20;         |

&#x20;         v

Register Application Services

&#x20;         |

&#x20;         v

Scan Application Assemblies

&#x20;         |

&#x20;         v

Register Scheduling Infrastructure

&#x20;         |

&#x20;         v

Scan Infrastructure Assemblies

&#x20;         |

&#x20;         v

Build and Start Host

&#x20;         |

&#x20;         v

RecurringJobBootstrapper Runs

&#x20;         |

&#x20;         v

Discover Trigger Implementations

&#x20;         |

&#x20;         v

Read RecurringJobAttribute

&#x20;         |

&#x20;         v

Register Recurring Schedules

&#x20;         |

&#x20;         v

Application Ready

```



Automatic discovery depends on correct assembly coverage, attribute placement, service registration, and successful startup.



A class that is not included in the configured scanning or discovery process will not become registered merely because it implements the correct interface.



\---



\## 12. Failure Scenarios and Expected Recovery



| Scenario                                                                        | Expected behavior                                                                                                                    |

| ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ |

| Hangfire storage is temporarily unavailable                                     | Enqueueing or execution fails according to the adapter's behavior; the caller or operational process must handle the failure.        |

| Handler throws an exception                                                     | The failure is logged and propagated to Hangfire for configured retry handling.                                                      |

| Handler times out                                                               | The job fails from the gateway's perspective; underlying work may continue unless cancellation is propagated and honored.            |

| Application restarts during processing                                          | Recovery depends on Hangfire storage, worker shutdown behavior, retry configuration, and the job's idempotency.                      |

| Recurring trigger fails                                                         | Hangfire handles the trigger execution according to its configuration; the trigger's enqueue operation may need retry-safe behavior. |

| Handler is missing or cannot be resolved                                        | Execution fails and should be observable; the job must not be reported as successful.                                                |

| Business transaction rolls back                                                 | The corresponding business changes and outbox message should both roll back when they share the same transaction.                    |

| Job enqueues a second job after a database commit and crashes before enqueueing | The second job may never be created; use durable coordination if the enqueue must be guaranteed.                                     |

| Retried job repeats a business operation                                        | The handler must enforce suitable idempotency or concurrency safeguards.                                                             |

| Circuit breaker is scoped per execution                                         | Failure state resets between executions; cross-execution protection is not provided.                                                 |



\---



\## 13. Workflow Invariants



The following rules must remain true as the pipeline evolves:



1\. All background job payloads execute through `IJobOrchestrator` when they belong to this gateway.

2\. Application code depends on `IJobScheduler`, not directly on Hangfire APIs.

3\. Handlers contain the work to execute and do not register or schedule themselves through Hangfire.

4\. Recurring triggers are thin scheduling adapters.

5\. The recurring bootstrapper discovers and registers configured trigger implementations automatically.

6\. Hangfire owns job retry behavior according to its configured policies.

7\. The orchestrator applies the configured execution policies consistently.

8\. Timeout handling must not be documented as forcibly cancelling handler execution unless the implementation actually guarantees that behavior.

9\. A scoped circuit breaker must not be described as retaining failure state across separate executions.

10\. Business state and its corresponding outbox event must be committed atomically.

11\. Not every Business Process Job must publish an integration event; an event is required when a relevant business fact must be communicated.

12\. Housekeeping Jobs must not publish integration events merely to report routine technical activity.

13\. Jobs that may be retried must be designed to tolerate repeated execution.

14\. The Outbox processing job uses the Background Jobs Pipeline to initiate processing but does not replace the Integration Events Pipeline.

15\. Automatic registration works only for assemblies and implementations included in the configured discovery process.



\---



\## 14. Final Summary



The Background Jobs Pipeline provides a common, policy-driven execution workflow for recurring and on-demand background jobs.



The workflow begins when a job is enqueued or a recurring trigger requests execution. Hangfire persists and executes the job, `HangfireJobDispatcher` constructs the execution context, and `JobOrchestrator` resolves the handler and applies the configured policies.



Business Process Jobs execute business operations and write integration events to the Outbox when cross-module communication is required. Housekeeping Jobs perform operational work without unnecessary business-event publication.



Feature-local jobs, including Outbox processing and dead-letter monitoring, use the same gateway while remaining owned by the features they serve.



The design centralizes execution behavior, keeps Hangfire behind Infrastructure abstractions, and separates job execution from durable integration-event delivery.



\*\*The essential distinction is that the Background Jobs Pipeline controls how a job executes, while the Integration Events Pipeline controls how a persisted business fact is delivered and processed asynchronously.\*\*



