# Post-Commit Processing

**E-Commerce Modular Monolith**

**Technical Capability — Communication**

**Application Abstraction:** `IPostCommitProcessor`

**Status:** Implemented

---

## 1. Overview

`IPostCommitProcessor` is an Application-layer abstraction for registering callbacks that execute **after the current Unit of Work transaction commits successfully**.

It allows best-effort side effects—such as SignalR hints, cache invalidation, and ephemeral observability signals—to run only after the corresponding database changes become durable.

The caller does not need to know whether the Unit of Work uses automatic or manual transaction management. It registers a callback, and the Unit of Work invokes the processor at the appropriate commit boundary.

### Core Characteristics

* **Transaction-aware:** Callbacks execute only after a successful commit.
* **In-memory:** Callbacks are not persisted and do not survive process termination.
* **Scoped:** Each request or dependency-injection scope has its own callback queue.
* **Ordered:** Callbacks execute in the order in which they were enqueued.
* **Awaited:** The Unit of Work awaits callback processing before its commit-related method returns.
* **Best-effort:** Callback failures do not undo committed database changes and are not reported as business-transaction failures.

**`IPostCommitProcessor` is not a durable queue or a delivery guarantee.** If an effect must survive a process crash, use a durable mechanism such as the transactional Outbox.

---

## 2. The Contract

### 2.1 Interface

```csharp
public interface IPostCommitProcessor
{
    void Enqueue(
        Func<IServiceProvider, CancellationToken, Task> callback);

    Task InvokeAsync(
        IServiceProvider serviceProvider,
        CancellationToken ct);
}
```

### 2.2 Responsibilities

| Member        | Responsibility                                                                                   |
| ------------- | ------------------------------------------------------------------------------------------------ |
| `Enqueue`     | Registers a callback for execution after the current transaction commits.                        |
| `InvokeAsync` | Executes the queued callbacks. Called by the Unit of Work, not directly by application handlers. |

The callback receives:

* `IServiceProvider` — the appropriate dependency-injection scope used to resolve services required by the callback.
* `CancellationToken` — the cancellation token supplied by the Unit of Work when invoking the processor.

A callback can resolve services lazily instead of capturing their instances when it is registered.

For example:

```csharp
_postCommitProcessor.Enqueue(async (serviceProvider, ct) =>
{
    var publisher =
        serviceProvider.GetRequiredService<IRealtimeEventPublisher>();

    await publisher.PublishAsync(message, ct);
});
```

The callback is registered while the transaction is active. Its body executes later, after a successful commit.

**Important:** The callback must not execute until the relevant transaction has committed. The processor's contract alone does not enforce this; the Unit of Work is responsible for invoking it at the correct time.

---

## 3. Integration with the Unit of Work

The Unit of Work determines when the database transaction commits and invokes `IPostCommitProcessor` immediately afterward.

The precise invocation point depends on the transaction mode.

### 3.1 Automatic Transaction Mode

In automatic mode, `SaveChangesAsync` owns the transaction.

The processing sequence is:

```text
SaveChangesAsync
    |
    v
Determine transaction ownership
    |
    v
Open transaction
    |
    v
Collect domain events
    |
    v
Save #1: Persist business changes
    |
    v
Dispatch domain events
    |
    | Domain-event handlers may enqueue callbacks
    v
Clear dispatched domain events
    |
    v
Save #2: Persist handler changes and Outbox records
    |
    v
Commit transaction
    |
    v
Invoke post-commit callbacks
    |
    v
Return from SaveChangesAsync
```

**Guarantee:** When `SaveChangesAsync` returns, the transaction has committed and the registered callbacks have finished executing, subject to the processor's failure-handling policy.

A handler can register a callback without knowing that the Unit of Work will commit automatically.

### 3.2 Manual Transaction Mode

In manual mode, the caller explicitly begins and commits the transaction.

The processing sequence is:

```text
Command Handler
    |
    v
BeginTransactionAsync
    |
    v
Perform business operations
    |
    v
SaveChangesAsync
    |
    +----> Persist business changes
    |
    +----> Dispatch domain events
    |
    +----> Persist handler changes and Outbox records
    |
    +----> Do not commit
    |
    +----> Do not invoke post-commit callbacks
    |
    v
CommitTransactionAsync
    |
    v
Commit transaction
    |
    v
Invoke post-commit callbacks
    |
    v
Return from CommitTransactionAsync
```

**Guarantee:** In manual mode, `SaveChangesAsync` does not trigger post-commit callbacks while the explicitly managed transaction remains active. Callbacks run when `CommitTransactionAsync` successfully commits and invokes the processor.

### 3.3 Unified Guarantee

Both modes follow the same essential sequence:

```text
Persist business changes
        |
        v
Dispatch domain events
        |
        v
Persist resulting changes and Outbox records
        |
        v
COMMIT
        |
        v
Execute post-commit callbacks
```

The difference is where the commit occurs:

* **Automatic mode:** Inside `SaveChangesAsync`.
* **Manual mode:** Inside `CommitTransactionAsync`.

In both modes, callbacks execute after the commit—not before it.

If the transaction fails to commit, the post-commit callbacks must not be invoked for that transaction.

---

## 4. When to Use It

Use `IPostCommitProcessor` when a side effect satisfies all three conditions:

1. **It must happen after the transaction commits.** Running it earlier could expose state that is subsequently rolled back.
2. **It must not run when the transaction rolls back.** Its execution depends on the successful persistence of the associated changes.
3. **It is acceptable to lose the effect.** If the process terminates before execution, no automatic recovery is provided.

Typical use cases include:

* Publishing ephemeral SignalR hints.
* Invalidating a cache after a manually managed transaction commits.
* Clearing short-lived in-memory projections or memoized data.
* Emitting best-effort metrics or observability signals associated with a successful write.

If losing the effect is unacceptable, use a durable mechanism instead.

---

## 5. When Not to Use It

| Requirement                                                      | Appropriate mechanism                                                 |
| ---------------------------------------------------------------- | --------------------------------------------------------------------- |
| Durable Email, SMS, or Push delivery                             | Transactional Outbox and integration events                           |
| A business fact that other modules must process reliably         | Transactional Outbox and integration events                           |
| A long-running background operation                              | `IJobScheduler` and the Scheduling capability                         |
| An effect that must survive process termination                  | Outbox, durable job, or another durable persistence mechanism         |
| A change that must be atomic with business state                 | The same database transaction                                         |
| An operation already running outside a Unit of Work transaction  | Execute it directly if appropriate, or use another suitable mechanism |
| An effect that requires durable retries and operational recovery | Outbox or durable job processing                                      |

Post-commit callbacks are for fast, best-effort work—not a substitute for durable messaging or background-job infrastructure.

---

## 6. Canonical Use Cases

### 6.1 Publishing a SignalR Hint

The real-time notification workflow is a canonical use case.

A domain-event handler persists an in-app notification and registers a callback to publish a SignalR hint after the transaction commits.

```csharp
await _userNotificationRepository.AddAsync(
    notification,
    cancellationToken);

_postCommitProcessor.Enqueue(async (serviceProvider, ct) =>
{
    var publisher =
        serviceProvider.GetRequiredService<IRealtimeEventPublisher>();

    await publisher.PublishAsync(
        new RealTimeMessage
        {
            UserId = customerId,
            Method = RealTimeEvents.NewNotification,
            Payload = new { type = "OrderPlaced" }
        },
        ct);
});
```

The example illustrates the intended flow; adapt the repository and DTO signatures to the actual implementation.

#### Why This Is Correct

* The in-app notification is persisted within the business transaction.
* The SignalR hint is published only after that transaction commits.
* A rollback prevents the notification from becoming durable and prevents its callback from running.
* If the process crashes after commit but before callback execution, the notification remains available through the REST API.
* A missed hint does not compromise the underlying business operation.

SignalR is an acceleration mechanism. The database remains authoritative.

See the [Real-Time Communication](../realtime/README.md) documentation for the complete SignalR workflow.

### 6.2 Cache Invalidation in Manual-Mode Handlers

Consider a command that updates data and then invalidates a cache.

In manual transaction mode, calling the cache directly after `SaveChangesAsync` is unsafe because the transaction has not necessarily committed.

Instead:

```csharp
await _unitOfWork.BeginTransactionAsync(ct);

// Perform business operations.

await _unitOfWork.SaveChangesAsync(ct);

_postCommitProcessor.Enqueue(async (serviceProvider, callbackCt) =>
{
    var cache = serviceProvider.GetRequiredService<ICache>();

    await cache.RemoveAsync(key, callbackCt);
});

await _unitOfWork.CommitTransactionAsync(ct);
```

The invalidation callback executes only after `CommitTransactionAsync` commits successfully.

This prevents the cache invalidation from running for a transaction that subsequently rolls back.

**Automatic-mode distinction:** If `SaveChangesAsync` owns and commits the transaction, a direct awaited cache invalidation after `SaveChangesAsync` is already post-commit. In that case, `IPostCommitProcessor` may not be necessary unless a consistent callback mechanism is preferred.

### 6.3 Other Suitable Candidates

Other suitable uses include:

* Publishing an ephemeral in-process signal after a successful commit.
* Clearing a short-lived memoized projection.
* Emitting a best-effort metric associated with a committed business operation.

These effects benefit from being tied to a successful write but do not require durable execution.

---

## 7. Availability Constraints

`IPostCommitProcessor.Enqueue` is meaningful only when the callback belongs to a Unit of Work transaction whose successful commit will invoke the processor.

If a callback is enqueued without a corresponding invocation, it remains in memory until the scope ends and is effectively lost.

### Usage by Execution Context

| Execution context                | Transaction context                                                                       | Appropriate use                                                                                                |
| -------------------------------- | ----------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| Command handler — automatic mode | `SaveChangesAsync` owns and commits the transaction                                       | Yes                                                                                                            |
| Command handler — manual mode    | Between `BeginTransactionAsync` and `CommitTransactionAsync`                              | Yes                                                                                                            |
| Domain-event handler             | Dispatched within the Unit of Work's save-and-commit workflow                             | Yes, when the transaction lifecycle guarantees post-commit invocation                                          |
| Integration-event handler        | Usually executes within Outbox background processing, outside the originating transaction | Do not enqueue unless the handler explicitly creates a Unit of Work transaction that will invoke the processor |
| Background-job handler           | Depends on whether the handler opens and commits its own Unit of Work                     | Only when a corresponding transaction and invocation exist                                                     |
| API controller                   | No transaction guarantee from the controller itself                                       | Do not enqueue merely because the request is active                                                            |
| Middleware                       | No transaction guarantee from middleware itself                                           | Do not enqueue merely because the request is active                                                            |

### The Integration-Event Handler Case

Integration-event handlers generally execute as part of the Outbox processing workflow, after the transaction that originally produced the event has already committed.

Consequently, the originating transaction's post-commit processor is no longer available as an execution boundary for the integration-event handler.

Calling `Enqueue` in such a handler without establishing a new compatible Unit of Work transaction can result in the callback never being invoked.

If an integration-event handler needs a post-commit effect, it should choose an approach appropriate to its work:

1. **Use a new Unit of Work:** If the handler performs database changes, let its domain-event processing register the callback and ensure the handler's Unit of Work commits and invokes the processor.
2. **Perform the effect directly:** If no additional transaction is required and the effect is appropriate to execute immediately.
3. **Schedule durable work:** Use the Scheduling capability when the work needs background execution or durable recovery.

Do not assume that an integration-event handler inherits the original command's transaction or callback queue.

---

## 8. Failure Semantics

Post-commit callbacks are best-effort. Once the transaction commits, a callback failure cannot roll back the database transaction.

The Unit of Work uses a defensive wrapper around callback processing:

```csharp
private async Task InvokePostCommitCallbacksAsync(
    CancellationToken cancellationToken)
{
    try
    {
        await _postCommitProcessor.InvokeAsync(
            _serviceProvider,
            cancellationToken);
    }
    catch (Exception)
    {
        // Post-commit callbacks are best-effort.
        // Individual callback failures should be logged by the processor.
        // The transaction has already committed.
    }
}
```

This is illustrative; the actual implementation should follow the project's logging and cancellation policies.

### Consequences

**1. Callback failure does not undo the transaction.**

The database commit has already succeeded. A later exception cannot reverse that commit.

**2. Callback failure should not be reported as a business-transaction rollback.**

The business operation succeeded from the database's perspective. A failed best-effort side effect is a separate operational concern.

**3. Failures must remain observable.**

The callback processor should log individual failures and continue with subsequent callbacks when that is the configured behavior. A Unit of Work catch-all is only a final safety boundary; it should not replace meaningful logging.

**4. Cancellation requires an explicit policy.**

A cancellation token may be signaled after the database commits. If the processor treats cancellation as a reason to stop, some callbacks may not execute.

For side effects that must be attempted despite request cancellation, consider a deliberately chosen token policy. However, `CancellationToken.None` does not make an effect durable and can cause the request to wait longer.

**5. There is no automatic retry.**

If a callback fails or is lost during process termination, `IPostCommitProcessor` does not retry it. Use a durable mechanism if recovery is required.

---

## 9. Ordering, Scope, and Lifetime

### Ordering

Callbacks are processed in the order in which they were enqueued, assuming the processor implements the documented FIFO behavior.

This does not establish ordering guarantees across separate requests, independent scopes, or multiple application instances.

### Scope

The processor is scoped, so each request or execution scope has an independent callback queue.

Services resolved through the provided `IServiceProvider` use that scope's dependency-injection lifetime rules.

If the callbacks resolve a scoped EF Core context, it may be the same context used by the Unit of Work. After the transaction commits, any further database work performed by a callback is a separate operation, not part of the completed transaction.

### Lifetime

Callbacks are closures stored in memory. They are not persisted to the database.

If the process terminates before callback execution, the callbacks are lost.

### Execution Duration

The Unit of Work awaits post-commit processing. Therefore:

* Callbacks finish before the relevant Unit of Work method returns, subject to the configured exception-handling policy.
* A slow callback increases the duration of `SaveChangesAsync` in automatic mode or `CommitTransactionAsync` in manual mode.
* A callback that waits indefinitely can delay completion of the request or background operation.
* Callbacks should remain short and bounded.

Long-running work belongs in a background job or another suitable execution mechanism.

---

## 10. Relationship to Other Mechanisms

| Mechanism                                              | Durable                                         | Runs after commit                                    | Can be lost on process crash                                  | Intended use                                             |
| ------------------------------------------------------ | ----------------------------------------------- | ---------------------------------------------------- | ------------------------------------------------------------- | -------------------------------------------------------- |
| `IPostCommitProcessor`                                 | No                                              | Yes                                                  | Yes                                                           | Fast, best-effort side effects                           |
| Transactional Outbox                                   | Outbox record is durable                        | Dispatch occurs later                                | The persisted intent survives; processing can be retried      | Reliable integration-event processing                    |
| Direct call after `SaveChangesAsync` in automatic mode | No                                              | Yes, if that method owns and commits the transaction | Yes                                                           | Simple best-effort post-commit work                      |
| Direct call inside an active manual transaction        | No                                              | Not necessarily                                      | Yes                                                           | Not appropriate when a post-commit guarantee is required |
| Durable background job                                 | Depends on configured storage and job lifecycle | Depends on when it is scheduled or enqueued          | Designed for persistence and recovery when configured durably | Background execution and retries                         |

### Important Distinction

`IPostCommitProcessor` and a direct call after `SaveChangesAsync` are functionally similar when automatic mode has already committed successfully.

The processor's main advantage is that it provides a consistent commit-aware mechanism for both automatic and manual transaction modes.

The Outbox solves a different problem: it durably records work that must remain recoverable even when the application crashes.

---

## 11. Anti-Patterns

### Do Not Use It for Durable Effects

Email, SMS, Push, and reliable cross-module integration events should use the Outbox or another appropriate durable mechanism.

### Do Not Enqueue Without a Matching Commit Lifecycle

A callback that is never invoked will be lost when its scope ends.

### Do Not Expect Callback Exceptions to Roll Back the Transaction

The transaction has already committed. Exceptions from best-effort callbacks should be logged and handled as post-commit failures.

### Do Not Enqueue Long-Running Work

Because callbacks are awaited, long operations can delay the completion of the command or background operation.

Schedule long-running work through the Scheduling capability when appropriate.

### Do Not Assume Global Ordering

FIFO ordering applies to the processor's queue, not to callbacks across separate scopes or application instances.

### Do Not Confuse Post-Commit Execution with Durability

Running after commit ensures the side effect is not attempted before the transaction commits. It does not guarantee that the effect will execute or succeed.

### Do Not Ignore Cancellation Semantics

The request cancellation token may be signaled after the business transaction commits. Decide explicitly whether a callback should honor that cancellation or receive a different token policy.

### Do Not Capture Expired or Unmanaged Dependencies

Resolve scoped services through the appropriate active service provider. Do not retain request-scoped services for execution after their scope has ended.

---

## 12. Design Rules

1. **Enqueue only when a matching Unit of Work commit lifecycle will invoke the processor.**
2. **Run callbacks only after the transaction commits successfully.**
3. **Keep callbacks short and bounded.**
4. **Treat callback execution as best-effort, not durable processing.**
5. **Do not use the processor in integration-event handlers without establishing a compatible transaction and invocation lifecycle.**
6. **Resolve required dependencies through the appropriate active service provider.**
7. **Define cancellation behavior deliberately, especially after commit.**
8. **Log callback failures because the Unit of Work may intentionally prevent them from surfacing as business-operation failures.**
9. **Use the Outbox or durable jobs whenever loss is unacceptable.**
10. **Ensure rollback and disposal paths cannot accidentally execute callbacks registered for an uncommitted transaction.**

---

## 13. Summary

| Question                                             | Answer                                                                                             |
| ---------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| What is `IPostCommitProcessor`?                      | A scoped, in-memory callback processor invoked after a successful Unit of Work transaction commit. |
| Who invokes it?                                      | The Unit of Work.                                                                                  |
| When does it run in automatic mode?                  | After the commit inside `SaveChangesAsync`.                                                        |
| When does it run in manual mode?                     | After the commit inside `CommitTransactionAsync`.                                                  |
| Is it durable?                                       | No. Callbacks are lost if the process terminates before execution.                                 |
| Are callbacks awaited?                               | Yes. The Unit of Work awaits callback processing before its commit-related method returns.         |
| Are callback failures business-transaction failures? | No. The transaction has already committed.                                                         |
| Does it retry failed callbacks?                      | No.                                                                                                |
| What belongs in it?                                  | Fast, best-effort side effects such as SignalR hints and suitable cache invalidation.              |
| What does not belong in it?                          | Durable notifications, reliable cross-module events, and long-running work.                        |
| What is the core guarantee?                          | Callbacks execute after the commit in both automatic and manual transaction modes.                 |

**The governing principle:** `IPostCommitProcessor` keeps best-effort side effects off the rollback path without turning them into durable work. The Unit of Work determines the commit boundary; the processor executes callbacks after that boundary; the Outbox handles work that must survive failure.
