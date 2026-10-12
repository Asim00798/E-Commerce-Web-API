# ADR-0002: Clean Architecture Dependency Direction

* **Status:** Accepted
* **Decision Type:** Architecture
* **Date:** 2026-10-13

## Context

The E-Commerce modular monolith contains several architectural layers with different responsibilities:

* API handles HTTP requests and transport-specific concerns.
* Application coordinates use cases and defines application-level contracts.
* Domain models business concepts, invariants, and domain behavior.
* Infrastructure implements persistence, framework integrations, and external technical mechanisms.
* ReadModel provides specialized read-side access where query requirements justify a dedicated implementation.

Without explicit dependency rules, higher-level business logic can become coupled to databases, web frameworks, identity providers, background-job libraries, and other implementation details.

Such coupling makes business behavior harder to isolate, infrastructure changes more invasive, and architectural boundaries more difficult to maintain.

The project therefore requires a consistent dependency direction that protects business logic while allowing Infrastructure to implement the technical mechanisms required by the application.

## Decision

The system adopts **Clean Architecture dependency direction**.

Dependencies must point toward inner layers and abstractions. Infrastructure implements interfaces defined by inner layers rather than requiring those layers to depend on infrastructure-specific implementations.

The principal dependency direction is:

**API → Application → Domain**

Infrastructure implements the abstractions required by Application and, where appropriate, Domain. API and Infrastructure may both reference Application, but Infrastructure-specific implementation details must not leak into Application or Domain.

The ReadModel is a specialized subsystem whose dependencies must be evaluated according to its dedicated read-side responsibilities. It must not become an unrestricted dependency of the Domain layer.

### 1. API Layer

The API layer is responsible for exposing application functionality through HTTP endpoints and other explicitly supported transport mechanisms.

Its responsibilities include:

* Routing and endpoint definitions.
* HTTP request and response handling.
* Request validation appropriate to the transport boundary.
* Authentication and endpoint-level authorization integration.
* Mapping application outcomes to HTTP responses.
* Transport-specific concerns such as status codes and headers.

The API invokes Application use cases through their public contracts.

It must not implement business rules that belong to Domain or bypass Application orchestration by directly manipulating persistence implementations.

### 2. Application Layer

The Application layer coordinates use cases and defines the contracts required to execute them.

Its responsibilities include:

* Command and query handling.
* Application-level orchestration.
* Use-case authorization.
* Transaction and Unit of Work coordination.
* Defining abstractions for persistence and external technical services.
* Coordinating domain-event and integration-event processing.
* Defining job contracts and other application-facing interfaces where appropriate.

Application may depend on Domain and on framework-independent or application-appropriate abstractions.

It must not depend directly on Infrastructure implementations such as EF Core repositories, SQL Server-specific persistence classes, Hangfire APIs, Firebase SDKs, or concrete external-service clients.

When a use case requires a technical operation, Application defines or consumes an appropriate abstraction, and Infrastructure supplies the implementation.

### 3. Domain Layer

The Domain layer owns business concepts, invariants, and domain behavior.

Depending on the business model, it may contain:

* Aggregates and aggregate roots.
* Entities and value objects.
* Domain services.
* Domain events.
* Domain-specific policies and rules.

The Domain layer must remain independent of application frameworks and infrastructure implementations.

It must not reference:

* ASP.NET Core.
* ASP.NET Core Identity.
* EF Core.
* Hangfire.
* Redis client implementations.
* External messaging or notification SDKs.
* SQL Server-specific persistence mechanisms.

Domain models should express business meaning rather than technical implementation details.

For example, Ordering determines whether an order can transition to a particular state. Infrastructure persists that state, but the database implementation does not define the business invariant.

### 4. Infrastructure Layer

Infrastructure implements technical mechanisms required by Application and, where applicable, Domain.

Its responsibilities include:

* EF Core and SQL Server persistence.
* Repository and Unit of Work implementations.
* ASP.NET Core Identity persistence and account-management infrastructure.
* Authentication and token-generation mechanisms.
* External payment, notification, and storage integrations.
* Outbox persistence and dispatch infrastructure.
* Hangfire job execution and scheduling integration.
* Caching, observability, and other technical capabilities.

Infrastructure may reference Application and Domain to implement their contracts and persist their models.

It must not become the owner of business rules simply because those rules require persistence or external services.

Business decisions remain with Domain or the appropriate Application use case, while Infrastructure executes the required technical operations.

### 5. ReadModel

The ReadModel is a specialized read-side subsystem used when query complexity, aggregation, reporting requirements, or performance characteristics justify a dedicated implementation.

It may use direct SQL and ADO.NET for optimized read-side queries.

The ReadModel must remain read-oriented and must not become an alternative path for modifying authoritative business state.

Its dependencies must be explicit and limited to what its read-side responsibilities require. It must not introduce dependencies from Domain onto query infrastructure.

The ReadModel is selective rather than mandatory: ordinary queries may remain in Application and use the existing persistence abstractions when a dedicated read-side implementation is unnecessary.

## Dependency Rules

The following table summarizes the intended dependency boundaries.

| Layer          | May depend on                                                             | Must not depend on                                                                                           |
| -------------- | ------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| API            | Application contracts and required API/framework libraries                | Infrastructure implementations for direct use-case execution; Domain for bypassing Application orchestration |
| Application    | Domain and appropriate application-level abstractions                     | Concrete Infrastructure implementations and infrastructure-specific SDKs                                     |
| Domain         | Domain-level types and framework-independent abstractions where justified | Application, API, Infrastructure, and external infrastructure frameworks                                     |
| Infrastructure | Application contracts, Domain models, and required technical frameworks   | Ownership of business rules that belong to Domain or Application                                             |
| ReadModel      | Explicit read-side contracts and required query infrastructure            | Domain-to-ReadModel dependencies or authoritative business-state mutation                                    |

These rules describe architectural intent. Exact project references must be aligned with the actual solution structure, and additional automated checks may be required to enforce the rules consistently.

## Dependency Inversion

The project applies the Dependency Inversion Principle where higher-level behavior requires lower-level technical mechanisms.

For example, Application may define an abstraction for file storage:

* Application defines the contract required by a use case.
* Infrastructure implements the contract using the configured storage provider.
* Application invokes the abstraction without knowing whether the implementation uses local storage or a cloud provider.

The same principle applies to persistence, job scheduling, and external integrations where abstractions provide a meaningful boundary.

Not every technical operation requires a new interface. Abstractions should represent genuine dependency boundaries rather than introduce unnecessary indirection.

## Dependency Injection and Composition

Concrete implementations are connected to application contracts through dependency injection.

Infrastructure registration is responsible for registering its implementations and required technical services. Application registration is responsible for registering application handlers, behaviors, and other application-level services.

The API composition root assembles the application and infrastructure components.

Composition may depend on concrete implementations because its purpose is to wire the system together. This exception does not permit ordinary Application or Domain code to depend on those implementations.

## Rationale

This decision provides the following benefits:

* **Business-rule isolation:** Domain behavior remains independent of persistence and framework choices.
* **Controlled change:** Replacing a technical implementation does not automatically require changing business logic.
* **Testability:** Application and Domain behavior can be exercised without requiring every infrastructure dependency.
* **Explicit responsibilities:** Each layer has a defined role in request processing, business behavior, and technical execution.
* **Consistent integration:** External systems are accessed through appropriate contracts rather than being embedded throughout business logic.
* **Architectural flexibility:** Infrastructure technologies can evolve without forcing corresponding changes throughout the inner layers.

The goal is not to eliminate all coupling. The goal is to ensure that dependencies follow stable architectural responsibilities rather than implementation convenience.

## Consequences

### Positive Consequences

* Business logic is less coupled to ASP.NET Core, EF Core, and external SDKs.
* Infrastructure implementations can be replaced or modified behind stable contracts.
* Application workflows can coordinate technical operations without directly invoking infrastructure-specific APIs.
* Architectural violations become easier to identify through project-reference rules and code review.
* Domain models remain focused on business meaning.

### Trade-offs and Constraints

* Some operations require explicit abstractions and dependency-injection registrations.
* The composition root must coordinate services implemented across different projects.
* Poorly designed abstractions can add unnecessary complexity.
* Clean Architecture does not automatically enforce boundaries; project references, automated architecture tests, and code-review practices may be needed.
* Not every dependency requires an interface, and not every query requires a dedicated ReadModel.
* The Domain layer's framework independence does not eliminate the need for infrastructure-specific mapping and persistence configuration.

## Alternatives Considered

### 1. Infrastructure-First Dependency Direction

**Description:** Allow Application and Domain to reference persistence implementations and external technical libraries directly.

**Reason for Rejection:** This would couple business behavior to implementation details and make infrastructure changes more invasive.

### 2. A Single Project Without Layer Boundaries

**Description:** Keep all functionality in one project and rely primarily on folder organization and developer conventions.

**Reason for Rejection:** Folder organization alone does not constrain project-level dependencies and offers weaker structural enforcement as the system grows.

### 3. Interfaces for Every Dependency

**Description:** Introduce an interface for every service, class, or internal interaction.

**Reason for Rejection:** Excessive abstraction adds maintenance overhead without necessarily improving dependency control. Interfaces should be introduced when they establish meaningful boundaries, support substitution, or express a useful contract.

## Conditions for Reconsideration

This decision should be revisited if the solution structure changes substantially or if a new architectural subsystem introduces dependency requirements that cannot be expressed cleanly under the current rules.

Any proposed exception must document its rationale, affected layers, and consequences. Convenience alone is not sufficient justification for allowing Infrastructure dependencies to leak into Application or Domain.

## Related Decisions

* [ADR-0001: Modular Monolith Architecture](0001-modular-monolith.md)
* [ADR-0003: Selective CQRS](0003-selective-cqrs.md)
* [ADR-0006: Two-Phase Save in Unit of Work](0006-two-phase-save-in-unit-of-work.md)
* [ADR-0008: ReadModel ADO.NET](0008-readmodel-ado-net.md)
* [ADR-0009: Scheduling Abstraction over Hangfire](0009-scheduling-abstraction-over-hangfire.md)

## Summary

The E-Commerce system uses Clean Architecture dependency direction to keep business logic independent of infrastructure implementations.

**The governing principle is that inner layers define business behavior and required abstractions, while outer layers implement the technical mechanisms that support them.**
