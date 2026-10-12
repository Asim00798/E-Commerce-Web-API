\# E-Commerce.Domain — Architecture Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Layer:\*\* Domain

\*\*Version:\*\* 1.0 – Final



\---



\## 1. Overview



`E-Commerce.Domain` is the innermost business layer of the E-Commerce modular monolith.



It contains the system's business model, including:



\* Bounded-context domain models

\* Aggregate roots

\* Entities

\* Value objects

\* Domain events

\* Domain exceptions

\* Domain policies

\* Domain services where required

\* Repository abstractions

\* Unit of Work abstractions

\* Shared domain primitives



The Domain layer represents \*\*business truth\*\*.



It does not represent:



\* HTTP

\* Databases

\* EF Core

\* ASP.NET Core

\* ASP.NET Core Identity

\* Redis

\* Hangfire

\* SignalR

\* Paymob

\* Firebase

\* SMTP

\* Twilio

\* ADO.NET

\* Other infrastructure technologies



\### 1.1 Core Principle



\*\*The Domain layer models what the business is and what the business allows. It does not model how the system technically persists, transports, schedules, or delivers that behavior.\*\*



The Domain therefore remains \*\*framework agnostic\*\* and follows Domain-Driven Design principles.



Its code should be understandable independently of ASP.NET Core, EF Core, SQL Server, or any other infrastructure technology.



\### 1.2 Domain as the Business Authority



The Domain is the authoritative location for business invariants and business state transitions.



For example:



```text

Payment

&#x20;  |

&#x20;  v

Can this payment transition to Paid?

```



This is a Domain question.



Whereas:



```text

How is Payment persisted?

```



is an Infrastructure question.



Similarly:



```text

Order

&#x20;  |

&#x20;  v

Can this order be cancelled?

```



belongs to Domain.



But:



```text

How is the Order stored in SQL Server?

```



belongs to Infrastructure.



This separation is fundamental to the architecture.



\---



\## 2. Domain-Driven Design Philosophy



The Domain layer uses DDD concepts to model business behavior explicitly.



The primary building blocks are:



\* Bounded contexts

\* Aggregate roots

\* Entities

\* Value objects

\* Domain events

\* Domain exceptions

\* Domain policies

\* Repository abstractions

\* Shared Kernel primitives



The Domain model is organized around business concepts rather than technical mechanisms.



\### 2.1 Bounded Contexts



The Domain is divided into business-oriented bounded contexts.



The current structure includes:



```text

E-Commerce.Domain/

└── BoundedContexts/

&#x20;   ├── Core/

&#x20;   │   ├── Catalog/

&#x20;   │   ├── CustomerEngagement/

&#x20;   │   ├── Finance/

&#x20;   │   ├── Ordering/

&#x20;   │   └── Shipping/

&#x20;   │

&#x20;   └── UserManagement/

&#x20;       ├── Onboarding/

&#x20;       └── People/

```



These boundaries represent different areas of business responsibility.



They are not merely folder categories.



A bounded context owns the terminology, rules, aggregates, and domain behavior relevant to its business area.



\### 2.2 Domain Does Not Model Infrastructure Contexts



Not every Application or Infrastructure module represents a Domain bounded context.



For example:



\* Authentication infrastructure

\* ASP.NET Core Identity

\* Scheduling

\* Notification delivery

\* Redis

\* File storage

\* ReadModel



These are technical or application concerns rather than Domain bounded contexts.



The Domain therefore does not contain an `IdentityUser`, Hangfire job, Redis cache, HTTP client, or similar framework model.



\---



\## 3. Domain Layer Responsibilities



\### 3.1 In Scope



The Domain layer is responsible for:



\* Business entities

\* Aggregate roots

\* Value objects

\* Business invariants

\* State transitions

\* Domain events

\* Domain exceptions

\* Domain policies

\* Domain services where genuinely required

\* Repository abstractions

\* Unit of Work abstractions

\* Shared domain abstractions

\* Domain-level time abstraction

\* Domain validation related to business rules



\### 3.2 Out of Scope



The Domain layer does not:



\* Access SQL Server

\* Use EF Core

\* Use `DbContext`

\* Use `DbSet`

\* Execute SQL

\* Use ADO.NET

\* Use Dapper

\* Use ASP.NET Core

\* Use ASP.NET Core Identity

\* Use `IdentityUser`

\* Use Redis

\* Use Hangfire

\* Use SignalR

\* Call Paymob

\* Call Twilio

\* Call Firebase

\* Send email

\* Expose HTTP endpoints

\* Define API DTOs

\* Perform database migrations

\* Perform external I/O

\* Schedule background jobs

\* Implement infrastructure providers



\---



\## 4. Framework Independence



Framework independence is a deliberate architectural property of the Domain layer.



The Domain should not require a framework to understand or execute its business rules.



Conceptually:



```text

E-Commerce.Domain

&#x20;      |

&#x20;      ├── C#

&#x20;      ├── Domain concepts

&#x20;      ├── Business rules

&#x20;      ├── Domain abstractions

&#x20;      └── Domain events

```



Rather than:



```text

E-Commerce.Domain

&#x20;      |

&#x20;      ├── EF Core

&#x20;      ├── ASP.NET Core

&#x20;      ├── Identity

&#x20;      ├── Redis

&#x20;      └── Hangfire

```



The Domain can therefore evolve independently of the technical infrastructure.



\### 4.1 Framework-Agnostic Does Not Mean Dependency-Free



Framework agnostic does not mean that the Domain contains no abstractions.



The Domain may define interfaces such as:



```csharp

IRepository<T>

IUnitOfWork

IClock

IDomainEvent

IAggregateRoot

```



These are architectural or domain abstractions, not framework dependencies.



Infrastructure later provides their concrete implementations.



\---



\## 5. Aggregate Roots



Aggregate roots are the primary consistency boundaries within the Domain.



The current system contains aggregate roots such as:



```text

Catalog

&#x20;├── Brand

&#x20;├── Category

&#x20;└── Product



CustomerEngagement

&#x20;├── Rating

&#x20;└── Wishlist



Finance

&#x20;├── Payment

&#x20;└── Refund



Ordering

&#x20;├── Cart

&#x20;└── Order



Shipping

&#x20;└── Shipment



Onboarding

&#x20;└── Registration



People

&#x20;└── Person

```



Each aggregate root controls the business state and behavior within its aggregate boundary.



\### 5.1 Aggregate Responsibilities



An aggregate root is responsible for:



\* Protecting its invariants

\* Controlling state transitions

\* Coordinating changes to its owned entities

\* Exposing valid domain operations

\* Raising domain events when meaningful business events occur



External code should not arbitrarily manipulate internal aggregate state.



Instead, operations should be expressed through meaningful domain behavior.



For example:



```csharp

Order.Cancel();

Payment.Capture();

Refund.Complete();

Shipment.Deliver();

Registration.VerifyEmail();

Person.UpdateInfo();

```



This is preferable to unrestricted public state mutation.



\### 5.2 Aggregate Consistency Boundary



The aggregate is the primary consistency boundary for business invariants.



Conceptually:



```text

Aggregate Root

&#x20;     |

&#x20;     ├── State

&#x20;     ├── Entities

&#x20;     ├── Value Objects

&#x20;     └── Invariants

```



A business invariant that must be enforced atomically within an aggregate belongs inside that aggregate.



Cross-aggregate workflows are coordinated by Application rather than by creating one giant aggregate.



\---



\## 6. Catalog Domain



```text

Catalog/

└── AggregateRoots/

&#x20;   ├── Brand/

&#x20;   ├── Category/

&#x20;   └── Product/

```



The Catalog context models product-related business concepts.



\### 6.1 Brand



The Brand aggregate contains:



\* Brand behavior

\* Update behavior

\* Brand-specific exceptions

\* `BrandLogo` value object



The aggregate controls valid brand state transitions.



\### 6.2 Category



The Category aggregate contains:



\* Category behavior

\* Category state

\* Category images

\* Category-specific exceptions



`CategoryImage` is an entity owned by the aggregate.



\### 6.3 Product



The Product aggregate contains:



\* Product behavior

\* Product state

\* Product images

\* Product variants

\* Product status

\* Product description



The aggregate controls product-specific business rules.



\---



\## 7. Customer Engagement Domain



```text

CustomerEngagement/

└── AggregateRoots/

&#x20;   ├── Rating/

&#x20;   └── Wishlist/

```



Customer Engagement models customer interaction with the catalog.



\### 7.1 Rating



The Rating aggregate contains:



\* Rating behavior

\* Validation

\* `StarRating`

\* `RatingSummary`



The aggregate ensures that rating-related invariants remain valid.



\### 7.2 Wishlist



The Wishlist aggregate contains:



\* Wishlist behavior

\* Wishlist validation

\* `WishlistItem` entities



The aggregate controls valid wishlist operations.



\---



\## 8. Finance Domain



```text

Finance/

├── AggregateRoots/

│   ├── Payment/

│   └── Refund/

├── Enums/

├── Repositories/

└── ValueObjects/

```



Finance is a true business bounded context.



It contains meaningful financial state, invariants, state transitions, and domain events.



\### 8.1 Payment



The Payment aggregate contains:



\* Payment behavior

\* Payment state

\* Validation

\* Refund-related domain behavior

\* `PaymentTransaction`

\* Payment domain events

\* Payment-specific exceptions



Payment state transitions are controlled by the aggregate.



For example:



```text

PendingPayment

&#x20;     |

&#x20;     v

&#x20;    Paid

&#x20;     |

&#x20;     v

&#x20;  Refunded

```



The actual allowed transitions are governed by Domain rules.



\### 8.2 Refund



The Refund aggregate contains:



\* Refund behavior

\* Refund state

\* Refund domain events

\* Refund exceptions



The Domain owns the meaning of refund state transitions.



The Paymob provider does not.



\### 8.3 Payment Provider Separation



The Domain does not know about Paymob.



The separation is:



```text

Domain

&#x20; Payment

&#x20;    |

&#x20;    v

Application

&#x20; Payment use case

&#x20;    |

&#x20;    v

Infrastructure

&#x20; PaymobPaymentGateway

&#x20;    |

&#x20;    v

Paymob

```



This allows the provider to change without changing the Domain model.



\---



\## 9. Ordering Domain



```text

Ordering/

├── AggregateRoots/

│   ├── Cart/

│   └── Order/

```



Ordering models the transactional purchasing lifecycle.



\### 9.1 Cart



The Cart aggregate contains:



\* Cart behavior

\* Cart state

\* Cart validation

\* `CartItem`

\* Cart exceptions



The aggregate controls operations such as adding or removing items and other cart-specific business rules.



\### 9.2 Order



The Order aggregate contains:



\* Order behavior

\* Order state

\* Order validation

\* Order items

\* Order status

\* Order domain events

\* Order exceptions



The aggregate controls order state transitions.



Examples include:



```text

PendingPayment

&#x20;     |

&#x20;     v

&#x20;    Paid

&#x20;     |

&#x20;     v

&#x20;  Shipped

&#x20;     |

&#x20;     v

&#x20;  Delivered

```



Cancellation and refund transitions are governed by the applicable business rules.



\### 9.3 Ordering Events



The Order aggregate may raise events such as:



\* `OrderPlacedDomainEvent`

\* `OrderPaidDomainEvent`

\* `OrderCancelledDomainEvent`



These events describe domain facts.



Infrastructure later provides the dispatching mechanism.



\---



\## 10. Shipping Domain



```text

Shipping/

├── AggregateRoots/

│   └── Shipment/

├── Enums/

├── Policies/

├── Repositories/

└── ValueObjects/

```



Shipping is a business bounded context.



It models shipment lifecycle, delivery attempts, shipping addresses, distance, and shipping-fee policies.



\### 10.1 Shipment



The Shipment aggregate contains:



\* Shipment behavior

\* Delivery behavior

\* Shipment state

\* Validation

\* `DeliveryAttempt`

\* Shipment events

\* Shipment exceptions



The aggregate controls transitions such as:



```text

Created

&#x20;  |

&#x20;  v

Shipped

&#x20;  |

&#x20;  v

Delivered

```



And:



```text

Shipped

&#x20;  |

&#x20;  v

Returned

```



These transitions follow the allowed domain rules.



\### 10.2 Delivery Attempts



`DeliveryAttempt` is an entity owned by the Shipment aggregate.



It represents delivery history and outcomes.



The entity should not be independently manipulated outside the aggregate boundary.



\### 10.3 Shipping Value Objects



Examples include:



\* `DeliveryAddressSnapshot`

\* `ShippingDistance`

\* `ShippingFeeResult`



These represent domain concepts whose identity is determined by their values rather than independent database identity.



\### 10.4 Shipping Policy



`LocalShippingFeePolicy` represents a pure business policy.



It belongs in Domain when the calculation is based entirely on business concepts and does not require external infrastructure.



For example:



```text

Shipping distance

&#x20;       +

Business pricing rules

&#x20;       |

&#x20;       v

&#x20;  Shipping fee

```



An external map or geolocation provider, however, belongs in Infrastructure.



\---



\## 11. User Management Domain



```text

UserManagement/

├── Onboarding/

└── People/

```



User Management contains business concepts related to onboarding and people.



It does \*\*not\*\* contain ASP.NET Core Identity.



\### 11.1 Onboarding



Onboarding models the registration workflow as a Domain concept.



```text

Onboarding/

└── AggregateRoots/

&#x20;   └── Registration/

```



The Registration aggregate contains:



\* Registration behavior

\* Email verification behavior

\* Phone verification behavior

\* Lifecycle behavior

\* Registration constants

\* Domain events

\* Exceptions

\* Credential-related value objects

\* Username

\* Verification channel



\### 11.2 Registration Aggregate



The Registration aggregate models the business lifecycle of registration.



Conceptually:



```text

Registration Created

&#x20;       |

&#x20;       v

Email Verification

&#x20;       |

&#x20;       v

Phone Verification

&#x20;       |

&#x20;       v

Fully Verified

&#x20;       |

&#x20;       v

Eligible for Identity Creation

```



The aggregate determines whether the registration workflow is in a valid state.



\### 11.3 Registration vs Identity



The Domain deliberately does not contain ASP.NET Core Identity.



The distinction is:



```text

Registration

&#x20;    |

&#x20;    v

Domain onboarding workflow

```



Versus:



```text

IdentityUser

&#x20;    |

&#x20;    v

Infrastructure authentication identity

```



Application orchestrates the transition between these systems.



For example:



```text

Registration fully verified

&#x20;          |

&#x20;          v

&#x20;     Application

&#x20;          |

&#x20;          v

&#x20;     IdentityService

&#x20;          |

&#x20;          v

ASP.NET Core Identity

```



The Domain remains independent of the framework identity system.



\---



\## 12. People Domain



```text

People/

└── AggregateRoots/

&#x20;   └── Person/

```



The Person aggregate represents business or customer personal information.



It is deliberately separate from authentication identity.



The Person aggregate contains:



\* Personal information

\* Update behavior

\* Validation

\* Full name

\* Addresses

\* Personal image

\* Gender- and address-related concepts



\### 12.1 Person vs Identity User



The architecture intentionally distinguishes:



```text

Identity User

&#x20;     |

&#x20;     ├── Authentication

&#x20;     ├── Authorization

&#x20;     ├── Login

&#x20;     └── Credentials

```



From:



```text

Person

&#x20;  |

&#x20;  ├── Business/customer information

&#x20;  ├── Name

&#x20;  ├── Addresses

&#x20;  └── Personal information

```



An authenticated identity does not automatically imply that a complete Person aggregate exists.



Application onboarding coordinates creation and linking of the Person when required by the business workflow.



\---



\## 13. Value Objects



Value objects represent domain concepts whose identity is defined by their values.



The SharedKernel contains reusable value objects such as:



\* `Email`

\* `Money`

\* `PhoneNumber`

\* `Quantity`

\* `Weight`

\* `Dimension`



Context-specific value objects also exist within their bounded contexts.



Examples include:



\* `BrandLogo`

\* `ProductDescription`

\* `StarRating`

\* `RatingSummary`

\* `PaymentMethod`

\* `DeliveryAddressSnapshot`

\* `ShippingDistance`

\* `ShippingFeeResult`

\* `CredentialHash`

\* `Username`

\* `VerificationChannel`



Value objects should:



\* Be immutable where appropriate

\* Enforce their own validity

\* Avoid framework dependencies

\* Express domain concepts explicitly



\### 13.1 Value Objects and Validation



Validation inside a value object protects the concept itself.



For example:



```text

Email

&#x20; |

&#x20; v

Valid email value

```



```text

Money

&#x20; |

&#x20; v

Valid monetary value

```



```text

Quantity

&#x20; |

&#x20; v

Valid quantity

```



Aggregate-level validation then protects relationships and invariants involving multiple domain concepts.



\---



\## 14. Entities



Entities have identity within an aggregate.



Examples include:



\* `ProductImage`

\* `ProductVariant`

\* `CategoryImage`

\* `PaymentTransaction`

\* `CartItem`

\* `OrderItem`

\* `DeliveryAttempt`

\* `WishlistItem`



Entities are controlled by their aggregate roots.



The aggregate remains responsible for maintaining consistency within its boundary.



\---



\## 15. Domain Events



Domain events represent facts that occurred within the Domain.



SharedKernel contains:



\* `DomainEvent.cs`

\* `IDomainEvent.cs`

\* `IDomainEventDispatcher.cs`

\* `IDomainEventHandler.cs`



Context-specific events include examples such as:



\* `OrderPlacedDomainEvent`

\* `OrderPaidDomainEvent`

\* `PaymentCapturedDomainEvent`

\* `PaymentFailedDomainEvent`

\* `RefundCompletedDomainEvent`

\* `ShipmentCreatedDomainEvent`

\* `ShipmentShippedDomainEvent`

\* `ShipmentDeliveredDomainEvent`

\* `RegistrationFullyVerifiedDomainEvent`



\### 15.1 Domain Event Responsibility



A domain event communicates:



\*\*Something meaningful happened in the Domain.\*\*



It does not define how that fact is transported.



For example, `OrderPaidDomainEvent` does not know about:



\* Hangfire

\* Outbox

\* Kafka

\* RabbitMQ

\* SignalR

\* HTTP

\* Email



Infrastructure determines how domain events are dispatched.



\### 15.2 Domain Events vs Integration Events



The architecture distinguishes domain events from integration events.



```text

Domain Event

&#x20;    |

&#x20;    v

Internal business fact

```



```text

Integration Event

&#x20;    |

&#x20;    v

Cross-module/application communication contract

```



A Domain event may lead to an integration event through Application and Infrastructure mechanisms.



The Domain itself does not need to know about the transport mechanism.



\---



\## 16. Domain Exceptions



Domain exceptions represent invalid business operations.



SharedKernel contains:



\* `BusinessRuleViolationException`

\* `DomainException`



Bounded contexts also contain context-specific exceptions, such as:



\* `OrderException`

\* `PaymentException`

\* `RefundException`

\* `ShipmentException`

\* `RegistrationException`

\* `ProductException`



Domain exceptions should communicate business failures rather than technical infrastructure failures.



Examples include:



\* Invalid order transition

\* Invalid payment transition

\* Invalid registration state

\* Invalid rating

\* Invalid quantity



These are Domain concerns.



\---



\## 17. Domain Policies



Policies encapsulate business rules that do not naturally belong to one aggregate.



Example:



```csharp

LocalShippingFeePolicy

```



A Domain policy should be:



\* Business-oriented

\* Deterministic where applicable

\* Framework-independent

\* Free from external I/O unless explicitly modeled through a domain abstraction



A policy should not become a generic utility class.



\### 17.1 Domain Policy vs Infrastructure Service



The distinction is:



```text

Business calculation

&#x20;       |

&#x20;       v

&#x20;  Domain Policy

```



Versus:



```text

External API / filesystem / Redis / database

&#x20;                 |

&#x20;                 v

&#x20;         Infrastructure Service

```



For example:



\* `LocalShippingFeePolicy` belongs in Domain.

\* `MapLocationService` belongs in Infrastructure.



\---



\## 18. Repository Abstractions



Repositories are domain/application persistence ports.



The Domain contains repository interfaces such as:



\* `IBrandRepository`

\* `ICategoryRepository`

\* `IProductRepository`

\* `IRatingRepository`

\* `IWishlistRepository`

\* `IPaymentRepository`

\* `IRefundRepository`

\* `ICartRepository`

\* `IOrderRepository`

\* `IShipmentRepository`

\* `IRegistrationRepository`

\* `IPersonRepository`



These interfaces describe what the business or application side requires from persistence.



They do not describe how SQL Server performs the operation.



\### 18.1 Repository Implementation



Infrastructure implements these abstractions.



```text

Domain

&#x20; |

&#x20; v

IOrderRepository

&#x20; ^

&#x20; |

Infrastructure

&#x20; |

&#x20; v

OrderRepository

&#x20; |

&#x20; v

EF Core

&#x20; |

&#x20; v

SQL Server

```



This is Dependency Inversion.



The Domain owns the abstraction.



Infrastructure owns the implementation.



\### 18.2 Repository Scope



Repositories should represent aggregate-oriented persistence access.



They should not become generic business-service containers.



The repository is responsible for persistence access.



The aggregate remains responsible for business behavior.



\---



\## 19. Unit of Work Abstraction



The SharedKernel contains:



```csharp

IUnitOfWork

```



This represents the application's transactional persistence boundary.



Infrastructure implements it through EF Core.



The Domain therefore knows that a transactional persistence boundary exists without knowing about:



\* EF Core

\* SQL Server

\* `DbContext`

\* Database transactions



\---



\## 20. SharedKernel



```text

SharedKernel/

├── Abstractions/

├── Events/

├── Exceptions/

├── PersistenceAbstractions/

├── Services/

└── ValueObjects/

```



The SharedKernel contains genuinely reusable domain concepts.



It must remain intentionally small.



\### 20.1 Shared Abstractions



Examples:



\* `BaseEntity`

\* `IAggregateRoot`

\* `IValidatableEntity`



These provide common domain-level behavior and contracts.



They must not contain infrastructure-specific implementation.



\### 20.2 Shared Domain Events



The SharedKernel defines the common event model:



\* `DomainEvent`

\* `IDomainEvent`

\* `IDomainEventHandler`

\* `IDomainEventDispatcher`



The concrete business events remain owned by their bounded contexts.



\### 20.3 Shared Persistence Abstractions



The SharedKernel contains:



\* `IRepository<T>`

\* `IUnitOfWork`



These are persistence ports.



Their implementations remain outside Domain.



\### 20.4 Shared Clock Abstraction



The Domain contains:



```csharp

IClock

```



This allows business logic to obtain time without depending directly on:



```csharp

DateTime.UtcNow

```



or framework-specific time services.



Infrastructure supplies the concrete clock implementation.



\---



\## 21. Domain Validation



Domain validation exists at multiple levels.



\*\*Value-object validation\*\*



Protects individual domain concepts.



\*\*Entity validation\*\*



Protects entity-specific rules.



\*\*Aggregate validation\*\*



Protects aggregate invariants.



\*\*Application validation\*\*



Protects request and use-case constraints.



The layers should not be collapsed into one generic validation mechanism.



For example:



```text

HTTP request validation

&#x20;         |

&#x20;         v

&#x20;    Application

```



And:



```text

Business invariant

&#x20;         |

&#x20;         v

&#x20;       Domain

```



\---



\## 22. State Machines and Domain State



Several aggregates explicitly organize state behavior.



Examples include:



\* `Payment.State`

\* `Order.State`

\* `Shipment.State`

\* `Registration.Lifecycle`

\* `Cart.State`

\* `Product.State`



These files represent business state management.



State transitions must remain protected by Domain rules.



For example:



```text

Current State

&#x20;     +

Requested Operation

&#x20;     |

&#x20;     v

Domain validation

&#x20;     |

&#x20;     v

New State

```



An infrastructure service must never bypass these transitions simply because it can technically update the database.



\---



\## 23. Partial Domain Classes



The Domain uses partial classes to separate related behavior.



Examples:



```text

Order.cs

Order.State.cs

Order.Validation.cs

```



Or:



```text

Payment.cs

Payment.State.cs

Payment.Validation.cs

Payment.Refund.cs

```



This is an organizational technique.



It does not divide the aggregate into separate domain objects.



The resulting compiled type remains one aggregate root.



Partial classes are appropriate when they improve readability without hiding important domain relationships.



\### 23.1 Domain Helpers



Files such as:



```csharp

Product.Helpers.cs

```



may contain narrowly scoped helper behavior associated with the aggregate.



They should not become generic utility containers.



If a helper represents a reusable business concept, it should instead be modeled explicitly as:



\* A value object

\* A policy

\* A domain service

\* Another appropriate domain abstraction



\---



\## 24. Domain Services



Not every business rule belongs inside an aggregate.



A domain service is appropriate when:



\* The operation is genuinely domain logic.

\* It does not naturally belong to a single aggregate.

\* It represents a meaningful domain concept.

\* Moving it into an aggregate would create an artificial responsibility.



Domain services should remain framework-independent.



They should not become wrappers around:



\* Repositories

\* HTTP clients

\* Redis

\* EF Core

\* Hangfire

\* External APIs



Those are Infrastructure or Application responsibilities.



\---



\## 25. Domain and Application



The relationship is:



```text

Domain

&#x20; ^

&#x20; |

Application

```



Application depends on Domain.



Domain does not depend on Application.



Application orchestrates domain behavior.



For example:



```text

Application Command

&#x20;       |

&#x20;       v

Load aggregate

&#x20;       |

&#x20;       v

Invoke domain behavior

&#x20;       |

&#x20;       v

Domain state changes

&#x20;       |

&#x20;       v

Domain event

&#x20;       |

&#x20;       v

Application continues workflow

```



The Domain does not know which Application command caused the operation.



\---



\## 26. Domain and Infrastructure



The dependency relationship is:



```text

Domain

&#x20; ^

&#x20; |

Infrastructure

```



Infrastructure depends on Domain to implement its technical requirements.



The reverse is forbidden.



For example:



```text

Domain

&#x20; |

&#x20; v

IOrderRepository

&#x20; ^

&#x20; |

Infrastructure

&#x20; |

&#x20; v

OrderRepository

```



The Domain remains unaware of the implementation.



\---



\## 27. Domain and API



The Domain has no dependency on API.



The API communicates with Application.



```text

API

&#x20;|

&#x20;v

Application

&#x20;|

&#x20;v

Domain

```



The Domain does not know about:



\* Controllers

\* HTTP status codes

\* JSON

\* API DTOs

\* Route templates

\* Authentication attributes



\---



\## 28. Domain and ReadModel



The Domain does not depend on ReadModel.



ReadModel also does not need to reference Domain directly in the current architecture.



The Application layer owns the query contract.



```text

Application Query

&#x20;      |

&#x20;      v

ReadModel

&#x20;      |

&#x20;      v

ADO.NET

&#x20;      |

&#x20;      v

SQL Server

```



This prevents specialized read infrastructure from becoming coupled to aggregate implementation details.



\---



\## 29. Business Authority vs Technical Representation



A database record is not automatically the business authority merely because it exists in SQL Server.



The authority remains the Domain/Application write model for business decisions.



For example:



```text

ReadModel result

&#x20;      |

&#x20;      v

Information retrieval

```



Must not replace:



```text

Aggregate

&#x20;   |

&#x20;   v

Business invariant

&#x20;   |

&#x20;   v

Transactional decision

```



This distinction protects the Domain from becoming dependent on read-side representations.



\---



\## 30. Domain Event Flow



A typical event flow is:



```text

Aggregate

&#x20;   |

&#x20;   v

Business operation

&#x20;   |

&#x20;   v

State change

&#x20;   |

&#x20;   v

Domain Event

&#x20;   |

&#x20;   v

Unit of Work

&#x20;   |

&#x20;   v

Persistence

&#x20;   |

&#x20;   v

Application / Infrastructure dispatch

&#x20;   |

&#x20;   v

Integration Event / side effect

```



The Domain stops at the domain fact.



Infrastructure provides the technical delivery mechanism.



\---



\## 31. Domain Integrity Rules



The following rules are architectural invariants.



\### Rule 1 — Domain Remains Framework Agnostic



No EF Core, ASP.NET Core, Identity, Redis, Hangfire, SignalR, provider SDK, or database-specific types.



\### Rule 2 — Business Invariants Belong in Domain



If a rule determines whether a business operation is valid, it should be represented in the Domain model.



\### Rule 3 — Aggregates Protect Their Invariants



External code should invoke aggregate behavior rather than directly manipulating protected business state.



\### Rule 4 — Domain Owns Business State Transitions



Infrastructure must never bypass aggregate rules by directly changing business state merely because the database allows it.



\### Rule 5 — Domain Events Represent Facts



Domain events describe meaningful business occurrences and remain independent of transport mechanisms.



\### Rule 6 — Domain Does Not Perform External I/O



No HTTP calls, SQL queries, Redis operations, file access, email, SMS, push notifications, or background scheduling.



\### Rule 7 — Domain Abstractions Are Ports, Not Implementations



Interfaces such as repositories and clocks express required capabilities without binding the Domain to a technology.



\### Rule 8 — SharedKernel Must Remain Small



A concept belongs in SharedKernel only when it is genuinely shared across multiple bounded contexts and has stable domain meaning.



SharedKernel must not become a dumping ground for convenience classes.



\### Rule 9 — Do Not Create Artificial Abstractions



DDD does not require an abstraction for every class.



Use entities, value objects, policies, domain services, and interfaces when they represent real domain concepts or architectural boundaries.



\### Rule 10 — Avoid Infrastructure Leakage



A Domain class should never need to know how its state is stored, transported, serialized, scheduled, or exposed through HTTP.



\---



\## 32. Complete Domain Responsibility Matrix



| Capability               | Domain Responsibility    |

| ------------------------ | ------------------------ |

| Business rules           | Yes                      |

| Business invariants      | Yes                      |

| Aggregate roots          | Yes                      |

| Entities                 | Yes                      |

| Value objects            | Yes                      |

| Domain events            | Yes                      |

| Domain exceptions        | Yes                      |

| Business policies        | Yes                      |

| Domain services          | Where genuinely required |

| Repository abstractions  | Yes                      |

| Unit of Work abstraction | Yes                      |

| Clock abstraction        | Yes                      |

| EF Core                  | No                       |

| `DbContext`              | No                       |

| SQL Server               | No                       |

| ASP.NET Core             | No                       |

| ASP.NET Core Identity    | No                       |

| Redis                    | No                       |

| Hangfire                 | No                       |

| SignalR                  | No                       |

| Paymob                   | No                       |

| Firebase                 | No                       |

| Twilio                   | No                       |

| ADO.NET                  | No                       |

| API controllers          | No                       |

| API DTOs                 | No                       |

| Database migrations      | No                       |

| Background jobs          | No                       |

| ReadModel implementation | No                       |



\---



\## 33. Current Domain Architecture



The current Domain can be summarized as:



```text

&#x20;                    E-Commerce.Domain

&#x20;                           |

&#x20;         ┌─────────────────┼─────────────────┐

&#x20;         |                 |                 |

&#x20;         v                 v                 v

&#x20;     Core Context     UserManagement    SharedKernel

&#x20;         |                 |                 |

&#x20;    ┌────┼────┐        ┌───┴────┐            |

&#x20;    |    |    |        |        |            |

&#x20;    v    v    v        v        v            v

&#x20; Catalog Finance ... Onboarding People  Domain primitives

&#x20;    |    |    |        |        |            |

&#x20;    └────┴────┴────────┴────────┴────────────┘

&#x20;                           |

&#x20;                           v

&#x20;                      Aggregates

&#x20;                      Entities

&#x20;                      Value Objects

&#x20;                      Events

&#x20;                      Policies

&#x20;                           |

&#x20;                           v

&#x20;                   Business Authority

```



The Domain is therefore the center of the system's business model while remaining independent of the technical mechanisms used to execute it.



\---



\## 34. Final Architectural Rules



1\. \*\*E-Commerce.Domain is the innermost business layer.\*\*

2\. \*\*The Domain is framework agnostic.\*\*

3\. \*\*The Domain follows DDD principles.\*\*

4\. \*\*Bounded contexts represent business boundaries rather than technical folders.\*\*

5\. \*\*Aggregate roots are the primary consistency boundaries for domain invariants.\*\*

6\. \*\*Entities belong to and are controlled by their aggregate roots.\*\*

7\. \*\*Value objects represent domain concepts whose identity is determined by their values.\*\*

8\. \*\*Domain events represent meaningful business facts and remain independent of transport technology.\*\*

9\. \*\*Domain exceptions represent business failures rather than infrastructure failures.\*\*

10\. \*\*Domain policies contain business rules that do not naturally belong to one aggregate.\*\*

11\. \*\*Domain services are introduced only when a genuine domain responsibility does not naturally belong elsewhere.\*\*

12\. \*\*Repository and Unit of Work interfaces represent persistence ports; Infrastructure provides their implementations.\*\*

13\. \*\*The Domain must not reference Infrastructure, Application, API, or ReadModel.\*\*

14\. \*\*The Domain must not reference EF Core, ASP.NET Core, Identity, Redis, Hangfire, SignalR, ADO.NET, or external provider SDKs.\*\*

15\. \*\*The Domain must not perform external I/O.\*\*

16\. \*\*Business state transitions must be protected by Domain behavior rather than arbitrary property mutation.\*\*

17\. \*\*Infrastructure must never bypass Domain invariants merely because the database technically permits the operation.\*\*

18\. \*\*The SharedKernel must contain only genuinely shared and stable domain concepts.\*\*

19\. \*\*Authentication identity and business Person identity remain separate concepts.\*\*

20\. \*\*ASP.NET Core Identity remains an Infrastructure concern; the Registration and Person models remain Domain concerns.\*\*

21\. \*\*Payment and refund business rules remain in Domain; Paymob integration remains in Infrastructure.\*\*

22\. \*\*Shipping business policies remain in Domain; external location and map providers remain in Infrastructure.\*\*

23\. \*\*Domain events do not know whether they will eventually be handled synchronously, asynchronously, through an Outbox, or through another transport mechanism.\*\*

24\. \*\*ReadModel is an information-retrieval subsystem and is never the authority for Domain invariants or transactional business decisions.\*\*

25\. \*\*The Domain model must remain understandable independently of the technical infrastructure used to persist or execute it.\*\*



\---



\## 35. Final Architecture Summary



`E-Commerce.Domain` represents the business heart of the E-Commerce modular monolith.



Its responsibility can be summarized as:



```text

&#x20;                      BUSINESS

&#x20;                         |

&#x20;                         v

&#x20;                 E-Commerce.Domain

&#x20;                         |

&#x20;       ┌─────────────────┼─────────────────┐

&#x20;       |                 |                 |

&#x20;       v                 v                 v

&#x20;   Aggregates       Value Objects     Domain Events

&#x20;       |                 |                 |

&#x20;       └─────────────────┼─────────────────┘

&#x20;                         |

&#x20;                         v

&#x20;                   Business Rules

&#x20;                         |

&#x20;                         v

&#x20;                  Business Authority

```



The surrounding layers then provide the mechanisms required to execute that model:



```text

&#x20;                        DOMAIN

&#x20;                           |

&#x20;                    Business Authority

&#x20;                           |

&#x20;                           v

&#x20;                      APPLICATION

&#x20;                           |

&#x20;                     Use Cases / Flow

&#x20;                           |

&#x20;              ┌────────────┴────────────┐

&#x20;              v                         v

&#x20;       INFRASTRUCTURE                READMODEL

&#x20;              |                         |

&#x20;       Technical execution         Specialized reads

&#x20;              |                         |

&#x20;       ┌──────┼──────┐                  |

&#x20;       v      v      v                  v

&#x20;      SQL    Redis  Hangfire          ADO.NET

&#x20;    Identity Providers                SQL Server

```



The architectural objective is:



\*\*Keep the business model independent, explicit, and authoritative while allowing persistence, frameworks, external providers, background execution, and specialized read infrastructure to evolve around it.\*\*



This preserves the central DDD principle of the system:



\*\*The Domain models the business. Infrastructure implements the technology. Application orchestrates the use cases. API exposes the system. ReadModel optimizes specialized information retrieval.\*\*



\*\*Document version:\*\* 1.0 – Final

\*\*Feature:\*\* Domain Architecture



