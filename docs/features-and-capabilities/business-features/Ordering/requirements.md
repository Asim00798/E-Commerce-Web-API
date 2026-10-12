\# Ordering Feature — Requirements Technical Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Bounded Context:\*\* Ordering — Cart, Order, Checkout Integration

\*\*Version:\*\* 1.4 — Final



\---



\## 1. Introduction



This document defines the technical requirements for the Ordering bounded context within the e-commerce modular monolith.



Ordering is responsible for managing customer purchase intent from cart creation through order placement and the complete order lifecycle. It integrates with Catalog for stock and product data, Shipping for shipping fees and delivery status, and Finance for payment and refund outcomes.



The feature spans Domain, Application, Infrastructure, and API layers, following Clean Architecture and DDD principles. The Domain layer is independent of infrastructure concerns.



\---



\## 2. Scope



\### 2.1 In Scope



\* Shopping cart management.

\* Cart item management.

\* Order creation from cart.

\* Order line snapshot at placement.

\* Order status lifecycle.

\* Order cancellation (user-initiated and system-initiated via expiration).

\* Integration with Catalog stock via `IStockService`.

\* Integration with Shipping fee calculation via `IShippingFeeCalculator`.

\* Consumption of Shipping integration events for delivery status.

\* Consumption of Finance integration events for payment/refund status.

\* Publishing order-related integration events for downstream contexts.

\* Pagination and querying of orders.

\* Authorization at API and Application layers.



\### 2.2 Out of Scope



\* Payment processing itself (owned by Finance).

\* Refund processing itself (owned by Finance).

\* Shipping/shipment lifecycle (owned by Shipping).

\* Invoice generation/billing documents (not currently required).

\* Product catalog management (owned by Catalog).

\* Customer identity/authentication (owned by Identity/Auth).

\* Promotion/discount engine (not required initially).



\---



\## 3. Business Responsibility



Ordering answers:



> What does the customer want to buy, what are they charged, and what is the current state of their order?



\### Ordering Owns



\* Cart and cart items.

\* Order and order items.

\* Order totals, including shipping fee and snapshot prices.

\* Order status and lifecycle.

\* Order cancellation.

\* Customer/order association.



\### Ordering Does Not Own



\* Payment lifecycle (Finance).

\* Refund lifecycle (Finance).

\* Shipment lifecycle (Shipping).

\* Product master data (Catalog).

\* Stock quantity authority (Catalog today, possibly Inventory later).

\* Customer identity master data (Identity).

\* Customer personal data (People).



\---



\## 4. Domain Model



\### 4.1 Aggregates



\#### Cart



Represents a customer's active shopping cart.



\*\*Properties\*\*



\* `Id`

\* `CustomerId`

\* `Items` — collection of `CartItem` entities



\*\*Behaviors\*\*



\* `Create`

\* `AddItem`

\* `UpdateQuantity`

\* `RemoveItem`

\* `Clear`



\*\*Invariants\*\*



\* A cart belongs to exactly one customer.

\* Duplicate product variants are merged by quantity rather than stored as duplicate lines.

\* Quantity must be positive.

\* Removed or nonexistent items are ignored.



\*\*Cart Price Semantics\*\*



`CartItem.UnitPrice` is the current displayed price when the item was added to the cart. It is \*\*not\*\* the authoritative locked price for checkout.



In the current implementation, the cart item data is used as the snapshot for the order line because no live price lookup is performed during checkout.



If authoritative price refresh becomes a requirement, a separate product snapshot provider can be introduced later. The contract would live in Application, and Ordering would depend on it rather than on Catalog directly.



\#### Order



Represents a placed order.



\*\*Properties\*\*



\* `Id`

\* `CustomerId`

\* `OrderItems` — collection of `OrderItem` entities

\* `Status`

\* `Subtotal`

\* `ShippingFee`

\* `Total`

\* `PlacedAtUtc`

\* `CancelledAtUtc`

\* `DeliveredAtUtc`

\* `RefundedAtUtc`



\*\*Behaviors\*\*



\* `Place` — from cart, product information snapshot, and shipping fee.

\* `MarkPaymentFailed` — from Finance event.

\* `MarkPaid` — from Finance event.

\* `MarkShipped` — from Shipping event.

\* `MarkDelivered` — from Shipping event.

\* `MarkReturned` — from Shipping event.

\* `MarkRefunded` — from Finance event.

\* `Cancel` — if allowed.



\*\*Invariants\*\*



\* An order must have at least one order item.

\* Order item prices are snapshot values at placement time, currently taken from the cart.

\* Shipping fee is finalized at placement time.

\* `Total = Subtotal + ShippingFee`.

\* An order cannot transition from terminal states.

\* Cancellation is allowed only from `PendingPayment` or `PaymentFailed`.

\* An order can be refunded only after reaching a refundable state: `Paid`, `Delivered`, or `Returned`.

\* An order can be marked delivered only after it has been shipped.

\* `PaymentFailed` is distinct from `Cancelled`.



\*\*Delivery Address Note\*\*



The Order aggregate does not store delivery address or customer contact information. The linked Person record provides current data when needed. Shipping later stores the delivery address as its own immutable `DeliveryAddressSnapshot`.



\### 4.2 Child Entities



\* \*\*`CartItem`\*\* — owned by Cart. Contains `ProductId`, `ProductVariantId`, `Sku`, `ProductName`, `VariantName`, `UnitPrice`, and `Quantity`.

\* \*\*`OrderItem`\*\* — owned by Order. Contains an immutable snapshot of the product and variant at order placement.



\### 4.3 Enums



```csharp

public enum OrderStatus

{

&#x20;   PendingPayment = 1,

&#x20;   PaymentFailed = 2,

&#x20;   Paid = 3,

&#x20;   Shipped = 4,

&#x20;   Delivered = 5,

&#x20;   Cancelled = 6,

&#x20;   Returned = 7,

&#x20;   Refunded = 8

}

```



No `Processing`, no `PartiallyRefunded`, and no unused states.



\*\*Terminal states:\*\* `Cancelled`, `Refunded`.



`Returned` \*\*is not terminal\*\*; it can transition to `Refunded`.



\*\*Expiration eligibility:\*\* Only orders in `PendingPayment` are eligible for system-initiated expiration. `PaymentFailed` orders are not processed by the expiration job; they require explicit user action through `CancelOrderCommand` to transition to `Cancelled`.



\### 4.4 Repositories



```csharp

ICartRepository : IRepository<Cart>



IOrderRepository : IRepository<Order>

```



\---



\## 5. Domain Events



Domain events are raised by the aggregates. Handlers are implemented in the Application layer. The aggregates never reference integration events.



\* `OrderPlacedDomainEvent`

\* `OrderCancelledDomainEvent`

\* `OrderPaidDomainEvent`



Other order status changes do not require domain events unless an internal handler needs to react.



\---



\## 6. Integration Events



\### 6.1 Event Ownership



| Event                                   | Owner    | Purpose                                                                                              |

| --------------------------------------- | -------- | ---------------------------------------------------------------------------------------------------- |

| `OrderPlacedIntegrationEvent`           | Ordering | Finance starts the payment process.                                                                  |

| `OrderReadyForShippingIntegrationEvent` | Ordering | Shipping creates a shipment after payment.                                                           |

| `OrderCancelledIntegrationEvent`        | Ordering | Notifies consumers; stock has already been restored by Ordering.                                     |

| `OrdersExpiredIntegrationEvent`         | Ordering | Internal event that triggers cancellation and stock restoration for expired `PendingPayment` orders. |

| `PaymentCompletedIntegrationEvent`      | Finance  | Ordering marks the order as Paid.                                                                    |

| `PaymentFailedIntegrationEvent`         | Finance  | Ordering marks the order as PaymentFailed.                                                           |

| `RefundCompletedIntegrationEvent`       | Finance  | Ordering marks the order as Refunded.                                                                |

| `RefundFailedIntegrationEvent`          | Finance  | Ordering does not change order state; it may log the failure.                                        |

| `ShipmentShippedIntegrationEvent`       | Shipping | Ordering marks the order as Shipped.                                                                 |

| `ShipmentDeliveredIntegrationEvent`     | Shipping | Ordering marks the order as Delivered.                                                               |

| `ShipmentReturnedIntegrationEvent`      | Shipping | Ordering marks the order as Returned.                                                                |



\### 6.2 Outbound from Ordering



\* `OrderPlacedIntegrationEvent`

\* `OrderReadyForShippingIntegrationEvent`

\* `OrderCancelledIntegrationEvent`

\* `OrdersExpiredIntegrationEvent` — internal to Ordering; not intended for cross-context consumption.



The first three are written to the Outbox by Application domain event handlers, atomically with the Order state change, and delivered later by the Outbox dispatcher.



`OrdersExpiredIntegrationEvent` is the exception: it is written directly by the job handler using `IOutboxMessageWriter.WriteAsync` as part of the detection-only flow, without a preceding aggregate state change.



\### 6.3 Inbound to Ordering



\*\*From Shipping\*\*



\* `ShipmentShippedIntegrationEvent`

\* `ShipmentDeliveredIntegrationEvent`

\* `ShipmentReturnedIntegrationEvent`



\*\*From Finance\*\*



\* `PaymentCompletedIntegrationEvent`

\* `PaymentFailedIntegrationEvent`

\* `RefundCompletedIntegrationEvent`

\* `RefundFailedIntegrationEvent`



Ordering consumes these events and updates its own Order aggregate.



\---



\## 7. Integration with Other Contexts



\### 7.1 Catalog



Ordering does not access Catalog repositories directly.



It uses:



\* `IStockService` for synchronous stock decrease during order placement and stock restoration on cancellation, both user-initiated and system-expiry.



Order items store an immutable snapshot of the product and variant at order placement. They do not reference live Catalog data.



`IStockService` is an intentional cross-context contract. In the current modular monolith, it can participate in the same database transaction as the caller's Unit of Work save. This is a deliberate optimization, not a DDD rule.



If Catalog becomes a separate service later, the implementation changes, but the contract remains.



\### 7.2 Shipping



Ordering calls `IShippingFeeCalculator` synchronously during checkout to obtain a shipping fee.



\*\*Contract ownership:\*\* `IShippingFeeCalculator` is a shared contract consumed by Ordering; its implementation is owned by Shipping. Ordering does not depend on Shipping's internal implementation.



Ordering consumes shipping integration events for shipment status. It does not call Shipping APIs directly.



\### 7.3 Finance



Ordering does not initiate payment directly. It publishes `OrderPlacedIntegrationEvent`. Finance consumes the event and starts the payment process.



The client obtains the payment URL from the Finance API, not from Ordering.



\*\*Client–Finance Correlation\*\*



The payment workflow must define how the client obtains the payment URL after Ordering publishes `OrderPlacedIntegrationEvent`.



The accepted approach is:



```text

Client places order

&#x20;       |

&#x20;       v

Ordering publishes OrderPlacedIntegrationEvent

&#x20;       |

&#x20;       v

Finance creates Payment and payment session

&#x20;       |

&#x20;       v

Client queries Finance (e.g., by OrderId) to retrieve the payment URL

&#x20;       |

&#x20;       v

Client redirects to Paymob

```



This correlation mechanism must be explicitly implemented. It is an API/workflow detail, not a DDD violation.



\*\*Ordering consumes Finance integration events:\*\*



\* `PaymentCompletedIntegrationEvent` → `Order.MarkPaid()` → publish `OrderReadyForShippingIntegrationEvent`.

\* `PaymentFailedIntegrationEvent` → `Order.MarkPaymentFailed()`; does not automatically cancel the order.

\* `RefundCompletedIntegrationEvent` → `Order.MarkRefunded()`.

\* `RefundFailedIntegrationEvent` → no order state change; log only.



\*\*Refund ownership:\*\* Finance owns the refund process. Ordering only updates its own Order state when it receives `RefundCompletedIntegrationEvent`.



If partial refunds are needed later, a separate `PartiallyRefunded` status and corresponding Finance events must be defined. They are not currently required.



\### 7.4 People — Personal Data



Ordering depends on People's `IPersonRepository` at the Application layer, not the Domain layer.



This repository is used in Application domain event handlers to fetch customer contact information and delivery address when needed.



\* \*\*During checkout:\*\* Fetch the Person data required by `IShippingFeeCalculator`, including delivery address and map URL. The address is not stored on the Order.

\* \*\*After payment:\*\* `OrderPaidDomainEventHandler` fetches the current delivery address from Person to build `OrderReadyForShippingIntegrationEvent`. Shipping then stores it as its own immutable snapshot.



\### 7.5 File Storage



Ordering does not need file storage initially.



\---



\## 8. Application Layer



\### 8.1 Commands



| Command                         | Purpose                           | Permission Attribute         |

| ------------------------------- | --------------------------------- | ---------------------------- |

| `CreateCartCommand`             | Create a new cart for a customer. | `OrderingPermissions.Place`  |

| `AddCartItemCommand`            | Add an item to a cart.            | `OrderingPermissions.Place`  |

| `UpdateCartItemQuantityCommand` | Update item quantity.             | `OrderingPermissions.Place`  |

| `RemoveCartItemCommand`         | Remove an item from a cart.       | `OrderingPermissions.Place`  |

| `ClearCartCommand`              | Clear all items from a cart.      | `OrderingPermissions.Place`  |

| `PlaceOrderCommand`             | Place an order from a cart.       | `OrderingPermissions.Place`  |

| `CancelOrderCommand`            | Cancel an order if allowed.       | `OrderingPermissions.Cancel` |



\*\*Rule:\*\* Command handlers do not write directly to the Outbox. Domain events raised by aggregates are handled by dedicated domain event handlers.



\### 8.2 Queries



| Query                      | Purpose                             | Permission Attribute         |

| -------------------------- | ----------------------------------- | ---------------------------- |

| `GetCartByCustomerIdQuery` | Get the customer's active cart.     | `OrderingPermissions.Read`   |

| `GetOrderByIdQuery`        | Get an order by ID.                 | `OrderingPermissions.Read`   |

| `GetCustomerOrdersQuery`   | Retrieve paginated customer orders. | `OrderingPermissions.Read`   |

| `ListOrdersQuery`          | Admin/support query with filters.   | `OrderingPermissions.Manage` |



Queries include resource authorization in the handler. For example, a customer can access only their own cart and orders unless they have `Manage` permission.



\### 8.3 Domain Event Handlers



| Handler                                       | Purpose                                                                                                                                                                              |

| --------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |

| `OrderPlacedDomainEventHandler`               | Creates `OrderPlacedIntegrationEvent` and writes it to the Outbox. Fetches Person data only when the integration-event contract requires customer information such as email or name. |

| `OrderCancelledDomainEventHandler`            | Creates `OrderCancelledIntegrationEvent` and writes it to the Outbox.                                                                                                                |

| `OrderPaidDomainEventHandler`                 | Fetches the current delivery address from Person, creates `OrderReadyForShippingIntegrationEvent`, and writes it to the Outbox.                                                      |

| `PersistUserNotificationOnOrderPlacedHandler` | Creates an in-app notification and a post-commit SignalR hint.                                                                                                                       |



\### 8.4 Integration Event Handlers



\*\*From Finance\*\*



\* `PaymentCompletedIntegrationEventHandler` → `Order.MarkPaid()`.

\* `PaymentFailedIntegrationEventHandler` → `Order.MarkPaymentFailed()`.

\* `RefundCompletedIntegrationEventHandler` → `Order.MarkRefunded()`.

\* `RefundFailedIntegrationEventHandler` → log only.



\*\*From Shipping\*\*



\* `ShipmentShippedIntegrationEventHandler` → `Order.MarkShipped()`.

\* `ShipmentDeliveredIntegrationEventHandler` → `Order.MarkDelivered()`.

\* `ShipmentReturnedIntegrationEventHandler` → `Order.MarkReturned()`.

\* `ScheduleFeedbackRequestOnOrderDeliveredHandler` → schedules the feedback job.



\*\*Expiration flow — Ordering-internal, reacting to `OrdersExpiredIntegrationEvent`\*\*



\* `CancelExpiredOrdersIntegrationEventHandler` → calls `Order.Cancel()` for each order. Each cancellation raises `OrderCancelledDomainEvent`, which the domain event handler translates into `OrderCancelledIntegrationEvent` in the Outbox.

\* `RestoreStockOnExpiredOrdersIntegrationEventHandler` → calls `IStockService.IncreaseStockAsync` for each item, gated on the order being `Cancelled`.



\*\*Local integration event handlers — Ordering's own outbound events\*\*



Ordering subscribes to some of its own outbound events to perform durable external side effects that must run outside the checkout transaction and receive Outbox retry semantics.



\* `SendOrderConfirmationEmailHandler` → reacts to `OrderPlacedIntegrationEvent` and sends the confirmation email via `IEmailChannel`.



This is deliberate self-consumption. The distinction from Section 8.3 is transactional scope:



\* Reactions that must commit with the order, such as the in-app notification row, use domain event handlers.

\* Durable external side effects use integration event handlers, even when the event originates from the same context.



The Outbox provides retry and external isolation, not cross-context delivery in this case.



All inbound handlers are idempotent through the existing `IdempotentIntegrationEventHandler<T>` decorator.



\---



\## 9. Authorization



\### 9.1 Permissions



```csharp

public static class OrderingPermissions

{

&#x20;   public const string Read = "Orders.Read";

&#x20;   public const string Place = "Orders.Place";

&#x20;   public const string Cancel = "Orders.Cancel";

&#x20;   public const string Manage = "Orders.Manage";

}

```



\### 9.2 API Layer



Controllers use role-based authorization:



\* Customers can view their own orders and cart.

\* Administrators and Support users can manage or query all orders.



\### 9.3 Application Layer



Commands and queries use `\[AuthorizePermission]` attributes.



\*\*Resource authorization:\*\*



\* Customers can access only their own cart and orders.

\* Order cancellation requires ownership or `Orders.Manage` permission.



\---



\## 10. Workflows



\### 10.1 Checkout / Place Order



```text

Customer has cart

&#x20;       |

&#x20;       v

PlaceOrderCommand

&#x20;       |

&#x20;       v

Load cart (including items)

&#x20;       |

&#x20;       v

Fetch Person data required by IShippingFeeCalculator

&#x20;       |

&#x20;       v

Calculate ShippingFee via IShippingFeeCalculator (outside transaction)

&#x20;       |

&#x20;       v

Create Order with OrderItem snapshots (from cart) and ShippingFee

&#x20;       |

&#x20;       v

Decrease stock via IStockService (same transaction)

&#x20;       |

&#x20;       v

Clear cart

&#x20;       |

&#x20;       v

Create Outbox record for OrderPlacedIntegrationEvent

(via OrderPlacedDomainEventHandler)

&#x20;       |

&#x20;       v

UnitOfWork.SaveChangesAsync

(Order + stock + cart + Outbox)

&#x20;       |

&#x20;       v

Commit

&#x20;       |

&#x20;       v

Outbox dispatcher later publishes OrderPlacedIntegrationEvent

```



\*\*Important:\*\* Shipping fee calculation occurs outside the transaction. Only the resulting value is persisted inside the order transaction.



Checkout uses manual transaction mode because Phase 1 performs cross-context synchronous reads outside the transaction.



\### 10.2 Payment Completed



```text

Finance publishes PaymentCompletedIntegrationEvent

&#x20;       |

&#x20;       v

Ordering handler → Order.MarkPaid()

&#x20;       |

&#x20;       v

OrderPaidDomainEventHandler fetches current delivery address from Person,

creates OrderReadyForShippingIntegrationEvent, and writes to Outbox

&#x20;       |

&#x20;       v

Save Order + Outbox

&#x20;       |

&#x20;       v

Commit

&#x20;       |

&#x20;       v

Outbox dispatcher later publishes OrderReadyForShippingIntegrationEvent

```



\### 10.3 Payment Failed



```text

Finance publishes PaymentFailedIntegrationEvent

&#x20;       |

&#x20;       v

Ordering handler → Order.MarkPaymentFailed()

&#x20;       |

&#x20;       v

Save

```



There is no automatic cancellation or stock restoration.



`PaymentFailed` orders are not processed by the expiration job. They require explicit user action through `CancelOrderCommand` to transition to `Cancelled`.



\### 10.4 Shipment Shipped



```text

Shipping publishes ShipmentShippedIntegrationEvent

&#x20;       |

&#x20;       v

Ordering handler → Order.MarkShipped()

&#x20;       |

&#x20;       v

Save

```



`ShipmentShippedIntegrationEvent` is raised when the shipment reaches `OutForDelivery`. This is an intentional business mapping between Shipping's lifecycle and Ordering's lifecycle.



\### 10.5 Shipment Delivered



```text

Shipping publishes ShipmentDeliveredIntegrationEvent

&#x20;       |

&#x20;       v

Ordering handler → Order.MarkDelivered()

&#x20;       |

&#x20;       v

Save

&#x20;       |

&#x20;       v

ScheduleFeedbackRequestOnOrderDeliveredHandler

schedules the feedback job

```



\### 10.6 Refund Completed



```text

Finance publishes RefundCompletedIntegrationEvent

&#x20;       |

&#x20;       v

Ordering handler → Order.MarkRefunded()

&#x20;       |

&#x20;       v

Save

```



\### 10.7 Cancellation — User-Initiated



```text

CancelOrderCommand

&#x20;       |

&#x20;       v

Load order and verify status

&#x20;       |

&#x20;       v

Resource authorization (owner or Manage)

&#x20;       |

&#x20;       v

Order.Cancel()

&#x20;       |

&#x20;       v

Restore stock via IStockService.IncreaseStockAsync

&#x20;       |

&#x20;       v

Create Outbox record for OrderCancelledIntegrationEvent

&#x20;       |

&#x20;       v

Save Order + stock + Outbox in one Unit of Work

&#x20;       |

&#x20;       v

Commit

&#x20;       |

&#x20;       v

Outbox dispatcher later publishes OrderCancelledIntegrationEvent

```



\*\*Stock restoration on cancellation:\*\* Restoration occurs in the same transaction. This is a deliberate design decision: restoration is coupled to the cancellation operation, not to `PaymentFailed`.



\### 10.8 Cancellation — System Expiry



Expiration is split into detection in the job handler and mutation in two dedicated integration event handlers.



```text

Hangfire trigger → ExpirePendingOrdersJob

&#x20;       |

&#x20;       v

ExpirePendingOrdersJobHandler

&#x20;       |

&#x20;       ├── Query IPendingOrderCleanupService.GetExpiredPendingOrderIdsAsync

&#x20;       |       └── Returns IDs of PendingPayment orders older than threshold

&#x20;       |

&#x20;       ├── If empty, log and return

&#x20;       |

&#x20;       ├── Build OrdersExpiredIntegrationEvent(expiredOrderIds)

&#x20;       |

&#x20;       ├── IOutboxMessageWriter.WriteAsync(event)

&#x20;       |

&#x20;       └── UnitOfWork.SaveChangesAsync

&#x20;               (single save — Outbox row)

&#x20;       |

&#x20;       v

Outbox dispatcher delivers OrdersExpiredIntegrationEvent

&#x20;       |

&#x20;       v

Two independent handlers react

```



\*\*Handler 1 — `CancelExpiredOrdersIntegrationEventHandler`\*\*



For each order ID:



1\. Load the order; skip it if already `Cancelled` (idempotent).

2\. Skip it if it is no longer `PendingPayment`.

3\. Call `Order.Cancel()`, raising `OrderCancelledDomainEvent`.

4\. The domain event handler writes `OrderCancelledIntegrationEvent` to the Outbox.

5\. Save changes.

6\. Commit the per-order transaction.



\*\*Handler 2 — `RestoreStockOnExpiredOrdersIntegrationEventHandler`\*\*



For each order ID:



1\. Load the order.

2\. If its status is not `Cancelled`, throw an exception so the Outbox retries.

3\. For each item, call `IStockService.IncreaseStockAsync`.

4\. Save changes.

5\. Commit the per-order transaction.



\*\*Convergence:\*\* The two handlers converge regardless of execution order. If the stock handler runs first, it throws because the order is not yet `Cancelled`. The Outbox marks the message as Failed and retries after cancellation has committed.



\*\*Concurrency with user-initiated cancellation:\*\* `RowVersion` on Order protects against double cancellation. The losing path throws `ConcurrencyException`, rolls back, and the Outbox message is retried. On retry, the order is already `Cancelled` and the handler skips it.



\---



\## 11. Transaction Boundaries



\* \*\*Checkout:\*\* Order persistence, stock decrease, cart clearing, and the `OrderPlacedIntegrationEvent` Outbox record are committed in one database transaction. Shipping fee calculation occurs outside the transaction and only returns a value. Checkout uses manual transaction mode (`BeginTransactionAsync` / `CommitTransactionAsync`).

\* \*\*Payment/fulfilment updates:\*\* Each integration event is processed in its own transaction, updating only Ordering's state and writing any resulting Outbox records atomically.

\* \*\*Cancellation (user-initiated):\*\* Order cancellation, stock restoration, and the `OrderCancelledIntegrationEvent` Outbox record are committed in one transaction.

\* \*\*Expiry detection (job):\*\* One transaction persists the `OrdersExpiredIntegrationEvent` Outbox record. No order state is mutated.

\* \*\*Expiry cancellation:\*\* Per-order transaction covering `Order.Cancel()` and the `OrderCancelledIntegrationEvent` Outbox record.

\* \*\*Expiry stock restoration:\*\* Per-order transaction covering stock increases only.

\* \*\*Cross-context transaction participation with Catalog:\*\* This is a deliberate modular-monolith design, not a required DDD pattern. It must be revisited if services are split.



\---



\## 12. Concurrency and Idempotency



\* `RowVersion` optimistic concurrency is enabled on Cart and Order.

\* Integration event handlers must be idempotent through the existing `IdempotentIntegrationEventHandler<T>` decorator.

\* Duplicate events (payment, shipping, or refund) must not cause duplicate state transitions.

\* A unique active cart per customer is enforced through a repository/application check. A database unique constraint can be added if stronger enforcement is required.

\* \*\*Expiration handler ordering:\*\* The two `OrdersExpiredIntegrationEvent` handlers are idempotent under duplicate delivery and convergent under any execution order. If the stock handler runs before cancellation commits, it throws and is retried.

\* \*\*Concurrent cancellation:\*\* User-initiated and expiry cancellation can race on the same order. `RowVersion` protects against double cancellation.



\---



\## 13. API Layer



\### Controllers



\* `CartsController`

\* `OrdersController`



\### Endpoints



```http

\# Cart

POST   /api/carts

GET    /api/carts/me

POST   /api/carts/items

PUT    /api/carts/items/{productVariantId}

DELETE /api/carts/items/{productVariantId}

DELETE /api/carts



\# Orders

POST   /api/orders/place

GET    /api/orders/{id}

GET    /api/orders/customer

GET    /api/orders

POST   /api/orders/{id}/cancel

```



Controllers are thin. They translate HTTP requests into MediatR commands and queries via `ISender`.



\---



\## 14. Non-Functional Requirements



\### 14.1 Performance



\* Cart operations should be fast.

\* Order placement should be optimized to avoid loading the entire catalog.

\* Order list queries should use pagination.



\### 14.2 Scalability



\* The API should be stateless.

\* Concurrency is protected by aggregate invariants and `RowVersion`.



\### 14.3 Reliability



\* Order placement must atomically persist the Order, cart changes, and stock mutation. The shipping fee is calculated beforehand, and its value is persisted as part of the order transaction.

\* Integration event processing must be idempotent.

\* Invalid state transitions must be rejected by the aggregate.

\* Expiration split handlers must converge under any execution order and be idempotent under retry.



\### 14.4 Maintainability



\* The Domain layer must remain independent of EF Core and ASP.NET Core.

\* Application handlers orchestrate across contexts through contracts.

\* API controllers remain thin.



\### 14.5 Testability



\* Domain tests for order and cart invariants.

\* Application tests with mocked services and repositories.

\* Integration tests for event handlers and order placement, including the expiration split-handler flow.



\---



\## 15. Security Requirements



\* Customers can access only their own cart and orders.

\* Order cancellation requires permission.

\* Order totals and personal data must not leak to unauthorized users.

\* Integration events must not expose unnecessary internal identifiers.



\---



\## 16. Configuration Requirements



Ordering consumes the `Ordering` configuration section through `OrderingOptions`.



```json

{

&#x20; "Ordering": {

&#x20;   "MaximumItemsPerCart": 500,

&#x20;   "PendingOrderExpirationHours": 24

&#x20; }

}

```



| Property                      | Purpose                                                                    | Default |

| ----------------------------- | -------------------------------------------------------------------------- | ------: |

| `MaximumItemsPerCart`         | Upper bound on distinct items per cart.                                    |     500 |

| `PendingOrderExpirationHours` | Threshold after which `PendingPayment` orders are eligible for expiration. |      24 |



Configuration is bound and validated with `ValidateOnStart`.



No secrets are required for Ordering itself. The section documents the intended configuration; missing values fall back to the defaults above.



\---



\## 17. Assumptions and Constraints



\### 17.1 Assumptions



\* The Shipping fee calculator exists and is registered.

\* The stock service exists and is registered.

\* Cart is persisted in the database as an aggregate.

\* Payment is handled by Finance; Ordering only publishes `OrderPlacedIntegrationEvent`.

\* Order status is updated through integration events from Shipping and Finance.

\* Stock decrease and restoration happen synchronously within the same transaction by deliberate design.

\* The client obtains the payment URL from the Finance API after `OrderPlacedIntegrationEvent` is delivered.



\### 17.2 Constraints



\* Ordering must not directly modify Catalog, Shipping, or Finance tables.

\* Ordering must not implement payment processing.

\* Ordering must not implement the shipment lifecycle.

\* Cart and Order aggregate boundaries must be respected.

\* Ordering must not call Paymob or any payment provider directly.



\---



\## 18. Traceability Matrix



| Requirement Area | Description                                                               |

| ---------------- | ------------------------------------------------------------------------- |

| Cart             | Create, add/update/remove item, and clear.                                |

| Order            | Placement, lifecycle, cancellation, and totals.                           |

| Expiration       | Detection job, split handlers, and stock restoration.                     |

| Integration      | Catalog stock, Shipping fee, Shipping events, Finance events, and People. |

| Authorization    | Roles/permissions and resource isolation.                                 |

| Concurrency      | `RowVersion` and idempotency.                                             |

| API              | Carts and Orders controllers.                                             |

| Testing          | Domain, Application, and integration testing.                             |



\---



\## 19. Acceptance Criteria



Ordering is considered complete when:



1\. The Cart aggregate enforces all invariants.

2\. The Order aggregate enforces the lifecycle, including the distinction between `PaymentFailed` and `Cancelled`.

3\. Order placement atomically persists the Order, cart, and stock mutation. The shipping fee is calculated beforehand and persisted as part of the order.

4\. Ordering consumes Shipping and Finance integration events correctly.

5\. Ordering publishes outbound integration events through the Outbox:



&#x20;  \* `OrderPlacedIntegrationEvent`

&#x20;  \* `OrderReadyForShippingIntegrationEvent`

&#x20;  \* `OrderCancelledIntegrationEvent`

&#x20;  \* `OrdersExpiredIntegrationEvent` — internal, detection flow only.

6\. The expiration flow is split: the job detects and publishes; two independent integration event handlers cancel and restore stock per order.

7\. Expiration handlers converge regardless of execution order and are idempotent under retry.

8\. Customer isolation and authorization are enforced.

9\. No direct cross-context table access occurs.

10\. Both user-initiated and expiry cancellation restore stock.

11\. Unit and integration tests cover critical paths, including the expiration split-handler flow.



\---



\## 20. Summary



Ordering is a core bounded context that handles cart and order lifecycles. It integrates with Catalog, Shipping, Finance, and People through shared contracts and integration events, preserving clear boundaries.



\### Key Clarifications in This Revision



\* Cart price is a display value; the order line snapshot is taken from the cart. No live Catalog price lookup occurs during checkout.

\* Order does not store the delivery address. The address is read from People when `OrderReadyForShipping` is created and snapshotted by Shipping.

\* Expiration is a system-initiated flow distinct from payment failure. It targets only `PendingPayment` orders, cancels them through the same `Order.Cancel()` path used by user-initiated cancellation, and restores stock. The job is detection-only; mutation is performed by two independent integration event handlers.

\* Intra-context reactions that must be transactional use domain event handlers. Intra-context durable external side effects, such as email, use integration event handlers routed through the Outbox for retry and external isolation. This is deliberate self-consumption.

\* Configuration is bound from the `Ordering` section through `OrderingOptions` with `ValidateOnStart`.



The result is an implementation-accurate requirements document that reflects the final architecture.



\---



\*\*Document Version:\*\* 1.4 — Final

\*\*Feature:\*\* Ordering / Cart and Order Lifecycle with Checkout Integration



