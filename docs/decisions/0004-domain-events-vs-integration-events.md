# ADR-0004: Domain Events vs. Integration Events

* **Status:** Accepted
* **Decision Type:** Architecture
* **Date:** 2026-10-13

## Context

The E-Commerce modular monolith uses Domain-Driven Design and event-driven mechanisms to coordinate business behavior within modules and communicate meaningful changes across module boundaries.

These interactions serve different purposes.

A domain event represents something meaningful that has happened within a domain model. It allows domain behavior and application orchestration to react to changes without embedding every reaction directly in the initiating operation.

An integration event communicates a meaningful business occurrence to another module that may need to react to it.

Although both mechanisms represent events, they have different ownership, lifecycle, and reliability requirements.

Treating them as interchangeable can introduce unnecessary coupling between domain models and integration contracts, expose internal implementation details, or cause unreliable cross-module processing.

The architecture therefore requires an explicit distinction between domain events and integration events, including how integration events are persisted and dispatched reliably.

## Decision

The system distinguishes between **domain events** and **integration events** according to their purpose and communication boundary.

* **Domain events** represent business occurrences within the domain model and are used to coordinate domain-related behavior.
* **Integration events** represent committed business occurrences that another module needs to know about.
* **The transactional outbox** provides durable publication of integration events when they must be recorded atomically with the corresponding business changes.

The system remains a modular monolith. Integration events do not imply that modules are separate services or communicate over a network.

### 1. Domain Events

Domain events express facts about business behavior within the domain model.

They are owned by the domain model that defines their meaning and are typically represented by domain-level event types.

Examples include:

* An order being placed.
* An order being cancelled.
* A payment being confirmed.
* A refund being completed.

The actual event types must reflect the responsibilities and vocabulary of the module that owns the relevant business concept.

Domain events may be raised by aggregates or other appropriate domain components when meaningful state transitions occur.

Their responsibilities include:

* Expressing a business fact.
* Preserving a meaningful domain vocabulary.
* Allowing relevant behavior to react without embedding every reaction in the aggregate.
* Supporting application-level coordination of domain behavior.

Domain events must not depend on infrastructure-specific mechanisms such as Hangfire, HTTP clients, or message brokers.

### 2. Domain-Event Processing

Application orchestration coordinates the processing of domain events raised during a use case.

Depending on the established Unit of Work design, domain events may be collected during domain operations and processed as part of a controlled persistence workflow.

Processing may result in:

* Additional domain changes.
* Application-level coordination.
* Creation of integration events.
* Other operations that must participate in the same transaction or occur after commit.

The exact transaction boundary depends on the operation and the established Unit of Work implementation.

A domain event being raised does not, by itself, guarantee that the corresponding database transaction will commit.

If a transaction fails, the system must not publish an integration event that falsely represents the failed business change as committed.

### 3. Integration Events

Integration events communicate business facts across module boundaries.

They are owned by the module that publishes the contract because that module is responsible for defining the meaning of the fact being communicated.

A consuming module owns its corresponding event handler and decides how to react to the published fact.

For example:

* Finance owns the contract for a payment-completed integration event.
* Ordering owns the handler that updates its order state in response to that event.

Similarly, Ordering may publish an event indicating that an order is ready for shipping, while Shipping owns the handler that initiates its shipping workflow.

Integration events should expose the information required by consumers without exposing internal aggregates, repositories, or persistence implementation details.

They should represent meaningful business facts rather than simply mirror every internal method call or database operation.

### 4. Integration Contracts and Ownership

The publishing module owns the integration-event contract. The consuming module owns its reaction.

This establishes a clear responsibility boundary:

| Concern                          | Owner                                               |
| -------------------------------- | --------------------------------------------------- |
| Business fact and its meaning    | Publishing module                                   |
| Integration-event contract       | Publishing module                                   |
| Decision to publish the event    | Publishing module's application workflow            |
| Event handler and reaction       | Consuming module                                    |
| Consumer-specific business rules | Consuming module                                    |
| Durable publication mechanism    | Shared technical infrastructure                     |
| Duplicate-processing protection  | Relevant consumer handler and persistence mechanism |

Integration contracts should remain stable enough for consumers to evolve independently within the modular monolith.

A change to an integration-event contract must consider all consumers and should be treated as an explicit compatibility decision.

### 5. Domain Events and Integration Events Are Not Interchangeable

A domain event and an integration event may describe related business facts, but they serve different architectural purposes.

For example, when an order is placed:

1. The Ordering domain model records the relevant business occurrence.
2. Application orchestration processes the resulting domain event.
3. If another module needs to react, the workflow creates an appropriate integration event.
4. The integration event is recorded in the outbox as part of the relevant transaction.
5. A background process dispatches the event after commit.
6. The consuming module processes the event according to its own responsibilities.

Not every domain event needs to become an integration event. Internal domain behavior may require no cross-module communication at all.

Likewise, integration-event contracts should not simply expose internal domain-event types by default.

### 6. Transactional Outbox Integration

Integration events that must be reliably published alongside business changes use the transactional outbox.

The intended flow is:

1. A use case performs its business operation.
2. Relevant aggregates are modified.
3. Domain events are collected and processed according to the Unit of Work workflow.
4. Integration-event records are created in the outbox.
5. Business changes and outbox records are committed atomically in the same database transaction.
6. A background job retrieves pending outbox records.
7. The dispatcher delivers the events to their intended handlers or configured transport.
8. Successful processing is recorded according to the dispatch and idempotency design.

The business transaction does not wait for every downstream reaction to finish.

The outbox ensures that the business change and the intent to publish its integration event are persisted together. It does not, by itself, guarantee that every consumer processes the event exactly once.

Delivery may be repeated, so consumers must be designed to handle duplicates where necessary.

The outbox is not a replacement for domain events. It addresses the reliability of integration-event publication.

### 7. In-Process Communication

Because the application is a modular monolith, integration-event dispatch may use in-process mechanisms where appropriate.

The term *integration event* describes the contract and communication boundary, not a required transport protocol.

A module can publish an integration event to another module without introducing HTTP calls, Kafka, RabbitMQ, or another network-based mechanism.

If the architecture later introduces an external broker or independently deployed service, the existing integration contracts may provide a starting point, but transport, delivery guarantees, security, and failure handling must be addressed explicitly.

## Rationale

This decision separates business meaning from communication and delivery mechanisms.

It provides the following benefits:

* **Clear ownership:** Each module defines its own business facts and publishes contracts for consumers.
* **Reduced coupling:** Consumers depend on integration contracts rather than the publisher's internal domain model.
* **Transaction safety:** The outbox records publication intent atomically with business changes.
* **Failure resilience:** Integration-event delivery can be retried independently of the original business transaction.
* **Explicit responsibilities:** Domain behavior remains separate from integration infrastructure.
* **Modular evolution:** Modules can change internal implementations while preserving stable contracts.
* **Operational simplicity:** The design supports reliable cross-module communication without requiring a distributed deployment.

The distinction also prevents a common design mistake: assuming that every event must be a message sent through a broker.

## Consequences

### Positive Consequences

* Domain models remain independent of transport and scheduling infrastructure.
* Integration contracts are owned by the modules that publish the relevant facts.
* Consuming modules control their own reactions and business rules.
* Reliable publication can be coordinated with the business transaction.
* Duplicate deliveries can be handled through explicit idempotency mechanisms.
* The architecture can support in-process dispatch today and evaluate external transports later if justified.

### Trade-offs and Constraints

* Some business occurrences require both a domain event and an integration event.
* Integration contracts require deliberate design and compatibility management.
* Outbox processing introduces operational concerns such as retries, monitoring, backlog management, and poison-message handling.
* Consumers must account for duplicate delivery and possible delays.
* A successful database commit does not imply that every downstream module has completed its reaction.
* Event ordering must not be assumed unless the implementation explicitly guarantees it.
* The transactional outbox does not make multiple independently committed module operations one atomic transaction.

## Alternatives Considered

### 1. Use Domain Events for All Cross-Module Communication

**Description:** Allow consuming modules to subscribe directly to the publisher's domain-event types.

**Reason for Rejection:** This couples consumers to internal domain-model details and makes domain-event refactoring more likely to affect unrelated modules. It also obscures the distinction between internal domain behavior and cross-module contracts.

### 2. Use Integration Events for All Internal Domain Behavior

**Description:** Represent every domain occurrence as an integration event, even when only the originating module needs to react.

**Reason for Rejection:** This introduces unnecessary contracts and infrastructure into internal domain behavior. Many domain events do not require cross-module communication.

### 3. Publish Integration Events Directly Before or During Commit

**Description:** Dispatch integration events immediately from command handlers without recording them in the same transaction as the business changes.

**Reason for Rejection:** A process failure can leave committed business state without a corresponding publication, or publish an event for a transaction that subsequently fails. The transactional outbox addresses this failure window.

### 4. Introduce a Message Broker for All Integration Events

**Description:** Require a distributed message broker for communication between modules.

**Reason for Rejection:** The application is a single deployable modular monolith, and network-based messaging is not a prerequisite for its module boundaries. A broker should be introduced when concrete transport, scale, isolation, or integration requirements justify it.

## Conditions for Reconsideration

This decision should be revisited if:

* A module becomes an independently deployed service.
* Integration events must be delivered to external systems through a broker or webhook.
* Event throughput or consumer isolation requires a different dispatch architecture.
* The system introduces separately persisted projections or additional asynchronous workflows.
* Contract compatibility requirements become more demanding.
* Existing transaction boundaries cannot support the required consistency guarantees.

Any change must preserve clear event ownership and explicitly address delivery guarantees, failure recovery, idempotency, and data consistency.

## Related Decisions

* [ADR-0001: Modular Monolith Architecture](0001-modular-monolith.md)
* [ADR-0005: Transactional Outbox](0005-transactional-outbox.md)
* [ADR-0006: Two-Phase Save in Unit of Work](0006-two-phase-save-in-unit-of-work.md)
* [ADR-0007: Per-Handler Idempotency](0007-per-handler-idempotency.md)
* [ADR-0009: Scheduling Abstraction over Hangfire](0009-scheduling-abstraction-over-hangfire.md)
* [ADR-0014: Single Deployable Unit](0014-single-deployable-unit.md)

## Summary

The E-Commerce system uses domain events to express business occurrences within a domain model and integration events to communicate relevant business facts across module boundaries.

The transactional outbox provides reliable persistence of integration-event publication intent alongside business changes.

**The governing principle is that domain events express internal business meaning, integration events define cross-module contracts, and infrastructure is responsible for reliable delivery.**
