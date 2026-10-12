\# Business Process Jobs vs. Housekeeping Jobs



\*\*A Complete Guide to Background Job Classification in a DDD Modular Monolith\*\*



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Architecture:\*\* Domain-Driven Design (DDD) / Modular Monolith

\*\*Execution Infrastructure:\*\* Scheduling / Hangfire / Outbox

\*\*Status:\*\* Architectural Reference



\---



\## 1. Overview



All background job payloads execute through the same \*\*Controlled Execution Gateway\*\*, implemented by `JobOrchestrator`.



The gateway centralizes execution orchestration, configured resilience policies, cancellation handling, and structured logging.



However, background jobs differ in their architectural purpose. They can be classified into two broad categories according to whether their outcome represents a business fact that other bounded contexts need to know about.



1\. \*\*Business Process Jobs\*\* — Execute business operations whose outcomes may need to be communicated to other bounded contexts through integration events.

2\. \*\*Housekeeping Jobs\*\* — Perform operational or technical tasks that do not produce a business fact requiring cross-context communication.



The distinction concerns \*\*business responsibility and communication\*\*, not the scheduling mechanism. Both categories use the same execution infrastructure.



> \*\*Important:\*\* Not every business process job must publish an integration event. An event is required when the business outcome represents a fact that other contexts or external consumers need to react to.



\---



\## 2. The Two Job Categories



\### 2.1 Business Process Jobs



Business Process Jobs execute business operations in the background. When an operation produces a business fact that other bounded contexts need to know about, the owning feature publishes an integration event through the Outbox.



Examples of such facts include an order expiring, a payment reconciliation completing, or a product import finishing successfully.



\#### Characteristics



\* Execute a business operation or use case.

\* May change business state owned by a bounded context.

\* Publish integration events when the outcome has cross-context business significance.

\* Persist the integration event through the Outbox in the same database transaction as the associated local business-state changes.

\* Allow interested bounded contexts to react independently and asynchronously.

\* Require appropriate idempotency and concurrency controls because background execution may occur more than once.



\#### Examples



| Job                      | Business Outcome                             | Example Integration Event                          |

| ------------------------ | -------------------------------------------- | -------------------------------------------------- |

| Inventory reconciliation | Inventory discrepancies have been reconciled | `InventoryReconciliationCompletedIntegrationEvent` |

| Order expiration         | An order has expired                         | `OrderExpiredIntegrationEvent`                     |

| Product import           | A product import has completed successfully  | `ProductsImportedIntegrationEvent`                 |

| Payment reconciliation   | A payment discrepancy has been resolved      | `PaymentReconciliationCompletedIntegrationEvent`   |



The events above are illustrative contracts. Their names and ownership should reflect the actual business facts defined by the owning feature.



\#### Example: Order Expiration



Suppose the Ordering feature expires an unpaid order.



Within the appropriate unit of work, it:



1\. Validates that the order is eligible for expiration.

2\. Transitions the order to its expired state.

3\. Records the relevant domain events.

4\. Writes an integration event to the Outbox if other contexts need to react.

5\. Commits the transaction.



The Outbox record and the associated order-state change must be committed atomically.



Afterward, the Outbox processor dispatches the integration event. Interested subscribers, such as Inventory or Shipping, handle the event according to their own responsibilities.



This approach prevents the order from being marked expired in a committed transaction while the corresponding integration event is silently lost because a separate publication attempt failed.



\*\*Architectural rule:\*\* The feature that owns the business fact owns the integration-event contract. The Outbox provides durable delivery; it does not determine the business meaning of the event.



\---



\### 2.2 Housekeeping Jobs



Housekeeping Jobs perform operational or technical work whose outcome does not represent a business fact requiring cross-context communication.



Their purpose is to maintain the system, clean up technical resources, monitor infrastructure, or improve operational efficiency.



\#### Characteristics



\* Perform maintenance, cleanup, monitoring, or technical administration.

\* Do not publish integration events merely to announce routine technical completion.

\* Do not require cross-context business reactions.

\* Still use the shared execution gateway.

\* Still require appropriate error handling, cancellation, observability, and retry safety.



\#### Examples



| Job                              | Purpose                                            | Integration Event Required?                               |

| -------------------------------- | -------------------------------------------------- | --------------------------------------------------------- |

| Temporary file cleanup           | Remove files that are eligible for deletion        | No, unless the outcome has separate business significance |

| Cache warming                    | Preload frequently accessed data                   | No                                                        |

| Log archiving                    | Move old logs to long-term storage                 | No                                                        |

| Technical health monitoring      | Check service availability and record measurements | No                                                        |

| Expired technical-record cleanup | Remove obsolete temporary records                  | No, unless another context must react to a business fact  |



A housekeeping job may update technical records or operational metrics. Those updates do not automatically justify an integration event.



However, classification depends on the actual responsibility, not the job's name. If a task changes a business state that other contexts need to know about, that business outcome must be handled appropriately even if the job also performs cleanup.



\---



\## 3. Decision Rule



When designing a background job, ask:



> \*\*Does the outcome represent a business fact that another bounded context or external consumer needs to know about?\*\*



\### If Yes



Treat the relevant operation as a Business Process Job and publish an integration event when the cross-context communication is required.



The event must be written to the Outbox within the same database transaction as the associated local business-state changes.



```csharp

// Illustrative flow inside the appropriate unit of work.



await \_orderRepository.UpdateAsync(order, cancellationToken);



await \_outboxMessageWriter.WriteAsync(

&#x20;   integrationEvent,

&#x20;   cancellationToken);



await \_unitOfWork.SaveChangesAsync(cancellationToken);

```



The example assumes that the repository changes and Outbox record participate in the same unit of work and database transaction. Actual method names and transaction behavior depend on the application's implementation.



\### If No



Treat the job as Housekeeping.



Perform the operation, record the necessary telemetry, and finish without writing an integration event solely to announce technical completion.



\### An Important Qualification



A job may execute a business operation without requiring cross-context communication.



For example, a job that recalculates an internal value for the owning context may be a business process, but if no other context needs to react to the result, an integration event may be unnecessary.



Therefore, the decision is not simply:



\*\*Business operation = integration event.\*\*



It is:



\*\*Business fact requiring cross-context communication = integration event.\*\*



\---



\## 4. Shared Infrastructure, Different Responsibilities



Both categories use the same job-payload execution pipeline.



```text

Hangfire

&#x20;  |

&#x20;  v

Hangfire Worker

&#x20;  |

&#x20;  v

HangfireJobDispatcher

&#x20;  |

&#x20;  v

JobOrchestrator

&#x20;  |

&#x20;  ├── Resolve IJobHandler<TJob>

&#x20;  ├── Apply configured execution policies

&#x20;  ├── Record structured telemetry

&#x20;  |

&#x20;  v

IJobHandler<TJob>.HandleAsync(...)

&#x20;  |

&#x20;  ├── Business Process Job

&#x20;  |       |

&#x20;  |       ├── Execute business operation

&#x20;  |       ├── Update local business state

&#x20;  |       └── Write Outbox event when required

&#x20;  |

&#x20;  └── Housekeeping Job

&#x20;          |

&#x20;          ├── Execute technical operation

&#x20;          └── Record operational results

```



The principal difference is the responsibility of the handler, not the scheduler.



| Concern                            | Business Process Job                         | Housekeeping Job                                          |

| ---------------------------------- | -------------------------------------------- | --------------------------------------------------------- |

| Uses `JobOrchestrator`             | Yes                                          | Yes                                                       |

| Uses configured execution policies | Yes                                          | Yes                                                       |

| Emits structured telemetry         | Yes                                          | Yes                                                       |

| Changes business state             | May do so                                    | Normally not its primary purpose                          |

| Writes an Outbox event             | When cross-context communication is required | Normally not required                                     |

| Requires idempotency consideration | Yes                                          | Yes, where repeated execution could cause harmful effects |

| Owns its business semantics        | Yes, through the owning feature              | Not applicable to purely technical work                   |



The exact policies applied depend on the configured implementation. A shared execution gateway does not itself guarantee that a timeout cancels underlying work or that a scoped circuit breaker tracks failures across separate executions.



\---



\## 5. Quick Reference



| Aspect                               | Business Process Job                                     | Housekeeping Job                           |

| ------------------------------------ | -------------------------------------------------------- | ------------------------------------------ |

| Primary purpose                      | Execute a business operation                             | Maintain or operate the system             |

| Produces a business outcome          | Often                                                    | Not normally                               |

| Requires cross-context communication | Sometimes                                                | Usually not                                |

| Publishes an integration event       | When required by the business outcome                    | Not for routine technical completion       |

| Uses the shared execution gateway    | Yes                                                      | Yes                                        |

| Requires failure handling            | Yes                                                      | Yes                                        |

| Requires retry-safety analysis       | Yes                                                      | Yes                                        |

| Examples                             | Order expiration, payment reconciliation, product import | File cleanup, cache warming, log archiving |



\---



\## 6. Naming Conventions



Consistent naming makes job responsibilities easier to identify during code review and maintenance.



\### 6.1 Payload, Handler, and Trigger



Use the established payload-handler-trigger pattern.



\*\*Business Process Job\*\*



```text

ReconcileInventoryJob

ReconcileInventoryJobHandler

InventoryReconciliationTrigger

```



\*\*Housekeeping Job\*\*



```text

CleanupTempFilesJob

CleanupTempFilesJobHandler

CleanupTempFilesTrigger

```



A trigger is needed when the job requires recurring scheduling. A job submitted directly by application code does not need a recurring trigger.



\### 6.2 Integration Events



Name integration events after the business facts they represent.



Examples:



```text

InventoryReconciliationCompletedIntegrationEvent

OrderExpiredIntegrationEvent

ProductsImportedIntegrationEvent

PaymentReconciliationCompletedIntegrationEvent

```



Integration events should describe completed facts, not merely announce that a technical method ran.



For example, `OrderExpiredIntegrationEvent` communicates a meaningful business transition. A generic event such as `BackgroundJobCompletedIntegrationEvent` usually exposes an implementation detail rather than a useful business contract.



The event contract belongs to the bounded context that owns the fact. Subscribers implement their own handlers for that contract.



\---



\## 7. Idempotency and Delivery Guarantees



Job classification does not eliminate the need for reliable execution.



Hangfire may execute a job more than once because of retries, worker interruptions, duplicate submissions, or manual reprocessing. Therefore, handlers must be designed with repeated execution in mind.



\### Business Process Jobs



The handler must protect business-state transitions and side effects against duplicate execution where necessary.



For integration events:



1\. The business operation and Outbox record are committed atomically.

2\. The Outbox processor dispatches the event asynchronously.

3\. Subscribers process events using the application's idempotency mechanism.

4\. Repeated delivery must not cause the same business effect to be applied more than once.



An `IdempotentIntegrationEventHandler<T>` decorator can help enforce subscriber idempotency, provided it uses durable deduplication and records successful processing consistently with the subscriber's business changes.



The decorator alone does not guarantee idempotency if the deduplication record and business changes are not coordinated correctly.



\### Housekeeping Jobs



Housekeeping handlers must also account for retries.



For example, deleting a file should tolerate the file already being absent, while a cleanup operation should avoid deleting resources that another process has claimed or renewed.



Idempotency is therefore a general background-execution requirement, not a property exclusive to Business Process Jobs.



\---



\## 8. Why This Classification Matters



\### Architectural Clarity



Developers can distinguish business operations from technical maintenance without introducing a separate scheduler or execution framework.



\### Controlled Event Publication



Integration events are published because a business fact requires communication, not merely because a job has completed.



\### Consistent DDD Boundaries



Integration events express meaningful facts owned by bounded contexts. Technical scheduling details remain inside the execution infrastructure.



\### Reliable Cross-Context Communication



The Outbox provides durable event publication when a business-state change and its corresponding event must be committed atomically.



\### Reusable Execution Infrastructure



Both job categories benefit from the same scheduler adapter, dispatcher, orchestrator, and configured execution policies.



\### Easier Evolution



New business jobs can introduce the integration events required by their use cases without changing the Scheduling infrastructure. New housekeeping jobs can use the same execution pipeline without generating unnecessary business events.



\---



\## 9. Summary



Business Process Jobs and Housekeeping Jobs are useful architectural categories for classifying background work in a DDD modular monolith.



\* \*\*Business Process Jobs\*\* execute business operations. When their outcomes represent facts that other contexts need to know about, they publish integration events through the Outbox.

\* \*\*Housekeeping Jobs\*\* perform operational or technical work and normally do not publish integration events.

\* \*\*Both categories\*\* use the shared `JobOrchestrator` execution path and require appropriate logging, cancellation, failure handling, and retry-safety considerations.

\* \*\*The Outbox\*\* provides durable integration-event delivery; it does not replace the scheduler or determine which events are meaningful.

\* \*\*Idempotency\*\* must be addressed at both the job-execution and integration-event-processing boundaries where repeated execution or delivery could cause duplicate effects.



The guiding principle is:



> \*\*Classify a job by its responsibility. Publish an integration event when a business fact requires cross-context communication—not simply because a background job has finished.\*\*



This keeps the Scheduling subsystem focused on execution, the Outbox focused on durable integration-event delivery, and each bounded context responsible for its own business semantics.



