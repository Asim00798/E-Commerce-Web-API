# ADR-0001: Modular Monolith Architecture

* **Status:** Accepted
* **Decision Type:** Architecture
* **Date:** 2026-10-13

## Context

The E-Commerce system requires an architecture that supports clear business boundaries, maintainable code, reliable transaction handling, and long-term evolution.

The application contains multiple business capabilities, including Catalog, Customer Engagement, Finance, Onboarding, Ordering, People, and Shipping. It also relies on shared technical capabilities such as authentication, authorization, caching, file storage, notifications, observability, and background-job scheduling.

Without explicit architectural boundaries, these responsibilities can become tightly coupled, making changes difficult to isolate and increasing the risk of unintended side effects.

A distributed microservices architecture could provide independent deployment and data ownership, but it would also introduce additional operational and development complexity, including network communication, distributed failure handling, service-to-service contracts, and cross-service consistency challenges.

The project therefore requires an architecture that provides modularity and separation of concerns without introducing distributed-system complexity prematurely.

## Decision

The system adopts a **modular monolith architecture**.

The application is organized into cohesive modules with explicit responsibilities and dependency boundaries while remaining part of a single deployable application.

The architecture follows these principles:

### 1. Logical Modularity

Business features and technical capabilities are organized into modules according to their responsibilities.

Business features own their business concepts, rules, and workflows. Technical capabilities provide reusable implementation mechanisms that support multiple features.

Modules should interact through explicit contracts and established integration mechanisms rather than relying on unrestricted access to one another's internal implementation details.

### 2. Layered Architecture

The solution follows Clean Architecture dependency direction:

* **API:** Exposes HTTP endpoints and handles transport-specific concerns.
* **Application:** Coordinates use cases, defines application contracts, and orchestrates workflows.
* **Domain:** Models business concepts, invariants, and domain behavior independently of infrastructure frameworks.
* **Infrastructure:** Implements persistence, external integrations, authentication mechanisms, background-job execution, and other technical concerns.
* **ReadModel:** Provides a dedicated read-side subsystem where query requirements justify a specialized implementation.

Dependencies must point inward toward the appropriate abstractions. Infrastructure implements contracts defined by inner layers rather than becoming the owner of business rules.

The ReadModel is used selectively; it is not a mandatory query layer for every use case.

### 3. Explicit Module Responsibilities

Each module should have a clearly defined responsibility and ownership boundary.

Business rules remain with the feature that owns the relevant business concept. Shared technical capabilities provide reusable mechanisms without taking ownership of feature-specific business behavior.

For example, Ordering determines when an order expires, while Scheduling provides the infrastructure used to execute the expiration job.

### 4. Controlled Cross-Module Communication

Modules communicate through deliberate contracts and integration mechanisms appropriate to the interaction.

These mechanisms include application-level abstractions, domain events, integration events, and transactional outbox processing where applicable.

Integration events represent module-level integration contracts; they do not imply that the modules are separate deployable services.

### 5. Shared Transactional Infrastructure

Modules operate within the same application process and may share the application's database and Unit of Work.

Transactions are used where atomic persistence is required. The transactional outbox supports reliable post-commit integration-event processing without requiring a distributed message broker for communication inside the monolith.

Shared infrastructure does not remove the need for clear module ownership or explicit contracts.

### 6. Single Deployable Application

The system is deployed as one application rather than as independently deployed microservices.

Its modules are not independently deployable services, and the architecture does not require a network boundary between modules.

The project does not introduce distributed infrastructure solely to imitate microservice patterns.

## Rationale

The modular monolith provides a balance between architectural discipline and operational simplicity.

The decision is based on the following considerations:

* **Maintainability:** Explicit module and layer boundaries help developers understand where responsibilities belong.
* **Consistency:** Related business operations can use shared transactional infrastructure when atomicity is required.
* **Reliability:** In-process module communication avoids unnecessary network failure modes, while the outbox addresses reliable post-commit processing.
* **Operational simplicity:** A single deployment reduces the infrastructure and coordination overhead associated with independently deployed services.
* **Controlled complexity:** The architecture can use established patterns such as CQRS, domain events, integration events, and background jobs without requiring microservices.
* **Evolution:** Clear module boundaries make future extraction of a module possible if independent deployment or data ownership becomes a justified requirement.

Microservices are not rejected as an architectural style. They are deferred until the system has concrete requirements that justify their additional complexity.

## Consequences

### Positive Consequences

* Business and technical responsibilities have explicit organizational boundaries.
* The application can maintain strong consistency across operations that share a database transaction.
* Developers can reason about most module interactions without distributed tracing or network-level failure analysis.
* Infrastructure remains simpler than an equivalent distributed deployment.
* Cross-cutting technical capabilities can be reused without duplicating their implementations across independently deployed services.
* Architectural decisions can be enforced through project references, contracts, code organization, and automated validation.

### Trade-offs and Constraints

* Modules share a deployment lifecycle and cannot be released independently.
* A process failure can affect the entire application.
* Modules may compete for shared resources, including database connections, memory, CPU, and thread-pool capacity.
* Shared database access can weaken module boundaries unless ownership and access rules are enforced.
* A single deployment does not guarantee scalability or reliability; these still depend on appropriate implementation, configuration, monitoring, and operational practices.
* In-process integration contracts require discipline because the compiler alone cannot enforce every architectural boundary.
* A future transition to microservices would require explicit decisions about data ownership, communication contracts, consistency, deployment, and operational responsibilities.

## Alternatives Considered

### 1. Traditional Monolith Without Explicit Module Boundaries

**Description:** Organize the application primarily by technical layer or general-purpose folders without enforcing meaningful module ownership.

**Reason for Rejection:** This approach offers a simple starting point but provides insufficient structural guidance as business features and cross-cutting concerns grow. Uncontrolled dependencies can make changes increasingly difficult to isolate.

### 2. Microservices

**Description:** Deploy business capabilities as independently running services with service-level boundaries and communication contracts.

**Reason for Rejection:** Independent deployment and service-level isolation are not established requirements sufficient to justify the additional distributed-system complexity. Introducing microservices prematurely would add operational overhead and distributed consistency challenges without a demonstrated corresponding benefit.

Microservices may be reconsidered if future requirements justify independently deployed services.

### 3. Distributed Modular Architecture

**Description:** Retain logical modules but distribute them across multiple processes or deployment units.

**Reason for Rejection:** This would introduce process and communication boundaries that the current system does not require. The additional complexity is not justified while a single deployable application satisfies the system's architectural needs.

## Conditions for Reconsideration

This decision should be revisited if concrete requirements emerge that cannot be adequately addressed by the existing architecture, including:

* A business capability requiring an independent deployment and release lifecycle.
* Demonstrated scaling requirements that justify independently scaling a particular capability.
* Isolation requirements that cannot be adequately satisfied within the existing process.
* Organizational or regulatory constraints requiring separate operational ownership.
* Resource contention or reliability problems that cannot be reasonably addressed within the monolith.
* A need for independent data ownership and service-level availability guarantees.

Reconsideration does not automatically imply adopting microservices. Any proposed change must compare the expected benefits with the operational, development, security, and consistency costs.

## Related Decisions

* [ADR-0002: Clean Architecture Dependency Direction](0002-clean-architecture-dependency-direction.md)
* [ADR-0003: Selective CQRS](0003-selective-cqrs.md)
* [ADR-0004: Domain Events vs. Integration Events](0004-domain-events-vs-integration-events.md)
* [ADR-0005: Transactional Outbox](0005-transactional-outbox.md)
* [ADR-0014: Single Deployable Unit](0014-single-deployable-unit.md)

## Summary

The E-Commerce system adopts a modular monolith to combine explicit architectural boundaries with the simplicity of a single deployable application.

**The governing principle is to enforce modularity within the application without introducing distributed-system complexity until concrete requirements justify it.**
