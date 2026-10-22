# Push Notification Pillar

**Durable Push Delivery via Firebase — Criticality-Based Durability**

| Property         | Value                                                                                          |
| ---------------- | ---------------------------------------------------------------------------------------------- |
| **Version**      | 2.0 — Final                                                                                    |
| **Architecture** | Channel-first, dispatcher-free                                                                 |
| **Status**       | Level 2 (standard Outbox path) is implemented. Levels 1 and 3 are documented as design intent. |

---

## 0. Document Scope

This document describes the push notification pillar: how push notifications are delivered from a business event to a user's device.

It covers:

* The architecture of push delivery in the current implementation.
* The responsibilities of each component: channel, composer, transport, and device repository.
* The error-handling and idempotency model.
* The criticality model as a design framework, including which levels are implemented today.

**Folder paths reflect the current codebase.** If the project layout changes, paths may need updating, but the component responsibilities and execution flow remain the same.

---

## 1. Overview

Push notifications are delivered through the **Transactional Outbox + Integration Events** pipeline, providing at-least-once delivery attempts and per-handler idempotency.

Multi-device delivery is supported: every active device registered for a user receives the notification. Invalid tokens are deactivated automatically.

### 1.1 Criticality Model

The subsystem is designed around three durability tiers corresponding to business importance.

**The current implementation provides Level 2 only.** Levels 1 and 3 describe the intended design direction and have not yet been implemented.

| Level                      | Examples                        | Intended Mechanism                         | Intended Guarantee                                               |
| -------------------------- | ------------------------------- | ------------------------------------------ | ---------------------------------------------------------------- |
| **Level 1 — Non-critical** | Promotions, recommendations     | Post-commit callback; best-effort delivery | Notify if possible; loss is acceptable                           |
| **Level 2 — Important**    | Order updates, shipping alerts  | Outbox + retries                           | Durable notification intent; implemented today                   |
| **Level 3 — Critical**     | Security alerts, fraud warnings | Outbox + tracking + multiple channels      | Enhanced communication assurance; design intent, not implemented |

This document describes Level 2 in detail. Section 8 documents the intended design for the other levels.

### 1.2 Guarantees Provided by Level 2

The implemented Level 2 pipeline provides:

* Atomic persistence of the notification intent alongside the relevant business-state change.
* At-least-once delivery attempts, with per-handler idempotency.
* Automatic deactivation of stale Firebase Installation IDs (FIDs).
* Multi-device fan-out for each user.
* An audit trail of delivery attempts.
* Retry handling for transient failures and quarantine of messages whose retries are exhausted.

**Reliability clarification:** The Outbox prevents the publication intent from being lost when the business transaction commits. It does not guarantee that Firebase will always deliver a notification to the device, nor does it provide unconditional exactly-once external delivery.

---

## 2. Architecture and End-to-End Flow

The following flow describes the implemented Level 2 pipeline.

```text
Business Transaction
       |
       v
Domain Event (e.g., OrderShipped)
       |
       v
Domain Event Handler
       |
       v
Integration Event
       |
       v
Outbox (atomic write)
       |
       v
UnitOfWork.Commit()
       |
       v
OutboxProcessingJob (Hangfire recurring)
       |
       v
IdempotentIntegrationEventHandler<T>
       |
       v
SendOrderShippedPushHandler
(Application/BoundedContexts/Orders/IntegrationEventHandlers)
       |
       | Creates PushNotification model
       | Calls IPushChannel.SendAsync(...)
       v
PushChannel (Infrastructure)
       |
       ├── Load user preferences
       |       └── If !AllowPush, stop
       |
       ├── PushComposer.ComposeAsync(...)
       |       └── Render payload once
       |
       └── IPushDeviceRepository.GetActiveDevicesAsync(userId)
               |
               └── For each active device
                       |
                       └── IPushTransport.SendAsync(...)
                               |
                               v
                       FirebasePushTransport
                               |
                               v
                       Firebase Admin SDK
                               |
                               ├── Success
                               |
                               └── Invalid token
                                       └── Deactivate device
```

### 2.1 Key Architectural Guarantees

* **Atomic Outbox write:** The notification intent is committed with the business-state change.
* **Per-handler idempotency:** Successfully processed integration-event handlers are skipped when the same event is redelivered, subject to the idempotency marker being committed.
* **Preference enforcement:** Push preferences are checked inside `PushChannel`; the integration-event handler does not manage preferences.
* **Stale FID cleanup:** Permanent Firebase rejection errors trigger device deactivation according to the transport's error-handling policy.
* **Delivery auditing:** `LoggedPushTransport` records delivery attempts.
* **No cross-channel coupling:** The push handler depends only on `IPushChannel`, not on Firebase or other notification channels.

---

## 3. Application Layer — Contracts and Models

Application contracts are defined under:

`Application/Shared/Communication/Notifications/`

They provide abstractions without depending on Firebase, transport implementations, or rendering infrastructure.

| Contract                 | Purpose                                                                                                                                                  |
| ------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `IPushChannel`           | Entry point for push notifications. The implementation loads preferences, composes the message, resolves devices, and delegates delivery to a transport. |
| `INotificationModel`     | Marker interface containing a `TemplateName` property. Typed notification models implement this contract.                                                |
| `PushNotification`       | Content model implementing `INotificationModel`. Carries notification content such as title, body, and template-driven fields.                           |
| `NotificationRequest<T>` | Generic wrapper binding a `UserId` to a typed model where `T : INotificationModel`.                                                                      |

### Placement

* **Channel interfaces:** `Application/Shared/Communication/Notifications/Channels/`
* **Typed models:** `Application/Shared/Communication/Notifications/Models/`

### 3.1 The Shape of `PushNotification`

`PushNotification` is a pure content model. It carries what the user sees, including:

* `Title`
* `Body`
* Template-driven fields
* `TemplateName`, which identifies the rendering template

It does **not** carry device identifiers.

The recipient is specified through `NotificationRequest<T>.UserId`, and Firebase Installation IDs are resolved inside `PushChannel`.

Any identifier-like field in `PushNotification` is informational only. The authoritative recipient for the request is its `UserId`.

---

## 4. Infrastructure Layer — Push Components

Concrete implementations reside under:

`Infrastructure/Communication/Notifications/`

### 4.1 `PushChannel`

**File:** `Infrastructure/Communication/Notifications/Channels/PushChannel.cs`
**Implements:** `IPushChannel`

Responsibilities:

* Loads user preferences through `INotificationPreferencesRepository`.
* Returns without sending when `AllowPush` is `false`.
* Calls `PushComposer.ComposeAsync` to render the payload **once**.
* Queries `IPushDeviceRepository.GetActiveDevicesAsync(userId)` for all active devices.
* Creates or prepares a per-device message containing the relevant FID.
* Calls `IPushTransport.SendAsync(...)` for each active device.

The channel is a dedicated push service. It encapsulates preference loading and multi-device orchestration so that integration-event handlers do not need to manage those details.

### 4.2 `PushComposer`

**File:** `Infrastructure/Communication/Notifications/Providers/Push/Composers/PushComposer.cs`

`PushComposer` uses `RazorTemplateRenderer` to render a template, such as:

`Templates/Push/OrderConfirmed.json`

The rendered template is combined with the notification model to produce a composed message containing the title, body, and other template-defined fields.

The Firebase Installation ID is **not** assigned by the composer. The channel supplies it when preparing each device-specific message.

### 4.3 `FirebasePushTransport`

**File:** `Infrastructure/Communication/Notifications/Providers/Push/Transport/FirebasePushTransport.cs`
**Implements:** `IPushTransport`

Responsibilities:

* Constructs and sends a Firebase message using the singleton `FirebaseMessaging` instance.
* Handles permanent device-registration errors according to the configured Firebase error policy.

For `Unregistered` or `InvalidArgument` errors, the documented behavior is:

1. Deactivate the corresponding device through `IPushDeviceRepository.DeactivateByFidAsync(fid)`.
2. Do not propagate the handled error to the caller.
3. Continue processing other devices.

Other exceptions propagate and can trigger an Outbox retry.

**Implementation note:** Firebase's `InvalidArgument` status can indicate an invalid message payload, not only an invalid registration token. The implementation should verify that the error specifically identifies an invalid token before deactivating the device. Otherwise, a malformed shared payload could incorrectly deactivate multiple valid devices.

### 4.4 `LoggedPushTransport` — Audit Decorator

**File:** `Infrastructure/Communication/Notifications/Providers/Push/Transport/LoggedPushTransport.cs`

`LoggedPushTransport` decorates `IPushTransport` and records each delivery attempt in the `NotificationLog` audit table.

Recorded information includes:

* Channel
* Provider
* User
* FID
* Elapsed time
* Result
* Error details

It follows the same architectural pattern as `LoggedEmailTransport` and `LoggedSmsTransport`.

**Registration example:**

```csharp
services.AddScoped<IPushTransport, FirebasePushTransport>();

services.Decorate<IPushTransport, LoggedPushTransport>();
```

### 4.5 `PushDevice` Entity and Repository

**Entity:** `Infrastructure/Communication/Notifications/Entities/PushDevice.cs`
**Repository interface:** `Infrastructure/Communication/Notifications/Contracts/IPushDeviceRepository.cs`
**Implementation:** `Infrastructure/Persistence/Modules/Notifications/Repositories/PushDeviceRepository.cs`

The repository provides the following operations:

| Method                          | Purpose                                               |
| ------------------------------- | ----------------------------------------------------- |
| `GetActiveDevicesAsync(userId)` | Returns all active devices registered for a user.     |
| `AddAsync(device)`              | Registers a device.                                   |
| `DeactivateAsync(deviceId)`     | Deactivates a specific device.                        |
| `DeactivateByFidAsync(fid)`     | Deactivates a device by its Firebase Installation ID. |

`PushChannel` uses the repository for multi-device fan-out, while `PushDeviceRegistrationService` uses it for registration and device management.

---

## 5. Device Management

Push device registration and deactivation are handled by the Notifications module:

`Application/Modules/Notifications/`

The module includes:

* `RegisterPushDeviceCommand` — stores a device registration, designed to be idempotent by FID.
* `DeactivatePushDeviceCommand` — marks a device inactive.
* REST endpoints:

  * `POST /api/push-devices`
  * `DELETE /api/push-devices/{id}`

The Application abstraction `IPushDeviceRegistrationService` is implemented in Infrastructure by `PushDeviceRegistrationService`.

Device registration must also enforce appropriate user authorization so that a user cannot register or deactivate another user's devices without permission.

---

## 6. Configuration

Example configuration:

```json
{
  "Push": {
    "CredentialFilePath": "Firebase/firebase-service-account.json"
  }
}
```

`PushOptions` is bound to the `Push` configuration section.

**Security requirements:**

* Never commit Firebase service-account credentials to source control.
* Use environment-based secret management or a dedicated secret store in production.
* Restrict access to the service account to the permissions needed for push delivery.
* Avoid logging credentials or sensitive configuration values.

Firebase initialization occurs once during startup through the Firebase registration extensions.

**Implementation note:** Verify the exact registration method name against `FirebaseExtensions.cs` in the current codebase.

---

## 7. Error Handling and Retries

| Scenario                              | Behavior                                                                                                                                                          |
| ------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Firebase temporarily unavailable      | The transport throws, allowing the Outbox to retry the affected message according to the configured retry policy and `MaxRetryCount`.                             |
| Invalid or unregistered token         | The device is deactivated when the Firebase error is confirmed to indicate a permanently invalid registration. The handled per-device failure does not propagate. |
| Other per-device transport exceptions | The exception propagates and can trigger a retry of the integration event.                                                                                        |
| Retry exhaustion                      | The message is moved to the dead-letter table; `DeadLetterMonitorJob` logs the quarantine.                                                                        |

### 7.1 Retry Scope

The Outbox retries **messages, not batches**.

During a processing cycle, `GetPendingMessagesAsync` retrieves a batch, and each message is processed independently. A retry applies to a specific message.

The idempotency decorator uses `(EventId, HandlerIdentifier)` to track processing per handler. If a message has multiple handlers, previously successful handlers can be skipped while failed or unprocessed handlers are retried.

This behavior depends on the implementation correctly persisting processing markers and handling failures around transaction boundaries.

**Important:** A transport exception may occur after Firebase has accepted a message but before the application receives or records the result. A subsequent retry can therefore produce a duplicate push. The system should not claim exactly-once external delivery.

---

## 8. Criticality-Based Durability — Design Model

**Status:** Only Level 2 is implemented. Levels 1 and 3 describe intended future behavior and are not part of the current implementation.

### 8.1 Level 1 — Non-Critical (Design)

**Intended examples:**

* Promotions
* Recommendations
* "Someone liked your post"

**Intended mechanism:** Best-effort delivery without Outbox involvement. Loss is acceptable.

A post-commit callback is available only within an active transaction managed by the Unit of Work. It is not directly available from an integration-event handler running in a background job outside that transaction.

Two coherent implementation options are available.

#### Option A — Post-Commit from a Domain Event Handler

A domain event handler enqueues a callback through `IPostCommitProcessor.Enqueue(...)`.

After the transaction commits, the callback sends the push directly through `IPushChannel.SendAsync(...)`.

Characteristics:

* Best-effort delivery
* No durable delivery intent
* No Outbox retry
* The event can be lost if the process crashes before callback execution or if the transport fails

#### Option B — Direct Send from a Background Job

A fire-and-forget job uses `IJobScheduler.Enqueue` to send the push directly.

Characteristics:

* No Outbox
* No post-commit callback
* Best-effort execution
* Loss is acceptable

Neither option is implemented as a criticality-based Level 1 mechanism today.

**Architectural clarification:** An integration-event handler cannot enqueue a callback into the original transaction's post-commit pipeline because the original transaction has already committed and the handler executes separately.

### 8.2 Level 2 — Important (Implemented)

**Examples:**

* Order updates
* Shipping alerts
* Chat message notifications

**Mechanism:** The full Outbox and idempotency pipeline described throughout this document.

The implementation provides:

* Atomic persistence of the notification intent with the business-state change.
* Asynchronous delivery through the Outbox processing job.
* Retries for processing failures.
* Automatic cleanup of confirmed invalid device registrations.
* Per-handler duplicate suppression through the idempotency mechanism.

The guarantee is durable notification intent and retryable delivery—not unconditional successful receipt by a device.

### 8.3 Level 3 — Critical (Design)

**Intended examples:**

* Security alerts
* Fraud warnings
* Critical financial events

**Intended mechanism:** Extend the Level 2 foundation with:

* Multi-channel delivery, such as push, email, and SMS.
* A persistent in-app notification record.
* Delivery tracking and escalation when acknowledgment is not received.

**Not implemented today.** The codebase does not currently include a multi-channel coordinator, acknowledgment tracking, or escalation logic.

A handler that injects multiple channels and calls them sequentially is performing manual composition. It is not using a dedicated critical-notification orchestration framework.

### 8.4 Responsibility Split

When Levels 1 and 3 are implemented, the intended responsibility split is:

* **Application:** Determines when to notify, which criticality level applies, and which channels are required.
* **Infrastructure:** Provides the delivery mechanisms, Outbox integration, transports, and tracking infrastructure.
* **Firebase:** Handles device routing and push delivery for the push channel.

The current implementation does not include a criticality-selection mechanism. Handlers choose their path through the dependencies they use and the channels they invoke.

---

## 9. Idempotency and Duplicate Prevention

The `IdempotentIntegrationEventHandler<T>` decorator records `(EventId, HandlerIdentifier)` in the `ProcessedEvents` table.

Consequently:

* Redelivery of an integration event skips a handler whose successful processing marker has already been persisted.
* If a message has multiple handlers, successfully completed handlers can be skipped while failed or unprocessed handlers are retried.
* The idempotency key is scoped to an event and a handler rather than to the entire message batch.

### Push-Specific Guarantee

The push idempotency guarantee is **sequential and conditional on successful persistence of the idempotency marker**.

A retried integration event will not normally cause another push for a handler whose success was recorded.

However, this guarantee is not transactional with Firebase. If the process crashes after Firebase accepts the push but before the corresponding idempotency marker commits, a retry can send the push again.

Therefore:

**The Outbox and idempotency decorator reduce duplicate processing, but they cannot guarantee exactly-once delivery to external devices.**

---

## 10. Testing

### Unit Tests

Mock the transport, composer, and device repository.

Verify:

* Multi-device iteration.
* Correct per-device FID assignment.
* Preference-based short-circuiting.
* Stale-FID deactivation behavior.
* Error propagation for retryable failures.
* Audit logging behavior where appropriate.

### Integration Tests

Exercise the full pipeline with a mocked Firebase transport.

Verify:

* Idempotency through `ProcessedEvents`.
* Unique constraints and registration behavior for `PushDevice`.
* Preference-based short-circuiting.
* Outbox retry behavior.
* Dead-letter behavior after retry exhaustion.

### End-to-End Tests

Use a real Firebase project and a test device.

Verify:

* Push delivery.
* Handling of confirmed stale or revoked registrations.
* Preference changes.
* Multi-device delivery.

Avoid assuming that every `InvalidArgument` response means the token is invalid; test invalid payloads separately from invalid registrations.

---

## 11. Observability

### 11.1 Structured Logging

Recommended structured fields for each push attempt:

```text
CorrelationId

Channel="Push"

Provider="Firebase"

UserId

FID

Elapsed

Result

RetryCount

Error
```

Sensitive information, including credentials and authentication tokens, must not be logged.

### 11.2 Recommended Metrics

The following metrics are recommended but are **not currently emitted**, according to the documented implementation status. See the Outbox Operational Setup Guide for the current state of metric emission.

* `notifications_sent_total{channel="Push"}`
* `notifications_failed_total{channel="Push"}`
* `stale_fids_deactivated_total`
* `active_push_devices` — gauge

Dead-letter monitoring and health checks are shared with the other notification channels. Refer to the Outbox Operational Setup Guide for those details.

---

## 12. Complete Push Handler Example

The following example demonstrates the Application-layer handler for an order-shipped push notification.

```csharp
using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Models;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEventHandlers;

public sealed class SendOrderShippedPushHandler
    : IIntegrationEventHandler<OrderShippedIntegrationEvent>
{
    private readonly IPushChannel _pushChannel;

    public SendOrderShippedPushHandler(IPushChannel pushChannel)
        => _pushChannel = pushChannel;

    public async Task HandleAsync(
        OrderShippedIntegrationEvent evt,
        CancellationToken ct)
    {
        var model = new PushNotification
        {
            TemplateName = "OrderShipped",
            Title = "Order Shipped",
            Body = $"Your order #{evt.OrderId} has been shipped."
        };

        await _pushChannel.SendAsync(
            new NotificationRequest<PushNotification>
            {
                UserId = evt.CustomerId,
                Model = model
            },
            ct);
    }
}
```

The handler:

* Depends only on `IPushChannel`, not on preferences, device iteration, or Firebase.
* Is automatically discovered and wrapped by the idempotency decorator when the corresponding assembly scanning and decorator registration are configured.
* Creates a content model that the channel composes, fans out, and delivers.

---

## 13. Dependency Injection Registration Summary

The following registrations illustrate the intended composition.

```csharp
// Channel
services.AddScoped<IPushChannel, PushChannel>();

// Composer
services.AddScoped<PushComposer>();

// Transport with audit decorator
services.AddScoped<IPushTransport, FirebasePushTransport>();

services.Decorate<IPushTransport, LoggedPushTransport>();

// Device repository
services.AddScoped<IPushDeviceRepository, PushDeviceRepository>();

// Preferences shared with email and SMS channels
services.AddScoped<
    INotificationPreferencesRepository,
    NotificationPreferencesRepository>();

// Device registration service
services.AddScoped<
    IPushDeviceRegistrationService,
    PushDeviceRegistrationService>();

// Firebase initialization and options
services.AddFirebaseMessaging(); // Verify against FirebaseExtensions.cs

services.Configure<PushOptions>(
    configuration.GetSection("Push"));
```

Registration of channels, transports, and repositories may be consolidated into:

`NotificationsInfrastructureExtensions.AddNotificationsInfrastructure(...)`

Verify the actual registration entry points and method names against the current codebase before using this example as a direct implementation reference.

---

## 14. Summary and Architectural Principle

The push notification pillar provides:

* **Durable notification intent:** Important push notifications use the Outbox and Firebase delivery path (Level 2, implemented).
* **Automatic stale-device cleanup:** Confirmed invalid registrations are deactivated.
* **Multi-device delivery:** Every active device registered for a user is eligible to receive the notification.
* **Per-handler idempotency:** The Outbox decorator suppresses repeated handler execution after successful processing has been recorded.
* **Separation of concerns:** The Application layer depends on `IPushChannel`; Infrastructure owns Firebase integration, transports, and device management.

### Future Design Intent

* **Level 1:** Best-effort push for non-critical notifications through a post-commit callback or fire-and-forget job. Loss is acceptable.
* **Level 3:** Critical multi-channel communication with persistent in-app notifications, delivery tracking, and escalation. Not implemented today.

### Core Principle

**The database and API contain the truth; push notifications inform the user that something has changed.**

The more critical the business event, the less the system should rely on push notifications alone. Select durability according to business impact, not technical convenience.

---

**Document version:** 2.0 — Final

This version frames the criticality model as a design framework rather than implying all three levels are implemented. It clarifies the Level 1 execution paths, the content-only shape of `PushNotification`, the retry scope, the limitations of external-delivery idempotency, and the distinction between confirmed invalid registrations and general Firebase argument errors. Folder paths reflect the documented `Providers/` structure, while unverified registration methods remain explicitly marked for confirmation.
