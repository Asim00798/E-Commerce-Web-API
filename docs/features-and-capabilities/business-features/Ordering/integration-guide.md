\# Ordering Integration Guide



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Scope:\*\* How Ordering Integrates with Catalog, Shipping, Finance, and Client Applications

\*\*Version:\*\* 2.3 – Final



\---



\## 1. Purpose



This document defines the integration boundaries of the Ordering bounded context. It focuses on cross-context communication, including:



\* Shared application contracts consumed by Ordering.

\* Integration events published and consumed via the Outbox.

\* REST APIs exposed to clients.

\* Transaction consistency and atomicity for cross-context operations.

\* Authorization and security at integration points.



It is not a full technical reference or workflow specification. Detailed use-case flows and aggregate behavior are described in the Ordering Technical Reference Document and the Ordering Workflow Technical Document.



\---



\## 2. Integration Overview



Ordering is a central bounded context that orchestrates the purchase flow. It interacts with other contexts and clients through the following mechanisms.



\### 2.1 Shared Application Contracts



Ordering consumes:



\* `IStockService` — Catalog.

\* `IShippingFeeCalculator` — Shipping.



\### 2.2 Integration Events via Outbox



Ordering publishes and consumes durable events for asynchronous communication.



\### 2.3 REST APIs



Clients use Cart and Order endpoints.



\### 2.4 Database Transactions



Ordering's stock operation through `IStockService` participates in the same database transaction as Order persistence in the current modular monolith. This is a deliberate optimization, not a DDD rule.



\---



\## 3. Cross-Context Integration Flows



The following diagrams highlight cross-context interactions. Internal Ordering steps are simplified; full workflows are documented separately.



\### 3.1 Checkout — Order Creation and Stock Decrease



```text

Customer

&#x20;  |

&#x20;  v

Ordering (PlaceOrderCommand)

&#x20;  |

&#x20;  +-- Fetch Person data required by IShippingFeeCalculator

&#x20;  |

&#x20;  +-- Calculate ShippingFee via IShippingFeeCalculator (synchronous)

&#x20;  |

&#x20;  +-- Create Order (PendingPayment)

&#x20;  |

&#x20;  +-- Decrease Stock via IStockService (same transaction)

&#x20;  |

&#x20;  +-- Clear Cart

&#x20;  |

&#x20;  +-- Publish OrderPlacedIntegrationEvent (via Outbox)

```



\*\*Integration notes:\*\*



\* Shipping fee calculation is synchronous and occurs outside the Ordering database transaction.

\* Stock decrease is synchronous and atomic with Order persistence in the current modular monolith.

\* `OrderPlacedIntegrationEvent` is written to the Outbox inside the Order transaction. Its consumer is Ordering-internal — `SendOrderConfirmationEmailHandler` — which sends the order confirmation email via the Notifications channel. Finance does not consume this event.



\### 3.2 Payment Initiation (Client-Initiated)



Ordering does not call Finance or Paymob directly, and Finance does not consume `OrderPlacedIntegrationEvent`. Payment initiation is a \*\*client-orchestrated\*\* flow:



```text

Client places order

&#x20;       |

&#x20;       v

POST /api/orders/place

&#x20;       |

&#x20;       v

OrderId returned

&#x20;       |

&#x20;       v

Client initiates payment

&#x20;       |

&#x20;       v

POST /api/payments/initiate

(OrderId, Amount, Currency, Method, ReturnUrl, CancelUrl)

&#x20;       |

&#x20;       v

Finance creates Payment (Pending)

&#x20;       |

&#x20;       v

Finance calls Paymob Intention API

&#x20;       |

&#x20;       v

Payment (AwaitingPayment)

&#x20;       |

&#x20;       v

Checkout URL returned

&#x20;       |

&#x20;       v

Client redirects customer to Paymob

```



\*\*Integration notes:\*\*



\* The client supplies `ReturnUrl` and `CancelUrl` — browser-session redirect targets that Finance forwards to Paymob. These belong to the initiating client, not to Ordering.

\* Finance's `InitiatePaymentCommandHandler` does not depend on any Ordering event. The two contexts meet only at `PaymentCompletedIntegrationEvent` / `PaymentFailedIntegrationEvent`, which are published back to Ordering.

\* Ordering does not receive payment initiation results. It only observes the eventual `PaymentCompletedIntegrationEvent` or `PaymentFailedIntegrationEvent`.



\### 3.3 Payment Success



After Finance publishes `PaymentCompletedIntegrationEvent`:



```text

Finance

&#x20;  |

&#x20;  | PaymentCompletedIntegrationEvent

&#x20;  v

Ordering (PaymentCompletedIntegrationEventHandler)

&#x20;  |

&#x20;  +-- Order.MarkPaid()

&#x20;  |

&#x20;  +-- Publish OrderReadyForShippingIntegrationEvent (via Outbox)

```



\*\*Integration note:\*\* `OrderPaidDomainEventHandler` fetches the current delivery address from People and creates `OrderReadyForShippingIntegrationEvent`. Shipping later stores that address as its own `DeliveryAddressSnapshot`.



\### 3.4 Payment Failure



```text

Finance

&#x20;  |

&#x20;  | PaymentFailedIntegrationEvent

&#x20;  v

Ordering (PaymentFailedIntegrationEventHandler)

&#x20;  |

&#x20;  +-- Order.MarkPaymentFailed()

```



\* The Order transitions from `PendingPayment` to `PaymentFailed`.

\* Stock is not restored automatically.

\* No cancellation is triggered automatically.

\* `PaymentFailed` is a distinct state that requires explicit user action through `CancelOrderCommand` to transition to `Cancelled`.



\### 3.5 Expiration (System-Initiated)



Expiration is a separate, system-initiated flow from payment failure. It runs as a recurring background job and targets only orders still in `PendingPayment`.



```text

Hangfire trigger (every N hours)

&#x20;       |

&#x20;       v

ExpirePendingOrdersJobHandler

&#x20;       |

&#x20;       +-- Query IPendingOrderCleanupService for PendingPayment orders

&#x20;       |   older than OrderingOptions.PendingOrderExpirationHours

&#x20;       |

&#x20;       +-- Publish OrdersExpiredIntegrationEvent

&#x20;           (batch of order IDs) via Outbox

&#x20;       |

&#x20;       +-- No state mutation in the job handler

&#x20;                   |

&#x20;                   v

Outbox dispatcher delivers OrdersExpiredIntegrationEvent

&#x20;                   |

&#x20;                   v

Two independent integration event handlers react

&#x20;                   |

&#x20;         +---------+----------+

&#x20;         |                    |

&#x20;         v                    v

CancelExpiredOrders       RestoreStockOnExpiredOrders

IntegrationEventHandler   IntegrationEventHandler

&#x20;         |                    |

&#x20;         v                    v

Cancel each PendingPayment  Restore stock for each

order (per-order transaction) Cancelled order

&#x20;         |                    (per-order transaction)

&#x20;         v                    |

Order.Cancel()                +-- If cancellation has not

&#x20;         |                       committed, retry via Outbox

&#x20;         v

OrderCancelledDomainEvent

&#x20;         |

&#x20;         v

OrderCancelledIntegrationEvent written to Outbox

```



\*\*Integration notes:\*\*



\* `OrdersExpiredIntegrationEvent` is an Ordering-owned outbound event. It carries the batch of expired order IDs.

\* `OrderCancelledIntegrationEvent` is emitted per cancelled order through the same domain event path used by user-initiated cancellation. Consumers see cancellations, not the distinction between "user cancelled" and "expired".

\* Stock is restored by Ordering, not by consumers. `OrderCancelledIntegrationEvent` remains informational.

\* `PaymentFailed` orders are not processed by the expiration job. Only `PendingPayment` orders older than the threshold are eligible.



\### 3.6 Shipment Status Updates



Ordering reacts to shipment events published by Shipping. It does not call Shipping APIs directly.



| Shipping Event                      | Ordering Status Transition           |

| ----------------------------------- | ------------------------------------ |

| `ShipmentShippedIntegrationEvent`   | `Paid` → `Shipped`                   |

| `ShipmentDeliveredIntegrationEvent` | `Shipped` → `Delivered`              |

| `ShipmentReturnedIntegrationEvent`  | `Delivered` / `Shipped` → `Returned` |



\### 3.7 Refund Completion



Finance publishes `RefundCompletedIntegrationEvent`. Ordering updates its local state.



```text

Finance

&#x20;  |

&#x20;  | RefundCompletedIntegrationEvent

&#x20;  v

Ordering (RefundCompletedIntegrationEventHandler)

&#x20;  |

&#x20;  +-- Order.MarkRefunded()

```



`RefundFailedIntegrationEvent` does not change Order state; it may be logged.



\### 3.8 Cancellation



Cancellation has two origins — user-initiated and system-expiry. Both converge on `Order.Cancel()`, `OrderCancelledDomainEvent`, and `OrderCancelledIntegrationEvent`. Stock restoration differs in where it is invoked.



\#### User-Initiated Cancellation



```text

Ordering (CancelOrderCommand)

&#x20;  |

&#x20;  +-- Order.Cancel()

&#x20;  |

&#x20;  +-- Restore stock via IStockService.IncreaseStockAsync

&#x20;  |

&#x20;  +-- Publish OrderCancelledIntegrationEvent (via Outbox)

&#x20;  |

&#x20;  +-- All in one transaction

```



\#### System-Expiry Cancellation



```text

OrdersExpiredIntegrationEvent

&#x20;  |

&#x20;  +-- CancelExpiredOrdersIntegrationEventHandler

&#x20;  |       |

&#x20;  |       +-- Order.Cancel()

&#x20;  |       |

&#x20;  |       +-- Publish OrderCancelledIntegrationEvent (via Outbox)

&#x20;  |

&#x20;  +-- RestoreStockOnExpiredOrdersIntegrationEventHandler

&#x20;          |

&#x20;          +-- Restore stock via IStockService.IncreaseStockAsync

&#x20;  |

&#x20;  +-- Each handler uses separate per-order transactions

```



`OrderCancelledIntegrationEvent` is informational; other contexts must not restore stock.



\---



\## 4. Shared Contracts Used by Ordering



\### 4.1 `IStockService`



\*\*Namespace:\*\* `Application.Shared.Stock`



```csharp

public interface IStockService

{

&#x20;   Task<Result> DecreaseStockAsync(

&#x20;       Guid productId,

&#x20;       Guid variantId,

&#x20;       int quantity,

&#x20;       CancellationToken ct = default);



&#x20;   Task<Result> IncreaseStockAsync(

&#x20;       Guid productId,

&#x20;       Guid variantId,

&#x20;       int quantity,

&#x20;       CancellationToken ct = default);

}

```



\* Used by Ordering for synchronous stock changes during order placement and for stock restoration on cancellation, both user-initiated and system-expiry.

\* In the current modular monolith, it participates in the same database transaction as the caller's Unit of Work save. This is an implementation choice, not a DDD requirement.

\* The current synchronous contract and transaction model are specific to the modular monolith. A future distributed deployment would require a redesigned integration and consistency model.



\### 4.2 `IShippingFeeCalculator`



\*\*Namespace:\*\* `Application.Shared.Shipping.Services`



```csharp

public interface IShippingFeeCalculator

{

&#x20;   Task<ShippingFeeCalculationResult> CalculateAsync(

&#x20;       ShippingFeeCalculationRequest request,

&#x20;       CancellationToken ct = default);

}

```



\* Used to compute the shipping fee during checkout.

\* Shipping owns the fee rules.

\* Ordering owns the final order amount.



\---



\## 5. Integration Events



\### 5.1 Event Ownership



| Event                                   | Owner    | Purpose                                                                                              |

| --------------------------------------- | -------- | ---------------------------------------------------------------------------------------------------- |

| `OrderPlacedIntegrationEvent`           | Ordering | Ordering-internal; consumed by `SendOrderConfirmationEmailHandler` to send the confirmation email.   |

| `OrderReadyForShippingIntegrationEvent` | Ordering | Shipping creates a shipment after payment.                                                           |

| `OrderCancelledIntegrationEvent`        | Ordering | Notifies consumers; stock has already been restored by Ordering.                                     |

| `OrdersExpiredIntegrationEvent`         | Ordering | Internal event that triggers cancellation and stock restoration for expired `PendingPayment` orders. |

| `PaymentCompletedIntegrationEvent`      | Finance  | Ordering marks the Order as `Paid`.                                                                  |

| `PaymentFailedIntegrationEvent`         | Finance  | Ordering marks the Order as `PaymentFailed`.                                                         |

| `RefundCompletedIntegrationEvent`       | Finance  | Ordering marks the Order as `Refunded`.                                                              |

| `RefundFailedIntegrationEvent`          | Finance  | Ordering does not change Order state; the event may be logged.                                       |

| `ShipmentShippedIntegrationEvent`       | Shipping | Ordering marks the Order as `Shipped`.                                                               |

| `ShipmentDeliveredIntegrationEvent`     | Shipping | Ordering marks the Order as `Delivered`.                                                             |

| `ShipmentReturnedIntegrationEvent`      | Shipping | Ordering marks the Order as `Returned`.                                                              |



\### 5.2 Outbound Events from Ordering



\* `OrderPlacedIntegrationEvent` — Ordering-owned, consumed locally by `SendOrderConfirmationEmailHandler`.

\* `OrderReadyForShippingIntegrationEvent` — consumed by Shipping.

\* `OrderCancelledIntegrationEvent` — consumed by Shipping and Notifications.

\* `OrdersExpiredIntegrationEvent` — internal to Ordering; not intended for cross-context consumption.



Domain event handlers translate domain facts into integration events, while the Outbox provides durable publication semantics.



`OrdersExpiredIntegrationEvent` is an exception: it is written directly by the job handler (Channel 5), not by a domain event handler.



`OrderPlacedIntegrationEvent` is a second exception: it is consumed by an Ordering-owned handler for local durable side effects (email), not by an external context. Finance does not consume it.



\### 5.3 Inbound Events to Ordering



\*\*From Shipping:\*\*



\* `ShipmentShippedIntegrationEvent`

\* `ShipmentDeliveredIntegrationEvent`

\* `ShipmentReturnedIntegrationEvent`



\*\*From Finance:\*\*



\* `PaymentCompletedIntegrationEvent`

\* `PaymentFailedIntegrationEvent`

\* `RefundCompletedIntegrationEvent`

\* `RefundFailedIntegrationEvent`



All inbound handlers are idempotent via the existing `IdempotentIntegrationEventHandler<T>` decorator.



\---



\## 6. Transaction Boundaries and Consistency (Integration Perspective)



\### 6.1 Checkout Transaction



\* Stock decrease, Order persistence, cart clearing, and the `OrderPlacedIntegrationEvent` Outbox record are committed in one database transaction.

\* Shipping fee calculation occurs before the transaction and returns only a value.

\* Checkout uses manual transaction mode (`BeginTransactionAsync` / `CommitTransactionAsync`) because Phase 1 performs cross-context synchronous reads outside the transaction.



\### 6.2 Payment and Fulfilment Updates



\* Payment and shipment events are processed asynchronously.

\* Each inbound event updates Order state in its own transaction, with any resulting Outbox events persisted atomically.



\### 6.3 Cancellation



\* \*\*User-initiated:\*\* cancellation, stock restoration, and the `OrderCancelledIntegrationEvent` Outbox record are committed atomically per order.

\* \*\*System-expiry:\*\* cancellation and stock restoration are performed by two independent integration event handlers. Each processes orders per order in its own transaction. The two handlers converge regardless of execution order; if the stock handler runs before cancellation commits, it throws and is retried by the Outbox dispatcher.



\### 6.4 Expiry Detection



\* `ExpirePendingOrdersJobHandler` performs one save: the `OrdersExpiredIntegrationEvent` Outbox record. No Order state is mutated by the job.



\---



\## 7. Authorization Integration



\### 7.1 Permissions



```csharp

public static class OrderingPermissions

{

&#x20;   public const string Read = "Orders.Read";

&#x20;   public const string Place = "Orders.Place";

&#x20;   public const string Cancel = "Orders.Cancel";

&#x20;   public const string Manage = "Orders.Manage";

}

```



\### 7.2 API Layer



Controllers use role-based authorization:



\* The Customer role can access its own cart and orders.

\* Admin/Support can manage all orders.



\### 7.3 Application Layer



Commands and queries carry `\[AuthorizePermission]` attributes. Resource authorization is performed inside handlers when necessary; the same permission is not re-checked if already declared.



\---



\## 8. API Integration for Clients



\### 8.1 Cart Endpoints



```http

POST   /api/carts

GET    /api/carts/me

POST   /api/carts/items

PUT    /api/carts/items/{productVariantId}

DELETE /api/carts/items/{productVariantId}

DELETE /api/carts

```



\### 8.2 Order Endpoints



```http

POST /api/orders/place

GET  /api/orders/{id}

GET  /api/orders/customer

GET  /api/orders

POST /api/orders/{id}/cancel

```



Controllers translate HTTP requests into MediatR commands and queries via `ISender`. They contain no business logic.



\### 8.3 Payment Initiation (Client → Finance)



Ordering does not expose a payment endpoint. After `POST /api/orders/place` returns an `OrderId`, the client calls Finance directly:



```http

POST /api/payments/initiate

```



Request fields:



\* `OrderId`

\* `Amount`

\* `Currency`

\* `Method`

\* `ReturnUrl`

\* `CancelUrl`



Finance returns a Paymob checkout URL. The client redirects the customer there. This is a client-to-Finance contract; Ordering is not involved in the initiation call.



\---



\## 9. Error Handling and Recovery



\* If stock decrease fails during checkout, the transaction rolls back; no Order is created.

\* If shipping fee calculation fails or the address is outside the service area, checkout fails and no Order is created. The failure is returned as a business error.

\* If the client never calls Finance's `POST /api/payments/initiate`, the Order remains `PendingPayment` and is eventually cancelled by the expiration job.

\* If Finance publishes `PaymentFailedIntegrationEvent`, the Order transitions to `PaymentFailed`. It is not automatically cancelled; the user can cancel explicitly.

\* Out-of-order Shipping events are rejected by the aggregate. They follow the integration-event failure/retry policy and are not silently swallowed.

\* If Ordering crashes after Order creation but before Outbox delivery, the Outbox record is already committed and will be delivered by the dispatcher.

\* \*\*Expiry handler ordering:\*\* if `RestoreStockOnExpiredOrdersIntegrationEventHandler` runs before `CancelExpiredOrdersIntegrationEventHandler`, it throws because the Order is not yet `Cancelled`. The Outbox marks the message `Failed` and retries. On retry, cancellation has committed and stock restoration completes. The worst case is one extra dispatch cycle.

\* \*\*Concurrent cancellation:\*\* user-initiated cancellation and expiry cancellation can race on the same Order. `RowVersion` on Order protects against double cancellation. The losing path throws `ConcurrencyException`, rolls back, and the Outbox retries. On retry, the Order is already `Cancelled` and the handler skips it.



\---



\## 10. Testing Integration



Focus on integration boundaries:



\* \*\*Event contracts:\*\* verify payloads and ownership.

\* \*\*Outbox persistence:\*\* ensure outbound integration events are stored atomically.

\* \*\*Inbound event consumption:\*\* validate idempotency and state transitions.

\* \*\*Duplicate delivery and idempotency:\*\* confirm duplicate events are safely ignored.

\* \*\*Cross-context transaction participation:\*\* test atomicity of Order, stock, and Outbox.

\* \*\*Expiry flow:\*\* verify that the two split handlers converge regardless of execution order, and that stock restoration is idempotent under retry.

\* \*\*Client-orchestrated payment initiation:\*\* verify that Ordering does not depend on Finance for initiation and that the Order remains `PendingPayment` until Finance publishes an outcome event.

\* \*\*API contracts and authorization:\*\* verify role-based access and resource isolation.



\---



\## 11. Summary



Ordering integrates with:



\* \*\*Catalog\*\* via `IStockService` for synchronous stock changes and restoration.

\* \*\*Shipping\*\* via `IShippingFeeCalculator` for fee calculation and via integration events for shipment status.

\* \*\*Finance\*\* via integration events for payment and refund \*\*outcomes only\*\*. Ordering does not call Finance or Paymob directly, and Finance does not consume any Ordering event. Payment initiation is client-orchestrated: the client calls `POST /api/payments/initiate` on Finance after placing the Order.

\* \*\*Clients\*\* via REST APIs.



The design maintains clear ownership boundaries, uses shared contracts for synchronous operations, and Outbox events for asynchronous communication. Domain event handlers translate domain facts into integration events, while the Outbox provides durable publication semantics.



`OrderPlacedIntegrationEvent` is Ordering-internal — it is consumed by Ordering's own `SendOrderConfirmationEmailHandler` for the confirmation email. It is not a cross-context contract.



The expiry flow is deliberately split: the recurring job detects and publishes, while cancellation and stock restoration are performed by two independent integration event handlers. This keeps the job stateless, makes each mutation path independently retryable, and preserves the same domain event and outbound event path used by user-initiated cancellation.



\---



\*\*Document version:\*\* 2.3 – Final

\*\*Feature:\*\* Ordering Integration Guide



