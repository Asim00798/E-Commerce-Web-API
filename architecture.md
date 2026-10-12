# E-Commerce — Architecture at a Glance

**Project:** E-Commerce Modular Monolith
**Version:** 1.0 – Final
**Location:** `/architecture.md`

**Purpose:** A concise, one-page-style reference to the system's architecture: project boundaries, dependency direction, runtime communication, architectural styles, DDD concepts, and design patterns.

**Full reference:** [`docs/architecture/solution-overview.md`](./docs/architecture/solution-overview.md)

---

## 1. THE SYSTEM

The E-Commerce solution is a **production-oriented modular monolith** built on .NET 8 and ASP.NET Core.

It consists of five projects, one deployable application process, and one SQL Server database. Background execution is provided by two distinct asynchronous pipelines—**Outbox** and **Scheduling**—that share a Hangfire runtime.

| Characteristic              | Architectural Decision                                                                       |
| --------------------------- | -------------------------------------------------------------------------------------------- |
| Application topology        | Modular monolith                                                                             |
| Deployment model            | Single application deployment                                                                |
| Runtime                     | .NET 8 / ASP.NET Core                                                                        |
| Projects                    | Five                                                                                         |
| Primary database            | SQL Server                                                                                   |
| Persistence                 | EF Core for transactional persistence; ADO.NET and stored procedures for specialized queries |
| Background execution        | Hangfire                                                                                     |
| Reliable integration events | Transactional Outbox                                                                         |
| Read optimization           | Selective CQRS through `E-Commerce.ReadModel`                                                |
| Domain modeling             | Domain-Driven Design                                                                         |
| Dependency management       | Clean Architecture principles and Dependency Inversion                                       |

**Core principle:** Preserve clear module boundaries and reliable execution without introducing distributed infrastructure unless a concrete requirement justifies it.

---

## 2. THE FIVE PROJECTS

Each project has a distinct architectural responsibility.

| Layer          | Project                     | Responsibility                                                                                                                                                               |
| -------------- | --------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Domain         | `E-Commerce.Domain`         | Aggregates, entities, value objects, domain events, domain rules, and core abstractions                                                                                      |
| Application    | `E-Commerce.Application`    | Use cases, CQRS handlers, application orchestration, integration-event contracts, and application-facing abstractions                                                        |
| Infrastructure | `E-Commerce.Infrastructure` | EF Core persistence, repository implementations, Outbox processing, Scheduling adapters, Identity integration, notifications, file storage, and payment-provider integration |
| Read Model     | `E-Commerce.ReadModel`      | Specialized read queries implemented with ADO.NET and DbUp-managed stored procedures                                                                                         |
| API            | `E-Commerce.Api`            | HTTP endpoints, middleware, dependency-injection composition, host configuration, and startup orchestration                                                                  |

### Ownership Rule

* **Domain** defines business rules and domain behavior.
* **Application** coordinates use cases and defines application-facing contracts.
* **Infrastructure** implements persistence and external-system integrations.
* **ReadModel** implements specialized query paths.
* **API** exposes the application and composes the runtime.

No project should assume another project's responsibilities merely because it can technically access its types.

---

## 3. PROJECT REFERENCE DIRECTION

Dependencies are constrained by project references.

The architecture follows an inward dependency direction: outer layers may depend on inner layers, but inner layers must not depend on outer layers.

### Reference Matrix

Rows represent the referencing project; columns represent the referenced project.

| From ↓ / To →      | Domain | Application | Infrastructure | ReadModel | API |
| ------------------ | -----: | ----------: | -------------: | --------: | --: |
| **Domain**         |      — |           — |              — |         — |   — |
| **Application**    |      ✓ |           — |              — |         — |   — |
| **Infrastructure** |      ✓ |           ✓ |              — |         — |   — |
| **ReadModel**      |      — |           ✓ |              — |         — |   — |
| **API**            |      — |           ✓ |              ✓ |         ✓ |   — |

### Enforced Rules

1. **Domain references nothing** from the other solution projects.
2. **Application references Domain only.**
3. **Infrastructure references Application and Domain.**
4. **ReadModel references Application only.**
5. **API references Application, Infrastructure, and ReadModel.**
6. **Infrastructure and ReadModel never reference each other.**
7. **No project references API.**

These restrictions are enforced through `.csproj` project references.

**Important distinction:** Dependency direction describes compile-time references. Runtime execution may flow through interfaces, dependency injection, and infrastructure implementations without reversing the project-reference direction.

---

## 4. RUNTIME COMMUNICATION

The projects collaborate through explicit contracts and controlled execution paths.

| Mechanism                  | Runtime Flow                                                                   | Purpose                                           |
| -------------------------- | ------------------------------------------------------------------------------ | ------------------------------------------------- |
| Dependency Injection       | Consumer → Interface → Resolved implementation                                 | Resolves abstractions to concrete services        |
| MediatR                    | Controller → `IMediator` → Application handler                                 | Dispatches application commands and queries       |
| Transactional Outbox       | Business transaction → Outbox record → Background dispatcher                   | Reliably delivers integration events after commit |
| Specialized query services | Application query handler → Query-service interface → ReadModel implementation | Executes optimized queries where justified        |
| Background scheduling      | Application feature → `IJobScheduler` → Hangfire adapter                       | Enqueues, delays, and schedules background work   |

### Communication Rules

* Application-facing contracts are declared in Application or, where appropriate, Domain.
* Infrastructure implements persistence and external-system adapters.
* ReadModel implements specialized query services.
* API composes the application's runtime through dependency injection.
* Application code does not directly depend on concrete infrastructure providers.
* Cross-context business communication uses explicit contracts rather than direct access to another context's internal implementation.

**Design objective:** Keep compile-time dependencies explicit while allowing runtime implementations to evolve independently.

---

## 5. ARCHITECTURAL STYLES

The solution combines several complementary architectural styles.

| Style                                     | Application in This Solution                                                                        |
| ----------------------------------------- | --------------------------------------------------------------------------------------------------- |
| **Modular Monolith**                      | One deployable application with logically separated business contexts and modules                   |
| **Layered Architecture**                  | Five projects with explicit responsibilities                                                        |
| **Clean Architecture / Onion Principles** | Domain and Application remain independent of Infrastructure                                         |
| **Ports and Adapters**                    | Application-facing abstractions are implemented by infrastructure adapters                          |
| **Domain-Driven Design**                  | Business modeling through bounded contexts, aggregates, value objects, and domain events            |
| **Event-Driven Architecture**             | Domain events for in-process business reactions; integration events for cross-context communication |
| **Selective CQRS**                        | Separate command and query responsibilities, with specialized read models only when justified       |
| **Dependency Inversion**                  | High-level policies depend on abstractions rather than concrete providers                           |
| **Shared Database with Schema Isolation** | One SQL Server database with schema separation for bounded contexts                                 |

These styles reinforce one another rather than representing competing architectural choices.

---

## 6. DOMAIN-DRIVEN DESIGN

DDD is used to structure business complexity and establish explicit ownership boundaries.

### 6.1 Strategic Design

| Concept                   | Application                                                                                                                                       |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Bounded Contexts**      | Catalog, Orders, Finance, Shipping, CustomerEngagement, Onboarding, People                                                                        |
| **Ubiquitous Language**   | Types, operations, and rules reflect the business vocabulary of each context                                                                      |
| **Context Boundaries**    | Each context owns its business rules and authoritative state                                                                                      |
| **Anti-Corruption Layer** | Integration boundaries translate external or foreign models where required                                                                        |
| **Shared Kernel**         | Deliberately shared foundational abstractions, such as `BaseEntity`, `IAggregateRoot`, `IDomainEvent`, `IRepository`, `IUnitOfWork`, and `IClock` |
| **Technical Modules**     | Authentication, Authorization, Identity, Notifications, Profiles, and Scheduling                                                                  |

**Note:** Technical modules are not automatically business bounded contexts. Their classification depends on their domain responsibility, not simply their folder or project location.

### 6.2 Tactical Design

| Concept                            | Application                                                                                           |
| ---------------------------------- | ----------------------------------------------------------------------------------------------------- |
| **Aggregates and Aggregate Roots** | Enforce business invariants within consistency boundaries                                             |
| **Entities**                       | Objects with identity, including child entities such as `CartItem`, `OrderItem`, and `ProductVariant` |
| **Value Objects**                  | Identity-less domain concepts such as `Money`, `Email`, `Address`, and `BrandLogo`                    |
| **Domain Events**                  | Represent facts that have occurred within the domain                                                  |
| **Repositories**                   | Abstract aggregate persistence; implementations live in Infrastructure                                |
| **Factories**                      | Aggregate creation through controlled factory methods                                                 |
| **Unit of Work**                   | Coordinates atomic persistence of business state and associated Outbox records                        |
| **Domain Exceptions**              | Represent violated business rules and invalid domain operations                                       |

### Ownership Principle

An aggregate protects its invariants. A bounded context owns its business model. Cross-context communication must respect context boundaries.

---

## 7. EVENT-DRIVEN DESIGN

The system distinguishes **domain events** from **integration events** by their purpose, lifetime, and delivery requirements.

### 7.1 Event Comparison

| Property                 | Domain Events                                           | Integration Events                                                              |
| ------------------------ | ------------------------------------------------------- | ------------------------------------------------------------------------------- |
| Purpose                  | Represent domain facts and trigger in-process reactions | Communicate business facts across context boundaries                            |
| Scope                    | In-process                                              | Cross-context                                                                   |
| Transaction relationship | Processed as part of the originating business operation | Persisted in the Outbox within the originating transaction                      |
| Durability               | Not inherently durable as independent messages          | Durable while stored in the Outbox and subject to delivery and retention policy |
| Delivery                 | In-process dispatch                                     | Asynchronous Outbox processing                                                  |
| Failure handling         | Depends on transaction and dispatch semantics           | Retries, idempotency, and dead-letter handling                                  |
| Persistence              | Not necessarily persisted independently                 | Persisted in the Outbox before delivery                                         |

### 7.2 Integration Mechanisms

| Mechanism                        | Responsibility                                                                 |
| -------------------------------- | ------------------------------------------------------------------------------ |
| **Transactional Outbox**         | Atomically commits business state and integration-event records                |
| **Idempotent Consumer**          | Prevents duplicate processing through explicit deduplication                   |
| **Dead Letter Channel**          | Quarantines messages that exceed the configured retry policy                   |
| **Correlation Identifier**       | Connects related operations across asynchronous execution paths                |
| **Event-Carried State Transfer** | Carries the information consumers need in an event payload when appropriate    |
| **Choreography**                 | Allows contexts to react independently without a central workflow orchestrator |
| **Post-Commit Callbacks**        | Executes eligible effects only after the transaction has committed             |

### 7.3 Outbox Execution Flow

```text
Business Operation
       │
       ▼
Domain State Change
       │
       ▼
Integration Event Created
       │
       ▼
Business State + Outbox Record
       │
       ▼
Single Database Transaction
       │
       ▼
Commit
       │
       ▼
Outbox Processing Job
       │
       ▼
Dispatch Integration Event
       │
       ▼
Idempotent Consumer
       │
       ▼
Receiving Bounded Context
```

The Outbox provides reliable message publication from the database transaction. It does not, by itself, guarantee exactly-once delivery or exactly-once consumer execution.

**Delivery may be repeated. Consumers must handle duplicates safely.**

---

## 8. CQRS AND THE READ MODEL

CQRS separates command responsibilities from query responsibilities without requiring separate services, databases, or event sourcing.

### Current Approach

* Commands and queries are separated by convention and folder structure.
* Application handlers coordinate use cases.
* EF Core serves ordinary persistence and query needs.
* `E-Commerce.ReadModel` serves specialized read requirements.
* ADO.NET and stored procedures are used where direct SQL provides a justified advantage.
* Aggregates are persisted as current state rather than event streams.

### Read-Path Decision

| Read Requirement                                             | Preferred Approach                |
| ------------------------------------------------------------ | --------------------------------- |
| Ordinary queries over the existing persistence model         | EF Core                           |
| Complex SQL or specialized joins                             | ReadModel when justified          |
| Cross-context projections                                    | ReadModel when justified          |
| Latency-sensitive queries requiring specialized optimization | ReadModel when justified          |
| Every query by default                                       | **Not** a reason to use ReadModel |

### Architectural Boundaries

* CQRS does not require separate databases.
* CQRS does not require microservices.
* CQRS does not imply event sourcing.
* The ReadModel is an optimization boundary, not a mandatory layer for every query.

**Principle:** Use the simplest query path that satisfies correctness, maintainability, and performance requirements.

---

## 9. DESIGN PATTERNS IN USE

The following patterns appear in the architecture. Some are classic object-oriented patterns; others are integration, resilience, or application-architecture patterns.

### 9.1 Object-Oriented and Application Patterns

| Pattern                          | Application                                                                          |
| -------------------------------- | ------------------------------------------------------------------------------------ |
| **Mediator**                     | MediatR dispatch                                                                     |
| **Command / Query**              | Explicit application request types                                                   |
| **Chain of Responsibility**      | MediatR behaviors and job-policy composition                                         |
| **Decorator**                    | Integration-event handler decorators and Outbox message-writer enrichment            |
| **Strategy**                     | Job policies, payment gateways, notification transports, file-storage providers      |
| **Factory**                      | Aggregate factory methods                                                            |
| **Adapter**                      | Paymob, SMTP, Twilio, Firebase, local storage, Hangfire                              |
| **Facade**                       | ReadModel query services                                                             |
| **Observer / Publish-Subscribe** | Domain-event and integration-event dispatch mechanisms                               |
| **Proxy / Dispatch Adapter**     | `HangfireJobDispatcher`, which bridges Hangfire execution to the application gateway |
| **Null Object**                  | `NullSpan`                                                                           |
| **Repository**                   | Domain/application contracts with Infrastructure implementations                     |
| **Unit of Work**                 | `IUnitOfWork` and transactional persistence                                          |

**Pattern classification note:** Not every similarly named component is automatically a formal GoF pattern. For example, `IJobHandler<TJob>` is primarily a handler contract; whether it implements Template Method depends on whether a base class defines an algorithm with overridable steps.

### 9.2 Integration Patterns

* Transactional Outbox
* Idempotent Consumer
* Dead Letter Channel
* Correlation Identifier
* Competing Consumers through the Hangfire worker pool
* Event-Carried State Transfer where appropriate
* Choreography across bounded contexts

### 9.3 Resilience Patterns

| Pattern              | Implementation                                                                                 |
| -------------------- | ---------------------------------------------------------------------------------------------- |
| Timeout              | `TimeoutPolicy`                                                                                |
| Circuit Breaker      | `CircuitBreakerPolicy`; current limitations are documented in the Scheduling feature reference |
| Retry with Backoff   | Hangfire retry configuration                                                                   |
| Compensating Action  | Operations such as `RestoreStockOnExpiredOrders` and `ProcessRefund`                           |
| Idempotent Operation | File deletion and Outbox processing                                                            |

**Important:** The current scoped `CircuitBreakerPolicy` does not maintain a failure counter across separate job executions. Its present behavior must not be treated as a fully functioning cross-execution circuit breaker.

### 9.4 Dependency Injection and Application Conventions

* **Constructor Injection** — Services receive their dependencies explicitly.
* **Options Pattern** — Configuration is represented through typed options where used.
* **Startup Validation** — Configuration and service requirements are validated where configured.
* **Result Pattern** — Application handlers return `Result<T>` where appropriate.
* **Ambient Context** — `IAppContext` and `ICurrentUser` expose contextual information through abstractions.

### 9.5 Structural and Organizational Patterns

* **Vertical Organization** — Code is grouped by bounded context and feature.
* **Feature-Local Jobs** — Job payloads, handlers, and triggers live with the features they serve.
* **Assembly Scanning** — Handlers, triggers, and supported repositories are discovered automatically.
* **Explicit Startup Discovery** — `RecurringJobBootstrapper` discovers and registers recurring triggers at startup.

---

## 10. BACKGROUND EXECUTION

The system has two distinct asynchronous execution responsibilities.

| Pipeline       | Purpose                                                    | Primary Mechanism                      |
| -------------- | ---------------------------------------------------------- | -------------------------------------- |
| **Outbox**     | Reliable delivery of integration events                    | Outbox records + `OutboxProcessingJob` |
| **Scheduling** | Enqueue, delay, and recurring execution of background work | `IJobScheduler` + Hangfire             |

Both pipelines use the Hangfire runtime, but they solve different problems.

### Scheduling Execution Path

```text
Feature
   │
   ▼
IJobScheduler
   │
   ▼
Hangfire Adapter
   │
   ▼
Hangfire Storage and Worker
   │
   ▼
HangfireJobDispatcher
   │
   ▼
JobOrchestrator
   │
   ▼
Policies and Handler
```

### Architectural Rules

* Business features define their own jobs and handlers.
* Scheduling owns the execution gateway, policy composition, and recurring-trigger discovery.
* Application code does not directly reference Hangfire.
* Hangfire owns job persistence, retries, and failed-job storage.
* Background handlers must not assume an authenticated HTTP request exists.
* Business state changes and their corresponding integration events must use the Outbox transaction when atomic publication is required.

**Scheduling executes work. The Outbox reliably delivers integration events. Neither replaces the other.**

---

## 11. WHERE TO GO NEXT

This document is the entry point to the architecture documentation.

| Goal                             | Document                                                                             |
| -------------------------------- | ------------------------------------------------------------------------------------ |
| Full architectural reference     | [`docs/architecture/solution-overview.md`](./docs/architecture/solution-overview.md) |
| Bounded-context map              | [`docs/architecture/context-map.md`](./docs/architecture/context-map.md)             |
| Architecture documentation index | [`docs/architecture/README.md`](./docs/architecture/README.md)                       |
| Architecture Decision Records    | [`docs/decisions/README.md`](./docs/decisions/README.md)                             |
| Feature documentation index      | [`docs/business-features/`](./docs/business-features/)                               |
| Technical capabilities index     | [`docs/technical-capabilities/`](./docs/technical-capabilities/)                     |
| Local development setup          | [`setup-guide.md`](./setup-guide.md)                                                 |

---

## FINAL ARCHITECTURAL SUMMARY

The E-Commerce solution is a **modular monolith organized around explicit business boundaries and inward-facing dependencies**.

Its architecture combines:

* Five projects with enforced reference direction.
* Domain-Driven Design for business modeling.
* Clean Architecture and Dependency Inversion for dependency control.
* Domain and integration events for appropriately scoped event-driven communication.
* A transactional Outbox for reliable integration-event publication.
* Selective CQRS for specialized query requirements.
* Hangfire-backed Scheduling for durable background execution.
* Explicit abstractions for infrastructure and external providers.

The architecture deliberately avoids unnecessary distribution.

Its primary objective is to maintain clear ownership, reliable execution, testable business behavior, and controlled evolution while preserving the operational simplicity of a single deployable application.

| Architectural Dimension  | Decision                   |
| ------------------------ | -------------------------- |
| **Architecture**         | Modular Monolith           |
| **Dependency Rule**      | Inward toward Domain       |
| **Business Ownership**   | Bounded Contexts           |
| **Reliable Integration** | Transactional Outbox       |
| **Background Execution** | Scheduling + Hangfire      |
| **Read Optimization**    | Selective CQRS             |
| **Deployment Model**     | Single Application Process |

**Document Version:** 1.0 – Final
**Location:** `/architecture.md`
