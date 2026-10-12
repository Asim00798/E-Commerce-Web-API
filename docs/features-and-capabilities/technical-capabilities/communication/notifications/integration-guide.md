\# Notifications — Integration Guide



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Document Type:\*\* Developer Integration Guide



\## 1. Overview



This guide explains how to add a new notification to the system, including email, SMS, push notifications, persistent in-app notifications, and ephemeral real-time events.



It assumes familiarity with the system's three notification categories:



| Category                      | Mechanism                                             | Durability                                                     | Example                                                   |

| ----------------------------- | ----------------------------------------------------- | -------------------------------------------------------------- | --------------------------------------------------------- |

| \*\*Durable External\*\*          | Outbox + Integration Events                           | Durable publication intent, retries, and idempotent processing | Order confirmation email, shipping SMS, push notification |

| \*\*In-App (Persistent)\*\*       | Domain event handler, within the business transaction | Durable database record, queryable through REST                | An "Order placed" entry in the notifications dropdown     |

| \*\*Ephemeral Real-Time Event\*\* | Post-commit callback                                  | Best-effort; no delivery guarantee                             | A toast indicating that a new notification is available   |



All three can be triggered by the same domain event, but they follow different implementation paths.



This guide walks through each path step by step.



\---



\## 2. Architecture at a Glance



```text

&#x20;                      Domain Event

&#x20;                           │

&#x20;             ┌─────────────┼─────────────┐

&#x20;             │             │             │

&#x20;             ▼             ▼             ▼

&#x20;      In-App Notification  Outbox    Post-Commit Event

&#x20;         (Database)      (External)    (Ephemeral)

&#x20;             │             │             │

&#x20;             ▼             ▼             ▼

&#x20;          REST API     Email / SMS /   SignalR

&#x20;             │             Push

&#x20;             ▼

&#x20;         Client UI

```



\* \*\*In-App Notifications\*\* are persisted within the business transaction.

\* \*\*Durable External Notifications\*\* are dispatched through the Transactional Outbox.

\* \*\*Ephemeral Real-Time Events\*\* provide immediate, best-effort hints and never replace durable state.



\---



\## 3. Core Design Principles



Before adding a notification, understand the principles that guide its implementation.



\* \*\*The database is the source of truth.\*\* User-visible state must be persisted before the client is informed about it.

\* \*\*External communication is eventually consistent.\*\* Email, SMS, and push notifications are dispatched through the Transactional Outbox.

\* \*\*Real-time events are ephemeral.\*\* They improve responsiveness but never replace durable state.

\* \*\*Business consistency takes precedence over user experience.\*\* A persistent notification must exist in the database before the client is told about it.

\* \*\*Application code is transport-agnostic.\*\* Handlers depend on abstractions such as `IRealtimeEventPublisher` and `IPostCommitProcessor`, not directly on SignalR.

\* \*\*Infrastructure provides reliability mechanisms.\*\* Retries, correlation, idempotency, logging, and background processing are handled by the platform rather than reimplemented in individual handlers.



These principles explain the reasoning behind the implementation and help maintain consistency as the system evolves.



\---



\## 4. Prerequisites — Understanding the Abstractions



Review the existing contracts before adding a notification.



| Layer              | Interface / Class                                                            | Purpose                                                                                                   |

| ------------------ | ---------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------- |

| \*\*Application\*\*    | `IEmailChannel`, `ISmsChannel`, `IPushChannel`                               | Entry points for durable external notification channels                                                   |

| \*\*Application\*\*    | `IEmailNotificationModel`, `ISmsNotificationModel`, `IPushNotificationModel` | Channel-specific model contracts that enforce properties such as recipient and subject                    |

| \*\*Application\*\*    | `INotificationModel`                                                         | Root marker contract providing `TemplateName`                                                             |

| \*\*Application\*\*    | `NotificationRequest<T>`                                                     | Wraps a user ID and a typed notification model                                                            |

| \*\*Application\*\*    | `IUserNotificationRepository`                                                | Persists in-app notification records; the repository does not call `SaveChangesAsync`                     |

| \*\*Application\*\*    | `IPostCommitProcessor`                                                       | Generic infrastructure abstraction for enqueuing callbacks that run after a successful transaction commit |

| \*\*Application\*\*    | `IRealtimeEventPublisher`                                                    | Publishes ephemeral real-time events to connected clients                                                 |

| \*\*Application\*\*    | `RealTimeEvents`                                                             | Transport-neutral constants for client-side event names                                                   |

| \*\*Infrastructure\*\* | Email, SMS, and push channels, composers, and transports                     | Concrete notification-delivery implementations                                                            |

| \*\*Infrastructure\*\* | `PostCommitProcessor`                                                        | Executes queued post-commit callbacks                                                                     |

| \*\*Infrastructure\*\* | `SignalRRealtimeEventPublisher`                                              | SignalR implementation of `IRealtimeEventPublisher`                                                       |



\---



\## 5. Decision Tree — Which Path Should You Use?



Ask these two questions.



\### Question 1: Does the notification need reliable delivery across process failures?



\* \*\*Yes:\*\* Use the Durable External path with the Outbox and Integration Events.

\* \*\*No:\*\* Consider an In-App notification, an Ephemeral Real-Time event, or both, depending on whether the information must be retained.



\### Question 2: Should the notification be persisted and available to the user later?



\* \*\*Yes:\*\* Use an In-App Notification persisted within the business transaction.

\* \*\*No:\*\* An Ephemeral Real-Time event may be sufficient, such as a live counter update.



You can combine these paths.



For example, placing an order can produce:



\* An \*\*external email\*\* that is dispatched through the Outbox.

\* An \*\*in-app notification record\*\* that is immediately queryable after the transaction commits.

\* An \*\*ephemeral real-time event\*\* that tells the client to refresh the in-app notification list.



\---



\## 6. Adding a Durable External Notification



This section explains how to add email, SMS, or push notifications using the Transactional Outbox.



\### 6.1 Create the Typed Model



Add a class under:



`Application/Shared/Communication/Notifications/Models/`



Implement the appropriate channel-specific interface.



\*\*Example: Email model\*\*



```csharp

public sealed class OrderConfirmationEmail : IEmailNotificationModel

{

&#x20;   public string RecipientEmail { get; init; }



&#x20;   public string Subject => "Order Confirmed";



&#x20;   public string CustomerName { get; init; }



&#x20;   public Guid OrderId { get; init; }



&#x20;   public decimal Total { get; init; }



&#x20;   public string TemplateName => "OrderConfirmation";

}

```



The `TemplateName` must match the template expected by the notification composer.



\### 6.2 Create or Update the Domain Event Handler



In the owning bounded context, such as `Ordering/DomainEventHandlers/`, add or update a handler that creates the integration event and writes it to the Outbox.



```csharp

public sealed class OrderPlacedDomainEventHandler

&#x20;   : IDomainEventHandler<OrderPlacedDomainEvent>

{

&#x20;   private readonly IOutboxMessageWriter \_outboxWriter;

&#x20;   private readonly IAppContext \_appContext;



&#x20;   public OrderPlacedDomainEventHandler(

&#x20;       IOutboxMessageWriter outboxWriter,

&#x20;       IAppContext appContext)

&#x20;   {

&#x20;       \_outboxWriter = outboxWriter;

&#x20;       \_appContext = appContext;

&#x20;   }



&#x20;   public async Task Handle(

&#x20;       OrderPlacedDomainEvent domainEvent,

&#x20;       CancellationToken ct)

&#x20;   {

&#x20;       var integrationEvent = new OrderPlacedIntegrationEvent(/\* ... \*/)

&#x20;       {

&#x20;           CorrelationId = \_appContext.CorrelationId

&#x20;       };



&#x20;       await \_outboxWriter.WriteAsync(integrationEvent, ct);

&#x20;   }

}

```



The integration event should contain the information required by downstream handlers without forcing them to query the originating aggregate.



Writing the Outbox message within the business transaction makes the publication intent atomic with the business-state change.



\### 6.3 Create the Integration Event Handler



In the appropriate integration-event handler location, create a handler that responds to the integration event and invokes the notification channel.



```csharp

public sealed class SendOrderConfirmationEmailHandler

&#x20;   : IIntegrationEventHandler<OrderPlacedIntegrationEvent>

{

&#x20;   private readonly IEmailChannel \_emailChannel;



&#x20;   public SendOrderConfirmationEmailHandler(

&#x20;       IEmailChannel emailChannel)

&#x20;   {

&#x20;       \_emailChannel = emailChannel;

&#x20;   }



&#x20;   public async Task HandleAsync(

&#x20;       OrderPlacedIntegrationEvent evt,

&#x20;       CancellationToken ct)

&#x20;   {

&#x20;       var model = new OrderConfirmationEmail

&#x20;       {

&#x20;           RecipientEmail = evt.CustomerEmail,

&#x20;           CustomerName = evt.CustomerName,

&#x20;           OrderId = evt.OrderId,

&#x20;           Total = evt.TotalAmount

&#x20;       };



&#x20;       await \_emailChannel.SendAsync(

&#x20;           new NotificationRequest<OrderConfirmationEmail>

&#x20;           {

&#x20;               UserId = evt.CustomerId,

&#x20;               Model = model

&#x20;           },

&#x20;           ct);

&#x20;   }

}

```



Use the actual property names and constructor signatures defined by your contracts.



\### 6.4 Add a Template



Create the appropriate template in the configured template directory.



For example:



`Templates/Email/OrderConfirmation.cshtml`



The template receives the corresponding notification model through its `@model` declaration.



Ensure that the template name, model type, and composer configuration agree.



\### 6.5 Dependency Injection Registration



No manual registration is required for the domain event handler or integration event handler if they are already discovered by the application's assembly-scanning configuration.



The notification channels, composers, and transports should already be registered through the existing infrastructure configuration.



\*\*Result:\*\* The notification is dispatched through the existing Outbox pipeline, with the configured retry, dead-letter, and idempotency mechanisms.



\---



\## 7. Adding an In-App Notification



An in-app notification is a persistent record that the user can retrieve later through the application's API.



\### 7.1 Create or Reuse a Domain Event Handler



Add a handler that responds to the relevant domain event and persists a notification using `IUserNotificationRepository`.



```csharp

public sealed class PersistUserNotificationOnOrderPlacedHandler

&#x20;   : IDomainEventHandler<OrderPlacedDomainEvent>

{

&#x20;   private readonly IUserNotificationRepository \_repository;



&#x20;   public PersistUserNotificationOnOrderPlacedHandler(

&#x20;       IUserNotificationRepository repository)

&#x20;   {

&#x20;       \_repository = repository;

&#x20;   }



&#x20;   public async Task Handle(

&#x20;       OrderPlacedDomainEvent domainEvent,

&#x20;       CancellationToken ct)

&#x20;   {

&#x20;       var dto = new UserNotificationDto

&#x20;       {

&#x20;           UserId = domainEvent.CustomerId,

&#x20;           Type = "OrderPlaced",

&#x20;           SourceEventId = domainEvent.OrderId,

&#x20;           Message = $"Your order #{domainEvent.OrderId} has been placed.",

&#x20;           CreatedAtUtc = DateTime.UtcNow

&#x20;       };



&#x20;       await \_repository.AddAsync(dto, ct);

&#x20;   }

}

```



\*\*Important considerations:\*\*



\* `SourceEventId` should identify the originating event or business occurrence according to the notification's idempotency design.

\* A database uniqueness constraint, such as `(UserId, SourceEventId, Type)`, can prevent duplicate notification records.

\* The handler executes within the business transaction, so the notification record is committed atomically with the business state.

\* The repository must not call `SaveChangesAsync`; the Unit of Work commits the changes together.



Use the application's established UTC time abstraction if one exists rather than introducing direct system-clock calls.



\### 7.2 Optionally Add an Ephemeral Real-Time Event



To notify the user immediately, enqueue a post-commit callback within the same handler.



```csharp

\_postCommitProcessor.Enqueue(async (sp, ct) =>

{

&#x20;   var publisher =

&#x20;       sp.GetRequiredService<IRealtimeEventPublisher>();



&#x20;   await publisher.PublishAsync(

&#x20;       new RealTimeMessage

&#x20;       {

&#x20;           UserId = domainEvent.CustomerId,

&#x20;           Method = RealTimeEvents.NewNotification,

&#x20;           Payload = new { type = "OrderPlaced" }

&#x20;       },

&#x20;       ct);

});

```



The callback is registered during the same transaction that creates the notification record.



The post-commit processor executes it only after a successful commit, preventing the application from intentionally publishing a real-time hint for a transaction that rolls back.



\*\*Important:\*\* Post-commit callbacks are best-effort. A process crash after the database commit but before callback execution can cause the real-time event to be lost. The persistent notification remains the source of truth, and the client can retrieve it through the API.



\### 7.3 Existing Infrastructure



No additional infrastructure registration should be necessary if the following components are already configured:



\* `IUserNotificationRepository`

\* `IPostCommitProcessor`

\* The real-time publisher

\* Notification REST endpoints, such as `GET /notifications`



Verify the actual endpoint routes and authorization requirements in the existing implementation.



\---



\## 8. Adding an Ephemeral Real-Time Event Only



Some real-time events do not require a persistent notification record.



Examples include:



\* A user is typing.

\* A live counter has changed.

\* A transient interface hint should be displayed.



For these cases, enqueue a post-commit callback from the relevant domain event handler when the event is associated with a committed business operation.



```csharp

\_postCommitProcessor.Enqueue(async (sp, ct) =>

{

&#x20;   var publisher =

&#x20;       sp.GetRequiredService<IRealtimeEventPublisher>();



&#x20;   await publisher.PublishAsync(

&#x20;       new RealTimeMessage

&#x20;       {

&#x20;           // Set the appropriate recipient, event name,

&#x20;           // and payload for the specific use case.

&#x20;       },

&#x20;       ct);

});

```



Place this code inside the appropriate `IDomainEventHandler<T>` when the real-time event is a consequence of a domain event.



For events that are inherently transient and unrelated to a committed business transaction, such as typing indicators, use an appropriate direct real-time application path instead of manufacturing a domain event or database transaction solely to publish the event.



`IPostCommitProcessor` remains a generic infrastructure abstraction; it is not specific to notifications.



\---



\## 9. Correlation and Observability



Correlation and logging are handled primarily by the shared infrastructure.



\### 9.1 Correlation ID Propagation



The correlation ID from the original HTTP request is propagated to integration events through `IAppContext`.



Domain event handlers that create integration events should set:



```csharp

CorrelationId = \_appContext.CorrelationId

```



For asynchronous Outbox processing, the `CorrelationScopeIntegrationEventHandler<T>` decorator restores the correlation ID to the logging scope so that downstream logs can be correlated with the original operation.



When these mechanisms are already configured, individual notification handlers do not need to implement correlation propagation independently.



\### 9.2 Structured Logging



Transport decorators such as:



\* `LoggedEmailTransport`

\* `LoggedSmsTransport`

\* `LoggedPushTransport`



record delivery attempts using structured fields, such as channel, provider, user ID, and delivery status.



The post-commit processor and SignalR real-time publisher also log their activities.



Ensure logs exclude passwords, verification codes, access tokens, and other sensitive notification content.



\### 9.3 Metrics



For production observability, consider instrumenting notification channels and the post-commit processor with counters and histograms.



Examples:



\* `notifications\_sent\_total`

\* `postcommit\_callback\_failures\_total`



Metrics are optional additions, but they help monitor delivery outcomes and operational failures.



\---



\## 10. Idempotency Guarantees



| Notification Type             | Idempotency Mechanism                                                                                                                       |

| ----------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |

| \*\*Durable External\*\*          | An `IdempotentIntegrationEventHandler<T>` decorator uses a composite key such as `EventId + HandlerIdentifier`.                             |

| \*\*In-App (Persistent)\*\*       | A unique database constraint on `(UserId, SourceEventId, Type)` prevents duplicate notification records for the same identified occurrence. |

| \*\*Ephemeral Real-Time Event\*\* | No durable idempotency guarantee. Duplicate or lost events may be acceptable, depending on the use case.                                    |



The integration-event idempotency decorator is automatically applied to integration-event handlers when configured by the existing infrastructure.



For in-app notifications, the uniqueness constraint is the final protection against duplicate records. Handler code should also be compatible with retries and uniqueness conflicts according to the application's established error-handling policy.



\*\*Important:\*\* Idempotent handler execution does not guarantee exactly-once external delivery. For example, an email provider may accept an email immediately before the application crashes, leaving the application uncertain whether a retry would send a duplicate. Use provider-supported idempotency where available and define the delivery guarantees explicitly.



\---



\## 11. Checklist — Adding a New Notification



\### Durable External Notification (Email, SMS, Push)



\* \[ ] Create a typed model implementing the appropriate channel-specific interface.

\* \[ ] Create or update the domain event handler that writes the integration event to the Outbox.

\* \[ ] Create the integration event handler that invokes the relevant notification channel.

\* \[ ] Add the required template file, if the channel uses templates.

\* \[ ] Set the correlation ID from `IAppContext` when creating the integration event.

\* \[ ] Verify retry, idempotency, and failure-handling behavior.



\### In-App Persistent Notification



\* \[ ] Add a domain event handler that calls `IUserNotificationRepository.AddAsync(...)`.

\* \[ ] Ensure the repository participates in the existing Unit of Work.

\* \[ ] Verify the unique constraint and idempotency behavior.

\* \[ ] Optionally enqueue a post-commit real-time callback in the same handler.

\* \[ ] Verify that the REST endpoints expose the notification appropriately.



\### Ephemeral Real-Time Event



\* \[ ] Identify the appropriate trigger.

\* \[ ] Use `IPostCommitProcessor.Enqueue(...)` when publication must occur after a successful database commit.

\* \[ ] Use the appropriate real-time publisher and event contract.

\* \[ ] Accept and document the possibility of lost or duplicate events.

\* \[ ] Avoid creating persistent notification records unless the business requires them.



\### All Notification Types



\* \[ ] Confirm that dependency injection and assembly scanning are configured.

\* \[ ] Confirm that sensitive data is not exposed in logs or real-time payloads.

\* \[ ] Verify authorization and recipient targeting.

\* \[ ] Confirm that failure behavior matches the notification's durability requirements.

\* \[ ] Reuse existing infrastructure rather than creating a new dispatcher or background job for each notification.



\---



\## 12. Summary



Adding a notification follows a consistent process:



1\. \*\*Choose the notification type:\*\* durable external, persistent in-app, or ephemeral real-time.

2\. \*\*Create the appropriate model or DTO\*\* when required.

3\. \*\*Implement the handler:\*\* use an integration-event handler for external delivery and a domain-event handler for transactionally persisted in-app notifications.

4\. \*\*Add the template or post-commit callback\*\* when needed.

5\. \*\*Verify the reliability requirements:\*\* persistence, retries, idempotency, correlation, and failure handling.



The existing infrastructure should handle shared delivery and execution concerns. Adding a notification should not require changes to the Outbox pipeline, Hangfire configuration, SignalR infrastructure, or transport registration when the required capabilities are already configured.



\*\*The governing principle is simple:\*\* persist business state when durability is required, use the Outbox for reliable publication, and treat real-time events as best-effort delivery hints rather than a source of truth.



