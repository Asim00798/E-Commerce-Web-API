\# Ordering Feature — Workflow Technical Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Scope:\*\* Cart, Checkout, Payment Integration, Shipment Updates, Refunds, Cancellation

\*\*Version:\*\* 2.2 – Final



\---



\## 1. Overview



This document defines the complete end-to-end workflows for the Ordering bounded context. It covers:



\* Cart creation and item management.

\* Checkout and order placement with synchronous stock and shipping fee integration.

\* Payment initiation via Finance.

\* Payment success/failure handling.

\* Shipment status updates from Shipping.

\* Refund completion handling.

\* Order cancellation (user-initiated and system-expiry).

\* Background jobs for pending order expiry and feedback requests.

\* Domain events and outbound integration events.



All cross-context communication uses the existing Outbox mechanism. Synchronous operations (stock decrease and shipping fee calculation) are used only when immediate consistency is required.



\---



\## 2. Actors and Components



\### 2.1 Actors



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



\### 2.2 Key Components



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



\---



\## 3. Workflow 1: Cart Management



\### 3.1 Create Cart



\*\*Trigger:\*\* The customer wants to start a shopping session.



```text

Customer

&#x20;  ↓ POST /api/carts

CartsController (role: Customer)

&#x20;  ↓ CreateCartCommand

CreateCartCommandHandler

&#x20;  ├── Check current user ID

&#x20;  ├── Check existing cart via ICartRepository.GetByCustomerIdAsync

&#x20;  ├── If none, Cart.Create(customerId)

&#x20;  ├── Add to repository

&#x20;  ├── UnitOfWork.SaveChangesAsync

&#x20;  └── Return cart ID

```



No domain event is raised. No integration event is published.



\### 3.2 Add Item to Cart



```text

Customer

&#x20;  ↓ POST /api/carts/items

CartsController (role: Customer)

&#x20;  ↓ AddCartItemCommand

AddCartItemCommandHandler

&#x20;  ├── Load cart via GetByCustomerIdAsync

&#x20;  ├── Enforce MaximumItemsPerCart policy (from OrderingOptions)

&#x20;  ├── cart.AddItem(...)

&#x20;  ├── Update repository

&#x20;  └── UnitOfWork.SaveChangesAsync

```



If the cart is not found, the handler returns a failure. The domain ensures duplicate product variants are merged by quantity.



\### 3.3 Update Quantity / Remove Item / Clear Cart



Similar flows use `UpdateCartItemQuantityCommand`, `RemoveCartItemCommand`, and `ClearCartCommand`.



All operations enforce cart aggregate rules and persist changes through `IUnitOfWork`.



\---



\## 4. Workflow 2: Checkout / Place Order



This is the primary flow integrating Catalog and Shipping directly, with Finance involved later through the client-orchestrated payment workflow.



```text

Customer

&#x20;  ↓ POST /api/orders/place

OrdersController (role: Customer)

&#x20;  ↓ PlaceOrderCommand

PlaceOrderCommandHandler

&#x20;  ├── Load cart (includes items)

&#x20;  ├── Fetch Person data required by IShippingFeeCalculator

&#x20;  ├── Build ShippingFeeCalculationRequest

&#x20;  ├── Call IShippingFeeCalculator.CalculateAsync

&#x20;  │   (synchronous, outside transaction)

&#x20;  ├── Map cart items to OrderItem domain objects

&#x20;  ├── For each item:

&#x20;  │   ├── Call IStockService.DecreaseStockAsync(productId, variantId, quantity)

&#x20;  │   └── If failure, return error and abort (no order created)

&#x20;  ├── Call Order.Place(customerId, items, shippingFee)

&#x20;  │   ├── Sets Status = PendingPayment

&#x20;  │   ├── Calculates Subtotal and Total

&#x20;  │   ├── Assigns OrderId to each item

&#x20;  │   └── Raises OrderPlacedDomainEvent

&#x20;  ├── Add order to IOrderRepository

&#x20;  ├── cart.Clear()

&#x20;  ├── Update cart in repository

&#x20;  ├── UnitOfWork.SaveChangesAsync

&#x20;  │   ├── Saves order, cart, and stock changes

&#x20;  │   ├── Dispatches OrderPlacedDomainEvent

&#x20;  │   │   └── OrderPlacedDomainEventHandler

&#x20;  │   │       ├── Fetch Person data if Finance requires email/name

&#x20;  │   │       ├── Create OrderPlacedIntegrationEvent

&#x20;  │   │       └── Write to Outbox (inside same transaction)

&#x20;  │   ├── Clears domain events

&#x20;  │   └── Commits transaction

&#x20;  └── Return order ID

```



\*\*Important:\*\* Shipping fee calculation occurs before the transaction. Stock decrease, order creation, cart clearing, and the Outbox record are atomic. If any step fails, the entire transaction rolls back.



\*\*Outbox record:\*\* `OrderPlacedIntegrationEvent` is written by `OrderPlacedDomainEventHandler`, not by the command handler.



\*\*Transaction mode:\*\* Checkout uses manual transaction mode (`BeginTransactionAsync` / `CommitTransactionAsync`) because Phase 1 performs cross-context synchronous reads that must run outside the transaction.



\---



\## 5. Workflow 3: Payment Initiation



Ordering does not call Finance directly. After the order is committed, the client initiates payment through Finance.



```text

Client places order

&#x20;  ↓

POST /api/orders/place

&#x20;  ↓

OrderId returned

&#x20;  ↓

Client calls Finance API

&#x20;  ↓

POST /api/payments/initiate

&#x20;  ↓

Finance creates Payment (Pending)

&#x20;  ↓

Finance calls Paymob to create an Intention

&#x20;  ↓

Payment → AwaitingPayment

&#x20;  ↓

Checkout URL becomes available through Finance API

&#x20;  ↓

Client queries Finance by OrderId to obtain payment URL

&#x20;  ↓

Client redirects customer to Paymob

```



No action from Ordering is required during payment initiation.



\---



\## 6. Workflow 4: Payment Success



After the customer completes payment through Paymob, Finance receives and verifies the webhook, then publishes `PaymentCompletedIntegrationEvent`.



```text

Finance publishes PaymentCompletedIntegrationEvent

&#x20;  ↓

Outbox

&#x20;  ↓

Ordering: PaymentCompletedIntegrationEventHandler

&#x20;  ├── Load Order by OrderId

&#x20;  ├── order.MarkPaid()

&#x20;  │   ├── Status: PendingPayment → Paid

&#x20;  │   └── Raises OrderPaidDomainEvent

&#x20;  ├── Update order in repository

&#x20;  ├── UnitOfWork.SaveChangesAsync

&#x20;  │   ├── Saves order

&#x20;  │   ├── Dispatches OrderPaidDomainEvent

&#x20;  │   │   └── OrderPaidDomainEventHandler

&#x20;  │   │       ├── Fetch current delivery address from Person

&#x20;  │   │       ├── Create OrderReadyForShippingIntegrationEvent

&#x20;  │   │       └── Write to Outbox (same transaction)

&#x20;  │   └── Commits

&#x20;  └── Outbox later publishes OrderReadyForShippingIntegrationEvent

```



Shipping consumes `OrderReadyForShippingIntegrationEvent` and creates a shipment. Ordering does not call the Shipping API directly.



\---



\## 7. Workflow 5: Payment Failure



Finance publishes `PaymentFailedIntegrationEvent`.



```text

Finance publishes PaymentFailedIntegrationEvent

&#x20;  ↓

Outbox

&#x20;  ↓

Ordering: PaymentFailedIntegrationEventHandler

&#x20;  ├── Load Order by OrderId

&#x20;  ├── order.MarkPaymentFailed()

&#x20;  │   ├── Status: PendingPayment → PaymentFailed

&#x20;  │   └── No domain event

&#x20;  ├── Update order

&#x20;  └── Save

```



\* The order remains in `PaymentFailed`.

\* Stock is not restored automatically.

\* No cancellation occurs.

\* The `ExpirePendingOrders` job does \*\*not\*\* process `PaymentFailed` orders. Only orders still in `PendingPayment` are eligible for expiration cancellation.

\* A `PaymentFailed` order is a distinct state that requires user action (`CancelOrderCommand`) to transition to `Cancelled`.



\---



\## 8. Workflow 6: Shipment Shipped



Shipping publishes `ShipmentShippedIntegrationEvent` when the shipment reaches `OutForDelivery`.



```text

Shipping publishes ShipmentShippedIntegrationEvent

&#x20;  ↓ Outbox

Ordering: ShipmentShippedIntegrationEventHandler

&#x20;  ├── Load Order

&#x20;  ├── order.MarkShipped()

&#x20;  │   └── Status: Paid → Shipped

&#x20;  ├── Update order

&#x20;  └── Save

```



No domain event is raised, and Ordering publishes no Outbox event for this transition.



\---



\## 9. Workflow 7: Shipment Delivered



Shipping publishes `ShipmentDeliveredIntegrationEvent` when the shipment is delivered.



```text

Shipping publishes ShipmentDeliveredIntegrationEvent

&#x20;  ↓ Outbox

Ordering: ShipmentDeliveredIntegrationEventHandler

&#x20;  ├── Load Order

&#x20;  ├── order.MarkDelivered()

&#x20;  │   ├── Status: Shipped → Delivered

&#x20;  │   └── Sets DeliveredAtUtc

&#x20;  ├── Update order

&#x20;  └── Save

```



A separate handler, `ScheduleFeedbackRequestOnOrderDeliveredHandler`, also handles the same `ShipmentDeliveredIntegrationEvent`:



```text

ScheduleFeedbackRequestOnOrderDeliveredHandler

&#x20;  ├── Receives ShipmentDeliveredIntegrationEvent

&#x20;  ├── Uses IJobScheduler.Schedule to enqueue SendFeedbackRequestJob

&#x20;  │   after 7 days

&#x20;  └── The job later fetches the order and Person, then sends the email

```



No Outbox event is published by Ordering for delivery. The feedback job is durable once successfully enqueued, but enqueueing is not atomic with the Ordering transaction. Therefore, a failure between transaction commit and job scheduling can result in the feedback job not being created. This is accepted for this non-critical workflow.



\---



\## 10. Workflow 8: Refund Completed



Finance publishes `RefundCompletedIntegrationEvent`.



```text

Finance publishes RefundCompletedIntegrationEvent

&#x20;  ↓ Outbox

Ordering: RefundCompletedIntegrationEventHandler

&#x20;  ├── Load Order by OrderId (included in event)

&#x20;  ├── order.MarkRefunded()

&#x20;  │   ├── Status: Paid/Delivered/Returned → Refunded

&#x20;  │   └── Sets RefundedAtUtc

&#x20;  ├── Update order

&#x20;  └── Save

```



No domain event or Outbox event is published by Ordering.



\---



\## 11. Workflow 9: Order Cancellation



Cancellation has two origins: user-initiated and system-expiry. Both converge on the same aggregate method (`Order.Cancel()`), the same domain event (`OrderCancelledDomainEvent`), and the same outbound integration event (`OrderCancelledIntegrationEvent`).



\### 11.1 User-Initiated Cancellation



```text

Customer or Admin/Support

&#x20;  ↓ POST /api/orders/{id}/cancel

OrdersController (role: Customer, Administrator, Support)

&#x20;  ↓ CancelOrderCommand

CancelOrderCommandHandler

&#x20;  ├── Load Order

&#x20;  ├── Resource authorization (owner or Manage permission)

&#x20;  ├── order.Cancel()

&#x20;  │   ├── Status: PendingPayment or PaymentFailed → Cancelled

&#x20;  │   └── Raises OrderCancelledDomainEvent

&#x20;  ├── Update order

&#x20;  ├── If business policy enabled:

&#x20;  │   └── For each item, call IStockService.IncreaseStockAsync

&#x20;  ├── UnitOfWork.SaveChangesAsync

&#x20;  │   ├── Saves order and stock changes

&#x20;  │   ├── Dispatches OrderCancelledDomainEvent

&#x20;  │   │   └── OrderCancelledDomainEventHandler

&#x20;  │   │       ├── Create OrderCancelledIntegrationEvent

&#x20;  │   │       └── Write to Outbox

&#x20;  │   └── Commits

&#x20;  └── Outbox publishes OrderCancelledIntegrationEvent

```



\### 11.2 System Expiry Cancellation



The `ExpirePendingOrders` recurring job runs through Hangfire. The flow is split across the job handler (detection and publication) and two dedicated integration event handlers (cancellation and stock restoration).



\#### Phase 1: Detect Expired Orders and Publish the Event



```text

Hangfire trigger (every N hours)

&#x20;  ↓

ExpirePendingOrdersJob

&#x20;  ↓

ExpirePendingOrdersJobHandler

&#x20;  ├── Calculate expiration threshold from

&#x20;  │   OrderingOptions.PendingOrderExpirationHours

&#x20;  ├── Call IPendingOrderCleanupService.GetExpiredPendingOrderIdsAsync

&#x20;  │   └── Query-only: returns IDs of PendingPayment orders

&#x20;  │       older than the threshold. No mutation, no save.

&#x20;  ├── If no IDs returned, log and return

&#x20;  ├── Build a single OrdersExpiredIntegrationEvent

&#x20;  │   carrying the batch of expired order IDs

&#x20;  ├── IOutboxMessageWriter.WriteAsync(event) (Channel 5, direct write)

&#x20;  └── UnitOfWork.SaveChangesAsync

&#x20;      (atomic: Outbox row committed)

```



\#### Phase 2: Cancel Expired Orders



The Outbox dispatcher delivers `OrdersExpiredIntegrationEvent` to `CancelExpiredOrdersIntegrationEventHandler`.



For each order ID:



1\. Load the order.

2\. If already `Cancelled`, skip it (idempotent).

3\. If not `PendingPayment`, skip it because the order has advanced past expiration.

4\. Call `order.Cancel()`, transitioning `PendingPayment` → `Cancelled` and raising `OrderCancelledDomainEvent`.

5\. Save changes. The resulting `OrderCancelledIntegrationEvent` is written to the Outbox atomically.

6\. Process each order in its own transaction.



\#### Phase 3: Restore Stock



The Outbox dispatcher also delivers `OrdersExpiredIntegrationEvent` to `RestoreStockOnExpiredOrdersIntegrationEventHandler`.



For each order ID:



1\. Load the order.

2\. If its status is not `Cancelled`, throw because cancellation may not yet have committed. The Outbox will retry.

3\. For each item, call `IStockService.IncreaseStockAsync`.

4\. Save changes in a per-order transaction.



Each handler is independently retryable.



No separate `OrderCancelledIntegrationEvent` is emitted outside the normal cancellation path. The event is emitted once per cancelled order through `OrderCancelledDomainEvent`, just as in user-initiated cancellation.



\*\*Stock restoration:\*\* Performed by `RestoreStockOnExpiredOrdersIntegrationEventHandler`. Unlike user-initiated cancellation, the expiry path does not gate stock restoration behind a business-policy flag in the current implementation.



\*\*Concurrency with user-initiated cancellation:\*\* Both paths can attempt to cancel the same order simultaneously. `RowVersion` on `Order` protects against double cancellation. The losing transaction throws `ConcurrencyException`, rolls back, and the Outbox message is marked `Failed` for retry. On retry, the order is already `Cancelled`, and the handler skips it.



\*\*Transaction model:\*\* `ExpirePendingOrdersJobHandler` performs a single save for the Outbox row, using auto mode without order-state mutation. Each subsequent order operation runs in its own transaction inside the respective integration event handler.



\---



\## 12. Correlation ID Propagation



The original correlation ID from the HTTP request flows through:



1\. `IAppContext.CorrelationId` is set by middleware.

2\. Domain event handlers set it on created integration events.

3\. The `CorrelationScopeIntegrationEventHandler` decorator restores it in the logging scope.

4\. `OrderPaidDomainEventHandler` uses the same `IAppContext` to set `CorrelationId` on `OrderReadyForShippingIntegrationEvent`.



This ensures trace continuity across asynchronous boundaries.



\*\*Note on the expiry flow:\*\* `OrdersExpiredIntegrationEvent` originates from a Hangfire job, not an HTTP request. `IAppContext.CorrelationId` resolves from `IHttpContextAccessor`, which is null on a job thread, so the correlation ID on `OrdersExpiredIntegrationEvent` is null. This is deliberate: the job has no originating user action. If correlation to the job scheduling event is required in the future, `IJobContext.CorrelationId` (per the scheduling abstraction) is the correct source.



\---



\## 13. Transaction Boundaries Summary



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



\*\*Transaction mode:\*\* Checkout uses manual mode (`BeginTransactionAsync` / `CommitTransactionAsync`). All other transactions in the table use `IUnitOfWork` auto mode, where `SaveChangesAsync` opens its own transaction, dispatches domain events, and commits. Both modes are managed by `IUnitOfWork`; the distinction is whether the caller opens the transaction explicitly.



\---



\## 14. Idempotency



\* All inbound integration event handlers are wrapped with the `IdempotentIntegrationEventHandler<T>` decorator.

\* Outbox delivery guarantees at-least-once delivery; idempotency prevents duplicate side effects.

\* Duplicate events are safely ignored by the idempotency layer.

\* Invalid or out-of-order events are rejected by the aggregate and should be handled according to the integration-event failure/retry policy, not silently swallowed.

\* \*\*Ordering between split handlers:\*\* The two `OrdersExpiredIntegrationEvent` handlers converge regardless of execution order. If `RestoreStockOnExpiredOrdersIntegrationEventHandler` runs first, it throws because the order is not yet `Cancelled`; the Outbox marks the message `Failed` and retries. On retry, `CancelExpiredOrdersIntegrationEventHandler` has committed, and the stock handler completes. The worst case is one extra retry cycle.



\---



\## 15. Conclusion



The Ordering workflows are fully specified and consistent with the modular monolith architecture. They use domain events for internal reactions, integration events for cross-context communication, and synchronous contracts only when immediate consistency is required. The combination of UnitOfWork transactions, Outbox, and idempotency supports reliability and maintainability.



The expiry flow is deliberately split: the job detects expired orders, publishes a single bulk event, and performs no state mutation itself. Cancellation and stock restoration are performed by two independent integration event handlers, each per-order transactional, independently retryable, and idempotent under duplicate delivery. This preserves the job's single responsibility and keeps the mutation paths observable and testable in isolation.



\---



\*\*Document version:\*\* 2.2 – Final

\*\*Feature:\*\* Ordering Workflows



