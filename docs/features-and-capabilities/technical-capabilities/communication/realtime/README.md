\# Real-Time Communication



\*\*E-Commerce Modular Monolith\*\*

\*\*Technical Capability — Communication\*\*

\*\*Ephemeral UI Hints via SignalR\*\*

\*\*Status:\*\* Implemented



\## 1. Overview



The Real-Time Communication capability provides \*\*instant, best-effort hints\*\* to connected clients using SignalR.



It allows the application to proactively inform users that new information may be available—for example, when a new in-app notification has been persisted. The hint carries only the information necessary for the client to identify the relevant update and retrieve the authoritative data through the REST API.



\*\*The database remains the source of truth; SignalR provides an acceleration mechanism, not durable application state.\*\*



The workflow is designed to be transaction-safe: real-time hints are published only after the database transaction that created the underlying data has committed successfully.



A lightweight post-commit callback mechanism coordinates this behavior without introducing SignalR dependencies into the Application layer.



\## 2. Goals and Non-Goals



\### Goals



\* Notify connected clients when a new in-app notification or another relevant application event occurs.

\* Ensure a hint is never published before the corresponding database transaction commits.

\* Keep the Application layer transport-agnostic.

\* Provide a consistent client recovery model in which the REST API remains authoritative.

\* Avoid introducing unnecessary infrastructure for transient real-time updates.



\### Non-Goals



\* Guarantee delivery of real-time messages.

\* Provide message replay or offline queuing.

\* Replace durable notification channels such as Email, SMS, or Push.

\* Guarantee strict message ordering or exactly-once transport semantics.

\* Make SignalR a source of truth for notification state.

\* Require clients to remain continuously connected to preserve application data.



\## 3. Architectural Components



The workflow involves the Application, Infrastructure, and client layers.



\### 3.1 Application Layer



| Component                                     | Responsibility                                                                                       |

| --------------------------------------------- | ---------------------------------------------------------------------------------------------------- |

| `IDomainEventHandler<T>`                      | Handles domain events within the application's persistence workflow.                                 |

| `PersistUserNotificationOnOrderPlacedHandler` | Creates a persistent in-app notification and enqueues a post-commit callback for the real-time hint. |

| `IPostCommitProcessor`                        | Collects callbacks during the transaction and executes them after a successful commit.               |

| `IRealtimeEventPublisher`                     | Transport-agnostic abstraction for publishing ephemeral messages to clients.                         |

| `RealTimeMessage`                             | DTO representing the target user, client method name, and lightweight payload.                       |

| `RealTimeEvents`                              | Constants defining client-side method names, such as `NewNotification`.                              |



The Application layer defines the publishing contract and orchestrates the workflow without depending directly on SignalR.



\### 3.2 Infrastructure Layer



| Component                       | Responsibility                                                                                                           |

| ------------------------------- | ------------------------------------------------------------------------------------------------------------------------ |

| `PostCommitProcessor`           | Scoped implementation of `IPostCommitProcessor` that maintains a FIFO queue of callbacks and executes them when invoked. |

| `SignalRRealtimeEventPublisher` | Implements `IRealtimeEventPublisher` using SignalR's `IHubContext<NotificationHub>`.                                     |

| `NotificationHub`               | Authenticated SignalR endpoint for client connections.                                                                   |

| `UnitOfWork`                    | Coordinates persistence, transaction commitment, and post-commit callback invocation.                                    |

| `SignalRExtensions`             | Configures SignalR services, hub routing, and JWT access-token extraction for supported connection requests.             |



\### 3.3 Client Layer



The browser or mobile client:



\* Connects to `/hubs/notification` using an authenticated SignalR connection.

\* Registers handlers for methods such as `NewNotification`.

\* Treats incoming messages as hints that data may have changed.

\* Fetches authoritative data from the REST API.

\* Resynchronizes relevant state after reconnection.



The client may use the SignalR JavaScript or TypeScript client, or an equivalent client library for its platform.



\## 4. End-to-End Workflow



The following diagram illustrates the complete flow from a business operation to the client refreshing its state.



```text

\[1] Business Transaction

&#x20;         |

&#x20;         v

&#x20;   OrderPlacedDomainEvent

&#x20;         |

&#x20;         v

\[2] PersistUserNotificationOnOrderPlacedHandler

&#x20;         |

&#x20;         +----> Create UserNotification record

&#x20;         |      (idempotency protected by database constraint)

&#x20;         |

&#x20;         +----> Enqueue post-commit callback

&#x20;                via IPostCommitProcessor.Enqueue()

&#x20;                        |

&#x20;                        | Captures:

&#x20;                        | - IRealtimeEventPublisher reference

&#x20;                        | - RealTimeMessage

&#x20;                        v

\[3] UnitOfWork.SaveChangesAsync()

&#x20;         |

&#x20;         v

\[4] UnitOfWork.CommitTransactionAsync()

&#x20;         |

&#x20;         | Database transaction commits successfully

&#x20;         v

&#x20;   IPostCommitProcessor.InvokeAsync(...)

&#x20;         |

&#x20;         v

\[5] PostCommitProcessor executes callbacks

&#x20;         |

&#x20;         +----> Resolve publisher if required

&#x20;         |

&#x20;         +----> PublishAsync(realTimeMessage)

&#x20;                        |

&#x20;                        v

\[6] SignalRRealtimeEventPublisher

&#x20;         |

&#x20;         +----> Clients.User(userId).SendAsync(...)

&#x20;                        |

&#x20;                        v

\[7] SignalR sends message to connected user clients

&#x20;         |

&#x20;         v

\[8] Client receives NewNotification hint

&#x20;         |

&#x20;         +----> Optional toast or badge update

&#x20;         |

&#x20;         +----> GET /notifications?page=1\&pageSize=20

&#x20;                        |

&#x20;                        v

\[9] REST API reads authoritative database state

&#x20;         |

&#x20;         v

&#x20;   Client updates its UI

```



The callback queue is in-memory. A successful database commit does not guarantee that the callback will execute or that SignalR will deliver the message.



\## 5. Step-by-Step Processing



\### 5.1 Inside the Transaction



\*\*1. Domain event raised\*\*



The `Order` aggregate records an `OrderPlacedDomainEvent` when the relevant business operation occurs.



\*\*2. Domain event dispatched\*\*



`UnitOfWork.SaveChangesAsync` dispatches the domain event through the configured domain-event processing mechanism.



\*\*3. Notification handler executes\*\*



`PersistUserNotificationOnOrderPlacedHandler`:



1\. Constructs the notification data using the relevant business information.

2\. Adds the persistent notification through the notification repository.

3\. Enqueues a callback through `IPostCommitProcessor.Enqueue`.

4\. Prepares the `RealTimeMessage` containing the target user, client method, and lightweight payload.



The callback is registered for later execution. \*\*No SignalR publication occurs inside the business transaction.\*\*



The notification record and business changes participate in the same database transaction, assuming the repository and unit-of-work implementation use the same transactional database context.



If the transaction rolls back, the notification record is not committed. The in-memory callback must also be discarded or cleared so that a later invocation cannot publish a hint for rolled-back data.



\### 5.2 After the Transaction Commits



\*\*4. Transaction commits\*\*



`UnitOfWork.CommitTransactionAsync` successfully commits the database transaction.



The notification record is now durable.



\*\*5. Post-commit processing begins\*\*



The unit of work invokes `IPostCommitProcessor.InvokeAsync` after the commit succeeds.



\*\*6. Callback executes\*\*



`PostCommitProcessor` processes its queued callbacks in FIFO order.



Each callback invokes the publishing abstraction using the prepared message and cancellation token. If dependency resolution is deferred until callback execution, the callback resolves `IRealtimeEventPublisher` from the appropriate service provider.



\*\*7. SignalR publication\*\*



`SignalRRealtimeEventPublisher` uses the injected `IHubContext<NotificationHub>` to target the user:



```csharp

await \_hubContext.Clients

&#x20;   .User(userId)

&#x20;   .SendAsync(methodName, payload, cancellationToken);

```



The exact argument types depend on the implementation of `RealTimeMessage` and the publisher abstraction.



SignalR targets connections associated with the specified user identifier. The application's configured user-ID provider and authentication claims must agree on how that identifier is determined.



\*\*8. Client receives the hint\*\*



Connected clients listening for the specified method can react to the message.



A successful server-side send operation does not prove that a client received or processed the message. Clients may be disconnected, reconnecting, or otherwise unable to consume it.



\### 5.3 Client Reaction



\*\*9. Hint received\*\*



The client invokes its registered handler for `NewNotification`.



\*\*10. Authoritative data fetched\*\*



The client requests the relevant information from the REST API, for example:



```http

GET /notifications?page=1\&pageSize=20

```



The endpoint and query parameters are illustrative; the client should use the actual API contract.



\*\*11. UI updated\*\*



The API returns the notification data persisted in the database. The client updates its view using that response rather than treating the SignalR payload as authoritative state.



\## 6. Error Handling and Resilience



| Failure scenario                                                   | Behavior                                                                                                       | Impact                                                                                     |

| ------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |

| Post-commit callback throws                                        | The processor logs the exception and continues with subsequent callbacks if that is its configured policy.     | The corresponding hint may be lost; persisted data remains available through the REST API. |

| User has no active SignalR connection                              | There may be no connected recipient for the message. A successful `SendAsync` call does not necessarily throw. | No immediate client update; the client must resynchronize later.                           |

| SignalR publication fails                                          | The publisher or callback reports an exception, which the post-commit processor can log and isolate.           | The hint is not reliably delivered.                                                        |

| Server crashes after commit but before callback invocation         | The in-memory callback queue is lost.                                                                          | The notification remains persisted, but the hint may never be published.                   |

| Server crashes during callback processing                          | Some callbacks may have run while others remain unexecuted.                                                    | Some clients may receive hints while others do not.                                        |

| Database transaction rolls back                                    | The notification is not committed, and its associated callback must not be executed.                           | No hint should be published for the rolled-back operation.                                 |

| Client disconnects after receiving a hint but before fetching data | The REST request may fail or remain incomplete.                                                                | The client must retry or resynchronize when connectivity returns.                          |



\### Key Design Principle



\*\*The database is authoritative; the real-time hint is best-effort.\*\*



Loss of a hint must not compromise business data integrity or permanently prevent the client from discovering persisted information.



This recovery model depends on the client performing appropriate API reads, including after reconnection or when it otherwise detects that its local state may be stale.



\## 7. Client Interaction and Recovery



Clients must assume that real-time messages can be lost, duplicated, or received when the local view is already stale.



\### Connection Management



The SignalR client should use automatic reconnection where supported.



After reconnection, it should fetch the relevant authoritative state from the REST API rather than assuming that it has received every event.



Automatic reconnection does not replay messages missed while disconnected.



\### State Synchronization



A hint means that new or changed data \*\*may\*\* be available.



The client should:



\* Fetch the relevant resource or collection through the REST API.

\* Handle empty results and unchanged data gracefully.

\* Retry failed API requests according to its normal HTTP error-handling strategy.

\* Avoid depending on the number or ordering of SignalR messages to calculate authoritative state.



\### Authentication



The SignalR connection requires valid authentication according to the application's security configuration.



For browser WebSocket connections, the JavaScript client may transmit the access token through the query string when required by the transport. The server's `OnMessageReceived` configuration should extract the token only for the intended hub route.



Because query-string tokens can appear in request logs, logging must avoid recording sensitive access-token values.



The client must also account for token expiration and refresh authentication when necessary.



\## 8. Testing Considerations



\### 8.1 Unit Tests



\*\*`PostCommitProcessor`\*\*



Verify that:



\* Callbacks execute in FIFO order.

\* A callback failure is logged and does not prevent subsequent callbacks when isolation is the configured behavior.

\* Cancellation follows the documented policy.

\* The queue is cleared or otherwise safely finalized after invocation.

\* A failed transaction does not result in callback execution.

\* Repeated invocation does not unintentionally execute previously completed callbacks.



\*\*`SignalRRealtimeEventPublisher`\*\*



Verify that:



\* The correct user identifier is targeted.

\* The expected client method name and payload are sent.

\* The cancellation token is propagated.

\* Publication failures are surfaced to the caller for logging and handling.



\*\*`PersistUserNotificationOnOrderPlacedHandler`\*\*



Verify that:



\* The notification is added to the repository.

\* The expected post-commit callback is enqueued.

\* The callback contains the correct user identifier and message.

\* Repository failures do not result in a callback being executed.

\* Duplicate domain-event processing does not create duplicate notifications when the configured idempotency constraint is exercised.



\### 8.2 Integration Tests



Use a real or in-memory database, as appropriate, to verify that:



\* The notification and business changes commit together.

\* Post-commit publication occurs only after a successful transaction commit.

\* A rollback prevents publication.

\* Callback failures do not undo an already committed notification.

\* Duplicate processing is handled by the notification's idempotency mechanism.



A mock SignalR publisher can be used to verify orchestration without requiring a live WebSocket connection.



\### 8.3 End-to-End Tests



With a running application and a real SignalR client:



1\. Establish an authenticated connection.

2\. Trigger a business operation that creates an in-app notification.

3\. Verify that the connected client receives the expected hint.

4\. Call the corresponding REST endpoint.

5\. Verify that the persisted notification is returned.

6\. Disconnect the client, trigger another notification, reconnect, and verify that API-based resynchronization retrieves the missing notification.



\## 9. Operational Considerations and Limitations



\### In-Memory Post-Commit Queue



The callback queue is not durable. A process crash after the database commit can permanently lose a pending hint.



This is acceptable for the current best-effort design because the underlying data is durable and clients can recover through the REST API.



If future requirements demand reliable publication attempts, consider a durable publication mechanism. That would introduce additional persistence and operational complexity and should be justified by an actual delivery requirement.



\### Multiple Application Instances



In a multi-instance deployment, SignalR user targeting and connection distribution must be compatible with the hosting topology.



If instances cannot reach the connections established on other instances, an appropriate SignalR scale-out mechanism may be required. This is a deployment concern, not a reason to introduce distributed infrastructure into the current single-process design prematurely.



\### Observability



Useful operational signals include:



\* Number of post-commit callbacks executed.

\* Number of callback failures.

\* SignalR publication exceptions.

\* Callback processing duration.

\* Client reconnection and resynchronization failures, where observable.



These measurements should distinguish server-side publication attempts from actual client receipt, which cannot be inferred from a successful `SendAsync` call alone.



\### Delivery Limitations



This capability does not currently provide:



\* Durable real-time message storage or replay.

\* Guaranteed client receipt or processing acknowledgements.

\* Offline message queues.

\* Exactly-once or strict-order transport semantics.

\* Automatic recovery of lost in-memory callbacks.



These limitations are intentional under the best-effort UI-hint model.



\## 10. Architectural Boundaries



The capability follows the modular monolith's dependency rules:



\* The Application layer depends on `IRealtimeEventPublisher`, not SignalR APIs.

\* SignalR configuration, hubs, and `IHubContext` usage belong to Infrastructure.

\* Post-commit callbacks are invoked only after a successful database commit.

\* Persistent notification data is stored independently of SignalR delivery.

\* The Outbox remains responsible for durable integration-event processing.

\* Notifications remain responsible for notification orchestration and delivery channels.

\* Scheduling remains responsible for background-job infrastructure.



Real-time communication is a technical capability, not a separate business bounded context. It can be used by multiple business modules without transferring their business responsibilities into the real-time subsystem.



\## 11. Related Technical Capabilities



| Capability                                  | Relationship                                                                               |

| ------------------------------------------- | ------------------------------------------------------------------------------------------ |

| \[Notifications](../notifications/README.md) | Owns notification orchestration and durable Email, SMS, and Push delivery workflows.       |

| \[Outbox](../outbox/README.md)               | Provides durable integration-event persistence and dispatch.                               |

| \[Post-Commit](../post-commit/README.md)     | Coordinates callbacks and side effects that must occur after a successful database commit. |

| Scheduling                                  | Provides the background-job execution infrastructure used by durable workflows.            |

| Observability                               | Supports logging, diagnostics, and operational monitoring.                                 |



\## 12. Summary



The Real-Time Communication capability provides best-effort SignalR hints without making transient client communication part of the business transaction.



Its workflow has three phases:



1\. \*\*Inside the transaction:\*\* Persist the notification and enqueue a post-commit callback.

2\. \*\*After commit:\*\* Execute the callback and attempt to publish a SignalR message.

3\. \*\*On the client:\*\* Receive the hint and fetch authoritative state from the REST API.



The design preserves transaction safety, keeps the Application layer transport-agnostic, and avoids unnecessary infrastructure.



\*\*The database guarantees persistence of committed data; SignalR accelerates client awareness of that data but does not guarantee delivery.\*\*



