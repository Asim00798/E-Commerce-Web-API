# ADR-0011: Notification Categories and Delivery Responsibilities

* **Status:** Accepted
* **Date:** 2026-10-13
* **Decision Type:** Architecture / Communication

## Context

The E-Commerce modular monolith requires several forms of communication with users and other application components.

Examples include:

* Transactional emails for account verification and password recovery.
* SMS messages for phone verification and other supported workflows.
* Push notifications for user-facing updates.
* In-application notifications that users can retrieve and review.
* Real-time messages that inform connected clients about newly available information.
* Internal integration events used to coordinate business behavior between modules.

Although these mechanisms may all involve messages or events, they serve different purposes and have different delivery, persistence, reliability, and ownership requirements.

Treating them as interchangeable would blur the distinction between business intent, notification delivery, durable notification records, and real-time client communication.

The application therefore needs an explicit classification that keeps business rules within their owning modules while providing reusable notification and communication infrastructure.

## Decision

**Classify communication by its purpose and responsibility, and keep notification delivery mechanisms within the Notifications capability.**

The architecture distinguishes the following categories:

1. Business-triggered notifications.
2. In-application notifications.
3. Real-time communication.
4. Internal domain events and cross-module integration events.

These categories are related but are not interchangeable. A single business occurrence may result in more than one communication mechanism.

The Notifications capability provides reusable notification creation and delivery infrastructure. The business feature that owns the relevant business event remains responsible for deciding whether a notification is required and what it means.

## 1. Business-Triggered Notifications

Business-triggered notifications communicate an outcome, action, or condition associated with a business workflow.

Examples include:

* Registration verification messages.
* Password recovery messages.
* Order confirmation notifications.
* Payment outcome notifications.
* Shipment and delivery updates.
* Refund status notifications.

The business feature owns the decision to initiate a notification and supplies the relevant business context.

The Notifications capability owns the reusable mechanisms for delivering the notification through supported channels.

For example, Ordering determines when an order confirmation should be initiated. Notifications provides the delivery infrastructure for the requested email, push notification, or other supported channel.

This separation prevents delivery infrastructure from taking ownership of business rules.

### Responsibility Boundaries

**Business feature responsibilities:**

* Determine when a notification is required.
* Establish the business meaning and relevant context.
* Determine the intended recipient according to business rules.
* Provide or select the appropriate notification content contract.
* Define whether a notification is mandatory for a particular workflow.

**Notifications capability responsibilities:**

* Provide reusable notification delivery contracts.
* Coordinate supported delivery channels.
* Persist notification records where required.
* Handle delivery failures and retries according to the configured mechanism.
* Track relevant delivery state.
* Integrate with external delivery providers.

The Notifications capability must not independently infer business decisions that belong to Ordering, Finance, People, Identity, or another owning module.

## 2. In-Application Notifications

In-application notifications are durable user-facing records that can be retrieved after the event that created them.

Examples include:

* An order status update.
* A refund status change.
* A shipment update.
* A notification that requires the user's attention.
* A historical notification that remains available after the user disconnects.

The `UserNotification` persistence model provides the application's source of truth for these notification records.

Its lifecycle may include creation, read/unread status, and other explicitly supported notification states.

The notification record is distinct from an email, push message, or real-time signal. Those mechanisms may inform the user about a notification, but they do not replace the durable in-application record.

### Persistence and Delivery

When a workflow requires a durable in-application notification, the application must persist the notification record through an appropriate reliable transaction boundary.

If notification creation must be atomic with a business change, the design must ensure that both operations participate in the intended transaction or use another explicit durability mechanism.

External delivery may then be performed asynchronously.

A successfully persisted in-application notification remains available even if an email, push notification, or real-time delivery attempt fails.

The exact retention policy and notification lifecycle must be defined according to the application's requirements.

## 3. Real-Time Communication

Real-time communication provides timely updates to connected clients.

The application uses SignalR for server-to-client communication where an active connection is appropriate.

Examples include:

* Informing a client that a new in-application notification is available.
* Indicating that an order or shipment update has occurred.
* Prompting a client to refresh information it can retrieve through an authorized API.

Real-time delivery is a transport mechanism, not a durable record of business state.

A client may be disconnected, experience network failure, or miss a message. The application must not assume that every connected client receives every real-time update.

### Post-Commit Delivery

Where a real-time message announces a committed database change, the application must not send the message before the corresponding transaction has committed.

A post-commit mechanism can publish the SignalR update after successful persistence.

If the real-time update must survive process failure, the system must provide a durable trigger or recovery mechanism. A simple in-memory post-commit callback does not, by itself, guarantee eventual delivery after a process crash.

For durable in-application notifications, the client should be able to retrieve the authoritative record through the application's normal authorized query path.

SignalR acts as a notification or refresh hint; it does not replace the persisted record or authorized API.

## 4. Email, SMS, and Push Delivery

Email, SMS, and push notifications are delivery channels rather than separate business domains.

The Notifications capability may implement these channels through provider-specific adapters.

Examples include:

* Email delivery for account verification or order updates.
* SMS delivery for phone verification.
* Push delivery through Firebase Cloud Messaging.

Provider integration details remain within Infrastructure.

Business features should not directly depend on provider-specific SDKs or implement their own delivery infrastructure.

### Delivery Reliability

External delivery is subject to provider availability, network failures, rate limits, invalid destinations, and uncertain outcomes.

The system must define appropriate retry and failure-handling policies for each channel.

A provider request timing out does not necessarily mean the provider failed to perform the operation. Retrying may result in duplicate delivery unless the provider or application offers an appropriate idempotency mechanism.

The application must therefore avoid assuming exactly-once external delivery.

For durable asynchronous delivery, the system may use the established Outbox and background-processing infrastructure where appropriate.

### Push Notification Device Identity

Where Firebase Cloud Messaging is used, the application manages the relevant Firebase registration token, commonly called an FCM registration token.

The token identifies an application installation or registration for push delivery; it is not interchangeable with a Firebase Installation ID (FID).

The application must use the identifier expected by the selected push-delivery API and manage token updates and invalidation according to provider behavior.

## 5. Domain Events and Integration Events Are Not Notifications

An event is not automatically a user notification.

The architecture distinguishes events by their purpose and communication boundary.

**Domain events** represent domain occurrences within the relevant business model.

**Integration events** communicate an occurrence across module boundaries through an explicit contract.

**Notifications** communicate information to a user or coordinate a user-facing delivery operation.

For example, a payment completion may produce a `PaymentCompletedIntegrationEvent` for Ordering. Ordering may update the order's payment state and determine whether a customer notification is required.

The integration event coordinates module behavior. The notification communicates relevant information to the customer.

An integration event may lead to a notification, but the two concepts have different responsibilities and should not be conflated.

The module that publishes an integration event owns its contract. The receiving module owns its handler. Notifications owns the reusable delivery mechanisms it provides.

## 6. Transactional Outbox and Background Processing

When a notification or integration-event delivery request must not be lost after a business transaction commits, the system must persist the required work through an appropriate durable mechanism.

The existing architecture uses a Transactional Outbox for integration-event publication and supports durable background processing through the centralized Scheduling capability.

These mechanisms have distinct responsibilities:

* The business transaction establishes the authoritative business change.
* The Outbox durably records integration-event publication intent.
* The Notifications capability manages notification records and delivery behavior.
* The Scheduling capability provides the background execution pipeline.
* External providers perform channel-specific delivery.
* Per-handler idempotency protects integration-event handlers against repeated delivery.

The system must not treat enqueueing an ordinary background job as a substitute for atomically persisting work that must survive a process failure.

The exact transaction and persistence arrangement for notification creation depends on the notification's requirements and the boundaries of the owning module.

## 7. Category and Channel Are Separate Dimensions

Notification category and delivery channel describe different properties.

A category expresses the notification's purpose or role. A channel describes how the information is delivered.

For example:

| Notification purpose         | Possible channel                            | Durable in-application record       |
| ---------------------------- | ------------------------------------------- | ----------------------------------- |
| Account verification         | Email or SMS                                | Optional, depending on requirements |
| Order confirmation           | Email, push, or in-application notification | Where required                      |
| Shipment update              | Push, email, or in-application notification | Where required                      |
| Payment outcome              | Email, push, or in-application notification | Where required                      |
| Real-time update hint        | SignalR                                     | Not provided by SignalR itself      |
| Internal module coordination | Integration event                           | Not inherently a user notification  |

The table illustrates possible combinations rather than requiring every notification to use every channel.

A notification may be delivered through multiple channels, and some delivery operations may not require a persistent in-application notification record.

The architecture must select channels according to the workflow's requirements rather than assuming that every message is an in-application notification or that every notification requires real-time delivery.

## Consequences

### Positive Consequences

* Business features retain ownership of notification decisions and business meaning.
* Delivery mechanisms can be reused across the application.
* Durable notification records are separated from best-effort real-time delivery.
* Email, SMS, and push providers can be integrated without leaking their SDKs into business logic.
* Integration events remain distinct from user-facing notifications.
* Failure handling and durability can be designed according to each communication mechanism's requirements.
* Notification categories and delivery channels can evolve independently.
* The design remains appropriate for a modular monolith without requiring a separate notification microservice.

### Trade-Offs

* The capability must coordinate multiple delivery channels and their differing failure semantics.
* Notification contracts and content ownership require clear boundaries.
* Durable delivery may require additional persistence and background processing.
* External providers can produce uncertain outcomes, making duplicate delivery possible.
* Real-time clients need a recovery strategy for missed messages.
* Transactional requirements differ between notification categories and must be documented rather than assumed.

These trade-offs are accepted because the application needs reusable communication infrastructure without transferring business ownership to that infrastructure.

## Alternatives Considered

### 1. Let Each Business Feature Implement Its Own Delivery Infrastructure

Each feature would integrate directly with email, SMS, push, and real-time providers.

**Rejected:** This would duplicate provider integration, delivery policies, error handling, and operational behavior across business modules.

### 2. Treat Every Event as a Notification

All domain and integration events would be handled as notification messages.

**Rejected:** Domain events, integration events, and user-facing notifications have different purposes and boundaries. Conflating them would couple business coordination to communication delivery.

### 3. Use SignalR as the Source of Truth

The application would send real-time messages without persisting notification records.

**Rejected as a general strategy:** SignalR does not provide durable notification history or guarantee delivery to disconnected clients. It remains appropriate for live updates and refresh hints.

### 4. Require Every Notification to Use Every Channel

Each notification would be stored in the application and delivered through email, SMS, push, and real-time communication.

**Rejected:** Channel selection must follow the requirements of each workflow. Mandatory use of every channel would create unnecessary delivery costs, complexity, and duplicate communication.

### 5. Create a Separate Notification Microservice

Notifications would run as an independently deployed service with its own storage and communication boundary.

**Not adopted:** The current architecture can provide reusable notification infrastructure within the modular monolith. Independent deployment is not justified solely by the presence of multiple notification channels.

## Reconsideration Criteria

Revisit this decision if:

* Notification delivery becomes an independently scalable or deployable business capability.
* Regulatory or business requirements introduce substantial independent notification-domain rules.
* Delivery volume or provider constraints require a different processing architecture.
* A durable delivery guarantee is required that the current mechanisms cannot provide.
* New channels or workflows require a materially different ownership model.
* The application introduces independently deployed consumers or a message broker for separately justified reasons.

Any change should preserve business ownership, distinguish durable state from transport delivery, and explicitly address failure and retry semantics.

## Related Decisions

* [ADR-0001: Modular Monolith](0001-modular-monolith.md)
* [ADR-0002: Clean Architecture Dependency Direction](0002-clean-architecture-dependency-direction.md)
* [ADR-0004: Domain Events vs. Integration Events](0004-domain-events-vs-integration-events.md)
* [ADR-0005: Transactional Outbox](0005-transactional-outbox.md)
* [ADR-0007: Per-Handler Idempotency for Integration Events](0007-per-handler-idempotency.md)
* [ADR-0009: Scheduling Abstraction over Hangfire](0009-scheduling-abstraction-over-hangfire.md)
* [ADR-0010: File Storage Is a Technical Capability, Not a Business Bounded Context](0010-file-storage-not-a-bounded-context.md)
* [ADR-0012: Automatic Handler and Trigger Discovery](0012-automatic-handler-and-trigger-discovery.md)
* [ADR-0013: Business Process vs. Housekeeping Jobs](0013-business-process-vs-housekeeping-jobs.md)

## Architectural Principle

**Business features decide what needs to be communicated; Notifications provides reusable delivery mechanisms; durable records, real-time transport, and integration events retain distinct responsibilities.**

The architecture separates business intent from delivery implementation and makes reliability guarantees explicit for each communication mechanism.
