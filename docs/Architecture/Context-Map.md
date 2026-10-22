# Context Map Technical Document

**Project:** E-Commerce Modular Monolith

**Scope:** Bounded Contexts and Integration Relationships

**Version:** 1.0 – Final

---

## 1. Purpose

This document provides a high-level context map of the e-commerce modular monolith. It identifies the bounded contexts, their responsibilities, and how they communicate with each other. The map serves as a shared architectural reference for teams working on different parts of the system.

The architecture follows Domain-Driven Design (DDD) principles with a modular monolith implementation. Bounded contexts are separated logically but may share the same process and database transaction where explicitly chosen. Communication between contexts is either **synchronous through shared application contracts** or **asynchronous through integration events via the Outbox**.

---

## 2. Bounded Contexts Overview

| Bounded Context / Capability | Responsibility                                                                    | Owns                                                     |
| ---------------------------- | --------------------------------------------------------------------------------- | -------------------------------------------------------- |
| **Catalog**                  | Product master data, stock authority, brands, categories, variants, and images.   | `Product`, `Brand`, `Category`, `ProductVariant`, stock  |
| **Ordering**                 | Customer purchase intent, cart, order lifecycle, totals, and cancellation.        | `Cart`, `Order`, `OrderItem`                             |
| **Finance**                  | Payments, refunds, and provider integration (Paymob).                             | `Payment`, `Refund`                                      |
| **Shipping**                 | Physical fulfillment, shipment lifecycle, delivery attempts, and fee calculation. | `Shipment`, `DeliveryAttempt`, `DeliveryAddressSnapshot` |
| **CustomerEngagement**       | Product ratings and wishlist.                                                     | `Rating`, `Wishlist`, `WishlistItem`                     |
| **Identity**                 | User authentication, account management, credentials, roles, and permissions.     | Identity users, roles, permissions                       |
| **People**                   | Customer personal data, addresses, and contact information.                       | `Person` aggregate                                       |
| **Notifications**            | Reusable notification infrastructure (email, SMS, push, and real-time).           | Channels, templates, user notification history           |
| **Scheduling**               | Background job management (Hangfire) and job contracts.                           | `IJob`, `IJobHandler`, triggers                          |
| **File Storage**             | Generic file storage capability and provider abstraction.                         | `StoredFile`, `IFileService`                             |

**Note:** Registration/onboarding is a workflow that spans Identity and People, not a standalone bounded context.

---

## 3. Context Relationships and Integration Patterns

### 3.1 Synchronous Shared Contracts

These are direct application-level contracts consumed across contexts, usually within the same database transaction in the modular monolith.

| Consumer Context | Shared Contract                                | Owner Context | Purpose                                                                                                             |
| ---------------- | ---------------------------------------------- | ------------- | ------------------------------------------------------------------------------------------------------------------- |
| Ordering         | `IStockService`                                | Catalog       | Decrease or increase stock during checkout and cancellation.                                                        |
| Ordering         | `IShippingFeeCalculator`                       | Shipping      | Calculate the shipping fee during checkout.                                                                         |
| Ordering         | `IPersonReader`                                | People        | Read customer contact and delivery address data through an application-level contract, not a repository dependency. |
| Any context      | `IFileService`, `IFileContentInspector`        | File Storage  | Upload and validate files.                                                                                          |
| Any context      | `IEmailChannel`, `ISmsChannel`, `IPushChannel` | Notifications | Send external notifications through typed models.                                                                   |
| Any context      | `IJobScheduler`                                | Scheduling    | Enqueue background jobs.                                                                                            |

**Key Principle:** The owner context implements the contract; consumers depend only on the abstraction. This maintains boundary integrity while allowing synchronous collaboration when immediate consistency is required.

The contract is not the owner’s repository, but a consumer-oriented service abstraction—for example, `IPersonReader` instead of `IPersonRepository`.

### 3.2 Asynchronous Integration Events (Outbox)

Integration events are owned by the publishing context and consumed by subscribing contexts asynchronously. All events are persisted to the Outbox atomically with the publisher’s business state, then delivered later.

| Event                                   | Publisher (Owner) | Subscriber(s)           | Purpose                                                                                         |
| --------------------------------------- | ----------------- | ----------------------- | ----------------------------------------------------------------------------------------------- |
| `OrderPlacedIntegrationEvent`           | Ordering          | Finance                 | Trigger payment initiation.                                                                     |
| `OrderReadyForShippingIntegrationEvent` | Ordering          | Shipping                | Create a shipment after payment success and the order’s transition to the shipping-ready state. |
| `OrderCancelledIntegrationEvent`        | Ordering          | Shipping, Notifications | Notify that the order was cancelled.                                                            |
| `PaymentCompletedIntegrationEvent`      | Finance           | Ordering                | Mark the order as Paid; triggers `OrderReadyForShippingIntegrationEvent`.                       |
| `PaymentFailedIntegrationEvent`         | Finance           | Ordering                | Mark the order as PaymentFailed.                                                                |
| `RefundCompletedIntegrationEvent`       | Finance           | Ordering                | Mark the order as Refunded.                                                                     |
| `RefundFailedIntegrationEvent`          | Finance           | Ordering                | No order refund transition; Finance remains authoritative for refund failure.                   |
| `ShipmentShippedIntegrationEvent`       | Shipping          | Ordering                | Mark the order as Shipped.                                                                      |
| `ShipmentDeliveredIntegrationEvent`     | Shipping          | Ordering, Notifications | Mark the order as Delivered.                                                                    |
| `ShipmentReturnedIntegrationEvent`      | Shipping          | Ordering, Notifications | Mark the order as Returned.                                                                     |

#### Semantic Clarifications

* **`OrderReadyForShippingIntegrationEvent`** is published after Ordering receives and successfully processes the payment-completed event and transitions the order into its shipping-ready state. It is not published at order placement.
* **`ShipmentShippedIntegrationEvent`** corresponds to the shipping business state where the shipment has left the fulfillment/shipping process. In the current Shipping state machine, this is represented by `OutForDelivery`. Do not assume `Shipped` equals `OutForDelivery` unless explicitly modeled that way; the mapping is defined by Shipping’s lifecycle.
* **`RefundFailedIntegrationEvent`** informs Ordering that the refund attempt failed; Ordering does not change its own state. Finance owns the refund lifecycle and failure details.

### 3.3 Self-Contained Contexts

* **CustomerEngagement** currently has no integration with other contexts. Ratings and wishlist operations are local; no events are published or consumed.
* **People** provides data through application-level contracts such as `IPersonReader` to other contexts when required. Historical data needed by Ordering or Shipping is captured as snapshots within those contexts rather than relying on live People queries.

---

## 4. Context Map Diagram

### 4.1 Primary Context Relationships

```text
                         ┌──────────────┐
                         │    Catalog   │
                         └──────▲───────┘
                                │
                          IStockService
                                │
                         ┌──────┴───────┐
                         │   Ordering   │
                         └──┬───────┬───┘
                            │       │
                   OrderPlaced     OrderReadyForShipping
                            │       │
                            ▼       ▼
                      ┌─────────┐ ┌──────────┐
                      │ Finance │ │ Shipping │
                      └────▲────┘ └────▲─────┘
                           │            │
                PaymentCompleted       │
                PaymentFailed          │
                RefundCompleted        │
                RefundFailed            │
                           │            │
                           └─────► Ordering
```

### 4.2 Additional Relationships

```text
Ordering ── IPersonReader ──► People

CustomerEngagement ── standalone

Ordering / Catalog / People / Shipping / Finance
         │
         ├── Notifications
         ├── Scheduling
         └── File Storage
```

**Diagram note:** The diagrams are conceptual. The tables in Section 3 define the authoritative publishers, subscribers, contract ownership, and communication semantics.

---

## 5. Shared Kernel and Shared Technical Abstractions

### Shared Kernel

Only genuinely shared domain concepts are considered Shared Kernel candidates, such as common value objects where identical semantics and invariants are required across contexts.

Examples include `Money`, `PhoneNumber`, and `Email`.

### Shared Technical Abstractions

Common technical abstractions such as `Entity`, `AggregateRoot`, `DomainEvent`, `IRepository<T>`, and `Result` are shared architectural infrastructure rather than DDD Shared Kernel concepts.

They do not carry business meaning and are used across the application for consistency.

---

## 6. Transaction Boundaries and Consistency Models

| Interaction                                  | Communication Style                           | Transaction                                                        | Consistency                                              |
| -------------------------------------------- | --------------------------------------------- | ------------------------------------------------------------------ | -------------------------------------------------------- |
| Ordering → Catalog (stock decrease/increase) | Synchronous shared contract                   | Same database transaction in the monolith                          | Strong / atomic                                          |
| Ordering → Shipping (fee calculation)        | Synchronous shared contract                   | Fee calculated outside the transaction; value persisted in `Order` | Immediate value; calculation itself is non-transactional |
| Ordering → Finance (payment initiation)      | Asynchronous integration event                | Separate transactions                                              | Eventually consistent                                    |
| Finance → Ordering (payment status)          | Asynchronous integration event                | Separate transactions                                              | Eventually consistent                                    |
| Shipping → Ordering (shipment status)        | Asynchronous integration event                | Separate transactions                                              | Eventually consistent                                    |
| Ordering → People (address lookup)           | Synchronous read through application contract | No transaction required                                            | Read consistency                                         |
| CustomerEngagement operations                | Local within context                          | Atomic per mutation                                                | Strong within context                                    |

**Cross-context transaction participation** is a deliberate modular-monolith optimization. It does not imply that DDD boundary ownership is compromised.

If a bounded context is later extracted into a separate process, synchronous in-process contracts that cross the boundary would need to be replaced with an appropriate inter-process communication mechanism.

---

## 7. Key Architectural Policies

1. **Publisher owns integration event contracts.** Subscribers own only their handlers.
2. **Domain events are local and transient.** They are not persisted to the Outbox unless a domain event handler explicitly translates them into integration events.
3. **Shared contracts are for synchronous operations only.** Asynchronous communication uses Outbox events.
4. **Authorization layers:** API controllers enforce roles; application commands and queries enforce permissions; handlers enforce resource ownership.
5. **Notification workflows are owned by the business context**, not by the Notifications module. The module provides channels and infrastructure.
6. **File Storage is a capability, not a bounded context.** It exposes `IFileService` for cross-cutting file operations.
7. **People data is accessed through consumer-oriented contracts**, such as `IPersonReader`, not by directly depending on People’s repositories.
8. **Historical data required by an order or shipment is captured as snapshots** within the respective aggregates, not continuously read from live Person data.

---

## 8. Summary

The context map reveals a clean, modular monolith with clear ownership boundaries. Synchronous shared contracts handle immediate atomic operations, while Outbox-based integration events handle eventual consistency across contexts.

Self-contained contexts like CustomerEngagement remain simple and isolated. People data is accessed through stable application contracts, and historical snapshots prevent cross-context leakage.

This structure supports current business requirements while preserving the ability to extract contexts later without architectural redesign.

---

**Document version:** 1.0 – Final

**Feature:** Context Map
