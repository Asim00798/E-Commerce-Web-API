# Shipping Feature – Technical Reference Document

**E-Commerce Modular Monolith**

**Internal Delivery Model, Shipment Lifecycle, Driver Assignment, Shipping Fee Calculation (Distance-Based), Shipment Cancellation**

**Version 1.5 – Final**

---

## 1. Overview

The Shipping bounded context provides the complete backend capability required to manage the physical fulfillment and delivery of customer orders after an order becomes eligible for shipping.

The company operates its own delivery service. Drivers are internal employees. No external shipping/carrier API is used.

Shipping is client-agnostic. It exposes backend services and enforces authorization for multiple client types:

* Customer client
* Driver client
* Packaging/Warehouse client
* Dispatcher/Admin client

The client applications themselves are outside the Shipping bounded context.

**Shipping owns:**

* Shipment lifecycle
* Shipment cancellation
* Driver assignment and reassignment
* Delivery attempt recording
* Delivery success and failure
* Retry after failed delivery
* Return-to-sender flow
* Shipping fee calculation rules (distance-based)
* Delivery address snapshot
* Shipping authorization rules

**Shipping does not own:**

* Orders
* Payments
* Products
* Inventory
* Customer master data
* Employee identity
* Address master data

The finalized shipping fee belongs to Ordering, because the order must preserve the exact amount charged even if shipping rates change later.

---

## 2. Architecture and Placement

Shipping is a Domain bounded context located at:

```text
Domain/BoundedContexts/Core/Shipping
```

Application use cases live under:

```text
Application/BoundedContexts/Shipping
```

Cross-context shipping contracts live under:

```text
Application/Shared/Shipping
```

Infrastructure persistence and configuration live under:

```text
Infrastructure/Persistence/Modules/Shipping

Infrastructure/Shipping
```

API controllers and DTOs live under:

```text
API/Controllers/Shipping

API/DTOs/Shipping
```

### 2.1 Layer Responsibilities

| Layer          | Responsibility                                                                                                                                                                                                      |
| -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Domain         | Shipment aggregate, DeliveryAttempt entity, DeliveryAddressSnapshot value object, ShippingDistance value object, ShippingFeeResult value object, LocalShippingFeePolicy, enums, domain events, repository interface |
| Application    | Commands, queries, handlers, shared cross-context fee contract (`IShippingFeeCalculator`), integration events, DTOs, internal `ILocationService`, authorization orchestration                                       |
| Infrastructure | EF Core configuration, repository implementation, MapLocationService, ShippingFeeCalculatorService, shipping extension registration                                                                                 |
| API            | Thin client-specific controllers, request/response DTOs, role and permission policies                                                                                                                               |

---

## 3. Shipping Fee Calculation (Distance-Based)

### 3.1 Responsibility Split

* Shipping owns shipping-fee calculation rules and exposes a shared cross-context `IShippingFeeCalculator` abstraction.
* Ordering requests the calculation during checkout/order pricing.
* Ordering stores the finalized `Order.ShippingFee` and includes it in `Order.Total`.
* Payment charges the finalized `Order.Total`; it does not calculate shipping fees.
* Shipment handles physical fulfillment and delivery; it does not recalculate the original order's shipping fee.

The fee result returned by Shipping is transient. Ordering persists the accepted monetary value as part of the order's financial snapshot. Changing future shipping rules never affects historical orders.

### 3.2 Flow

```text
Customer selects delivery address
        ↓
Ordering calls IShippingFeeCalculator (Shared)
        ↓
ShippingFeeCalculationResult returned (Shared)
        ↓
Ordering stores Order.ShippingFee
        ↓
Ordering includes in Order.Total
        ↓
Payment charges Order.Total
        ↓
OrderReadyForShippingIntegrationEvent
        ↓
Shipping creates Shipment
```

### 3.3 Distance-Based Pricing

The system uses the company's local fulfillment location and the customer's delivery map URL to compute a straight-line distance, then applies local distance bands.

```text
Delivery Address (including LocationMapUrl)
        ↓
ILocationService resolves ShippingDistance
        ↓
LocalShippingFeePolicy applies fee bands
        ↓
ShippingFeeResult
```

No country, international region, carrier, or external geographic service abstraction is necessary.

**Initial fee bands:**

```text
0–5 km   → 10 AED

5–10 km  → 15 AED

10–20 km → 25 AED

>20 km   → outside service area
```

The policy is an extension point for future rules such as:

* Free-shipping thresholds
* Delivery methods
* Weight-based pricing
* Different local regions

### 3.4 Domain Policy

`LocalShippingFeePolicy` is a concrete Domain class that encapsulates the fee rules. It does not depend on infrastructure.

```csharp
public sealed class LocalShippingFeePolicy
{
    public ShippingFeeResult CalculateFee(ShippingDistance distance)
}
```

### 3.5 Shared Application Contract

`IShippingFeeCalculator` is a shared cross-context contract used by Ordering.

```csharp
public interface IShippingFeeCalculator
{
    Task<ShippingFeeCalculationResult> CalculateAsync(
        ShippingFeeCalculationRequest request,
        CancellationToken ct = default);
}
```

The implementation lives in Infrastructure (`ShippingFeeCalculatorService`) and orchestrates:

1. Map shared request → `DeliveryAddressSnapshot`
2. Call internal `ILocationService` → `ShippingDistance`
3. Call `LocalShippingFeePolicy` → `ShippingFeeResult`
4. Map Domain result → shared `ShippingFeeCalculationResult`

---

## 4. Domain Model

### 4.1 Shipment Aggregate

Shipment is the aggregate root and owns the complete delivery lifecycle.

#### 4.1.1 Properties

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

#### 4.1.2 Aggregate Behaviors

| Method                                                | Allowed From                                       | Result                                                                                                                     |
| ----------------------------------------------------- | -------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| `AssignDriver(driverId)`                              | Created, Assigned                                  | Sets AssignedDriverId and AssignedAtUtc; transitions to Assigned if from Created                                           |
| `ReassignDriver(newDriverId)`                         | Assigned, ReadyForPickup, PickedUp, OutForDelivery | Replaces AssignedDriverId, updates AssignedAtUtc                                                                           |
| `MarkReadyForPickup()`                                | Assigned                                           | Transitions to ReadyForPickup                                                                                              |
| `MarkPickedUp()`                                      | ReadyForPickup                                     | Transitions to PickedUp                                                                                                    |
| `StartDelivery()`                                     | PickedUp                                           | Transitions to OutForDelivery; raises ShipmentShippedDomainEvent                                                           |
| `RecordDeliveryAttempt(result, failureReason, notes)` | OutForDelivery                                     | Adds DeliveryAttempt; if Delivered, transitions to Delivered; if failed, remains OutForDelivery                            |
| `Retry(maximumDeliveryAttempts)`                      | OutForDelivery with previous failed attempt        | Checks attempt count against maximumDeliveryAttempts; if allowed, remains OutForDelivery; if exceeded, calls BeginReturn() |
| `BeginReturn()`                                       | OutForDelivery with failed attempts                | Transitions to ReturnToSender                                                                                              |
| `CompleteReturn()`                                    | ReturnToSender                                     | Transitions to Returned                                                                                                    |
| `Cancel()`                                            | Created, Assigned, ReadyForPickup                  | Transitions to Cancelled and sets CancelledAtUtc                                                                           |

All state mutations are explicit methods. No public setters for stateful properties.

#### 4.1.3 Shipment Status

```csharp
public enum ShipmentStatus
{
    Created = 1,
    Assigned = 2,
    ReadyForPickup = 3,
    PickedUp = 4,
    OutForDelivery = 5,
    ReturnToSender = 6,
    Delivered = 7,
    Returned = 8,
    Cancelled = 9
}
```

**Terminal states:**

```text
Delivered

Returned

Cancelled
```

No transition out of terminal states.

**Important clarification:** Do not add a Shipped status. PickedUp and OutForDelivery are the precise backend states. UI may display "Shipped" as a presentation concern, but it has no domain meaning.

The absence of a Shipped state does not mean the absence of a Shipped event. `ShipmentShippedDomainEvent` fires when the shipment transitions to `OutForDelivery`, and is translated into `ShipmentShippedIntegrationEvent` for cross-context consumers (Ordering, Notifications). The event name describes the business fact — the shipment has entered the delivery process — while the aggregate's state remains `OutForDelivery`, which is the precise internal representation.

### 4.2 DeliveryAttempt Entity

`DeliveryAttempt` is an entity inside the Shipment aggregate. It is not an aggregate root and must be manipulated only through Shipment.

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

#### 4.2.1 DeliveryAttemptResult Enum

```csharp
public enum DeliveryAttemptResult
{
    Delivered = 1,
    CustomerUnavailable = 2,
    WrongAddress = 3,
    CustomerRefused = 4,
    Other = 5
}
```

### 4.3 DeliveryAddressSnapshot Value Object

Shipping stores a snapshot of the local delivery address when the shipment is created.

```text
DeliveryAddressSnapshot
├── FullName
├── PhoneNumber
├── Street
├── City
└── LocationMapUrl
```

This value object is immutable after shipment creation. It contains only the information needed for local delivery.

### 4.4 ShippingDistance Value Object

```csharp
public sealed record ShippingDistance(decimal Kilometers);
```

Represents a delivery distance in kilometers. Used as input for the fee policy.

### 4.5 ShippingFeeResult Value Object

Represents the calculated shipping fee.

```csharp
public sealed record ShippingFeeResult(
    decimal Amount,
    string Currency,
    ShippingDistance Distance,
    string CalculationBasis);
```

This result is transient. Ordering persists the accepted monetary value.

---

## 5. Application Layer

### 5.1 Abstractions

**Shared cross-context:**

* `IShippingFeeCalculator` — used by Ordering; implementation in Infrastructure.

**Internal Shipping:**

* `ILocationService` — resolves `ShippingDistance` from a `DeliveryAddressSnapshot`.

### 5.2 Commands and Handlers

| Command                        | Purpose                                           | Permission Attribute                                 |
| ------------------------------ | ------------------------------------------------- | ---------------------------------------------------- |
| `AssignDriverCommand`          | Assigns a driver to a shipment                    | `[AuthorizePermission(ShippingPermissions.Assign)]`  |
| `ReassignDriverCommand`        | Reassigns a shipment to a different driver        | `[AuthorizePermission(ShippingPermissions.Assign)]`  |
| `MarkReadyForPickupCommand`    | Marks shipment ready for pickup                   | `[AuthorizePermission(ShippingPermissions.Manage)]`  |
| `MarkPickedUpCommand`          | Marks shipment as picked up                       | `[AuthorizePermission(ShippingPermissions.Deliver)]` |
| `StartDeliveryCommand`         | Starts delivery and transitions to OutForDelivery | `[AuthorizePermission(ShippingPermissions.Deliver)]` |
| `RecordDeliveryAttemptCommand` | Records a delivery attempt                        | `[AuthorizePermission(ShippingPermissions.Deliver)]` |
| `RetryDeliveryCommand`         | Retries a failed delivery, if allowed by policy   | `[AuthorizePermission(ShippingPermissions.Deliver)]` |
| `BeginReturnCommand`           | Begins return-to-sender flow                      | `[AuthorizePermission(ShippingPermissions.Deliver)]` |
| `CompleteReturnCommand`        | Completes return flow and transitions to Returned | `[AuthorizePermission(ShippingPermissions.Manage)]`  |
| `CancelShipmentCommand`        | Cancels a shipment in an allowed state            | `[AuthorizePermission(ShippingPermissions.Manage)]`  |

**Important:** There is no `CreateShipmentCommand`. Shipment creation is handled exclusively by the inbound integration event handler, which directly uses repositories and `Shipment.Create`.

### 5.3 Queries

| Query                       | Purpose                                | Permission Attribute                                 |
| --------------------------- | -------------------------------------- | ---------------------------------------------------- |
| `GetShipmentByIdQuery`      | Returns shipment by ID                 | `[AuthorizePermission(ShippingPermissions.Read)]`    |
| `GetShipmentByOrderIdQuery` | Returns shipment for an order          | `[AuthorizePermission(ShippingPermissions.Read)]`    |
| `GetCustomerShipmentsQuery` | Returns shipments for a customer       | `[AuthorizePermission(ShippingPermissions.Read)]`    |
| `GetDriverShipmentsQuery`   | Returns shipments assigned to a driver | `[AuthorizePermission(ShippingPermissions.Deliver)]` |

Queries also include resource authorization in the handler, ensuring customers can access only their own shipments and drivers only assigned shipments.

### 5.4 Domain Event Handlers

| Handler                               | Translates To                       | Outbox Destination |
| ------------------------------------- | ----------------------------------- | ------------------ |
| `ShipmentShippedDomainEventHandler`   | `ShipmentShippedIntegrationEvent`   | Outbox             |
| `ShipmentDeliveredDomainEventHandler` | `ShipmentDeliveredIntegrationEvent` | Outbox             |
| `ShipmentReturnedDomainEventHandler`  | `ShipmentReturnedIntegrationEvent`  | Outbox             |

`ShipmentCreatedDomainEvent` and `ShipmentCancelledDomainEvent` are raised by the aggregate but have no handlers — no cross-context consumer needs to react to them. In particular, `ShipmentCancelledDomainEvent` intentionally produces no outbound integration event; cancellation is either user-initiated (no downstream reaction) or a response to `OrderCancelledIntegrationEvent` (Ordering already knows).

### 5.5 Integration Events

**Inbound:**

`OrderReadyForShippingIntegrationEvent`

```text
OrderId

CustomerId

FullName

PhoneNumber

Street

City

LocationMapUrl
```

`OrderCancelledIntegrationEvent`

```text
OrderId

CancelledAtUtc
```

**Outbound:**

`ShipmentShippedIntegrationEvent`

```text
ShipmentId

OrderId

ShippedAtUtc
```

`ShipmentDeliveredIntegrationEvent`

```text
ShipmentId

OrderId

DeliveredAtUtc
```

`ShipmentReturnedIntegrationEvent`

```text
ShipmentId

OrderId

ReturnedAtUtc
```

### 5.6 Integration Event Handlers

* `OrderReadyForShippingIntegrationEventHandler` — creates Shipment directly using `IShipmentRepository` and `IUnitOfWork`. Enforces idempotency via `GetActiveByOrderIdAsync` and the filtered unique index.
* `OrderCancelledIntegrationEventHandler` — attempts to cancel the active shipment, if it is in a cancellable state; otherwise completes without state change.

No `ISender` is used inside integration event handlers. They are executed by the Outbox processor and contain the actual side-effect logic.

---

## 6. Infrastructure Layer

### 6.1 Persistence

EF Core configurations:

* `ShipmentConfiguration`
* `DeliveryAttemptConfiguration`

**Tables:**

```text
shipping.Shipments

shipping.DeliveryAttempts
```

`DeliveryAttempts` is part of the Shipment aggregate. The specific EF Core mapping (owned entity vs. dependent entity) is an infrastructure decision. The domain rule is that DeliveryAttempt cannot be modified as an aggregate root.

### 6.2 Filtered Unique Index

A filtered unique index on `Shipments.OrderId` ensures:

```text
One active shipment per order
```

**Active statuses included:**

```text
Created, Assigned, ReadyForPickup, PickedUp, OutForDelivery, ReturnToSender
```

**Terminal statuses excluded:**

```text
Delivered, Returned, Cancelled
```

This allows historical terminal shipments while preventing duplicate active ones.

### 6.3 Repository

`ShipmentRepository` implements `IShipmentRepository`.

**Key methods:**

* `GetByIdAsync`
* `GetByOrderIdAsync`
* `GetActiveByOrderIdAsync`
* `GetCustomerShipmentsAsync`
* `GetDriverShipmentsAsync`

### 6.4 Location Service

`MapLocationService` implements `ILocationService`.

It parses coordinates from `LocationMapUrl` and calculates the Haversine distance between the company's configured location and the delivery address.

`LocationOptions` holds the company's latitude and longitude, configured via `Shipping:Location`.

### 6.5 Shipping Fee Calculator Service

`ShippingFeeCalculatorService` implements the shared `IShippingFeeCalculator`. It orchestrates location resolution and domain fee policy application.

### 6.6 Extensions

`ShippingInfrastructureExtensions` registers:

* `ILocationService` → `MapLocationService`
* `IShippingFeeCalculator` → `ShippingFeeCalculatorService`
* `LocalShippingFeePolicy` → domain policy class
* `LocationOptions` from configuration

`ShipmentRepository` is discovered automatically by the shared `AddRepositories` Scrutor scan on the Infrastructure assembly — no manual registration is required.

---

## 7. API Layer

### 7.1 Controllers

Shipping uses multiple controllers, grouped by client responsibility.

| Controller                      | Route Prefix                | Default Role              |
| ------------------------------- | --------------------------- | ------------------------- |
| `CustomerShipmentsController`   | `/api/customer/shipments`   | Customer                  |
| `DriverShipmentsController`     | `/api/driver/shipments`     | Driver                    |
| `DispatcherShipmentsController` | `/api/dispatcher/shipments` | Dispatcher, Administrator |
| `PackagingShipmentsController`  | `/api/packaging/shipments`  | Packaging                 |

Each controller uses permission policies per action, in addition to the role-level authorization.

### 7.2 Endpoints

```text
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

### 7.3 DTOs

**Requests:**

* `AssignDriverRequest`
* `ReassignDriverRequest`
* `RecordDeliveryAttemptRequest`
* `BeginReturnRequest`

**Responses:**

* `ShipmentResponse`
* `DeliveryAttemptResponse`

---

## 8. Authorization

### 8.1 Permissions

```text
Shipments.Read

Shipments.Assign

Shipments.Manage

Shipments.Deliver
```

Shipment creation is a system/application operation triggered by integration events. It is not assigned as a user permission.

Cancellation is protected by `Shipments.Manage` or handled via integration event from Ordering.

### 8.2 Permission vs Resource Authorization

Permission authorization determines whether a user may perform a category of operation. Resource authorization determines whether the user may operate on a specific shipment.

**Examples:**

* A driver with `Shipments.Deliver` may record delivery attempts. However, they can only do so on shipments where `AssignedDriverId` matches the current driver.
* A customer with `Shipments.Read` can only view shipments where `CustomerId` matches the current user.

Authorization is enforced in the Application layer through `[AuthorizePermission]` attributes on commands/queries and resource checks inside handlers. API controllers add defense-in-depth via role and permission policies.

### 8.3 Typical Actor / Operation Mapping

| Operation               | Typical actor                                                     |
| ----------------------- | ----------------------------------------------------------------- |
| Create shipment         | System / Ordering integration                                     |
| Cancel shipment         | System / Ordering integration or Dispatcher/Admin                 |
| Assign driver           | Dispatcher / Admin                                                |
| Reassign driver         | Dispatcher / Admin                                                |
| Mark ready for pickup   | Packaging/Warehouse                                               |
| Mark picked up          | Driver (or Packaging/Warehouse, depending on workflow)            |
| Start delivery          | Driver                                                            |
| Record delivery attempt | Driver                                                            |
| Retry delivery          | Driver/Dispatcher according to policy                             |
| Begin return            | Driver/Dispatcher                                                 |
| Complete return         | Packaging/Warehouse                                               |
| View shipment           | Customer / Driver / Dispatcher / Admin according to authorization |

The exact actor can remain configurable through permissions rather than being hard-coded into the domain.

---

## 9. Folder Structure

```text
Domain/
└── BoundedContexts/
    └── Core/
        └── Shipping/
            ├── AggregateRoots/
            │   └── Shipment/
            │       ├── Behaviors/
            │       │   ├── Shipment.cs
            │       │   ├── Shipment.State.cs
            │       │   ├── Shipment.Delivery.cs
            │       │   └── Shipment.Validation.cs
            │       ├── Entities/
            │       │   └── DeliveryAttempt.cs
            │       ├── Events/
            │       │   ├── ShipmentCreatedDomainEvent.cs
            │       │   ├── ShipmentShippedDomainEvent.cs
            │       │   ├── ShipmentDeliveredDomainEvent.cs
            │       │   ├── ShipmentReturnedDomainEvent.cs
            │       │   └── ShipmentCancelledDomainEvent.cs
            │       └── Exceptions/
            │           └── ShipmentException.cs
            ├── ValueObjects/
            │   ├── DeliveryAddressSnapshot.cs
            │   ├── ShippingDistance.cs
            │   └── ShippingFeeResult.cs
            ├── Enums/
            │   ├── ShipmentStatus.cs
            │   └── DeliveryAttemptResult.cs
            ├── Policies/
            │   └── LocalShippingFeePolicy.cs
            └── Repositories/
                └── IShipmentRepository.cs

Application/
├── Shared/
│   ├── Security/
│   │   └── Authorization/
│   │       └── Permissions/
│   │           └── ShippingPermissions.cs
│   └── Shipping/
│       ├── Models/
│       │   ├── ShippingFeeCalculationRequest.cs
│       │   └── ShippingFeeCalculationResult.cs
│       └── Services/
│           └── IShippingFeeCalculator.cs
└── BoundedContexts/
    └── Shipping/
        ├── Abstractions/
        │   └── ILocationService.cs
        ├── Commands/
        │   ├── AssignDriver/
        │   ├── ReassignDriver/
        │   ├── MarkReadyForPickup/
        │   ├── MarkPickedUp/
        │   ├── StartDelivery/
        │   ├── RecordDeliveryAttempt/
        │   ├── RetryDelivery/
        │   ├── BeginReturn/
        │   ├── CompleteReturn/
        │   └── CancelShipment/
        ├── Queries/
        │   ├── GetShipmentById/
        │   ├── GetShipmentByOrderId/
        │   ├── GetCustomerShipments/
        │   └── GetDriverShipments/
        ├── DomainEventHandlers/
        │   ├── ShipmentShippedDomainEventHandler.cs
        │   ├── ShipmentDeliveredDomainEventHandler.cs
        │   └── ShipmentReturnedDomainEventHandler.cs
        ├── IntegrationEvents/
        │   ├── Inbound/
        │   │   ├── OrderReadyForShippingIntegrationEvent.cs
        │   │   └── OrderCancelledIntegrationEvent.cs
        │   └── Outbound/
        │       ├── ShipmentShippedIntegrationEvent.cs
        │       ├── ShipmentDeliveredIntegrationEvent.cs
        │       └── ShipmentReturnedIntegrationEvent.cs
        ├── IntegrationEventHandlers/
        │   ├── OrderReadyForShippingIntegrationEventHandler.cs
        │   └── OrderCancelledIntegrationEventHandler.cs
        ├── Models/
        │   └── ShippingOptions.cs
        └── Dtos/
            ├── ShipmentDto.cs
            └── DeliveryAttemptDto.cs

Infrastructure/
├── Persistence/
│   ├── Context/
│   │   └── AppDbContext.Shipping.cs
│   └── Modules/
│       └── Shipping/
│           ├── Configurations/
│           │   ├── ShipmentConfiguration.cs
│           │   └── DeliveryAttemptConfiguration.cs
│           └── Repositories/
│               └── ShipmentRepository.cs
└── Shipping/
    ├── Extensions/
    │   └── ShippingInfrastructureExtensions.cs
    ├── Location/
    │   ├── MapLocationService.cs
    │   └── LocationOptions.cs
    └── Services/
        └── ShippingFeeCalculatorService.cs

API/
├── Controllers/
│   └── Shipping/
│       ├── CustomerShipmentsController.cs
│       ├── DriverShipmentsController.cs
│       ├── DispatcherShipmentsController.cs
│       └── PackagingShipmentsController.cs
└── DTOs/
    └── Shipping/
        ├── Requests/
        │   ├── AssignDriverRequest.cs
        │   ├── ReassignDriverRequest.cs
        │   ├── RecordDeliveryAttemptRequest.cs
        │   └── BeginReturnRequest.cs
        └── Responses/
            ├── ShipmentResponse.cs
            └── DeliveryAttemptResponse.cs
```

---

## 10. Workflows

### 10.1 Shipping Fee Calculation

```text
Ordering
  ↓
IShippingFeeCalculator (Shared)
  ↓
ShippingFeeCalculatorService (Infrastructure)
  ├── ILocationService → MapLocationService → ShippingDistance
  └── LocalShippingFeePolicy → ShippingFeeResult
       ↓
Map to ShippingFeeCalculationResult (Shared)
  ↓
Return to Ordering
```

### 10.2 Shipment Creation

```text
Ordering
  ↓
OrderReadyForShippingIntegrationEvent
  ↓
Outbox
  ↓
OrderReadyForShippingIntegrationEventHandler
  ├── Check existing active shipment
  ├── Build DeliveryAddressSnapshot
  ├── Generate TrackingNumber
  └── Shipment.Create() → repository → SaveChanges
```

No command is used.

### 10.3 Shipment Cancellation

```text
Ordering
  ↓
OrderCancelledIntegrationEvent
  ↓
Outbox
  ↓
OrderCancelledIntegrationEventHandler
  └── If active shipment and cancellable → Shipment.Cancel() → save
```

Cancellation is allowed only from:

```text
Created

Assigned

ReadyForPickup
```

It is not allowed from:

```text
PickedUp

OutForDelivery

ReturnToSender

Delivered

Returned
```

If the shipment is already terminal or in a non-cancellable state, the handler completes without state change.

### 10.4 Delivery Lifecycle

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
  ├── RecordDeliveryAttempt(Delivered) → Delivered
  └── RecordDeliveryAttempt(Failed) → remains OutForDelivery
       ↓ RetryDelivery if allowed
       OutForDelivery
       ↓ if max attempts exceeded
       BeginReturn → ReturnToSender
       ↓ CompleteReturn
       Returned
```

### 10.5 Delivery Success

```text
OutForDelivery
  ↓ RecordDeliveryAttempt(Delivered)
Delivered
  ↓ ShipmentDeliveredDomainEvent
  ↓ ShipmentDeliveredIntegrationEvent
  ↓ Outbox
  ↓ Orders / Notifications
```

### 10.6 Return

```text
OutForDelivery
  ↓ max attempts exceeded
BeginReturn
  ↓ ReturnToSender
CompleteReturn
  ↓ Returned
  ↓ ShipmentReturnedDomainEvent
  ↓ ShipmentReturnedIntegrationEvent
  ↓ Outbox
```

### 10.7 Shipped Notification

```text
PickedUp
  ↓ StartDelivery
OutForDelivery
  ↓ ShipmentShippedDomainEvent (raises on transition)
  ↓ ShipmentShippedIntegrationEvent
  ↓ Outbox
  ↓ Orders (Paid → Shipped) / Notifications (SMS)
```

The event fires when the shipment enters the delivery process. Ordering maps this to its own Shipped state; the aggregate's own state remains OutForDelivery. This is a deliberate semantic mapping between two bounded contexts, not a shared enum.

---

## 11. Concurrency and Idempotency

* RowVersion optimistic concurrency on Shipment.
* Filtered unique index on OrderId prevents duplicate active shipments.
* Inbound `OrderReadyForShippingIntegrationEvent` handled idempotently through existing idempotency decorator and active shipment check.
* Inbound `OrderCancelledIntegrationEvent` handled idempotently; cancelling an already cancelled shipment returns success.
* Duplicate integration events are safe.
* No provider reconciliation is required.

---

## 12. Testing

### 12.1 Domain Tests

* Valid/invalid state transitions
* Assign/Reassign rules
* Delivery attempt recording
* Max attempts enforcement
* Return lifecycle
* Duplicate delivery prevention
* Address snapshot immutability
* Cancellation rules
* Shipping fee policy for each distance band

### 12.2 Application Tests

* Idempotent shipment creation (integration event handler)
* Idempotent shipment cancellation
* Non-cancellable shipment handling
* Authorization isolation (permission + resource)
* Integration event mapping (including ShipmentShippedIntegrationEvent)
* Retry/return orchestration
* Fee calculation orchestration

### 12.3 Integration Tests

* EF mappings
* Filtered unique index
* RowVersion concurrency
* Outbox persistence
* Idempotent event consumption

### 12.4 API Tests

* Permission policies
* Role-based controller authorization
* Customer isolation
* Driver isolation
* State transition validation
* Cancellation endpoint behavior

---

## 13. Summary

Shipping is a complete backend internal-delivery capability. It includes shipment lifecycle management, cancellation, driver assignment, delivery attempts, retries, returns, distance-based shipping fee calculation, and strong authorization.

Shipping is client-agnostic: multiple client applications (customer, driver, packaging, dispatcher, admin) integrate with its API. The UI/client applications are outside this bounded context.

Shipping remains simpler than Finance: no external provider, no webhooks, no reconciliation. Its reliability comes from explicit domain invariants, optimistic concurrency, filtered unique constraints, idempotent event handling, and Outbox integration.

**Design properties preserved in this revision:**

* The absence of a Shipped state in ShipmentStatus reflects the precise internal representation (OutForDelivery); the ShipmentShipped event is the business fact consumed by Ordering and Notifications. The two live at different layers.
* `ShipmentShippedIntegrationEvent` is a first-class outbound contract, consumed by Ordering (Paid → Shipped) and Notifications (SMS). It fires on `StartDelivery()`.
* `ShipmentCancelledDomainEvent` is raised by the aggregate but has no handler and no outbound integration event — no consumer reacts to a cancellation fact.
* `ShipmentRepository` is auto-discovered by the shared Scrutor scan; `ShippingInfrastructureExtensions` registers only the services it owns.

---

**Document version:** 1.5 – Final

**Feature:** Shipping / Internal Delivery Model with Distance-Based Fee Calculation and Shipment Cancellation
