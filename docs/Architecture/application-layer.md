\# E-Commerce.Application — Architecture Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Layer:\*\* Application

\*\*Version:\*\* 1.0 – Final



\---



\## 1. Overview



The `E-Commerce.Application` project is the \*\*application/use-case layer\*\* of the E-Commerce modular monolith.



It coordinates application workflows between the API, Domain, Infrastructure, and specialized ReadModel subsystem while keeping business use-case orchestration independent from infrastructure technologies.



The Application layer is responsible for answering:



\*\*“What does the system need to do in response to a use case?”\*\*



It coordinates domain behavior, persistence abstractions, authorization requirements, integration contracts, queries, commands, background-work contracts, and other application-level workflows.



It does \*\*not\*\* own:



\* HTTP

\* EF Core

\* SQL Server

\* ASP.NET Core Identity implementation

\* Hangfire

\* Redis

\* SignalR

\* Firebase

\* SMTP

\* Twilio

\* Paymob

\* ADO.NET implementation

\* Concrete infrastructure services



Those concerns belong to API, Infrastructure, or ReadModel.



The Application layer therefore acts as the \*\*orchestration boundary between business capabilities and technical implementations\*\*.



\---



\## 2. Architectural Philosophy



The Application layer follows the principles of:



\* Clean Architecture

\* Domain-Driven Design

\* CQRS

\* Dependency Inversion

\* Explicit use-case orchestration

\* Separation of commands and queries

\* Infrastructure abstraction

\* Domain authority over business rules

\* Framework independence where practical



The central rule is:



\*\*Application coordinates; Domain decides; Infrastructure implements; API translates HTTP.\*\*



For example:



```text

HTTP Request

&#x20;    |

&#x20;    v

API Controller

&#x20;    |

&#x20;    v

Application Command

&#x20;    |

&#x20;    v

Application Handler

&#x20;    |

&#x20;    v

Domain Aggregate

&#x20;    |

&#x20;    v

Repository / Unit of Work abstraction

&#x20;    |

&#x20;    v

Infrastructure implementation

&#x20;    |

&#x20;    v

SQL Server

```



For specialized reads:



```text

HTTP Request

&#x20;    |

&#x20;    v

API Controller

&#x20;    |

&#x20;    v

Application Query

&#x20;    |

&#x20;    v

ReadModel Query Service

&#x20;    |

&#x20;    v

ADO.NET

&#x20;    |

&#x20;    v

SQL Server

```



The Application layer does not need to know whether persistence is implemented using EF Core, ADO.NET, or another technology.



\---



\## 3. Primary Responsibilities



The Application layer owns the following responsibilities.



\### 3.1 Use Cases



Defines the application's supported operations through:



\* Commands

\* Command handlers

\* Queries

\* Query handlers

\* Application services where appropriate



Examples include:



\* Register user

\* Verify email

\* Verify phone

\* Complete onboarding

\* Create person

\* Place order

\* Cancel order

\* Capture payment

\* Request refund

\* Create shipment

\* Add product to wishlist

\* Submit rating

\* Retrieve specialized read models



\### 3.2 Application Orchestration



Application handlers coordinate multiple operations when a use case crosses multiple abstractions.



For example:



```text

Place Order

&#x20;    |

&#x20;    v

Load Cart

&#x20;    |

&#x20;    v

Validate application-level conditions

&#x20;    |

&#x20;    v

Invoke Order domain behavior

&#x20;    |

&#x20;    v

Persist Order

&#x20;    |

&#x20;    v

Commit Unit of Work

```



The handler coordinates the workflow.



The Order aggregate remains responsible for its own business invariants.



\### 3.3 Application Contracts



Application owns contracts required by the use cases.



Examples include:



\* Repository interfaces

\* Query interfaces

\* Identity service contracts

\* Notification contracts

\* File service contracts

\* Scheduling contracts

\* Current-user abstractions

\* Authorization requirements

\* ReadModel query contracts



These contracts allow Application to depend on capabilities rather than technical implementations.



\### 3.4 CQRS



The Application layer provides the CQRS boundary.



Commands represent operations that change application state.



Queries represent operations that retrieve information.



Conceptually:



```text

Command

&#x20;  |

&#x20;  v

Command Handler

&#x20;  |

&#x20;  v

Domain / Persistence

```



And:



```text

Query

&#x20;  |

&#x20;  v

Query Handler

&#x20;  |

&#x20;  v

Read abstraction

```



CQRS is \*\*selective\*\*, not mandatory for every operation.



Simple reads may continue to use the normal write-side persistence model.



Specialized queries may use `E-Commerce.ReadModel`.



\---



\## 4. Application Is Not the Domain Layer



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



The Application layer determines \*\*when and how the operation is invoked\*\*.



The Domain determines \*\*whether the business operation is valid\*\*.



\---



\## 5. Domain Dependency



The Application layer depends on the Domain layer.



```text

Application

&#x20;    |

&#x20;    v

Domain

```



This allows Application handlers to use:



\* Aggregates

\* Entities

\* Value objects

\* Domain exceptions

\* Domain events

\* Domain repository contracts

\* Domain services and policies



The reverse dependency is prohibited.



```text

Domain

&#x20;  X

Application

```



The Domain must never depend on Application.



\---



\## 6. Application Project Structure



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



\---



\## 7. Business-Oriented Application Modules



The primary business-oriented modules correspond closely to the Domain bounded contexts.



These include:



\* Catalog

\* CustomerEngagement

\* Finance

\* Orders

\* People

\* Shipping



These modules contain application use cases that orchestrate the corresponding domain capabilities.



For example:



```text

Application.Modules.Orders

&#x20;         |

&#x20;         v

Domain.Ordering

```



And:



```text

Application.Modules.Finance

&#x20;         |

&#x20;         v

Domain.Finance

```



The Application layer does not duplicate the Domain structure mechanically; it exposes the \*\*use cases required by the application\*\*.



\---



\## 8. Application Workflow and Platform Modules



Some Application modules represent workflows or application capabilities rather than Domain bounded contexts.



Examples include:



\* Identity

\* Authentication

\* Authorization

\* Notifications

\* Scheduling

\* Profiles

\* Onboarding



These are intentionally treated differently from genuine Domain bounded contexts.



For example, \*\*Identity\*\* is an application capability backed by ASP.NET Core Identity in Infrastructure.



It is not a Domain aggregate or Domain bounded context.



\---



\## 9. Onboarding



Onboarding coordinates the user-registration workflow.



The current registration lifecycle is:



```text

Registration Request

&#x20;       |

&#x20;       v

Registration aggregate

&#x20;       |

&#x20;       v

Email verification

&#x20;       |

&#x20;       v

Phone verification

&#x20;       |

&#x20;       v

Fully verified

&#x20;       |

&#x20;       v

Identity creation

&#x20;       |

&#x20;       v

Authentication

&#x20;       |

&#x20;       v

Person creation / completion

&#x20;       |

&#x20;       v

Business operations

```



The Application layer coordinates this process.



The Domain layer owns registration business rules and state transitions.



Infrastructure implements persistence and ASP.NET Identity.



Therefore:



```text

Application

&#x20;   ├── Coordinates registration workflow

&#x20;   ├── Invokes Registration aggregate

&#x20;   └── Invokes identity capability



Domain

&#x20;   └── Owns Registration business rules



Infrastructure

&#x20;   ├── Persists Registration

&#x20;   └── Implements ASP.NET Identity

```



\---



\## 10. Identity



Identity is an Application capability rather than a Domain bounded context.



The Application layer may define an abstraction such as:



```csharp

IIdentityService

```



Infrastructure implements it using:



\* `UserManager<User>`

\* `SignInManager<User>`

\* ASP.NET Core Identity



The Application layer therefore does not depend directly on:



\* `IdentityUser`

\* `UserManager`

\* `SignInManager`

\* EF Core Identity stores



Those belong to Infrastructure.



The important boundary is:



\*\*Application expresses the identity operation; Infrastructure performs it using ASP.NET Core Identity.\*\*



\---



\## 11. Authentication



Authentication orchestration belongs to Application-level workflows.



Typical responsibilities include:



\* Login

\* Refresh-token workflow

\* Logout and revocation

\* Credential operations

\* External-login workflow where applicable

\* Authentication-related application decisions



Infrastructure implements the technical mechanisms:



\* JWT generation

\* Refresh-token hashing

\* ASP.NET Identity

\* Cryptographic services

\* Authentication middleware



The Application layer should therefore depend on capabilities rather than token-generation implementation details.



\---



\## 12. Authorization



Authorization has two complementary responsibilities.



\### Application



Application owns application-level permission requirements and authorization decisions associated with use cases.



For example:



\* Create Product

\* Manage Orders

\* Process Refund

\* Manage Permissions



The Application use case may require a permission such as:



```text

Catalog.Products.Create

```



\### Infrastructure



Infrastructure implements the ASP.NET Core authorization machinery:



\* `IAuthorizationPolicyProvider`

\* Authorization handlers

\* Policies

\* Requirements

\* Permission persistence



Therefore:



```text

Application

&#x20;    |

&#x20;    v

Permission requirement / capability

&#x20;    |

&#x20;    v

Infrastructure

&#x20;    |

&#x20;    v

ASP.NET Core Authorization

```



\---



\## 13. Commands



Commands represent application operations that change state.



Conceptually:



\* `CreateProductCommand`

\* `PlaceOrderCommand`

\* `CancelOrderCommand`

\* `CapturePaymentCommand`

\* `RequestRefundCommand`

\* `VerifyEmailCommand`

\* `VerifyPhoneCommand`

\* `CreatePersonCommand`

\* `CreateShipmentCommand`



A command contains the data required to request an operation.



It should not contain infrastructure behavior.



Example:



```csharp

public sealed record PlaceOrderCommand(

&#x20;   Guid CartId);

```



The command itself does not execute the operation.



Its handler performs the application orchestration.



\---



\## 14. Command Handlers



Command handlers are responsible for executing application use cases.



A typical flow is:



```text

Command

&#x20;  |

&#x20;  v

Handler

&#x20;  |

&#x20;  v

Load required state

&#x20;  |

&#x20;  v

Invoke domain behavior

&#x20;  |

&#x20;  v

Persist changes

&#x20;  |

&#x20;  v

Commit Unit of Work

```



Handlers should remain orchestration-oriented.



They should not become large procedural implementations of business rules.



For example, `Order.Place(...)` belongs to the Domain.



The handler should call it rather than reproduce its rules.



\---



\## 15. Queries



Queries represent read operations.



Examples:



\* `GetProductQuery`

\* `GetOrderQuery`

\* `GetCustomerProfileQuery`

\* `GetPaymentQuery`

\* `GetShipmentQuery`

\* `GetWishlistQuery`



Application owns the query contract.



The implementation may use:



\* Write-side EF Core

\* ReadModel

\* Specialized ADO.NET

\* Another approved read implementation



The choice depends on the query's requirements.



\---



\## 16. Specialized ReadModel Queries



The Application layer owns the abstraction for specialized read operations.



For example:



```text

Application

&#x20;    |

&#x20;    v

ICustomerProfileQuery

&#x20;    |

&#x20;    v

ReadModel

&#x20;    |

&#x20;    v

ADO.NET

```



The Application layer does not know that the implementation uses:



\* `SqlConnection`

\* `SqlCommand`

\* `SqlDataReader`

\* Stored procedures

\* DbUp



Those are ReadModel implementation details.



This preserves the dependency boundary:



```text

API

&#x20;├── Application

&#x20;├── Infrastructure

&#x20;└── ReadModel



ReadModel

&#x20;   └── Application

```



Infrastructure does not depend on ReadModel.



\---



\## 17. Selective CQRS



CQRS is applied where it provides practical value.



The architecture does \*\*not\*\* require every read to use the specialized ReadModel.



A simple read may remain:



```text

Application Query

&#x20;      |

&#x20;      v

EF Core Repository

&#x20;      |

&#x20;      v

SQL Server

```



A complex or performance-sensitive read may use:



```text

Application Query

&#x20;      |

&#x20;      v

ReadModel Contract

&#x20;      |

&#x20;      v

ADO.NET

&#x20;      |

&#x20;      v

Stored Procedure

&#x20;      |

&#x20;      v

SQL Server

```



Selection is based on:



\* Query shape

\* Aggregation requirements

\* Performance characteristics

\* Reporting complexity

\* Read-side optimization requirements



CQRS is not applied mechanically.



\---



\## 18. Repository Abstractions



Repository interfaces provide persistence ports for domain aggregates and application workflows.



Examples include:



\* `IProductRepository`

\* `IOrderRepository`

\* `IPaymentRepository`

\* `IRefundRepository`

\* `IPersonRepository`

\* `IShipmentRepository`

\* `IRegistrationRepository`



The implementation belongs to Infrastructure.



```text

Application / Domain abstraction

&#x20;            |

&#x20;            v

Infrastructure repository

&#x20;            |

&#x20;            v

EF Core

&#x20;            |

&#x20;            v

SQL Server

```



The Application layer should not instantiate EF Core repositories directly.



\---



\## 19. Unit of Work



Application use cases may depend on the `IUnitOfWork` abstraction.



The Application handler determines the transactional boundary of a use case.



For example:



```text

Load aggregate

&#x20;     |

&#x20;     v

Perform domain operation

&#x20;     |

&#x20;     v

Persist changes

&#x20;     |

&#x20;     v

Commit Unit of Work

```



Infrastructure provides the actual EF Core implementation.



The Application layer therefore does not depend directly on:



\* `DbContext`

\* `IDbContextTransaction`

\* `SaveChangesAsync`

\* EF Core transaction APIs



\---



\## 20. Transactional Boundaries



The Application layer is responsible for defining \*\*which operations belong to the same use-case transaction\*\*.



Infrastructure is responsible for implementing the transaction.



For example:



```text

Place Order

&#x20; ├── Modify Order

&#x20; ├── Persist Order

&#x20; ├── Persist Outbox message

&#x20; └── Commit

```



The Application layer should not manually manipulate database transactions.



Instead:



```text

Application

&#x20;    |

&#x20;    v

IUnitOfWork

&#x20;    |

&#x20;    v

Infrastructure UnitOfWork

&#x20;    |

&#x20;    v

EF Core transaction

```



This maintains separation between application intent and persistence technology.



\---



\## 21. Domain Events



Domain events originate in the Domain layer.



Application may participate in workflows that react to domain events, but it does not redefine them.



Example:



```text

Order

&#x20; |

&#x20; v

OrderPaidDomainEvent

```



Infrastructure provides event-dispatching mechanisms.



The conceptual responsibility is:



\* \*\*Domain:\*\* Defines the business event.

\* \*\*Application:\*\* Coordinates the application response where appropriate.

\* \*\*Infrastructure:\*\* Provides dispatch, persistence, and transport mechanisms.



\---



\## 22. Integration Events



Integration events represent messages crossing module or application boundaries.



The producing capability owns the integration-event contract.



For example:



```text

Finance

&#x20;  |

&#x20;  v

PaymentCompletedIntegrationEvent

```



Ordering can consume that event:



```text

PaymentCompletedIntegrationEvent

&#x20;             |

&#x20;             v

&#x20;      Ordering handler

```



The durable flow is:



```text

Domain Event

&#x20;    |

&#x20;    v

Application / integration-event handling

&#x20;    |

&#x20;    v

Outbox Message

&#x20;    |

&#x20;    v

Commit

&#x20;    |

&#x20;    v

Hangfire OutboxProcessingJob

&#x20;    |

&#x20;    v

Dispatcher

&#x20;    |

&#x20;    v

Integration Handler

```



The Application layer owns the business and application contracts involved in this workflow.



Infrastructure owns the outbox storage and execution machinery.



\---



\## 23. Outbox Boundary



The Application layer must not directly publish durable external messages from inside a command handler.



Instead, the architecture uses the transactional outbox.



Conceptually:



```text

Application use case

&#x20;      |

&#x20;      v

Domain state change

&#x20;      |

&#x20;      v

Integration event

&#x20;      |

&#x20;      v

Outbox persistence

&#x20;      |

&#x20;      v

Same transaction

&#x20;      |

&#x20;      v

Commit

```



Infrastructure later processes the outbox.



This prevents a successful database transaction from being followed by a lost message because the process failed before publication.



\---



\## 24. Background Work and Scheduling



Scheduling is an Application capability.



The Application layer defines scheduler-agnostic contracts such as:



\* `IJob`

\* `IJobContext`

\* `IJobDispatcher`

\* `IJobExecutionEngine`

\* `IJobHandler<T>`

\* `IJobScheduler`



The purpose is to prevent application and business logic from depending directly on Hangfire.



The flow is:



```text

Application

&#x20;    |

&#x20;    v

IJobScheduler

&#x20;    |

&#x20;    v

Infrastructure

&#x20;    |

&#x20;    v

Hangfire

```



Hangfire is therefore an implementation detail.



Application code should not directly contain the following for business scheduling:



```csharp

BackgroundJob.Enqueue(...);

RecurringJob.AddOrUpdate(...);

```



\---



\## 25. Controlled Execution Gateway



The Application scheduling subsystem acts as a controlled execution gateway.



It provides a consistent place for execution concerns such as:



\* Job dispatch

\* Execution context

\* Timeout handling

\* Logging context

\* Execution pipeline

\* Failure handling

\* Circuit-breaking where applicable



Infrastructure adapts this abstraction to Hangfire.



This creates:



```text

Business / Application code

&#x20;            |

&#x20;            v

Application Scheduling Abstraction

&#x20;            |

&#x20;            v

Execution Gateway

&#x20;            |

&#x20;            v

Hangfire Adapter

&#x20;            |

&#x20;            v

Hangfire

```



The business layer therefore remains scheduler-agnostic.



\---



\## 26. Notifications



The Application layer expresses notification intent.



Business modules determine:



\* What happened

\* Who should be notified

\* Why they should be notified

\* When the notification should occur

\* What notification content is required



Infrastructure determines how the notification is delivered.



Examples:



```text

Application

&#x20;    |

&#x20;    v

Email notification request

&#x20;    |

&#x20;    v

Infrastructure EmailChannel

&#x20;    |

&#x20;    v

SMTP / provider

```



Or:



```text

Application

&#x20;    |

&#x20;    v

Push notification request

&#x20;    |

&#x20;    v

Infrastructure PushChannel

&#x20;    |

&#x20;    v

Firebase

```



The Application layer does not depend on SMTP, Twilio, Firebase, or SignalR implementations.



\---



\## 27. Realtime Notifications



Realtime delivery is deliberately separated from durable notification delivery.



The Application layer may produce a notification or application event.



Infrastructure may then provide SignalR as a best-effort realtime hint.



The architectural distinction is:



```text

Database

&#x20;   |

&#x20;   v

Source of truth



SignalR

&#x20;   |

&#x20;   v

Realtime client hint

```



SignalR is therefore not the durable notification mechanism.



\---



\## 28. File Service Abstraction



Application-facing file operations are expressed through abstractions such as:



```csharp

IFileService

```



The Application layer does not know whether the underlying storage is:



\* Local filesystem

\* Amazon S3

\* Azure Blob Storage

\* Another provider



Infrastructure implements:



```csharp

IFileStorageProvider

```



and connects it to the concrete storage technology.



Therefore:



```text

Application

&#x20;    |

&#x20;    v

IFileService

&#x20;    |

&#x20;    v

Infrastructure

&#x20;    |

&#x20;    v

IFileStorageProvider

&#x20;    |

&#x20;    v

Local / S3 / Azure / etc.

```



\---



\## 29. Current User Abstraction



Application use cases may require knowledge of the currently authenticated user.



Rather than depending on ASP.NET Core HTTP abstractions, Application should depend on an application-facing abstraction such as:



```csharp

ICurrentUser

```



The Infrastructure implementation resolves the current identity from the authentication context.



This prevents application use cases from depending directly on:



\* `HttpContext`

\* `ClaimsPrincipal`

\* ASP.NET Core authentication implementation



where those details are unnecessary.



\---



\## 30. External Services



Application defines capabilities required from external systems.



Examples include:



\* Identity operations

\* Payment operations

\* Notification operations

\* File operations

\* Scheduling operations

\* Clock and time operations



Infrastructure implements those capabilities.



For example:



```text

Application

&#x20;    |

&#x20;    v

IPaymentGateway

&#x20;    |

&#x20;    v

Infrastructure

&#x20;    |

&#x20;    v

Paymob

```



The Application layer must not depend directly on:



\* Paymob SDK/API models

\* `HttpClient` implementation details

\* Firebase SDK

\* Twilio SDK

\* SMTP APIs

\* Redis APIs

\* Hangfire APIs



\---



\## 31. Payment Workflow



Finance remains a Domain bounded context.



Application orchestrates payment use cases.



For example:



```text

Capture Payment Command

&#x20;         |

&#x20;         v

Finance Application Handler

&#x20;         |

&#x20;         v

Payment Aggregate

&#x20;         |

&#x20;         v

Payment business operation

&#x20;         |

&#x20;         v

Repository

&#x20;         |

&#x20;         v

Unit of Work

&#x20;         |

&#x20;         v

Outbox

```



External Paymob communication is Infrastructure.



Therefore:



\* \*\*Application:\*\* Payment use case.

\* \*\*Domain:\*\* Payment business rules.

\* \*\*Infrastructure:\*\* Paymob communication, webhook verification, and persistence.



Webhook requests must not bypass the Application/Finance workflow.



\---



\## 32. Shipping Workflow



Shipping use cases follow the same separation.



Application coordinates:



\* Create Shipment

\* Ship Shipment

\* Deliver Shipment

\* Record Delivery Attempt

\* Return Shipment

\* Cancel Shipment



Domain owns shipment state transitions and invariants.



Infrastructure owns:



\* External location services

\* Map providers

\* Persistence

\* Technical communication



The Application layer should not reproduce shipment state-machine rules.



\---



\## 33. Validation



Validation exists at multiple architectural levels.



\### API



Validates HTTP and wire-level concerns.



\### Application



Validates application-level input and use-case requirements.



\### Domain



Enforces business invariants.



For example:



```text

API

&#x20;|

&#x20;└── Request shape



Application

&#x20;|

&#x20;└── Required application conditions



Domain

&#x20;|

&#x20;└── Business invariant

```



Application validation must not become a second copy of Domain validation.



\---



\## 34. Exception Boundaries



Application may allow domain and application exceptions to propagate toward the API boundary.



Examples include:



\* `BusinessRuleViolationException`

\* `DomainException`

\* `NotFoundException`

\* `ForbiddenException`

\* `ConcurrencyException`

\* `ValidationException`



Infrastructure may translate technical failures into appropriate application-facing exceptions where required.



The API's global exception handler ultimately translates those exceptions into HTTP responses.



Conceptually:



```text

Domain / Infrastructure

&#x20;         |

&#x20;         v

Application boundary

&#x20;         |

&#x20;         v

API exception handler

&#x20;         |

&#x20;         v

HTTP response

```



\---



\## 35. Optimistic Concurrency



Optimistic concurrency is primarily implemented by Infrastructure persistence mechanisms.



Application participates by treating concurrency failure as an application-level failure.



For example:



```text

Application

&#x20;    |

&#x20;    v

Update aggregate

&#x20;    |

&#x20;    v

Unit of Work

&#x20;    |

&#x20;    v

Concurrency conflict

&#x20;    |

&#x20;    v

Infrastructure translates exception

&#x20;    |

&#x20;    v

Application / API boundary

&#x20;    |

&#x20;    v

HTTP 409 Conflict

```



The Application layer does not implement SQL Server row-version mechanics.



\---



\## 36. Application Behaviors



Cross-cutting application behavior may be implemented through MediatR pipeline behaviors.



Examples include:



\* Validation

\* Logging context

\* Performance measurement

\* Transaction behavior where appropriate

\* Authorization

\* Other application-level policies



These behaviors operate around commands and queries.



They should not become hidden substitutes for explicit domain behavior.



The architecture therefore distinguishes:



\*\*Application pipeline behavior ≠ Domain business rule\*\*



\---



\## 37. Performance Behavior



The Application pipeline may measure use-case execution performance.



For example:



```text

Command / Query

&#x20;     |

&#x20;     v

PerformanceBehavior

&#x20;     |

&#x20;     v

Handler

```



This allows operation performance to be observed without placing timing logic into every handler.



Infrastructure and observability systems provide the actual telemetry implementation.



\---



\## 38. Application and Infrastructure Dependency



The dependency direction is:



```text

Application

&#x20;    |

&#x20;    v

Domain

```



Infrastructure points inward:



```text

Infrastructure

&#x20;  ├── Application

&#x20;  └── Domain

```



This means Infrastructure can implement Application abstractions.



Application cannot depend on Infrastructure.



\*\*Correct:\*\*



```text

Application → IIdentityService

Infrastructure → IdentityService

```



\*\*Incorrect:\*\*



```text

Application → IdentityService

```



when `IdentityService` is the concrete Infrastructure implementation.



\---



\## 39. Application and API Dependency



The API depends on Application:



```text

API

&#x20;|

&#x20;v

Application

&#x20;|

&#x20;v

Domain

```



Controllers should primarily:



1\. Receive HTTP requests.

2\. Translate requests into Application commands or queries.

3\. Send them through MediatR.

4\. Translate results into HTTP responses.



The controller should not contain business orchestration.



\---



\## 40. Application and ReadModel Dependency



The ReadModel implements Application-owned query contracts.



Therefore:



```text

API

&#x20;├── Application

&#x20;├── Infrastructure

&#x20;└── ReadModel

&#x20;         |

&#x20;         v

&#x20;    Application

```



This allows Application to define:



\* `ICustomerProfileQuery`

\* `IOrderSummaryQuery`

\* `IProductSearchQuery`



while ReadModel provides the specialized implementation.



Application remains independent of ADO.NET and stored-procedure details.



\---



\## 41. Application Dependency Diagram



The overall dependency architecture is:



```text

&#x20;                 ┌─────────────────┐

&#x20;                 │       API       │

&#x20;                 └────────┬────────┘

&#x20;                          |

&#x20;                          v

&#x20;                 ┌─────────────────┐

&#x20;                 │   Application   │

&#x20;                 └────────┬────────┘

&#x20;                          |

&#x20;                          v

&#x20;                 ┌─────────────────┐

&#x20;                 │     Domain      │

&#x20;                 └─────────────────┘





&#x20;                 ┌─────────────────┐

&#x20;                 │ Infrastructure  │

&#x20;                 └───────┬─┬───────┘

&#x20;                         | |

&#x20;                         | └──────────► Domain

&#x20;                         |

&#x20;                         └────────────► Application





&#x20;                 ┌─────────────────┐

&#x20;                 │    ReadModel    │

&#x20;                 └────────┬────────┘

&#x20;                          |

&#x20;                          v

&#x20;                 ┌─────────────────┐

&#x20;                 │   Application   │

&#x20;                 └─────────────────┘

```



The critical rule is:



\*\*Infrastructure and ReadModel depend inward on Application; Application never depends outward on Infrastructure or ReadModel implementations.\*\*



\---



\## 42. Application Use-Case Flow



A typical command follows:



```text

HTTP Request

&#x20;    |

&#x20;    v

API Controller

&#x20;    |

&#x20;    v

Application Command

&#x20;    |

&#x20;    v

MediatR

&#x20;    |

&#x20;    v

Application Pipeline

&#x20;    |

&#x20;    v

Command Handler

&#x20;    |

&#x20;    v

Repository / Application Service

&#x20;    |

&#x20;    v

Domain Aggregate

&#x20;    |

&#x20;    v

Domain Events

&#x20;    |

&#x20;    v

Unit of Work

&#x20;    |

&#x20;    v

Infrastructure Persistence

&#x20;    |

&#x20;    v

SQL Server

```



If an integration event is produced:



```text

Domain Event

&#x20;    |

&#x20;    v

Integration Event

&#x20;    |

&#x20;    v

Outbox

&#x20;    |

&#x20;    v

Commit

&#x20;    |

&#x20;    v

Hangfire

&#x20;    |

&#x20;    v

Dispatcher

&#x20;    |

&#x20;    v

Application Handler

```



\---



\## 43. Query Flow



A normal read:



```text

HTTP Request

&#x20;    |

&#x20;    v

API

&#x20;    |

&#x20;    v

Application Query

&#x20;    |

&#x20;    v

Query Handler

&#x20;    |

&#x20;    v

Repository

&#x20;    |

&#x20;    v

EF Core

&#x20;    |

&#x20;    v

SQL Server

```



A specialized read:



```text

HTTP Request

&#x20;    |

&#x20;    v

API

&#x20;    |

&#x20;    v

Application Query

&#x20;    |

&#x20;    v

Query Handler

&#x20;    |

&#x20;    v

ReadModel Query Contract

&#x20;    |

&#x20;    v

ReadModel

&#x20;    |

&#x20;    v

ADO.NET

&#x20;    |

&#x20;    v

SQL Server

```



The choice depends on the query's requirements.



\---



\## 44. Application Boundaries



The Application layer must maintain the following boundaries.



\### Application must not



\* Access `DbContext` directly.

\* Execute raw SQL for normal application persistence.

\* Call Hangfire directly.

\* Call Redis directly.

\* Call SignalR directly.

\* Call Firebase directly.

\* Call Twilio directly.

\* Call SMTP directly.

\* Call Paymob directly.

\* Depend on ASP.NET Core Identity implementation types.

\* Depend on HTTP-specific infrastructure.

\* Reproduce aggregate business rules.

\* Bypass Domain aggregates for business state transitions.



\### Application may



\* Define abstractions for these capabilities.

\* Orchestrate use cases.

\* Invoke Domain behavior.

\* Coordinate repositories.

\* Coordinate Unit of Work.

\* Define commands and queries.

\* Define application events and contracts.

\* Define scheduling contracts.

\* Define notification intent.

\* Define authorization requirements.

\* Consume specialized ReadModel query contracts.



\---



\## 45. Application Services



Application services may be used when a workflow does not naturally fit a single command handler or when a capability needs a reusable orchestration boundary.



They should remain focused on:



\* Application workflows

\* Coordination

\* External capability invocation

\* Transaction boundaries

\* Use-case sequencing



They should not become generic containers for business logic.



If behavior represents a business rule, it belongs in Domain.



\---



\## 46. DTOs and Application Results



Application owns internal use-case contracts and result models where appropriate.



These models represent application-level data rather than HTTP wire contracts.



The API may translate them into API-specific request and response DTOs.



Therefore:



```text

API DTO

&#x20;  ≠

Application DTO

&#x20;  ≠

Domain Entity

```



This prevents HTTP representation from leaking into application or domain design.



\---



\## 47. Application Does Not Own Persistence Technology



The Application layer defines what persistence capability is required.



For example:



```csharp

IOrderRepository

```



expresses the required repository capability.



Infrastructure decides that the implementation uses:



\* EF Core

\* `DbContext`

\* SQL Server



Likewise:



```csharp

ICustomerProfileQuery

```



expresses a read capability.



ReadModel decides that it uses:



\* ADO.NET

\* `SqlConnection`

\* `SqlCommand`

\* `SqlDataReader`

\* Stored procedures



This is Dependency Inversion in practice.



\---



\## 48. Testing Strategy



Application use cases should primarily be testable without requiring infrastructure implementations.



The architecture allows:



```text

Command Handler

&#x20;     |

&#x20;     v

Mock / fake Application abstractions

&#x20;     |

&#x20;     v

Domain behavior

```



For example, a handler can test orchestration using test doubles for:



\* Repositories

\* Identity services

\* Notification services

\* Scheduling services

\* External capabilities



The specialized ReadModel implementation is tested separately against real SQL Server because its correctness depends on actual database behavior.



Domain logic remains independently testable.



\---



\## 49. Security Boundary



Application is responsible for application-level authorization requirements.



Sensitive operations must not rely solely on the API layer.



For example:



```text

API

&#x20;|

&#x20;└── Authentication / boundary authorization



Application

&#x20;|

&#x20;└── Permission requirement



Domain

&#x20;|

&#x20;└── Business invariants



Infrastructure

&#x20;|

&#x20;└── ASP.NET Core authorization implementation

```



This provides defense in depth while preserving layer responsibilities.



\---



\## 50. Reliability and Failure Handling



The Application layer coordinates failure-aware workflows but does not own infrastructure retry mechanisms.



\### Database failure



```text

Application

&#x20;    |

&#x20;    v

Unit of Work

&#x20;    |

&#x20;    v

Infrastructure

&#x20;    |

&#x20;    v

SQL failure

```



Infrastructure handles technical persistence behavior and translates appropriate failures.



\### Outbox failure



```text

Transaction succeeds

&#x20;       |

&#x20;       v

Outbox persists

&#x20;       |

&#x20;       v

Background processing

&#x20;       |

&#x20;       v

Temporary delivery failure

&#x20;       |

&#x20;       v

Hangfire retry

```



\### External payment failure



```text

Application payment workflow

&#x20;         |

&#x20;         v

Infrastructure Paymob adapter

&#x20;         |

&#x20;         v

External failure

&#x20;         |

&#x20;         v

Application-facing failure

```



The Application layer should not implement duplicate infrastructure retry systems when the underlying infrastructure already owns them.



\---



\## 51. Application-Level Consistency



The Application layer defines the consistency requirements of each use case.



For a single aggregate operation:



```text

Command

&#x20;  |

&#x20;  v

Aggregate

&#x20;  |

&#x20;  v

Unit of Work

&#x20;  |

&#x20;  v

Commit

```



For operations requiring multiple persistence changes within the same module transaction:



```text

Application workflow

&#x20;       |

&#x20;       v

Multiple changes

&#x20;       |

&#x20;       v

Single Unit of Work

&#x20;       |

&#x20;       v

Single transaction

&#x20;       |

&#x20;       v

Commit

```



For asynchronous cross-module work:



```text

Transaction

&#x20;    |

&#x20;    v

Outbox

&#x20;    |

&#x20;    v

Post-commit processing

```



This allows the system to maintain strong transactional consistency where required without introducing distributed transactions.



\---



\## 52. Modular Monolith Alignment



The Application layer is designed for a modular monolith.



Modules remain logically separated while running inside one process.



The architecture can therefore use:



\* One process

\* One application

\* One SQL Server database

\* One shared Unit of Work where atomicity is required

\* Multiple domain and application modules



Application boundaries prevent accidental coupling even though modules share the same runtime.



The architecture does not require:



\* Microservices

\* Service-to-service HTTP

\* Kafka

\* RabbitMQ

\* Distributed transactions



unless the system later becomes genuinely distributed.



\---



\## 53. Shared Application Components



Shared application code should remain deliberately small.



Only genuinely cross-module concerns should be placed in common or shared areas.



Examples may include:



\* Common application result abstractions

\* Shared pipeline behaviors

\* Common authorization abstractions

\* Common application interfaces



Business-specific functionality should remain inside its owning module.



The rule is:



\*\*Shared code must represent a genuine shared capability, not merely convenient code reuse.\*\*



\---



\## 54. Application vs Domain vs Infrastructure



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



\---



\## 55. Final Architectural Rules



The Application layer follows these rules:



1\. \*\*Application owns use cases and orchestration.\*\*

2\. \*\*Domain owns business rules and invariants.\*\*

3\. \*\*Application depends inward on Domain.\*\*

4\. \*\*Domain never depends on Application.\*\*

5\. \*\*Application does not depend on Infrastructure implementations.\*\*

6\. \*\*Infrastructure implements Application-facing abstractions.\*\*

7\. \*\*Commands represent state-changing use cases.\*\*

8\. \*\*Queries represent read use cases.\*\*

9\. \*\*CQRS is applied selectively rather than mechanically.\*\*

10\. \*\*Specialized reads may be implemented by ReadModel.\*\*

11\. \*\*Application owns ReadModel query contracts.\*\*

12\. \*\*ReadModel owns ADO.NET and stored-procedure implementation details.\*\*

13\. \*\*Application does not directly use EF Core.\*\*

14\. \*\*Application does not directly use Hangfire.\*\*

15\. \*\*Application does not directly use Redis, SignalR, Firebase, Twilio, SMTP, or Paymob.\*\*

16\. \*\*External technologies are accessed through application-facing capabilities.\*\*

17\. \*\*Domain aggregates remain authoritative for business state transitions.\*\*

18\. \*\*Application handlers orchestrate rather than reproduce domain rules.\*\*

19\. \*\*Unit of Work defines transactional application boundaries through an abstraction.\*\*

20\. \*\*Outbox provides durable asynchronous integration.\*\*

21\. \*\*Background execution is scheduler-agnostic at the Application boundary.\*\*

22\. \*\*Identity remains an Application capability with ASP.NET Identity implemented by Infrastructure.\*\*

23\. \*\*Authorization requirements belong at the Application/use-case boundary, while ASP.NET Core authorization implementation belongs to Infrastructure.\*\*

24\. \*\*Notifications express application intent; Infrastructure performs delivery.\*\*

25\. \*\*Application DTOs remain separate from API wire contracts.\*\*

26\. \*\*Shared Application code must remain genuinely shared.\*\*

27\. \*\*The Application layer must remain independent of HTTP and infrastructure frameworks wherever practical.\*\*

28\. \*\*The modular-monolith boundary is preserved through explicit application contracts and module ownership.\*\*



\---



\## 56. Final Responsibility Matrix



| Layer              | Primary Responsibility                                                                         |

| ------------------ | ---------------------------------------------------------------------------------------------- |

| \*\*API\*\*            | HTTP boundary, request/response translation, composition                                       |

| \*\*Application\*\*    | Use cases, orchestration, CQRS, contracts, application workflows                               |

| \*\*Domain\*\*         | Business rules, aggregates, invariants, domain events                                          |

| \*\*Infrastructure\*\* | EF Core, SQL persistence, Identity, external services, messaging, jobs, caching, observability |

| \*\*ReadModel\*\*      | Specialized high-performance read implementation using ADO.NET                                 |

| \*\*Database\*\*       | Durable application data and transactional persistence                                         |



The central relationship is:



```text

&#x20;                   APPLICATION

&#x20;                        |

&#x20;         ┌──────────────┼──────────────┐

&#x20;         v              v              v

&#x20;      DOMAIN      INFRASTRUCTURE    READMODEL

&#x20;      authority    implementation   specialized reads

```



More precisely, the dependency direction is:



```text

API ───────────────► Application ─────────────► Domain

&#x20;│                         ▲

&#x20;│                         │

&#x20;├────────────► Infrastructure

&#x20;│

&#x20;└────────────► ReadModel ─────────────► Application

```



\---



\## 57. Final Architecture Summary



`E-Commerce.Application` is the \*\*use-case and orchestration layer\*\* of the modular monolith.



It provides the system's application capabilities through commands, queries, handlers, application services, contracts, authorization requirements, scheduling abstractions, notification intent, and specialized read contracts.



It coordinates Domain behavior without replacing it.



It defines persistence and external-service capabilities without implementing their technologies.



It provides the CQRS boundary without forcing every read into a separate read model.



It enables specialized high-performance reads through the `E-Commerce.ReadModel` subsystem while keeping ADO.NET and SQL implementation details outside Application.



It enables durable asynchronous workflows through the transactional outbox and scheduler-agnostic background execution abstractions.



It keeps ASP.NET Core Identity, EF Core, Hangfire, Redis, SignalR, Firebase, Paymob, SMTP, Twilio, and other infrastructure technologies outside the use-case layer.



The resulting architecture can therefore be summarized as:



\*\*API translates HTTP. Application orchestrates use cases. Domain enforces business rules. Infrastructure implements technical capabilities. ReadModel optimizes specialized reads.\*\*



This preserves Clean Architecture, DDD, CQRS, Dependency Inversion, transactional consistency, modular boundaries, and infrastructure independence while remaining appropriate for a production-oriented modular monolith.



