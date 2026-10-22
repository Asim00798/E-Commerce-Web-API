# Ordering Feature — Technical Reference Document

**Project:** E-Commerce Modular Monolith
**Bounded Context:** Ordering — Cart, Order, Checkout Integration
**Version:** 2.4 – Final

---

## 1. Introduction

The Ordering bounded context manages customer purchase intent from cart creation through order placement and the complete order lifecycle. It integrates with Catalog for stock and product data, Shipping for shipping fees and delivery status, and Finance for payment and refund outcomes.

The feature spans Domain, Application, Infrastructure, and API layers following Clean Architecture and DDD principles. The Domain layer is independent of infrastructure concerns.

---

## 2. Scope

### 2.1 In Scope

* Shopping cart management.
* Cart item management.
* Order creation from cart.
* Order line snapshot at placement.
* Order status lifecycle.
* Order cancellation (user-initiated and system-initiated via expiration).
* Integration with Catalog stock via `IStockService`.
* Integration with Shipping fee calculation via `IShippingFeeCalculator`.
* Consumption of Shipping integration events for delivery status.
* Consumption of Finance integration events for payment/refund status.
* Publishing order-related integration events for downstream contexts.
* Pagination and querying of orders.
* Authorization at API and Application layers.

### 2.2 Out of Scope

* Payment processing itself (owned by Finance).
* Refund processing itself (owned by Finance).
* Payment initiation (client-orchestrated directly with Finance).
* Shipping/shipment lifecycle (owned by Shipping).
* Invoice generation/billing documents (not currently required).
* Product catalog management (owned by Catalog).
* Customer identity/authentication (owned by Identity/Auth).
* Promotion/discount engine (not required initially).

---

## 3. Business Responsibility

Ordering answers:

> What does the customer want to buy, what is the order total, and what is the current state of the order?

### Ordering Owns

* Cart and cart items.
* Order and order items.
* Order totals, including shipping fee and snapshot prices.
* Order status and lifecycle.
* Order cancellation.
* Customer/order association.

### Ordering Does Not Own

* Payment lifecycle (Finance).
* Payment initiation (Finance; triggered by the client).
* Refund lifecycle (Finance).
* Shipment lifecycle (Shipping).
* Product master data (Catalog).
* Stock quantity authority (Catalog today, possibly Inventory later).
* Customer identity master data (Identity).
* Customer personal data (People).

---

## 4. Domain Model

### 4.1 Aggregates

#### Cart

Represents a customer's active shopping cart.

**Properties:**

* `Id`
* `CustomerId`
* `Items` — collection of `CartItem` entities.

**Behaviors:**

* `Create`
* `AddItem`
* `UpdateQuantity`
* `RemoveItem`
* `Clear`

**Invariants:**

* Cart belongs to exactly one customer.
* Duplicate product variants are merged by quantity rather than stored as duplicate lines.
* Quantity must be positive.
* Removed/nonexistent items are ignored.

**Price semantics:**

`CartItem.UnitPrice` is the current displayed price when the item was added to the cart. It is not the authoritative locked price for checkout. In the current implementation, the cart item data is used as the snapshot for the order line because no live price lookup is performed during checkout. If authoritative price refresh becomes a requirement, a separate product snapshot provider can be introduced later.

#### Order

Represents a placed order.

**Properties:**

* `Id`
* `CustomerId`
* `OrderItems` — collection of `OrderItem` entities.
* `Status`
* `Subtotal`
* `ShippingFee`
* `Total`
* `PlacedAtUtc`
* `CancelledAtUtc`
* `DeliveredAtUtc`
* `RefundedAtUtc`

**Behaviors:**

* `Place` — from cart, authoritative product info, and shipping fee.
* `MarkPaymentFailed` — from Finance event.
* `MarkPaid` — from Finance event.
* `MarkShipped` — from Shipping event.
* `MarkDelivered` — from Shipping event.
* `MarkReturned` — from Shipping event.
* `MarkRefunded` — from Finance event.
* `Cancel` — if allowed.

**Invariants:**

* Order must have at least one order item.
* Order item prices are snapshot values at placement time (currently taken from the cart).
* Shipping fee is finalized at placement time.
* `Total = Subtotal + ShippingFee`.
* Order cannot transition from terminal states.
* Cancellation is allowed only from certain statuses.
* Refunded only after the order has reached a refundable state (`Paid`, `Delivered`, or `Returned`).
* Delivered only after shipped.
* `PaymentFailed` is distinct from `Cancelled`.

**Note:** The Order aggregate does not store delivery address or customer contact information. The linked Person record provides current data at the moment it is needed. Shipping later stores the delivery address as its own immutable `DeliveryAddressSnapshot`.

### 4.2 Child Entities

* `CartItem` — owned by `Cart`. Contains `ProductId`, `ProductVariantId`, `Sku`, `ProductName`, `VariantName`, `UnitPrice`, and `Quantity`.
* `OrderItem` — owned by `Order`. Contains an immutable snapshot of the product/variant at order placement.

### 4.3 Enums

```csharp
public enum OrderStatus
{
    PendingPayment = 1,
    PaymentFailed = 2,
    Paid = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6,
    Returned = 7,
    Refunded = 8
}
```

No `Processing`, no `PartiallyRefunded`, and no unused states.

### 4.4 Domain Events

These are domain events raised by the aggregate. Their handlers are implemented in the Application layer. The aggregate never references integration events.

* `OrderPlacedDomainEvent`
* `OrderCancelledDomainEvent`
* `OrderPaidDomainEvent`

Other order status changes do not require domain events unless an internal handler needs to react.

### 4.5 State Transition Table

| Current State    | Trigger / Event                              | Next State      |
| ---------------- | -------------------------------------------- | --------------- |
| `PendingPayment` | `PaymentCompletedIntegrationEvent`           | `Paid`          |
| `PendingPayment` | `PaymentFailedIntegrationEvent`              | `PaymentFailed` |
| `PendingPayment` | `CancelOrderCommand`                         | `Cancelled`     |
| `PendingPayment` | Expiration (`OrdersExpiredIntegrationEvent`) | `Cancelled`     |
| `PaymentFailed`  | `CancelOrderCommand`                         | `Cancelled`     |
| `Paid`           | `ShipmentShippedIntegrationEvent`            | `Shipped`       |
| `Shipped`        | `ShipmentDeliveredIntegrationEvent`          | `Delivered`     |
| `Shipped`        | `ShipmentReturnedIntegrationEvent`           | `Returned`      |
| `Delivered`      | `ShipmentReturnedIntegrationEvent`           | `Returned`      |
| `Paid`           | `RefundCompletedIntegrationEvent`            | `Refunded`      |
| `Delivered`      | `RefundCompletedIntegrationEvent`            | `Refunded`      |
| `Returned`       | `RefundCompletedIntegrationEvent`            | `Refunded`      |

**Terminal states:** `Cancelled`, `Refunded`.

**Returned** is not terminal; it can transition to `Refunded`.

**Expiration eligibility:** Only orders in `PendingPayment` are eligible for system-initiated expiration. `PaymentFailed` orders are not processed by the expiration job; they require explicit user action (`CancelOrderCommand`) to transition to `Cancelled`.

### 4.6 Repositories

* `ICartRepository : IRepository<Cart>`
* `IOrderRepository : IRepository<Order>`

---

## 5. Integration Events

### 5.1 Event Ownership

| Event                                   | Owner    | Purpose                                                                                            |
| --------------------------------------- | -------- | -------------------------------------------------------------------------------------------------- |
| `OrderPlacedIntegrationEvent`           | Ordering | Ordering-internal; consumed by `SendOrderConfirmationEmailHandler` to send the confirmation email. |
| `OrderReadyForShippingIntegrationEvent` | Ordering | Shipping creates a shipment after payment.                                                         |
| `OrderCancelledIntegrationEvent`        | Ordering | Notifies consumers; stock has already been restored by Ordering.                                   |
| `OrdersExpiredIntegrationEvent`         | Ordering | Internal event; triggers cancellation and stock restoration for expired `PendingPayment` orders.   |
| `PaymentCompletedIntegrationEvent`      | Finance  | Ordering marks the order as `Paid`.                                                                |
| `PaymentFailedIntegrationEvent`         | Finance  | Ordering marks the order as `PaymentFailed`.                                                       |
| `RefundCompletedIntegrationEvent`       | Finance  | Ordering marks the order as `Refunded`.                                                            |
| `RefundFailedIntegrationEvent`          | Finance  | Ordering does not change order state; may log the failure.                                         |
| `ShipmentShippedIntegrationEvent`       | Shipping | Ordering marks the order as `Shipped`.                                                             |
| `ShipmentDeliveredIntegrationEvent`     | Shipping | Ordering marks the order as `Delivered`.                                                           |
| `ShipmentReturnedIntegrationEvent`      | Shipping | Ordering marks the order as `Returned`.                                                            |

### 5.2 Outbound Events from Ordering

* `OrderPlacedIntegrationEvent` — Ordering-owned; consumed locally by `SendOrderConfirmationEmailHandler`.
* `OrderReadyForShippingIntegrationEvent` — consumed by Shipping.
* `OrderCancelledIntegrationEvent` — consumed by Shipping and Notifications.
* `OrdersExpiredIntegrationEvent` — internal to Ordering; not intended for cross-context consumption.

`OrderReadyForShippingIntegrationEvent` and `OrderCancelledIntegrationEvent` are written to the Outbox by Application domain event handlers atomically with the Order state change, and delivered later by the Outbox dispatcher.

`OrdersExpiredIntegrationEvent` is written directly by the job handler (`IOutboxMessageWriter.WriteAsync`) as part of the detection-only flow, without a preceding aggregate state change.

`OrderPlacedIntegrationEvent` is written by `OrderPlacedDomainEventHandler` and consumed by Ordering's own `SendOrderConfirmationEmailHandler` for the confirmation email. **Finance does not consume it.** Payment initiation is a client-orchestrated flow (see §6.3).

### 5.3 Inbound Events to Ordering

**From Shipping:**

* `ShipmentShippedIntegrationEvent`
* `ShipmentDeliveredIntegrationEvent`
* `ShipmentReturnedIntegrationEvent`

**From Finance:**

* `PaymentCompletedIntegrationEvent`
* `PaymentFailedIntegrationEvent`
* `RefundCompletedIntegrationEvent`
* `RefundFailedIntegrationEvent`

All inbound handlers are idempotent, using the existing `IdempotentIntegrationEventHandler<T>` decorator.

---

## 6. Integration with Other Contexts

### 6.1 Catalog

Ordering does not access Catalog repositories directly. It uses `IStockService` for synchronous stock decrease during order placement and for stock restoration on cancellation (both user-initiated and system-expiry).

`IStockService` is a shared contract (owned by Catalog) that currently participates in the same database transaction in the modular monolith. This transactional model is specific to the monolith; a future distributed deployment would require a redesigned integration/consistency mechanism.

### 6.2 Shipping

Ordering calls `IShippingFeeCalculator` synchronously during checkout to obtain a shipping fee. The fee is persisted as a snapshot on the Order.

Ordering consumes shipping integration events for shipment status. It does not call Shipping APIs directly.

### 6.3 Finance

**Payment initiation is client-orchestrated. Ordering does not initiate payment, and Finance does not consume any Ordering integration event.**

The flow is:

```text
Client places order
       ↓
POST /api/orders/place → OrderId returned
       ↓
Client initiates payment
       ↓
POST /api/payments/initiate
(OrderId, Amount, Currency, Method, ReturnUrl, CancelUrl)
       ↓
Finance creates Payment (Pending)
       ↓
Finance calls Paymob Intention API
       ↓
Payment (AwaitingPayment); checkout URL returned
       ↓
Client redirects customer to Paymob
```

Ordering's role is limited to:

1. Producing the order in `PendingPayment` status via `PlaceOrderCommand`.
2. Consuming Finance integration events after payment outcomes are known.

Ordering consumes Finance integration events:

* `PaymentCompletedIntegrationEvent` → `Order.MarkPaid()` → then `OrderPaidDomainEventHandler` writes `OrderReadyForShippingIntegrationEvent` to the Outbox.
* `PaymentFailedIntegrationEvent` → `Order.MarkPaymentFailed()`; this does not automatically cancel the order.
* `RefundCompletedIntegrationEvent` → `Order.MarkRefunded()`.
* `RefundFailedIntegrationEvent` → no order state change; log only.

**Consequence for the Order lifecycle:** If the client never calls `POST /api/payments/initiate`, the Order remains `PendingPayment` until it is cancelled by the expiration job. There is no automatic initiation step from Ordering.

`OrderPlacedIntegrationEvent` **is not a Finance contract.** It is published to the Outbox and consumed by Ordering's own `SendOrderConfirmationEmailHandler` to send the confirmation email.

### 6.4 People (Personal Data)

Ordering depends on People's `IPersonRepository` at the Application layer, not Domain. This repository is used in Application domain event handlers to fetch customer contact and delivery address when needed.

* **During checkout:** Fetch the Person data required by `IShippingFeeCalculator` (for example, delivery address or map URL). The address is not stored on the Order.
* **After payment:** `OrderPaidDomainEventHandler` fetches the current delivery address from Person to build `OrderReadyForShippingIntegrationEvent`. Shipping then stores it as its own immutable snapshot.

---

## 7. Application Layer

### 7.1 Commands

| Command                         | Purpose                         | Permission Attribute         |
| ------------------------------- | ------------------------------- | ---------------------------- |
| `CreateCartCommand`             | Create a new cart for customer. | `OrderingPermissions.Place`  |
| `AddCartItemCommand`            | Add item to cart.               | `OrderingPermissions.Place`  |
| `UpdateCartItemQuantityCommand` | Update item quantity.           | `OrderingPermissions.Place`  |
| `RemoveCartItemCommand`         | Remove item from cart.          | `OrderingPermissions.Place`  |
| `ClearCartCommand`              | Clear all items from cart.      | `OrderingPermissions.Place`  |
| `PlaceOrderCommand`             | Place order from cart.          | `OrderingPermissions.Place`  |
| `CancelOrderCommand`            | Cancel an order if allowed.     | `OrderingPermissions.Cancel` |

No direct Outbox writes occur inside command handlers. Domain events raised by aggregates are handled by dedicated domain event handlers.

### 7.2 Queries

| Query                      | Purpose                           | Permission Attribute         |
| -------------------------- | --------------------------------- | ---------------------------- |
| `GetCartByCustomerIdQuery` | Get customer's active cart.       | `OrderingPermissions.Read`   |
| `GetOrderByIdQuery`        | Get order by ID.                  | `OrderingPermissions.Read`   |
| `GetCustomerOrdersQuery`   | Paginated customer orders.        | `OrderingPermissions.Read`   |
| `ListOrdersQuery`          | Admin/support query with filters. | `OrderingPermissions.Manage` |

Queries also include resource authorization in the handler (e.g., customers can access only their own carts/orders unless they have `Manage` permission).

### 7.3 Domain Event Handlers

| Handler                                       | Purpose                                                                                                                                                                                                                                                    |
| --------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `OrderPlacedDomainEventHandler`               | Creates `OrderPlacedIntegrationEvent` and writes it to the Outbox. Fetches Person data only when the integration-event contract requires customer information such as email or name. The event is consumed locally by `SendOrderConfirmationEmailHandler`. |
| `OrderCancelledDomainEventHandler`            | Creates `OrderCancelledIntegrationEvent` and writes it to the Outbox.                                                                                                                                                                                      |
| `OrderPaidDomainEventHandler`                 | Fetches the current delivery address from Person, creates `OrderReadyForShippingIntegrationEvent`, and writes it to the Outbox.                                                                                                                            |
| `PersistUserNotificationOnOrderPlacedHandler` | Creates an in-app notification and post-commit SignalR hint.                                                                                                                                                                                               |

### 7.4 Integration Event Handlers

All inbound integration handlers update the Order aggregate via its domain methods and persist through `IUnitOfWork`. They do not write to the Outbox directly, except where noted.

**From Finance:**

* `PaymentCompletedIntegrationEventHandler` → `Order.MarkPaid()`
* `PaymentFailedIntegrationEventHandler` → `Order.MarkPaymentFailed()`
* `RefundCompletedIntegrationEventHandler` → `Order.MarkRefunded()`
* `RefundFailedIntegrationEventHandler` → log only.

**From Shipping:**

* `ShipmentShippedIntegrationEventHandler` → `Order.MarkShipped()`
* `ShipmentDeliveredIntegrationEventHandler` → `Order.MarkDelivered()`
* `ShipmentReturnedIntegrationEventHandler` → `Order.MarkReturned()`

**Expiration flow (Ordering-internal, reacting to `OrdersExpiredIntegrationEvent`):**

* `CancelExpiredOrdersIntegrationEventHandler` → calls `Order.Cancel()` for each order. Each cancellation raises `OrderCancelledDomainEvent`, which the domain event handler translates into `OrderCancelledIntegrationEvent` in the Outbox.
* `RestoreStockOnExpiredOrdersIntegrationEventHandler` → calls `IStockService.IncreaseStockAsync` for each item, gated on the order being `Cancelled`.

**Local integration event handlers (Ordering's own outbound events):**

Ordering subscribes to some of its own outbound events to perform durable external side effects—email, SMS, and push notifications—that must run outside the checkout transaction and receive Outbox retry semantics.

* `SendOrderConfirmationEmailHandler` — reacts to `OrderPlacedIntegrationEvent` and sends the confirmation email via `IEmailChannel`. It runs through the Outbox so the send can be retried and isolated from the checkout transaction.

This is deliberate self-consumption. The distinction from §7.3 is transactional scope: reactions that must commit with the order (such as the in-app notification row) use domain event handlers; durable external side effects use integration event handlers, even when the event originates from the same context. The Outbox provides retry and external isolation, not cross-context delivery in this case.

**Additional handlers:**

* `ScheduleFeedbackRequestOnOrderDeliveredHandler` — handles `ShipmentDeliveredIntegrationEvent` and schedules a feedback job.

### 7.5 Jobs

| Job                      | Trigger                  | Description                                                                                                                                                                    |
| ------------------------ | ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `ExpirePendingOrdersJob` | Recurring (configurable) | Detects pending orders older than the threshold and publishes a single `OrdersExpiredIntegrationEvent` containing a batch of IDs. No state mutation occurs in the job handler. |
| `SendFeedbackRequestJob` | Scheduled one-shot       | Sends a feedback email seven days after delivery.                                                                                                                              |

**Expiration transaction boundary:** The job handler performs exactly one save—the `OrdersExpiredIntegrationEvent` Outbox record. Cancellation and stock restoration are performed by two independent integration event handlers, each processing orders per-order in its own transaction. Each mutation path is independently retryable via the Outbox dispatcher.

---

## 8. Infrastructure Layer

### 8.1 Persistence

* `AppDbContext.Ordering.cs` — `DbSet<Cart>`, `DbSet<CartItem>`, `DbSet<Order>`, and `DbSet<OrderItem>`.
* Configurations: `CartConfiguration`, `CartItemConfiguration`, `OrderConfiguration`, and `OrderItemConfiguration`. All inherit `BaseEntityConfiguration<T>` and define table names, keys, relationships, owned `Money` value objects, and the `RowVersion` shadow property.

### 8.2 Repositories

* `CartRepository` — implements `ICartRepository` and includes items when loading a cart.
* `OrderRepository` — implements `IOrderRepository` and includes `GetPendingOrderIdsOlderThanAsync` for the cleanup job.

Repositories are auto-discovered by the shared `AddRepositories` Scrutor scan on the Infrastructure assembly. No manual registration is required.

### 8.3 Services

* `PendingOrderCleanupService` — implements `IPendingOrderCleanupService`. It is query-only: it returns the IDs of `PendingPayment` orders older than the threshold. It does not mutate state or save changes. Cancellation and stock restoration are handled by dedicated integration event handlers.

### 8.4 DI Registration

`OrderingInfrastructureExtensions.AddOrderingInfrastructure()` registers:

* `IPendingOrderCleanupService` → `PendingOrderCleanupService`.

Repositories are registered automatically by the shared `RepositoryRegistrationExtensions` scan. The expiration integration event handlers and local integration event handlers (including `SendOrderConfirmationEmailHandler`) are discovered automatically by the `ApplicationServiceRegistration` scan for `IIntegrationEventHandler<T>` implementations.

---

## 9. API Layer

Controllers are thin, use role-based authorization, and delegate to MediatR commands and queries.

### 9.1 Controllers

| Controller         | Route         | Role(s)                          |
| ------------------ | ------------- | -------------------------------- |
| `CartsController`  | `/api/carts`  | Customer                         |
| `OrdersController` | `/api/orders` | Customer, Administrator, Support |

### 9.2 Endpoints

**Carts:**

```http
POST   /api/carts
GET    /api/carts/me
POST   /api/carts/items
PUT    /api/carts/items/{productVariantId}
DELETE /api/carts/items/{productVariantId}
DELETE /api/carts
```

**Orders:**

```http
POST /api/orders/place
GET  /api/orders/{id}
GET  /api/orders/customer
GET  /api/orders
POST /api/orders/{id}/cancel
```

Permissions are enforced at the Application layer through commands and queries; controllers only enforce roles.

**Note on payment initiation:** Ordering does not expose a payment endpoint. After `POST /api/orders/place` returns an `OrderId`, the client calls `POST /api/payments/initiate` on the Finance API (see §6.3).

---

## 10. Authorization

### 10.1 Permissions

```csharp
public static class OrderingPermissions
{
    public const string Read = "Orders.Read";
    public const string Place = "Orders.Place";
    public const string Cancel = "Orders.Cancel";
    public const string Manage = "Orders.Manage";
}
```

### 10.2 Enforcement

* **API layer:** Role-based authorization.
* **Application layer:** `[AuthorizePermission]` attributes on commands and queries.
* **Handlers:** Resource ownership checks (e.g., customers can access only their own carts/orders unless they have `Manage` permission).

---

## 11. Workflows

### 11.1 Checkout / Place Order

```text
Customer has Cart
     ↓
PlaceOrderCommand
     ↓
Load cart (includes items)
     ↓
Fetch Person data required by IShippingFeeCalculator
     ↓
Calculate ShippingFee via IShippingFeeCalculator (outside transaction)
     ↓
Create Order with item snapshots (from cart) and shipping fee
     ↓
Decrease Stock via IStockService (same transaction)
     ↓
Clear Cart
     ↓
Create Outbox record for OrderPlacedIntegrationEvent
(via OrderPlacedDomainEventHandler)
     ↓
Save Order + stock + cart + Outbox in one UnitOfWork
     ↓
Commit
```

At the end of this workflow, the Order is `PendingPayment` and no payment has been initiated. Payment initiation is the client's next step (see §11.2).

### 11.2 Payment Initiation (Client → Finance)

```text
Client
     ↓
POST /api/payments/initiate
(OrderId, Amount, Currency, Method, ReturnUrl, CancelUrl)
     ↓
Finance creates Payment (Pending)
     ↓
Finance calls Paymob Intention API
     ↓
Payment (AwaitingPayment); checkout URL returned
     ↓
Client redirects customer to Paymob
```

Ordering is not involved in this workflow. The Order remains `PendingPayment` until Finance publishes an outcome event.

### 11.3 Payment Completed

```text
Finance publishes PaymentCompletedIntegrationEvent
     ↓
Ordering handler → Order.MarkPaid()
     ↓
OrderPaidDomainEventHandler fetches current delivery address from Person,
creates OrderReadyForShippingIntegrationEvent, writes to Outbox
     ↓
Commit
```

### 11.4 Payment Failed

```text
Finance publishes PaymentFailedIntegrationEvent
     ↓
Ordering handler → Order.MarkPaymentFailed()
     ↓
No cancellation, no stock restoration automatically
```

### 11.5 Shipment Shipped

```text
Shipping publishes ShipmentShippedIntegrationEvent
     ↓
Ordering handler → Order.MarkShipped()
```

`ShipmentShippedIntegrationEvent` is raised when the shipment reaches `OutForDelivery`. This is an intentional business mapping between Shipping's lifecycle and Ordering's lifecycle; Ordering's `Shipped` state represents that the shipment has entered the delivery process.

### 11.6 Shipment Delivered

```text
Shipping publishes ShipmentDeliveredIntegrationEvent
     ↓
Ordering handler → Order.MarkDelivered()
     ↓
ScheduleFeedbackRequestOnOrderDeliveredHandler → schedules feedback job
```

### 11.7 Refund Completed

```text
Finance publishes RefundCompletedIntegrationEvent
     ↓
Ordering handler → Order.MarkRefunded()
```

### 11.8 Cancellation (User-Initiated)

```text
CancelOrderCommand
     ↓
Load order, verify status
     ↓
Resource authorization (owner or Manage)
     ↓
Order.Cancel()
     ↓
Restore stock via IStockService.IncreaseStockAsync
     ↓
Create OrderCancelledIntegrationEvent via OrderCancelledDomainEventHandler
     ↓
Save Order + stock + Outbox in one UnitOfWork
```

### 11.9 Cancellation (System Expiry)

Expiration is split into detection (in the job handler) and mutation (in two dedicated integration event handlers).

```text
Hangfire trigger → ExpirePendingOrdersJob
     ↓
ExpirePendingOrdersJobHandler
     ├── Query IPendingOrderCleanupService.GetExpiredPendingOrderIdsAsync
     │      └── Returns IDs of PendingPayment orders older than threshold
     ├── If empty, log and return
     ├── Build OrdersExpiredIntegrationEvent(expiredOrderIds)
     ├── IOutboxMessageWriter.WriteAsync(event)
     └── UnitOfWork.SaveChangesAsync (single save — outbox row)
     ↓
Outbox dispatcher delivers OrdersExpiredIntegrationEvent
     ↓
Two independent handlers react:
```

**`CancelExpiredOrdersIntegrationEventHandler`**

For each order ID:

1. Load the order; skip if already `Cancelled` (idempotent).
2. Skip if not `PendingPayment` (the order has advanced past expiration).
3. Call `Order.Cancel()`, raising `OrderCancelledDomainEvent`.
4. The domain event handler writes `OrderCancelledIntegrationEvent` to the Outbox.
5. Save changes in a per-order transaction.

**`RestoreStockOnExpiredOrdersIntegrationEventHandler`**

For each order ID:

1. Load the order.
2. If status is not `Cancelled`, throw so the Outbox retries.
3. For each item, call `IStockService.IncreaseStockAsync`.
4. Save changes in a per-order transaction.

**Convergence:** The two handlers converge regardless of execution order. If the stock handler runs first, it throws because the order is not yet `Cancelled`; the Outbox marks the message `Failed` and retries after cancellation has committed.

**Concurrency with user-initiated cancellation:** `RowVersion` on `Order` protects against double cancellation. The losing path throws `ConcurrencyException`, rolls back, and the Outbox message is retried. On retry, the order is already `Cancelled` and the handler skips it.

**Expiration as a safety net for unpaid orders:** Orders created by checkout but never paid—because the client never called `POST /api/payments/initiate` or the customer abandoned payment—remain `PendingPayment` and are eventually cancelled by this flow.

---

## 12. Transaction Boundaries and Consistency

* **Checkout:** Order persistence, stock decrease, cart clearing, and the `OrderPlacedIntegrationEvent` Outbox record are committed in one database transaction. Shipping fee calculation occurs outside the transaction. Uses manual transaction mode (`BeginTransactionAsync` / `CommitTransactionAsync`).
* **Payment/fulfilment updates:** Each integration event is processed in its own transaction, updating Order state and any resulting Outbox records atomically.
* **Cancellation (user-initiated):** Order cancellation, stock restoration, and the `OrderCancelledIntegrationEvent` Outbox record are committed in one transaction.
* **Expiry detection (job):** One transaction writes the `OrdersExpiredIntegrationEvent` Outbox record. No order state mutation occurs.
* **Expiry cancellation:** Per-order transaction for `Order.Cancel()` and the resulting `OrderCancelledIntegrationEvent` Outbox record.
* **Expiry stock restoration:** Per-order transaction for stock increases only.
* **Cross-context transaction participation with Catalog** is a deliberate modular-monolith design.

---

## 13. Concurrency and Idempotency

* `RowVersion` optimistic concurrency on `Cart` and `Order`.
* The idempotency decorator prevents duplicate processing of the same integration event.
* Aggregate invariants reject invalid state transitions. These are separate mechanisms.
* Out-of-order or otherwise invalid integration events are rejected by the aggregate and handled according to the integration-event failure/retry policy. They must not be silently swallowed.
* A unique active cart per customer is enforced by the repository/application check. A database unique constraint can be added if stronger concurrency enforcement is required.
* **Expiration handler ordering:** The two `OrdersExpiredIntegrationEvent` handlers are idempotent under duplicate delivery and convergent under any execution order. If the stock handler runs before cancellation commits, it throws and is retried.

---

## 14. Testing

### 14.1 Domain Tests

* Cart/order invariants and state transitions.
* Cancellation rules.
* Refundability rules.
* Expiration eligibility (only `PendingPayment`).

### 14.2 Application Tests

* Idempotent integration event handlers.
* Checkout orchestration with mocked services.
* Cancellation stock restoration.
* Expiration split handlers: convergence regardless of execution order and idempotency under retry.
* Concurrent cancellation via `RowVersion`.

### 14.3 Integration Tests

* EF mappings, unique constraints, and `RowVersion`.
* Outbox persistence.
* Idempotent event consumption.
* End-to-end expiration flow with both handlers.

### 14.4 API Tests

* Role-based authorization.
* Customer isolation.
* Cart/order CRUD.
* Absence of a payment-initiation endpoint on Ordering (the client orchestrates payment directly with Finance).

---

## 15. Summary

The Ordering bounded context provides a complete cart-to-order lifecycle with clear integration points to Catalog, Shipping, and Finance. It uses shared contracts for synchronous operations, Outbox events for asynchronous communication, and a layered authorization model. The domain remains pure, and the infrastructure is isolated.

**Key design properties:**

* The Order does not store the delivery address; Shipping captures the address as its own immutable snapshot at shipment creation time.
* **Payment initiation is client-orchestrated.** The client calls `POST /api/payments/initiate` on Finance after placing the order. Ordering does not initiate payment, and Finance does not consume any Ordering event. `OrderPlacedIntegrationEvent` is Ordering-internal and consumed by `SendOrderConfirmationEmailHandler`.
* If the client never initiates payment, the Order remains `PendingPayment` and is eventually cancelled by the expiration job.
* `PaymentFailed` orders are not processed by the expiration job; only `PendingPayment` orders are eligible.
* Expired `PendingPayment` orders are cancelled through the same `Order.Cancel()` path used by user-initiated cancellation, restoring stock.
* The expiration flow is split: the recurring job detects and publishes; cancellation and stock restoration are performed by two independent integration event handlers, each per-order transactional and independently retryable.
* Intra-context reactions that must be transactional use **domain event handlers** (`PersistUserNotificationOnOrderPlacedHandler` commits the in-app notification row with the order). Intra-context durable external side effects—email, SMS, and push—use **integration event handlers** routed through the Outbox (`SendOrderConfirmationEmailHandler`), so sending is retried and isolated from the checkout transaction. The Outbox is used here for retry and external isolation, not for cross-context delivery.

This design is production-ready and maintainable.

---

**Document version:** 2.4 – Final
**Feature:** Ordering / Cart and Order Lifecycle with Checkout Integration
