# Shipping Feature Integration Guide

**E-Commerce Modular Monolith**

**How Other Contexts and Clients Integrate with the Shipping Bounded Context**

**Version 1.1 – Final**

---

## 1. Purpose

This document explains how to integrate with the Shipping bounded context.

It covers:

* Requesting shipping fee calculations from Ordering.
* Consuming inbound Shipping events.
* Publishing outbound Shipping events.
* Using the Shipping REST APIs from different client types.
* Authorization and permission requirements.
* Integration rules and boundaries.

The guide is intended for developers working in other contexts (Ordering, Notifications) or building client applications that consume Shipping APIs.

---

## 2. Integration Overview

Shipping is a backend capability. It does not expose a UI. Other contexts and clients integrate in three ways:

1. **Shared Application Contract**

   `IShippingFeeCalculator` for synchronous fee calculation during checkout/order pricing.

2. **Outbox Integration Events**

   Inbound events from Ordering; outbound events to other contexts.

3. **REST APIs**

   Client-specific controllers for Customer, Driver, Dispatcher/Admin, and Packaging/Warehouse.

---

## 3. Shipping Fee Calculation Integration

### 3.1 When to Use

Ordering calls the Shipping fee calculator during order creation/checkout, after the customer selects a delivery address.

### 3.2 Shared Contract

```csharp
public interface IShippingFeeCalculator
{
    Task<ShippingFeeCalculationResult> CalculateAsync(
        ShippingFeeCalculationRequest request,
        CancellationToken ct = default);
}
```

### 3.3 Input

```csharp
public sealed record ShippingFeeCalculationRequest
{
    public string FullName { get; init; }
    public string PhoneNumber { get; init; }
    public string Street { get; init; }
    public string City { get; init; }
    public string LocationMapUrl { get; init; }
}
```

### 3.4 Output

```csharp
public sealed record ShippingFeeCalculationResult
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }
    public decimal DistanceKm { get; init; }
    public string CalculationBasis { get; init; }
}
```

### 3.5 Responsibility

* Shipping calculates the fee.
* Ordering stores `Order.ShippingFee` and includes it in `Order.Total`.
* Payment charges `Order.Total`.
* Shipment does not recalculate the original shipping fee.

### 3.6 Example Flow

```text
Ordering Checkout
    ↓
IShippingFeeCalculator.CalculateAsync(request)
    ↓
ShippingFeeCalculationResult
    ↓
Order.ShippingFee = result.Amount
    ↓
Order.Total = Order.Subtotal + Order.ShippingFee
    ↓
Payment charges Order.Total
```

---

## 4. Inbound Integration Events

Shipping consumes events published by Ordering.

### 4.1 OrderReadyForShippingIntegrationEvent

Published when an order becomes eligible for shipping.

**Properties:**

```text
OrderId

CustomerId

FullName

PhoneNumber

Street

City

LocationMapUrl
```

**Shipping handler behavior:**

* Checks if an active shipment already exists.
* Creates a new Shipment if none exists.
* Generates tracking number.
* Saves using UnitOfWork.

**Idempotency:**

Duplicate events do not create duplicate active shipments. Enforced by both an application-level check and a filtered unique index on `OrderId` for non-terminal statuses.

### 4.2 OrderCancelledIntegrationEvent

Published when an order is cancelled.

**Properties:**

```text
OrderId

CancelledAtUtc
```

**Shipping handler behavior:**

* Finds active shipment by `OrderId`.
* Attempts to cancel it if state is `Created`, `Assigned`, or `ReadyForPickup`.
* If shipment is in a later state (`PickedUp`, `OutForDelivery`, `ReturnToSender`, `Delivered`, `Returned`), cancellation is ignored.
* If no active shipment exists, no action.

**Idempotency:**

Duplicate events safe.

---

## 5. Outbound Integration Events

Shipping publishes events that other contexts may consume.

All outbound events are raised as domain events inside the Shipment aggregate, translated by an Application domain event handler, and written to the Outbox. The Outbox processor delivers them to consumers.

### 5.1 ShipmentShippedIntegrationEvent

**Properties:**

```text
ShipmentId

OrderId

ShippedAtUtc
```

**Raised when:** `Shipment.StartDelivery()` transitions the aggregate to `OutForDelivery`.

**Potential consumers:**

* Ordering — marks the order as Shipped (mapped from Paid → Shipped).
* Notifications — sends the shipped SMS to the customer.

**Note on naming:** The aggregate's state machine has no Shipped state — the precise internal states are `PickedUp` and `OutForDelivery`. The event is named after the business fact ("the shipment has entered the delivery process"), not after an aggregate state. Consumers should not assume a Shipped state exists inside Shipping.

### 5.2 ShipmentDeliveredIntegrationEvent

**Properties:**

```text
ShipmentId

OrderId

DeliveredAtUtc
```

**Potential consumers:**

* Ordering — marks order delivered/completed.
* Notifications — informs the customer; also used to schedule the feedback request.

### 5.3 ShipmentReturnedIntegrationEvent

**Properties:**

```text
ShipmentId

OrderId

ReturnedAtUtc
```

**Potential consumers:**

* Ordering — marks order returned.
* Notifications — informs the customer.

### 5.4 Event Publishing Mechanism

* Domain event raised inside Shipment.
* Application domain event handler translates to integration event.
* Integration event written to Outbox.
* Outbox processor delivers to consumers.

**Not published:** `ShipmentCreatedDomainEvent` and `ShipmentCancelledDomainEvent` are raised by the aggregate but have no domain event handler and no outbound integration event. No cross-context consumer reacts to shipment creation or shipment-level cancellation.

---

## 6. REST API Integration

Shipping exposes separate controllers by client type.

### 6.1 Customer API

**Base route:**

```text
/api/customer/shipments
```

| Method | Endpoint                                     | Purpose                    |
| ------ | -------------------------------------------- | -------------------------- |
| GET    | `/api/customer/shipments/{shipmentId}`       | View own shipment by ID    |
| GET    | `/api/customer/shipments/by-order/{orderId}` | View shipment for an order |
| GET    | `/api/customer/shipments`                    | List own shipments         |

**Authorization:**

* Role: `Customer`
* Permission: `Shipments.Read`
* Resource: only shipments where `CustomerId == current user id`.

### 6.2 Driver API

**Base route:**

```text
/api/driver/shipments
```

| Method | Endpoint                                              | Purpose                 |
| ------ | ----------------------------------------------------- | ----------------------- |
| GET    | `/api/driver/shipments`                               | List assigned shipments |
| POST   | `/api/driver/shipments/{shipmentId}/pickup`           | Mark picked up          |
| POST   | `/api/driver/shipments/{shipmentId}/start-delivery`   | Start delivery          |
| POST   | `/api/driver/shipments/{shipmentId}/delivery-attempt` | Record delivery attempt |
| POST   | `/api/driver/shipments/{shipmentId}/retry`            | Retry failed delivery   |
| POST   | `/api/driver/shipments/{shipmentId}/begin-return`     | Begin return flow       |

**Authorization:**

* Role: `Driver`
* Permission: `Shipments.Deliver`
* Resource: only shipments where `AssignedDriverId == current user id`.

### 6.3 Dispatcher/Admin API

**Base route:**

```text
/api/dispatcher/shipments
```

| Method | Endpoint                                                 | Purpose         |
| ------ | -------------------------------------------------------- | --------------- |
| GET    | `/api/dispatcher/shipments/{shipmentId}`                 | View shipment   |
| POST   | `/api/dispatcher/shipments/{shipmentId}/assign-driver`   | Assign driver   |
| POST   | `/api/dispatcher/shipments/{shipmentId}/reassign-driver` | Reassign driver |
| POST   | `/api/dispatcher/shipments/{shipmentId}/cancel`          | Cancel shipment |

**Authorization:**

* Role: `Dispatcher`, `Administrator`
* Permissions: `Shipments.Assign`, `Shipments.Manage`, `Shipments.Read`.

### 6.4 Packaging/Warehouse API

**Base route:**

```text
/api/packaging/shipments
```

| Method | Endpoint                                                 | Purpose               |
| ------ | -------------------------------------------------------- | --------------------- |
| POST   | `/api/packaging/shipments/{shipmentId}/ready-for-pickup` | Mark ready for pickup |
| POST   | `/api/packaging/shipments/{shipmentId}/complete-return`  | Complete return       |

**Authorization:**

* Role: `Packaging`
* Permission: `Shipments.Manage`.

---

## 7. Authorization Integration

### 7.1 Permission Constants

Use the shared `ShippingPermissions` class:

```csharp
ShippingPermissions.Read

ShippingPermissions.Assign

ShippingPermissions.Manage

ShippingPermissions.Deliver
```

### 7.2 Application Layer

Commands and queries are decorated with `[AuthorizePermission(...)]`.

### 7.3 API Layer

Controllers use:

* Role authorization at class level: `[Authorize(Roles = "...")]`
* Permission policies at action level: `[Authorize(Policy = "Permission:...")]`

### 7.4 Resource Authorization

Inside handlers:

* Customer can only access own shipments.
* Driver can only modify assigned shipments.
* Dispatcher/Admin can access according to permissions.

---

## 8. Integration Rules

* Do not modify Shipping tables directly from other contexts.
* Use Outbox events for cross-context state changes.
* Use `IShippingFeeCalculator` for fee calculation.
* Do not recalculate shipping fee inside Shipment.
* Do not depend on Shipping Domain types outside Shipping; use shared contracts.
* Ordering owns `Order.ShippingFee` and `Order.Total`.
* Payment charges `Order.Total`.
* Shipment creation is integration-event driven, not API-driven.

---

## 9. Common Integration Scenarios

### 9.1 Order Ready for Shipping

```text
Ordering
    ↓ publish OrderReadyForShippingIntegrationEvent
Outbox
    ↓
Shipping creates Shipment
```

### 9.2 Customer Views Shipment

```text
Customer App
    ↓ GET /api/customer/shipments/{id}
Shipping API
    ↓
Application query with authorization
    ↓
ShipmentDto
```

### 9.3 Driver Starts Delivery

```text
Driver App
    ↓ POST /api/driver/shipments/{id}/start-delivery
Shipping API
    ↓
Application command with authorization
    ↓
Shipment.StartDelivery()
    │
    ├── Status = OutForDelivery
    └── Raises ShipmentShippedDomainEvent
    ↓
ShipmentShippedIntegrationEvent
    ↓
Outbox
    ↓
Ordering (Paid → Shipped) / Notifications (OrderShippedSms)
```

### 9.4 Driver Records Delivery

```text
Driver App
    ↓ POST /api/driver/shipments/{id}/delivery-attempt
Shipping API
    ↓
Application command with authorization
    ↓
Shipment.RecordDeliveryAttempt(...)
```

### 9.5 Delivery Completed

```text
Shipment domain event
    ↓
Application handler
    ↓
ShipmentDeliveredIntegrationEvent
    ↓
Outbox
    ↓
Ordering / Notifications
```

### 9.6 Return Completed

```text
Shipment domain event
    ↓
Application handler
    ↓
ShipmentReturnedIntegrationEvent
    ↓
Outbox
    ↓
Ordering / Notifications
```

---

## 10. Summary

Integration with Shipping is done through three consistent channels:

* Shared application contract for fee calculation.
* Outbox integration events for asynchronous business facts.
* Client-specific REST APIs for operational workflows.

Shipping publishes three outbound integration events — `ShipmentShippedIntegrationEvent`, `ShipmentDeliveredIntegrationEvent`, `ShipmentReturnedIntegrationEvent` — each translated from a corresponding domain event. `ShipmentCreatedDomainEvent` and `ShipmentCancelledDomainEvent` are raised inside the aggregate but intentionally have no outbound counterpart.

By following these integration points, other contexts and client applications remain decoupled from Shipping internals, and the architecture stays clean, secure, and maintainable.

---

**Document version:** 1.1 – Final

**Feature:** Shipping Integration Guide
