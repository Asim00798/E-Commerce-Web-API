# E-Commerce Modular Monolith

A production-oriented e-commerce backend built with **.NET 8, ASP.NET Core Web API, Entity Framework Core, and Microsoft SQL Server**.

The solution follows a modular monolith architecture built around Domain-Driven Design (DDD), explicit architectural boundaries, transactional persistence, reliable event processing, and controlled background execution.

Its objective is to provide a maintainable, reliable, and evolvable foundation for the company's e-commerce operations without introducing unnecessary distributed infrastructure.

---

## Business Model and Operational Scope

The application represents a retail company that sells products directly to customers through an e-commerce platform.

The initial operational scope is limited to a single city, with business operations and order fulfillment designed around that geographical boundary.

The system models a **single retail business**, rather than a multi-vendor marketplace. Its architecture supports the company's core commercial operations without introducing the additional complexity associated with independent sellers, seller-specific workflows, or geographically distributed operations.

The single-city scope is a business constraint of the current operating model, not necessarily a permanent architectural limitation. Expansion to additional cities can be considered when future business requirements justify it.

---

## 1. Technology Stack

| Area                                   | Technology              |
| -------------------------------------- | ----------------------- |
| Platform                               | .NET 8                  |
| Backend Framework                      | ASP.NET Core Web API    |
| Programming Language                   | C#                      |
| Relational Database                    | Microsoft SQL Server    |
| Object-Relational Mapper               | Entity Framework Core   |
| Authentication and Identity Management | ASP.NET Core Identity   |
| Background Processing                  | Hangfire                |
| Logging                                | Serilog                 |
| Real-Time Communication                | SignalR                 |
| Push Notifications                     | Firebase Admin SDK      |
| Specialized Read-Side Data Access      | ADO.NET                 |
| Read-Side Database Deployment          | DbUp                    |
| Application Caching                    | Redis, where configured |

Technologies are used according to their defined architectural responsibilities. Their presence does not imply that every feature uses every component or that every optional integration is enabled in every environment.

For initial setup and configuration instructions, see [`setup-guide.md`](./setup-guide.md).

---

## 2. Business Features

The solution organizes business capabilities around their respective responsibilities and domain boundaries.

| Feature                 | Scope                                                                                                    |
| ----------------------- | -------------------------------------------------------------------------------------------------------- |
| **Catalog**             | Product catalog capabilities and related business operations.                                            |
| **Ordering**            | Order management, order state transitions, and associated workflows.                                     |
| **Finance**             | Payment and refund workflows, payment-provider integration, and financial failure handling.              |
| **Shipping**            | Shipment-related business operations and fulfillment workflows.                                          |
| **People**              | Business-person information and related domain operations.                                               |
| **Customer Engagement** | Customer engagement capabilities and associated workflows.                                               |
| **Onboarding**          | Registration, verification, and onboarding workflows that establish eligibility for business operations. |

These features represent the business-facing capabilities of the application. Their documentation describes their requirements, technical design, integration contracts, and workflows.

Business-feature documentation is organized under [`docs/business-features/`](./docs/business-features/).

---

## 3. Platform Capabilities

The solution also provides reusable technical capabilities that support business features and cross-cutting application requirements.

| Capability                         | Scope                                                                                                        |
| ---------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| **File Storage**                   | Storage abstraction, file lifecycle management, controlled deletion, and storage-provider integration.       |
| **Notifications**                  | Push, email, and SMS delivery, with notification intent separated from delivery mechanisms.                  |
| **Transactional Outbox**           | Durable recording and asynchronous processing of integration events.                                         |
| **Background Scheduling**          | Application-facing scheduling abstractions, background execution, and recurring processing through Hangfire. |
| **ReadModel**                      | Specialized ADO.NET queries and stored-procedure-based read access.                                          |
| **Application Caching**            | Application-level caching and cache-management mechanisms.                                                   |
| **HTTP Caching**                   | HTTP caching behavior and response-cache considerations.                                                     |
| **Database Startup and Migration** | Database initialization, versioned schema changes, and controlled migration execution.                       |
| **Observability**                  | Logging, metrics, tracing, health checks, and alerting.                                                      |
| **API Security**                   | Authentication, authorization, CORS, and rate-limiting capabilities.                                         |
| **External Authentication**        | Integration with external authentication providers where configured.                                         |
| **Authorization Data Seeding**     | Initialization of authorization-related data through the documented seeding process.                         |

These capabilities have dedicated documentation describing their requirements, architecture, integration, and operational considerations.

Their presence does not imply that every provider, optional integration, or deployment-specific capability is enabled in every environment.

Technical-capability documentation is organized under [`docs/technical-capabilities/`](./docs/technical-capabilities/).

---

## 4. Architecture Overview

The application is a **single-deployable modular monolith**.

It runs as one application and uses a shared relational database. Internal modules maintain explicit responsibilities and boundaries while collaborating through in-process application contracts, domain behavior, and integration-event workflows where appropriate.

The architecture does not require microservices, a message broker, or service-to-service HTTP communication as its default operating model.

### Architectural Principles

* **Separation of concerns:** Each project has a defined responsibility.
* **Dependency inversion:** Infrastructure implements abstractions defined by inner layers.
* **Domain independence:** The Domain layer remains independent of frameworks and infrastructure.
* **Modularity:** Business capabilities are organized around explicit responsibilities and boundaries.
* **Transactional consistency:** Related database changes are committed within their intended transaction boundaries.
* **Reliable side effects:** The Transactional Outbox pattern supports durable recording and asynchronous dispatch of integration events.
* **Controlled background execution:** Scheduling is abstracted from the underlying job-processing infrastructure.
* **Selective CQRS:** Specialized read models are introduced where query requirements justify them.
* **Operational awareness:** Failure handling, idempotency, observability, and concurrency are treated as architectural concerns.
* **Proportionate complexity:** Infrastructure and abstractions address actual requirements rather than speculative future needs.

For the detailed architectural overview, see [`architecture.md`](./architecture.md).

---

## 5. Solution Structure

The repository is organized into application projects, documentation, and supporting repository configuration.

```text
E-Commerce/
├── .github/
│   └── workflows/
├── docs/
│   ├── architecture/
│   ├── decisions/
│   ├── business-features/
│   └── technical-capabilities/
├── src/
│   ├── E-Commerce.sln
│   ├── global.json
│   ├── E-Commerce.Api/
│   ├── E-Commerce.Application/
│   ├── E-Commerce.Domain/
│   ├── E-Commerce.Infrastructure/
│   └── E-Commerce.ReadModel/
├── .gitignore
├── architecture.md
├── README.md
└── setup-guide.md
```

This tree illustrates the principal repository structure. Individual projects contain additional implementation files, configuration, and supporting components.

### 5.1 E-Commerce.Api

The HTTP entry point and application composition boundary.

**Responsibilities:**

* API controllers and HTTP concerns.
* Middleware and request processing.
* Dependency injection and application composition.
* Authentication and authorization integration at the API boundary.
* Translation of application results into HTTP responses.

The API delegates business operations to the Application layer.

### 5.2 E-Commerce.Application

Coordinates application use cases and defines the contracts required to execute them.

**Responsibilities:**

* Commands, queries, and application handlers.
* Application services and orchestration.
* Validation and use-case coordination.
* Repository and Unit of Work abstractions.
* Contracts for infrastructure capabilities.
* Application-level integration and background-job contracts where appropriate.

The Application project references Domain but does not depend on Infrastructure.

### 5.3 E-Commerce.Domain

Contains the business domain model and its rules.

**Responsibilities:**

* Entities and aggregate roots.
* Value objects.
* Domain services.
* Domain events.
* Business invariants and state transitions.

The Domain project does not reference other solution projects or depend on infrastructure frameworks.

### 5.4 E-Commerce.Infrastructure

Implements infrastructure capabilities required by the Application layer.

**Responsibilities:**

* EF Core and SQL Server persistence.
* Repository and Unit of Work implementations.
* ASP.NET Core Identity persistence and integration.
* Outbox persistence and dispatching infrastructure.
* Hangfire integration.
* External service integrations.
* Infrastructure-specific implementations of application contracts.

Infrastructure references Application and Domain.

### 5.5 E-Commerce.ReadModel

Provides a dedicated read-side implementation for queries that benefit from specialized data access.

**Responsibilities:**

* Specialized read queries.
* ADO.NET-based data access.
* Stored-procedure integration.
* Read-side database deployment through DbUp.

ReadModel references Application and remains independent of Infrastructure.

The ReadModel project is an intentional optimization boundary, not a requirement that every query use a separate read-side implementation.

---

## 6. Project Dependency Direction

Project references follow an explicit dependency direction.

```text
E-Commerce.Api
├── E-Commerce.Application
├── E-Commerce.Infrastructure
└── E-Commerce.ReadModel

E-Commerce.Infrastructure
├── E-Commerce.Application
└── E-Commerce.Domain

E-Commerce.Application
└── E-Commerce.Domain

E-Commerce.ReadModel
└── E-Commerce.Application

E-Commerce.Domain
└── No project references
```

### Dependency Rules

| Project        | May Reference                          |
| -------------- | -------------------------------------- |
| Domain         | No other solution project              |
| Application    | Domain                                 |
| Infrastructure | Application, Domain                    |
| ReadModel      | Application                            |
| API            | Application, Infrastructure, ReadModel |

These rules prevent inner layers from depending on outer implementation details.

In particular:

* Domain must not depend on EF Core, ASP.NET Core Identity, Hangfire, or API concerns.
* Application must not reference Infrastructure.
* Infrastructure and ReadModel must not reference each other.
* No project may reference the API project.
* Infrastructure implementations are connected to application abstractions through dependency injection at the composition boundary.

The API acts as the composition root, connecting the required implementations and services.

For the detailed architectural rules, see [`docs/architecture/`](./docs/architecture/).

---

## 7. Domain and Module Organization

Business capabilities are organized according to their responsibilities and domain boundaries.

### Business-Domain Areas

* Catalog
* Ordering
* Finance
* Shipping
* Customer Engagement
* Onboarding
* People

### Technical and Application Modules

* Authentication
* Authorization
* Identity
* Notifications
* Profiles
* Scheduling
* File Storage
* Observability
* Caching

These lists represent different organizational concerns. A technical module is not automatically a DDD bounded context, and not every business capability requires a separate deployable service.

The solution distinguishes application-layer organization from domain-layer organization:

* **Application modules** coordinate use cases, application workflows, and integrations.
* **Domain models** encapsulate business rules and invariants where domain modeling is appropriate.
* **Infrastructure components** implement persistence, external integrations, and other technical mechanisms.

Modules collaborate within the same application process while preserving their intended responsibilities and boundaries.

---

## 8. Architectural Highlights

The solution emphasizes explicit architectural decisions, reliable execution, and controlled complexity.

### 8.1 Domain-Driven Design

Business rules, invariants, and state transitions belong to the appropriate domain model or application boundary.

Domain concepts remain independent of infrastructure implementation details, allowing business behavior to evolve without unnecessary coupling to technical frameworks.

### 8.2 Clean Architecture

Project dependencies follow an explicit direction.

Infrastructure implements inner-layer contracts rather than becoming a dependency of the Application or Domain layers.

### 8.3 Selective CQRS

The solution distinguishes write-side business operations from specialized read-side access without requiring a separate read model for every query.

### 8.4 Transactional Outbox

Outgoing integration events can be recorded atomically with the business changes they represent.

Background processing subsequently dispatches the recorded events. Retry and idempotency mechanisms address execution failures and duplicate processing where implemented.

### 8.5 Controlled Background Execution

Application-facing abstractions separate scheduling and job orchestration from Hangfire-specific infrastructure.

### 8.6 Concurrency and Idempotency

Concurrency controls protect operations where competing updates must be detected or rejected.

Idempotency mechanisms protect operations where repeated execution must not produce unintended duplicate effects.

These mechanisms have different responsibilities and do not, by themselves, guarantee exactly-once execution across independent systems.

### 8.7 Operational Visibility

Logging, metrics, tracing, health checks, and alerting provide mechanisms for understanding application behavior and diagnosing operational failures.

### 8.8 Security in Depth

Authentication, authorization, request restrictions, and secure configuration address different security concerns rather than relying on a single control.

---

## 9. Persistence and Data Access

Microsoft SQL Server is the primary relational database.

Entity Framework Core supports transactional persistence for business and application workflows that use the write-side persistence model.

The ReadModel project provides a specialized implementation for selected queries whose performance, aggregation, or data-shaping requirements justify a dedicated read-side approach.

### Persistence Principles

* Keep persistence implementation details outside the Domain layer.
* Define application-facing persistence contracts in the appropriate inner layer.
* Keep related transactional changes within their intended Unit of Work.
* Use optimistic concurrency controls where concurrent updates must be detected.
* Keep read-side specialization proportional to actual query requirements.
* Separate persistence implementation details from business rules.
* Use versioned database changes and controlled migration processes.
* Make transaction and consistency guarantees explicit.

The ReadModel represents a specialized read-side implementation. It does not imply that the system necessarily uses a separate read database or event-driven projections.

EF Core-based persistence and specialized ADO.NET access serve different requirements within the same application.

---

## 10. Event-Driven Design and Transactional Outbox

The application uses events where they provide a clear architectural benefit.

### 10.1 Domain Events

Domain events represent meaningful occurrences within the domain model.

They can coordinate reactions to domain state changes and support communication between domain behavior and application orchestration.

A domain event is not inherently a durable message. Its durability depends on how event handling, persistence, and publication are implemented.

### 10.2 Integration Events

Integration events communicate significant occurrences across module boundaries or to external consumers.

The module responsible for an integration-event contract owns its definition, while consumers implement their own handling logic.

Within the modular monolith, integration events can coordinate business workflows across modules without requiring a message broker.

### 10.3 Transactional Outbox

The Transactional Outbox pattern supports reliable integration-event publication by recording an outgoing event in the same database transaction as the business changes it represents.

The general workflow is:

```text
Business Operation
        |
        v
Domain State Changes
        |
        v
Integration Event Prepared
        |
        v
Business Changes + Outbox Record
        |
        v
Single Database Transaction
        |
        v
Outbox Processing Job
        |
        v
Dispatch Integration Event
        |
        v
Consumer Handles Event
        |
        v
Idempotency Protection
```

The Outbox closes the gap between committing business data and recording the intent to publish an integration event.

It does not, by itself, guarantee exactly-once delivery or execution. Dispatch and consumer handling must account for retries, duplicate processing, and failures.

In this modular monolith, durable database-backed processing is preferred over introducing a message broker without a concrete requirement.

For implementation details, see [`docs/technical-capabilities/communication/outbox/`](./docs/technical-capabilities/communication/outbox/).

---

## 11. Background Execution

Hangfire provides the underlying background-job processing infrastructure.

The Scheduling module establishes an application-facing abstraction so that application code can request background execution without depending directly on Hangfire APIs.

Its responsibilities include coordinating job execution, scheduling, and applicable execution policies.

Hangfire remains responsible for its infrastructure-level capabilities, including job persistence and configured retry behavior.

### Key Principles

* Keep application use cases independent of Hangfire-specific APIs.
* Separate job orchestration from the underlying execution engine.
* Avoid competing retry mechanisms that unintentionally multiply execution attempts.
* Design jobs to tolerate repeated execution.
* Use idempotency for operations that must not produce duplicate business effects.
* Evaluate timeout and circuit-breaker behavior according to their actual implementation and limitations.

### Scheduling and Outbox Processing

Scheduling and Outbox processing have different responsibilities.

| Capability               | Responsibility                                                                                                 |
| ------------------------ | -------------------------------------------------------------------------------------------------------------- |
| **Scheduling**           | Coordinates when and how background jobs execute.                                                              |
| **Transactional Outbox** | Records outgoing integration events durably alongside business changes and supports their subsequent dispatch. |

The Outbox processing job uses the Scheduling infrastructure to execute its delivery workflow. The Scheduling module does not replace the Outbox's persistence and delivery guarantees.

For the detailed design, see [`docs/technical-capabilities/scheduling/`](./docs/technical-capabilities/scheduling/).

---

## 12. CQRS and Read Models

The application uses Command Query Responsibility Segregation (CQRS) selectively.

Write operations coordinate business behavior and state changes. Read operations use the data-access approach appropriate to their requirements.

The dedicated ReadModel project exists for queries that benefit from optimized SQL, specialized projections, aggregation, or stored procedures.

Not every query requires a separate read model. CQRS does not automatically require separate databases, independently deployed services, or event-driven projections.

The objective is to introduce read-side complexity only when it provides a meaningful benefit.

For the specialized read-side design, see the ReadModel documentation under [`docs/architecture/`](./docs/architecture/).

---

## 13. Authentication, Identity, and People

Authentication identity and business-person information are separate concerns.

ASP.NET Core Identity manages authentication-related identity through the Infrastructure layer.

The People domain represents business-person information independently of the authentication framework.

This separation allows authentication mechanisms to evolve without making the business domain dependent on ASP.NET Core Identity.

Registration, verification, onboarding, and access to business operations are governed by their respective application workflows and business rules.

The application distinguishes between having an authenticated account and satisfying the business requirements needed to perform customer operations.

For the detailed documentation, see:

* [`docs/technical-capabilities/security/identity/`](./docs/technical-capabilities/security/identity/)
* [`docs/technical-capabilities/security/authentication/`](./docs/technical-capabilities/security/authentication/)
* [`docs/business-features/Onboarding/`](./docs/business-features/Onboarding/)
* [`docs/business-features/People/`](./docs/business-features/People/)

---

## 14. Documentation

The repository documentation is organized by purpose, from high-level architecture to detailed business features and technical capabilities.

| Location                                                         | Purpose                                                                                                   |
| ---------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------- |
| [`README.md`](./README.md)                                       | Project introduction, capabilities, architecture summary, and documentation entry point.                  |
| [`architecture.md`](./architecture.md)                           | High-level architectural overview and architectural principles.                                           |
| [`setup-guide.md`](./setup-guide.md)                             | Solution setup and initial configuration.                                                                 |
| [`docs/README.md`](./docs/README.md)                             | Documentation index and navigation.                                                                       |
| [`docs/architecture/`](./docs/architecture/)                     | Layer responsibilities, context mapping, and architectural details.                                       |
| [`docs/decisions/`](./docs/decisions/)                           | Architecture Decision Records documenting significant decisions and their rationale.                      |
| [`docs/business-features/`](./docs/business-features/)           | Business-feature requirements, references, workflows, and integration guides.                             |
| [`docs/technical-capabilities/`](./docs/technical-capabilities/) | Technical-capability requirements, references, workflows, and integration documentation where applicable. |
| [`.github/workflows/`](./.github/workflows/)                     | Repository workflow automation.                                                                           |

### 14.1 Business-Feature Documentation

Business-feature documentation covers:

* Catalog
* Customer Engagement
* Finance
* Onboarding
* Ordering
* People
* Shipping

Each feature directory organizes its documentation around the files available for that feature, including requirements, technical references, workflows, and integration guides.

### 14.2 Technical-Capability Documentation

Technical-capability documentation covers:

* **Caching:** Application caching and HTTP caching.
* **Communication:** Notifications, Outbox, Post-Commit processing, and Real-Time Communication.
* **Database Startup and Migration:** Database initialization and schema migration.
* **File Storage:** Storage abstractions and file lifecycle management.
* **Observability:** Alerting, health checks, logging, metrics, and tracing.
* **Scheduling:** Background execution and job orchestration.
* **Security:** Authentication, authorization, identity, and related data-seeding guidance.

Some capabilities have specialized documentation structures rather than using an identical set of files. The documentation index describes the available material and provides navigation to the relevant guides.

Architecture Decision Records document important architectural choices, their context, and their rationale. They complement implementation references rather than replacing them.

**Feature-specific and capability-specific documentation remains the authoritative source for detailed implementation behavior.**

---

## 15. Architectural Scope and Evolution

This project is intentionally designed as a modular monolith.

It does not introduce distributed-system components merely because they are commonly used in large systems.

A message broker, independently deployed service, or separate database should be considered when a concrete requirement justifies the additional operational and consistency complexity.

Examples include:

* Independent deployment requirements.
* Distinct scaling requirements that cannot be adequately addressed within the existing deployment model.
* Integration with external consumers that requires an appropriate messaging boundary.
* Data isolation or operational requirements that justify separate persistence infrastructure.

Until such requirements arise, explicit module boundaries, transactional persistence, reliable event processing, and controlled background execution provide the foundation for evolving the application without prematurely distributing it.

The architecture is designed to support deliberate evolution while keeping current operational complexity proportionate to business requirements.

---

## Summary

The E-Commerce backend combines:

* A layered .NET solution with explicit dependency direction.
* Domain-driven business modeling.
* Modular organization within a single-deployable application.
* A single-company retail business model with an initial single-city operational scope.
* Business capabilities spanning Catalog, Ordering, Finance, Shipping, People, Customer Engagement, and Onboarding.
* Reusable platform capabilities for file storage, notifications, caching, scheduling, observability, and security.
* EF Core and SQL Server for transactional persistence.
* Selective CQRS and specialized read-side access through ADO.NET.
* Transactional Outbox processing for reliable integration-event publication.
* Hangfire-backed background execution behind application-facing abstractions.
* Separation between authentication identity and business-person data.
* Explicit attention to concurrency, idempotency, failure handling, and operational reliability.
* Structured documentation covering architecture, business features, technical capabilities, and architectural decisions.

**The architectural goal is not maximum infrastructure complexity. It is a maintainable, reliable, and evolvable system whose complexity is justified by its requirements.**
