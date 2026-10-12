# Outbox Feature — Technical Reference Document

**Project:** E-Commerce Modular Monolith
**Scope:** Transactional Outbox pipeline — persistence, dispatch, idempotency, retry, and dead lettering
**Version:** 1.0 — Final

**Related documents:**

* Outbox Technical Requirements
* Event Dispatching + Outbox Architecture
* End-to-End Outbox Dispatching + Per-Handler Idempotency
* Dead Letter Queue for the Outbox Pipeline

---

## 1. Overview

The Outbox feature provides durable, transactional persistence of integration events within the modular monolith. It bridges the gap between a successfully committed business transaction and the asynchronous side effects that must eventually occur.

Two distinct pipelines exist:

1. **Write path (transactional)** — domain events raised by aggregates are dispatched inside the unit-of-work transaction. Domain event handlers may translate them into integration events and write those events to the Outbox table. Business state changes and outbox rows commit as a single unit.
2. **Delivery path (background)** — persisted integration events are fetched by a recurring job, deserialized, and dispatched to registered handlers. Failures increment a retry counter; messages that exceed the configured retry threshold are quarantined in a dead-letter table.

The paths are independent after the transaction commits. Their shared boundary is the domain event handler that writes an integration event to the Outbox during the business transaction. Everything after that commit is asynchronous and outside the original business transaction.

The Outbox implementation resides in the Infrastructure layer. Domain and Application code interact with it through abstractions, including `IDomainEvent`, `IDomainEventHandler<T>`, `IDomainEventDispatcher`, `IIntegrationEvent`, `IIntegrationEventHandler<T>`, `IIntegrationEventDispatcher`, `IOutboxMessageWriter`, and `IProcessedEventRepository`.

The Application layer owns the integration-event contracts and handler decorators; Infrastructure owns their persistence and execution mechanisms.

### Delivery guarantee

The Outbox provides **transactional persistence and at-least-once dispatch attempts**, with sequential per-handler deduplication.

It does not provide universal exactly-once execution. A process crash after a handler's side effect succeeds but before its processed-event marker commits can cause that handler to execute again. External side effects that require stronger guarantees must use downstream idempotency or another suitable coordination mechanism.

---

## 2. Architecture and Placement

### 2.1 Dependency Direction

```text
Domain
  │
  ▼
Application
  │
  ▼
Infrastructure
  │
  ▼
External Systems
SQL Server · Hangfire · Serilog · OpenTelemetry
```

* **Domain** defines `IDomainEvent`, `IDomainEventHandler<T>`, `IDomainEventDispatcher`, `IUnitOfWork`, and `BaseEntity` with `AddDomainEvent` and `ClearDomainEvents`.
* **Application** defines `IIntegrationEvent`, `IIntegrationEventHandler<T>`, `IIntegrationEventDispatcher`, `IOutboxMessageWriter`, `IProcessedEventRepository`, and the idempotency and correlation-scope decorators.
* **Infrastructure** implements dispatchers, Outbox entities and repositories, serialization, dispatch processing, dead-letter handling, background jobs, and EF Core configurations.

The Domain layer does not depend on the Outbox implementation, EF Core, Hangfire, or external delivery providers.

### 2.2 Layer Ownership

| Concern                                         | Layer          | Location                                                    |
| ----------------------------------------------- | -------------- | ----------------------------------------------------------- |
| Domain event definitions                        | Domain         | `Domain/BoundedContexts/*/AggregateRoots/*/Events/`         |
| Domain event handler abstraction                | Domain         | `Domain/SharedKernel/Events/IDomainEventHandler.cs`         |
| Domain event dispatcher abstraction             | Domain         | `Domain/SharedKernel/Events/IDomainEventDispatcher.cs`      |
| Integration event definitions                   | Application    | `Application/BoundedContexts/*/IntegrationEvents/`          |
| Integration event handler abstractions          | Application    | `Application/Shared/Communication/Messaging/Abstractions/`  |
| Idempotency and correlation decorators          | Application    | `Application/Shared/Communication/Messaging/Decorators/`    |
| Domain event dispatcher implementation          | Infrastructure | `Infrastructure/Communication/Messaging/Dispatching/`       |
| Integration event dispatcher implementation     | Infrastructure | `Infrastructure/Communication/Messaging/Dispatching/`       |
| Outbox writer, serializer, and dispatch service | Infrastructure | `Infrastructure/Communication/Messaging/Outbox/`            |
| Outbox and dead-letter entities                 | Infrastructure | `Infrastructure/Communication/Messaging/Outbox/Entities/`   |
| Outbox repositories                             | Infrastructure | `Infrastructure/Persistence/Modules/Outbox/Repositories/`   |
| EF Core configurations                          | Infrastructure | `Infrastructure/Persistence/Modules/Outbox/Configurations/` |
| Background jobs                                 | Infrastructure | `Infrastructure/Scheduling/Hangfire/`                       |

### 2.3 Trigger Contract

The write path is triggered by `UnitOfWork.SaveChangesAsync()`, either automatically from a command handler or as part of a manually managed transaction.

The delivery path is triggered by Hangfire, which invokes `OutboxProcessingJob.ExecuteAsync` on a recurring schedule, currently `Cron.Minutely`.

There is no continuous `BackgroundService` loop. Scheduling is external to the Outbox processing logic.

---

## 3. Folder and File Structure

### 3.1 Application Layer

```text
Application/
├── Shared/
│   └── Communication/
│       └── Messaging/
│           ├── Abstractions/
│           │   ├── IIntegrationEvent.cs
│           │   ├── IIntegrationEventHandler.cs
│           │   ├── IIntegrationEventDispatcher.cs
│           │   ├── IOutboxMessageWriter.cs
│           │   └── IProcessedEventRepository.cs
│           └── Decorators/
│               ├── CorrelationScopeIntegrationEventHandler.cs
│               └── IdempotentIntegrationEventHandler.cs
└── BoundedContexts/
    └── <Context>/
        ├── DomainEventHandlers/       # Domain-to-integration event translation
        ├── IntegrationEvents/         # Outbound event contracts owned by this context
        └── IntegrationEventHandlers/  # Inbound subscribers from other contexts
```

### 3.2 Infrastructure Layer

```text
Infrastructure/
├── Communication/
│   └── Messaging/
│       ├── Dispatching/
│       │   ├── DomainEventDispatcher.cs
│       │   └── IntegrationEventDispatcher.cs
│       └── Outbox/
│           ├── Configuration/
│           │   └── OutboxOptions.cs
│           ├── Contracts/
│           │   ├── IDeadLetterRepository.cs
│           │   └── IOutboxMessageRepository.cs
│           ├── Decorators/
│           │   └── EnrichedOutboxMessageWriter.cs
│           ├── Entities/
│           │   ├── DeadLetterMessage.cs
│           │   ├── OutboxMessage.cs
│           │   └── ProcessedEvent.cs
│           ├── Extensions/
│           │   └── OutboxInfrastructureExtensions.cs
│           ├── Implementation/
│           │   └── OutboxMessageWriter.cs
│           ├── Processing/
│           │   └── OutboxDispatchService.cs
│           └── Serialization/
│               └── OutboxSerializer.cs
├── Persistence/
│   ├── Common/
│   │   └── Implementation/
│   │       └── UnitOfWork.cs
│   └── Modules/
│       └── Outbox/
│           ├── Configurations/
│           │   ├── DeadLetterMessageConfiguration.cs
│           │   ├── OutboxConfiguration.cs
│           │   └── ProcessedEventConfiguration.cs
│           └── Repositories/
│               ├── DeadLetterRepository.cs
│               ├── OutboxMessageRepository.cs
│               └── ProcessedEventRepository.cs
└── Scheduling/
    ├── Extensions/
    │   └── SchedulingInfrastructureExtensions.cs
    └── Hangfire/
        ├── OutboxProcessingJob.cs
        ├── DeadLetterMonitorJob.cs
        ├── HangfireJobDispatcher.cs
        └── HangfireJobScheduler.cs
```

### 3.3 Domain Layer (Referenced Only)

```text
Domain/
└── SharedKernel/
    ├── Abstractions/
    │   └── BaseEntity.cs             # AddDomainEvent, ClearDomainEvents, DomainEvents
    ├── Events/
    │   ├── IDomainEvent.cs
    │   ├── IDomainEventHandler.cs
    │   ├── IDomainEventDispatcher.cs
    │   └── DomainEvent.cs
    └── PersistenceAbstractions/
        └── IUnitOfWork.cs
```

---

## 4. Domain Layer

The Domain layer participates in the Outbox pipeline only through domain-event abstractions. It does not know how integration events are persisted or delivered.

### 4.1 BaseEntity

Aggregate roots inherit from `BaseEntity`, which maintains domain events raised during domain operations.

```csharp
public abstract class BaseEntity : IValidatableEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents =>
        _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();
}
```

The `DomainEvents` collection is the source that `UnitOfWork` reads when dispatching domain events during `SaveChangesAsync`.

The exact collection and clearing strategy must ensure that events raised during handler execution are not accidentally lost before they have been dispatched and persisted successfully.

### 4.2 IDomainEvent

```csharp
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
```

This marker interface carries the occurrence timestamp. It does not define persistence semantics.

### 4.3 IDomainEventHandler<T>

```csharp
public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task Handle(
        TDomainEvent domainEvent,
        CancellationToken cancellationToken = default);
}
```

Domain event handlers are Application-layer classes. They may coordinate work within the same transaction and call `IOutboxMessageWriter.WriteAsync` to persist integration events.

They must not directly publish integration events to external systems from inside the business transaction.

### 4.4 IDomainEventDispatcher

```csharp
public interface IDomainEventDispatcher
{
    Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default);
}
```

`UnitOfWork` invokes this abstraction within the write transaction. The dispatcher coordinates handler execution; it does not itself persist domain events.

### 4.5 IUnitOfWork

```csharp
public interface IUnitOfWork
{
    Task BeginTransactionAsync(
        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(
        CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync();
}
```

The `SaveChangesAsync` implementation coordinates persistence and domain-event dispatch so business changes and resulting Outbox rows can be committed atomically. See §7.

---

## 5. Application Layer

The Application layer owns the integration-event abstractions used by publishers and subscribers, together with the decorators that wrap integration-event handlers.

### 5.1 Abstractions

#### IIntegrationEvent

The durable application-level message contract:

```csharp
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
    string? CorrelationId { get; init; }
}
```

* **EventId** identifies the integration event and participates in per-handler idempotency.
* **OccurredAt** preserves the event's occurrence time, rather than its persistence time.
* **CorrelationId** carries the originating request's correlation identifier across the asynchronous boundary.

#### IIntegrationEventHandler<T>

The subscriber contract:

```csharp
public interface IIntegrationEventHandler<in TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    Task HandleAsync(
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken);
}
```

#### IIntegrationEventDispatcher

The delivery mechanism invoked by `OutboxDispatchService`:

```csharp
public interface IIntegrationEventDispatcher
{
    Task DispatchAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken);
}
```

#### IOutboxMessageWriter

The write-side bridge used by domain event handlers:

```csharp
public interface IOutboxMessageWriter
{
    Task WriteAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);
}
```

#### IProcessedEventRepository

The per-handler idempotency marker store:

```csharp
public interface IProcessedEventRepository
{
    Task<bool> IsProcessedAsync(
        Guid eventId,
        string handlerIdentifier,
        CancellationToken cancellationToken);

    Task MarkAsProcessedAsync(
        Guid eventId,
        string handlerIdentifier,
        CancellationToken cancellationToken);
}
```

### 5.2 Decorators

Both decorators implement `IIntegrationEventHandler<T>` and wrap concrete handlers. They are applied through Scrutor during `AddApplication`.

#### 5.2.1 IdempotentIntegrationEventHandler<T>

```csharp
public sealed class IdempotentIntegrationEventHandler<TIntegrationEvent>
    : IIntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    private readonly IIntegrationEventHandler<TIntegrationEvent> _inner;
    private readonly IProcessedEventRepository _processedEvents;
    private readonly string _handlerId;

    public IdempotentIntegrationEventHandler(
        IIntegrationEventHandler<TIntegrationEvent> inner,
        IProcessedEventRepository processedEvents)
    {
        _inner = inner;
        _processedEvents = processedEvents;
        _handlerId = inner.GetType().FullName!;
    }

    public async Task HandleAsync(
        TIntegrationEvent integrationEvent,
        CancellationToken ct)
    {
        if (await _processedEvents.IsProcessedAsync(
                integrationEvent.EventId,
                _handlerId,
                ct))
        {
            return;
        }

        await _inner.HandleAsync(integrationEvent, ct);

        await _processedEvents.MarkAsProcessedAsync(
            integrationEvent.EventId,
            _handlerId,
            ct);
    }
}
```

Responsibilities:

* Derives the handler identifier from the concrete handler type.
* Checks `ProcessedEvents` before invoking the handler.
* Records the marker after the handler succeeds.
* Relies on the repository's database constraints to handle concurrent marker-insert races.

**Important limitation:** this decorator does not make the handler's side effects and marker insertion atomic. A crash between those operations can cause a successful side effect to be repeated.

#### 5.2.2 CorrelationScopeIntegrationEventHandler<T>

```csharp
public sealed class CorrelationScopeIntegrationEventHandler<TIntegrationEvent>
    : IIntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    private readonly IIntegrationEventHandler<TIntegrationEvent> _inner;
    private readonly ILogger<
        CorrelationScopeIntegrationEventHandler<TIntegrationEvent>> _logger;

    public CorrelationScopeIntegrationEventHandler(
        IIntegrationEventHandler<TIntegrationEvent> inner,
        ILogger<CorrelationScopeIntegrationEventHandler<TIntegrationEvent>> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task HandleAsync(
        TIntegrationEvent integrationEvent,
        CancellationToken ct)
    {
        using var scope = BeginCorrelationScope(integrationEvent);

        _logger.LogDebug(
            "Handling integration event {EventType} (Handler: {HandlerType})",
            typeof(TIntegrationEvent).Name,
            _inner.GetType().Name);

        await _inner.HandleAsync(integrationEvent, ct);
    }

    private IDisposable? BeginCorrelationScope(
        TIntegrationEvent integrationEvent)
    {
        if (string.IsNullOrEmpty(integrationEvent.CorrelationId))
            return null;

        return _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = integrationEvent.CorrelationId
        });
    }
}
```

Responsibilities:

* Reads `CorrelationId` from the event.
* Pushes it into the logging scope so downstream log entries inherit the identifier.
* Emits a debug-level log entry announcing handler invocation.

When `CorrelationId` is absent, no correlation scope is created.

### 5.3 Decorator Stack Ordering

Registration in `ApplicationServiceRegistration.AddApplication`:

```csharp
services.Scan(scan => scan
    .FromAssemblies(Assembly.GetExecutingAssembly())
    .AddClasses(classes =>
        classes.AssignableTo(typeof(IIntegrationEventHandler<>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

services.Decorate(
    typeof(IIntegrationEventHandler<>),
    typeof(IdempotentIntegrationEventHandler<>));

services.Decorate(
    typeof(IIntegrationEventHandler<>),
    typeof(CorrelationScopeIntegrationEventHandler<>));
```

With Scrutor's decoration semantics, the first decorator becomes the inner wrapper and the last becomes the outer wrapper.

The resulting pipeline is:

```text
CorrelationScopeIntegrationEventHandler<T>
  → IdempotentIntegrationEventHandler<T>
    → Concrete IIntegrationEventHandler<T>
```

The correlation scope therefore surrounds the idempotency check and concrete handler invocation. Logs produced during a skip decision can carry the same correlation ID.

---

## 6. Infrastructure Layer

### 6.1 Dispatching

#### 6.1.1 DomainEventDispatcher

`DomainEventDispatcher` implements `IDomainEventDispatcher` and is invoked by `UnitOfWork` during the write transaction.

For each domain event, it:

1. Constructs the closed generic handler type using `typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType())`.
2. Resolves registered handlers using the service provider.
3. Invokes each handler's `Handle` method and awaits its returned task.

Characteristics:

* Uses reflection because the concrete event type is known only at runtime.
* Runs within the transaction coordinated by the unit of work.
* Allows handler exceptions to propagate so the transaction can be rolled back.
* Does not persist domain events as Outbox messages.

#### 6.1.2 IntegrationEventDispatcher

`IntegrationEventDispatcher` implements `IIntegrationEventDispatcher` and is invoked by `OutboxDispatchService` after deserialization.

For each integration event, it:

1. Constructs `typeof(IIntegrationEventHandler<>).MakeGenericType(integrationEvent.GetType())`.
2. Resolves the registered decorated handler pipeline.
3. Invokes handlers sequentially through reflection.
4. Stops on the first exception and propagates it to the caller.

Handler order should not be treated as a business contract unless it is explicitly implemented and guaranteed.

### 6.2 Outbox Core

#### 6.2.1 Entities

**OutboxMessage** represents the durable integration-event row:

```csharp
public class OutboxMessage
{
    public Guid Id { get; set; }

    public string EventType { get; set; } = default!;

    public string Payload { get; set; } = default!;

    public DateTime OccurredAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public int RetryCount { get; set; }

    public string? Error { get; set; }

    public OutboxMessageStatus Status { get; set; }
        = OutboxMessageStatus.Pending;
}

public enum OutboxMessageStatus
{
    Pending,
    Processing, // Reserved for future claim-based multi-instance processing
    Processed,
    Failed,
    DeadLettered
}
```

`EventType` stores the CLR type name used to resolve the event during dispatch. `Payload` stores its serialized JSON representation.

**DeadLetterMessage** represents a quarantined message:

```csharp
public class DeadLetterMessage
{
    public Guid Id { get; set; }

    public Guid OriginalMessageId { get; set; }

    public string EventType { get; set; } = null!;

    public string Payload { get; set; } = null!;

    public string? Error { get; set; }

    public int RetryCount { get; set; }

    public DateTime DeadLetteredAt { get; set; }

    public DeadLetterStatus Status { get; set; }
        = DeadLetterStatus.DeadLettered;
}

public enum DeadLetterStatus
{
    DeadLettered,
    Reprocessing
}
```

`OriginalMessageId` is a soft reference to the source Outbox row; it does not enforce a foreign-key relationship.

**ProcessedEvent** records a handler's successful processing of an integration event:

```csharp
public class ProcessedEvent
{
    public Guid EventId { get; set; }

    public string HandlerIdentifier { get; set; } = null!;

    public DateTime ProcessedAt { get; set; }
}
```

#### 6.2.2 Repository Contracts

`IOutboxMessageRepository`:

```csharp
public interface IOutboxMessageRepository
{
    Task AddAsync(
        OutboxMessage message,
        CancellationToken cancellationToken);

    Task<List<OutboxMessage>> GetPendingMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken = default);

    Task MarkAsProcessedAsync(
        Guid messageId,
        CancellationToken cancellationToken = default);

    Task MarkAsFailedAsync(
        Guid messageId,
        string error,
        CancellationToken cancellationToken = default);

    Task MarkAsDeadLetteredAsync(
        Guid messageId,
        CancellationToken cancellationToken = default);
}
```

`IDeadLetterRepository`:

```csharp
public interface IDeadLetterRepository
{
    Task AddAsync(
        DeadLetterMessage message,
        CancellationToken cancellationToken);

    Task<List<DeadLetterMessage>> GetDeadLetteredAsync(
        CancellationToken cancellationToken);

    Task MarkAsReprocessingAsync(
        Guid deadLetterId,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        Guid deadLetterId,
        CancellationToken cancellationToken);
}
```

Repository implementations must be consistent about transaction ownership. A repository may call `SaveChangesAsync`, but when it shares a `DbContext` and connection with an active explicit transaction, that save does not independently commit the transaction.

#### 6.2.3 OutboxMessageWriter

`OutboxMessageWriter` implements `IOutboxMessageWriter`. It serializes an integration event into an `OutboxMessage` and adds it through `IOutboxMessageRepository`.

```csharp
public async Task WriteAsync(
    IIntegrationEvent integrationEvent,
    CancellationToken ct)
{
    var eventType = integrationEvent.GetType();

    var message = new OutboxMessage
    {
        Id = Guid.NewGuid(),
        EventType =
            $"{eventType.FullName}, {eventType.Assembly.GetName().Name}",
        Payload = _serializer.Serialize(integrationEvent),
        OccurredAt = integrationEvent.OccurredAt,
        Status = OutboxMessageStatus.Pending
    };

    await _outboxRepository.AddAsync(message, ct);
}
```

The stored type name includes the type's full name and assembly name, omitting version and culture. See §10.

The writer must add the Outbox row to the same transaction as the business state changes when called from a domain event handler.

#### 6.2.4 EnrichedOutboxMessageWriter

`EnrichedOutboxMessageWriter` is an internal decorator that logs before delegating:

```csharp
internal sealed class EnrichedOutboxMessageWriter
    : IOutboxMessageWriter
{
    private readonly IOutboxMessageWriter _inner;
    private readonly ILogger<EnrichedOutboxMessageWriter> _logger;
    private readonly IAppContext _appContext;

    public EnrichedOutboxMessageWriter(
        IOutboxMessageWriter inner,
        ILogger<EnrichedOutboxMessageWriter> logger,
        IAppContext appContext)
    {
        _inner = inner;
        _logger = logger;
        _appContext = appContext;
    }

    public async Task WriteAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Publishing integration event {EventType} with CorrelationId {CorrelationId}",
            integrationEvent.GetType().Name,
            _appContext.CorrelationId ?? "N/A");

        await _inner.WriteAsync(integrationEvent, ct);
    }
}
```

It is registered through Scrutor:

```csharp
services.TryDecorate<
    IOutboxMessageWriter,
    EnrichedOutboxMessageWriter>();
```

`TryDecorate` avoids applying the same decoration again when the service is already decorated. It does not replace the need to ensure that the underlying writer is registered.

#### 6.2.5 OutboxSerializer

`OutboxSerializer` is a stateless serializer using `System.Text.Json`:

```csharp
public class OutboxSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public string Serialize<T>(T obj)
        => JsonSerializer.Serialize(obj, Options);

    public object? Deserialize(string json, Type type)
        => JsonSerializer.Deserialize(json, type, Options);
}
```

**Known caveat:** enums serialize as integers by default. If event contracts contain enums, either maintain stable numeric values or introduce an explicit string-enum converter with a compatibility plan for existing persisted payloads.

Value objects must also round-trip correctly through `System.Text.Json`. Types with private constructors or custom construction requirements may need `[JsonConstructor]` or an explicit converter.

#### 6.2.6 OutboxDispatchService

`OutboxDispatchService` coordinates deserialization and dispatch for a single `OutboxMessage`. It is invoked by `OutboxProcessingJob`.

```csharp
public async Task DispatchMessageAsync(
    OutboxMessage message,
    CancellationToken ct)
{
    var eventType = ResolveEventType(message.EventType);

    if (eventType is null)
    {
        throw new InvalidOperationException(
            $"Unknown event type: {message.EventType}");
    }

    var integrationEvent = (IIntegrationEvent?)_serializer
        .Deserialize(message.Payload, eventType);

    if (integrationEvent is null)
    {
        throw new InvalidOperationException(
            $"Could not deserialize integration event: {message.EventType}");
    }

    await _dispatcher.DispatchAsync(integrationEvent, ct);
}

private static Type? ResolveEventType(string typeName)
{
    var direct = Type.GetType(typeName);

    if (direct is not null)
        return direct;

    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
    {
        var fromAssembly = assembly.GetType(typeName);

        if (fromAssembly is not null)
            return fromAssembly;
    }

    return null;
}
```

The type-resolution strategy is described in §10.

### 6.3 Dead-Letter Handling

#### 6.3.1 DeadLetterRepository

`DeadLetterRepository` implements `IDeadLetterRepository`. Its methods may call `SaveChangesAsync` to persist changes consistently with sibling repositories.

Example:

```csharp
public async Task AddAsync(
    DeadLetterMessage message,
    CancellationToken ct)
{
    await _dbContext.Set<DeadLetterMessage>()
        .AddAsync(message, ct);

    await _dbContext.SaveChangesAsync(ct);
}
```

When `AddAsync` is called inside an explicit transaction, the save participates in that transaction. The outer commit determines whether the change becomes durable.

#### 6.3.2 DeadLetterMonitorJob

`DeadLetterMonitorJob` is a recurring Hangfire job that checks the dead-letter table and logs a warning when quarantined messages exist.

```csharp
public async Task ExecuteAsync(
    IJobCancellationToken cancellationToken)
{
    var ct = cancellationToken.ShutdownToken;

    var deadMessages = await _deadLetterRepo
        .GetDeadLetteredAsync(ct);

    if (deadMessages.Count > 0)
    {
        _logger.LogWarning(
            "Dead-lettered messages detected: {Count}. Oldest: {Oldest}",
            deadMessages.Count,
            deadMessages.Min(m => m.DeadLetteredAt));
    }
}
```

The monitor provides a log-based operational signal. Alert routing through email, Slack, PagerDuty, or another system is external unless explicitly implemented.

### 6.4 Persistence

#### 6.4.1 UnitOfWork

`UnitOfWork` coordinates the write path. Its `SaveChangesAsync` implementation performs two saves when domain events are present:

```csharp
public async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
{
    bool ownsTransaction = _currentTransaction is null;

    if (ownsTransaction)
    {
        _currentTransaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);
    }

    try
    {
        var domainEvents = CollectDomainEvents();

        int affected = await SaveChangesInternalAsync(
            cancellationToken); // Save #1

        if (domainEvents.Count > 0)
        {
            await _domainEventDispatcher.DispatchAsync(
                domainEvents,
                cancellationToken);

            ClearDomainEvents();

            affected += await SaveChangesInternalAsync(
                cancellationToken); // Save #2
        }

        if (ownsTransaction)
        {
            await _currentTransaction!.CommitAsync(
                cancellationToken);

            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;

            await InvokePostCommitCallbacksAsync(
                cancellationToken);
        }

        return affected;
    }
    catch
    {
        if (ownsTransaction && _currentTransaction is not null)
        {
            await _currentTransaction.RollbackAsync(
                cancellationToken);

            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        throw;
    }
}
```

The snippet illustrates the intended sequence. The production implementation must also account for event collection and clearing semantics, transaction ownership, callback failure behavior, and exception paths.

* **Save #1** persists business state and any previously tracked changes.
* Domain events are dispatched. Their handlers may mutate additional aggregates or add Outbox rows to the same `DbContext`.
* **Save #2** persists those changes and Outbox rows.
* **Commit** finalizes the transaction.
* Post-commit callbacks run after commit and cannot roll back the committed transaction.

`DbUpdateConcurrencyException` may be translated to the application's `ConcurrencyException` for the caller.

**Manual transaction mode:** if a caller invokes `BeginTransactionAsync` first, `SaveChangesAsync` detects the active transaction and does not commit it. The caller remains responsible for the final commit or rollback.

**Important implementation considerations:**

* If a post-commit callback throws after the transaction commits, the caller may receive an exception even though the business transaction succeeded. Callbacks must be treated as best-effort, and their failures must not imply that the database transaction was rolled back.
* When `SaveChangesAsync` participates in a caller-owned transaction, the event-dispatch and save sequence must remain coordinated with the caller's transaction boundary.
* Clearing domain events before the transaction successfully commits requires careful failure handling. A rollback should not silently discard events if the same tracked aggregates may be reused.
* EF Core execution strategies that retry transactions must be coordinated with domain-event dispatch to avoid duplicate in-memory work or duplicate Outbox entries.

#### 6.4.2 OutboxMessageRepository

The repository fetches pending and failed messages in occurrence-time order:

```csharp
public async Task<List<OutboxMessage>> GetPendingMessagesAsync(
    int batchSize,
    CancellationToken ct)
{
    return await _dbContext.OutboxMessages
        .Where(m =>
            m.Status == OutboxMessageStatus.Pending ||
            m.Status == OutboxMessageStatus.Failed)
        .OrderBy(m => m.OccurredAt)
        .Take(batchSize)
        .ToListAsync(ct);
}
```

The repository updates a message after processing:

```csharp
public async Task MarkAsProcessedAsync(
    Guid messageId,
    CancellationToken ct)
{
    var msg = await _dbContext.OutboxMessages
        .FindAsync(new object[] { messageId }, ct);

    if (msg is null)
        return;

    msg.Status = OutboxMessageStatus.Processed;
    msg.ProcessedAt = DateTime.UtcNow;

    await _dbContext.SaveChangesAsync(ct);
}
```

Failed processing increments the retry count:

```csharp
public async Task MarkAsFailedAsync(
    Guid messageId,
    string error,
    CancellationToken ct)
{
    var msg = await _dbContext.OutboxMessages
        .FindAsync(new object[] { messageId }, ct);

    if (msg is null)
        return;

    msg.Status = OutboxMessageStatus.Failed;
    msg.Error = error;
    msg.RetryCount++;

    await _dbContext.SaveChangesAsync(ct);
}
```

Dead-lettering updates the source message status:

```csharp
public async Task MarkAsDeadLetteredAsync(
    Guid messageId,
    CancellationToken ct)
{
    var msg = await _dbContext.OutboxMessages
        .FindAsync(new object[] { messageId }, ct);

    if (msg is null)
        return;

    msg.Status = OutboxMessageStatus.DeadLettered;

    await _dbContext.SaveChangesAsync(ct);
}
```

`GetPendingMessagesAsync` orders results by `OccurredAt` within each fetched batch and includes failed messages for retry. This is not a global FIFO guarantee: timestamps may tie, and processing order across concurrent workers or separate batches is not guaranteed.

#### 6.4.3 ProcessedEventRepository

The repository uses a composite primary key to enforce uniqueness for each event-handler pair.

```csharp
public async Task MarkAsProcessedAsync(
    Guid eventId,
    string handlerIdentifier,
    CancellationToken ct)
{
    if (await IsProcessedAsync(eventId, handlerIdentifier, ct))
        return;

    var processedEvent = new ProcessedEvent
    {
        EventId = eventId,
        HandlerIdentifier = handlerIdentifier,
        ProcessedAt = DateTime.UtcNow
    };

    try
    {
        await _dbContext.Set<ProcessedEvent>()
            .AddAsync(processedEvent, ct);

        await _dbContext.SaveChangesAsync(ct);
    }
    catch (DbUpdateException ex)
        when (IsUniqueConstraintViolation(ex))
    {
        _dbContext.Entry(processedEvent).State =
            EntityState.Detached;
    }
}

private static bool IsUniqueConstraintViolation(
    DbUpdateException ex)
{
    return ex.InnerException is SqlException sql
        && (sql.Number == 2601 || sql.Number == 2627);
}
```

SQL Server error numbers `2601` and `2627` represent duplicate-key or unique-constraint violations.

Only those recognized unique-constraint failures should be treated as a competing insert. Other database errors must propagate.

**Concurrency limitation:** swallowing a duplicate marker insert prevents the unique-key race from causing an unnecessary retry, but it does not prevent two workers from executing the same handler concurrently. The marker only exists after the handler succeeds. A claim or locking strategy is required if concurrent handler execution must be prevented.

### 6.5 EF Core Configurations

#### OutboxConfiguration

```csharp
public void Configure(EntityTypeBuilder<OutboxMessage> builder)
{
    builder.ToTable("OutboxMessages");

    builder.HasKey(m => m.Id);

    builder.Property(m => m.EventType)
        .IsRequired()
        .HasMaxLength(500);

    builder.Property(m => m.Payload)
        .IsRequired()
        .HasColumnType("nvarchar(max)");

    builder.Property(m => m.Error)
        .HasMaxLength(4000);

    builder.Property(m => m.Status)
        .HasConversion<string>()
        .IsRequired();

    builder.HasIndex(m => new { m.Status, m.OccurredAt })
        .HasDatabaseName("IX_OutboxMessages_Status_OccurredAt");
}
```

The `(Status, OccurredAt)` index supports the message-fetch query and can reduce the need for a table scan. Actual index usage depends on the query plan and data distribution.

#### DeadLetterMessageConfiguration

```csharp
public void Configure(
    EntityTypeBuilder<DeadLetterMessage> builder)
{
    builder.ToTable("DeadLetterMessages");

    builder.HasKey(e => e.Id);

    builder.Property(e => e.EventType)
        .IsRequired()
        .HasMaxLength(500);

    builder.Property(e => e.Payload)
        .IsRequired()
        .HasColumnType("nvarchar(max)");

    builder.Property(e => e.Error)
        .HasMaxLength(4000);

    builder.Property(e => e.Status)
        .HasConversion<string>()
        .IsRequired();

    builder.HasIndex(e => new { e.Status, e.DeadLetteredAt })
        .HasDatabaseName(
            "IX_DeadLetterMessages_Status_DeadLetteredAt");
}
```

#### ProcessedEventConfiguration

```csharp
public void Configure(
    EntityTypeBuilder<ProcessedEvent> builder)
{
    builder.ToTable("ProcessedEvents");

    builder.HasKey(e => new
    {
        e.EventId,
        e.HandlerIdentifier
    });

    builder.Property(e => e.HandlerIdentifier)
        .IsRequired()
        .HasMaxLength(500);
}
```

The composite primary key enforces per-handler uniqueness at the database level.

### 6.6 Scheduling

#### OutboxProcessingJob

The job is registered by `SchedulingInfrastructureExtensions`:

```csharp
services.AddScoped<OutboxProcessingJob>();

RecurringJob.AddOrUpdate<OutboxProcessingJob>(
    "outbox-processor",
    job => job.ExecuteAsync(JobCancellationToken.Null),
    Cron.Minutely);
```

`JobCancellationToken.Null` is a Hangfire convention used in recurring-job registration; Hangfire supplies the execution context when the job runs.

This is Infrastructure-owned scheduling, not a feature-local recurring-job trigger.

#### DeadLetterMonitorJob

```csharp
services.AddScoped<DeadLetterMonitorJob>();

RecurringJob.AddOrUpdate<DeadLetterMonitorJob>(
    "dead-letter-monitor",
    job => job.ExecuteAsync(JobCancellationToken.Null),
    Cron.Hourly);
```

These examples reflect direct Hangfire registration. If the project standardizes all scheduling through a scheduling gateway and recurring-job bootstrapper, the actual registration should follow that standard rather than bypass it.

### 6.7 Dependency Injection Registration

`OutboxInfrastructureExtensions.AddOutboxMessaging`:

```csharp
public static IServiceCollection AddOutboxMessaging(
    this IServiceCollection services)
{
    services.AddOptions<OutboxOptions>()
        .BindConfiguration(OutboxOptions.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();

    services.AddScoped<IOutboxMessageWriter, OutboxMessageWriter>();
    services.AddSingleton<OutboxSerializer>();
    services.AddScoped<OutboxDispatchService>();
    services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
    services.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();
    services.AddScoped<IDeadLetterRepository, DeadLetterRepository>();

    services.TryDecorate<
        IOutboxMessageWriter,
        EnrichedOutboxMessageWriter>();

    return services;
}
```

`OutboxMessageRepository` and `ProcessedEventRepository` are registered through `RepositoryRegistrationExtensions` and assembly scanning.

Verify whether repository scanning also registers `IDeadLetterRepository`. Avoid duplicate or unintended registrations; keep explicit registration if it is needed by the project's registration conventions.

---

## 7. Write Path — Transactional

This path transforms a business fact into a durable integration event.

```text
1. Command handler executes:
   └── aggregate.MarkPaid()
       ├── Status = Paid
       └── AddDomainEvent(new OrderPaidDomainEvent(Id, CustomerId))

2. Handler calls UnitOfWork.SaveChangesAsync(ct)

3. UnitOfWork:
   a. Opens a transaction if one is not already active.
   b. Save #1 — persists business state.
   c. Collects domain events from tracked BaseEntity instances.
   d. Dispatches each domain event to registered IDomainEventHandler<T> handlers.
      ├── Handler A: mutates another aggregate.
      └── Handler B: calls IOutboxMessageWriter.WriteAsync(integrationEvent).
          └── OutboxMessageWriter serializes the event and adds an OutboxMessage
              to the same DbContext.
   e. Clears dispatched domain events according to the unit-of-work lifecycle.
   f. Save #2 — persists handler changes and Outbox rows.
   g. Commits the transaction if the unit of work owns it.
   h. Runs post-commit callbacks.
```

### Atomicity

When all relevant writes use the same database transaction, a failure before commit rolls back the business state and Outbox rows together. The database cannot commit one of these changes without the other.

This guarantee depends on the actual transaction boundary: handlers must not use a separate database transaction for the Outbox write.

Domain events do not themselves enter the Outbox. They are in-memory objects collected from tracked aggregates, dispatched, and cleared according to the unit-of-work lifecycle. Only integration events written through `IOutboxMessageWriter` become Outbox rows.

A domain event handler may produce zero, one, or many integration events. The mapping is not necessarily one-to-one.

---

## 8. Delivery Path — Background

The delivery path begins when `OutboxProcessingJob.ExecuteAsync` runs on Hangfire's recurring schedule, currently `Cron.Minutely`.

```text
1. Fetch a batch:
   GetPendingMessagesAsync(batchSize: 20, ct)

   WHERE Status IN (Pending, Failed)
   ORDER BY OccurredAt

2. Process each message sequentially, checking cancellation between messages.

   If RetryCount >= MaxRetryCount:
     BEGIN TRANSACTION
       Insert DeadLetterMessage
       Mark OutboxMessage as DeadLettered
     COMMIT
     Continue to next message.

   Otherwise:
     Try:
       OutboxDispatchService.DispatchMessageAsync(message)
         ├── Resolve type from message.EventType.
         ├── Deserialize message.Payload.
         └── IntegrationEventDispatcher.DispatchAsync(integrationEvent)
               └── For each resolved handler:
                   CorrelationScope → Idempotency → ConcreteHandler

       MarkAsProcessedAsync(message.Id)
       Log success.

     Catch cancellation requested by the job:
       Rethrow; do not classify shutdown as a processing failure.

     Catch other exceptions:
       MarkAsFailedAsync(message.Id, ex.ToString())
       Log failure.

3. Log batch summary:
   processed / failed / dead-lettered counts.
```

### 8.1 Idempotency on Retry

If a message fails after some handlers have succeeded, their `ProcessedEvents` markers allow subsequent sequential attempts to skip those handlers. Handlers that failed or were not reached may run on retry.

This behavior depends on each successful handler's marker being committed before processing proceeds to the next handler.

It does not prevent concurrent workers from executing the same handler before either marker is committed, nor does it close the crash window between an external side effect and marker persistence.

### 8.2 Failure Isolation

Failures to persist a dead-letter record or to mark a message as failed should be logged and handled without unnecessarily preventing subsequent messages from being attempted.

The job must distinguish ordinary per-message failures from cancellation or shutdown. Cancellation should propagate so the scheduler can stop execution cleanly.

### 8.3 Batch Boundary

Messages within a batch are processed sequentially. This does not by itself prevent overlapping executions on different servers. Concurrency controls and the limitations of `DisableConcurrentExecution` are covered in §9.

---

## 9. Concurrency, Cancellation, and Atomicity

### 9.1 Concurrent Execution

`OutboxProcessingJob` is decorated with:

```csharp
[DisableConcurrentExecution(timeoutInSeconds: 120)]
```

This attribute uses Hangfire's distributed-lock mechanism backed by the configured storage provider. Its behavior and failure guarantees depend on that provider, lock lifetime, and deployment configuration. It should not be described as a universally reliable, per-process-only lock.

* **Single-server deployment:** it can prevent overlapping executions of the same recurring job when the configured storage and lock behavior work as expected.
* **Multi-server deployment:** a correctly configured shared Hangfire storage can coordinate this lock across servers, but it does not make the message-fetch-and-process sequence atomic. Duplicate processing remains possible if the lock expires, execution outlasts the timeout, another code path processes the same rows, or another worker bypasses the same lock.

The long-term solution for concurrent Outbox workers is an explicit claim mechanism: atomically select eligible rows and transition them to `Processing`, using suitable locking or conditional updates, then recover abandoned claims through a lease or timeout. The reserved `Processing` status exists for this future purpose.

Handler-level idempotency remains necessary even with message claiming.

### 9.2 Cancellation

The job accepts `IJobCancellationToken`:

```csharp
public async Task ExecuteAsync(
    IJobCancellationToken cancellationToken)
{
    var ct = cancellationToken.ShutdownToken;

    // ...
}
```

* Check `ct.ThrowIfCancellationRequested()` between messages.
* Pass `ct` to repository and dispatcher operations.
* Handle cancellation explicitly before the general exception handler:

```csharp
catch (OperationCanceledException) when (ct.IsCancellationRequested)
{
    throw;
}
catch (Exception ex)
{
    // Mark failed and log, subject to failure-handling policy.
}
```

Cancellation is not treated as an ordinary handler failure. However, the in-flight message's persisted state depends on where cancellation occurs. If a handler side effect succeeded but the processed marker did not commit, a later attempt may execute the handler again.

### 9.3 Dead-Letter Atomicity

`TryMoveToDeadLetterAsync` coordinates insertion of the dead-letter row and updating the source Outbox row inside an explicit transaction:

```csharp
private async Task TryMoveToDeadLetterAsync(
    OutboxMessage message,
    CancellationToken ct)
{
    _logger.LogWarning(
        "Moving outbox message {MessageId} to dead-letter after {RetryCount} retries",
        message.Id,
        message.RetryCount);

    var deadLetter = new DeadLetterMessage
    {
        // Populate from the source message.
    };

    await using var tx =
        await _dbContext.Database.BeginTransactionAsync(ct);

    try
    {
        await _deadLetterRepo.AddAsync(deadLetter, ct);

        await _outboxRepo.MarkAsDeadLetteredAsync(
            message.Id,
            ct);

        await tx.CommitAsync(ct);
    }
    catch (Exception ex)
    {
        await tx.RollbackAsync(ct);

        _logger.LogError(
            ex,
            "Failed to move outbox message {MessageId} to dead-letter",
            message.Id);
    }
}
```

Both repositories must share the same scoped `AppDbContext` and transaction connection for their `SaveChangesAsync` calls to participate in this transaction.

**Important:** this method logs and swallows the exception. The caller must not assume dead-lettering succeeded merely because the method returned. The source message must remain eligible for a later retry if the transaction failed, and the job's summary must count only confirmed state transitions.

### 9.4 Exception Isolation

The implementation should isolate failures by message:

* Dispatch and marking as processed: failure should enter the per-message failure path.
* Marking as failed: failure should be logged; do not pretend the retry count or status was successfully persisted.
* Dead-lettering: failure should be logged; the source message must not be reported as successfully quarantined.
* Cancellation and shutdown: propagate rather than treating them as ordinary processing errors.

The batch should continue after recoverable per-message failures, but not at the cost of hiding an unknown persisted state.

---

## 10. Type Resolution

Integration events are stored in the Outbox as a string (`EventType`) and resolved back to CLR types at dispatch time.

### 10.1 Storage Format

The writer stores the type's full name and assembly name:

```csharp
EventType =
    $"{eventType.FullName}, {eventType.Assembly.GetName().Name}";
```

Example:

```text
E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents.OrderPlacedIntegrationEvent, E-Commerce.Application
```

Assembly version and culture are omitted. This reduces coupling to assembly version changes, but does not make the contract independent of CLR type names or assembly names.

`Type.GetType` can resolve the type when the relevant assembly is available and the stored name is valid.

### 10.2 Resolution Strategy

```csharp
private static Type? ResolveEventType(string typeName)
{
    var direct = Type.GetType(typeName);

    if (direct is not null)
        return direct;

    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
    {
        var fromAssembly = assembly.GetType(typeName);

        if (fromAssembly is not null)
            return fromAssembly;
    }

    return null;
}
```

**Fast path:** `Type.GetType` attempts to resolve the supplied type name, including its assembly component when present.

**Fallback:** the application scans currently loaded assemblies. This can support legacy rows that contain only a fully qualified type name and types that remain available under a different assembly arrangement.

The fallback does not automatically load every assembly in the application. It only searches assemblies already loaded into the current `AppDomain`. The application assembly must be loaded before the fallback can find types in it.

If both paths fail, the method returns `null`, and `OutboxDispatchService` throws an `InvalidOperationException`. The message can then enter the retry and dead-letter workflow.

### 10.3 Known Limitations

* **Enum serialization:** `System.Text.Json` serializes enums as integers by default. Maintain stable numeric values or introduce string-enum serialization with a compatibility plan for existing payloads.
* **Value objects:** types with private constructors or custom construction requirements may not round-trip correctly. Use `[JsonConstructor]` or explicit converters where necessary.
* **Schema versioning:** the Outbox does not version payloads. Renaming or removing a property may cause older payloads to deserialize with missing or default values without throwing an exception.
* **Type renames and assembly moves:** existing rows can become undispatchable unless compatibility mappings or legacy type resolution are provided.

The current contract-evolution policy is to add optional fields only and avoid renaming, removing, or changing the meaning of existing fields. When breaking changes are unavoidable, introduce an explicit versioning or migration strategy.

---

## 11. Idempotency Model

### 11.1 Marker Design

Each handler's successful processing of an integration event is recorded independently.

* **Composite key:** `(EventId, HandlerIdentifier)`, enforced by the database primary key.
* **HandlerIdentifier:** the concrete handler's CLR type name, obtained from `inner.GetType().FullName` inside the idempotency decorator.

This relies on the idempotency decorator wrapping the concrete handler directly. If decorator registration changes, verify that the identifier still resolves to the intended stable handler identity.

A CLR type name is convenient but can change during refactoring. Renaming a handler can make existing markers appear to belong to a different handler, potentially causing old events to be processed again. For long-lived systems, an explicit stable handler identifier may be preferable.

### 11.2 Sequential Retry

Suppose handler A succeeds and handler B throws:

```text
Attempt 1:
  Handler A → succeeds → marker inserted (EventId, "HandlerA")
  Handler B → throws   → exception propagates
  Outbox job marks message Failed

Attempt 2:
  Handler A → marker exists → skipped
  Handler B → no marker     → executes again
  Handler B → succeeds      → marker inserted
  Outbox job marks message Processed
```

This is the intended sequential retry behavior, assuming each marker is committed and the same handler identity is used on both attempts.

It is not an exactly-once guarantee across concurrent workers or external systems.

### 11.3 Concurrent Insert Race

Two workers may both pass the `IsProcessedAsync` check before either has inserted a marker. Both may then attempt to insert the same `(EventId, HandlerIdentifier)` row.

The repository handles a duplicate-key insert by detaching the failed entity:

```csharp
try
{
    await _dbContext.Set<ProcessedEvent>()
        .AddAsync(processedEvent, ct);

    await _dbContext.SaveChangesAsync(ct);
}
catch (DbUpdateException ex)
    when (IsUniqueConstraintViolation(ex))
{
    _dbContext.Entry(processedEvent).State =
        EntityState.Detached;
}
```

SQL Server errors `2601` and `2627` are treated as duplicate-key or unique-constraint violations. Other database errors must propagate.

Detaching the entity prevents the failed insert from remaining in the change tracker.

**Effect:** a concurrent marker insert does not need to turn into an ordinary processing failure. However, this mechanism alone does not stop both workers from executing the concrete handler before attempting to insert the marker. It therefore does not guarantee single execution under concurrency.

### 11.4 The Mark-Then-Crash Window

The handler's external side effect and the marker insertion are not atomic across independent systems.

For example:

1. A handler successfully sends an email or calls an external HTTP API.
2. The process crashes before the processed-event marker commits.
3. The next attempt finds no marker.
4. The handler executes again, potentially repeating the side effect.

This is a fundamental limitation of sequential idempotency based on a post-execution marker.

Handlers with critical external side effects should use downstream idempotency keys, provider-supported deduplication, or another suitable mechanism. The Outbox decorator provides sequential per-handler deduplication; it does not provide an atomic commit across the local database and an independent external system.

---

## 12. Configuration

### 12.1 appsettings.json

```json
{
  "Outbox": {
    "MaxRetryCount": 5
  }
}
```

### 12.2 OutboxOptions

```csharp
public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    [Range(1, 100)]
    public int MaxRetryCount { get; set; } = 5;
}
```

### 12.3 Registration

```csharp
services.AddOptions<OutboxOptions>()
    .BindConfiguration(OutboxOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

`MaxRetryCount` is validated at startup. Values outside `[1, 100]` prevent the host from starting.

The job's comparison logic must define the threshold consistently. For example, checking `RetryCount >= MaxRetryCount` before another attempt means the precise number of actual executions depends on when `RetryCount` is incremented. Test this boundary explicitly to ensure the configured value matches the intended retry policy.

---

## 13. Database Schema

### 13.1 OutboxMessages

| Column        | Type                  | Description                                                                     |
| ------------- | --------------------- | ------------------------------------------------------------------------------- |
| `Id`          | `uniqueidentifier`    | Primary key                                                                     |
| `EventType`   | `nvarchar(500)`       | CLR type and assembly name used to resolve the integration event                |
| `Payload`     | `nvarchar(max)`       | JSON-serialized integration event                                               |
| `OccurredAt`  | `datetime2`           | Event occurrence time                                                           |
| `ProcessedAt` | `datetime2 NULL`      | Set when the message reaches `Processed`                                        |
| `RetryCount`  | `int`                 | Number of recorded failed attempts                                              |
| `Error`       | `nvarchar(4000) NULL` | Last recorded error                                                             |
| `Status`      | `nvarchar`            | String value: `Pending`, `Processing`, `Processed`, `Failed`, or `DeadLettered` |

**Index:** `IX_OutboxMessages_Status_OccurredAt` on `(Status, OccurredAt)` supports the pending-message query.

### 13.2 DeadLetterMessages

| Column              | Type                  | Description                                    |
| ------------------- | --------------------- | ---------------------------------------------- |
| `Id`                | `uniqueidentifier`    | Primary key                                    |
| `OriginalMessageId` | `uniqueidentifier`    | Soft reference to the source Outbox message    |
| `EventType`         | `nvarchar(500)`       | CLR type and assembly name                     |
| `Payload`           | `nvarchar(max)`       | Original JSON payload                          |
| `Error`             | `nvarchar(4000) NULL` | Last recorded error before quarantine          |
| `RetryCount`        | `int`                 | Recorded attempts before quarantine            |
| `DeadLetteredAt`    | `datetime2`           | Quarantine timestamp                           |
| `Status`            | `nvarchar`            | String value: `DeadLettered` or `Reprocessing` |

**Index:** `IX_DeadLetterMessages_Status_DeadLetteredAt` on `(Status, DeadLetteredAt)` supports status-based queries and monitoring.

### 13.3 ProcessedEvents

| Column              | Type               | Description                  |
| ------------------- | ------------------ | ---------------------------- |
| `EventId`           | `uniqueidentifier` | Integration event identifier |
| `HandlerIdentifier` | `nvarchar(500)`    | Handler identifier           |
| `ProcessedAt`       | `datetime2`        | Handler success timestamp    |

**Primary key:** composite `(EventId, HandlerIdentifier)`.

---

## 14. Logging and Correlation

### 14.1 Log Coverage

| Event                            | Level                                            | Location                                       |
| -------------------------------- | ------------------------------------------------ | ---------------------------------------------- |
| Integration event written        | Information                                      | `EnrichedOutboxMessageWriter`                  |
| Non-empty batch fetched          | Debug                                            | `OutboxProcessingJob`                          |
| Handler invocation               | Debug                                            | `CorrelationScopeIntegrationEventHandler`      |
| Message processed                | Depends on implementation; typically Information | `OutboxProcessingJob`                          |
| Message failed                   | Error                                            | `OutboxProcessingJob.ProcessMessageAsync`      |
| Failure to persist failed status | Error                                            | `OutboxProcessingJob.ProcessMessageAsync`      |
| Message moved to dead letter     | Warning                                          | `OutboxProcessingJob.TryMoveToDeadLetterAsync` |
| Dead-letter operation failed     | Error                                            | `OutboxProcessingJob.TryMoveToDeadLetterAsync` |
| Batch summary                    | Information                                      | `OutboxProcessingJob.ExecuteAsync`             |
| Dead-letter backlog detected     | Warning                                          | `DeadLetterMonitorJob`                         |

The actual implementation should ensure that the batch summary reports confirmed outcomes rather than merely attempted operations.

### 14.2 Correlation Propagation

The correlation ID flows from the originating HTTP request into log entries concerning the resulting integration event:

1. `CorrelationIdMiddleware` sets the correlation ID in `HttpContext.Items` and pushes it into the request's logging scope.
2. A domain event handler reads the ID through `IAppContext` and assigns it to the integration event. Background jobs may have no originating HTTP request, so the value can be null.
3. `OutboxMessageWriter` serializes the integration event, including its correlation ID.
4. During dispatch, `CorrelationScopeIntegrationEventHandler` pushes the ID into a logging scope before invoking the idempotency decorator and concrete handler.
5. Downstream logs inherit the correlation ID when they use the same logging infrastructure and execution context.

This allows a request and its asynchronous side effects to be correlated across the Outbox boundary. Correlation identifies related operations; it is not itself an idempotency key.

---

## 15. Testing Guidance

### 15.1 Domain-Level

* Test `BaseEntity.AddDomainEvent` and `ClearDomainEvents`.
* Verify that aggregates raise the appropriate domain events at the correct points in their state transitions.

### 15.2 Application-Level

* **`IdempotentIntegrationEventHandler<T>`:** verify skip-on-processed behavior and marker creation after success.
* **`CorrelationScopeIntegrationEventHandler<T>`:** verify correlation scope creation when an ID is present and no scope when it is absent.
* **Concrete handlers:** verify business logic and domain invariants without adding duplicate idempotency logic unnecessarily.

### 15.3 Infrastructure-Level

* **`OutboxMessageWriter`:** verify stored `EventType` formatting and preservation of `OccurredAt`.
* **`OutboxSerializer`:** round-trip every integration-event type, including value objects and enums.
* **`DomainEventDispatcher`:** verify correct handler resolution, invocation, and asynchronous completion.
* **`IntegrationEventDispatcher`:** verify that the decorated handler pipeline is resolved for each event type.
* **`OutboxDispatchService`:** test assembly-qualified resolution, fallback resolution, unknown types, and deserialization failures.
* **`ProcessedEventRepository`:** verify unique-key handling and behavior under concurrent marker inserts.
* **`OutboxMessageRepository`:** verify that pending and failed messages are returned in `OccurredAt` order and other statuses are excluded.
* **`DeadLetterRepository`:** test insertion, querying, status changes, and deletion.
* **`UnitOfWork`:** verify the two-phase save and atomic persistence of business state and Outbox rows.

### 15.4 Integration-Level (Real Database)

* Verify EF Core indexes, composite-key enforcement, and string status conversion.
* Inject a failure between Save #1 and Save #2 and verify that the transaction leaves neither the business changes nor Outbox rows committed.
* Verify SQL Server duplicate-key handling for errors `2601` and `2627`.
* Test pending-message queries against realistic data volumes and inspect the query plan to confirm that the intended index is useful.

### 15.5 End-to-End

* Business operation → Outbox row persists → background job dispatches → handler executes → marker is recorded → message becomes `Processed`.
* Unknown event type or invalid payload → retry policy → dead-lettering after the configured threshold.
* Duplicate delivery of an event to a handler that already has a committed marker → handler is skipped during sequential processing.
* Correlation ID propagation across the request and background-processing boundary.
* A message with multiple subscribers where one succeeds and another fails → successful subscriber is skipped on sequential retry.
* Dead-letter reprocessing → previously successful handlers are skipped when their markers are retained and their handler identifiers remain stable.

---

## 16. Operational Notes

* **Scheduling:** `OutboxProcessingJob` runs every minute and `DeadLetterMonitorJob` hourly. The implementation should follow the project's scheduling conventions for Infrastructure-owned recurring jobs.
* **Batch size:** currently a constant of `20` in `OutboxProcessingJob`. Tuning requires a code change unless the value becomes configurable.
* **Backlog visibility:** no dedicated backlog metric is emitted by the described implementation. Batch-summary logs and `OutboxBacklogHealthCheck` are the current signals.
* **Pause / kill switch:** no feature flag is described for halting Outbox processing independently of Hangfire.
* **Retention:** no cleanup job is described for processed Outbox rows, processed-event markers, or dead-letter rows. These tables can grow indefinitely without a retention policy.
* **Reprocessing:** currently manual. A possible approach is to create a new `OutboxMessage` from the dead-letter payload with a new message ID and reset retry state, while preserving the original event's `EventId` in the payload. The dead-letter record can then be marked `Reprocessing` or removed according to the operational policy. Reprocessing must account for existing processed-event markers and the possibility of repeated external side effects.
* **Alerting:** the monitor job logs a warning when dead-lettered messages exist. Routing alerts to email, Slack, PagerDuty, or another system is outside the described implementation.
* **Security:** Outbox payloads are stored in the database and may contain sensitive business data. Keep payloads minimal, avoid secrets and unnecessary personal data, and protect database access, backups, logs, and diagnostic tooling appropriately.
* **Schema evolution:** integration-event contracts are persisted data contracts. Renames, removals, assembly moves, and semantic changes require a compatibility plan for messages that have already been stored.

---

## 17. Summary

The Outbox feature coordinates transactional event persistence and asynchronous dispatch within the E-Commerce Modular Monolith.

Its core mechanisms are:

* **Two-phase unit-of-work save:** business state and resulting Outbox rows can commit atomically within the same database transaction.
* **Integration-event persistence:** the writer stores event type information, serialized payload, occurrence time, and processing status.
* **Background dispatch:** a recurring job loads eligible messages and invokes registered integration-event handlers.
* **Per-handler idempotency markers:** a composite primary key records successful handler processing and supports deduplication on sequential retries.
* **Retry and dead-letter handling:** failed messages are retried according to the configured threshold and eventually quarantined when eligible.
* **Cancellation and failure isolation:** the job distinguishes shutdown from ordinary processing failures and handles messages individually.
* **Structured logging and correlation:** correlation information can be propagated from an originating request into asynchronous handler execution.
* **Validated configuration:** invalid retry settings prevent application startup.

The documented operational limitations include the absence of an explicit atomic message-claim mechanism, automated dead-letter reprocessing, retention policies, a dedicated backlog metric, and an Outbox-specific kill switch.

### Guarantee boundary

**Once the business transaction commits, the resulting integration-event records are durably stored for background processing. The system attempts delivery with retries and uses per-handler markers to suppress repeated execution during sequential retries.**

This does **not** guarantee exactly-once execution or unconditional eventual delivery. Delivery can remain blocked by persistent failures, incompatible event contracts, unavailable dependencies, or disabled processing. Concurrent execution and crashes between external side effects and marker persistence can also cause duplicate effects.

The Outbox prevents the business transaction from depending on synchronous external delivery. Reliable operation still requires compatible event contracts, appropriate retry and dead-letter handling, operational monitoring, and idempotent consumers.

---

**Document version:** 1.0 — Final Outbox Technical Reference

This document describes the Outbox folder structure, class responsibilities, write and delivery pipelines, type resolution, idempotency model, configuration, database schema, logging, testing guidance, and operational limitations for the transactional Outbox pipeline in the E-Commerce Modular Monolith.
