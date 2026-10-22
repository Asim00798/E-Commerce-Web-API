# Shipping Feature — Workflow Technical Document

**Project:** E-Commerce Modular Monolith

**Scope:** Shipment Lifecycle, Driver Assignment, Delivery Attempts, Returns, Cancellation, and Fee Calculation

**Version:** 1.1 — Final

---

## 1. Overview

This document defines the complete end-to-end workflows for the Shipping bounded context.

It covers:

* Distance-based shipping fee calculation during checkout.
* Shipment creation from Ordering integration.
* Driver assignment and reassignment.
* Shipment lifecycle transitions.
* Delivery attempts and retry policy.
* Return-to-sender flow.
* Shipment cancellation.
* Domain events and outbound integration events.

Shipping is an internal-delivery bounded context. It is client-agnostic, meaning multiple client applications (Customer, Driver, Packaging/Warehouse, Dispatcher/Admin) consume the same backend capabilities through separate API controllers with role and permission authorization.

All cross-context communication uses the existing Outbox mechanism. Shipping has no external-provider reconciliation because the company owns the delivery operation.

---

## 2. Actors and Components

### 2.1 Actors

| Actor               | Description                                                                             |
| ------------------- | --------------------------------------------------------------------------------------- |
| Customer            | Views their own shipments and follows delivery status.                                  |
| Driver              | Performs pickup, starts delivery, records attempts, and begins returns.                 |
| Packaging/Warehouse | Marks shipments ready for pickup and completes returns.                                 |
| Dispatcher/Admin    | Assigns and reassigns drivers, cancels shipments, and views shipments.                  |
| Ordering Context    | Publishes `OrderReadyForShippingIntegrationEvent` and `OrderCancelledIntegrationEvent`. |
| Outbox Processor    | Executes inbound integration-event handlers reliably and idempotently.                  |

### 2.2 Key Components

| Component                      | Layer              | Responsibility                                                   |
| ------------------------------ | ------------------ | ---------------------------------------------------------------- |
| Shipment aggregate             | Domain             | Owns delivery lifecycle, invariants, attempts, and cancellation. |
| `DeliveryAttempt` entity       | Domain             | Records individual delivery results.                             |
| `DeliveryAddressSnapshot`      | Domain             | Immutable address snapshot for a shipment.                       |
| `ShippingDistance`             | Domain             | Distance value object.                                           |
| `LocalShippingFeePolicy`       | Domain             | Calculates fees from distance.                                   |
| `IShippingFeeCalculator`       | Shared Application | Cross-context fee contract used by Ordering.                     |
| `ILocationService`             | Application        | Resolves distance from an address map URL.                       |
| `MapLocationService`           | Infrastructure     | Parses map URLs and calculates distance.                         |
| `ShippingFeeCalculatorService` | Infrastructure     | Orchestrates fee calculation.                                    |
| `IShipmentRepository`          | Domain             | Persistence abstraction.                                         |
| `IUnitOfWork`                  | SharedKernel       | Transaction boundary.                                            |
| `IOutboxMessageWriter`         | Application        | Writes outbound integration events.                              |

---

## 3. Workflow 1: Shipping Fee Calculation (Synchronous, Cross-Context)

### 3.1 Trigger

The customer is in checkout and has selected a delivery address.

### 3.2 Flow

```text
Ordering Checkout
       │
       ▼
IShippingFeeCalculator.CalculateAsync(request)
       │
       ▼
ShippingFeeCalculatorService
       │
       ├── Map shared request → DeliveryAddressSnapshot
       │
       ├── ILocationService.GetDeliveryDistanceAsync(address)
       │       │
       │       ▼
       │   MapLocationService
       │       │
       │       ▼
       │   ShippingDistance
       │
       └── LocalShippingFeePolicy.CalculateFee(distance)
                   │
                   ▼
            ShippingFeeResult
                   │
                   ▼
       Map to shared ShippingFeeCalculationResult
                   │
                   ▼
            Return to Ordering
                   │
                   ▼
       Ordering stores Order.ShippingFee
                   │
                   ▼
       Ordering includes it in Order.Total
                   │
                   ▼
       Payment charges Order.Total
```

### 3.3 Important Rules

* Shipping calculates the fee; Ordering persists the finalized monetary value.
* The Shipment aggregate does not recalculate the shipping fee.
* The fee result is transient. Changing future fee rules never affects historical orders.

### 3.4 Fee Bands

| Distance |         Shipping Fee |
| -------- | -------------------: |
| 0–5 km   |               10 AED |
| 5–10 km  |               15 AED |
| 10–20 km |               25 AED |
| >20 km   | Outside service area |

---

## 4. Workflow 2: Shipment Creation (Asynchronous Integration Event)

### 4.1 Trigger

Ordering publishes `OrderReadyForShippingIntegrationEvent` after the order becomes eligible for shipping.

### 4.2 Flow

```text
Ordering
  │
  ▼
OrderReadyForShippingIntegrationEvent
  │
  ▼
Outbox
  │
  ▼
Outbox Processor
  │
  ▼
OrderReadyForShippingIntegrationEventHandler
  │
  ├── Check existing active shipment by OrderId
  ├── If one exists → return (idempotent)
  │
  ├── Build DeliveryAddressSnapshot
  ├── Generate TrackingNumber
  │
  └── Shipment.Create(...)
          │
          ├── Status = Created
          └── Raises ShipmentCreatedDomainEvent
                   │
                   ▼
          IShipmentRepository.AddAsync
                   │
                   ▼
          IUnitOfWork.SaveChangesAsync
```

### 4.3 Idempotency

* The handler checks `GetActiveByOrderIdAsync`.
* The database enforces one active shipment per order through a filtered unique index.
* Duplicate `OrderReadyForShippingIntegrationEvent` messages are safe.

---

## 5. Workflow 3: Shipment Lifecycle

### 5.1 State Transitions

```text
Created
  ↓ AssignDriver
Assigned
  ↓ MarkReadyForPickup
ReadyForPickup
  ↓ MarkPickedUp
PickedUp
  ↓ StartDelivery
OutForDelivery
  ├── RecordDeliveryAttempt(Delivered) → Delivered (terminal)
  └── RecordDeliveryAttempt(Failed) → remains OutForDelivery
         ↓ RetryDelivery if allowed
         OutForDelivery
         ↓ If maximum attempts exceeded
         BeginReturn → ReturnToSender
         ↓ CompleteReturn
         Returned (terminal)
```

### 5.2 Terminal States

```text
Delivered

Returned

Cancelled
```

No transition is allowed out of a terminal state.

### 5.3 The Shipped Fact

The aggregate's state machine has no `Shipped` state. The precise internal states are `PickedUp` and `OutForDelivery`.

However, entering `OutForDelivery` is a business fact that other contexts react to. When `StartDelivery()` transitions the aggregate to `OutForDelivery`, the aggregate raises `ShipmentShippedDomainEvent`. That domain event is translated by `ShipmentShippedDomainEventHandler` into `ShipmentShippedIntegrationEvent` and written to the Outbox.

This separation is deliberate:

* **State:** `OutForDelivery` — the aggregate's precise internal representation.
* **Event:** Shipped — the business fact consumed by Ordering and Notifications.
* **UI label:** "Shipped" is a presentation concern, unrelated to either.

---

## 6. Workflow 4: Driver Assignment

### 6.1 Assign Driver

**Trigger:** Dispatcher/Admin uses `AssignDriverCommand`.

```text
Dispatcher/Admin
  ↓
AssignDriverCommand
  ↓
MediatR Pipeline
  ├── PermissionAuthorizationBehavior: Shipments.Assign
  └── Handler
       ├── Load Shipment
       ├── Shipment.AssignDriver(driverId)
       ├── Update repository
       └── UnitOfWork.SaveChangesAsync
```

### 6.2 Reassign Driver

The workflow is similar but uses `ReassignDriverCommand` and `Shipment.ReassignDriver(newDriverId)`.

### 6.3 Authorization

* **Permission:** `Shipments.Assign`
* No resource authorization is needed because Dispatcher/Admin has broad scope.

---

## 7. Workflow 5: Delivery Execution

### 7.1 Mark Ready for Pickup

**Trigger:** Packaging/Warehouse.

```text
Packaging/Warehouse
  ↓
MarkReadyForPickupCommand
  ↓
Shipment.MarkReadyForPickup()
  ↓
Status = ReadyForPickup
```

**Permission:** `Shipments.Manage`

### 7.2 Mark Picked Up

**Trigger:** Driver or Packaging/Warehouse, depending on the workflow.

```text
Driver / Packaging
  ↓
MarkPickedUpCommand
  ↓
Shipment.MarkPickedUp()
  ↓
Status = PickedUp
```

**Permission:** `Shipments.Deliver`

Resource authorization ensures the driver is assigned to the shipment when the driver performs the operation.

### 7.3 Start Delivery

**Trigger:** Driver. This transition raises `ShipmentShippedDomainEvent`.

```text
Driver
  ↓
StartDeliveryCommand
  ↓
Shipment.StartDelivery()
  │
  ├── Status = OutForDelivery
  └── Raises ShipmentShippedDomainEvent
  ↓
UnitOfWork.SaveChangesAsync
  │
  ▼
Domain Event Handler (ShipmentShippedDomainEventHandler)
  │
  ▼
ShipmentShippedIntegrationEvent
  │
  ▼
Outbox
  │
  ├── Ordering     (Paid → Shipped)
  └── Notifications (OrderShippedSms)
```

**Permission:** `Shipments.Deliver`

**Resource authorization:** `AssignedDriverId == current user ID`

---

## 8. Workflow 6: Delivery Attempts

### 8.1 Record Successful Attempt

```text
Driver
  ↓
RecordDeliveryAttemptCommand(Result = Delivered)
  ↓
Shipment.RecordDeliveryAttempt(Delivered)
  │
  ├── Adds DeliveryAttempt with result Delivered
  ├── Status = Delivered
  ├── Raises ShipmentDeliveredDomainEvent
  └── Saves
       │
       ▼
Domain Event Handler
  │
  ▼
ShipmentDeliveredIntegrationEvent
  │
  ▼
Outbox
  │
  ├── Ordering
  └── Notifications
```

### 8.2 Record Failed Attempt

```text
Driver
  ↓
RecordDeliveryAttemptCommand(Result = CustomerUnavailable)
  ↓
Shipment.RecordDeliveryAttempt(CustomerUnavailable)
  │
  ├── Adds DeliveryAttempt with failure
  ├── Status remains OutForDelivery
  └── Saves
```

No domain event is raised for a failed attempt because no external business fact currently requires a reaction.

---

## 9. Workflow 7: Retry Delivery

### 9.1 Trigger

The Driver or Dispatcher initiates a retry, depending on policy.

### 9.2 Flow

```text
RetryDeliveryCommand
  ↓
Shipment.Retry(maximumDeliveryAttempts)
  │
  ├── Attempt count < maximum → remains OutForDelivery
  │
  └── Attempt count >= maximum → BeginReturn()
          │
          ▼
     ReturnToSender
```

**Permission:** `Shipments.Deliver`

**Resource authorization:** Assigned driver.

---

## 10. Workflow 8: Return-to-Sender

### 10.1 Begin Return

```text
BeginReturnCommand
  ↓
Shipment.BeginReturn()
  ↓
Status = ReturnToSender
```

**Permission:** `Shipments.Deliver`

### 10.2 Complete Return

```text
CompleteReturnCommand
  ↓
Shipment.CompleteReturn()
  │
  ├── Status = Returned
  ├── Raises ShipmentReturnedDomainEvent
  └── Saves
       │
       ▼
Domain Event Handler
  │
  ▼
ShipmentReturnedIntegrationEvent
  │
  ▼
Outbox
  │
  ├── Ordering
  └── Notifications
```

**Permission:** `Shipments.Manage`

---

## 11. Workflow 9: Shipment Cancellation

### 11.1 Trigger

Ordering publishes `OrderCancelledIntegrationEvent`, or Dispatcher/Admin uses `CancelShipmentCommand`.

### 11.2 Integration Event Flow

```text
Ordering
  ↓
OrderCancelledIntegrationEvent
  ↓
Outbox
  ↓
Outbox Processor
  ↓
OrderCancelledIntegrationEventHandler
  ├── Find active shipment by OrderId
  ├── If none → return
  ├── Attempt Shipment.Cancel()
  │     ├── If state is Created/Assigned/ReadyForPickup → Cancelled
  │     └── If state is PickedUp or later → catch ShipmentException, no change
  └── Save
```

### 11.3 Cancellation Rules

**Allowed from:**

```text
Created

Assigned

ReadyForPickup
```

**Not allowed from:**

```text
PickedUp

OutForDelivery

ReturnToSender

Delivered

Returned

Cancelled
```

If the shipment is already terminal, the handler treats the operation as successful.

### 11.4 No Outbound Event on Cancellation

`Shipment.Cancel()` raises `ShipmentCancelledDomainEvent`. No handler exists for it, and no outbound integration event is produced.

Reasons:

* **User-initiated cancellation** through `CancelShipmentCommand`: Ordering is not notified because Ordering initiated the cancellation flow or has no reaction to a shipment-level cancellation.
* **Integration-event-driven cancellation** through `OrderCancelledIntegrationEvent`: Ordering already knows the order is cancelled.

If a future consumer needs the cancellation fact, a handler can be added without changing the aggregate.

---

## 12. Cross-Context Event Summary

| Event                                   | Direction                          | When                                                            |
| --------------------------------------- | ---------------------------------- | --------------------------------------------------------------- |
| `OrderReadyForShippingIntegrationEvent` | Ordering → Shipping                | Order becomes eligible for shipping.                            |
| `OrderCancelledIntegrationEvent`        | Ordering → Shipping                | Order is cancelled.                                             |
| `ShipmentShippedIntegrationEvent`       | Shipping → Ordering, Notifications | Shipment enters the delivery process through `StartDelivery()`. |
| `ShipmentDeliveredIntegrationEvent`     | Shipping → Ordering, Notifications | Delivery succeeds.                                              |
| `ShipmentReturnedIntegrationEvent`      | Shipping → Ordering, Notifications | Return is completed.                                            |

No outbound event is published for `ShipmentCancelledDomainEvent` or `ShipmentCreatedDomainEvent` unless a future requirement arises.

---

## 13. Concurrency and Reliability

* `RowVersion` optimistic concurrency on Shipment.
* Filtered unique index on active shipment per order.
* Idempotent integration-event handlers.
* Unit of Work transaction boundary in application handlers.
* No external-provider reconciliation required.

---

## 14. Testing

### 14.1 Domain Tests

* Valid and invalid state transitions.
* Maximum-attempts enforcement.
* Return lifecycle.
* Cancellation rules.
* Fee-policy bands.
* Address snapshot immutability.
* `ShipmentShippedDomainEvent` raised on `StartDelivery()`.

### 14.2 Application Tests

* Authorization (permission and resource).
* Idempotent shipment creation and cancellation.
* Integration-event mapping, including `ShipmentShippedDomainEvent` → `ShipmentShippedIntegrationEvent`.
* Retry/return orchestration.
* Fee-calculation orchestration.

### 14.3 Integration Tests

* EF mappings.
* Filtered unique index.
* `RowVersion` concurrency.
* Outbox persistence.
* Idempotent event consumption.

### 14.4 API Tests

* Role/permission policies.
* Customer/driver isolation.
* State-transition validation.
* Cancellation endpoint behavior.

---

## 15. Conclusion

The Shipping workflows are complete and consistent with the modular monolith architecture. The feature uses domain invariants, explicit state transitions, permission and resource authorization, and Outbox integration to deliver a reliable internal-delivery capability without external-carrier complexity.

### Key Distinctions Preserved in This Revision

* The aggregate's state machine has no `Shipped` state; the precise states are `PickedUp` and `OutForDelivery`.
* `ShipmentShippedDomainEvent` fires on `StartDelivery()` and is translated into `ShipmentShippedIntegrationEvent` for cross-context consumers. This represents the business fact that the shipment has entered the delivery process, which Ordering (Paid → Shipped) and Notifications consume.
* `ShipmentCancelledDomainEvent` is raised but has no handler and no outbound integration event; no consumer reacts to a shipment-level cancellation fact.
* `ShipmentCreatedDomainEvent` is raised but has no consumer.

---

**Document version:** 1.1 — Final

**Feature:** Shipping — Internal Delivery Model Workflows
