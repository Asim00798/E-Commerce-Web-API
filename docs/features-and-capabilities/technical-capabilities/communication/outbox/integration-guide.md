# Outbox Feature — Integration Guide

**E-Commerce Modular Monolith**
**Scope:** How bounded contexts and modules integrate with the Outbox pipeline
**Version:** 1.0 — Final

**Related documents:**

* Outbox Technical Requirements
* Outbox Technical Reference
* Event Dispatching + Outbox Architecture
* End-to-End Outbox Dispatching + Per-Handler Idempotency
* Dead Letter Queue for the Outbox Pipeline

## 1. Purpose

This guide explains how bounded contexts and modules integrate with the Outbox pipeline.

It is intended for developers who need to:

* Add an integration event that other contexts should react to.
* Add a domain event handler that translates a business fact into one or more integration events.
* Add an integration event handler that consumes events published by another context.
* Test the integration flow end to end.

This document is **prescriptive**: it explains what to implement, where to place the code, and which rules to follow.

It is not a specification of Outbox internals or failure-handling behavior. Those concerns belong to the Outbox Technical Reference and Dead Letter Queue documentation.

## 2. Integration Overview

The Outbox pipeline hides its infrastructure details from the code that publishes and consumes integration events.

A typical integration involves three elements:

1. **Domain event:** Represents a business fact raised by an aggregate and dispatched within the Unit of Work's persistence workflow.
2. **Domain event handler:** Translates the domain fact into one or more integration events and writes them through `IOutboxMessageWriter`.
3. **Integration event handler:** Consumes an integration event and executes the subscriber context's application logic.

The Infrastructure layer handles persistence, serialization, dispatch, retry, dead-lettering, correlation propagation, and related processing concerns.

Application registration and Scheduling infrastructure provide handler discovery, decoration, and background execution according to the project's configuration.

**Integration code should not manually configure individual Outbox handlers, event types, or recurring processing jobs.** However, the relevant assemblies, services, decorators, and background infrastructure must still be registered correctly by the application's composition root.

## 3. The Three Integration Points

### 3.1 Point 1 — Publishing an Integration Event from a Business Fact

**Scenario:** A business operation produces a fact that another bounded context should eventually react to.

#### Steps

1. Add a domain event class in the aggregate's `Events/` folder.
2. Add a domain event handler in `Application/BoundedContexts/<Context>/DomainEventHandlers/`.
3. Add an integration event class in `Application/BoundedContexts/<Publisher>/IntegrationEvents/`.
4. Translate the domain event into the integration event inside the handler.
5. Call `IOutboxMessageWriter.WriteAsync(...)` to stage the integration event for persistence.

#### Example: Domain Event

```csharp
namespace E_Commerce.Domain.BoundedContexts.Core.Ordering
    .AggregateRoots.Order.Events;

public sealed record OrderPaidDomainEvent(
    Guid OrderId,
    Guid CustomerId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

The domain event represents a fact that has occurred within the Ordering domain.

#### Example: Integration Event

```csharp
namespace E_Commerce.Application.BoundedContexts.Orders
    .IntegrationEvents;

public sealed record OrderReadyForShippingIntegrationEvent
    : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public string? CorrelationId { get; init; }

    public Guid OrderId { get; init; }

    public Guid CustomerId { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string Street { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public string LocationMapUrl { get; init; } = string.Empty;
}
```

The event contains the information a subscriber needs to react to the business fact without having to query the publisher's private persistence model.

Only include fields justified by the event's consumers and data-handling requirements.

#### Example: Domain Event Handler

```csharp
namespace E_Commerce.Application.BoundedContexts.Orders
    .DomainEventHandlers;

public sealed class OrderPaidDomainEventHandler
    : IDomainEventHandler<OrderPaidDomainEvent>
{
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly IPersonRepository _personRepository;
    private readonly IAppContext _appContext;

    public OrderPaidDomainEventHandler(
        IOutboxMessageWriter outboxWriter,
        IPersonRepository personRepository,
        IAppContext appContext)
    {
        _outboxWriter = outboxWriter;
        _personRepository = personRepository;
        _appContext = appContext;
    }

    public async Task Handle(
        OrderPaidDomainEvent domainEvent,
        CancellationToken ct = default)
    {
        var person =
            await _personRepository.GetByCustomerIdAsync(
                domainEvent.CustomerId,
                ct);

        var integrationEvent =
            new OrderReadyForShippingIntegrationEvent
            {
                CorrelationId = _appContext.CorrelationId,
                OrderId = domainEvent.OrderId,
                CustomerId = domainEvent.CustomerId,
                FullName = person?.FullName ?? string.Empty,
                PhoneNumber = person?.PhoneNumber ?? string.Empty,
                Street = person?.Address?.Street ?? string.Empty,
                City = person?.Address?.City ?? string.Empty,
                LocationMapUrl =
                    person?.Address?.LocationMapUrl ?? string.Empty
            };

        await _outboxWriter.WriteAsync(integrationEvent, ct);
    }
}
```

The example assumes that the repository and `IOutboxMessageWriter` participate in the same database transaction managed by the Unit of Work. Adapt the repository, event, and property names to the actual contracts.

#### What Happens Automatically

* The domain event handler runs within the configured domain-event dispatch workflow.
* `IOutboxMessageWriter` serializes the integration event and stages an `OutboxMessage` through the Outbox persistence implementation.
* The Unit of Work persists the Outbox record within the business transaction.
* The business state change and Outbox record commit atomically.
* The Outbox processing job subsequently discovers the persisted message and attempts dispatch.

The exact number of `SaveChangesAsync` calls depends on the Unit of Work implementation; the essential guarantee is that both the business change and Outbox record participate in the same transaction.

#### What the Publisher Handler Does Not Do

The handler does not:

* Serialize the integration event manually.
* Insert rows directly into the Outbox table.
* Inject the Outbox repository or manipulate `OutboxMessage` directly.
* Implement dispatch retries or dead-lettering.
* Implement subscriber idempotency.

**Important:** Do not assume the domain event handler is invoked exactly once in every possible execution. Domain-event dispatch and transaction retry behavior must be understood separately. If duplicate Outbox writes are possible in the actual execution model, protect against them where required.

### 3.2 Point 2 — Consuming an Integration Event

**Scenario:** A bounded context needs to react to an integration event published by another context.

#### Steps

1. Add an integration event handler under `Application/BoundedContexts/<Subscriber>/IntegrationEventHandlers/`.
2. Implement `IIntegrationEventHandler<TIntegrationEvent>` using the publisher's event type.
3. Implement the subscriber's application behavior.
4. Let the Outbox pipeline handle dispatch and per-handler deduplication.

#### Example: Subscriber Handler

```csharp
namespace E_Commerce.Application.BoundedContexts.Orders
    .IntegrationEventHandlers;

public sealed class PaymentCompletedIntegrationEventHandler
    : IIntegrationEventHandler<PaymentCompletedIntegrationEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentCompletedIntegrationEventHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        PaymentCompletedIntegrationEvent integrationEvent,
        CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(
            integrationEvent.OrderId,
            ct);

        if (order is null)
            return;

        order.MarkPaid();

        await _orderRepository.UpdateAsync(order, ct);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
```

This example is illustrative. The handler must follow the subscriber's actual transaction, aggregate-loading, and missing-order policies. Returning successfully when the order is missing is appropriate only if that outcome is intentional and does not represent a processing failure.

#### What Happens Automatically

When the application is configured correctly:

1. The handler is discovered through assembly scanning.
2. The integration-event handler is wrapped with the configured decorators.
3. `CorrelationScopeIntegrationEventHandler<T>` restores the event's correlation ID into the logging scope.
4. `IdempotentIntegrationEventHandler<T>` checks whether the handler has already been recorded as processed for the event.
5. If a matching processed-event marker exists, the handler is skipped.
6. Otherwise, the concrete handler executes.
7. After successful handler execution, the idempotency mechanism records the handler's completion.
8. The Outbox message is marked `Processed` after all applicable handlers complete successfully, according to the pipeline's dispatch policy.
9. If a handler fails, the exception propagates to the Outbox processing layer, which applies its retry and dead-letter policies.

The actual decorator ordering must match the application's dependency-injection registration.

#### The Subscriber Must Not

* Catch and swallow exceptions that represent real processing failures.
* Directly insert records into the Outbox table to publish a follow-up event.
* Assume the idempotency decorator guarantees exactly-once external effects.
* Assume concurrent executions cannot overlap.
* Assume a handler's external side effects can be rolled back with its database transaction.

If a follow-up integration event is required, mutate the relevant aggregate and let its domain event handler produce the new integration event.

The subscriber remains responsible for designing safe business behavior, especially when it performs non-transactional external operations.

### 3.3 Point 3 — Publishing Without a Preceding Domain Event

**Scenario:** A background job or application workflow must publish an integration event that is not naturally associated with an aggregate's domain event.

This is an exception to the normal publishing flow.

#### Steps

1. Inject `IOutboxMessageWriter` into the appropriate application service or job handler.
2. Construct the integration event.
3. Write the event through `IOutboxMessageWriter`.
4. Ensure the Outbox record is persisted and committed using the correct Unit of Work lifecycle.

#### Example: Background Job

```csharp
public sealed class SomeJobHandler
    : IJobHandler<SomeJobPayload>
{
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly IUnitOfWork _unitOfWork;

    public SomeJobHandler(
        IOutboxMessageWriter outboxWriter,
        IUnitOfWork unitOfWork)
    {
        _outboxWriter = outboxWriter;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        SomeJobPayload job,
        CancellationToken ct)
    {
        var integrationEvent = new SomeIntegrationEvent
        {
            CorrelationId = null
            // Populate the remaining event fields.
        };

        await _outboxWriter.WriteAsync(integrationEvent, ct);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
```

The example assumes automatic transaction mode. In manual transaction mode, the caller must explicitly commit the transaction.

For background work, a correlation ID may be unavailable from an HTTP request. In that case, use `null` or establish a job-specific correlation convention supported by the application.

**This is the exception, not the rule.** Domain-event handlers remain the preferred mechanism for publishing integration events that represent aggregate business facts.

## 4. Contracts Reference

### 4.1 `IIntegrationEvent`

**Layer:** Application
**Responsibility:** Defines the publisher-owned contract for a durable application-level event.

```csharp
public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTime OccurredAt { get; }

    string? CorrelationId { get; init; }
}
```

| Property        | Rule                                                                                                                           |
| --------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| `EventId`       | Assign a unique event identifier when constructing the event.                                                                  |
| `OccurredAt`    | Record the event occurrence time using a consistent UTC convention.                                                            |
| `CorrelationId` | Populate from `IAppContext.CorrelationId` when available; otherwise follow the application's background-processing convention. |

Event IDs must remain stable across retries of the same persisted event. Do not create a new event ID each time the same Outbox message is dispatched.

### 4.2 `IIntegrationEventHandler<T>`

**Layer:** Application
**Responsibility:** Defines a subscriber handler for a particular integration event.

```csharp
public interface IIntegrationEventHandler<in TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    Task HandleAsync(
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken);
}
```

Handlers are discovered through the application's configured registration mechanism. No per-handler registration is required when the relevant assembly scanning is configured correctly.

### 4.3 `IDomainEventHandler<T>`

**Layer:** Domain/Application, according to the project's domain-event contract placement
**Responsibility:** Defines a handler for a domain event raised by the domain model.

```csharp
public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task Handle(
        TDomainEvent domainEvent,
        CancellationToken cancellationToken = default);
}
```

Domain-event handlers run through the configured domain-event dispatch mechanism. Their execution must participate in the Unit of Work lifecycle that persists the resulting changes and Outbox records.

### 4.4 `IOutboxMessageWriter`

**Layer:** Application
**Responsibility:** Provides the sanctioned write-side abstraction for staging integration events.

```csharp
public interface IOutboxMessageWriter
{
    Task WriteAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);
}
```

Publishers must use this abstraction rather than accessing the underlying Outbox repository or entity directly.

## 5. Integration Rules

### 5.1 Publisher Rules

| ID  | Rule                                                                                                                                                                           |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| P-1 | Publish integration events through `IOutboxMessageWriter.WriteAsync(...)`. Never insert Outbox rows directly.                                                                  |
| P-2 | Prefer domain-event handlers for integration events that represent aggregate business facts. Use direct publishing only when justified.                                        |
| P-3 | Populate `CorrelationId` from `IAppContext` when a request correlation exists.                                                                                                 |
| P-4 | Include the information subscribers need to react to the event without querying the publisher's private persistence model for data that belongs in the event contract.         |
| P-5 | Keep payloads reasonably small to reduce serialization and database-storage overhead.                                                                                          |
| P-6 | Never include passwords, access tokens, secrets, full payment-card numbers, or other inappropriate sensitive data. Outbox payloads require suitable data-handling protections. |
| P-7 | Prefer JSON-compatible primitives and simple records. Avoid serializing domain entities or types that cannot reliably round-trip through the configured serializer.            |
| P-8 | Treat event contracts as compatibility-sensitive. Prefer additive, backward-compatible changes and do not remove or rename fields without a migration strategy.                |
| P-9 | Make enum serialization explicit and stable where event compatibility depends on enum values. Do not assume that changing enum declarations is harmless.                       |

**Contract evolution:** Adding an optional property is not automatically backward-compatible in every serializer configuration. Verify serialization behavior, default values, and compatibility with already-persisted messages before changing a contract.

### 5.2 Subscriber Rules

| ID  | Rule                                                                                                                                                                   |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| S-1 | Design handlers to tolerate redelivery. The pipeline provides sequential per-handler deduplication, not universal exactly-once execution.                              |
| S-2 | Do not publish follow-up events by writing directly to the Outbox table. Use the appropriate aggregate and domain-event workflow when a new business fact is produced. |
| S-3 | Let genuine processing failures propagate so the Outbox pipeline can apply retry and dead-letter policies.                                                             |
| S-4 | Do not assume ordering across different event types or independent business transactions.                                                                              |
| S-5 | Do not assume that a handler's non-transactional side effects cannot happen more than once.                                                                            |
| S-6 | Do not read Outbox, dead-letter, or processed-event tables directly from business handlers.                                                                            |
| S-7 | Do not inject `IOutboxMessageRepository`, `IDeadLetterRepository`, or `OutboxMessage` into subscriber business code. These are internal persistence concerns.          |
| S-8 | Do not branch business behavior on `RetryCount`, `Status`, or other Outbox implementation metadata. Handle the event according to its business meaning.                |

### 5.3 Event Contract Rules

| ID  | Rule                                                                                                                                                      |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | Integration events are owned by the publisher context. Subscribers reference the publisher's event contract rather than defining duplicate versions.      |
| C-2 | Prefer records for integration-event contracts.                                                                                                           |
| C-3 | Use immutable or init-only properties wherever practical.                                                                                                 |
| C-4 | Name events after facts that occurred, not commands that request an action. For example, `OrderPlacedIntegrationEvent`, not `PlaceOrderIntegrationEvent`. |
| C-5 | Store event contracts under the publisher's `IntegrationEvents/` directory.                                                                               |
| C-6 | Store subscriber handlers under the subscriber's `IntegrationEventHandlers/` directory.                                                                   |

## 6. Common Integration Scenarios

### 6.1 Publishing from a Business Operation

```text
Command
   |
   v
Command Handler
   |
   v
Aggregate.MarkPaid()
   |
   v
Aggregate records OrderPaidDomainEvent
   |
   v
UnitOfWork.SaveChangesAsync()
   |
   +----> Save business state
   |
   +----> Dispatch OrderPaidDomainEvent
   |          |
   |          v
   |      OrderPaidDomainEventHandler
   |          |
   |          v
   |      IOutboxMessageWriter.WriteAsync(...)
   |
   +----> Save Outbox record
   |
   v
Commit transaction
```

**Result:** The business state change and Outbox record are committed atomically, provided they participate in the same database transaction.

### 6.2 Consuming an Event in Another Context

```text
OutboxProcessingJob
       |
       v
OutboxDispatchService
       |
       +----> Resolve concrete event type
       |
       +----> Deserialize event payload
       |
       v
IntegrationEventDispatcher
       |
       v
Correlation Scope Decorator
       |
       v
Idempotency Decorator
       |
       v
Concrete Subscriber Handler
       |
       v
Record successful handler processing
```

The exact decorator order is determined by the configured dependency-injection decoration sequence.

**Result:** The subscriber executes through the configured processing pipeline, with correlation context restored and sequential per-handler deduplication applied.

### 6.3 Multiple Subscribers for One Event

An integration event may have zero, one, or multiple subscriber handlers.

For example, `ShipmentDeliveredIntegrationEvent` might be handled by:

* An Ordering handler that updates the order's delivery state.
* A Notifications handler that initiates a customer notification workflow.

Each handler has its own processing marker.

If the Ordering handler succeeds but the Notifications handler fails, the next attempt can skip the successfully marked Ordering handler and retry the Notifications handler.

This behavior depends on successful marker persistence and the pipeline's per-handler idempotency implementation.

It does not prevent duplicate external side effects if a handler performs an effect and then fails before its processing marker commits.

### 6.4 Correlation Across the Asynchronous Boundary

The correlation ID can flow through the following sequence:

1. `CorrelationIdMiddleware` establishes the request's correlation ID and logging scope.
2. A domain event handler reads `IAppContext.CorrelationId`.
3. The handler assigns that ID to the integration event.
4. The Outbox serializer persists the correlation ID with the event payload.
5. During dispatch, `CorrelationScopeIntegrationEventHandler` restores the ID into the subscriber's logging scope.
6. Subscriber logs and downstream operations can use the same correlation ID.

This enables an originating HTTP request to be correlated with its asynchronous consequences.

Background jobs without an originating HTTP request should follow the application's established correlation policy.

### 6.5 An Integration Event with No Subscribers

If an event has no registered handlers and the dispatcher treats zero handlers as a successful dispatch, the Outbox message can be marked `Processed` without side effects.

This behavior should be intentional and verified against the actual dispatcher implementation. It can be useful when events are introduced before subscribers exist, but it can also hide a missing handler caused by configuration errors.

If a particular event requires at least one subscriber, enforce that requirement through configuration validation or another explicit mechanism.

### 6.6 Adding an Integration Event Without Modifying the Pipeline

To add a new event type:

1. Create the event contract in the publisher's `IntegrationEvents/` directory.
2. Create the domain event and publisher handler if the event represents an aggregate business fact.
3. Create subscriber handlers in the contexts that need to react.
4. Verify the relevant assemblies are included in handler discovery.
5. Add appropriate tests.

Under the existing conventions, no per-event changes should be required in `OutboxInfrastructureExtensions`, `OutboxProcessingJob`, `OutboxDispatchService`, or `OutboxSerializer`.

Changes to the infrastructure are needed only when the integration requires capabilities beyond the existing pipeline.

## 7. What Integration Code Does Not Need to Know

The following are Outbox infrastructure concerns:

| Concern                                   | Example location                                                  |
| ----------------------------------------- | ----------------------------------------------------------------- |
| Outbox table configuration                | `Persistence/Modules/Outbox/Configurations/`                      |
| Outbox persistence entity                 | `Communication/Messaging/Outbox/Entities/`                        |
| Event serialization                       | `Communication/Messaging/Outbox/Serialization/OutboxSerializer`   |
| Event type resolution and deserialization | `Communication/Messaging/Outbox/Processing/OutboxDispatchService` |
| Retry configuration                       | `Communication/Messaging/Outbox/Configuration/OutboxOptions`      |
| Dead-letter entity and persistence        | `Communication/Messaging/Outbox/Entities/DeadLetterMessage`       |
| Recurring processing job                  | `Scheduling/Hangfire/OutboxProcessingJob`                         |
| Handler decorator registration            | `Application/DependencyInjection/ApplicationServiceRegistration`  |
| Persistence indexes                       | Outbox EF Core configuration                                      |

These paths illustrate the documented organization and should match the actual repository structure.

Integration handlers should depend on application-level contracts rather than internal Outbox entities, repositories, or table schemas.

If business code references these internal implementation details, review the dependency boundary.

## 8. Testing Integration

### 8.1 Testing a Publisher

#### Unit Tests

Unit-test the domain event handler using a mocked `IOutboxMessageWriter`.

Verify that:

* The expected integration event is written.
* Event properties are populated correctly from the domain event and required dependencies.
* The correlation ID is propagated according to policy.
* The correct number of integration events is produced for the scenario.

#### Integration Tests

Use a real database context and the actual `IOutboxMessageWriter` implementation.

Verify that:

* The business state and Outbox record commit together.
* A failure before commit does not leave the business change or Outbox record partially persisted.
* The stored event type can be resolved by the dispatcher.
* The serialized payload can be deserialized into the intended event type.
* A transaction rollback prevents the integration event from becoming visible as a committed Outbox message.

### 8.2 Testing a Subscriber

#### Unit Tests

Test the concrete handler using mocked repositories and other dependencies.

Verify that:

* The expected business logic executes.
* Business invariants are enforced.
* Genuine processing failures are propagated.
* The handler does not duplicate idempotency logic that belongs to the pipeline.

#### Integration Tests

Persist an Outbox message and run the processing job or dispatch service.

Verify that:

* The expected handler executes.
* A successful `ProcessedEvents` marker is recorded for that handler.
* The Outbox message is marked `Processed` after all applicable handlers succeed.
* Reprocessing skips a handler whose marker has already been committed.
* A handler failure leaves the event eligible for retry according to the configured pipeline behavior.

### 8.3 Testing Correlation Propagation

Publish an event with a known correlation ID and run it through the real dispatch pipeline.

Capture the relevant logs and verify that the subscriber's processing logs contain the expected correlation ID.

### 8.4 Testing Failure Handling

Create an Outbox message whose subscriber fails.

Verify that:

* The failure is recorded according to the configured state model.
* The retry count changes as expected.
* The handler is retried on a subsequent processing attempt.
* After the configured retry limit is exhausted, the message follows the documented dead-letter policy.

The exact retry count, transition sequence, and dead-letter timing must match the Outbox Technical Reference.

### 8.5 Testing Multiple Subscribers

Register two handlers for the same event type. Configure one to succeed and the other to fail.

Verify that:

* The successful handler's processing marker is committed.
* The message is not considered fully processed while a required handler remains unsuccessful.
* On retry, the previously marked handler is skipped.
* The failing handler is attempted again.
* The message becomes `Processed` once all applicable handlers have succeeded.

### 8.6 What Not to Test Here

Integration-feature tests should not duplicate the Outbox implementation's internal tests for:

* Serialization internals.
* Database index definitions.
* Recurring-job registration details.
* Infrastructure decorator implementation details.

Those concerns belong in the relevant infrastructure test suites.

Do not assert a particular handler order unless ordering is an explicit, documented contract. Assembly-scanning order is not a reliable substitute for a business-level ordering guarantee.

## 9. Integration Checklist

Use this checklist when introducing an integration.

### Publishing an Event

* [ ] The domain event is defined in the aggregate's `Events/` directory when applicable.
* [ ] The domain event handler is in the publisher context's `DomainEventHandlers/` directory.
* [ ] The integration event is in the publisher context's `IntegrationEvents/` directory.
* [ ] The event implements `IIntegrationEvent`.
* [ ] `EventId` is unique and remains stable for the persisted event.
* [ ] `OccurredAt` follows the application's UTC timestamp convention.
* [ ] `CorrelationId` follows the application's correlation policy.
* [ ] The domain event handler injects `IOutboxMessageWriter`.
* [ ] The handler writes through `WriteAsync`; it does not insert Outbox rows directly.
* [ ] The payload contains only appropriate, serializable data.
* [ ] No inappropriate sensitive information is included.
* [ ] The event name describes a fact, not a command.
* [ ] The business change and Outbox record participate in the same transaction.
* [ ] Tests cover event construction and atomic persistence.

### Consuming an Event

* [ ] The handler is in the subscriber context's `IntegrationEventHandlers/` directory.
* [ ] The handler implements `IIntegrationEventHandler<TPublisherEvent>`.
* [ ] The publisher's event contract is referenced rather than duplicated.
* [ ] The handler implements the required business behavior.
* [ ] Genuine processing failures propagate.
* [ ] The handler does not write directly to the Outbox table.
* [ ] Non-transactional side effects have an appropriate idempotency or deduplication strategy.
* [ ] The handler is discovered by the configured assembly scanning.
* [ ] Tests cover successful processing, failures, and redelivery.

### Registration and Pipeline

* [ ] No per-handler registration is required under the existing discovery conventions.
* [ ] The publisher and subscriber assemblies are included in the configured scanning.
* [ ] The correlation and idempotency decorators are correctly registered.
* [ ] The Outbox processing job is configured.
* [ ] Retry and dead-letter behavior is covered by the relevant infrastructure tests.

## 10. Summary

Integrating with the Outbox pipeline involves three primary patterns:

1. **Publish from a business fact:** Raise a domain event, translate it in a domain event handler, and persist the integration event through `IOutboxMessageWriter`.
2. **Consume an integration event:** Implement `IIntegrationEventHandler<T>` in the subscriber context and let the configured pipeline handle dispatch and per-handler deduplication.
3. **Publish without a preceding domain event:** Write through `IOutboxMessageWriter` directly only when the integration event is not naturally produced by an aggregate's business fact.

The Outbox pipeline provides durable message persistence, dispatch, correlation propagation, per-handler deduplication, retry handling, and dead-letter processing according to its configuration.

The integration surface remains intentionally small:

* `IIntegrationEvent`
* `IIntegrationEventHandler<T>`
* `IDomainEventHandler<T>`
* `IOutboxMessageWriter`

**The governing principle:** Business modules define the facts and the behavior that follows them. The Outbox infrastructure provides the durable processing mechanism without exposing its internal persistence and dispatch details to integration code.

The pipeline provides at-least-once processing with per-handler deduplication—not universal exactly-once execution. Subscribers remain responsible for safe business behavior and for protecting external side effects against duplication where required.
