\# Finance Feature — Requirements Technical Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Scope:\*\* Functional, Non-Functional, Security, and Integration Requirements

\*\*Version:\*\* 1.1 – Final



\---



\## 1. Introduction



This document defines the technical requirements for the Finance feature of the E-Commerce Modular Monolith. It serves as the specification for design, development, testing, and acceptance.



The Finance feature manages payments, refunds, and financial transactions. It integrates with Paymob through the current Intention API, isolated behind a provider-agnostic gateway abstraction. The feature ensures reliable payment processing with synchronous initiation, asynchronous confirmation, background refunds, and two-phase reconciliation for payments and refunds.



\*\*Payment initiation is client-orchestrated.\*\* Finance does not consume any cross-context integration event as a trigger. After Ordering places an order, the client calls `POST /api/payments/initiate` on Finance with the `OrderId` and redirect URLs. Finance resolves authoritative pricing from Ordering via `IOrderPricingReader`.



The feature is implemented across Domain, Application, Infrastructure, and API layers, following Clean Architecture and DDD principles. The Domain layer is completely independent of Paymob or any other payment provider.



\*\*Version note:\*\* This revision reflects the applied remediation: authoritative pricing resolved from Ordering, HMAC canonical string and source corrected, payment reconciliation querying by intention ID, refund reconciliation split into two phases, refund requeue behavior removed, concurrent over-refund prevention in place, resource authorization enforced on refund and status handlers, and `RowVersion` configured as a shadow property.



\---



\## 2. Scope



\### 2.1 In Scope



\* Payment initiation using the Paymob Intention API with authoritative pricing resolved from Ordering.

\* Provider abstraction (`IPaymentGateway`) for future provider additions.

\* Payment aggregate with an explicit state machine.

\* Refund aggregate with an asynchronous processing lifecycle.

\* Webhook processing for payment confirmation with HMAC verification and idempotency.

\* Payment reconciliation for stuck or uncertain payments via intention ID.

\* Refund reconciliation for stale `Requested` refunds and stuck `Processing` refunds.

\* Asynchronous refund processing using existing background job infrastructure.

\* Integration events published to other bounded contexts via the Outbox.

\* Domain value objects: `Money` and `PaymentMethod`.

\* Repository abstractions for Payment and Refund.



\### 2.2 Out of Scope



\* Coupons, discounts, and taxes.

\* Manual payment capture or void operations not required by the Paymob flow.

\* Recurring billing or subscriptions.

\* Payouts or merchant fund transfers.

\* Multi-provider payment routing. Paymob is the initial provider, although the abstraction allows additional providers.

\* Payment method storage or vaulting.

\* A full accounting or financial reporting module.

\* Dispute or chargeback management.



\---



\## 3. Functional Requirements



\### 3.1 Payment Initiation



\*\*FR 1:\*\* The system shall allow an authenticated customer to initiate a payment for a specific order.



\*\*FR 2:\*\* The system shall resolve the payment's amount, currency, and customer identity from Ordering via `IOrderPricingReader`. The client shall not supply these values.



\*\*FR 3:\*\* The system shall verify that the resolved `CustomerId` equals the authenticated user's identifier, unless the caller holds the `Administrator` or `Support` role.



\*\*FR 4:\*\* The system shall reject payment initiation if a `Payment` already exists for the given `OrderId`.



\*\*FR 5:\*\* The system shall create a `Payment` aggregate in `Pending` status with a positive amount, order ID, customer ID, and payment method.



\*\*FR 6:\*\* The system shall persist the `Payment` aggregate and commit before making any external provider call.



\*\*FR 7:\*\* The system shall call `IPaymentGateway.InitiatePaymentAsync` synchronously to create a Paymob intention.



\*\*FR 8:\*\* The system shall use a second database transaction to save the provider intention ID and transition the payment to `AwaitingPayment` after the external call succeeds.



\*\*FR 9:\*\* The system shall return a checkout URL to the client immediately after intention creation.



\*\*FR 10:\*\* If the provider initiation call fails, the system shall mark the payment as `Failed` and persist the failure.



\*\*FR 11:\*\* The system shall not hold an open database transaction while waiting for the external provider response.



\### 3.2 Payment Confirmation via Webhook



\*\*FR 12:\*\* The system shall expose a webhook endpoint to receive Paymob transaction callbacks.



\*\*FR 13:\*\* The webhook endpoint shall be unauthenticated but must verify the HMAC signature before processing.



\*\*FR 14:\*\* The system shall verify the HMAC-SHA512 signature using the configured webhook secret and Paymob's documented canonical field ordering.



\*\*FR 15:\*\* The system shall bind the HMAC from the query parameter (`?hmac=...`), falling back to a header binding if the query parameter is absent.



\*\*FR 16:\*\* The system shall reject webhook requests with invalid signatures using `401 Unauthorized`.



\*\*FR 17:\*\* The system shall ensure webhook processing is idempotent using the provider transaction ID to prevent duplicate processing.



\*\*FR 18:\*\* If the callback indicates successful payment and the `Payment` is in `AwaitingPayment`, the system shall transition the `Payment` to `Captured` and record the provider transaction ID.



\*\*FR 19:\*\* If the callback indicates successful payment and the `Payment` is still `Pending` because the second transaction has not yet committed, the system shall return a transient error so the provider retries.



\*\*FR 20:\*\* If the callback indicates failed payment, the system shall transition the `Payment` from `AwaitingPayment` or `Pending` to `Failed`.



\*\*FR 21:\*\* The system shall raise corresponding domain events (`PaymentCapturedDomainEvent` and `PaymentFailedDomainEvent`) after state transitions.



\*\*FR 22:\*\* Domain event handlers shall write integration events (`PaymentCompletedIntegrationEvent` and `PaymentFailedIntegrationEvent`) to the Outbox atomically with the domain state change.



\### 3.3 Refund Request



\*\*FR 23:\*\* The system shall allow an authenticated customer to request a refund for their own payment, and privileged users (`Administrator` or `Support`) to request a refund for any payment.



\*\*FR 24:\*\* The system shall verify that `Payment.CustomerId` equals the authenticated user's identifier unless the caller holds the `Administrator` or `Support` role.



\*\*FR 25:\*\* The system shall validate that the refund amount is positive and does not exceed the remaining unrefunded captured amount.



\*\*FR 26:\*\* The system shall reject the refund request if the sum of outstanding refunds (`Requested` + `Processing`) plus the new amount would exceed the remaining refundable balance.



\*\*FR 27:\*\* The system shall create a `Refund` aggregate in `Requested` status, with `OrderId` denormalized from the loaded `Payment`, and persist it.



\*\*FR 28:\*\* The system shall enqueue a `ProcessRefundJob` via the existing background job infrastructure rather than process the refund synchronously in the HTTP request.



\### 3.4 Refund Processing



\*\*FR 29:\*\* The background refund handler shall atomically claim the refund via `TryMarkProcessingAsync`, transitioning it from `Requested` to `Processing`.



\*\*FR 30:\*\* The background refund handler shall call `IPaymentGateway.RefundAsync` using a provider reference built from the parent payment's transaction ID.



\*\*FR 31:\*\* On successful provider refund, the handler shall mark the `Refund` as `Completed` and call `Payment.ApplyRefund()` to update the payment's refunded amount and status.



\*\*FR 32:\*\* On a definitively failed provider refund, the handler shall mark the `Refund` as `Failed` and not apply the refund to the payment.



\*\*FR 33:\*\* On an unknown provider outcome (timeout, 5xx, HTTP 408, or HTTP 429), the handler shall leave the `Refund` in `Processing` for reconciliation.



\*\*FR 34:\*\* The system shall raise domain events (`RefundCompletedDomainEvent` and `RefundFailedDomainEvent`) after refund processing and write integration events to the Outbox. Both events shall carry `OrderId`.



\*\*FR 35:\*\* Refund processing shall never requeue a refund with an unknown outcome. Requeueing would reissue the refund against Paymob without an idempotency key, risking a double refund.



\### 3.5 Payment Reconciliation



\*\*FR 36:\*\* The system shall provide a recurring background job (`ReconcilePaymentsJob`) to reconcile payments that may be stuck or uncertain.



\*\*FR 37:\*\* The reconciliation job shall identify payments with `AwaitingPayment` status older than a configured threshold and a non-null `ProviderIntentionId`.



\*\*FR 38:\*\* The reconciliation job shall not process `Pending` payments without a `ProviderIntentionId`, because no queryable identifier exists.



\*\*FR 39:\*\* The reconciliation job shall call `IPaymentGateway.GetPaymentStatusAsync` using the payment's `ProviderIntentionId` when no `ProviderTransactionId` is available, and using `ProviderTransactionId` otherwise.



\*\*FR 40:\*\* The system shall update the payment state based on the provider status returned by the inquiry.



\*\*FR 41:\*\* The reconciliation job must be idempotent and safe to run repeatedly without causing duplicate transitions or events.



\### 3.6 Refund Reconciliation



\*\*FR 42:\*\* The system shall provide a recurring background job (`ReconcileRefundsJob`) with two phases.



\*\*FR 43:\*\* Phase 1 shall identify `Requested` refunds older than a short threshold (5 minutes) and re-enqueue `ProcessRefundJob` for each. This recovers refunds whose initial job enqueue was lost between the refund save and the enqueue call.



\*\*FR 44:\*\* Phase 2 shall identify `Processing` refunds older than a threshold (15 minutes) and resolve them by querying the parent payment's transaction status.



\*\*FR 45:\*\* For a `Processing` refund whose parent payment has no `ProviderTransactionId`, the reconciliation job shall leave the refund in `Processing`, log a warning, and take no further action. Manual reconciliation is required.



\*\*FR 46:\*\* For a `Processing` refund whose parent payment has a `ProviderTransactionId`, the job shall query the parent transaction and read `is\_refunded` to determine the outcome.



\*\*FR 47:\*\* The reconciliation job must be idempotent and safe to run repeatedly.



\### 3.7 Payment Status Query



\*\*FR 48:\*\* The system shall allow an authorized user to query the status of a payment by payment ID.



\*\*FR 49:\*\* The system shall verify that `Payment.CustomerId` equals the authenticated user's identifier unless the caller holds the `Administrator` or `Support` role.



\*\*FR 50:\*\* The query response shall contain the payment status, amount, currency, provider reference, and timestamps.



\*\*FR 51:\*\* The query shall not call the provider directly; it shall return the current state from the system database.



\### 3.8 Domain Model Requirements



\*\*FR 52:\*\* The system shall model `Money` as a value object with amount and currency, reused from the SharedKernel.



\*\*FR 53:\*\* The system shall enforce positive money amounts for payment and refund operations.



\*\*FR 54:\*\* The system shall enforce currency consistency in all money arithmetic.



\*\*FR 55:\*\* The system shall model `PaymentMethod` as a business-level value object with a method type (`Card`, `Wallet`, or `BankTransfer`) and no provider-specific details.



\*\*FR 56:\*\* The system shall model provider identifiers explicitly on the `Payment` aggregate as `Provider`, `ProviderIntentionId`, and `ProviderTransactionId`.



\*\*FR 57:\*\* The `Payment` aggregate shall expose only the states `Pending`, `AwaitingPayment`, `Captured`, `Failed`, `PartiallyRefunded`, and `Refunded`. The `Cancelled` state and `Cancel()` method shall not exist. Cancellation is an Ordering concern, not a Finance concern.



\*\*FR 58:\*\* The `Refund` aggregate shall carry `OrderId` alongside `PaymentId`, denormalized at creation.



\*\*FR 59:\*\* The `RowVersion` concurrency token shall be configured as an EF Core shadow property. The Domain aggregates shall not declare a `RowVersion` field.



\*\*FR 60:\*\* The Domain layer shall not contain any Paymob-specific types, constants, or logic.



\---



\## 4. Non-Functional Requirements



\### 4.1 Performance



\*\*NFR 1:\*\* Payment initiation should complete within 3 seconds under normal conditions, excluding Paymob API latency. This is a target, not a hard acceptance criterion.



\*\*NFR 2:\*\* Webhook processing should complete within 2 seconds under normal load, excluding asynchronous work.



\### 4.2 Scalability



\*\*NFR 3:\*\* The system must support multiple API instances. Payment and refund state transitions must be concurrency-safe using `RowVersion` optimistic concurrency and atomic database operations (`TryMarkProcessingAsync`, filtered unique indexes).



\*\*NFR 4:\*\* Background jobs (refund processing, payment reconciliation, and refund reconciliation) must be safe to run concurrently across multiple instances without causing duplicate processing.



\### 4.3 Reliability



\*\*NFR 5:\*\* The system must not lose payment state due to provider failures. Reconciliation and idempotency mechanisms ensure eventual consistency.



\*\*NFR 6:\*\* Webhook duplicate deliveries must not cause duplicate payment state transitions or duplicate integration events.



\*\*NFR 7:\*\* The system must recover from a crash after Paymob intention creation but before the second database transaction via payment reconciliation, provided the payment has a persisted `ProviderIntentionId`.



\*\*NFR 8:\*\* Refund processing must survive temporary provider failures. Unknown outcomes remain `Processing` and are resolved via reconciliation. The system shall not requeue unknown-outcome refunds to avoid double refunds.



\*\*NFR 9:\*\* Concurrent duplicate refund requests for the same `(PaymentId, Amount, Currency)` shall be rejected by a filtered unique index on the `Refunds` table.



\*\*NFR 10:\*\* `PaymobApiClient` shall be registered as a Singleton so its token cache is process-wide. Every Paymob call shall reuse the cached token until it expires or is invalidated.



\### 4.4 Maintainability



\*\*NFR 11:\*\* Provider-specific logic must be isolated in Infrastructure. Changing or adding providers must not require changes to Domain or business contexts.



\*\*NFR 12:\*\* The Application layer must not reference the Paymob SDK, EF Core, or provider-specific models.



\*\*NFR 13:\*\* The Domain must express all payment/refund state transitions as explicit methods, not public setters.



\### 4.5 Testability



\*\*NFR 14:\*\* Domain aggregates, rules, and value objects must be unit testable without external dependencies.



\*\*NFR 15:\*\* Application handlers must be testable with mocked `IPaymentGateway`, `IOrderPricingReader`, repositories, and Outbox writer.



\*\*NFR 16:\*\* Reconciliation and refund jobs must be testable with mocked providers and real database integration tests.



\---



\## 5. Security Requirements



\### 5.1 Webhook Security



\*\*SEC 1:\*\* The system shall verify Paymob webhook HMAC signatures using the configured webhook secret and Paymob's documented canonical field list.



\*\*SEC 2:\*\* The system shall reject any webhook with an invalid signature.



\*\*SEC 3:\*\* The webhook endpoint shall not require authentication but shall rely on signature verification for integrity.



\*\*SEC 4:\*\* The webhook secret must be stored in secure configuration and never logged.



\*\*SEC 5:\*\* HMAC comparison shall use `CryptographicOperations.FixedTimeEquals` with a length guard. A malformed or truncated signature shall return `false`, not throw.



\### 5.2 Provider Credentials



\*\*SEC 6:\*\* Paymob API keys and secrets must be stored via secure configuration, such as environment variables, User Secrets, or a key vault.



\*\*SEC 7:\*\* Provider credentials must never be exposed in DTOs, logs, or error messages.



\### 5.3 Payment Data Security



\*\*SEC 8:\*\* The system must validate all monetary amounts to prevent negative or zero payments/refunds.



\*\*SEC 9:\*\* The system must enforce currency consistency to prevent financial errors.



\*\*SEC 10:\*\* DTOs shall not expose internal provider identifiers beyond what is necessary for the consumer.



\*\*SEC 11:\*\* `POST /api/payments/initiate` shall not accept `Amount`, `Currency`, or `CustomerId` from the client. These values shall be resolved server-side via `IOrderPricingReader`.



\*\*SEC 12:\*\* `InitiatePaymentCommandHandler` shall verify the resolved `CustomerId` against `ICurrentUser.UserId` unless the caller holds the `Administrator` or `Support` role.



\*\*SEC 13:\*\* `RequestRefundCommandHandler` and `GetPaymentStatusQueryHandler` shall verify `Payment.CustomerId == currentUser.UserId` unless the caller holds the `Administrator` or `Support` role. On ownership mismatch, the handlers shall return `"Payment not found"` to avoid leaking existence.



\### 5.4 Idempotency



\*\*SEC 14:\*\* Webhook processing must be idempotent. Already-final payments return success without state change; `Pending` payments return a transient error for provider retry.



\*\*SEC 15:\*\* Refund processing must be idempotent. The atomic claim via `TryMarkProcessingAsync` prevents duplicate concurrent processing. The filtered unique index prevents duplicate refund rows.



\*\*SEC 16:\*\* Reconciliation must be idempotent and must not create duplicate domain events.



\---



\## 6. Integration Requirements



\### 6.1 Paymob Integration



\* The Paymob provider must use the current Intention API.

\* The provider must create an intention and return a checkout URL.

\* The provider must verify HMAC-SHA512 callbacks using Paymob's documented field concatenation.

\* Refund status shall be obtained by querying the parent payment transaction and reading `is\_refunded` / `refunded\_amount\_cents`. Paymob does not expose a dedicated refund-status endpoint.

\* Provider-specific request/response models must remain in Infrastructure.



\### 6.2 Ordering Integration



\* Finance depends on `IOrderPricingReader` (`Application/Shared/Orders`), implemented by Ordering Infrastructure.

\* Finance shall use this contract to resolve authoritative `Amount`, `Currency`, and `CustomerId` during payment initiation.

\* Finance shall not consume any cross-context integration event.



\### 6.3 Outbox Integration



\* Domain events resulting from payment and refund state changes shall be translated into integration events.

\* Integration events shall be persisted atomically with the business state using the existing Outbox infrastructure.

\* Finance publishes:



&#x20; \* `PaymentCompletedIntegrationEvent`

&#x20; \* `PaymentFailedIntegrationEvent`

&#x20; \* `RefundCompletedIntegrationEvent`

&#x20; \* `RefundFailedIntegrationEvent`



All four integration events carry `OrderId`.



\### 6.4 Background Processing



\* Refund processing uses internal background jobs, not cross-context integration events.

\* Payment and refund reconciliation use recurring background jobs.

\* Both use the existing scheduling and background job infrastructure.



\### 6.5 Persistence



\* `Payment`, `PaymentTransaction`, and `Refund` aggregates shall be persisted in the Finance schema using EF Core.

\* `Money` shall be mapped as an owned type with `Amount` and `Currency` columns.

\* `RowVersion` shall be configured as a shadow property in the EF Core configuration, not on the aggregate.

\* A filtered unique index shall be created on `Refunds(PaymentId, Amount, Currency)` where `Status <> 'Failed'`. This index is created via migration because EF Core does not support composite indexes mixing owner and owned-type properties.

\* Repository interfaces are defined in Domain; implementations are in Infrastructure.



\### 6.6 MediatR



\* All Application operations are exposed as commands and queries with MediatR handlers.

\* Authorization is enforced via resource ownership in handlers, not via `\[AuthorizePermission]` attributes. Finance commands and queries do not carry permission decorators.



\---



\## 7. Configuration Requirements



The following configuration keys are required for the Finance feature:



| Section / Key                  | Purpose                               | Validation                               |

| ------------------------------ | ------------------------------------- | ---------------------------------------- |

| `Payment:Provider`             | Selects the payment provider.         | Must be `"Paymob"`.                      |

| `Payment:Paymob:ApiKey`        | Paymob API key.                       | Required, not empty.                     |

| `Payment:Paymob:SecretKey`     | Paymob secret key.                    | Required, not empty.                     |

| `Payment:Paymob:IntegrationId` | Paymob integration ID.                | Required; must parse as an integer.      |

| `Payment:Paymob:BaseUrl`       | Paymob API base URL.                  | Must be a valid absolute HTTP/HTTPS URL. |

| `Payment:Paymob:WebhookSecret` | HMAC secret for webhook verification. | Required, not empty.                     |



Startup validation must fail fast on missing or invalid configuration. `IntegrationId` integer parsing is enforced at boot; the gateway does not guard at first payment.



\*\*Note:\*\* Reconciliation cadences are compile-time values in the `\[RecurringJob]` attributes:



\* `ReconcilePaymentsTrigger`: `\*/30 \* \* \* \*`

\* `ReconcileRefundsTrigger`: `\*/15 \* \* \* \*`



These schedules are not read from configuration.



\---



\## 8. Assumptions and Constraints



\### 8.1 Assumptions



\* Orders exist in a separate bounded context. Finance processes payment for an existing order ID and reads its authoritative pricing.

\* Only one payment provider (Paymob) is required initially.

\* Paymob webhooks are reliable but may be delayed; reconciliation handles missed or delayed callbacks.

\* Refunds are initiated by an authenticated customer for their own payments, or by `Administrator`/`Support` for any payment.

\* The client is responsible for calling `POST /api/payments/initiate` after `POST /api/orders/place` completes.



\### 8.2 Constraints



\* The Domain layer must not reference Paymob, provider SDKs, EF Core, or any infrastructure concern.

\* Payment initiation must not hold a database transaction open during the external Paymob call.

\* Refunds must not call the provider inside the request transaction.

\* Finance must not directly modify Orders, Inventory, or Notifications; integration events are used.

\* Finance must not requeue refunds with unknown outcomes.

\* Finance must not accept client-supplied `Amount`, `Currency`, or `CustomerId` during payment initiation.



\---



\## 9. Traceability Matrix



| Requirement ID   | Description                                                             |

| ---------------- | ----------------------------------------------------------------------- |

| FR 1–FR 11       | Payment initiation                                                      |

| FR 12–FR 22      | Webhook processing                                                      |

| FR 23–FR 28      | Refund request                                                          |

| FR 29–FR 35      | Refund processing                                                       |

| FR 36–FR 41      | Payment reconciliation                                                  |

| FR 42–FR 47      | Refund reconciliation                                                   |

| FR 48–FR 51      | Payment status query                                                    |

| FR 52–FR 60      | Domain model requirements                                               |

| NFR 1–NFR 16     | Performance, scalability, reliability, maintainability, and testability |

| SEC 1–SEC 16     | Security requirements                                                   |

| §6 Integration   | Paymob, Ordering, Outbox, background jobs, persistence, and MediatR     |

| §7 Configuration | Provider selection and Paymob credentials                               |



\---



\## 10. Acceptance Criteria



The Finance feature is considered complete when:



1\. Payment initiation resolves authoritative pricing from Ordering and does not accept client-supplied `Amount`, `Currency`, or `CustomerId`.

2\. Ownership is enforced on initiation, refund, and status handlers.

3\. Duplicate payment initiation for the same order returns a business failure, not a `500` response.

4\. Payment state transitions correctly through `Pending` → `AwaitingPayment` → `Captured`/`Failed`.

5\. Webhook HMAC verification uses Paymob's documented canonical field list and binds from the query parameter.

6\. Duplicate webhooks are processed idempotently; webhooks arriving while the `Payment` is `Pending` return a transient error for provider retry.

7\. Payment capture raises `PaymentCapturedDomainEvent` and publishes `PaymentCompletedIntegrationEvent` via the Outbox.

8\. Refund requests enforce ownership, validate the outstanding refundable balance, and create a `Refund` with denormalized `OrderId`.

9\. Refund processing claims the refund atomically, calls the provider, and updates both `Refund` and `Payment` on success.

10\. Unknown-outcome refunds remain `Processing` and are never requeued.

11\. Concurrent duplicate refund requests for the same `(PaymentId, Amount, Currency)` are rejected by the filtered unique index.

12\. `ReconcilePaymentsJob` recovers stuck `AwaitingPayment` payments via intention-ID status inquiry.

13\. `ReconcileRefundsJob` performs both phases: stale `Requested` recovery and stuck `Processing` resolution.

14\. Refund domain events carry `OrderId`; refund event handlers do not depend on `IPaymentRepository`.

15\. `RowVersion` is a shadow property on `Payment` and `Refund`.

16\. `PaymobApiClient` is registered as a Singleton.

17\. The Domain contains no Paymob-specific code.

18\. All security requirements are satisfied; no secrets are logged.

19\. The Application layer remains free of infrastructure dependencies.

20\. All Finance-related configuration validates at startup.



\---



\## 11. Summary



These requirements define a production-oriented Finance feature integrated with Paymob. The feature uses client-orchestrated synchronous payment initiation with server-side authoritative pricing, asynchronous webhook confirmation with a corrected HMAC verification path, background refund processing with double-refund prevention, and two-phase reconciliation for both payments and refunds.



The design enforces financial invariants in the Domain and isolates provider-specific logic in Infrastructure. Reliability rests on:



\* Idempotent webhook processing with a `Pending` → transient retry path.

\* Atomic refund claims and a filtered unique index preventing concurrent over-refund.

\* Two-phase refund reconciliation that closes the job-loss window without risking a double refund.

\* Payment reconciliation via intention-ID status inquiry.

\* A Singleton `PaymobApiClient` with a working token cache.

\* Resource-based authorization on all customer-facing handlers.



Cross-context integration is limited to one shared contract consumed by Finance (`IOrderPricingReader`) and four integration events published by Finance. Finance does not consume any cross-context event.



\---



\*\*Document Version:\*\* 1.1 – Final

\*\*Feature:\*\* Finance / Paymob Integration — Requirements



