\# Finance Workflow Technical Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Scope:\*\* Payment Initiation, Webhook Confirmation, Refunds, Reconciliation

\*\*Version:\*\* 1.1 — Final



\---



\## 1. Overview



This document defines the complete end-to-end workflows for the Finance bounded context. It covers:



\* Payment initiation using the Paymob Intention API.

\* Payment confirmation through Paymob webhooks.

\* Payment reconciliation for missed or delayed webhooks.

\* Refund requests.

\* Asynchronous refund processing.

\* Refund reconciliation for stuck refunds and stale `Requested` refunds.



The Finance feature is a true Domain bounded context with provider-agnostic aggregates and business invariants. Paymob is isolated behind Infrastructure adapters. The Application layer orchestrates workflows through commands, queries, and background jobs.



All cross-context communication is done through durable integration events via the Outbox.



\*\*Version note:\*\* This revision reflects the applied remediation: authoritative pricing is resolved from Ordering, HMAC verification uses the full Paymob canonical string and query-parameter source, payment reconciliation queries by intention ID, refund reconciliation no longer requeues unknown-outcome refunds, and concurrent same-amount refunds are prevented at the database level.



\---



\## 2. Actors and Components



\### 2.1 Actors



| Actor                      | Description                                                                                        |

| -------------------------- | -------------------------------------------------------------------------------------------------- |

| Client                     | Initiates payment using `OrderId` and redirect URLs, requests refunds, and queries payment status. |

| Ordering                   | Provides authoritative order pricing through `IOrderPricingReader`.                                |

| Paymob                     | External payment provider that sends webhooks and processes refunds.                               |

| API Controller             | Thin HTTP adapter that receives requests and forwards commands.                                    |

| MediatR                    | Command/query pipeline.                                                                            |

| Command Handlers           | Application use cases.                                                                             |

| Domain Aggregates          | Enforce business rules and state transitions.                                                      |

| Repositories               | Persistence abstractions.                                                                          |

| `IPaymentGateway`          | Provider abstraction for payment and refund operations.                                            |

| `IPaymentWebhookProcessor` | Webhook processing abstraction.                                                                    |

| `IOrderPricingReader`      | Shared contract implemented by Ordering.                                                           |

| Outbox                     | Durable integration-event persistence.                                                             |

| Background Jobs            | Asynchronous processing for refunds and reconciliation.                                            |



\### 2.2 Key Abstractions



| Abstraction                | Implemented By                  | Purpose                                                                      |

| -------------------------- | ------------------------------- | ---------------------------------------------------------------------------- |

| `IPaymentGateway`          | `PaymobPaymentGateway`          | Initiate payments, get payment status, issue refunds, and get refund status. |

| `IPaymentWebhookProcessor` | `PaymobWebhookProcessor`        | Deserialize, verify HMAC, validate, and dispatch webhook commands.           |

| `IPaymentRepository`       | `PaymentRepository`             | Persist and query payments.                                                  |

| `IRefundRepository`        | `RefundRepository`              | Persist and query refunds.                                                   |

| `IOrderPricingReader`      | `OrderPricingReader` (Ordering) | Provide authoritative order pricing.                                         |

| `IUnitOfWork`              | `UnitOfWork`                    | Define the transaction boundary and save changes.                            |

| `IJobScheduler`            | `HangfireJobScheduler`          | Enqueue background jobs.                                                     |

| `IOutboxMessageWriter`     | `OutboxMessageWriter`           | Persist integration events atomically.                                       |



\---



\## 3. Workflow 1: Payment Initiation (Synchronous)



\### 3.1 Description



The client requests to pay for an order. The handler resolves authoritative pricing from Ordering, verifies ownership, checks for an existing Payment, creates the Payment aggregate, calls Paymob to create a payment intention, and returns a checkout URL immediately. The server does not wait for the customer to complete payment.



The client does not supply `CustomerId`, `Amount`, or `Currency`; these values are resolved server-side.



\### 3.2 Flow Diagram



```text

Client

&#x20; |

&#x20; | POST /api/payments/initiate

&#x20; | { OrderId, Method, ReturnUrl, CancelUrl, IdempotencyKey }

&#x20; v

PaymentsController

&#x20; |

&#x20; v

InitiatePaymentCommand

&#x20; |

&#x20; v

InitiatePaymentCommandHandler

&#x20; |

&#x20; +-- IOrderPricingReader.GetByOrderIdAsync(OrderId)

&#x20; |      |

&#x20; |      +-- null -> fail ("Order not found")

&#x20; |      +-- snapshot -> continue

&#x20; |

&#x20; +-- Ownership check

&#x20; |      |

&#x20; |      +-- snapshot.CustomerId == ICurrentUser.UserId

&#x20; |          (or caller is Administrator/Support)

&#x20; |

&#x20; +-- PaymentRepository.GetByOrderIdAsync(OrderId)

&#x20; |      |

&#x20; |      +-- exists -> fail ("Payment already initiated")

&#x20; |

&#x20; +-- Payment.Create(

&#x20; |       snapshot.OrderId,

&#x20; |       snapshot.CustomerId,

&#x20; |       Money(snapshot.Amount, snapshot.Currency),

&#x20; |       Method)

&#x20; |      |

&#x20; |      +-- Status = Pending

&#x20; |

&#x20; +-- SaveChangesAsync

&#x20; |

&#x20; +-- IPaymentGateway.InitiatePaymentAsync()

&#x20; |      |

&#x20; |      v

&#x20; |   PaymobPaymentGateway

&#x20; |      |

&#x20; |      v

&#x20; |   Paymob Intention Created

&#x20; |

&#x20; +-- Payment.AssignProviderIntention(provider, intentionId)

&#x20; |      |

&#x20; |      +-- Status = AwaitingPayment

&#x20; |

&#x20; +-- SaveChangesAsync

&#x20; |

&#x20; +-- Return Checkout URL

```



\### 3.3 Steps



1\. The client sends `InitiatePaymentCommand` with `OrderId`, `Method`, `ReturnUrl`, `CancelUrl`, and an optional `IdempotencyKey`.

2\. The handler resolves authoritative pricing through `IOrderPricingReader`. If the Order is not found, it returns a business failure.

3\. The handler verifies ownership against `ICurrentUser.UserId`, unless the caller is an Administrator or Support user.

4\. The handler checks for an existing Payment for the Order. If one exists, it returns `"Payment already initiated for this order"`.

5\. The handler creates Payment with `Status = Pending`, using the snapshot's `Amount`, `Currency`, and `CustomerId`.

6\. Payment is persisted in database transaction #1.

7\. The handler calls `IPaymentGateway.InitiatePaymentAsync`.

8\. If the Paymob call fails, the handler calls `Payment.Fail()` and persists the change.

9\. If successful, the handler calls `Payment.AssignProviderIntention(provider, intentionId)`.

10\. Payment transitions to `AwaitingPayment` and is saved in database transaction #2.

11\. The handler returns `PaymentInitiationResult` with the checkout URL.



\### 3.4 Transaction Boundary



Payment initiation uses two database transactions:



```text

DB transaction #1:

&#x20;   Create Payment (Pending)



External provider call:

&#x20;   Create Paymob Intention



DB transaction #2:

&#x20;   Save provider intention ID

&#x20;   Transition Payment to AwaitingPayment

```



The Paymob HTTP call occurs between the two database transactions.



A crash after Paymob succeeds but before the second database transaction leaves Payment in `Pending` with no `ProviderIntentionId`. Reconciliation cannot query Paymob for this payment because reconciliation requires `ProviderIntentionId != null`.



\*\*Residual risk:\*\* Manual cleanup is required for this specific failure window.



\---



\## 4. Workflow 2: Payment Confirmation via Webhook (Asynchronous)



\### 4.1 Description



When the customer completes payment on Paymob, Paymob sends a transaction callback. The system verifies the HMAC signature, processes the webhook idempotently, and updates the Payment aggregate to `Captured` or `Failed`.



\### 4.2 Flow Diagram



```text

Paymob

&#x20; |

&#x20; | POST /api/payments/webhook/paymob?hmac=...

&#x20; | Raw JSON body + HMAC query parameter

&#x20; v

PaymobWebhookController

&#x20; |

&#x20; | Reads raw body; binds HMAC from query (header fallback)

&#x20; v

IPaymentWebhookProcessor.ProcessAsync(provider, payload, signature)

&#x20; |

&#x20; v

PaymobWebhookProcessor

&#x20; |

&#x20; +-- Deserialize TransactionCallback

&#x20; +-- Verify HMAC (20-field canonical string)

&#x20; +-- Validate required fields (Id, IntentionId)

&#x20; +-- Create HandlePaymentWebhookCommand

&#x20;         |

&#x20;         v

HandlePaymentWebhookCommandHandler

&#x20; |

&#x20; +-- Find Payment by ProviderIntentionId

&#x20; |      |

&#x20; |      +-- null -> Transient (provider retries)

&#x20; |

&#x20; +-- If already final -> idempotent success

&#x20; |

&#x20; +-- If Success and Status == Pending

&#x20; |      |

&#x20; |      +-- Transient (transaction #2 not committed yet)

&#x20; |

&#x20; +-- If Success and Status == AwaitingPayment

&#x20; |      |

&#x20; |      +-- Payment.Capture()

&#x20; |

&#x20; +-- If failure -> Payment.Fail()

&#x20; |

&#x20; +-- Save Payment

&#x20;         |

&#x20;         v

PaymentCapturedDomainEvent / PaymentFailedDomainEvent

&#x20; |

&#x20; v

Domain Event Handler

&#x20; |

&#x20; v

PaymentCompletedIntegrationEvent / PaymentFailedIntegrationEvent

&#x20; |

&#x20; v

Outbox

```



\### 4.3 Steps



1\. Paymob sends a webhook with HMAC as a \*\*query parameter\*\* (`?hmac=...`). The controller also accepts it as a header for resilience.

2\. The controller reads the raw request body.

3\. The controller calls `IPaymentWebhookProcessor.ProcessAsync`.

4\. Infrastructure deserializes the callback and verifies HMAC against the 20-field canonical string.

5\. If HMAC is invalid, a typed `Unauthorized` result is returned and the API responds with HTTP 401.

6\. Required provider identifiers (`Id`, `IntentionId`) are validated. Missing identifiers produce a `Validation` result and HTTP 400.

7\. `HandlePaymentWebhookCommand` is sent to Application.

8\. The handler looks up Payment by `ProviderIntentionId`.

9\. If Payment is not found, the handler returns `Transient`, resulting in HTTP 500 so the provider retries.

10\. If Payment is already in a final state (`Captured` or `Failed`), the handler returns success without changes.

11\. If the callback indicates success while Payment is still `Pending` because transaction #2 has not committed, the handler returns `Transient`, resulting in a retry.

12\. If the callback indicates success and Payment is `AwaitingPayment`, the handler calls `Payment.Capture(providerTransactionId)`.

13\. If the callback indicates failure, the handler calls `Payment.Fail()`.

14\. The domain event is raised and translated into an integration event through the Outbox.



\### 4.4 Idempotency and Concurrency



\* Payments already in a final state return success without changes.

\* `RowVersion`, configured as a shadow property, protects against concurrent updates to the same payment.

\* `DbUpdateConcurrencyException` should be handled by global middleware or treated as idempotent success for webhook delivery when the current persisted state confirms the callback has already been applied.

\* A `Pending` status during a success webhook returns `Transient`. The provider retries, and on retry Payment should be `AwaitingPayment`.



\---



\## 5. Workflow 3: Payment Reconciliation (Background)



\### 5.1 Description



If a webhook is lost or delayed, a recurring job queries Paymob for the current status of stale `AwaitingPayment` payments and updates them accordingly.



\### 5.2 Flow Diagram



```text

Hangfire Recurring Trigger (\*/30 \* \* \* \*)

&#x20; |

&#x20; v

ReconcilePaymentsJob

&#x20; |

&#x20; v

ReconcilePaymentsCommand

&#x20; |

&#x20; v

ReconcilePaymentsCommandHandler

&#x20; |

&#x20; +-- Find AwaitingPayment older than threshold

&#x20; |      |

&#x20; |      +-- Filter: ProviderIntentionId != null

&#x20; |

&#x20; +-- For each payment:

&#x20;        |

&#x20;        +-- Build PaymentProviderReference

&#x20;        |      (TransactionId if present, otherwise IntentionId)

&#x20;        |

&#x20;        +-- IPaymentGateway.GetPaymentStatusAsync

&#x20;        |      |

&#x20;        |      v

&#x20;        |   PaymobPaymentGateway

&#x20;        |      |

&#x20;        |      +-- TransactionId present -> GetTransactionStatusAsync

&#x20;        |      +-- IntentionId only -> GetIntentionStatusAsync

&#x20;        |

&#x20;        +-- If Captured -> Payment.Capture()

&#x20;        +-- If Failed -> Payment.Fail()

&#x20;        +-- Save



&#x20; +-- Result.Success()

```



\### 5.3 Steps



1\. The recurring trigger enqueues `ReconcilePaymentsJob`.

2\. The handler queries `GetAwaitingPaymentWithIntentionOlderThanAsync`.

3\. Payments without `ProviderIntentionId` are excluded because reconciliation requires a queryable identifier.

4\. For each payment, the handler calls the Paymob status API using the transaction ID when available, otherwise the intention ID.

5\. If Paymob returns success, Payment is captured.

6\. If Paymob returns failure, Payment is failed.

7\. Each update is saved individually to isolate failures.



\### 5.4 Error Handling



\* `HttpRequestException` is logged and the payment is skipped; the next cycle retries.

\* `ArgumentException` caused by a reference without a queryable identifier is logged and skipped.

\* `InvalidOperationException` is logged and skipped.

\* Cancellation is propagated.

\* Success and failure updates are idempotent.



\*\*Verification requirement:\*\* Confirm that Paymob exposes `GET v1/intention/{intention\_id}` and that its response shape matches `PaymobStatusResponse`. If not, reconciliation for `AwaitingPayment` payments cannot work, and this becomes a documented gap.



\---



\## 6. Workflow 4: Refund Request (Synchronous API)



\### 6.1 Description



An authorized user requests a refund for a captured or partially refunded payment. The system verifies ownership, checks the outstanding refundable balance, creates a Refund aggregate in `Requested` status, persists it, and enqueues a background job.



\### 6.2 Flow Diagram



```text

Client

&#x20; |

&#x20; | POST /api/payments/{paymentId}/refund

&#x20; | { Amount, Currency, Reason }

&#x20; v

PaymentsController

&#x20; |

&#x20; v

RequestRefundCommand

&#x20; |

&#x20; v

RequestRefundCommandHandler

&#x20; |

&#x20; +-- Load Payment

&#x20; |      |

&#x20; |      +-- null -> fail ("Payment not found")

&#x20; |

&#x20; +-- Ownership check

&#x20; |      |

&#x20; |      +-- payment.CustomerId == ICurrentUser.UserId

&#x20; |          (or caller is Administrator/Support)

&#x20; |

&#x20; +-- payment.CanApplyRefund(Money)

&#x20; |      |

&#x20; |      +-- false -> fail ("not eligible")

&#x20; |

&#x20; +-- IRefundRepository.GetOutstandingByPaymentIdAsync(payment.Id)

&#x20; |      |

&#x20; |      +-- Sum(Requested + Processing) + new amount <= Remaining?

&#x20; |             |

&#x20; |             +-- false -> fail ("exceeds remaining refundable balance")

&#x20; |

&#x20; +-- Refund.Create(payment.Id, payment.OrderId, Amount, Reason)

&#x20; |      |

&#x20; |      +-- Status = Requested

&#x20; |

&#x20; +-- SaveChangesAsync

&#x20; |

&#x20; +-- Enqueue ProcessRefundJob(refund.Id)

&#x20; |

&#x20; +-- Return RefundId (202 Accepted)

```



\### 6.3 Steps



1\. The client sends a refund request with `Amount`, `Currency`, and `Reason`.

2\. The handler loads Payment. If it is not found, the request fails with a business error.

3\. The handler verifies ownership against `ICurrentUser.UserId`, unless the caller is privileged.

4\. The handler checks domain eligibility through `Payment.CanApplyRefund`.

5\. The handler checks outstanding refunds. The sum of `Requested` and `Processing` refunds plus the new amount must not exceed the remaining refundable balance.

6\. The handler creates Refund with `OrderId` denormalized from Payment.

7\. Refund is persisted.

8\. `ProcessRefundJob` is enqueued.

9\. The handler returns HTTP 202 Accepted with the refund ID.



\### 6.4 Important Design Decisions



\* \*\*Authoritative values:\*\* The refund amount and currency come from the client's request intentionally. The client specifies how much to refund, and the Domain enforces that the amount does not exceed the remaining refundable balance.

\* \*\*Two-layer protection against concurrent duplicates:\*\*



&#x20; \* Application-layer outstanding-balance check through `GetOutstandingByPaymentIdAsync`.

&#x20; \* Database-level filtered unique index on `(PaymentId, Amount, Currency)` where `Status <> 'Failed'`.

\* \*\*Enqueue-after-save window:\*\* If the process crashes between `SaveChangesAsync` and enqueueing the job, the refund remains `Requested`. `ReconcileRefundsJob` recovers it; see §8.



\---



\## 7. Workflow 5: Refund Processing (Asynchronous Background)



\### 7.1 Description



The background job processes the refund by claiming it atomically, calling Paymob, and updating both Refund and Payment aggregates.



\### 7.2 Flow Diagram



```text

Hangfire ProcessRefundJob

&#x20; |

&#x20; v

ProcessRefundJobHandler

&#x20; |

&#x20; v

ProcessRefundCommand

&#x20; |

&#x20; v

ProcessRefundCommandHandler

&#x20; |

&#x20; +-- Load Refund

&#x20; |

&#x20; +-- If final -> success

&#x20; |

&#x20; +-- Atomic claim: TryMarkProcessingAsync()

&#x20; |      |

&#x20; |      +-- false -> already claimed

&#x20; |      +-- true -> Refund = Processing

&#x20; |

&#x20; +-- Load Payment

&#x20; |      |

&#x20; |      +-- null -> requeue refund (returns to Requested)

&#x20; |

&#x20; +-- IPaymentGateway.RefundAsync(reference, amount)

&#x20; |      |

&#x20; |      +-- Succeeded -> Refund.Complete() + Payment.ApplyRefund()

&#x20; |      +-- Failed -> Refund.Fail()

&#x20; |      +-- Unknown -> remain Processing

&#x20; |

&#x20; +-- Save all aggregates

&#x20;         |

&#x20;         v

RefundCompletedDomainEvent / RefundFailedDomainEvent

&#x20;         |

&#x20;         v

Integration Event -> Outbox

```



\### 7.3 Steps



1\. `ProcessRefundJobHandler` sends `ProcessRefundCommand`.

2\. The handler loads Refund and checks whether it is in a final state.

3\. If Refund is `Requested`, the handler atomically claims it using `TryMarkProcessingAsync`.

4\. If the claim fails, another worker is already processing the refund.

5\. The handler loads the related Payment.

6\. If Payment is missing, the refund is requeued and returns to `Requested`. This is the only requeue path retained.

7\. The handler calls `IPaymentGateway.RefundAsync`.

8\. If the provider result succeeds, Refund completes and Payment applies the refund.

9\. If the provider result definitively fails, Refund fails.

10\. If the provider outcome is unknown (timeout, 5xx, 408, or 429), Refund remains `Processing` for reconciliation.

11\. Domain events are raised and integration events are written to the Outbox.



\### 7.4 Transaction Boundary



The provider call happens outside the database transaction. The handler first claims Refund and saves it, then calls Paymob, then updates the aggregates and saves again.



A crash after Paymob succeeds but before the final save leaves Refund in `Processing`. If the provider response was received before the crash, the refund's provider transaction ID is persisted; otherwise, it is not. Reconciliation resolves both cases through the parent payment's transaction ID.



\*\*Note on requeue safety:\*\* Paymob's refund endpoint does not accept an idempotency key. Requeueing an unknown-outcome refund after a crash could issue a second refund. Reconciliation therefore does \*\*not\*\* requeue unknown-outcome refunds; it resolves them by querying the parent payment transaction. See §8.



\---



\## 8. Workflow 6: Refund Reconciliation (Background)



\### 8.1 Description



A recurring job performs two phases:



1\. \*\*Stale `Requested` recovery:\*\* Re-enqueues `ProcessRefundJob` for refunds whose original enqueue was lost.

2\. \*\*Stuck `Processing` resolution:\*\* Queries Paymob for the parent payment transaction and reads `is\_refunded`.



\### 8.2 Flow Diagram



```text

Hangfire Recurring Trigger (\*/15 \* \* \* \*)

&#x20; |

&#x20; v

ReconcileRefundsJob

&#x20; |

&#x20; v

ReconcileRefundsCommand

&#x20; |

&#x20; v

ReconcileRefundsCommandHandler

&#x20; |

&#x20; +-- Phase 1: Stale Requested recovery

&#x20; |      |

&#x20; |      +-- GetRequestedOlderThanAsync(cutoff - 5 min)

&#x20; |      |

&#x20; |      +-- For each -> Enqueue ProcessRefundJob

&#x20; |

&#x20; +-- Phase 2: Stuck Processing resolution

&#x20;        |

&#x20;        +-- GetProcessingOlderThanAsync(cutoff - 15 min)

&#x20;        |

&#x20;        +-- For each refund:

&#x20;               |

&#x20;               +-- Load parent Payment

&#x20;               |      |

&#x20;               |      +-- null -> log warning, leave Processing

&#x20;               |

&#x20;               +-- Payment.ProviderTransactionId is null?

&#x20;               |      |

&#x20;               |      +-- yes -> log warning, leave Processing (no requeue)

&#x20;               |

&#x20;               +-- IPaymentGateway.GetRefundStatusAsync(parent reference)

&#x20;                      |

&#x20;                      +-- Succeeded -> Refund.Complete() + Payment.ApplyRefund()

&#x20;                      +-- Failed -> Refund.Fail()

&#x20;                      +-- Unknown -> remain Processing

&#x20;                      |

&#x20;                      +-- Save

```



\### 8.3 Steps



\*\*Phase 1 — Stale `Requested` recovery:\*\*



1\. The handler selects `Requested` refunds older than five minutes.

2\. For each refund, it enqueues `ProcessRefundJob(refundId)`.

3\. The atomic claim in `ProcessRefundCommandHandler` makes duplicate enqueues safe.



\*\*Phase 2 — Stuck `Processing` resolution:\*\*



4\. The handler selects `Processing` refunds older than 15 minutes.

5\. For each refund, it loads the parent Payment.

6\. If Payment is missing or has no `ProviderTransactionId`, the handler logs a warning and leaves Refund in `Processing`. \*\*No requeue\*\* is performed because requeueing would call Paymob again without an idempotency key, risking a double refund.

7\. The handler builds a `PaymentProviderReference` from the parent payment's `ProviderTransactionId`.

8\. The handler calls `IPaymentGateway.GetRefundStatusAsync`, which queries the parent transaction and reads `is\_refunded`.

9\. Based on the outcome:



&#x20;  \* \*\*Succeeded:\*\* Complete Refund and apply the refund to Payment.

&#x20;  \* \*\*Failed:\*\* Fail Refund.

&#x20;  \* \*\*Unknown:\*\* Leave Refund in `Processing`.

10\. Domain events are raised and integration events are written to the Outbox.



\### 8.4 Error Handling



\* `ArgumentException` caused by a missing identifier is logged and skipped.

\* `HttpRequestException` is logged and the refund is skipped.

\* `InvalidOperationException` is logged and the refund is skipped.

\* Cancellation is propagated.

\* An unknown outcome leaves Refund in `Processing` for the next reconciliation cycle.



\*\*Operational signal:\*\* Refunds stuck in `Processing` with no parent-payment transaction ID require manual reconciliation. The warning log is the operator's cue.



\---



\## 9. Cross-Cutting Concerns



\### 9.1 Idempotency



\* \*\*Webhooks:\*\* Duplicate callbacks are idempotent because already-final payments return success. A webhook for a `Pending` payment returns `Transient`, allowing the provider to retry.

\* \*\*Refund processing:\*\* Atomic claim prevents duplicate concurrent processing.

\* \*\*Refund requests:\*\* The application-layer outstanding-balance check and a filtered unique index prevent duplicate refund rows for the same `(PaymentId, Amount, Currency)`.

\* \*\*Reconciliation:\*\* Status updates are idempotent; repeated runs have no side effects if the state has already been applied.



\### 9.2 Concurrency



\* `RowVersion` shadow properties provide optimistic concurrency for Payment and Refund.

\* `TryMarkProcessingAsync` uses a conditional `ExecuteUpdate` to atomically claim refunds.

\* Concurrent webhook delivery is protected by final-state checks and concurrency tokens.

\* Concurrent refund requests are protected by the outstanding-balance check and unique index.



\### 9.3 Transaction Boundaries



\* \*\*Payment initiation:\*\* Two database transactions separated by an external provider call.

\* \*\*Webhook processing:\*\* One database transaction after HMAC verification.

\* \*\*Refund processing:\*\* Claim transaction, provider call, final-update transaction.

\* \*\*Refund request:\*\* One database transaction, followed by job enqueue.

\* \*\*Reconciliation:\*\* Individual save per updated aggregate.



\### 9.4 Error Handling Summary



| Scenario                                            | Outcome                                                |

| --------------------------------------------------- | ------------------------------------------------------ |

| Invalid HMAC                                        | HTTP 401 Unauthorized typed result                     |

| Invalid webhook payload                             | HTTP 400 Bad Request typed result                      |

| Payment not found in webhook                        | Transient error → HTTP 500 → provider retry            |

| Webhook arrives while Payment is `Pending`          | Transient error → HTTP 500 → provider retry            |

| Provider unreachable during refund                  | `RefundOutcome.Unknown` → remains `Processing`         |

| Provider refund rejected definitively               | `RefundOutcome.Failed` → Refund fails                  |

| Provider refund returns 408/429                     | `RefundOutcome.Unknown` → reconciliation               |

| Payment missing during refund processing            | Requeue; Refund returns to `Requested`                 |

| Refund stuck in `Processing`, no parent transaction | Leave `Processing`, log warning, manual reconciliation |

| Provider status query fails                         | Log and skip; next reconciliation retries              |

| Duplicate webhook after final state                 | Success without changes                                |

| Concurrent refund claim                             | One worker succeeds; others return failure             |

| Concurrent duplicate refund request                 | Outstanding check and unique index reject duplicate    |

| Refund amount exceeds remaining balance             | Business failure                                       |

| Client-tampered payment fields                      | Resolve server-side through `IOrderPricingReader`      |



\---



\## 10. Integration Points



\* \*\*\[ASP.NET Core](https://asp.net/):\*\* Controllers, model binding, and routing.

\* \*\*MediatR:\*\* Commands, queries, and pipeline behaviors.

\* \*\*EF Core:\*\* `AppDbContext.Finance`, configurations, and repositories.

\* \*\*Hangfire:\*\* Fire-and-forget refund job and recurring reconciliation jobs.

\* \*\*Outbox:\*\* Durable integration events for cross-context communication.

\* \*\*Ordering:\*\* `IOrderPricingReader` for authoritative order pricing.

\* \*\*Paymob:\*\* Intention API, transaction callbacks, refund API, and transaction inquiry for refund status.

\* \*\*Configuration:\*\* Options validation and secure secrets.



\*\*Note:\*\* Paymob does not expose a dedicated refund-status endpoint. Refund status is read from the parent payment transaction's `is\_refunded` field through the transaction inquiry endpoint.



\---



\## 11. Conclusion



The Finance workflows are consistent with the modular monolith architecture. Each workflow has clear transaction boundaries, concurrency control, idempotency, and recovery mechanisms.



\* \*\*Payment initiation\*\* is synchronous, resolves authoritative fields from Ordering, enforces ownership, and rejects duplicate initiation.

\* \*\*Payment confirmation\*\* is asynchronous through webhooks verified against Paymob's canonical HMAC, using the query parameter as the signature source.

\* \*\*Payment reconciliation\*\* recovers missed webhooks by querying Paymob through the intention ID.

\* \*\*Refund requests\*\* are synchronous, enforce ownership and outstanding balance, and are backed by a database-level unique index.

\* \*\*Refund processing\*\* is asynchronous and designed for reliability; unknown outcomes are never requeued.

\* \*\*Refund reconciliation\*\* recovers stale `Requested` refunds and resolves stuck `Processing` refunds through the parent payment transaction.



The result is a production-oriented Finance feature that isolates Paymob, protects the Domain, and integrates cleanly with the existing Outbox, scheduling, and persistence infrastructure.



\---



\*\*Document version:\*\* 1.1 — Final

\*\*Feature:\*\* Finance / Paymob Integration Workflows



