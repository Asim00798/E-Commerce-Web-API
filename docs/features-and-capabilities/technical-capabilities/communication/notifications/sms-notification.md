# SMS Notification Pillar

**Durable SMS Delivery via Twilio in the DDD Modular Monolith**

**Version:** 2.0 — Final
**Architecture:** Channel-First, Dispatcher-Free, Typed Model Contracts, End-to-End Correlation
**Status:** Implemented

---

## 1. Overview

The SMS pillar is one of the three durable external notification channels, alongside Email and Push. It sends transactional SMS messages—including order updates, shipping notifications, and authentication codes—through the shared notification infrastructure.

All SMS messages are delivered through the **Transactional Outbox + Integration Events** pipeline used by the other notification channels. The Application layer expresses the intent to send an SMS through a typed model, while the Infrastructure layer handles rendering, composition, and delivery through Twilio.

This document covers the lifecycle of an SMS notification, from the domain event that triggers it through the provider delivery attempt.

### 1.1 Delivery Guarantee

The pipeline provides **at-least-once processing with sequential per-handler deduplication**:

* A message is retried until processing succeeds or the message is moved to the dead-letter store.
* A handler that has already succeeded for an event is skipped on retry because its `ProcessedEvents` marker prevents re-execution.
* A handler that has not yet succeeded, or that failed during a previous attempt, runs again.

The pipeline does **not** guarantee exactly-once delivery across the entire pipeline. A documented mark-then-crash window (see §9.3) can cause a handler to execute again even though its external side effect has already completed. For SMS, this means that duplicate messages are possible.

The Outbox provides durable tracking and retry of the intent to send. It does not guarantee that an SMS ultimately reaches the recipient's phone. Messages that exhaust their retries are quarantined for investigation.

---

## 2. Architecture and End-to-End Flow

```text
[Business Transaction]
        |
        v
Domain Event (e.g., OrderShipped)
        |
        v
Domain Event Handler
 - Creates OrderShippedIntegrationEvent
 - Writes the integration event to the Outbox
 - Populates CorrelationId from IAppContext
        |
        v
UnitOfWork.Commit()
        |
        v
OutboxProcessingJob (Hangfire recurring)
        |
        v
CorrelationScopeIntegrationEventHandler<T>
 - Restores CorrelationId into the logging scope
        |
        v
IdempotentIntegrationEventHandler<T>
 - Checks the processed-event marker
        |
        v
SendOrderShippedSmsHandler
        |
        v
ISmsChannel.SendAsync(
   NotificationRequest<OrderShippedSms>)
        |
        v
SmsChannel (Infrastructure)
 - Loads user preferences
 - Stops if AllowSms is false
 - Composes the SMS
 - Sends through ISmsTransport
        |
        v
SmsComposer.ComposeAsync(model)
 - Renders the template
 - Builds SmsMessage
        |
        v
ISmsTransport.SendAsync(smsMessage)
        |
        v
LoggedSmsTransport (audit decorator)
        |
        v
TwilioSmsTransport
 - Sends through the Twilio REST API
        |
        v
Twilio accepts the SMS request
```

**Delivery distinction:** A successful Twilio API response indicates that Twilio accepted the request. It does not necessarily mean the carrier delivered the SMS or that the recipient received it.

### 2.1 Key Guarantees

* **Atomic Outbox write:** The intent to send the SMS is committed with the business state.
* **Correlation ID propagation:** The correlation ID is embedded in the integration event and restored into the logging scope, allowing downstream logs to be associated with the originating request.
* **Per-handler idempotency:** A retried event does not cause a handler whose successful processing has already been recorded to execute again.
* **Retry and dead-lettering:** Twilio failures are retried by the Outbox pipeline up to `MaxRetryCount`. Messages that exhaust their retries are moved to the dead-letter store.
* **Audit trail:** `LoggedSmsTransport` records delivery attempts in the `NotificationLog` table.
* **Internal preference checking:** The channel checks notification preferences, keeping that concern out of the handler.
* **No dispatcher:** The handler depends only on `ISmsChannel`.

---

## 3. Application Layer — Contracts and Models

All contracts are defined under:

```text
Application/Shared/Communication/Notifications/
```

These are application-level abstractions. Twilio, its REST API, and rendering details do not leak into the Application layer.

### 3.1 Core Abstractions

| Contract                | Purpose                                                                                                                                      |
| ----------------------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| `ISmsChannel`           | Entry point for SMS notifications. The implementation loads preferences, composes the message, and delivers it through a transport.          |
| `INotificationModel`    | Root marker interface with a `TemplateName` property. All notification models implement it.                                                  |
| `ISmsNotificationModel` | Extends `INotificationModel` and defines the SMS-specific content contract, including access to the recipient phone number and text content. |

**Placement:**

* Channel interfaces: `Application/Shared/Communication/Notifications/Channels/`
* Marker and model interfaces: `Application/Shared/Communication/Notifications/Abstractions/`

### 3.2 Typed SMS Models

Each model implements `ISmsNotificationModel` and provides a stable `TemplateName` identifying its plain-text template.

| Model                         | `TemplateName`               | Purpose                                        |
| ----------------------------- | ---------------------------- | ---------------------------------------------- |
| `OrderShippedSms`             | `"OrderShipped"`             | Sent when an order has been shipped.           |
| `RegistrationVerificationSms` | `"RegistrationVerification"` | Sent during onboarding for phone verification. |

Models reside in:

```text
Application/Shared/Communication/Notifications/Models/
```

**Model shape:** The exact properties depend on what each template renders. For example, `OrderShippedSms` may expose `OrderId` and `TrackingNumber`.

The `ISmsNotificationModel` contract guarantees access to the phone number and the other SMS-specific members defined by the actual interface. Model-specific properties remain specific to each notification type.

### 3.3 Generic Wrapper

```csharp
public sealed class NotificationRequest<T>
    where T : INotificationModel
{
    public Guid UserId { get; init; }

    public T Model { get; init; } = default!;
}
```

The wrapper binds the recipient's `UserId` to a typed model.

* `UserId` identifies the user whose notification preferences should be checked.
* `Model` carries the SMS content and the model-specific recipient information.

### 3.4 Relationship Between `INotificationModel` and `ISmsNotificationModel`

`ISmsNotificationModel` extends `INotificationModel`.

The channel contract may accept `NotificationRequest<T>` with `T : INotificationModel`, while the SMS implementation requires the model to satisfy `ISmsNotificationModel` to access SMS-specific properties.

**Type-safety consideration:** A generic constraint of `where T : INotificationModel` alone does not prevent an email model from being passed to an SMS channel at compile time. If the actual `ISmsChannel.SendAsync` signature uses that broad constraint, the implementation must validate or cast the model at runtime.

To make the channel contract enforce the intended separation at compile time, prefer a signature constrained to `where T : ISmsNotificationModel`, assuming it is compatible with the shared notification design.

---

## 4. Infrastructure Layer — SMS Channel Components

Concrete implementations live under:

```text
Infrastructure/Communication/Notifications/
```

### 4.1 `SmsChannel`

**File:** `Infrastructure/Communication/Notifications/Channels/SmsChannel.cs`
**Implements:** `ISmsChannel`

Responsibilities:

* Loads user preferences through `INotificationPreferencesRepository`.
* Returns without sending if `AllowSms` is `false`.
* Calls `SmsComposer.ComposeAsync(model)` to construct the `SmsMessage`.
* Passes the composed message to `ISmsTransport.SendAsync(smsMessage)`.

The channel is a dedicated SMS service. It hides preference loading and message composition from handlers and contains no generic broadcast logic.

### 4.2 `SmsComposer`

**File:** `Infrastructure/Communication/Notifications/Providers/Sms/Composers/SmsComposer.cs`

Responsibilities:

* Uses `RazorTemplateRenderer` to render the `.txt` template identified by `model.TemplateName`.
* Reads the recipient phone number from the model through `ISmsNotificationModel`.
* Builds and returns an `SmsMessage` DTO containing the phone number and rendered text.

The composer constructs the SMS message. The transport is responsible for delivering it.

### 4.3 `TwilioSmsTransport`

**File:** `Infrastructure/Communication/Notifications/Providers/Sms/Transport/TwilioSmsTransport.cs`
**Implements:** `ISmsTransport`

Responsibilities:

* Uses the Twilio SDK, such as `MessageResource.CreateAsync`, to send the SMS.
* Configures the recipient number, sender number, and message body.
* Receives credentials and sender configuration through `IOptions<SmsOptions>`, bound from application configuration.
* Initializes the Twilio client through `TwilioClient.Init(accountSid, authToken)` according to the documented implementation.
* Propagates delivery failures to the caller, allowing the Outbox pipeline to record the failure and retry the message.

**Verification required:** Confirm the actual client initialization, lifetime, and credential handling against `TwilioSmsTransport.cs`. In particular, ensure that credentials are initialized safely and that configuration changes or multiple account configurations cannot lead to unintended global-client behavior.

### 4.4 `LoggedSmsTransport` — Audit Decorator

**File:** `Infrastructure/Communication/Notifications/Providers/Sms/Transport/LoggedSmsTransport.cs`

`LoggedSmsTransport` decorates `ISmsTransport` and records send attempts in the `NotificationLog` table.

Responsibilities:

* On success, writes a `NotificationLog` row with a `Sent` outcome.
* On failure, writes a row with a `Failed` outcome and the associated error details.
* Delegates delivery to the inner transport.

The decorator is applied through Scrutor in `NotificationsInfrastructureExtensions`.

**Operational consideration:** Audit persistence should not inadvertently hide the original Twilio failure. Verify how exceptions from the inner transport and the audit write are handled.

### 4.5 `ISmsTransport` — Transport Interface

**File:** `Infrastructure/Communication/Notifications/Providers/Sms/Transport/ISmsTransport.cs`

```csharp
public interface ISmsTransport
{
    Task SendAsync(
        SmsMessage message,
        CancellationToken cancellationToken = default);
}
```

This interface allows the provider implementation to be replaced—for example, switching from Twilio to Vonage—without changing the channel or its consumers.

### 4.6 `SmsMessage` — Message DTO

**File:** `Infrastructure/Communication/Notifications/Messages/SmsMessage.cs`

Expected shape:

```csharp
public sealed class SmsMessage
{
    public string PhoneNumber { get; init; } = string.Empty;

    public string Text { get; init; } = string.Empty;
}
```

**Verification required:** Confirm the actual property set and mutability against `SmsMessage.cs`.

---

## 5. Templates

SMS templates are plain-text files (`.txt`) located under:

```text
Infrastructure/Communication/Notifications/Templates/Sms/
```

They are rendered at runtime by `RazorTemplateRenderer`.

| Template file                  | Used by                       |
| ------------------------------ | ----------------------------- |
| `OrderShipped.txt`             | `OrderShippedSms`             |
| `RegistrationVerification.txt` | `RegistrationVerificationSms` |

Templates have access to the properties exposed by their typed model.

### Example: `OrderShipped.txt`

```text
Dear customer, your order #@Model.OrderId has been shipped!

Track it here: https://example.com/track/@Model.TrackingNumber
```

The property names and URL must match the actual model and routing design.

Templates are deployed alongside the application. Whether they can be updated without recompilation depends on how the application packages and resolves Razor templates; verify the actual deployment configuration before treating runtime template updates as a guarantee.

**SMS length consideration:** SMS messages may be split into multiple segments depending on encoding and message length. This can increase cost. The familiar 160-character limit applies to certain single-segment GSM-7 messages; Unicode encoding and concatenated messages have different limits. See §12 for the content-length limitation.

---

## 6. Configuration

SMS settings are read from configuration and mapped to `SmsOptions`.

### 6.1 `SmsOptions` — Infrastructure

**File:** `Infrastructure/Communication/Notifications/Options/SmsOptions.cs`

The expected options shape is:

```csharp
public sealed class SmsOptions
{
    public string AccountSid { get; init; } = string.Empty;

    public string AuthToken { get; init; } = string.Empty;

    public string FromNumber { get; init; } = string.Empty;
}
```

**Verification required:** Confirm the actual property names against `SmsOptions.cs` before copying the configuration example below. If the implementation differs, update the class or the JSON example as appropriate.

### 6.2 Example `appsettings.json`

```json
{
  "Sms": {
    "AccountSid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "AuthToken": "",
    "FromNumber": "+1234567890"
  }
}
```

In production, credentials must be provided through environment variables or a secret manager. **Never commit Twilio credentials or other secrets to source control.**

### 6.3 Fallback Behavior

When Twilio credentials are absent and a logged fallback is enabled, the system may use `LoggedSmsTransport` or an equivalent fallback to write the composed message to application logs instead of sending it.

This allows development and testing without a real Twilio account.

Verify the actual fallback implementation and configuration in `SmsOptions` and `NotificationsInfrastructureExtensions`.

**Production warning:** A logged fallback is not successful SMS delivery. Its use should be explicit and observable, and it should not silently replace the real transport in production.

---

## 7. Integration Event Handlers, Outbox, and Correlation

### 7.1 Setting the Correlation ID

Domain event handlers that create integration events populate `CorrelationId` from the current `IAppContext`.

The correlation ID must be set on the integration event **before** `IOutboxMessageWriter.WriteAsync` is called, so it becomes part of the persisted event payload.

This allows the later background processing flow to associate notification activity with the originating request.

### 7.2 Automatic Correlation Scope Restoration

`CorrelationScopeIntegrationEventHandler<T>` wraps integration event handlers and restores the event's correlation ID into the logging scope before execution.

This allows subsequent log entries—including idempotency checks, channel operations, composition, transport, and audit activity—to carry the same correlation ID, provided the relevant logging operations execute within that scope.

Example registration in `AddApplication`:

```csharp
services.Decorate(
    typeof(IIntegrationEventHandler<>),
    typeof(IdempotentIntegrationEventHandler<>));

services.Decorate(
    typeof(IIntegrationEventHandler<>),
    typeof(CorrelationScopeIntegrationEventHandler<>));
```

According to the documented Scrutor decoration order, the first decorator becomes the inner wrapper and the last becomes the outer wrapper.

The resulting pipeline, from outermost to innermost, is:

```text
CorrelationScopeIntegrationEventHandler<T>
   |
   v
IdempotentIntegrationEventHandler<T>
   |
   v
Concrete integration event handler
```

The correlation scope surrounds the idempotency check. Therefore, logs for both the handler's execution and an already-processed skip decision can carry the correlation ID.

Handlers remain unaware of correlation management. They simply call `ISmsChannel.SendAsync(...)`.

**Verification required:** Confirm the actual decorator registration order and correlation-scope implementation against the application startup configuration.

### 7.3 Handler Example

Handlers depend directly on `ISmsChannel`. They construct a typed SMS model, wrap it in `NotificationRequest<T>`, and call `SendAsync`.

```csharp
public class SendOrderShippedSmsHandler
    : IIntegrationEventHandler<OrderShippedIntegrationEvent>
{
    private readonly ISmsChannel _smsChannel;

    public SendOrderShippedSmsHandler(ISmsChannel smsChannel)
        => _smsChannel = smsChannel;

    public async Task HandleAsync(
        OrderShippedIntegrationEvent evt,
        CancellationToken ct)
    {
        var model = new OrderShippedSms
        {
            PhoneNumber = evt.CustomerPhone,
            OrderId = evt.OrderId,
            TrackingNumber = evt.TrackingNumber
        };

        await _smsChannel.SendAsync(
            new NotificationRequest<OrderShippedSms>
            {
                UserId = evt.CustomerId,
                Model = model
            },
            ct);
    }
}
```

**Verification required:** The fields used to populate the model—`CustomerPhone`, `OrderId`, and `TrackingNumber`—must match the actual `OrderShippedIntegrationEvent` contract. Adjust the handler if the event exposes different fields or nested objects.

The handler is automatically discovered and wrapped by the idempotency and correlation decorators at startup. It does not require manual registration or contain its own idempotency or correlation logic.

### 7.4 Outbox and Idempotency

The processing sequence is:

1. The integration event is written to the Outbox atomically with the business transaction.
2. `OutboxProcessingJob` delivers the event to the decorated handler pipeline.
3. `IdempotentIntegrationEventHandler<T>` checks `(EventId, HandlerIdentifier)` in `ProcessedEvents` before invoking the concrete handler.
4. On successful handler execution, the decorator records the processed marker.
5. On retry, handlers whose successful markers exist are skipped.

This provides **sequential per-handler deduplication across retries**. It does not provide transactional exactly-once delivery across the database and external Twilio API. See §9.

---

## 8. Error Handling and Retries

| Scenario                                               | Behavior                                                                                                                                                                                       |
| ------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Twilio API unreachable or returns a server error       | `TwilioSmsTransport` throws. The Outbox message is marked as failed and retried according to the Outbox processing policy.                                                                     |
| Authentication failure                                 | The message is retried until the configured limit is reached, then dead-lettered. Persistent credential failures should be investigated rather than expected to recover through retries alone. |
| Invalid phone number (for example, Twilio error 21211) | Twilio reports a permanent error. The message may be retried until the limit is reached, then dead-lettered.                                                                                   |
| Transient network issue                                | The Outbox pipeline retries the message according to its configured processing policy.                                                                                                         |
| Template-rendering error                               | The exception propagates from the composer. The message is marked as failed and retried unless the error-handling policy classifies it otherwise.                                              |

The retry limit is configurable through `OutboxOptions.MaxRetryCount`, with a documented default of `5`.

After the retry limit is reached, the message is moved to the `DeadLetterMessages` table. `DeadLetterMonitorJob` logs a warning when dead-lettered messages exist.

### 8.1 Retry Scope

The Outbox retries **messages, not batches**.

On each cycle, `GetPendingMessagesAsync` returns a batch of messages, and each message is processed independently.

Because idempotency is keyed on `(EventId, HandlerIdentifier)`, a message with multiple handlers can be retried without rerunning handlers whose successful completion has already been recorded. Handlers that failed or were not reached can execute again.

This behavior depends on the processed markers being persisted successfully and on retries using the same event and handler identifiers.

---

## 9. Idempotency and Duplicate Prevention

### 9.1 Marker Design

* Each handler's progress for each event is recorded independently in `ProcessedEvents`.
* The composite key is `(EventId, HandlerIdentifier)`.
* `HandlerIdentifier` is the fully qualified CLR type name of the concrete handler.
* Before a handler executes, the idempotency decorator checks whether the marker exists.
* After the handler succeeds, the decorator records the marker.

### 9.2 Sequential Retry Behavior

Consider a message with two handlers, where the first succeeds and the second fails during the first attempt.

```text
Attempt 1:

    Handler A -> succeeds -> marker inserted

    Handler B -> throws
        |
        v
    OutboxProcessingJob marks the message Failed
    RetryCount = 1


Attempt 2:

    Handler A -> marker exists -> skipped

    Handler B -> no marker -> executes
                           -> succeeds
                           -> marker inserted
        |
        v
    OutboxProcessingJob marks the message Processed
```

Handlers whose successful processing has been recorded are not re-executed.

### 9.3 Mark-Then-Crash Window

A fundamental limitation exists in non-transactional idempotency schemes: the external side effect (sending the SMS) and the processed-marker insert cannot be committed atomically across independent systems.

If the process crashes after Twilio accepts the SMS request but before the `ProcessedEvents` marker commits:

* Twilio may already have accepted the SMS.
* The processed marker does not exist.
* On retry, the decorator sees no marker and executes the handler again.
* The SMS may be sent a second time.

The pipeline cannot eliminate this window by itself.

For critical external side effects, provider-supported idempotency should be used where available. Otherwise, the system must explicitly accept and document the residual duplicate-delivery risk.

A stable message identifier can assist with correlation and troubleshooting, but it only prevents duplicates if the provider or receiving system actually supports deduplication based on that identifier.

### 9.4 Duplicate Marker Insert Race

If two workers process the same event simultaneously, both may pass the `IsProcessedAsync` check and attempt to insert the same `(EventId, HandlerIdentifier)` row.

The composite primary key rejects the duplicate insert.

According to the documented implementation, `ProcessedEventRepository.MarkAsProcessedAsync` catches this case, detaches the failed entity, and treats the outcome as success to avoid a spurious message failure.

**Important:** This race handling protects the processed-marker table from duplicate rows. It does not prevent both workers from sending the SMS before either marker is committed. Concurrent execution therefore requires additional protection if duplicate sends are unacceptable.

---

## 10. Testing

### 10.1 Unit Tests

* **`SmsChannel`:** Use a mock `SmsComposer` and mock `ISmsTransport`. Verify the `AllowSms` preference check and delegation.
* **`SmsComposer`:** Use a fake `RazorTemplateRenderer` to verify template rendering and `SmsMessage` construction.
* **`TwilioSmsTransport`:** Test through an appropriate Twilio client abstraction or isolated provider adapter. Mocking a static SDK API directly may require additional seams.
* **`LoggedSmsTransport`:** Verify that a `NotificationLog` row is created for successful and failed send attempts.

### 10.2 Integration Tests

* **Full flow:** Place an order, execute the Outbox processing flow, and verify that the mock transport received the expected SMS and that the `ProcessedEvents` marker was recorded.
* **Idempotency:** Redeliver the same integration event after successful handler processing. Verify that the transport is not called again.
* **Preference short-circuit:** Disable SMS in the user's preferences and verify that the transport is not called.
* **Retry:** Configure the transport to fail and verify that the message is marked as failed. Then configure it to succeed and verify that processing completes successfully.

### 10.3 End-to-End Tests

Use a Twilio test account or an appropriate provider testing facility to verify the outgoing request, content, and provider response.

If actual delivery is tested, use a verified or otherwise authorized destination and distinguish provider acceptance from carrier delivery confirmation.

---

## 11. Operational Behavior and Monitoring

### 11.1 Logging

`LoggedSmsTransport` records send attempts using structured fields.

The exact field set is defined by the `NotificationLog` entity. Expected fields include:

* `Channel = "Sms"`
* `Provider = "Twilio"`
* Recipient phone number
* Outcome (`Sent` / `Failed`)
* Error message, when applicable
* Elapsed time
* Timestamp

**Verification required:** Confirm the actual field names against:

```text
Infrastructure/Communication/Notifications/Entities/NotificationLog.cs
```

**Privacy note:** Phone numbers are personally identifiable information (PII). If notification logs persist raw phone numbers, ensure that storage, access controls, and retention comply with the application's data-handling policy.

Where practical, store a masked phone number for operational logs—for example, `+1234***7890`—and retain the full number only where necessary for delivery and authorized troubleshooting.

### 11.2 Metrics

The following metrics are recommended but are not currently emitted according to this document:

```text
notifications_sent_total{channel="Sms", provider="Twilio"}

notifications_failed_total{channel="Sms", provider="Twilio"}

notification_delivery_duration_seconds{channel="Sms"}
```

See the Outbox Operational Setup Guide for shared monitoring and operational considerations.

### 11.3 Health Checks

No dedicated SMS health check is implemented.

Twilio problems are currently observed through transport failures and the `NotificationLog` audit trail. Persistent failures may indicate connectivity, authentication, account, or provider issues.

A future `SmsHealthCheck` could periodically make an appropriate Twilio API request to verify connectivity and credentials without sending an actual message. The selected API operation and required permissions would need to be verified.

This is a potential enhancement, not an implemented capability.

### 11.4 Dead-Letter Monitoring

`DeadLetterMonitorJob` logs a warning whenever dead-lettered messages exist.

SMS integration events use the shared Outbox dead-letter mechanism. There is no separate SMS-specific dead-letter pipeline.

Dead-lettered messages require investigation and an appropriate recovery or replay procedure; logging their existence alone does not restore delivery.

---

## 12. Limitations and Future Enhancements

| Limitation                                                    | Impact                                                                                                                                  | Possible mitigation                                                                                                                        |
| ------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| Only Twilio is supported                                      | The system cannot currently switch to other providers such as Vonage or Plivo without implementation changes.                           | Add additional `ISmsTransport` implementations and select the appropriate provider through dependency injection or explicit configuration. |
| No delivery-status tracking                                   | The application cannot distinguish between queued, delivered, and undelivered messages using its own persisted delivery-status records. | Enable Twilio status callbacks and persist relevant callback events in a dedicated table.                                                  |
| No SMS rate limiting                                          | A burst of events may produce a burst of SMS sends.                                                                                     | Add rate limiting inside the transport or an upstream execution mechanism. Consider provider throughput limits as well.                    |
| No content-length validation                                  | Long messages may be split into multiple segments, increasing cost.                                                                     | Validate estimated segment count in the composer and warn, reject, or apply business-specific length limits.                               |
| No dedicated health check                                     | Twilio problems surface through the audit trail and processing failures rather than a dedicated health indicator.                       | Add a future `SmsHealthCheck` that periodically tests the Twilio API.                                                                      |
| Possible duplicate delivery during the mark-then-crash window | An SMS may be sent again if processing crashes before the processed marker is committed.                                                | Use provider-supported idempotency where available; otherwise, document and accept the residual risk.                                      |
| No region-specific sender selection                           | All SMS messages use the configured `FromNumber`.                                                                                       | Introduce sender selection based on supported destination regions and provider requirements as the platform expands.                       |

### 12.1 Logged Fallback

When Twilio credentials are absent and the logged fallback is enabled, the channel may use `LoggedSmsTransport` or an equivalent fallback to record the composed message instead of sending it.

This supports local development and testing without a real Twilio account.

Verify the fallback implementation and configuration in `SmsOptions` and `NotificationsInfrastructureExtensions`. See §6.3.

**Production warning:** A logged fallback must not be mistaken for successful SMS delivery.

---

## 13. Summary

The SMS pillar provides:

* **Durable notification intent:** The Transactional Outbox records the intent to send an SMS atomically with the business transaction.
* **Per-handler idempotency:** Successful handler executions are skipped on subsequent retries when their processed markers have been committed.
* **End-to-end correlation:** The correlation ID embedded in the integration event is restored into the logging scope, linking downstream processing logs to the originating request when logging is performed within that scope.
* **Clean separation of concerns:** The Application layer depends on `ISmsChannel`, while Infrastructure owns Twilio, templates, composition, and transports.
* **Auditability:** `LoggedSmsTransport` records send attempts in `NotificationLog`.
* **Retry and dead-lettering:** The shared Outbox infrastructure handles retries and quarantines messages that exhaust their retry limit.

### 13.1 Delivery Guarantee — Precisely Stated

The pipeline provides **at-least-once processing with sequential per-handler deduplication**. It does not guarantee exactly-once delivery across the database, Twilio, carriers, and the recipient's phone.

A crash after an external side effect but before the processed marker commits can result in duplicate delivery.

The system durably records notification intent, retries processing failures, and quarantines exhausted messages for investigation. These mechanisms improve reliability and operational visibility but cannot guarantee that every SMS reaches its final recipient.

### 13.2 Architectural Principle

The database is the source of truth for the intent to send. The SMS channel is a delivery mechanism, not a system of record.

The Outbox preserves the integration event that initiated the notification. Processing records and transport audit logs provide visibility into subsequent attempts and outcomes.

**Durable intent, observable processing, and explicit failure handling are the guarantees. Successful arrival on the recipient's phone is not.**

---

**Document version:** 2.0 — Final SMS Notification Pillar.

This version standardizes the delivery guarantee as at-least-once processing with sequential per-handler deduplication; removes the implication of literal exactly-once delivery; documents the mark-then-crash window and duplicate-marker race; clarifies per-message retry behavior; moves the unimplemented SMS health check to future enhancements; refreshes folder paths from `External/` to `Providers/`; flags unverified implementation details, including `SmsOptions`, `SmsMessage`, `NotificationLog`, and event property names; documents the `INotificationModel` → `ISmsNotificationModel` hierarchy; and includes privacy considerations for phone numbers in logs.
