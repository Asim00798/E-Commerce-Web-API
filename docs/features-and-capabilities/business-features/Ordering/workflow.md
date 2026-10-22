# Ordering Feature — Workflow Technical Document

**Project:** E-Commerce Modular Monolith

**Scope:** Cart, Checkout, Payment Integration, Shipment Updates, Refunds, Cancellation

**Version:** 2.2 – Final

---

## 1. Overview

This document defines the complete end-to-end workflows for the Ordering bounded context. It covers:

* Cart creation and item management.
* Checkout and order placement with synchronous stock and shipping fee integration.
* Payment initiation via Finance.
* Payment success/failure handling.
* Shipment status updates from Shipping.
* Refund completion handling.
* Order cancellation (user-initiated and system-expiry).
* Background jobs for pending order expiry and feedback requests.
* Domain events and outbound integration events.

All cross-context communication uses the existing Outbox mechanism. Synchronous operations (stock decrease and shipping fee calculation) are used only when immediate consistency is required.

---

## 2. Actors and Components

### 2.1 Actors

| Actor            | Description                                                                  |
| ---------------- | ---------------------------------------------------------------------------- |
| Customer         | Browses products, manages cart, places orders, and views their own orders.   |
| Admin/Support    | Can manage all orders and cancel orders on behalf of customers.              |
| Finance Context  | Publishes payment/refund events; consumes `OrderPlacedIntegrationEvent`.     |
| Shipping Context | Publishes shipment events; consumes `OrderReadyForShippingIntegrationEvent`. |
| Catalog Context  | Provides stock through `IStockService` synchronously.                        |
| People Context   | Provides customer contact information and delivery address.                  |
| Outbox Processor | Delivers integration events asynchronously.                                  |
| Hangfire         | Executes background jobs (`ExpirePendingOrders`, `SendFeedbackRequest`).     |

### 2.2 Key Components

| Component                                            | Layer                       | Responsibility                                                                                            |
| ---------------------------------------------------- | --------------------------- | --------------------------------------------------------------------------------------------------------- |
| Cart aggregate                                       | Domain                      | Cart lifecycle, item management, and invariants.                                                          |
| Order aggregate                                      | Domain                      | Order lifecycle, status transitions, totals, and cancellation.                                            |
| `IStockService`                                      | Shared Application contract | Synchronous stock decrease/increase.                                                                      |
| `IShippingFeeCalculator`                             | Shared Application contract | Synchronous shipping fee calculation.                                                                     |
| `IPersonRepository`                                  | Application                 | Fetches customer contact information and address.                                                         |
| `IUnitOfWork`                                        | SharedKernel                | Transaction boundary and domain event dispatch.                                                           |
| `IOutboxMessageWriter`                               | Application                 | Persists outbound integration events.                                                                     |
| Domain event handlers                                | Application                 | Translate domain events into outbound integration events.                                                 |
| Integration event handlers                           | Application                 | React to inbound integration events and update local aggregates.                                          |
| `PendingOrderCleanupService`                         | Infrastructure              | Query-only service that returns IDs of pending orders older than the threshold. It does not mutate state. |
| `CancelExpiredOrdersIntegrationEventHandler`         | Application                 | Reacts to `OrdersExpiredIntegrationEvent` and cancels each eligible order.                                |
| `RestoreStockOnExpiredOrdersIntegrationEventHandler` | Application                 | Reacts to `OrdersExpiredIntegrationEvent` and restores stock for each cancelled order.                    |
| `SendFeedbackRequestJobHandler`                      | Application                 | Sends a feedback email after delivery.                                                                    |

---

## 3. Workflow 1: Cart Management

### 3.1 Create Cart

**Trigger:** The customer wants to start a shopping session.

```text
Customer
  ↓ POST /api/carts
CartsController (role: Customer)
  ↓ CreateCartCommand
CreateCartCommandHandler
  ├── Check current user ID
  ├── Check existing cart via ICartRepository.GetByCustomerIdAsync
  ├── If none, Cart.Create(customerId)
  ├── Add to repository
  ├── UnitOfWork.SaveChangesAsync
  └── Return cart ID
```

No domain event is raised. No integration event is published.

### 3.2 Add Item to Cart

```text
Customer
  ↓ POST /api/carts/items
CartsController (role: Customer)
  ↓ AddCartItemCommand
AddCartItemCommandHandler
  ├── Load cart via GetByCustomerIdAsync
  ├── Enforce MaximumItemsPerCart policy (from OrderingOptions)
  ├── cart.AddItem(...)
  ├── Update repository
  └── UnitOfWork.SaveChangesAsync
```

If the cart is not found, the handler returns a failure. The domain ensures duplicate product variants are merged by quantity.

### 3.3 Update Quantity / Remove Item / Clear Cart

Similar flows use `UpdateCartItemQuantityCommand`, `RemoveCartItemCommand`, and `ClearCartCommand`.

All operations enforce cart aggregate rules and persist changes through `IUnitOfWork`.

---

## 4. Workflow 2: Checkout / Place Order

This is the primary flow integrating Catalog and Shipping directly, with Finance involved later through the client-orchestrated payment workflow.

```text
Customer
  ↓ POST /api/orders/place
OrdersController (role: Customer)
  ↓ PlaceOrderCommand
PlaceOrderCommandHandler
  ├── Load cart (includes items)
  ├── Fetch Person data required by IShippingFeeCalculator
  ├── Build ShippingFeeCalculationRequest
  ├── Call IShippingFeeCalculator.CalculateAsync
  │   (synchronous, outside transaction)
  ├── Map cart items to OrderItem domain objects
  ├── For each item:
  │   ├── Call IStockService.DecreaseStockAsync(productId, variantId, quantity)
  │   └── If failure, return error and abort (no order created)
  ├── Call Order.Place(customerId, items, shippingFee)
  │   ├── Sets Status = PendingPayment
  │   ├── Calculates Subtotal and Total
  │   ├── Assigns OrderId to each item
  │   └── Raises OrderPlacedDomainEvent
  ├── Add order to IOrderRepository
  ├── cart.Clear()
  ├── Update cart in repository
  ├── UnitOfWork.SaveChangesAsync
  │   ├── Saves order, cart, and stock changes
  │   ├── Dispatches OrderPlacedDomainEvent
  │   │   └── OrderPlacedDomainEventHandler
  │   │       ├── Fetch Person data if Finance requires email/name
  │   │       ├── Create OrderPlacedIntegrationEvent
  │   │       └── Write to Outbox (inside same transaction)
  │   ├── Clears domain events
  │   └── Commits transaction
  └── Return order ID
```

**Important:** Shipping fee calculation occurs before the transaction. Stock decrease, order creation, cart clearing, and the Outbox record are atomic. If any step fails, the entire transaction rolls back.

**Outbox record:** `OrderPlacedIntegrationEvent` is written by `OrderPlacedDomainEventHandler`, not by the command handler.

**Transaction mode:** Checkout uses manual transaction mode (`BeginTransactionAsync` / `CommitTransactionAsync`) because Phase 1 performs cross-context synchronous reads that must run outside the transaction.

---

## 5. Workflow 3: Payment Initiation

Ordering does not call Finance directly. After the order is committed, the client initiates payment through Finance.

```text
Client places order
  ↓
POST /api/orders/place
  ↓
OrderId returned
  ↓
Client calls Finance API
  ↓
POST /api/payments/initiate
  ↓
Finance creates Payment (Pending)
  ↓
Finance calls Paymob to create an Intention
  ↓
Payment → AwaitingPayment
  ↓
Checkout URL becomes available through Finance API
  ↓
Client queries Finance by OrderId to obtain payment URL
  ↓
Client redirects customer to Paymob
```

No action from Ordering is required during payment initiation.

---

## 6. Workflow 4: Payment Success

After the customer completes payment through Paymob, Finance receives and verifies the webhook, then publishes `PaymentCompletedIntegrationEvent`.

```text
Finance publishes PaymentCompletedIntegrationEvent
  ↓
Outbox
  ↓
Ordering: PaymentCompletedIntegrationEventHandler
  ├── Load Order by OrderId
  ├── order.MarkPaid()
  │   ├── Status: PendingPayment → Paid
  │   └── Raises OrderPaidDomainEvent
  ├── Update order in repository
  ├── UnitOfWork.SaveChangesAsync
  │   ├── Saves order
  │   ├── Dispatches OrderPaidDomainEvent
  │   │   └── OrderPaidDomainEventHandler
  │   │       ├── Fetch current delivery address from Person
  │   │       ├── Create OrderReadyForShippingIntegrationEvent
  │   │       └── Write to Outbox (same transaction)
  │   └── Commits
  └── Outbox later publishes OrderReadyForShippingIntegrationEvent
```

Shipping consumes `OrderReadyForShippingIntegrationEvent` and creates a shipment. Ordering does not call the Shipping API directly.

---

## 7. Workflow 5: Payment Failure

Finance publishes `PaymentFailedIntegrationEvent`.

```text
Finance publishes PaymentFailedIntegrationEvent
  ↓
Outbox
  ↓
Ordering: PaymentFailedIntegrationEventHandler
  ├── Load Order by OrderId
  ├── order.MarkPaymentFailed()
  │   ├── Status: PendingPayment → PaymentFailed
  │   └── No domain event
  ├── Update order
  └── Save
```

* The order remains in `PaymentFailed`.
* Stock is not restored automatically.
* No cancellation occurs.
* The `ExpirePendingOrders` job does **not** process `PaymentFailed` orders. Only orders still in `PendingPayment` are eligible for expiration cancellation.
* A `PaymentFailed` order is a distinct state that requires user action (`CancelOrderCommand`) to transition to `Cancelled`.

---

## 8. Workflow 6: Shipment Shipped

Shipping publishes `ShipmentShippedIntegrationEvent` when the shipment reaches `OutForDelivery`.

```text
Shipping publishes ShipmentShippedIntegrationEvent
  ↓ Outbox
Ordering: ShipmentShippedIntegrationEventHandler
  ├── Load Order
  ├── order.MarkShipped()
  │   └── Status: Paid → Shipped
  ├── Update order
  └── Save
```

No domain event is raised, and Ordering publishes no Outbox event for this transition.

---

## 9. Workflow 7: Shipment Delivered

Shipping publishes `ShipmentDeliveredIntegrationEvent` when the shipment is delivered.

```text
Shipping publishes ShipmentDeliveredIntegrationEvent
  ↓ Outbox
Ordering: ShipmentDeliveredIntegrationEventHandler
  ├── Load Order
  ├── order.MarkDelivered()
  │   ├── Status: Shipped → Delivered
  │   └── Sets DeliveredAtUtc
  ├── Update order
  └── Save
```

A separate handler, `ScheduleFeedbackRequestOnOrderDeliveredHandler`, also handles the same `ShipmentDeliveredIntegrationEvent`:

```text
ScheduleFeedbackRequestOnOrderDeliveredHandler
  ├── Receives ShipmentDeliveredIntegrationEvent
  ├── Uses IJobScheduler.Schedule to enqueue SendFeedbackRequestJob
  │   after 7 days
  └── The job later fetches the order and Person, then sends the email
```

No Outbox event is published by Ordering for delivery. The feedback job is durable once successfully enqueued, but enqueueing is not atomic with the Ordering transaction. Therefore, a failure between transaction commit and job scheduling can result in the feedback job not being created. This is accepted for this non-critical workflow.

---

## 10. Workflow 8: Refund Completed

Finance publishes `RefundCompletedIntegrationEvent`.

```text
Finance publishes RefundCompletedIntegrationEvent
  ↓ Outbox
Ordering: RefundCompletedIntegrationEventHandler
  ├── Load Order by OrderId (included in event)
  ├── order.MarkRefunded()
  │   ├── Status: Paid/Delivered/Returned → Refunded
  │   └── Sets RefundedAtUtc
  ├── Update order
  └── Save
```

No domain event or Outbox event is published by Ordering.

---

## 11. Workflow 9: Order Cancellation

Cancellation has two origins: user-initiated and system-expiry. Both converge on the same aggregate method (`Order.Cancel()`), the same domain event (`OrderCancelledDomainEvent`), and the same outbound integration event (`OrderCancelledIntegrationEvent`).

### 11.1 User-Initiated Cancellation

```text
Customer or Admin/Support
  ↓ POST /api/orders/{id}/cancel
OrdersController (role: Customer, Administrator, Support)
  ↓ CancelOrderCommand
CancelOrderCommandHandler
  ├── Load Order
  ├── Resource authorization (owner or Manage permission)
  ├── order.Cancel()
  │   ├── Status: PendingPayment or PaymentFailed → Cancelled
  │   └── Raises OrderCancelledDomainEvent
  ├── Update order
  ├── If business policy enabled:
  │   └── For each item, call IStockService.IncreaseStockAsync
  ├── UnitOfWork.SaveChangesAsync
  │   ├── Saves order and stock changes
  │   ├── Dispatches OrderCancelledDomainEvent
  │   │   └── OrderCancelledDomainEventHandler
  │   │       ├── Create OrderCancelledIntegrationEvent
  │   │       └── Write to Outbox
  │   └── Commits
  └── Outbox publishes OrderCancelledIntegrationEvent
```

### 11.2 System Expiry Cancellation

The `ExpirePendingOrders` recurring job runs through Hangfire. The flow is split across the job handler (detection and publication) and two dedicated integration event handlers (cancellation and stock restoration).

#### Phase 1: Detect Expired Orders and Publish the Event

```text
Hangfire trigger (every N hours)
  ↓
ExpirePendingOrdersJob
  ↓
ExpirePendingOrdersJobHandler
  ├── Calculate expiration threshold from
  │   OrderingOptions.PendingOrderExpirationHours
  ├── Call IPendingOrderCleanupService.GetExpiredPendingOrderIdsAsync
  │   └── Query-only: returns IDs of PendingPayment orders
  │       older than the threshold. No mutation, no save.
  ├── If no IDs returned, log and return
  ├── Build a single OrdersExpiredIntegrationEvent
  │   carrying the batch of expired order IDs
  ├── IOutboxMessageWriter.WriteAsync(event) (Channel 5, direct write)
  └── UnitOfWork.SaveChangesAsync
      (atomic: Outbox row committed)
```

#### Phase 2: Cancel Expired Orders

The Outbox dispatcher delivers `OrdersExpiredIntegrationEvent` to `CancelExpiredOrdersIntegrationEventHandler`.

For each order ID:

1. Load the order.
2. If already `Cancelled`, skip it (idempotent).
3. If not `PendingPayment`, skip it because the order has advanced past expiration.
4. Call `order.Cancel()`, transitioning `PendingPayment` → `Cancelled` and raising `OrderCancelledDomainEvent`.
5. Save changes. The resulting `OrderCancelledIntegrationEvent` is written to the Outbox atomically.
6. Process each order in its own transaction.

#### Phase 3: Restore Stock

The Outbox dispatcher also delivers `OrdersExpiredIntegrationEvent` to `RestoreStockOnExpiredOrdersIntegrationEventHandler`.

For each order ID:

1. Load the order.
2. If its status is not `Cancelled`, throw because cancellation may not yet have committed. The Outbox will retry.
3. For each item, call `IStockService.IncreaseStockAsync`.
4. Save changes in a per-order transaction.

Each handler is independently retryable.

No separate `OrderCancelledIntegrationEvent` is emitted outside the normal cancellation path. The event is emitted once per cancelled order through `OrderCancelledDomainEvent`, just as in user-initiated cancellation.

**Stock restoration:** Performed by `RestoreStockOnExpiredOrdersIntegrationEventHandler`. Unlike user-initiated cancellation, the expiry path does not gate stock restoration behind a business-policy flag in the current implementation.

**Concurrency with user-initiated cancellation:** Both paths can attempt to cancel the same order simultaneously. `RowVersion` on `Order` protects against double cancellation. The losing transaction throws `ConcurrencyException`, rolls back, and the Outbox message is marked `Failed` for retry. On retry, the order is already `Cancelled`, and the handler skips it.

**Transaction model:** `ExpirePendingOrdersJobHandler` performs a single save for the Outbox row, using auto mode without order-state mutation. Each subsequent order operation runs in its own transaction inside the respective integration event handler.

---

## 12. Correlation ID Propagation

The original correlation ID from the HTTP request flows through:

1. `IAppContext.CorrelationId` is set by middleware.
2. Domain event handlers set it on created integration events.
3. The `CorrelationScopeIntegrationEventHandler` decorator restores it in the logging scope.
4. `OrderPaidDomainEventHandler` uses the same `IAppContext` to set `CorrelationId` on `OrderReadyForShippingIntegrationEvent`.

This ensures trace continuity across asynchronous boundaries.

**Note on the expiry flow:** `OrdersExpiredIntegrationEvent` originates from a Hangfire job, not an HTTP request. `IAppContext.CorrelationId` resolves from `IHttpContextAccessor`, which is null on a job thread, so the correlation ID on `OrdersExpiredIntegrationEvent` is null. This is deliberate: the job has no originating user action. If correlation to the job scheduling event is required in the future, `IJobContext.CorrelationId` (per the scheduling abstraction) is the correct source.

---

## 13. Transaction Boundaries Summary

| Phase                    | Transaction Type                 | Participants                                                           |
| ------------------------ | -------------------------------- | ---------------------------------------------------------------------- |
| Checkout                 | Single transaction (manual mode) | Order, stock via `IStockService`, cart clearing, Outbox record.        |
| Payment success          | Single transaction               | Order state and `OrderReadyForShippingIntegrationEvent` Outbox record. |
| Payment failure          | Single transaction               | Order state only.                                                      |
| Shipment status updates  | One transaction per update       | Order state only.                                                      |
| Refund completion        | Single transaction               | Order state only.                                                      |
| Cancellation (user)      | Single transaction               | Order state, stock restoration (optional), Outbox record.              |
| Expiry detection (job)   | Single transaction               | Outbox record only; no state mutation.                                 |
| Expiry cancellation      | Per-order transaction            | Order state and `OrderCancelledIntegrationEvent` Outbox record.        |
| Expiry stock restoration | Per-order transaction            | Stock restoration only.                                                |

**Transaction mode:** Checkout uses manual mode (`BeginTransactionAsync` / `CommitTransactionAsync`). All other transactions in the table use `IUnitOfWork` auto mode, where `SaveChangesAsync` opens its own transaction, dispatches domain events, and commits. Both modes are managed by `IUnitOfWork`; the distinction is whether the caller opens the transaction explicitly.

---

## 14. Idempotency

* All inbound integration event handlers are wrapped with the `IdempotentIntegrationEventHandler<T>` decorator.
* Outbox delivery guarantees at-least-once delivery; idempotency prevents duplicate side effects.
* Duplicate events are safely ignored by the idempotency layer.
* Invalid or out-of-order events are rejected by the aggregate and should be handled according to the integration-event failure/retry policy, not silently swallowed.
* **Ordering between split handlers:** The two `OrdersExpiredIntegrationEvent` handlers converge regardless of execution order. If `RestoreStockOnExpiredOrdersIntegrationEventHandler` runs first, it throws because the order is not yet `Cancelled`; the Outbox marks the message `Failed` and retries. On retry, `CancelExpiredOrdersIntegrationEventHandler` has committed, and the stock handler completes. The worst case is one extra retry cycle.

---

## 15. Conclusion

The Ordering workflows are fully specified and consistent with the modular monolith architecture. They use domain events for internal reactions, integration events for cross-context communication, and synchronous contracts only when immediate consistency is required. The combination of UnitOfWork transactions, Outbox, and idempotency supports reliability and maintainability.

The expiry flow is deliberately split: the job detects expired orders, publishes a single bulk event, and performs no state mutation itself. Cancellation and stock restoration are performed by two independent integration event handlers, each per-order transactional, independently retryable, and idempotent under duplicate delivery. This preserves the job's single responsibility and keeps the mutation paths observable and testable in isolation.

---

**Document version:** 2.2 – Final

**Feature:** Ordering Workflows
