# Shipping Feature — Requirements Technical Document

**Project:** E-Commerce Modular Monolith

**Bounded Context:** Shipping — Internal Delivery Model

**Version:** 1.5 — Final

---

## 1. Introduction

This document defines the technical requirements for the Shipping bounded context within the e-commerce modular monolith.

Shipping is responsible for managing the physical fulfillment and delivery of customer orders after an order becomes eligible for shipment. The company operates its own delivery service, and drivers are internal employees. No external shipping/carrier API is required.

The feature spans Domain, Application, Infrastructure, and API layers, following Clean Architecture and DDD principles. The Domain layer is independent of infrastructure concerns.

Shipping is client-agnostic. It exposes backend capabilities for multiple client types:

* Customer client
* Driver client
* Packaging/Warehouse client
* Dispatcher/Admin client

The client applications themselves are outside the Shipping bounded context.

---

## 2. Scope

### 2.1 In Scope

* Shipment creation after an order is ready for shipping.
* Shipment lifecycle management.
* Shipment cancellation.
* Driver assignment and reassignment.
* Delivery attempt recording.
* Delivery success and failure.
* Retry after failed delivery, controlled by a maximum-attempts policy.
* Return-to-sender flow.
* Shipping-owned delivery address snapshot.
* Distance-based shipping fee calculation.
* Shipping permissions and authorization.
* Resource-level authorization for customer, driver, packaging, and dispatcher clients.
* Cross-context communication using Outbox integration events.
* Optimistic concurrency and idempotency.

### 2.2 Out of Scope

* External carrier integration (FedEx, UPS, Aramex, etc.).
* Carrier webhooks.
* Carrier reconciliation.
* GPS tracking.
* Live driver location.
* Route optimization.
* Maps/geofencing.
* Fleet maintenance.
* Automatic dispatch optimization.
* Warehouse robotics.
* Driver mobile application UI.
* Packaging/warehouse UI.
* Customer UI.
* Dispatcher/admin UI.
* Kafka/RabbitMQ.
* Microservices.

---

## 3. Business Responsibility

Shipping answers:

> How does a confirmed order physically get from our fulfillment operation to the customer?

**Shipping owns:**

* Shipment creation.
* Shipment lifecycle.
* Shipment cancellation.
* Driver assignment.
* Delivery attempts.
* Delivery status.
* Delivery completion.
* Delivery failure.
* Return-to-sender flow.
* Shipping fee calculation rules.
* Shipping-related business rules.

**Shipping does not own:**

* Orders.
* Payments.
* Products.
* Inventory.
* Customer master data.
* Employee identity.
* Address master data.

The finalized shipping fee belongs to Ordering because the order must preserve the exact amount charged, even if shipping rates change later.

---

## 4. Core Business Scenario

### 4.1 Main Lifecycle

```text
Order
  ↓
Eligible for Shipping
  ↓
Shipment Created
  ↓
Driver Assigned
  ↓
Ready for Pickup
  ↓
Picked Up
  ↓
Out for Delivery
  ↓
Delivery Attempt
  ↓
Delivered
```

### 4.2 Delivery Failure Path

```text
Out for Delivery
  ↓
Delivery Attempt Failed
  ↓
Retry allowed?
  ├── Yes → Out for Delivery
  └── No  → Return to Sender → Returned
```

### 4.3 Cancellation Path

```text
Created / Assigned / ReadyForPickup
  ↓
Order Cancelled
  ↓
Shipment Cancelled
```

---

## 5. Shipment Aggregate

The Shipment aggregate is the primary business entity and owns the entire delivery lifecycle.

### 5.1 Conceptual Properties

```text
Shipment
├── Id
├── OrderId
├── CustomerId
├── DeliveryAddress
├── Status
├── AssignedDriverId
├── TrackingNumber
├── CreatedAtUtc
├── AssignedAtUtc
├── ReadyForPickupAtUtc
├── PickedUpAtUtc
├── OutForDeliveryAtUtc
├── DeliveredAtUtc
├── ReturnedAtUtc
├── CancelledAtUtc
├── RowVersion
└── DeliveryAttempts
```

`DeliveryAttempts` are owned entities inside the Shipment aggregate.

---

## 6. Shipment State Machine

The state machine must be explicitly enforced inside the aggregate.

### 6.1 States

```text
Created
Assigned
ReadyForPickup
PickedUp
OutForDelivery
ReturnToSender
Delivered
Returned
Cancelled
```

`Delivered`, `Returned`, and `Cancelled` are terminal states.

### 6.2 Frozen Lifecycle

```text
Created
  ├── AssignDriver
  │       ↓
  │    Assigned
  │       ↓ MarkReadyForPickup
  │    ReadyForPickup
  │       ↓ MarkPickedUp
  │    PickedUp
  │       ↓ StartDelivery
  │    OutForDelivery
  │       ├── RecordDeliveryAttempt(Delivered) → Delivered (terminal)
  │       └── RecordDeliveryAttempt(Failed)
  │                   ↓
  │              Attempt recorded
  │                   ↓
  │              Can retry according to maximum-attempts policy?
  │                   ├── Yes → Retry → OutForDelivery
  │                   └── No  → BeginReturn → ReturnToSender
  │                                      ↓ CompleteReturn
  │                                  Returned (terminal)
  │
  ├── Cancel (from Created/Assigned/ReadyForPickup) → Cancelled (terminal)
  │
  └── Cancel not allowed from PickedUp/OutForDelivery/ReturnToSender/Delivered/Returned
```

### 6.3 Important Distinctions

* A failed attempt does not create a separate `DeliveryAttemptFailed` shipment state. The shipment can remain `OutForDelivery` while its `DeliveryAttempts` collection records the failure.
* `ReturnToSender` is not terminal; `Returned` is terminal.
* `Cancelled` is terminal.
* No transition is allowed out of `Delivered`, `Returned`, or `Cancelled`.
* **Do not add a `Shipped` status.** `PickedUp` and `OutForDelivery` are precise backend states. The UI may display "Shipped" as a presentation concern, but it has no domain meaning.
* **The absence of a `Shipped` state does not mean the absence of a `Shipped` event.** `ShipmentShippedDomainEvent` is raised when the aggregate transitions to `OutForDelivery` via `StartDelivery()`. The event captures the business fact that the shipment has entered the delivery process, while the aggregate's state remains `OutForDelivery`, the precise internal representation. This distinction matters because the event is a first-class cross-context contract consumed by Ordering and Notifications.

---

## 7. Maximum Delivery Attempts Policy

The aggregate owns the invariant that controls when a retry is allowed.

### 7.1 Domain Rule

A shipment can be retried only if:

```text
Successful delivery has not occurred

AND

Current attempt count < MaximumDeliveryAttempts
```

### 7.2 Policy Value

The maximum-attempts value comes from Application/configuration and is passed into the aggregate method:

```csharp
shipment.Retry(maximumDeliveryAttempts);
```

The Domain does not hard-code a number. It enforces the rule based on the supplied value and its own attempt history.

### 7.3 Failed Attempt Behavior

When a delivery attempt fails:

1. Record a `DeliveryAttempt` with a failure result.
2. Keep the shipment in `OutForDelivery`.
3. Check whether retry is allowed according to the application/domain policy.
4. If allowed, the shipment can be retried.
5. If not allowed, transition the shipment to `ReturnToSender` and then `Returned`.

---

## 8. Delivery Attempt Entity

`DeliveryAttempt` is an entity inside the Shipment aggregate. It is not a separate aggregate.

### 8.1 Conceptual Properties

```text
DeliveryAttempt
├── Id
├── ShipmentId
├── AttemptNumber
├── AttemptedAtUtc
├── Result
├── FailureReason
└── Notes
```

### 8.2 Delivery Attempt Result

Use a domain enum:

```csharp
public enum DeliveryAttemptResult
{
    Delivered,
    CustomerUnavailable,
    WrongAddress,
    CustomerRefused,
    Other
}
```

Only include values required by the business.

---

## 9. Delivery Address Snapshot

Shipping must store the delivery address used for a shipment at the time it is created. The address must not change if the customer later changes their address.

### 9.1 Value Object

`DeliveryAddressSnapshot` must contain:

```text
DeliveryAddressSnapshot
├── FullName
├── PhoneNumber
├── Street
├── City
└── LocationMapUrl
```

This is a Shipping-owned value object. It is not the same as the People context's `Address`. The value object must be immutable after shipment creation.

---

## 10. Driver Assignment

A Shipment may be assigned to an internal driver.

### 10.1 Driver Reference

Shipping stores only:

```text
AssignedDriverId
```

The Shipping context does not own employee lifecycle. It references an identity owned elsewhere.

### 10.2 Assignment Rules

* Assignment is only allowed from an assignable state.
* A delivered or returned shipment cannot be assigned.
* A cancelled shipment cannot be assigned.
* A driver can only act on shipments assigned to them.
* Assignment must be an explicit aggregate behavior.

Example:

```csharp
Shipment.AssignDriver(driverId);
```

If reassignment is needed:

```csharp
Shipment.ReassignDriver(newDriverId);
```

There must be no public setters for `AssignedDriverId`.

### 10.3 Reassignment

The first version supports reassignment by overwriting `AssignedDriverId` and updating `AssignedAtUtc`.

No assignment history table is introduced unless business requirements explicitly demand it.

---

## 11. Shipment Creation

A Shipment is created when Shipping receives a business signal that an order is ready for shipping.

### 11.1 Trigger

Ordering publishes:

```text
OrderReadyForShippingIntegrationEvent
```

The event must include:

```text
OrderId

CustomerId

FullName

PhoneNumber

Street

City

LocationMapUrl
```

Shipping consumes this event and creates:

```text
Shipment
Status = Created
```

Shipment creation is performed directly inside the integration event handler using `IShipmentRepository` and `IUnitOfWork`. There is no `CreateShipmentCommand`.

### 11.2 Idempotency

A shipment must not be created twice for the same active order.

Enforce:

> One active shipment per order.

Use a filtered unique index on `OrderId` where the status is not terminal.

**Active statuses:**

```text
Created
Assigned
ReadyForPickup
PickedUp
OutForDelivery
ReturnToSender
```

**Terminal statuses:**

```text
Delivered
Returned
Cancelled
```

This allows a historical returned/cancelled shipment and a new shipment for the same order if the business later requires it.

---

## 12. Application Commands

Initial commands:

* `AssignDriverCommand`
* `ReassignDriverCommand`
* `MarkReadyForPickupCommand`
* `MarkPickedUpCommand`
* `StartDeliveryCommand`
* `RecordDeliveryAttemptCommand`
* `RetryDeliveryCommand`
* `BeginReturnCommand`
* `CompleteReturnCommand`
* `CancelShipmentCommand`

`RecordDeliveryAttemptCommand` may handle both successful and failed attempts based on the provided result.

The Application layer must supply the maximum-attempts value to the retry operation from configuration or policy.

All commands are decorated with `AuthorizePermission` attributes. Resource authorization, such as checking whether a driver is assigned to a shipment, is enforced in handlers.

---

## 13. Queries

Initial queries:

* `GetShipmentByIdQuery`
* `GetShipmentByOrderIdQuery`
* `GetCustomerShipmentsQuery`
* `GetDriverShipmentsQuery`

All queries are decorated with `AuthorizePermission` attributes. Handlers also enforce resource authorization.

---

## 14. API Surface

The API is split by client responsibility.

### 14.1 Controllers

| Controller                      | Route Prefix                | Default Role              |
| ------------------------------- | --------------------------- | ------------------------- |
| `CustomerShipmentsController`   | `/api/customer/shipments`   | Customer                  |
| `DriverShipmentsController`     | `/api/driver/shipments`     | Driver                    |
| `DispatcherShipmentsController` | `/api/dispatcher/shipments` | Dispatcher, Administrator |
| `PackagingShipmentsController`  | `/api/packaging/shipments`  | Packaging                 |

### 14.2 Endpoints

```http
# Customer
GET    /api/customer/shipments/{shipmentId}
GET    /api/customer/shipments/by-order/{orderId}
GET    /api/customer/shipments

# Driver
GET    /api/driver/shipments
POST   /api/driver/shipments/{shipmentId}/pickup
POST   /api/driver/shipments/{shipmentId}/start-delivery
POST   /api/driver/shipments/{shipmentId}/delivery-attempt
POST   /api/driver/shipments/{shipmentId}/retry
POST   /api/driver/shipments/{shipmentId}/begin-return

# Dispatcher
GET    /api/dispatcher/shipments/{shipmentId}
POST   /api/dispatcher/shipments/{shipmentId}/assign-driver
POST   /api/dispatcher/shipments/{shipmentId}/reassign-driver
POST   /api/dispatcher/shipments/{shipmentId}/cancel

# Packaging
POST   /api/packaging/shipments/{shipmentId}/ready-for-pickup
POST   /api/packaging/shipments/{shipmentId}/complete-return
```

Controllers use role-based authorization at the class level and permission policies at the action level.

---

## 15. Authorization

### 15.1 Permissions

```text
Shipments.Read
Shipments.Assign
Shipments.Manage
Shipments.Deliver
```

Shipment creation is a system/application operation triggered by integration events. It is not assigned as a user permission.

Cancellation is protected by `Shipments.Manage` or handled through an integration event from Ordering.

### 15.2 Role/Permission Mapping

| Role                | Permissions                                                     |
| ------------------- | --------------------------------------------------------------- |
| Customer            | `Shipments.Read` — own shipments only                           |
| Driver              | `Shipments.Read` — assigned shipments only; `Shipments.Deliver` |
| Packaging/Warehouse | Appropriate Shipping permissions for pickup and return          |
| Dispatcher/Admin    | `Shipments.Assign`, `Shipments.Manage`                          |
| Administrator       | Full access                                                     |

### 15.3 Resource Authorization

* Customers can only see shipments where `CustomerId` matches the current user.
* Drivers can only modify shipments where `AssignedDriverId` matches the current driver.
* Dispatchers/Admins can manage shipments within their scope.
* Packaging/Warehouse roles have permissions for specific operational steps.

Authorization is enforced in the Application layer through `AuthorizePermission` attributes on commands and queries and resource checks inside handlers. API controllers add defense in depth through role and permission policies.

---

## 16. Domain Events

Shipping raises the following domain events:

```text
ShipmentCreatedDomainEvent

ShipmentShippedDomainEvent

ShipmentDeliveredDomainEvent

ShipmentReturnedDomainEvent

ShipmentCancelledDomainEvent
```

### 16.1 Events Handled by Application Domain-Event Handlers

* `ShipmentShippedDomainEvent` — translated into `ShipmentShippedIntegrationEvent` in the Outbox.
* `ShipmentDeliveredDomainEvent` — translated into `ShipmentDeliveredIntegrationEvent` in the Outbox.
* `ShipmentReturnedDomainEvent` — translated into `ShipmentReturnedIntegrationEvent` in the Outbox.

### 16.2 Events Raised but Not Handled

* `ShipmentCreatedDomainEvent` — has no cross-context consumer.
* `ShipmentCancelledDomainEvent` — has no cross-context consumer. Cancellation is either user-initiated, in which case Ordering already knows, or a response to `OrderCancelledIntegrationEvent`.

No other status transitions raise domain events unless a real consumer exists.

---

## 17. Integration Events

Meaningful events that need to leave Shipping are published through the Outbox.

### 17.1 Inbound

```text
OrderReadyForShippingIntegrationEvent

OrderCancelledIntegrationEvent
```

### 17.2 Outbound

```text
ShipmentShippedIntegrationEvent

ShipmentDeliveredIntegrationEvent

ShipmentReturnedIntegrationEvent
```

**Possible consumers:**

* Ordering
* Notifications
* Analytics

Events must contain only primitive/contract data, not domain entities.

---

## 18. Shipping Fee Calculation

### 18.1 Responsibility Split

* Shipping owns shipping fee calculation rules and exposes a shared cross-context `IShippingFeeCalculator` abstraction.
* Ordering requests the calculation during checkout/order pricing.
* Ordering stores the finalized `Order.ShippingFee` and includes it in `Order.Total`.
* Payment charges the finalized `Order.Total`; it does not calculate shipping fees.
* Shipment handles physical fulfillment and delivery; it does not recalculate the original order's shipping fee.

The fee result returned by Shipping is transient. Ordering persists the accepted monetary value as part of the order's financial snapshot.

### 18.2 Flow

```text
Customer selects delivery address
       ↓
Ordering calls IShippingFeeCalculator (Shared)
       ↓
ShippingFeeCalculationResult returned (Shared)
       ↓
Ordering stores Order.ShippingFee
       ↓
Ordering includes it in Order.Total
       ↓
Payment charges Order.Total
       ↓
OrderReadyForShippingIntegrationEvent
       ↓
Shipping creates Shipment
```

### 18.3 Distance-Based Pricing

The system uses the company's local fulfillment location and the customer's delivery map URL to compute distance, then applies local distance bands.

```text
Delivery Address (including LocationMapUrl)
       ↓
ILocationService resolves ShippingDistance
       ↓
LocalShippingFeePolicy applies fee bands
       ↓
ShippingFeeResult
```

**Initial fee bands:**

| Distance |         Shipping Fee |
| -------- | -------------------: |
| 0–5 km   |               10 AED |
| 5–10 km  |               15 AED |
| 10–20 km |               25 AED |
| >20 km   | Outside service area |

The policy is an extension point for future rules.

---

## 19. Concurrency and Reliability

* Use `RowVersion` optimistic concurrency on Shipment.
* Use a filtered unique index to enforce one active shipment per order.
* Use transactional persistence through `IUnitOfWork`.
* Use the Outbox for cross-context integration events.
* Use idempotent integration-event consumption.
* Background jobs are optional and should only be introduced to satisfy a concrete requirement.
* Shipping does not require external-provider reconciliation.

---

## 20. Persistence

Likely tables:

```text
shipping.Shipments

shipping.DeliveryAttempts
```

`DeliveryAttempts` are owned by Shipment and persisted as part of the aggregate.

No separate driver assignment table is initially required.

The specific EF Core mapping, whether as an owned entity or dependent entity, is an Infrastructure-layer decision.

---

## 21. Non-Functional Requirements

### 21.1 Performance

* Standard read/write operations should complete within typical database/API latency.
* No external provider latency is involved.

### 21.2 Scalability

* Stateless API instances.
* Concurrency protected by `RowVersion` and database unique constraints.

### 21.3 Reliability

* Duplicate `OrderReadyForShippingIntegrationEvent` messages must not create duplicate active shipments.
* Duplicate `OrderCancelledIntegrationEvent` messages must not cause errors; handling must be idempotent.
* Invalid state transitions must be rejected by the aggregate.
* Concurrent driver assignments and delivery updates must not corrupt state.

### 21.4 Maintainability

* The Domain layer must be independent of EF Core and Infrastructure.
* API controllers must remain thin.
* Application handlers must contain use-case orchestration only.

### 21.5 Testability

* Domain aggregate rules must be testable without Infrastructure.
* Application handlers must be testable with mocked repositories and current-user context.
* Integration tests must cover persistence, unique constraints, and concurrency.

---

## 22. Security Requirements

* Enforce customer isolation.
* Enforce driver-assignment isolation.
* Use the existing permission/policy infrastructure.
* Store no sensitive data beyond the shipping address and customer information required for delivery.
* Integration events must not expose unnecessary internal identifiers.

---

## 23. Integration Requirements

### 23.1 Ordering → Shipping

Ordering publishes:

```text
OrderReadyForShippingIntegrationEvent

OrderCancelledIntegrationEvent
```

Shipping consumes these events and creates or cancels a Shipment accordingly.

The `OrderReadyForShippingIntegrationEvent` must include the delivery address snapshot. Shipping must not query Ordering later to retrieve the address.

### 23.2 Shipping → Ordering/Notifications

Shipping publishes:

```text
ShipmentShippedIntegrationEvent

ShipmentDeliveredIntegrationEvent

ShipmentReturnedIntegrationEvent
```

Ordering and Notifications consume these through the Outbox-based integration-event flow.

Specifically:

* `ShipmentShippedIntegrationEvent` → Ordering marks the order as Shipped; Notifications may send an SMS to the customer.
* `ShipmentDeliveredIntegrationEvent` → Ordering marks the order as Delivered; Notifications schedules the feedback request.
* `ShipmentReturnedIntegrationEvent` → Ordering marks the order as Returned.

---

## 24. Testing Requirements

### 24.1 Domain Tests

* Valid state transitions.
* Invalid state transitions.
* Assign/Reassign rules.
* Cancellation rules.
* Delivery attempt recording.
* Delivery success.
* Delivery failure and retry.
* Maximum-attempts policy.
* Return-to-sender.
* Duplicate delivery prevention.
* Address snapshot immutability.
* Shipping fee policy for each distance band.

### 24.2 Application Tests

* Idempotent shipment creation through the integration-event handler.
* Idempotent shipment cancellation.
* Handling non-cancellable shipments.
* Driver assignment with authorization.
* Driver authorization isolation.
* Customer read authorization.
* Recording delivery attempts.
* Retry limits.
* Beginning and completing a return.
* Integration-event creation, including `ShipmentShippedIntegrationEvent` on `StartDelivery()`.

### 24.3 Integration Tests

* EF Core mappings.
* Filtered unique index on active shipments.
* `RowVersion` concurrency.
* Outbox event persistence.
* Idempotent consumption of `OrderReadyForShippingIntegrationEvent`.
* Idempotent consumption of `OrderCancelledIntegrationEvent`.

### 24.4 API Tests

* Permission enforcement.
* Role-based controller authorization.
* Customer isolation.
* Driver isolation.
* Valid and invalid state transitions.
* Cancellation endpoint behavior.

---

## 25. Configuration Requirements

Shipping has no external-provider configuration.

Configuration includes:

```text
Shipping:MaximumDeliveryAttempts

Shipping:Location:CompanyLatitude

Shipping:Location:CompanyLongitude
```

The maximum-attempts value is supplied to the Domain as a value; it is not hard-coded.

---

## 26. Assumptions and Constraints

### 26.1 Assumptions

* Orders are already created and eligible for shipping.
* Customer and driver identities exist elsewhere.
* The company operates its own delivery fleet.
* The address snapshot is sufficient for delivery.
* No external carrier integration is required initially.

### 26.2 Constraints

* The Domain must not reference EF Core or Infrastructure.
* Shipping must not directly mutate Order tables.
* Shipping must not contain notification implementation.
* Shipping must not create external-carrier machinery.

---

## 27. Traceability Matrix

| Requirement Area  | Description                                                                                                                                                                                       |
| ----------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Domain            | Shipment aggregate, lifecycle, and invariant enforcement                                                                                                                                          |
| Value Objects     | `DeliveryAddressSnapshot`, `ShippingDistance`, `ShippingFeeResult`                                                                                                                                |
| Delivery Attempts | Owned entity inside Shipment                                                                                                                                                                      |
| Assignment        | Explicit aggregate methods                                                                                                                                                                        |
| Maximum Attempts  | Aggregate enforces retry/return policy                                                                                                                                                            |
| Cancellation      | Explicit aggregate method, policy-driven                                                                                                                                                          |
| Shipping Fee      | Shipping calculates distance-based fees; Ordering persists the accepted amount                                                                                                                    |
| Idempotency       | Filtered unique index on active shipment per order                                                                                                                                                |
| Concurrency       | `RowVersion` optimistic concurrency                                                                                                                                                               |
| Integration       | `OrderReadyForShippingIntegrationEvent`, `OrderCancelledIntegrationEvent`, `ShipmentShippedIntegrationEvent`, `ShipmentDeliveredIntegrationEvent`, `ShipmentReturnedIntegrationEvent`, and Outbox |
| Authorization     | Customer, driver, packaging, and dispatcher permissions                                                                                                                                           |
| Persistence       | `shipping.Shipments`, `shipping.DeliveryAttempts`                                                                                                                                                 |
| Testing           | Domain, Application, Integration, and API tests                                                                                                                                                   |

---

## 28. Acceptance Criteria

Shipping is considered complete when:

1. The Shipment aggregate enforces the frozen lifecycle.
2. Invalid transitions are rejected.
3. Delivery attempts are recorded inside the Shipment.
4. The delivery address snapshot is immutable after shipment creation.
5. One active shipment per order is enforced by a filtered unique index.
6. Driver assignment is explicit and enforced.
7. The maximum-delivery-attempts policy is enforced by the aggregate using the supplied policy value.
8. Cancellation is enforced by the aggregate and triggered by an integration event or an authorized user.
9. Driver and customer authorization are enforced.
10. `OrderReadyForShippingIntegrationEvent` is handled idempotently.
11. `OrderCancelledIntegrationEvent` is handled idempotently.
12. `ShipmentShippedIntegrationEvent`, `ShipmentDeliveredIntegrationEvent`, and `ShipmentReturnedIntegrationEvent` are published through the Outbox.
13. Shipping fee calculation is owned by Shipping and consumed by Ordering.
14. Existing modular-monolith patterns are reused.
15. Unit and integration tests cover critical paths.
16. No external-carrier complexity is introduced.

---

## 29. Summary

Shipping is a complete backend internal-delivery bounded context. It includes shipment lifecycle management, cancellation, driver assignment, delivery attempts, retries, returns, distance-based shipping fee calculation, and strong authorization.

Shipping is client-agnostic: multiple client applications (customer, driver, packaging, dispatcher, and admin) integrate with its API. The UI/client applications are outside this bounded context.

Shipping remains simpler than Finance: there is no external provider, no webhooks, and no reconciliation. Its reliability comes from:

* Explicit domain invariants.
* Optimistic concurrency.
* Filtered unique constraints.
* Idempotent integration-event handling.
* Outbox-based cross-context events.
* Explicit maximum-delivery-attempts policy.
* Explicit cancellation policy.
* Distance-based fee calculation.

### Key Distinctions Preserved in This Revision

* There is no `Shipped` state in `ShipmentStatus`. The aggregate's precise internal state is `OutForDelivery`. The UI may present "Shipped" as a presentation concern.
* There is a `ShipmentShipped` event at both the Domain level (`ShipmentShippedDomainEvent`) and as a cross-context contract (`ShipmentShippedIntegrationEvent`). It captures the business fact that the shipment has entered the delivery process and is consumed by Ordering (Paid → Shipped) and Notifications (SMS).
* `ShipmentCancelledDomainEvent` is raised by the aggregate but has no consumer. Cancellation is either user-initiated, requiring no downstream reaction, or a response to `OrderCancelledIntegrationEvent`, where Ordering already knows about the cancellation.

---

**Document version:** 1.5 — Final

**Feature:** Shipping — Internal Delivery Model with Distance-Based Fee Calculation and Shipment Cancellation
