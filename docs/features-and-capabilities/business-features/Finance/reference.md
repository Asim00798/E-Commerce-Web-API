\# Finance Feature — Technical Reference Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Scope:\*\* Payments, Refunds, Paymob Integration, Reconciliation

\*\*Version:\*\* 1.1 — Final



\---



\## 1. Overview



The Finance bounded context manages payments, refunds, and financial transactions. It is a true Domain bounded context with real business invariants, state transitions, and domain events.



The feature provides:



\* Provider-agnostic Payment and Refund aggregates.

\* Integration with Paymob using the current Intention API.

\* Webhook processing with HMAC verification and typed error handling.

\* Asynchronous refund processing.

\* Recurring reconciliation for stuck payments and refunds.

\* Durable integration events through the existing Outbox.

\* Complete separation between Application orchestration and Infrastructure provider implementation.



The Domain layer has zero Paymob-specific knowledge. Paymob is isolated behind Infrastructure adapters.



\*\*Version note:\*\* This revision reflects the applied corrections from the Finance remediation. Payment initiation now resolves authoritative fields from Ordering; the HMAC canonical string and source are corrected; payment reconciliation filters on intention ID; refund reconciliation no longer requeues unknown-outcome refunds; concurrent over-refund is prevented by a unique index and an outstanding-balance check; resource authorization is enforced on refund and status handlers; `PaymobApiClient` is a process-wide Singleton; and `RowVersion` is configured as a shadow property.



\---



\## 2. Architecture and Placement



Finance is a Domain bounded context located at:



```text

Domain/BoundedContexts/Core/Finance

```



Application use cases live under:



```text

Application/BoundedContexts/Finance

```



The cross-context shared contract for order pricing lives under:



```text

Application/Shared/Orders

```



Infrastructure Paymob integration and persistence live under:



```text

Infrastructure/Payment

Infrastructure/Persistence/Modules/Finance

```



API controllers and DTOs live under:



```text

API/Controllers/v1/Payments

API/DTOs/v1/Payments

```



\### 2.1 Layer Responsibilities



| Layer              | Responsibility                                                                                                                                        |

| ------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------- |

| \*\*Domain\*\*         | Payment aggregate, Refund aggregate, value objects, domain events, repository interfaces                                                              |

| \*\*Application\*\*    | Commands, queries, handlers, webhook abstraction, integration events, reconciliation workflows, `IOrderPricingReader` contract                        |

| \*\*Infrastructure\*\* | Paymob API client, HMAC verifier, gateway, EF Core persistence, repository implementations, `IOrderPricingReader` implementation on the Ordering side |

| \*\*API\*\*            | Thin HTTP controllers, request/response DTOs, raw webhook body handling                                                                               |



\### 2.2 Provider Isolation



Application depends on `IPaymentGateway` and `IPaymentWebhookProcessor`. It does not reference Paymob.



```text

Application

&#x20;   |

&#x20;   v

IPaymentGateway / IPaymentWebhookProcessor

&#x20;   |

&#x20;   v

Infrastructure

&#x20;   |

&#x20;   +-- Paymob

```



\---



\## 3. Processing Model



\### 3.1 Payment Initiation — Synchronous



1\. The client sends `POST /api/payments/initiate` with `OrderId`, `Method`, `ReturnUrl`, `CancelUrl`, and `IdempotencyKey`.

2\. `InitiatePaymentCommandHandler` resolves authoritative pricing through `IOrderPricingReader`, implemented by Ordering.

3\. The handler verifies ownership. Unless the caller is an Administrator or Support user, `snapshot.CustomerId` must equal `ICurrentUser.UserId`.

4\. The handler checks for an existing Payment for the Order. If one exists, it returns a business failure: `"Payment already initiated for this order"`.

5\. Payment is created in the `Pending` state and persisted.

6\. `IPaymentGateway.InitiatePaymentAsync` is called synchronously to create a Paymob Intention.

7\. Payment transitions to `AwaitingPayment`, and `ProviderIntentionId` is persisted.

8\. The checkout URL is returned immediately.



The server does not wait for the customer to complete payment.



\### 3.2 Payment Confirmation — Asynchronous



1\. The customer completes payment on Paymob.

2\. Paymob sends a webhook with the HMAC as a query parameter (`?hmac=...`).

3\. `PaymobWebhookController` reads the raw request body and binds HMAC from the query, with header fallback.

4\. `IPaymentWebhookProcessor` verifies HMAC, deserializes the callback, and dispatches `HandlePaymentWebhookCommand`.

5\. `HandlePaymentWebhookCommandHandler` correlates the callback using `ProviderIntentionId`.

6\. Payment transitions to `Captured` or `Failed`, or the handler returns `Transient` if Payment is still `Pending`, allowing the provider to retry.

7\. The domain event is translated into an integration event and written to the Outbox.



\### 3.3 Refund Processing — Asynchronous



1\. `RequestRefundCommand` creates a Refund in `Requested`. Ownership is verified, the outstanding balance is checked, and a filtered unique index backs the same-amount case at the database level.

2\. Refund is persisted.

3\. `ProcessRefundJob` is enqueued.

4\. `ProcessRefundCommandHandler` atomically claims the refund through `TryMarkProcessingAsync`.

5\. The provider refund is executed through `IPaymentGateway.RefundAsync`.

6\. Refund completes, fails, or remains `Processing` if the outcome is unknown.

7\. On completion, `Payment.ApplyRefund` updates the payment's refunded amount and status.



\### 3.4 Reconciliation — Background Recovery



\* `ReconcilePaymentsJob` finds stale `AwaitingPayment` payments with a `ProviderIntentionId` and asks Paymob for their status using the intention ID.

\* `ReconcileRefundsJob` performs two phases:



&#x20; 1. \*\*Stale Requested recovery:\*\* Re-enqueues `ProcessRefundJob` for refunds whose original enqueue was lost.

&#x20; 2. \*\*Stuck Processing resolution:\*\* Queries Paymob using the parent payment's transaction ID to read `is\_refunded`.



Both are recurring jobs with automatic discovery and scheduling.



\---



\## 4. Domain Model



\### 4.1 Payment Aggregate



Payment owns the payment lifecycle and financial state.



\*\*States:\*\*



```text

Pending

AwaitingPayment

Captured

Failed

PartiallyRefunded

Refunded

```



\*\*Key methods:\*\*



\* `Create`

\* `AssignProviderIntention`

\* `Capture`

\* `Fail`

\* `CanApplyRefund`

\* `ApplyRefund`



\*\*Note:\*\* The `Cancelled` state and `Cancel()` method were removed as dead code because no caller existed. Cancellation is a business outcome that occurs at the Ordering layer, not at the Finance Payment layer.



\### 4.2 Refund Aggregate



Refund owns the refund lifecycle.



\*\*States:\*\*



```text

Requested

Processing

Completed

Failed

```



\*\*Key methods:\*\*



\* `Create(paymentId, orderId, amount, reason)`

\* `MarkProcessing`

\* `SetProviderTransactionId`

\* `Complete`

\* `Fail`

\* `Requeue` — retained on the aggregate but not called by reconciliation. See §9.2.



\*\*Denormalization:\*\* Refund carries `OrderId` alongside `PaymentId`. `OrderId` is set at creation from the loaded Payment. This lets refund domain event handlers publish integration events with order context without querying `IPaymentRepository`.



\### 4.3 Value Objects



\* `Money` — reused from SharedKernel.

\* `PaymentMethod` — Finance-specific value object.



\### 4.4 Domain Events



\*\*Payment events:\*\*



\* `PaymentCapturedDomainEvent`

\* `PaymentFailedDomainEvent`



`PaymentCancelledDomainEvent` was removed alongside the `Cancelled` state.



\*\*Refund events:\*\*



\* `RefundCompletedDomainEvent` — carries `RefundId`, `PaymentId`, `OrderId`, and `Amount`.

\* `RefundFailedDomainEvent` — carries `RefundId`, `PaymentId`, `OrderId`, `Amount`, and `Reason`.



These domain events represent meaningful business outcomes. Integration events are Application contracts.



\---



\## 5. Application Layer



\### 5.1 Abstractions



\*\*`IPaymentGateway`:\*\*



\* `InitiatePaymentAsync`

\* `GetPaymentStatusAsync` — accepts a reference with `TransactionId` or `IntentionId`.

\* `RefundAsync`

\* `GetRefundStatusAsync` — queries the parent payment transaction.



\*\*`IPaymentWebhookProcessor`:\*\*



\* `ProcessAsync(provider, payload, signature)`



\*\*`IOrderPricingReader`\*\* — shared with Ordering:



\* `GetByOrderIdAsync(orderId)` — returns `OrderPricingSnapshot`.



\### 5.2 Commands



| Command                       | Purpose                                                                                                                 |

| ----------------------------- | ----------------------------------------------------------------------------------------------------------------------- |

| `InitiatePaymentCommand`      | Creates a payment intention synchronously. Carries `OrderId`, `Method`, `ReturnUrl`, `CancelUrl`, and `IdempotencyKey`. |

| `HandlePaymentWebhookCommand` | Processes a provider webhook.                                                                                           |

| `RequestRefundCommand`        | Creates a refund and enqueues processing.                                                                               |

| `ProcessRefundCommand`        | Processes a refund asynchronously.                                                                                      |

| `ReconcilePaymentsCommand`    | Reconciles stuck payments.                                                                                              |

| `ReconcileRefundsCommand`     | Reconciles stuck refunds and recovers stale `Requested` refunds.                                                        |



\### 5.3 Queries



| Query                   | Purpose                                                    |

| ----------------------- | ---------------------------------------------------------- |

| `GetPaymentStatusQuery` | Returns the current payment status and enforces ownership. |



\### 5.4 Integration Events



\* `PaymentCompletedIntegrationEvent` — `PaymentId`, `OrderId`, `Amount`, `Currency`, `ProviderTransactionId`.

\* `PaymentFailedIntegrationEvent` — `PaymentId`, `OrderId`.

\* `RefundCompletedIntegrationEvent` — `RefundId`, `PaymentId`, `OrderId`, `Amount`, `Currency`.

\* `RefundFailedIntegrationEvent` — `RefundId`, `PaymentId`, `OrderId`, `Amount`, `Currency`, `Reason`.



`RefundFailedIntegrationEvent` carries `OrderId` for operational observability. Ordering's handler logs it; no state change occurs.



\### 5.5 Jobs



\*\*Fire-and-forget:\*\*



\* `ProcessRefundJob`

\* `ProcessRefundJobHandler`



\*\*Recurring:\*\*



\* `ReconcilePaymentsJob`, `ReconcilePaymentsJobHandler`, `ReconcilePaymentsTrigger` — cron: `\*/30 \* \* \* \*`.

\* `ReconcileRefundsJob`, `ReconcileRefundsJobHandler`, `ReconcileRefundsTrigger` — cron: `\*/15 \* \* \* \*`.



\---



\## 6. Infrastructure Layer



\### 6.1 Paymob Integration



Infrastructure contains:



\* \*\*`PaymobApiClient`\*\* — authenticates, creates intentions, queries status by transaction or intention ID, initiates refunds, and queries refund status through the parent payment transaction. Registered as a \*\*Singleton\*\* so the token cache is process-wide.

\* \*\*`PaymobPaymentGateway`\*\* — implements `IPaymentGateway`. Dispatches status queries by available identifier. Classifies refund outcomes, with transient 4xx mapped to `Unknown`.

\* \*\*`PaymobHmacVerifier`\*\* — verifies webhook HMAC using the current Paymob canonical field list.

\* \*\*`PaymobWebhookProcessor`\*\* — implements `IPaymentWebhookProcessor`.

\* \*\*`PaymobCurrencyConverter`\*\* — converts `Money` to minor units.

\* \*\*`PaymobPaymentMapper`\*\* — maps provider responses to Application models.

\* \*\*`PaymobApiException`\*\* — provider-specific transport exception.



\*\*API models:\*\*



\* `CreateIntentionRequest`

\* `CreateIntentionResponse`

\* `PaymobAuthResponse`

\* `PaymobRefundResponse`

\* `PaymobStatusResponse` — extended with `IsRefunded` and `RefundedAmountCents`.



`PaymobRefundStatusResponse` was removed because Paymob does not expose a dedicated refund-status endpoint. Refund status is read from the parent payment transaction through `is\_refunded`.



\### 6.2 Persistence



\*\*EF Core configurations:\*\*



\* `PaymentConfiguration`

\* `PaymentTransactionConfiguration`

\* `RefundConfiguration` — includes a filtered unique index on `(PaymentId, Amount, Currency)` where `Status <> 'Failed'`. This was added through a migration because EF Core does not support composite indexes mixing owner and owned-type properties.



\*\*Repositories:\*\*



\* `PaymentRepository`



&#x20; \* `GetByOrderIdAsync`

&#x20; \* `GetByProviderIntentionIdAsync`

&#x20; \* `GetByProviderTransactionIdAsync`

&#x20; \* `GetAwaitingPaymentWithIntentionOlderThanAsync`

\* `RefundRepository`



&#x20; \* `GetByPaymentIdAndAmountAsync`

&#x20; \* `GetOutstandingByPaymentIdAsync`

&#x20; \* `GetRequestedOlderThanAsync`

&#x20; \* `GetProcessingOlderThanAsync`

&#x20; \* `TryMarkProcessingAsync`



`AppDbContext.Finance.cs` exposes:



\* `DbSet<Payment>`

\* `DbSet<PaymentTransaction>`

\* `DbSet<Refund>`



`RowVersion` is a shadow property on both Payment and Refund. The Domain aggregates do not carry a `RowVersion` field.



\### 6.3 DI Registration



`PaymentInfrastructureExtensions.AddFinancePayment` registers:



\* `PaymobOptions` binding and validator.

\* Named HTTP client `"Paymob"`.

\* `PaymobApiClient` as a \*\*Singleton\*\* through `IHttpClientFactory`.

\* `IPaymentGateway` → `PaymobPaymentGateway` (Scoped).

\* `PaymobHmacVerifier` (Scoped, constructed from `PaymobOptions.WebhookSecret`).

\* `IPaymentWebhookProcessor` → `PaymobWebhookProcessor` (Scoped).



Repositories are auto-discovered by `RepositoryRegistrationExtensions`.



\---



\## 7. API Layer



\### 7.1 Controllers



\*\*`PaymentsController`:\*\*



\* `POST /api/payments/initiate` — accepts `OrderId`, `Method`, `ReturnUrl`, `CancelUrl`, and `IdempotencyKey`.

\* `GET /api/payments/{paymentId}`

\* `POST /api/payments/{paymentId}/refund`



\*\*`PaymobWebhookController`:\*\*



\* `POST /api/payments/webhook/paymob` — reads the raw request body and binds HMAC from the query parameter, falling back to the header.



\### 7.2 DTOs



\*\*Requests:\*\*



\* `InitiatePaymentRequest` — `OrderId`, `Method`, `ReturnUrl`, `CancelUrl`, `IdempotencyKey`. The client does not supply `CustomerId`, `Amount`, or `Currency`.

\* `RequestRefundRequest` — `Amount`, `Currency`, `Reason`.



\*\*Responses:\*\*



\* `PaymentInitiationResponse`

\* `PaymentResponse`

\* `RefundResponse`



\---



\## 8. Folder Structure



```text

Domain/

└── BoundedContexts/

&#x20;   └── Core/

&#x20;       └── Finance/

&#x20;           ├── AggregateRoots/

&#x20;           │   ├── Payment/

&#x20;           │   │   ├── Behaviors/

&#x20;           │   │   │   ├── Payment.cs

&#x20;           │   │   │   ├── Payment.State.cs

&#x20;           │   │   │   ├── Payment.Refund.cs

&#x20;           │   │   │   └── Payment.Validation.cs

&#x20;           │   │   ├── Entities/

&#x20;           │   │   │   └── PaymentTransaction.cs

&#x20;           │   │   ├── Events/

&#x20;           │   │   │   ├── PaymentCapturedDomainEvent.cs

&#x20;           │   │   │   └── PaymentFailedDomainEvent.cs

&#x20;           │   │   └── Exceptions/

&#x20;           │   │       └── PaymentException.cs

&#x20;           │   │

&#x20;           │   └── Refund/

&#x20;           │       ├── Behaviors/

&#x20;           │       │   └── Refund.cs

&#x20;           │       ├── Events/

&#x20;           │       │   ├── RefundCompletedDomainEvent.cs

&#x20;           │       │   └── RefundFailedDomainEvent.cs

&#x20;           │       └── Exceptions/

&#x20;           │           └── RefundException.cs

&#x20;           │

&#x20;           ├── ValueObjects/

&#x20;           │   └── PaymentMethod.cs

&#x20;           │

&#x20;           ├── Enums/

&#x20;           │   ├── PaymentStatus.cs

&#x20;           │   ├── RefundStatus.cs

&#x20;           │   ├── PaymentTransactionType.cs

&#x20;           │   └── PaymentMethodType.cs

&#x20;           │

&#x20;           └── Repositories/

&#x20;               ├── IPaymentRepository.cs

&#x20;               └── IRefundRepository.cs



Application/

├── BoundedContexts/

│   └── Finance/

│       ├── Abstractions/

│       │   ├── IPaymentGateway.cs

│       │   └── IPaymentWebhookProcessor.cs

│       │

│       ├── Commands/

│       │   ├── InitiatePayment/

│       │   ├── HandlePaymentWebhook/

│       │   ├── ProcessRefund/

│       │   ├── ReconcilePayments/

│       │   ├── ReconcileRefunds/

│       │   └── RequestRefund/

│       │

│       ├── Queries/

│       │   └── GetPaymentStatus/

│       │

│       ├── DomainEventHandlers/

│       │   ├── PaymentCapturedDomainEventHandler.cs

│       │   ├── PaymentFailedDomainEventHandler.cs

│       │   ├── RefundCompletedDomainEventHandler.cs

│       │   └── RefundFailedDomainEventHandler.cs

│       │

│       ├── IntegrationEvents/

│       │   ├── PaymentCompletedIntegrationEvent.cs

│       │   ├── PaymentFailedIntegrationEvent.cs

│       │   ├── RefundCompletedIntegrationEvent.cs

│       │   └── RefundFailedIntegrationEvent.cs

│       │

│       ├── Jobs/

│       │   ├── ProcessRefund/

│       │   ├── ReconcilePayments/

│       │   └── ReconcileRefunds/

│       │

│       ├── Models/

│       │   ├── PaymentInitiationRequest.cs

│       │   ├── PaymentInitiationResult.cs

│       │   ├── PaymentProviderReference.cs

│       │   ├── PaymentStatusResult.cs

│       │   ├── PaymentWebhookCommandResult.cs

│       │   ├── RefundResult.cs

│       │   └── RefundStatusResult.cs

│       │

│       └── Dtos/

│           └── PaymentDto.cs

│

└── Shared/

&#x20;   ├── Orders/

&#x20;   │   ├── IOrderPricingReader.cs

&#x20;   │   └── OrderPricingSnapshot.cs

&#x20;   │

&#x20;   └── Security/

&#x20;       └── Authorization/

&#x20;           └── Permissions/

&#x20;               └── FinancePermissions.cs



Infrastructure/

├── Payment/

│   ├── Configuration/

│   │   ├── PaymentOptions.cs

│   │   ├── PaymobOptions.cs

│   │   └── PaymobOptionsValidator.cs

│   │

│   ├── Extensions/

│   │   └── PaymentInfrastructureExtensions.cs

│   │

│   └── Providers/

│       └── Paymob/

│           ├── Api/

│           │   ├── Client/

│           │   │   └── PaymobApiClient.cs

│           │   └── Models/

│           │       ├── CreateIntentionRequest.cs

│           │       ├── CreateIntentionResponse.cs

│           │       ├── PaymobAuthResponse.cs

│           │       ├── PaymobRefundResponse.cs

│           │       └── PaymobStatusResponse.cs

│           │

│           ├── Conversion/

│           ├── Exceptions/

│           ├── Gateway/

│           ├── Mapping/

│           └── Webhooks/

│               ├── PaymobHmacVerifier.cs

│               ├── PaymobWebhookProcessor.cs

│               └── TransactionCallback.cs

│

└── Persistence/

&#x20;   ├── Context/

&#x20;   │   └── AppDbContext.Finance.cs

&#x20;   │

&#x20;   └── Modules/

&#x20;       ├── Finance/

&#x20;       │   ├── Configurations/

&#x20;       │   │   ├── PaymentConfiguration.cs

&#x20;       │   │   ├── PaymentTransactionConfiguration.cs

&#x20;       │   │   └── RefundConfiguration.cs

&#x20;       │   └── Repositories/

&#x20;       │       ├── PaymentRepository.cs

&#x20;       │       └── RefundRepository.cs

&#x20;       │

&#x20;       └── Orders/

&#x20;           └── Services/

&#x20;               └── OrderPricingReader.cs



API/

├── Controllers/

│   └── v1/

│       └── Payments/

│           ├── PaymentsController.cs

│           └── PaymobWebhookController.cs

│

└── DTOs/

&#x20;   └── v1/

&#x20;       └── Payments/

&#x20;           ├── Requests/

&#x20;           │   ├── InitiatePaymentRequest.cs

&#x20;           │   └── RequestRefundRequest.cs

&#x20;           │

&#x20;           └── Responses/

&#x20;               ├── PaymentInitiationResponse.cs

&#x20;               ├── PaymentResponse.cs

&#x20;               └── RefundResponse.cs

```



\---



\## 9. Security and Reliability



\### 9.1 Webhook Security



\* The raw request body is read before processing.

\* HMAC verification happens before semantic validation.

\* HMAC is bound from the query parameter `?hmac=...`, with header fallback.

\* The canonical string uses Paymob's documented 20-field order.

\* `FixedTimeEquals` is used with a length guard to avoid throwing on malformed input.

\* Invalid HMAC returns typed `Unauthorized` (401).

\* Provider-specific HMAC construction stays inside Infrastructure.



\### 9.2 Refund Reliability



\* Refund records a provider transaction ID when available.

\* Unknown-outcome refunds remain `Processing` and are \*\*not requeued\*\*. Requeueing would re-issue a refund against Paymob, which has no refund-idempotency key. Preventing potential money loss takes priority; manual reconciliation is the trade-off.

\* Stale `Requested` refunds, whose jobs were never enqueued, are recovered by `ReconcileRefundsJob` and re-enqueued. The atomic claim makes duplicate enqueues safe.

\* `TryMarkProcessingAsync` uses an atomic conditional `UPDATE` to prevent duplicate concurrent processing.

\* A filtered unique index on `Refunds(PaymentId, Amount, Currency)` where `Status <> 'Failed'` prevents exact-duplicate refund rows at the database level.

\* `RequestRefundCommandHandler` sums outstanding refunds (`Requested` + `Processing`) and rejects new requests that would exceed the remaining refundable balance.



\### 9.3 Payment Reliability



\* Payment reconciliation filters on `ProviderIntentionId != null` and queries Paymob by intention ID.

\* `PaymobPaymentGateway.GetPaymentStatusAsync` dispatches by whichever identifier the reference carries.

\* `RowVersion` is a shadow property on both aggregates; concurrency enforcement is unchanged.

\* The webhook processor does not catch unexpected exceptions; those flow to global middleware.

\* The webhook handler returns `Transient` when Payment is still `Pending`. The provider retries, and on retry the Payment is expected to be `AwaitingPayment`.



\### 9.4 Typed Webhook Errors



Webhook flow uses `PaymentWebhookCommandResult` and `PaymentWebhookErrorType`:



\* `Validation`

\* `Unauthorized`

\* `NotFound`

\* `Conflict`

\* `Transient`

\* `Unexpected`



This avoids string-based error classification.



\### 9.5 Client-Tampered Payment Fields — Eliminated



`InitiatePaymentCommand` no longer carries `Amount`, `Currency`, or `CustomerId` from the client. `InitiatePaymentCommandHandler` resolves all three through `IOrderPricingReader`, which reads the Order aggregate's `Total`, `Currency`, and `CustomerId`.



Ownership is verified unless the caller holds the Administrator or Support role.



\### 9.6 Resource Authorization



`RequestRefundCommandHandler` and `GetPaymentStatusQueryHandler` verify that `payment.CustomerId == currentUser.UserId` unless the caller is privileged.



Both return `"Payment not found"` on ownership mismatch to avoid leaking resource existence.



\---



\## 10. Configuration



```json

{

&#x20; "Payment": {

&#x20;   "Provider": "Paymob",

&#x20;   "Paymob": {

&#x20;     "ApiKey": "",

&#x20;     "SecretKey": "",

&#x20;     "IntegrationId": "",

&#x20;     "BaseUrl": "https://accept.paymob.com/api",

&#x20;     "WebhookSecret": ""

&#x20;   }

&#x20; }

}

```



`PaymobOptionsValidator` validates configuration at startup. `IntegrationId` must parse as an integer; this is enforced at boot, not at first payment.



\*\*Security note:\*\* The values shown above are placeholders. Provider credentials and webhook secrets must be supplied through secure configuration, such as environment variables, .NET User Secrets for local development, or a secrets manager. Do not hardcode real secrets in source control.



\---



\## 11. Workflows



\### 11.1 Payment Initiation



```text

Client

&#x20;  |

&#x20;  v

PaymentsController

&#x20;  |

&#x20;  v

InitiatePaymentCommand

(OrderId, Method, ReturnUrl, CancelUrl, IdempotencyKey)

&#x20;  |

&#x20;  v

IOrderPricingReader.GetByOrderIdAsync

&#x20;  |

&#x20;  v

Ownership check + existing Payment check

&#x20;  |

&#x20;  v

Payment.Create (Pending)

&#x20;  |

&#x20;  v

Paymob Intention (via IPaymentGateway)

&#x20;  |

&#x20;  v

Payment.AssignProviderIntention (AwaitingPayment)

&#x20;  |

&#x20;  v

Return checkout URL

```



\### 11.2 Payment Webhook



```text

Paymob

&#x20;  |

&#x20;  | POST with ?hmac=...

&#x20;  v

PaymobWebhookController

&#x20;  |

&#x20;  v

IPaymentWebhookProcessor

&#x20;  |

&#x20;  v

HMAC verification (20-field canonical string)

&#x20;  |

&#x20;  v

HandlePaymentWebhookCommand

&#x20;  |

&#x20;  v

Correlate by ProviderIntentionId

&#x20;  |

&#x20;  v

Captured / Failed / Transient (if still Pending)

&#x20;  |

&#x20;  v

Domain Event

&#x20;  |

&#x20;  v

Integration Event

&#x20;  |

&#x20;  v

Outbox

```



\### 11.3 Refund



```text

RequestRefundCommand

&#x20;  |

&#x20;  v

Ownership check + outstanding balance check

&#x20;  |

&#x20;  v

Refund.Create(paymentId, orderId, amount, reason) (Requested)

&#x20;  |

&#x20;  v

ProcessRefundJob

&#x20;  |

&#x20;  v

ProcessRefundCommand

&#x20;  |

&#x20;  v

TryMarkProcessingAsync (Processing)

&#x20;  |

&#x20;  v

Paymob Refund

&#x20;  |

&#x20;  v

Completed / Failed / Unknown

&#x20;  |

&#x20;  v

(Unknown: stays Processing; reconciliation resolves via parent transaction)

```



\### 11.4 Reconciliation



```text

Recurring Trigger

&#x20;  |

&#x20;  v

ReconcilePaymentsJob / ReconcileRefundsJob

&#x20;  |

&#x20;  v

Application Command

&#x20;  |

&#x20;  v

Provider Status Inquiry

&#x20;  |

&#x20;  v

Update Aggregate

```



`ReconcileRefundsJob` also performs stale-`Requested` recovery: refunds whose original job was never enqueued are re-enqueued.



\---



\## 12. Testing and Verification



\### Recommended Tests



\* Payment aggregate state transitions.

\* Refund aggregate lifecycle.

\* Money arithmetic and currency checks.

\* `ProcessRefundCommandHandler` atomic claim and provider result handling.

\* `ReconcileRefundsCommandHandler` stale-`Requested` recovery and stuck-`Processing` resolution.

\* `RequestRefundCommandHandler` ownership and outstanding-balance checks.

\* `InitiatePaymentCommandHandler` authoritative pricing resolution.

\* `PaymobPaymentMapper`.

\* `PaymobHmacVerifier` against a real Paymob signature.

\* Webhook processor typed errors.



\### Integration Tests



Integration tests should use real SQL Server or LocalDB for:



\* Unique indexes, including the `IX\_Refunds\_ActiveUnique` filtered index.

\* `RowVersion` concurrency.

\* `TryMarkProcessingAsync` atomic update.

\* `GetOutstandingByPaymentIdAsync`.

\* `GetRequestedOlderThanAsync`.

\* `GetProcessingOlderThanAsync`.



\### End-to-End Tests



\* Initiate payment with tampered client fields; verify that authoritative Order values are used.

\* Webhook success and failure.

\* Duplicate webhook.

\* Refund request.

\* Concurrent same-amount refund requests; verify the second request fails.

\* Refund reconciliation.

\* Payment reconciliation.



\---



\## 13. Operational Expectations



\* Register `services.AddFinancePayment(configuration)`.

\* Apply the EF Core migration for the Finance schema, including the filtered unique index on `Refunds(PaymentId, Amount, Currency)` where `Status <> 'Failed'`.

\* Seed the following permissions:



&#x20; \* `Payments.Read`

&#x20; \* `Payments.Refund`

&#x20; \* `Payments.Manage`

\* Ensure recurring triggers are auto-discovered by `RecurringJobBootstrapper`.

\* Verify Paymob API contracts against current documentation before deployment:



&#x20; \* HMAC canonical field list and order.

&#x20; \* Intention status endpoint existence and response shape.

&#x20; \* Refund endpoint path.

&#x20; \* `is\_refunded` behavior on the parent transaction.

\* Monitor webhook failures, reconciliation outcomes, and manual-reconciliation warnings.

\* Ensure provider credentials are stored through secure configuration.



\---



\## 14. Summary



The Finance feature provides a production-oriented payment and refund system integrated with Paymob through a provider-agnostic abstraction.



\### Key Architectural Properties



\* Domain owns payment and refund invariants.

\* Application orchestrates workflows.

\* Infrastructure isolates Paymob.

\* Payment initiation is synchronous and resolves authoritative fields from Ordering.

\* Payment confirmation is asynchronous through webhooks verified against Paymob's canonical HMAC.

\* Refunds are asynchronous and are never requeued when the outcome is unknown.

\* Concurrent same-amount refunds are prevented by a database-level unique index.

\* Reconciliation provides recovery for stuck payments and stale or stuck refunds.

\* Outbox provides durable integration events.

\* Webhook processing uses typed errors and raw-body verification.

\* `PaymobApiClient` is a process-wide Singleton with a working token cache.



\### Recent Corrections



\* Removed the `PaymentStatus.Cancelled` state and `Payment.Cancel()` as dead code.

\* Refund now carries `OrderId`; refund domain events carry `OrderId`; refund event handlers no longer query `IPaymentRepository`.

\* `RowVersion` is a shadow property on Payment and Refund.

\* `RefundFailedIntegrationEvent` carries `OrderId`.

\* `PaymobRefundStatusResponse` was removed; refund status is read from the parent transaction's `is\_refunded`.

\* Webhook HMAC uses the full 20-field canonical string and query-parameter binding.

\* Payment reconciliation filters on intention ID; the gateway accepts intention-ID lookups.

\* Client-tampered fields are eliminated through `IOrderPricingReader`.

\* Resource authorization is enforced on refund and status handlers.

\* Duplicate payment initiation returns a business failure instead of a 500.



The result is a clean, maintainable, and reliable Finance bounded context aligned with the modular monolith architecture.



\---



\*\*Document version:\*\* 1.1 — Final

\*\*Feature:\*\* Finance / Paymob Integration



