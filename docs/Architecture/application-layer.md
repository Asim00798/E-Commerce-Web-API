# E-Commerce.Application — Architecture Document

**Project:** E-Commerce Modular Monolith

**Layer:** Application

**Version:** 1.0 – Final

---

## 1. Overview

The `E-Commerce.Application` project is the **application/use-case layer** of the E-Commerce modular monolith.

It coordinates application workflows between the API, Domain, Infrastructure, and specialized ReadModel subsystem while keeping business use-case orchestration independent from infrastructure technologies.

The Application layer is responsible for answering:

**“What does the system need to do in response to a use case?”**

It coordinates domain behavior, persistence abstractions, authorization requirements, integration contracts, queries, commands, background-work contracts, and other application-level workflows.

It does **not** own:

* HTTP
* EF Core
* SQL Server
* ASP.NET Core Identity implementation
* Hangfire
* Redis
* SignalR
* Firebase
* SMTP
* Twilio
* Paymob
* ADO.NET implementation
* Concrete infrastructure services

Those concerns belong to API, Infrastructure, or ReadModel.

The Application layer therefore acts as the **orchestration boundary between business capabilities and technical implementations**.

---

## 2. Architectural Philosophy

The Application layer follows the principles of:

* Clean Architecture
* Domain-Driven Design
* CQRS
* Dependency Inversion
* Explicit use-case orchestration
* Separation of commands and queries
* Infrastructure abstraction
* Domain authority over business rules
* Framework independence where practical

The central rule is:

**Application coordinates; Domain decides; Infrastructure implements; API translates HTTP.**

For example:

```text
HTTP Request
     |
     v
API Controller
     |
     v
Application Command
     |
     v
Application Handler
     |
     v
Domain Aggregate
     |
     v
Repository / Unit of Work abstraction
     |
     v
Infrastructure implementation
     |
     v
SQL Server
```

For specialized reads:

```text
HTTP Request
     |
     v
API Controller
     |
     v
Application Query
     |
     v
ReadModel Query Service
     |
     v
ADO.NET
     |
     v
SQL Server
```

The Application layer does not need to know whether persistence is implemented using EF Core, ADO.NET, or another technology.

---

## 3. Primary Responsibilities

The Application layer owns the following responsibilities.

### 3.1 Use Cases

Defines the application's supported operations through:

* Commands
* Command handlers
* Queries
* Query handlers
* Application services where appropriate

Examples include:

* Register user
* Verify email
* Verify phone
* Complete onboarding
* Create person
* Place order
* Cancel order
* Capture payment
* Request refund
* Create shipment
* Add product to wishlist
* Submit rating
* Retrieve specialized read models

### 3.2 Application Orchestration

Application handlers coordinate multiple operations when a use case crosses multiple abstractions.

For example:

```text
Place Order
     |
     v
Load Cart
     |
     v
Validate application-level conditions
     |
     v
Invoke Order domain behavior
     |
     v
Persist Order
     |
     v
Commit Unit of Work
```

The handler coordinates the workflow.

The Order aggregate remains responsible for its own business invariants.

### 3.3 Application Contracts

Application owns contracts required by the use cases.

Examples include:

* Repository interfaces
* Query interfaces
* Identity service contracts
* Notification contracts
* File service contracts
* Scheduling contracts
* Current-user abstractions
* Authorization requirements
* ReadModel query contracts

These contracts allow Application to depend on capabilities rather than technical implementations.

### 3.4 CQRS

The Application layer provides the CQRS boundary.

Commands represent operations that change application state.

Queries represent operations that retrieve information.

Conceptually:

```text
Command
   |
   v
Command Handler
   |
   v
Domain / Persistence
```

And:

```text
Query
   |
   v
Query Handler
   |
   v
Read abstraction
```

CQRS is **selective**, not mandatory for every operation.

Simple reads may continue to use the normal write-side persistence model.

Specialized queries may use `E-Commerce.ReadModel`.

---

## 4. Application Is Not the Domain Layer

The Application layer must not become a replacement for Domain.

The distinction is:

| Responsibility                      | Layer                                         |
| ----------------------------------- | --------------------------------------------- |
| Business invariant                  | Domain                                        |
| Aggregate state transition          | Domain                                        |
| Value-object behavior               | Domain                                        |
| Domain event                        | Domain                                        |
| Business policy                     | Domain                                        |
| Use-case orchestration              | Application                                   |
| Command/query contract              | Application                                   |
| Repository abstraction              | Application or Domain, depending on ownership |
| Authorization requirement           | Application                                   |
| Infrastructure implementation       | Infrastructure                                |
| HTTP translation                    | API                                           |
| Specialized SQL read implementation | ReadModel                                     |

For example, the Application handler may execute:

```csharp
Order.Place(...);
```

But it should not reproduce the rules contained inside `Order`.

The Application layer determines **when and how the operation is invoked**.

The Domain determines **whether the business operation is valid**.

---

## 5. Domain Dependency

The Application layer depends on the Domain layer.

```text
Application
     |
     v
Domain
```

This allows Application handlers to use:

* Aggregates
* Entities
* Value objects
* Domain exceptions
* Domain events
* Domain repository contracts
* Domain services and policies

The reverse dependency is prohibited.

```text
Domain
  X
Application
```

The Domain must never depend on Application.

---

## 6. Application Project Structure

The Application layer is organized primarily around application modules and capabilities rather than technical infrastructure.

Recommended structure:

```text
E-Commerce.Application/
|
├── Modules/
│   ├── Catalog/
│   ├── CustomerEngagement/
│   ├── Finance/
│   ├── Onboarding/
│   ├── Orders/
│   ├── People/
│   ├── Shipping/
│   ├── Authentication/
│   ├── Authorization/
│   ├── Identity/
│   ├── Notifications/
│   ├── Profiles/
│   └── Scheduling/
│
├── Common/
│
└── Behaviors/
```

The exact internal organization can vary by module, but the architectural responsibilities remain stable.

---

## 7. Business-Oriented Application Modules

The primary business-oriented modules correspond closely to the Domain bounded contexts.

These include:

* Catalog
* CustomerEngagement
* Finance
* Orders
* People
* Shipping

These modules contain application use cases that orchestrate the corresponding domain capabilities.

For example:

```text
Application.Modules.Orders
          |
          v
Domain.Ordering
```

And:

```text
Application.Modules.Finance
          |
          v
Domain.Finance
```

The Application layer does not duplicate the Domain structure mechanically; it exposes the **use cases required by the application**.

---

## 8. Application Workflow and Platform Modules

Some Application modules represent workflows or application capabilities rather than Domain bounded contexts.

Examples include:

* Identity
* Authentication
* Authorization
* Notifications
* Scheduling
* Profiles
* Onboarding

These are intentionally treated differently from genuine Domain bounded contexts.

For example, **Identity** is an application capability backed by ASP.NET Core Identity in Infrastructure.

It is not a Domain aggregate or Domain bounded context.

---

## 9. Onboarding

Onboarding coordinates the user-registration workflow.

The current registration lifecycle is:

```text
Registration Request
        |
        v
Registration aggregate
        |
        v
Email verification
        |
        v
Phone verification
        |
        v
Fully verified
        |
        v
Identity creation
        |
        v
Authentication
        |
        v
Person creation / completion
        |
        v
Business operations
```

The Application layer coordinates this process.

The Domain layer owns registration business rules and state transitions.

Infrastructure implements persistence and ASP.NET Identity.

Therefore:

```text
Application
   ├── Coordinates registration workflow
   ├── Invokes Registration aggregate
   └── Invokes identity capability

Domain
   └── Owns Registration business rules

Infrastructure
   ├── Persists Registration
   └── Implements ASP.NET Identity
```

---

## 10. Identity

Identity is an Application capability rather than a Domain bounded context.

The Application layer may define an abstraction such as:

```csharp
IIdentityService
```

Infrastructure implements it using:

* `UserManager<User>`
* `SignInManager<User>`
* ASP.NET Core Identity

The Application layer therefore does not depend directly on:

* `IdentityUser`
* `UserManager`
* `SignInManager`
* EF Core Identity stores

Those belong to Infrastructure.

The important boundary is:

**Application expresses the identity operation; Infrastructure performs it using ASP.NET Core Identity.**

---

## 11. Authentication

Authentication orchestration belongs to Application-level workflows.

Typical responsibilities include:

* Login
* Refresh-token workflow
* Logout and revocation
* Credential operations
* External-login workflow where applicable
* Authentication-related application decisions

Infrastructure implements the technical mechanisms:

* JWT generation
* Refresh-token hashing
* ASP.NET Identity
* Cryptographic services
* Authentication middleware

The Application layer should therefore depend on capabilities rather than token-generation implementation details.

---

## 12. Authorization

Authorization has two complementary responsibilities.

### Application

Application owns application-level permission requirements and authorization decisions associated with use cases.

For example:

* Create Product
* Manage Orders
* Process Refund
* Manage Permissions

The Application use case may require a permission such as:

```text
Catalog.Products.Create
```

### Infrastructure

Infrastructure implements the ASP.NET Core authorization machinery:

* `IAuthorizationPolicyProvider`
* Authorization handlers
* Policies
* Requirements
* Permission persistence

Therefore:

```text
Application
     |
     v
Permission requirement / capability
     |
     v
Infrastructure
     |
     v
ASP.NET Core Authorization
```

---

## 13. Commands

Commands represent application operations that change state.

Conceptually:

* `CreateProductCommand`
* `PlaceOrderCommand`
* `CancelOrderCommand`
* `CapturePaymentCommand`
* `RequestRefundCommand`
* `VerifyEmailCommand`
* `VerifyPhoneCommand`
* `CreatePersonCommand`
* `CreateShipmentCommand`

A command contains the data required to request an operation.

It should not contain infrastructure behavior.

Example:

```csharp
public sealed record PlaceOrderCommand(
    Guid CartId);
```

The command itself does not execute the operation.

Its handler performs the application orchestration.

---

## 14. Command Handlers

Command handlers are responsible for executing application use cases.

A typical flow is:

```text
Command
  |
  v
Handler
  |
  v
Load required state
  |
  v
Invoke domain behavior
  |
  v
Persist changes
  |
  v
Commit Unit of Work
```

Handlers should remain orchestration-oriented.

They should not become large procedural implementations of business rules.

For example, `Order.Place(...)` belongs to the Domain.

The handler should call it rather than reproduce its rules.

---

## 15. Queries

Queries represent read operations.

Examples:

* `GetProductQuery`
* `GetOrderQuery`
* `GetCustomerProfileQuery`
* `GetPaymentQuery`
* `GetShipmentQuery`
* `GetWishlistQuery`

Application owns the query contract.

The implementation may use:

* Write-side EF Core
* ReadModel
* Specialized ADO.NET
* Another approved read implementation

The choice depends on the query's requirements.

---

## 16. Specialized ReadModel Queries

The Application layer owns the abstraction for specialized read operations.

For example:

```text
Application
     |
     v
ICustomerProfileQuery
     |
     v
ReadModel
     |
     v
ADO.NET
```

The Application layer does not know that the implementation uses:

* `SqlConnection`
* `SqlCommand`
* `SqlDataReader`
* Stored procedures
* DbUp

Those are ReadModel implementation details.

This preserves the dependency boundary:

```text
API
 ├── Application
 ├── Infrastructure
 └── ReadModel

ReadModel
   └── Application
```

Infrastructure does not depend on ReadModel.

---

## 17. Selective CQRS

CQRS is applied where it provides practical value.

The architecture does **not** require every read to use the specialized ReadModel.

A simple read may remain:

```text
Application Query
      |
      v
EF Core Repository
      |
      v
SQL Server
```

A complex or performance-sensitive read may use:

```text
Application Query
      |
      v
ReadModel Contract
      |
      v
ADO.NET
      |
      v
Stored Procedure
      |
      v
SQL Server
```

Selection is based on:

* Query shape
* Aggregation requirements
* Performance characteristics
* Reporting complexity
* Read-side optimization requirements

CQRS is not applied mechanically.

---

## 18. Repository Abstractions

Repository interfaces provide persistence ports for domain aggregates and application workflows.

Examples include:

* `IProductRepository`
* `IOrderRepository`
* `IPaymentRepository`
* `IRefundRepository`
* `IPersonRepository`
* `IShipmentRepository`
* `IRegistrationRepository`

The implementation belongs to Infrastructure.

```text
Application / Domain abstraction
             |
             v
Infrastructure repository
             |
             v
EF Core
             |
             v
SQL Server
```

The Application layer should not instantiate EF Core repositories directly.

---

## 19. Unit of Work

Application use cases may depend on the `IUnitOfWork` abstraction.

The Application handler determines the transactional boundary of a use case.

For example:

```text
Load aggregate
     |
     v
Perform domain operation
     |
     v
Persist changes
     |
     v
Commit Unit of Work
```

Infrastructure provides the actual EF Core implementation.

The Application layer therefore does not depend directly on:

* `DbContext`
* `IDbContextTransaction`
* `SaveChangesAsync`
* EF Core transaction APIs

---

## 20. Transactional Boundaries

The Application layer is responsible for defining **which operations belong to the same use-case transaction**.

Infrastructure is responsible for implementing the transaction.

For example:

```text
Place Order
 ├── Modify Order
 ├── Persist Order
 ├── Persist Outbox message
 └── Commit
```

The Application layer should not manually manipulate database transactions.

Instead:

```text
Application
     |
     v
IUnitOfWork
     |
     v
Infrastructure UnitOfWork
     |
     v
EF Core transaction
```

This maintains separation between application intent and persistence technology.

---

## 21. Domain Events

Domain events originate in the Domain layer.

Application may participate in workflows that react to domain events, but it does not redefine them.

Example:

```text
Order
 |
 v
OrderPaidDomainEvent
```

Infrastructure provides event-dispatching mechanisms.

The conceptual responsibility is:

* **Domain:** Defines the business event.
* **Application:** Coordinates the application response where appropriate.
* **Infrastructure:** Provides dispatch, persistence, and transport mechanisms.

---

## 22. Integration Events

Integration events represent messages crossing module or application boundaries.

The producing capability owns the integration-event contract.

For example:

```text
Finance
  |
  v
PaymentCompletedIntegrationEvent
```

Ordering can consume that event:

```text
PaymentCompletedIntegrationEvent
             |
             v
      Ordering handler
```

The durable flow is:

```text
Domain Event
    |
    v
Application / integration-event handling
    |
    v
Outbox Message
    |
    v
Commit
    |
    v
Hangfire OutboxProcessingJob
    |
    v
Dispatcher
    |
    v
Integration Handler
```

The Application layer owns the business and application contracts involved in this workflow.

Infrastructure owns the outbox storage and execution machinery.

---

## 23. Outbox Boundary

The Application layer must not directly publish durable external messages from inside a command handler.

Instead, the architecture uses the transactional outbox.

Conceptually:

```text
Application use case
      |
      v
Domain state change
      |
      v
Integration event
      |
      v
Outbox persistence
      |
      v
Same transaction
      |
      v
Commit
```

Infrastructure later processes the outbox.

This prevents a successful database transaction from being followed by a lost message because the process failed before publication.

---

## 24. Background Work and Scheduling

Scheduling is an Application capability.

The Application layer defines scheduler-agnostic contracts such as:

* `IJob`
* `IJobContext`
* `IJobDispatcher`
* `IJobExecutionEngine`
* `IJobHandler<T>`
* `IJobScheduler`

The purpose is to prevent application and business logic from depending directly on Hangfire.

The flow is:

```text
Application
     |
     v
IJobScheduler
     |
     v
Infrastructure
     |
     v
Hangfire
```

Hangfire is therefore an implementation detail.

Application code should not directly contain the following for business scheduling:

```csharp
BackgroundJob.Enqueue(...);

RecurringJob.AddOrUpdate(...);
```

---

## 25. Controlled Execution Gateway

The Application scheduling subsystem acts as a controlled execution gateway.

It provides a consistent place for execution concerns such as:

* Job dispatch
* Execution context
* Timeout handling
* Logging context
* Execution pipeline
* Failure handling
* Circuit-breaking where applicable

Infrastructure adapts this abstraction to Hangfire.

This creates:

```text
Business / Application code
             |
             v
Application Scheduling Abstraction
             |
             v
Execution Gateway
             |
             v
Hangfire Adapter
             |
             v
Hangfire
```

The business layer therefore remains scheduler-agnostic.

---

## 26. Notifications

The Application layer expresses notification intent.

Business modules determine:

* What happened
* Who should be notified
* Why they should be notified
* When the notification should occur
* What notification content is required

Infrastructure determines how the notification is delivered.

Examples:

```text
Application
     |
     v
Email notification request
     |
     v
Infrastructure EmailChannel
     |
     v
SMTP / provider
```

Or:

```text
Application
     |
     v
Push notification request
     |
     v
Infrastructure PushChannel
     |
     v
Firebase
```

The Application layer does not depend on SMTP, Twilio, Firebase, or SignalR implementations.

---

## 27. Realtime Notifications

Realtime delivery is deliberately separated from durable notification delivery.

The Application layer may produce a notification or application event.

Infrastructure may then provide SignalR as a best-effort realtime hint.

The architectural distinction is:

```text
Database
   |
   v
Source of truth

SignalR
   |
   v
Realtime client hint
```

SignalR is therefore not the durable notification mechanism.

---

## 28. File Service Abstraction

Application-facing file operations are expressed through abstractions such as:

```csharp
IFileService
```

The Application layer does not know whether the underlying storage is:

* Local filesystem
* Amazon S3
* Azure Blob Storage
* Another provider

Infrastructure implements:

```csharp
IFileStorageProvider
```

and connects it to the concrete storage technology.

Therefore:

```text
Application
     |
     v
IFileService
     |
     v
Infrastructure
     |
     v
IFileStorageProvider
     |
     v
Local / S3 / Azure / etc.
```

---

## 29. Current User Abstraction

Application use cases may require knowledge of the currently authenticated user.

Rather than depending on ASP.NET Core HTTP abstractions, Application should depend on an application-facing abstraction such as:

```csharp
ICurrentUser
```

The Infrastructure implementation resolves the current identity from the authentication context.

This prevents application use cases from depending directly on:

* `HttpContext`
* `ClaimsPrincipal`
* ASP.NET Core authentication implementation

where those details are unnecessary.

---

## 30. External Services

Application defines capabilities required from external systems.

Examples include:

* Identity operations
* Payment operations
* Notification operations
* File operations
* Scheduling operations
* Clock and time operations

Infrastructure implements those capabilities.

For example:

```text
Application
     |
     v
IPaymentGateway
     |
     v
Infrastructure
     |
     v
Paymob
```

The Application layer must not depend directly on:

* Paymob SDK/API models
* `HttpClient` implementation details
* Firebase SDK
* Twilio SDK
* SMTP APIs
* Redis APIs
* Hangfire APIs

---

## 31. Payment Workflow

Finance remains a Domain bounded context.

Application orchestrates payment use cases.

For example:

```text
Capture Payment Command
         |
         v
Finance Application Handler
         |
         v
Payment Aggregate
         |
         v
Payment business operation
         |
         v
Repository
         |
         v
Unit of Work
         |
         v
Outbox
```

External Paymob communication is Infrastructure.

Therefore:

* **Application:** Payment use case.
* **Domain:** Payment business rules.
* **Infrastructure:** Paymob communication, webhook verification, and persistence.

Webhook requests must not bypass the Application/Finance workflow.

---

## 32. Shipping Workflow

Shipping use cases follow the same separation.

Application coordinates:

* Create Shipment
* Ship Shipment
* Deliver Shipment
* Record Delivery Attempt
* Return Shipment
* Cancel Shipment

Domain owns shipment state transitions and invariants.

Infrastructure owns:

* External location services
* Map providers
* Persistence
* Technical communication

The Application layer should not reproduce shipment state-machine rules.

---

## 33. Validation

Validation exists at multiple architectural levels.

### API

Validates HTTP and wire-level concerns.

### Application

Validates application-level input and use-case requirements.

### Domain

Enforces business invariants.

For example:

```text
API
 |
 └── Request shape

Application
 |
 └── Required application conditions

Domain
 |
 └── Business invariant
```

Application validation must not become a second copy of Domain validation.

---

## 34. Exception Boundaries

Application may allow domain and application exceptions to propagate toward the API boundary.

Examples include:

* `BusinessRuleViolationException`
* `DomainException`
* `NotFoundException`
* `ForbiddenException`
* `ConcurrencyException`
* `ValidationException`

Infrastructure may translate technical failures into appropriate application-facing exceptions where required.

The API's global exception handler ultimately translates those exceptions into HTTP responses.

Conceptually:

```text
Domain / Infrastructure
         |
         v
Application boundary
         |
         v
API exception handler
         |
         v
HTTP response
```

---

## 35. Optimistic Concurrency

Optimistic concurrency is primarily implemented by Infrastructure persistence mechanisms.

Application participates by treating concurrency failure as an application-level failure.

For example:

```text
Application
     |
     v
Update aggregate
     |
     v
Unit of Work
     |
     v
Concurrency conflict
     |
     v
Infrastructure translates exception
     |
     v
Application / API boundary
     |
     v
HTTP 409 Conflict
```

The Application layer does not implement SQL Server row-version mechanics.

---

## 36. Application Behaviors

Cross-cutting application behavior may be implemented through MediatR pipeline behaviors.

Examples include:

* Validation
* Logging context
* Performance measurement
* Transaction behavior where appropriate
* Authorization
* Other application-level policies

These behaviors operate around commands and queries.

They should not become hidden substitutes for explicit domain behavior.

The architecture therefore distinguishes:

**Application pipeline behavior ≠ Domain business rule**

---

## 37. Performance Behavior

The Application pipeline may measure use-case execution performance.

For example:

```text
Command / Query
     |
     v
PerformanceBehavior
     |
     v
Handler
```

This allows operation performance to be observed without placing timing logic into every handler.

Infrastructure and observability systems provide the actual telemetry implementation.

---

## 38. Application and Infrastructure Dependency

The dependency direction is:

```text
Application
     |
     v
Domain
```

Infrastructure points inward:

```text
Infrastructure
  ├── Application
  └── Domain
```

This means Infrastructure can implement Application abstractions.

Application cannot depend on Infrastructure.

**Correct:**

```text
Application → IIdentityService
Infrastructure → IdentityService
```

**Incorrect:**

```text
Application → IdentityService
```

when `IdentityService` is the concrete Infrastructure implementation.

---

## 39. Application and API Dependency

The API depends on Application:

```text
API
 |
 v
Application
 |
 v
Domain
```

Controllers should primarily:

1. Receive HTTP requests.
2. Translate requests into Application commands or queries.
3. Send them through MediatR.
4. Translate results into HTTP responses.

The controller should not contain business orchestration.

---

## 40. Application and ReadModel Dependency

The ReadModel implements Application-owned query contracts.

Therefore:

```text
API
 ├── Application
 ├── Infrastructure
 └── ReadModel
          |
          v
     Application
```

This allows Application to define:

* `ICustomerProfileQuery`
* `IOrderSummaryQuery`
* `IProductSearchQuery`

while ReadModel provides the specialized implementation.

Application remains independent of ADO.NET and stored-procedure details.

---

## 41. Application Dependency Diagram

The overall dependency architecture is:

```text
                 ┌─────────────────┐
                 │       API       │
                 └────────┬────────┘
                          |
                          v
                 ┌─────────────────┐
                 │   Application   │
                 └────────┬────────┘
                          |
                          v
                 ┌─────────────────┐
                 │     Domain      │
                 └─────────────────┘


                 ┌─────────────────┐
                 │ Infrastructure  │
                 └───────┬─┬───────┘
                         | |
                         | └──────────► Domain
                         |
                         └────────────► Application


                 ┌─────────────────┐
                 │    ReadModel    │
                 └────────┬────────┘
                          |
                          v
                 ┌─────────────────┐
                 │   Application   │
                 └─────────────────┘
```

The critical rule is:

**Infrastructure and ReadModel depend inward on Application; Application never depends outward on Infrastructure or ReadModel implementations.**

---

## 42. Application Use-Case Flow

A typical command follows:

```text
HTTP Request
     |
     v
API Controller
     |
     v
Application Command
     |
     v
MediatR
     |
     v
Application Pipeline
     |
     v
Command Handler
     |
     v
Repository / Application Service
     |
     v
Domain Aggregate
     |
     v
Domain Events
     |
     v
Unit of Work
     |
     v
Infrastructure Persistence
     |
     v
SQL Server
```

If an integration event is produced:

```text
Domain Event
    |
    v
Integration Event
    |
    v
Outbox
    |
    v
Commit
    |
    v
Hangfire
    |
    v
Dispatcher
    |
    v
Application Handler
```

---

## 43. Query Flow

A normal read:

```text
HTTP Request
     |
     v
API
     |
     v
Application Query
     |
     v
Query Handler
     |
     v
Repository
     |
     v
EF Core
     |
     v
SQL Server
```

A specialized read:

```text
HTTP Request
     |
     v
API
     |
     v
Application Query
     |
     v
Query Handler
     |
     v
ReadModel Query Contract
     |
     v
ReadModel
     |
     v
ADO.NET
     |
     v
SQL Server
```

The choice depends on the query's requirements.

---

## 44. Application Boundaries

The Application layer must maintain the following boundaries.

### Application must not

* Access `DbContext` directly.
* Execute raw SQL for normal application persistence.
* Call Hangfire directly.
* Call Redis directly.
* Call SignalR directly.
* Call Firebase directly.
* Call Twilio directly.
* Call SMTP directly.
* Call Paymob directly.
* Depend on ASP.NET Core Identity implementation types.
* Depend on HTTP-specific infrastructure.
* Reproduce aggregate business rules.
* Bypass Domain aggregates for business state transitions.

### Application may

* Define abstractions for these capabilities.
* Orchestrate use cases.
* Invoke Domain behavior.
* Coordinate repositories.
* Coordinate Unit of Work.
* Define commands and queries.
* Define application events and contracts.
* Define scheduling contracts.
* Define notification intent.
* Define authorization requirements.
* Consume specialized ReadModel query contracts.

---

## 45. Application Services

Application services may be used when a workflow does not naturally fit a single command handler or when a capability needs a reusable orchestration boundary.

They should remain focused on:

* Application workflows
* Coordination
* External capability invocation
* Transaction boundaries
* Use-case sequencing

They should not become generic containers for business logic.

If behavior represents a business rule, it belongs in Domain.

---

## 46. DTOs and Application Results

Application owns internal use-case contracts and result models where appropriate.

These models represent application-level data rather than HTTP wire contracts.

The API may translate them into API-specific request and response DTOs.

Therefore:

```text
API DTO
  ≠
Application DTO
  ≠
Domain Entity
```

This prevents HTTP representation from leaking into application or domain design.

---

## 47. Application Does Not Own Persistence Technology

The Application layer defines what persistence capability is required.

For example:

```csharp
IOrderRepository
```

expresses the required repository capability.

Infrastructure decides that the implementation uses:

* EF Core
* `DbContext`
* SQL Server

Likewise:

```csharp
ICustomerProfileQuery
```

expresses a read capability.

ReadModel decides that it uses:

* ADO.NET
* `SqlConnection`
* `SqlCommand`
* `SqlDataReader`
* Stored procedures

This is Dependency Inversion in practice.

---

## 48. Testing Strategy

Application use cases should primarily be testable without requiring infrastructure implementations.

The architecture allows:

```text
Command Handler
     |
     v
Mock / fake Application abstractions
     |
     v
Domain behavior
```

For example, a handler can test orchestration using test doubles for:

* Repositories
* Identity services
* Notification services
* Scheduling services
* External capabilities

The specialized ReadModel implementation is tested separately against real SQL Server because its correctness depends on actual database behavior.

Domain logic remains independently testable.

---

## 49. Security Boundary

Application is responsible for application-level authorization requirements.

Sensitive operations must not rely solely on the API layer.

For example:

```text
API
 |
 └── Authentication / boundary authorization

Application
 |
 └── Permission requirement

Domain
 |
 └── Business invariants

Infrastructure
 |
 └── ASP.NET Core authorization implementation
```

This provides defense in depth while preserving layer responsibilities.

---

## 50. Reliability and Failure Handling

The Application layer coordinates failure-aware workflows but does not own infrastructure retry mechanisms.

### Database failure

```text
Application
     |
     v
Unit of Work
     |
     v
Infrastructure
     |
     v
SQL failure
```

Infrastructure handles technical persistence behavior and translates appropriate failures.

### Outbox failure

```text
Transaction succeeds
       |
       v
Outbox persists
       |
       v
Background processing
       |
       v
Temporary delivery failure
       |
       v
Hangfire retry
```

### External payment failure

```text
Application payment workflow
         |
         v
Infrastructure Paymob adapter
         |
         v
External failure
         |
         v
Application-facing failure
```

The Application layer should not implement duplicate infrastructure retry systems when the underlying infrastructure already owns them.

---

## 51. Application-Level Consistency

The Application layer defines the consistency requirements of each use case.

For a single aggregate operation:

```text
Command
  |
  v
Aggregate
  |
  v
Unit of Work
  |
  v
Commit
```

For operations requiring multiple persistence changes within the same module transaction:

```text
Application workflow
       |
       v
Multiple changes
       |
       v
Single Unit of Work
       |
       v
Single transaction
       |
       v
Commit
```

For asynchronous cross-module work:

```text
Transaction
    |
    v
Outbox
    |
    v
Post-commit processing
```

This allows the system to maintain strong transactional consistency where required without introducing distributed transactions.

---

## 52. Modular Monolith Alignment

The Application layer is designed for a modular monolith.

Modules remain logically separated while running inside one process.

The architecture can therefore use:

* One process
* One application
* One SQL Server database
* One shared Unit of Work where atomicity is required
* Multiple domain and application modules

Application boundaries prevent accidental coupling even though modules share the same runtime.

The architecture does not require:

* Microservices
* Service-to-service HTTP
* Kafka
* RabbitMQ
* Distributed transactions

unless the system later becomes genuinely distributed.

---

## 53. Shared Application Components

Shared application code should remain deliberately small.

Only genuinely cross-module concerns should be placed in common or shared areas.

Examples may include:

* Common application result abstractions
* Shared pipeline behaviors
* Common authorization abstractions
* Common application interfaces

Business-specific functionality should remain inside its owning module.

The rule is:

**Shared code must represent a genuine shared capability, not merely convenient code reuse.**

---

## 54. Application vs Domain vs Infrastructure

| Concern                                | Application                         | Domain                         | Infrastructure                      |
| -------------------------------------- | ----------------------------------- | ------------------------------ | ----------------------------------- |
| Use-case orchestration                 | ✓                                   | —                              | —                                   |
| Commands                               | ✓                                   | —                              | —                                   |
| Queries                                | ✓                                   | —                              | —                                   |
| Aggregate business rules               | —                                   | ✓                              | —                                   |
| Value objects                          | —                                   | ✓                              | —                                   |
| Domain events                          | —                                   | ✓                              | —                                   |
| Integration contracts                  | ✓, where application-owned          | May own domain event contracts | Implements persistence and delivery |
| Repository abstraction                 | ✓ or Domain, depending on ownership | ✓, where domain-owned          | —                                   |
| Repository implementation              | —                                   | —                              | ✓                                   |
| Unit of Work abstraction               | ✓ or Domain, depending on ownership | ✓, where domain-owned          | —                                   |
| Unit of Work implementation            | —                                   | —                              | ✓                                   |
| EF Core                                | —                                   | —                              | ✓                                   |
| SQL Server persistence                 | —                                   | —                              | ✓                                   |
| ASP.NET Core Identity                  | —                                   | —                              | ✓                                   |
| JWT implementation                     | —                                   | —                              | ✓                                   |
| Hangfire                               | —                                   | —                              | ✓                                   |
| Redis                                  | —                                   | —                              | ✓                                   |
| SignalR                                | —                                   | —                              | ✓                                   |
| Firebase                               | —                                   | —                              | ✓                                   |
| Paymob integration                     | —                                   | —                              | ✓                                   |
| ADO.NET ReadModel implementation       | —                                   | —                              | ReadModel                           |
| Business invariants                    | —                                   | ✓                              | —                                   |
| Application authorization requirements | ✓                                   | —                              | —                                   |
| HTTP                                   | —                                   | —                              | API                                 |
| API DTOs                               | —                                   | —                              | API                                 |

---

## 55. Final Architectural Rules

The Application layer follows these rules:

1. **Application owns use cases and orchestration.**
2. **Domain owns business rules and invariants.**
3. **Application depends inward on Domain.**
4. **Domain never depends on Application.**
5. **Application does not depend on Infrastructure implementations.**
6. **Infrastructure implements Application-facing abstractions.**
7. **Commands represent state-changing use cases.**
8. **Queries represent read use cases.**
9. **CQRS is applied selectively rather than mechanically.**
10. **Specialized reads may be implemented by ReadModel.**
11. **Application owns ReadModel query contracts.**
12. **ReadModel owns ADO.NET and stored-procedure implementation details.**
13. **Application does not directly use EF Core.**
14. **Application does not directly use Hangfire.**
15. **Application does not directly use Redis, SignalR, Firebase, Twilio, SMTP, or Paymob.**
16. **External technologies are accessed through application-facing capabilities.**
17. **Domain aggregates remain authoritative for business state transitions.**
18. **Application handlers orchestrate rather than reproduce domain rules.**
19. **Unit of Work defines transactional application boundaries through an abstraction.**
20. **Outbox provides durable asynchronous integration.**
21. **Background execution is scheduler-agnostic at the Application boundary.**
22. **Identity remains an Application capability with ASP.NET Identity implemented by Infrastructure.**
23. **Authorization requirements belong at the Application/use-case boundary, while ASP.NET Core authorization implementation belongs to Infrastructure.**
24. **Notifications express application intent; Infrastructure performs delivery.**
25. **Application DTOs remain separate from API wire contracts.**
26. **Shared Application code must remain genuinely shared.**
27. **The Application layer must remain independent of HTTP and infrastructure frameworks wherever practical.**
28. **The modular-monolith boundary is preserved through explicit application contracts and module ownership.**

---

## 56. Final Responsibility Matrix

| Layer              | Primary Responsibility                                                                         |
| ------------------ | ---------------------------------------------------------------------------------------------- |
| **API**            | HTTP boundary, request/response translation, composition                                       |
| **Application**    | Use cases, orchestration, CQRS, contracts, application workflows                               |
| **Domain**         | Business rules, aggregates, invariants, domain events                                          |
| **Infrastructure** | EF Core, SQL persistence, Identity, external services, messaging, jobs, caching, observability |
| **ReadModel**      | Specialized high-performance read implementation using ADO.NET                                 |
| **Database**       | Durable application data and transactional persistence                                         |

The central relationship is:

```text
                   APPLICATION
                        |
         ┌──────────────┼──────────────┐
         v              v              v
      DOMAIN      INFRASTRUCTURE    READMODEL
      authority    implementation   specialized reads
```

More precisely, the dependency direction is:

```text
API ───────────────► Application ─────────────► Domain
 │                         ▲
 │                         │
 ├────────────► Infrastructure
 │
 └────────────► ReadModel ─────────────► Application
```

---

## 57. Final Architecture Summary

`E-Commerce.Application` is the **use-case and orchestration layer** of the modular monolith.

It provides the system's application capabilities through commands, queries, handlers, application services, contracts, authorization requirements, scheduling abstractions, notification intent, and specialized read contracts.

It coordinates Domain behavior without replacing it.

It defines persistence and external-service capabilities without implementing their technologies.

It provides the CQRS boundary without forcing every read into a separate read model.

It enables specialized high-performance reads through the `E-Commerce.ReadModel` subsystem while keeping ADO.NET and SQL implementation details outside Application.

It enables durable asynchronous workflows through the transactional outbox and scheduler-agnostic background execution abstractions.

It keeps ASP.NET Core Identity, EF Core, Hangfire, Redis, SignalR, Firebase, Paymob, SMTP, Twilio, and other infrastructure technologies outside the use-case layer.

The resulting architecture can therefore be summarized as:

**API translates HTTP. Application orchestrates use cases. Domain enforces business rules. Infrastructure implements technical capabilities. ReadModel optimizes specialized reads.**

This preserves Clean Architecture, DDD, CQRS, Dependency Inversion, transactional consistency, modular boundaries, and infrastructure independence while remaining appropriate for a production-oriented modular monolith.
