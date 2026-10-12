\# Email Notification Pillar



\*\*Durable Email Delivery in the DDD Modular Monolith\*\*

\*\*Version:\*\* 2.0 — Final

\*\*Architecture:\*\* Channel-First, Dispatcher-Free

\*\*Status:\*\* Implemented



\---



\## 1. Overview



The Email pillar is one of the three durable external notification channels, alongside SMS and Push. It handles transactional and operational emails—including order confirmations, password resets, and welcome messages—using the Transactional Outbox and Integration Events pipeline.



The Application layer expresses the intent to send an email through a typed model. The Infrastructure layer handles rendering, composition, and transport through SMTP.



This document covers the complete lifecycle of an email notification, from the domain event that triggers it to the delivery attempt.



\### 1.1 Delivery Guarantee



The pipeline provides \*\*at-least-once delivery with sequential per-handler deduplication\*\*:



\* A message is retried until processing succeeds or the message is moved to the dead-letter store.

\* A handler that has already succeeded for an event is skipped on retry because its `ProcessedEvents` marker prevents re-execution.

\* A handler that has not yet succeeded, or that failed during a previous attempt, runs again.



The pipeline does \*\*not\*\* guarantee exactly-once delivery across the entire pipeline. A documented mark-then-crash window (see §9.3) can cause a handler to execute again even though its external side effect has already completed. Handlers with critical external side effects must implement their own external idempotency where supported.



The Outbox provides durable tracking and retry of the intent to send. It does not guarantee that an email ultimately reaches the recipient's inbox. Messages that exhaust their retries are quarantined for investigation.



\---



\## 2. Architecture and End-to-End Flow



```text

\[Business Transaction]

&#x20;        |

&#x20;        v

Domain Event (e.g., OrderPlaced)

&#x20;        |

&#x20;        v

Domain Event Handler

&#x20; - Creates OrderPlacedIntegrationEvent

&#x20; - Writes the integration event to the Outbox

&#x20;        |

&#x20;        v

UnitOfWork.Commit()

&#x20;        |

&#x20;        v

OutboxProcessingJob (Hangfire recurring)

&#x20;        |

&#x20;        v

IdempotentIntegrationEventHandler<T> (decorator)

&#x20;        |

&#x20;        v

SendOrderConfirmationEmailHandler

&#x20;        |

&#x20;        v

IEmailChannel.SendAsync(

&#x20;   NotificationRequest<OrderConfirmationEmail>)

&#x20;        |

&#x20;        v

EmailChannel (Infrastructure)

&#x20; - Loads user preferences

&#x20; - Stops if AllowEmail is false

&#x20; - Composes the email

&#x20; - Sends the message through IEmailTransport

&#x20;        |

&#x20;        v

EmailComposer.ComposeAsync(model)

&#x20; - RazorTemplateRenderer renders the template

&#x20; - Builds an EmailMessage

&#x20;        |

&#x20;        v

IEmailTransport.SendAsync(emailMessage)

&#x20;        |

&#x20;        v

SmtpEmailTransport

&#x20; - Sends through SMTP using MailKit

&#x20;        |

&#x20;        v

Email delivery attempt

```



\### 2.1 Key Guarantees



\* \*\*Atomic Outbox write:\*\* The intent to send the email is committed with the business state, avoiding the gap between committing a business change and separately recording its notification.

\* \*\*Per-handler idempotency:\*\* `IdempotentIntegrationEventHandler<T>` prevents re-execution of handlers whose successful processing has been recorded.

\* \*\*Retry and dead-lettering:\*\* SMTP failures are retried by the Outbox pipeline up to `MaxRetryCount`. Messages that exhaust the retry limit are moved to the dead-letter store.

\* \*\*Audit trail:\*\* `LoggedEmailTransport` records delivery attempts in the `NotificationLog` table.

\* \*\*Internal preference checking:\*\* The channel checks notification preferences, so integration event handlers do not need to implement that concern.

\* \*\*No dispatcher:\*\* The concrete handler depends only on `IEmailChannel`; it does not depend on a generic notification dispatcher.



\---



\## 3. Application Layer — Contracts and Models



All contracts are defined under:



```text

Application/Shared/Communication/Notifications/

```



They are abstractions that keep SMTP, MailKit, and template-rendering details out of the Application layer.



\### 3.1 Core Abstractions



| Contract             | Purpose                                                                                                                                                                       |

| -------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |

| `IEmailChannel`      | Entry point for email notifications. Its implementation loads preferences, composes the message, and delivers it through a transport. Handlers depend only on this interface. |

| `INotificationModel` | Marker interface with a `TemplateName` property. Notification models implement it to identify the template used to render their content.                                      |



\*\*Placement:\*\*



\* Channel interfaces: `Application/Shared/Communication/Notifications/Channels/`

\* Marker and model interfaces: `Application/Shared/Communication/Notifications/Abstractions/`



\### 3.2 Typed Email Models



Each model is a simple POCO implementing `INotificationModel`, with a stable `TemplateName` that identifies its Razor template.



| Model                           | `TemplateName`               | Purpose                                        |

| ------------------------------- | ---------------------------- | ---------------------------------------------- |

| `OrderConfirmationEmail`        | `"OrderConfirmation"`        | Sent when an order is placed.                  |

| `PasswordResetEmail`            | `"PasswordReset"`            | Sent when a user requests a password reset.    |

| `WelcomeEmail`                  | `"Welcome"`                  | Sent after successful registration.            |

| `RegistrationVerificationEmail` | `"RegistrationVerification"` | Sent during onboarding for email verification. |

| `FeedbackRequestEmail`          | `"FeedbackRequest"`          | Sent after delivery to request feedback.       |



Models reside in:



```text

Application/Shared/Communication/Notifications/Models/

```



\### 3.3 Generic Wrapper



```csharp

public sealed class NotificationRequest<T>

&#x20;   where T : INotificationModel

{

&#x20;   public Guid UserId { get; init; }



&#x20;   public T Model { get; init; } = default!;

}

```



The wrapper binds the recipient's `UserId` to a typed model.



\* `UserId` identifies the user whose notification preferences should be checked.

\* `Model` carries the content needed to construct the email.



The channel resolves the recipient's email address from the model or associated application data, according to the implementation. The wrapper's `UserId` remains the authoritative identifier for preference lookup.



\---



\## 4. Infrastructure Layer — Email Channel Components



Concrete implementations live under:



```text

Infrastructure/Communication/Notifications/

```



\### 4.1 `EmailChannel`



\*\*File:\*\* `Infrastructure/Communication/Notifications/Channels/EmailChannel.cs`

\*\*Implements:\*\* `IEmailChannel`



Responsibilities:



\* Loads user preferences through `INotificationPreferencesRepository`.

\* Returns without sending if `AllowEmail` is `false`.

\* Calls `EmailComposer.ComposeAsync(model)` to construct the `EmailMessage`.

\* Passes the composed message to `IEmailTransport.SendAsync(emailMessage)`.



The channel is a dedicated email service. It hides preference loading and message composition from integration event handlers and contains no generic broadcast logic.



\### 4.2 `EmailComposer`



\*\*File:\*\* `Infrastructure/Communication/Notifications/Providers/Email/Composers/EmailComposer.cs`



Responsibilities:



\* Uses `RazorTemplateRenderer` to render the `.cshtml` template identified by `model.TemplateName`.

\* Extracts the recipient address and subject from the model.

\* Builds and returns an `EmailMessage` DTO containing the recipient, subject, body, and HTML-formatting information.



The composer is the component responsible for constructing the email message. The transport is responsible for delivery.



\*\*Composer extraction — verify:\*\* How the composer reads the recipient and subject depends on the actual model interface.



\* If the composer uses reflection, the limitations described in §12.1 apply.

\* If the model exposes strongly typed properties, or implements a dedicated `IEmailNotificationModel` interface, extraction can be compile-checked and the reflection limitation does not apply.



Verify the implementation against `EmailComposer.cs`.



\### 4.3 `SmtpEmailTransport`



\*\*File:\*\* `Infrastructure/Communication/Notifications/Providers/Email/Transport/SmtpEmailTransport.cs`

\*\*Implements:\*\* `IEmailTransport`



Responsibilities:



\* Configures a `MimeMessage` from the `EmailMessage` DTO.

\* Connects to an SMTP server using MailKit.

\* Authenticates when credentials are provided.

\* Sends the email and disconnects.

\* Propagates delivery failures to the caller, allowing the Outbox pipeline to record the failure and retry the message.



Configuration is injected through `IOptions<EmailOptions>`, bound to the `Email` configuration section.



\### 4.4 `LoggedEmailTransport` — Audit Decorator



\*\*File:\*\* `Infrastructure/Communication/Notifications/Providers/Email/Transport/LoggedEmailTransport.cs`



`LoggedEmailTransport` decorates `IEmailTransport` and records send attempts in the `NotificationLog` table.



Responsibilities:



\* On success, writes a `NotificationLog` row with a `Sent` outcome.

\* On failure, writes a row with a `Failed` outcome and the associated error details.

\* Delegates delivery to the inner transport.



The decorator is applied through Scrutor in `NotificationsInfrastructureExtensions`.



\*\*Operational consideration:\*\* Audit persistence must not unintentionally hide the original SMTP failure. Verify how exceptions from both the inner transport and audit write are handled.



\### 4.5 `IEmailTransport` — Transport Interface



\*\*File:\*\* `Infrastructure/Communication/Notifications/Providers/Email/Transport/IEmailTransport.cs`



```csharp

public interface IEmailTransport

{

&#x20;   Task SendAsync(

&#x20;       EmailMessage message,

&#x20;       CancellationToken cancellationToken = default);

}

```



This interface allows the transport implementation to be replaced—for example, switching from SMTP to SendGrid—without changing the channel or its consumers.



\### 4.6 `EmailMessage` — Message DTO



\*\*File:\*\* `Infrastructure/Communication/Notifications/Messages/EmailMessage.cs`



Expected shape:



```csharp

public sealed class EmailMessage

{

&#x20;   public string To { get; init; } = string.Empty;



&#x20;   public string Subject { get; init; } = string.Empty;



&#x20;   public string Body { get; init; } = string.Empty;



&#x20;   public bool IsHtml { get; init; } = true;

}

```



\*\*Verification required:\*\* Confirm the actual property set and mutability against the implementation. The real DTO may differ, for example by including `Cc`, `Bcc`, or `ReplyTo`.



\---



\## 5. Templates



Email templates are Razor views (`.cshtml` files) located under:



```text

Infrastructure/Communication/Notifications/Templates/Email/

```



They are rendered at runtime by `RazorTemplateRenderer`.



| Template file                     | Used by                         |

| --------------------------------- | ------------------------------- |

| `OrderConfirmation.cshtml`        | `OrderConfirmationEmail`        |

| `PasswordReset.cshtml`            | `PasswordResetEmail`            |

| `Welcome.cshtml`                  | `WelcomeEmail`                  |

| `RegistrationVerification.cshtml` | `RegistrationVerificationEmail` |

| `FeedbackRequest.cshtml`          | `FeedbackRequestEmail`          |



Templates have access to the properties of their typed model.



\### Example



```html

<html>

<body>

&#x20;   <h1>Order #@Model.OrderId Confirmed</h1>

&#x20;   <p>Dear @Model.CustomerName,</p>

&#x20;   <p>Your order total is @Model.Total</p>

</body>

</html>

```



This is an illustrative example; the property names must match the actual model.



\*\*Note on formatted values:\*\* If `Model.Total` is a `Money` value object rather than a `decimal`, render its amount and currency explicitly—for example, `@Model.Total.Amount @Model.Total.Currency`—instead of assuming that `@Model.Total.ToString("C")` is supported.



If `Total` is a `decimal`, `ToString("C")` produces a culture-formatted currency string.



Verify the property's type and available formatting methods before relying on a particular template expression.



Templates are deployed alongside the application. Whether they can be updated without recompilation depends on how the application packages and resolves Razor views; verify the actual deployment configuration before treating runtime template updates as a guarantee.



\---



\## 6. Configuration



Email settings are read from configuration and mapped to `EmailOptions`.



\### 6.1 `EmailOptions` — Infrastructure



\*\*File:\*\* `Infrastructure/Communication/Notifications/Options/EmailOptions.cs`



The options class exposes SMTP connection parameters and sender identity. The following is an \*\*expected example\*\*, not a verified representation of the actual implementation:



```csharp

public sealed class EmailOptions

{

&#x20;   public string Host { get; init; } = "localhost";



&#x20;   public int Port { get; init; } = 587;



&#x20;   public bool UseSsl { get; init; } = false;



&#x20;   public string Username { get; init; } = string.Empty;



&#x20;   public string Password { get; init; } = string.Empty;



&#x20;   public string SenderName { get; init; } = "E-Commerce";



&#x20;   public string SenderEmail { get; init; } = "noreply@example.com";

}

```



\*\*Verify the actual property names\*\* against `EmailOptions.cs` before copying the configuration example below. If the implementation differs, update the class or the JSON example as appropriate.



\### 6.2 Example `appsettings.json`



```json

{

&#x20; "Email": {

&#x20;   "Host": "smtp.example.com",

&#x20;   "Port": 587,

&#x20;   "UseSsl": true,

&#x20;   "Username": "user@example.com",

&#x20;   "Password": "",

&#x20;   "SenderName": "E-Commerce Store",

&#x20;   "SenderEmail": "noreply@example.com"

&#x20; }

}

```



In production, credentials must be supplied through environment variables or a secret manager. \*\*Never commit SMTP credentials or other secrets to source control.\*\*



\### 6.3 Validation and Fallback



Configuration validation should verify the required SMTP settings at startup when the SMTP transport is active.



If SMTP is not configured and a logged fallback transport is enabled, the system may write composed messages to the application log instead of sending them. This behavior depends on the actual fallback implementation and configuration; see §12.2.



A logged fallback is useful for development and testing, but it is not successful email delivery and must not be treated as such in production monitoring.



\---



\## 7. Integration Event Handlers and Outbox



\### 7.1 Handler Example



Handlers depend directly on `IEmailChannel`. They construct a typed email model, wrap it in `NotificationRequest<T>`, and call `SendAsync`.



```csharp

public class SendOrderConfirmationEmailHandler

&#x20;   : IIntegrationEventHandler<OrderPlacedIntegrationEvent>

{

&#x20;   private readonly IEmailChannel \_emailChannel;



&#x20;   public SendOrderConfirmationEmailHandler(

&#x20;       IEmailChannel emailChannel)

&#x20;       => \_emailChannel = emailChannel;



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



\*\*Verification required:\*\* The fields used to populate the model—`CustomerEmail`, `CustomerName`, `OrderId`, and `TotalAmount`—must match the actual `OrderPlacedIntegrationEvent` contract. If the event exposes different fields or nested objects, adjust the handler accordingly.



The handler is automatically discovered and wrapped by the idempotency decorator at startup. It does not require manual registration or contain its own idempotency logic.



\### 7.2 Outbox and Idempotency



The processing sequence is:



1\. The integration event is written to the Outbox atomically with the business transaction.

2\. `OutboxProcessingJob` delivers the event to the integration-event handler pipeline.

3\. `IdempotentIntegrationEventHandler<T>` checks the `(EventId, HandlerIdentifier)` key in `ProcessedEvents` before invoking the concrete handler.

4\. On successful handler execution, the decorator records the processed marker.

5\. On retry, handlers whose successful markers exist are skipped.



This provides \*\*sequential per-handler deduplication across retries\*\*, not transactional exactly-once delivery across the database and external SMTP system. See §9 for the full idempotency model.



\---



\## 8. Error Handling and Retries



| Scenario                                          | Behavior                                                                                                                                                                                         |

| ------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |

| SMTP server unreachable                           | `SmtpEmailTransport` throws. The Outbox message is marked as failed and retried according to the Outbox processing policy.                                                                       |

| Authentication failure                            | The message is retried until the configured limit is reached, then dead-lettered.                                                                                                                |

| Transient network issue                           | The Outbox pipeline retries the message according to its configured processing policy.                                                                                                           |

| Invalid email address or permanent SMTP rejection | The message may be retried until the limit is reached, then dead-lettered. Permanent failures should be identifiable for investigation rather than assumed to become successful through retries. |

| Template-rendering error                          | The exception propagates from the composer. The message is marked as failed and retried unless the error-handling policy classifies it otherwise.                                                |



The retry limit is configurable through `OutboxOptions.MaxRetryCount`, with a documented default of `5`.



After the retry limit is reached, the message is moved to the `DeadLetterMessages` table. `DeadLetterMonitorJob` logs a warning when dead-lettered messages exist.



\### 8.1 Retry Scope



The Outbox retries \*\*messages, not batches\*\*.



On each cycle, `GetPendingMessagesAsync` returns a batch of messages, and each message is processed independently.



Because idempotency is keyed on `(EventId, HandlerIdentifier)`, a message with multiple handlers can be retried without rerunning handlers whose successful completion has already been recorded. Handlers that failed or were not reached can execute again.



This behavior depends on the processed markers being persisted successfully and on the retry path using the same event and handler identifiers.



\---



\## 9. Idempotency and Duplicate Prevention



\### 9.1 Marker Design



\* Each handler's progress for each event is recorded independently in `ProcessedEvents`.

\* The composite key is `(EventId, HandlerIdentifier)`.

\* `HandlerIdentifier` is the fully qualified CLR type name of the concrete handler.

\* Before a handler executes, the idempotency decorator checks whether the marker exists.

\* After the handler succeeds, the decorator records the marker.



\### 9.2 Sequential Retry Behavior



Consider a message with two handlers, where the first succeeds and the second fails during the first attempt.



```text

Attempt 1:



&#x20;   Handler A -> succeeds -> marker inserted



&#x20;   Handler B -> throws

&#x20;       |

&#x20;       v

&#x20;   OutboxProcessingJob marks the message Failed

&#x20;   RetryCount = 1





Attempt 2:



&#x20;   Handler A -> marker exists -> skipped



&#x20;   Handler B -> no marker -> executes

&#x20;                          -> succeeds

&#x20;                          -> marker inserted

&#x20;       |

&#x20;       v

&#x20;   OutboxProcessingJob marks the message Processed

```



Handlers whose successful processing has been recorded are not re-executed. This is the primary idempotency guarantee.



\### 9.3 Mark-Then-Crash Window



A fundamental limitation exists in non-transactional idempotency schemes: the external side effect (sending the email) and the processed-marker insert cannot be committed atomically across independent systems.



If the process crashes after SMTP accepts the email but before the `ProcessedEvents` marker is committed:



\* The email may already have been accepted by the SMTP server.

\* The processed marker does not exist.

\* On retry, the decorator sees no marker and executes the handler again.

\* The email may be sent a second time.



The pipeline cannot eliminate this window by itself.



For critical external side effects, use provider-supported idempotency where available. Email generally does not provide a universal mechanism for recipient-side deduplication based solely on a message identifier. A stable message identifier can assist with correlation and troubleshooting, but it does not guarantee that receiving systems will deduplicate messages.



For most transactional email, the system must explicitly accept the possibility of occasional duplicate delivery rather than risk losing the notification.



\### 9.4 Duplicate Marker Insert Race



If two workers process the same event simultaneously, both may pass the `IsProcessedAsync` check and attempt to insert the same `(EventId, HandlerIdentifier)` row.



The composite primary key rejects the duplicate insert.



According to the documented implementation, `ProcessedEventRepository.MarkAsProcessedAsync` catches this case, detaches the failed entity, and treats the outcome as success to avoid a spurious message failure.



\*\*Important:\*\* This race handling protects the processed-marker table from duplicate rows. It does not prevent both workers from executing the external email side effect before either marker is committed. Concurrent execution therefore requires additional protection if duplicate sends are unacceptable.



\---



\## 10. Testing



\### 10.1 Unit Tests



\* \*\*`EmailChannel`:\*\* Use a mock `EmailComposer` and mock `IEmailTransport`. Verify the `AllowEmail` preference check and delegation behavior.

\* \*\*`EmailComposer`:\*\* Use a fake `RazorTemplateRenderer` to verify template rendering and `EmailMessage` construction.

\* \*\*`SmtpEmailTransport`:\*\* Test against a fake SMTP server, such as MailHog or Papercut, or isolate the underlying SMTP client behind a testable abstraction.

\* \*\*`LoggedEmailTransport`:\*\* Verify that a `NotificationLog` row is created for successful and failed send attempts.



\### 10.2 Integration Tests



\* \*\*Full flow:\*\* Place an order, execute the Outbox processing flow, and verify that the mock transport received the expected email and that the `ProcessedEvents` marker was recorded.

\* \*\*Idempotency:\*\* Redeliver the same integration event after its handler has been successfully marked as processed. Verify that the transport is not called again.

\* \*\*Preference short-circuit:\*\* Disable email in the user's preferences and verify that the transport is not called.

\* \*\*Retry:\*\* Configure the transport to fail and verify that the message is marked as failed. Then configure it to succeed and verify that processing completes successfully.



\### 10.3 End-to-End Tests



\* Use a real SMTP test server, such as MailHog or Papercut, to inspect the outgoing email.

\* Verify that the rendered HTML contains the expected values from the model.

\* Verify that SMTP failures are reflected in the audit trail and that exhausted retries are visible through dead-letter monitoring.



\---



\## 11. Operational Behavior and Monitoring



\### 11.1 Logging



`LoggedEmailTransport` records send attempts using structured fields.



The exact field set is defined by the `NotificationLog` entity. Expected fields include:



\* `Channel = "Email"`

\* `Provider = "Smtp"`

\* Recipient

\* Outcome (`Sent` / `Failed`)

\* Error message, when applicable

\* Elapsed time

\* Timestamp



\*\*Verification required:\*\* Confirm the actual field names against:



```text

Infrastructure/Communication/Notifications/Entities/NotificationLog.cs

```



Audit records describe transport attempts. A successful SMTP send indicates acceptance by the SMTP server, not proof that the message reached the recipient's inbox.



\### 11.2 Metrics



The following metrics are recommended but are not currently emitted according to this document:



```text

notifications\_sent\_total{channel="Email", provider="Smtp"}



notifications\_failed\_total{channel="Email", provider="Smtp"}



notification\_delivery\_duration\_seconds{channel="Email"}

```



See the Outbox Operational Setup Guide for shared monitoring and operational considerations.



\### 11.3 Health Checks



No dedicated email health check is implemented.



SMTP problems are currently observed through transport failures and the `NotificationLog` audit trail. Persistent failures can indicate connectivity, authentication, or provider issues.



A future `EmailHealthCheck` could periodically test SMTP connectivity and credentials without sending a real notification. This is a potential enhancement, not an implemented capability.



See §12 for limitations and future enhancements.



\### 11.4 Dead-Letter Monitoring



`DeadLetterMonitorJob` logs a warning whenever dead-lettered messages exist.



Email integration events use the shared Outbox dead-letter mechanism. There is no separate email-specific dead-letter pipeline.



Dead-lettered messages require operational investigation and an appropriate recovery or replay procedure; logging their existence alone does not restore delivery.



\---



\## 12. Limitations and Future Enhancements



| Limitation                                                    | Impact                                                                                                               | Possible mitigation                                                                                                  |

| ------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------- |

| No email-open tracking                                        | Engagement cannot be measured through open events.                                                                   | Add a tracking pixel if appropriate for the product and privacy requirements. Outside the current subsystem's scope. |

| No email-link click tracking                                  | Click-through activity cannot be measured.                                                                           | Add link wrapping and tracking where appropriate. Outside the current subsystem's scope.                             |

| No dedicated SMTP health check                                | SMTP failures are surfaced through the audit trail and processing failures rather than a dedicated health indicator. | Add a future `EmailHealthCheck` that periodically tests connectivity.                                                |

| Single SMTP provider                                          | There is no provider failover if the primary provider becomes unavailable.                                           | Add a composite transport that supports multiple providers and an explicit failover policy.                          |

| No email batching                                             | Each email is sent individually.                                                                                     | Introduce digest generation or batching where the business use case supports it.                                     |

| No channel-level rate limiting                                | A burst of events may cause a burst of SMTP sends.                                                                   | Add rate limiting in the transport or an upstream execution mechanism.                                               |

| Possible duplicate delivery during the mark-then-crash window | A message may be sent again if the process crashes before the processed marker is committed.                         | Use provider-supported idempotency where available; otherwise, document and accept the residual risk for email.      |



\### 12.1 Reflection in the Composer — Verify



If `EmailComposer` uses reflection to extract the recipient and subject from a model, this introduces potential fragility:



\* Renaming a property may break extraction.

\* Adding a model without the expected property may cause a runtime failure.

\* The compiler cannot verify the extraction.



A possible improvement is to require an explicit interface, such as:



```csharp

public interface IEmailNotificationModel : INotificationModel

{

&#x20;   string RecipientEmail { get; }



&#x20;   string Subject { get; }

}

```



Every email model would implement this interface, making the required properties compile-checked.



\*\*Verify first:\*\* If the composer already uses a strongly typed interface or another compile-checked mechanism, this limitation does not apply and should be removed.



\### 12.2 Logged Fallback



When SMTP credentials are absent and a logged fallback is enabled, the channel may use `LoggedEmailTransport` or an equivalent fallback to record the composed message in application logs instead of sending it.



This can support local development and testing without real SMTP credentials.



Verify the actual fallback implementation and its configuration in `EmailOptions` and the notification infrastructure registration.



\*\*Production warning:\*\* A logged fallback must not be mistaken for successful delivery. Production configuration should make fallback behavior explicit and observable.



\---



\## 13. Summary



The Email pillar provides:



\* \*\*Durable notification intent:\*\* The Transactional Outbox records the intent to send an email atomically with the business transaction.

\* \*\*Per-handler idempotency:\*\* Successful handler executions are skipped on subsequent retries when their processed markers have been committed.

\* \*\*Clean separation of concerns:\*\* The Application layer depends on `IEmailChannel`, while Infrastructure owns SMTP, templates, composition, and transports.

\* \*\*Auditability:\*\* `LoggedEmailTransport` records delivery attempts in `NotificationLog`.

\* \*\*Retry and dead-lettering:\*\* The shared Outbox infrastructure handles retries and quarantines messages that exhaust their retry limit.



\### 13.1 Delivery Guarantee — Precisely Stated



The pipeline provides \*\*at-least-once processing with sequential per-handler deduplication\*\*.



It does not provide exactly-once delivery across the database, SMTP server, and recipient's inbox. A crash after an external side effect but before the processed marker commits can result in duplicate delivery.



The system durably records notification intent, retries processing failures, and quarantines exhausted messages for investigation. These mechanisms improve reliability and visibility but cannot guarantee that every email reaches its final recipient.



\### 13.2 Architectural Principle



The database is the source of truth for the intent to send. The email channel is a delivery mechanism, not a system of record.



The Outbox preserves the integration event that initiated the notification. The processing and audit records provide operational visibility into subsequent attempts and outcomes.



\*\*Durable intent, observable processing, and explicit failure handling are the guarantees. Successful arrival in the recipient's inbox is not.\*\*



\---



\*\*Document version:\*\* 2.0 — Final Email Notification Pillar.



This version standardizes the delivery guarantee as at-least-once processing with sequential per-handler deduplication; removes the implication of literal exactly-once delivery; documents the mark-then-crash window and duplicate-marker race; clarifies per-message retry behavior; moves the unimplemented email health check to future enhancements; updates folder paths from `External/` to `Providers/`; and flags unverified implementation details, including `EmailOptions`, `EmailMessage`, `NotificationLog`, and integration-event property names.



