\# E-Commerce.Infrastructure — Architecture Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Version:\*\* 1.0 – Final



\---



\## 1. Overview



E-Commerce.Infrastructure is the outer-layer implementation project of the E-Commerce modular monolith.



It contains the concrete technical implementations required by the Application and Domain layers, including persistence, external integrations, authentication infrastructure, authorization infrastructure, caching, background execution, messaging, notifications, file storage, observability, and other framework-specific capabilities.



Infrastructure exists to keep technical concerns outside the Domain and Application layers while providing those inner layers with the implementations they require.



\### Core Principle



\*\*Application and Domain define what the system needs; Infrastructure defines how those capabilities are technically implemented.\*\*



Infrastructure is therefore not the owner of business behavior.



It is the implementation boundary between the application's architectural abstractions and external technologies such as:



\* SQL Server

\* Entity Framework Core

\* ASP.NET Core Identity

\* Redis

\* Hangfire

\* Paymob

\* Twilio

\* Firebase

\* SMTP

\* SignalR

\* OpenTelemetry

\* Serilog

\* The local file system

\* Other infrastructure technologies



\### 1.1 Architectural Role



The Infrastructure project provides implementations for capabilities such as:



\* Persistence

\* Repositories

\* Unit of Work

\* EF Core configuration

\* Database migrations

\* Transaction handling

\* Persistence interceptors

\* Authentication

\* ASP.NET Core Identity

\* Authorization infrastructure

\* Permission persistence

\* Password/token cryptography

\* Caching

\* External payment providers

\* Webhook processing

\* Shipping/location providers

\* Stock services

\* Notifications

\* Email/SMS/push delivery

\* SignalR delivery

\* Transactional Outbox processing

\* Integration-event dispatching

\* Background job execution

\* File storage

\* Health checks

\* Logging

\* Tracing

\* Metrics

\* Application clock

\* Post-commit processing



Infrastructure therefore provides the concrete runtime capabilities required by the rest of the application.



\### 1.2 Infrastructure Is Not a Business Layer



Infrastructure must not become a second Application or Domain layer.



The following distinction is fundamental:



| Layer          | Primary responsibility                                                   |

| -------------- | ------------------------------------------------------------------------ |

| Domain         | Business rules, aggregates, invariants, domain events                    |

| Application    | Use cases, orchestration, contracts, permissions, transaction boundaries |

| Infrastructure | Technical implementations and external-system integration                |

| API            | HTTP transport and API-facing concerns                                   |

| ReadModel      | Specialized read-side query implementation                               |



Infrastructure may contain substantial code, but that code is technical rather than business-authoritative.



For example:



\* Paymob integration belongs in Infrastructure.

\* Payment state rules belong in Domain.

\* Payment use-case orchestration belongs in Application.

\* HTTP payment endpoints belong in API.



The same principle applies throughout the system.



\---



\## 2. Architectural Philosophy



The E-Commerce modular monolith follows a dependency-inversion architecture.



Inner layers define abstractions where appropriate, and Infrastructure implements those abstractions.



```text

&#x20;                E-Commerce.Domain

&#x20;                        ▲

&#x20;                        │

&#x20;             E-Commerce.Application

&#x20;                   ▲          ▲

&#x20;                   │          │

&#x20;                   │          │

&#x20;      E-Commerce.Infrastructure   E-Commerce.ReadModel

&#x20;                   ▲          ▲

&#x20;                   │          │

&#x20;                   └────┬─────┘

&#x20;                        │

&#x20;                 E-Commerce.Api

```



More precisely:



```text

E-Commerce.Api

├── E-Commerce.Application

│   └── E-Commerce.Domain

│

├── E-Commerce.Infrastructure

│   ├── E-Commerce.Application

│   └── E-Commerce.Domain

│

└── E-Commerce.ReadModel

&#x20;   └── E-Commerce.Application

```



\*\*Infrastructure does not depend on ReadModel.\*\*



\*\*ReadModel does not depend on Infrastructure.\*\*



Application and Domain do not depend on Infrastructure.



This preserves the architectural direction toward the inner layers.



\### 2.1 Dependency Inversion



Infrastructure implements interfaces defined by inner layers whenever the capability represents an architectural boundary.



Examples include:



```text

Application abstraction

&#x20;       │

&#x20;       ▼

Infrastructure implementation

```



Examples:



```text

IFileService

&#x20;   ↓

FileService



ICache

&#x20;   ↓

RedisCache



IJobDispatcher

&#x20;   ↓

HangfireJobDispatcher



IEmailTransport

&#x20;   ↓

SmtpEmailTransport



IRepository<T>

&#x20;   ↓

Repository<T>



IUnitOfWork

&#x20;   ↓

UnitOfWork

```



The exact ownership of each abstraction follows the architectural responsibility of that abstraction.



Infrastructure-internal interfaces may also exist when they are purely implementation seams and are not application-facing contracts.



\---



\## 3. Scope and Responsibilities



\### 3.1 In Scope



Infrastructure is responsible for:



\* Persistence implementation

\* EF Core configuration

\* Database access

\* Repository implementations

\* Unit of Work implementation

\* Persistence interceptors

\* Database migrations

\* Identity persistence

\* Authentication implementation

\* Authorization framework integration

\* Permission persistence

\* Cryptographic implementations

\* Redis caching

\* Background job execution

\* Hangfire integration

\* Transactional Outbox processing

\* Integration-event dispatching

\* Notification delivery

\* Email/SMS/push providers

\* SignalR infrastructure

\* Post-commit processing

\* File storage

\* Payment provider integration

\* Payment webhooks

\* Shipping/location infrastructure

\* Stock infrastructure

\* Time infrastructure

\* Health checks

\* Logging

\* Tracing

\* Metrics

\* Infrastructure configuration

\* Dependency-injection registration



\### 3.2 Out of Scope



Infrastructure does not own:



\* Domain invariants

\* Aggregate business behavior

\* Business state transitions

\* Application use cases

\* HTTP controllers

\* API DTOs

\* Application query/command contracts

\* Domain authorization rules

\* Transactional business decisions

\* Domain-owned business policies

\* ReadModel query contracts

\* Specialized ReadModel SQL implementation



Infrastructure also must not become a dumping ground for reusable business logic simply because that logic is difficult to place elsewhere.



\---



\## 4. Infrastructure Project Structure



The current project is organized primarily by technical capability and infrastructure responsibility.



```text

E-Commerce.Infrastructure/

├── Caching/

├── Communication/

├── DependencyInjection/

├── Execution/

├── Extensions/

├── Files/

├── Observability/

├── Payment/

├── Persistence/

├── Scheduling/

├── Security/

├── Shipping/

├── Stock/

└── Time/

```



This organization intentionally differs from the Domain organization.



The Domain is organized around business concepts and bounded contexts.



Infrastructure is organized around the technical mechanisms required to implement those concepts.



\---



\## 5. Caching



```text

Caching/

├── Configuration/

│   └── RedisOptions.cs

├── Extensions/

│   └── RedisCachingExtensions.cs

└── Services/

&#x20;   └── RedisCache.cs

```



The Caching subsystem provides the concrete Redis-backed implementation of the application's caching abstraction.



Redis is an infrastructure technology and therefore must not leak into Domain code.



Responsibilities include:



\* Redis configuration

\* Connection registration

\* Cache implementation

\* Serialization

\* Expiration handling

\* Infrastructure-level Redis operations



Application code interacts with the cache through its abstraction rather than directly through StackExchange.Redis.



The Application layer therefore remains independent of Redis.



\---



\## 6. Communication Infrastructure



```text

Communication/

├── Messaging/

├── Notifications/

├── PostCommit/

└── Realtime/

```



Communication contains infrastructure mechanisms for asynchronous messaging, notifications, post-commit processing, and real-time delivery.



These mechanisms are deliberately separated from business meaning.



\### 6.1 Messaging



```text

Messaging/

├── Dispatching/

│   ├── DomainEventDispatcher.cs

│   └── IntegrationEventDispatcher.cs

└── Outbox/

&#x20;   ├── Configuration/

&#x20;   ├── Contracts/

&#x20;   ├── Decorators/

&#x20;   ├── Entities/

&#x20;   ├── Extensions/

&#x20;   ├── Implementation/

&#x20;   ├── Jobs/

&#x20;   ├── Processing/

&#x20;   └── Serialization/

```



The messaging subsystem implements the system's event-driven infrastructure.



The architecture distinguishes:



```text

Domain Event

&#x20;   ↓

Integration Event

&#x20;   ↓

Outbox

&#x20;   ↓

Background Processing

&#x20;   ↓

Integration Event Dispatcher

&#x20;   ↓

Application Handler

```



Domain events represent internal domain occurrences.



Integration events represent events intended to cross application/module boundaries.



Infrastructure provides the durable delivery mechanism.



\### 6.2 Transactional Outbox



The Outbox is a reliability mechanism that allows integration-event publication to be coupled atomically with the write transaction.



The conceptual flow is:



```text

Application command

&#x20;       ↓

Domain behavior

&#x20;       ↓

Domain event

&#x20;       ↓

Integration event

&#x20;       ↓

Outbox row

&#x20;       ↓

Same database transaction

&#x20;       ↓

Commit

&#x20;       ↓

Hangfire OutboxProcessingJob

&#x20;       ↓

Dispatcher

&#x20;       ↓

Integration-event handler

```



The Outbox ensures that the event is durably recorded with the business transaction before asynchronous delivery occurs.



Infrastructure contains:



\* `OutboxMessage`

\* `ProcessedEvent`

\* Dead-letter records

\* Outbox serialization

\* Outbox persistence

\* Outbox processing

\* Dispatching

\* Retry coordination

\* Dead-letter monitoring



The Outbox does not determine business meaning.



The module that owns the business event remains responsible for defining its meaning and contract.



\### 6.3 Idempotency



Integration-event consumers must be able to tolerate duplicate delivery.



The infrastructure provides `ProcessedEvent` persistence to support idempotent processing.



The conceptual rule is:



```text

Receive Event

&#x20;    ↓

Check ProcessedEvent

&#x20;    ↓

Already processed?

&#x20;  /          \\

&#x20;Yes           No

&#x20; ↓             ↓

Skip          Process

&#x20;               ↓

&#x20;      Record processed event

```



This is particularly important because durable asynchronous processing generally provides at-least-once delivery semantics rather than assuming exactly-once execution.



\### 6.4 Dead-Letter Handling



Infrastructure provides dead-letter persistence and monitoring for messages that cannot be successfully processed after the configured processing policy.



Dead-letter handling is operational infrastructure.



It does not decide whether the underlying business operation is valid.



The purpose is to preserve failed messages for investigation and controlled recovery rather than silently losing them.



\---



\## 7. Notifications Infrastructure



```text

Communication/

└── Notifications/

```



Notifications provide technical delivery mechanisms for:



\* Email

\* SMS

\* Push notifications



The architecture separates:



```text

Business meaning

&#x20;      ↓

Application / business module



Delivery

&#x20;      ↓

Infrastructure

```



For example, the Ordering module determines that an order confirmation should be sent.



Infrastructure determines how that message is technically delivered.



\### 7.1 Notification Channels



```text

Channels/

├── EmailChannel.cs

├── PushChannel.cs

└── SmsChannel.cs

```



Channels provide delivery-oriented infrastructure.



Providers include:



```text

Email

├── Composer

└── Transport

&#x20;   └── SMTP



SMS

├── Composer

└── Transport

&#x20;   └── Twilio



Push

├── Composer

└── Transport

&#x20;   └── Firebase

```



This isolates provider-specific technology from the rest of the application.



\### 7.2 Provider Abstraction



Provider implementations are isolated behind transport seams.



For example:



```text

IEmailTransport

&#x20;   ↓

SmtpEmailTransport



ISmsTransport

&#x20;   ↓

TwilioSmsTransport



IPushTransport

&#x20;   ↓

FirebasePushTransport

```



The provider-specific implementation remains in Infrastructure.



Changing SMTP, Twilio, or Firebase does not require Domain changes.



\### 7.3 Notification Persistence



Infrastructure also persists notification-related state such as:



\* Notification logs

\* Notification preferences

\* Push-device registrations

\* User notifications



These are persistence representations rather than Domain aggregates.



The Application layer owns the relevant use-case contracts and business behavior.



\---



\## 8. Realtime Infrastructure



```text

Communication/

└── Realtime/

&#x20;   ├── Extensions/

&#x20;   ├── Hubs/

&#x20;   └── Publishers/

```



SignalR belongs to Infrastructure because it is a transport mechanism.



The conceptual flow is:



```text

Business event

&#x20;     ↓

Application / post-commit processing

&#x20;     ↓

Realtime publisher

&#x20;     ↓

SignalR

&#x20;     ↓

Connected client

```



SignalR must not become the authoritative mechanism for durable business notifications.



The database/Outbox-backed notification state remains authoritative where durability is required.



SignalR is primarily a real-time delivery mechanism.



\---



\## 9. Post-Commit Infrastructure



```text

PostCommit/

├── Extensions/

└── Processing/

&#x20;   └── PostCommitProcessor.cs

```



Post-commit processing provides a mechanism for actions that should occur after a successful transaction commit.



This supports the architectural distinction between:



```text

Transaction

&#x20;   ↓

Commit

&#x20;   ↓

Best-effort post-commit work

```



And durable asynchronous work:



```text

Transaction

&#x20;   ↓

Outbox row

&#x20;   ↓

Commit

&#x20;   ↓

Background processing

```



Post-commit callbacks are therefore not a replacement for the transactional Outbox when durability is required.



\---



\## 10. Execution Context



```text

Execution/

└── AppContexts/

&#x20;   └── ExecutionContext.cs

```



Execution context infrastructure provides runtime context information required by infrastructure operations.



It may carry information such as:



\* Correlation context

\* Execution identity

\* Background execution state

\* Request/job context



Infrastructure execution context must remain technical.



Business meaning belongs to Application and Domain.



\---



\## 11. Persistence Architecture



Persistence is one of the primary Infrastructure responsibilities.



```text

Persistence/

├── Audit/

├── Common/

├── Configuration/

├── Context/

├── Extensions/

├── Interceptors/

├── Migrations/

└── Modules/

```



The Persistence subsystem implements the write-side database architecture using:



\* EF Core

\* SQL Server

\* Repositories

\* Unit of Work

\* Entity configurations

\* Persistence interceptors

\* Migrations



\### 11.1 AppDbContext



`AppDbContext` is the primary EF Core persistence context.



The context is organized through partial classes for modularity:



```text

Context/

├── AppDbContext.cs

└── partials/

&#x20;   ├── Catalog/

&#x20;   ├── CustomerEngagement/

&#x20;   ├── Finance/

&#x20;   ├── Files/

&#x20;   ├── Notifications/

&#x20;   ├── Onboarding/

&#x20;   ├── Ordering/

&#x20;   ├── People/

&#x20;   ├── Security/

&#x20;   └── Shipping/

```



This keeps a single transactional `DbContext` while allowing persistence configuration to remain organized by module.



The architecture remains a modular monolith rather than creating one `DbContext` per module without a business requirement.



\### 11.2 Persistence Modules



```text

Persistence/

└── Modules/

&#x20;   ├── Catalog/

&#x20;   ├── CustomerEngagement/

&#x20;   ├── Files/

&#x20;   ├── Finance/

&#x20;   ├── Notifications/

&#x20;   ├── Onboarding/

&#x20;   ├── Orders/

&#x20;   ├── Outbox/

&#x20;   ├── People/

&#x20;   ├── Security/

&#x20;   └── Shipping/

```



Each module contains its persistence-specific:



\* Entity configurations

\* Repositories

\* Persistence services



This mirrors the Domain/Application module boundaries without turning Infrastructure into another Domain model.



\### 11.3 Repository Implementation



Infrastructure implements repository abstractions defined by the appropriate inner layer.



The general dependency is:



```text

Repository abstraction

&#x20;       ↓

Infrastructure repository

&#x20;       ↓

EF Core

&#x20;       ↓

SQL Server

```



Repositories encapsulate persistence operations.



They should not become a second location for domain business rules.



\### 11.4 Unit of Work



Infrastructure implements the Unit of Work abstraction using EF Core.



The Unit of Work coordinates:



\* Entity persistence

\* Transactions

\* Change tracking

\* Domain-event collection

\* Outbox persistence where configured

\* Concurrency handling



The Unit of Work represents the transactional boundary of the write side.



It does not control external asynchronous delivery directly.



For example:



```text

UoW

&#x20;↓

Save business state

&#x20;↓

Persist Outbox

&#x20;↓

Commit

```



Rather than:



```text

UoW

&#x20;↓

Schedule Hangfire directly

```



Durable asynchronous work is triggered through the established Outbox/background-processing architecture.



\### 11.5 Persistence Interceptors



```text

Interceptors/

├── AuditAndSoftDeleteInterceptor.cs

├── LoggingInterceptor.cs

├── TimestampInterceptor.cs

└── ValidationInterceptor.cs

```



EF Core interceptors provide centralized persistence-level behavior.



Examples include:



\* Audit metadata

\* Soft-delete behavior

\* Timestamps

\* Persistence logging

\* Validation integration



These are technical persistence concerns.



They do not replace domain invariants.



\### 11.6 Optimistic Concurrency



The write side uses optimistic concurrency.



Entities that require concurrency protection expose a row-version/concurrency token.



Conceptually:



```text

Read entity

&#x20;   ↓

Version = X

&#x20;   ↓

Modify

&#x20;   ↓

UPDATE ... WHERE Version = X

&#x20;   ↓

Success → commit



Failure → concurrency conflict

```



Infrastructure translates the EF Core/database concurrency failure into the application's established concurrency exception path.



The API layer can then expose the appropriate HTTP conflict response.



\### 11.7 Audit and Soft Delete



Infrastructure provides persistence-level support for:



\* Audit logging

\* Created/modified timestamps

\* Soft deletion



These mechanisms are intentionally implemented outside Domain when they represent technical persistence behavior.



Domain rules concerning whether an entity may be deleted remain Domain concerns.



\### 11.8 Database Migrations



EF Core migrations belong to Infrastructure because they describe the physical persistence schema.



The conceptual ownership is:



```text

Domain

&#x20; ↓

Application

&#x20; ↓

Persistence model

&#x20; ↓

EF Core configuration

&#x20; ↓

EF Core migration

&#x20; ↓

SQL Server

```



Infrastructure also contains the startup migration support and migration configuration.



Startup migration remains a deployment/runtime decision rather than a Domain concern.



\---



\## 12. Audit Persistence



```text

Persistence/

└── Audit/

&#x20;   └── AuditLog.cs

```



`AuditLog` is infrastructure persistence state.



It records technical/business-operation audit information without becoming a Domain aggregate merely because it is stored in the database.



Audit requirements should not cause Domain objects to depend on EF Core or SQL Server.



\---



\## 13. Security Infrastructure



```text

Security/

├── Authentication/

├── Authorization/

├── Cryptography/

├── Extensions/

├── Identity/

└── Verification/

```



Security infrastructure integrates the application's security requirements with ASP.NET Core and external/security technologies.



\### 13.1 ASP.NET Core Identity



ASP.NET Core Identity belongs entirely outside the Domain model.



Infrastructure contains:



```text

Security/

└── Identity/

&#x20;   ├── Entities/

&#x20;   │   └── User.cs

&#x20;   └── Services/

```



`User` may derive from:



```csharp

IdentityUser<Guid>

```



Because ASP.NET Core Identity is a framework persistence/authentication mechanism.



The Domain does not depend on `IdentityUser`.



The architectural distinction is:



```text

IdentityUser

&#x20;   ↓

Authentication identity

```



Versus:



```text

Person

&#x20;   ↓

Business/customer identity

```



These concepts remain separate.



\### 13.2 Authentication



Infrastructure implements authentication mechanisms such as:



\* JWT generation

\* Refresh tokens

\* Authentication services

\* Current-user resolution

\* Token protection



Application owns the use cases and authorization decisions.



Infrastructure owns the concrete authentication technology.



\### 13.3 Authorization



Infrastructure integrates with ASP.NET Core authorization.



It contains:



\* Permission authorization handlers

\* Authorization policy providers

\* Authorization requirements

\* Permission persistence

\* Role persistence

\* Role-permission persistence

\* Data seeding



The distinction is:



```text

Application

&#x20;   ↓

What permission does the use case require?



Infrastructure

&#x20;   ↓

How does ASP.NET Core enforce that permission?

```



Permission business meaning should not become coupled to ASP.NET Core policy APIs.



\### 13.4 Cryptography



```text

Cryptography/

├── PasswordHasher.cs

├── RefreshTokenHasher.cs

└── RefreshTokenHasherOptions.cs

```



Cryptographic implementations belong in Infrastructure.



Password handling must not persist plaintext passwords.



Where ASP.NET Core Identity manages password hashing, the preferred flow is to provide the plaintext password to `UserManager.CreateAsync(...)` and allow Identity's configured password hasher to perform the hashing.



Infrastructure should not introduce redundant manual password-hash handling when Identity already owns that responsibility.



Refresh-token hashing may be implemented separately because refresh-token storage has different persistence/security requirements.



\### 13.5 Verification



Verification-code generation and technical verification services belong in Infrastructure when they depend on cryptographic/random generation or external delivery mechanisms.



The business workflow remains in Application/Domain.



For example:



```text

Registration aggregate

&#x20;       ↓

Verification requirement

&#x20;       ↓

Application workflow

&#x20;       ↓

Infrastructure verification implementation

```



\---



\## 14. Payment Infrastructure



```text

Payment/

├── Configuration/

├── Extensions/

└── Providers/

&#x20;   └── Paymob/

```



Payment infrastructure contains the technical integration with Paymob.



It includes:



\* Paymob API client

\* Provider request/response models

\* Currency conversion

\* Gateway implementation

\* Provider mapping

\* Provider exceptions

\* Webhook HMAC verification

\* Webhook processing



\### 14.1 Provider Isolation



The architecture isolates Paymob-specific concepts from the Domain.



```text

Domain Payment

&#x20;      ▲

&#x20;      │

Application payment contract

&#x20;      ▲

&#x20;      │

Infrastructure Paymob gateway

&#x20;      │

&#x20;      ▼

Paymob API

```



The Domain does not know about:



\* Paymob

\* HTTP clients

\* Paymob intention APIs

\* Paymob response models

\* Provider-specific HMAC implementation



\### 14.2 Webhooks



Payment webhooks are external-system communication.



Infrastructure is responsible for:



\* Receiving and parsing provider-specific data

\* Verifying HMAC/signatures

\* Converting provider payloads

\* Rejecting invalid provider messages

\* Passing validated information into the appropriate Application workflow



The webhook processor must not bypass the normal Application/Domain business process.



The business meaning of the following remains owned by the Finance business model:



\* Payment completed

\* Payment failed

\* Refund completed

\* Refund failed



\---



\## 15. Shipping Infrastructure



```text

Shipping/

├── Extensions/

├── Location/

└── Services/

```



Shipping infrastructure contains technical services such as:



\* Location providers

\* Map/geocoding integration

\* Infrastructure-level shipping calculations where appropriate



The distinction between Domain and Infrastructure remains important.



For example, `LocalShippingFeePolicy` may represent a business pricing policy.



Whereas `MapLocationService` represents an external technical integration.



Infrastructure must not duplicate Domain shipping rules.



\---



\## 16. Stock Infrastructure



```text

Stock/

├── Extensions/

└── Services/

```



Stock infrastructure provides technical implementations for stock-related application capabilities.



If stock availability is a business invariant, the authoritative decision must remain on the write/domain side.



Infrastructure may provide the persistence or external technical mechanism required to execute that decision.



\---



\## 17. Time Infrastructure



```text

Time/

├── Extensions/

└── Services/

&#x20;   └── ClockService.cs

```



The Domain depends on an abstraction such as:



```csharp

IClock

```



Infrastructure provides the runtime implementation.



This avoids direct use of system time from Domain code and makes time a replaceable infrastructure capability.



The dependency is:



```text

Domain

&#x20;  ↓

IClock

&#x20;  ↑

ClockService

```



\---



\## 18. File Storage Infrastructure



```text

Files/

├── Abstractions/

├── Configuration/

├── Entities/

├── Extensions/

├── Jobs/

├── Services/

└── Storage/

```



File storage separates application-level file operations from physical storage implementation.



The current provider is local file storage.



The architecture leaves room for future providers such as:



\* S3

\* Azure Blob Storage

\* Shared object storage



\### 18.1 File Storage Provider



The provider boundary is:



```text

File service

&#x20;     ↓

IFileStorageProvider

&#x20;     ↓

LocalFileStorageProvider

&#x20;     ↓

storage/files

```



The application-facing abstraction should remain independent of the physical storage mechanism when the capability is required by Application.



Infrastructure owns the concrete local filesystem implementation.



\### 18.2 File Lifecycle



The infrastructure supports controlled file lifecycle states such as:



```text

Available

&#x20;  ↓

PendingDeletion

&#x20;  ↓

ProcessingDeletion

&#x20;  ↓

Deleted

```



Deletion uses conditional state transitions and leases where required.



This protects against concurrent cleanup operations.



Physical deletion is a technical infrastructure operation.



\### 18.3 File Cleanup Jobs



Hangfire executes cleanup work through the Scheduling abstraction.



The architecture separates:



```text

Scheduling abstraction

&#x20;      ↓

Infrastructure scheduler

&#x20;      ↓

Hangfire

&#x20;      ↓

File cleanup job

```



The business application does not directly depend on Hangfire APIs.



\---



\## 19. Scheduling Infrastructure



```text

Scheduling/

├── Execution/

├── Extensions/

└── Hangfire/

```



Scheduling provides the infrastructure implementation of the application's controlled background-execution abstraction.



\### 19.1 Hangfire



Hangfire is an implementation detail.



Application code should depend on the application's scheduling abstractions rather than directly on:



\* `Hangfire.BackgroundJob`

\* `Hangfire.RecurringJob`



Infrastructure contains:



\* `HangfireJobDispatcher`

\* `HangfireJobScheduler`



These adapt the application scheduling model to Hangfire.



\### 19.2 Background Execution



The execution model is:



```text

Application job

&#x20;     ↓

Application scheduling abstraction

&#x20;     ↓

Infrastructure scheduler

&#x20;     ↓

Hangfire

&#x20;     ↓

Job handler

```



Hangfire owns scheduler-level concerns such as:



\* Persistence

\* Execution

\* Retry

\* Worker management

\* Recurring execution



The application's execution gateway owns application-level policies such as:



\* Execution context

\* Timeout policy

\* Logging

\* Tracing

\* Controlled invocation

\* Application-level resilience policies



Retries should not be redundantly implemented at multiple layers.



\### 19.3 Recurring Jobs



Recurring jobs are registered by Infrastructure through the scheduling bootstrapper.



Examples include:



\* Outbox processing

\* Dead-letter monitoring

\* File cleanup

\* Other infrastructure maintenance jobs



Business code does not need to know how those jobs are registered with Hangfire.



\---



\## 20. Observability Infrastructure



```text

Observability/

├── HealthChecks/

├── Logging/

├── Metrics/

└── Tracing/

```



Observability is an infrastructure concern because it integrates the application with runtime and operational telemetry systems.



\### 20.1 Logging



Serilog infrastructure provides:



\* Structured logging

\* Request logging integration

\* Infrastructure logging

\* Startup/shutdown logging

\* Provider diagnostics



Application and Domain should not depend on Serilog-specific APIs merely to express business behavior.



\### 20.2 Metrics



Infrastructure exposes application/runtime metrics through the configured meter infrastructure.



Metrics can represent:



\* Outbox backlog

\* Background job activity

\* External integration activity

\* Infrastructure failures

\* Operational measurements



Metrics are observational and must not become business state.



\### 20.3 Tracing



The tracing subsystem integrates OpenTelemetry with the application's execution flow.



It provides:



\* Activity sources

\* Span creation

\* Trace-context propagation

\* Infrastructure spans

\* External-call visibility



Tracing should allow an operation to be followed across:



```text

HTTP request

&#x20;   ↓

Application handler

&#x20;   ↓

Database

&#x20;   ↓

Outbox

&#x20;   ↓

Background job

&#x20;   ↓

External provider

```



This applies where explicit context propagation exists.



\### 20.4 Health Checks



Health checks expose infrastructure availability signals.



Examples include:



\* SQL Server

\* Redis

\* Hangfire

\* Outbox health

\* Outbox backlog



Health checks should represent operational health rather than business validity.



For example, SQL being unavailable is an infrastructure health problem, not a Domain business rule violation.



\---



\## 21. Database Health and Reliability



Infrastructure owns database reliability mechanisms such as:



\* Connection handling

\* Migration execution

\* Transaction handling

\* Concurrency translation

\* SQL diagnostics

\* Health checks



The system should distinguish:



```text

Transient infrastructure failure

&#x20;       ↓

Retry/recovery mechanism

```



From:



```text

Business rule violation

&#x20;       ↓

Domain/Application response

```



This distinction prevents technical failures from being modeled as business failures.



\---



\## 22. Dependency Injection



```text

DependencyInjection/

└── InfrastructureServiceRegistration.cs

```



Infrastructure provides the concrete dependency registrations required by the application.



Feature-level extension methods organize registrations by capability.



Examples include:



```csharp

AddPersistence();

AddSecurity();

AddCaching();

AddScheduling();

AddNotifications();

AddPayment();

AddShipping();

AddFileStorage();

AddHealthChecks();

```



The composition root invokes these registrations.



\### 22.1 Composition vs Activation



Infrastructure registration and infrastructure execution are separate concerns.



For example, `AddReadModel()` or `AddInfrastructure()` registers dependencies.



It should not implicitly perform unrelated database deployment operations.



Similarly, `DbUpMigrationRunner` executes database migrations when explicitly invoked.



This separation keeps dependency registration predictable.



\---



\## 23. Configuration



Infrastructure owns configuration for technical capabilities.



Examples include:



\* Redis options

\* Payment provider options

\* File-storage options

\* Migration options

\* Outbox options

\* Health-check options

\* Hangfire configuration

\* JWT configuration

\* Verification options



Options should be validated during startup where appropriate.



Configuration validation prevents invalid infrastructure configuration from remaining undetected until a runtime operation occurs.



\---



\## 24. External Integration Boundary



Infrastructure is the system's external technology boundary.



```text

&#x20;                 Infrastructure

&#x20;                       │

&#x20;         ┌─────────────┼─────────────┐

&#x20;         │             │             │

&#x20;       Paymob        Twilio        Firebase

&#x20;         │             │             │

&#x20;         └─────────────┼─────────────┘

&#x20;                       │

&#x20;                      SMTP

```



Other external technologies include:



\* Redis

\* SQL Server

\* Hangfire

\* Filesystem

\* OpenTelemetry infrastructure



External models should not leak into Domain.



Provider-specific DTOs remain inside the corresponding Infrastructure integration.



\---



\## 25. Failure Handling



Infrastructure is responsible for translating technical failures into the application's established failure mechanisms.



\### Database



```text

SQL exception

&#x20;   ↓

EF Core / ADO.NET

&#x20;   ↓

Infrastructure handling/translation

&#x20;   ↓

Application exception model

```



\### Concurrency



```text

DbUpdateConcurrencyException

&#x20;   ↓

Infrastructure

&#x20;   ↓

Concurrency exception

&#x20;   ↓

API → 409 Conflict

```



\### External Provider



```text

Paymob failure

&#x20;   ↓

PaymobApiException

&#x20;   ↓

Infrastructure/Application handling

&#x20;   ↓

Established application failure path

```



\### Background Processing



```text

Job failure

&#x20;   ↓

Hangfire execution/retry policy

&#x20;   ↓

Retry

&#x20;   ↓

Dead-letter/monitoring where applicable

```



Infrastructure must not hide failures merely to make operations appear successful.



\---



\## 26. Transaction Boundaries



Infrastructure implements the transactional capabilities required by Application.



The principal write transaction is centered around the EF Core `DbContext` and Unit of Work.



A typical operation is:



```text

Application command

&#x20;      ↓

Domain behavior

&#x20;      ↓

Repository changes

&#x20;      ↓

Unit of Work

&#x20;      ↓

SaveChanges

&#x20;      ↓

Outbox persistence

&#x20;      ↓

Database commit

```



External side effects such as email, push notifications, or integration-event delivery should not be treated as part of the same database transaction.



Instead, durable asynchronous work uses the Outbox.



\---



\## 27. Outbox vs Direct External Calls



The architecture deliberately avoids patterns such as:



```text

Database transaction

&#x20;     ↓

Send email

&#x20;     ↓

Call payment provider

&#x20;     ↓

Publish event

&#x20;     ↓

Commit database

```



An external call cannot participate reliably in the SQL Server transaction.



Instead:



```text

Business state

&#x20;     +

Outbox record

&#x20;     ↓

Single transaction

&#x20;     ↓

Commit

&#x20;     ↓

Background processing

&#x20;     ↓

External side effect

```



This reduces failure windows and makes asynchronous delivery recoverable.



\---



\## 28. Persistence vs ReadModel



Infrastructure owns the EF Core \*\*write-side persistence\*\*.



ReadModel owns the specialized \*\*read-side implementation\*\*.



The distinction is:



| Concern                   | Infrastructure                             | ReadModel                        |

| ------------------------- | ------------------------------------------ | -------------------------------- |

| EF Core                   | Yes                                        | No                               |

| DbContext                 | Yes                                        | No                               |

| DbSet                     | Yes                                        | No                               |

| Repository                | Yes                                        | No                               |

| Unit of Work              | Yes                                        | No                               |

| Transactional writes      | Yes                                        | No                               |

| ADO.NET                   | Not as the ReadModel mechanism             | Yes                              |

| Stored procedures         | General infrastructure only where required | Yes                              |

| DbUp ReadModel migrations | No                                         | Yes                              |

| Specialized SQL queries   | Not its primary role                       | Yes                              |

| Read projections          | No current persisted projections           | No current persisted projections |

| Read replica              | No current replica                         | No current replica               |



The two projects therefore have different responsibilities.



Infrastructure is the write-side technical implementation layer.



ReadModel is the specialized read-side technical subsystem.



\---



\## 29. Domain Boundary Protection



Infrastructure may reference Domain, but Domain must never reference Infrastructure.



For example, Domain may define:



```csharp

IRepository<T>

IUnitOfWork

IClock

```



Infrastructure provides:



```csharp

Repository<T>

UnitOfWork

ClockService

```



The Domain remains unaware of:



\* EF Core

\* SQL Server

\* Redis

\* Hangfire

\* ASP.NET Core

\* Identity

\* Paymob

\* Twilio

\* Firebase

\* SignalR



This is a critical architectural invariant.



\---



\## 30. Application Boundary Protection



Application may define:



```csharp

IFileService

ICache

IJobDispatcher

IEmailService

IPaymentGateway

```



Infrastructure provides concrete implementations.



Application therefore does not need to know whether the underlying technology is:



\* Redis

\* Hangfire

\* Paymob

\* SMTP

\* Firebase

\* Local filesystem



This allows infrastructure technology to evolve without rewriting application use cases.



\---



\## 31. Infrastructure and API



The API is the composition root and HTTP boundary.



The relationship is:



```text

API

&#x20;├── Registers Infrastructure

&#x20;├── Configures Infrastructure

&#x20;├── Invokes required infrastructure startup operations

&#x20;└── Exposes HTTP endpoints

```



API may reference Infrastructure for composition.



Infrastructure must not depend on API controllers.



Infrastructure does not contain:



\* Controllers

\* API DTOs

\* HTTP route definitions



Provider-specific HTTP communication belongs in Infrastructure only when the HTTP communication is to an \*\*external system\*\*.



\---



\## 32. Infrastructure and ReadModel



Infrastructure does not reference ReadModel.



The ReadModel is an independent outer implementation project that also references Application.



```text

&#x20;         Application

&#x20;              ▲

&#x20;              │

&#x20;      ┌───────┴────────┐

&#x20;      │                │

Infrastructure      ReadModel

```



This prevents the write-side infrastructure from becoming coupled to the specialized read subsystem.



The API composition root may reference both.



\---



\## 33. Security Boundaries



Infrastructure contains sensitive technical capabilities.



Examples include:



\* Database connections

\* Payment credentials

\* JWT signing configuration

\* Redis credentials

\* SMTP credentials

\* Firebase credentials

\* Migration credentials



Secrets must not be committed to source control.



Production credentials should follow the principle of least privilege.



Where possible, separate credentials should be used for different security roles.



A particularly important example is:



```text

Runtime database identity

&#x20;       ↓

Normal application permissions



Migration identity

&#x20;       ↓

Schema modification permissions

```



The runtime application identity should not receive unnecessary DDL privileges.



\---



\## 34. Generated Artifacts



Build-generated directories such as:



```text

bin/

obj/

```



are not architectural source structure.



They should be:



\* Excluded from source control

\* Excluded from architecture documentation

\* Generated by the build process



The documented Infrastructure architecture should represent source-controlled implementation structure only.



\---



\## 35. Naming and Organizational Conventions



Infrastructure should maintain consistent naming conventions across modules.



For example, `Repositories/`, `Configurations/`, and `Services/` should be preferred consistently over mixed singular/plural naming unless a deliberate distinction exists.



Similarly, `Configuration/` and `Configurations/` should not be mixed arbitrarily.



These are organizational concerns rather than architectural defects, but consistency improves maintainability and discoverability.



\---



\## 36. Architectural Decision Rules



The following rules govern Infrastructure evolution.



\### Rule 1 — Keep Infrastructure Technical



If a component primarily answers:



> "How do we technically perform this operation?"



it is a candidate for Infrastructure.



If it answers:



> "What business rule should apply?"



it belongs in Domain/Application.



\### Rule 2 — Keep Framework Dependencies Outside Domain



EF Core, ASP.NET Core Identity, Hangfire, Redis, SignalR, Firebase, Paymob, and similar frameworks/providers must not leak into Domain.



\### Rule 3 — Prefer Inner-Layer Abstractions for Application-Facing Capabilities



When Application requires a replaceable infrastructure capability, the abstraction should normally be owned by Application and implemented here.



\### Rule 4 — Infrastructure May Contain Internal Implementation Abstractions



Not every interface needs to live in Application.



An interface that exists solely to separate two Infrastructure implementation details may remain inside Infrastructure.



The deciding factor is architectural ownership, not the mere presence of an interface.



\### Rule 5 — Do Not Duplicate Business Logic



Infrastructure may transform, serialize, map, persist, transport, or execute.



It must not duplicate Domain rules simply because those rules are needed to perform a technical operation.



\### Rule 6 — Use Durable Mechanisms for Durable Requirements



If an operation must survive process restart or temporary failure, use durable infrastructure such as:



\* Database persistence

\* Outbox

\* Hangfire

\* Durable notification state



Do not rely solely on in-memory callbacks for durable business effects.



\### Rule 7 — Do Not Introduce Distributed Infrastructure Without a Real Requirement



The current architecture is intentionally a modular monolith.



Kafka, RabbitMQ, service-to-service HTTP, separate databases, or other distributed infrastructure should only be introduced when an actual architectural requirement justifies the additional operational complexity.



\### Rule 8 — Prefer Measured Optimization



Infrastructure performance optimizations should be driven by workload evidence.



Examples include:



\* Connection-pool tuning

\* Redis optimization

\* Database indexing

\* Caching

\* Batching

\* Read replicas

\* Specialized SQL



Do not introduce complexity merely because a future workload might theoretically require it.



\---



\## 37. Current Infrastructure Architecture



The current Infrastructure architecture can be summarized as:



```text

&#x20;                        APPLICATION

&#x20;                             │

&#x20;               ┌─────────────┼─────────────┐

&#x20;               │             │             │

&#x20;               ▼             ▼             ▼

&#x20;          Persistence     Security    Application Services

&#x20;               │             │             │

&#x20;               ▼             ▼             ▼

&#x20;            EF Core       Identity   Technical Providers

&#x20;               │             │             │

&#x20;               └─────────────┼─────────────┘

&#x20;                             │

&#x20;                E-Commerce.Infrastructure

&#x20;                             │

&#x20;      ┌──────────┬───────────┼───────────┬──────────┐

&#x20;      │          │           │           │          │

&#x20;      ▼          ▼           ▼           ▼          ▼

&#x20;  SQL Server   Redis      Hangfire    Providers    Files

&#x20;                             │

&#x20;                             ▼

&#x20;                      Background Jobs

&#x20;                             │

&#x20;                 ┌───────────┼───────────┐

&#x20;                 ▼           ▼           ▼

&#x20;               Outbox   Notifications  Cleanup

&#x20;                             │

&#x20;                 ┌───────────┼───────────┐

&#x20;                 ▼           ▼           ▼

&#x20;                SMTP       Twilio      Firebase

```



\---



\## 38. Complete Responsibility Matrix



| Capability                           | Owner          |

| ------------------------------------ | -------------- |

| Business invariants                  | Domain         |

| Aggregate behavior                   | Domain         |

| Domain events                        | Domain         |

| Use cases                            | Application    |

| Application contracts                | Application    |

| Permission requirements              | Application    |

| Transaction orchestration            | Application    |

| EF Core implementation               | Infrastructure |

| SQL Server persistence               | Infrastructure |

| Repositories                         | Infrastructure |

| Unit of Work implementation          | Infrastructure |

| EF Core migrations                   | Infrastructure |

| Persistence interceptors             | Infrastructure |

| ASP.NET Identity                     | Infrastructure |

| JWT implementation                   | Infrastructure |

| Refresh tokens                       | Infrastructure |

| Redis implementation                 | Infrastructure |

| Hangfire implementation              | Infrastructure |

| Outbox persistence/processing        | Infrastructure |

| Integration-event transport/dispatch | Infrastructure |

| Email/SMS/Push delivery              | Infrastructure |

| SignalR transport                    | Infrastructure |

| Paymob integration                   | Infrastructure |

| Payment webhook verification         | Infrastructure |

| Location provider                    | Infrastructure |

| File storage provider                | Infrastructure |

| Health checks                        | Infrastructure |

| Serilog/OpenTelemetry integration    | Infrastructure |

| Specialized ADO.NET read subsystem   | ReadModel      |

| Read-side SQL procedures             | ReadModel      |

| ReadModel DbUp migrations            | ReadModel      |

| HTTP endpoints                       | API            |

| API DTOs                             | API            |

| API versioning                       | API            |



\---



\## 39. Final Architectural Rules



1\. \*\*Infrastructure is an outer implementation layer.\*\*

2\. \*\*Infrastructure implements technical capabilities required by Application and Domain without becoming a business layer.\*\*

3\. \*\*Domain remains completely framework agnostic.\*\*

4\. \*\*Application owns use cases and application-facing contracts.\*\*

5\. \*\*Infrastructure owns concrete technical implementations.\*\*

6\. \*\*EF Core and SQL Server belong to Infrastructure.\*\*

7\. \*\*ASP.NET Core Identity belongs to Infrastructure.\*\*

8\. \*\*Redis, Hangfire, SignalR, Firebase, Twilio, SMTP, Paymob, and filesystem implementations belong to Infrastructure.\*\*

9\. \*\*The transactional Outbox belongs to Infrastructure as a persistence and delivery mechanism, while integration-event meaning remains owned by the appropriate business/application module.\*\*

10\. \*\*Background jobs execute through the application's scheduling abstraction and are implemented by Hangfire infrastructure.\*\*

11\. \*\*External side effects that require durability should use the established Outbox/background-processing architecture rather than direct in-transaction scheduling or unreliable in-memory execution.\*\*

12\. \*\*Infrastructure repositories implement inner-layer persistence abstractions and do not contain business invariants.\*\*

13\. \*\*The Unit of Work coordinates transactional persistence but does not directly perform durable external side effects.\*\*

14\. \*\*Persistence interceptors provide technical database behavior and do not replace Domain invariants.\*\*

15\. \*\*Payment provider details remain isolated from the Domain model.\*\*

16\. \*\*Webhook verification is an infrastructure security concern; the resulting business event is handled through the normal Application/Domain workflow.\*\*

17\. \*\*Notification delivery is Infrastructure; notification meaning belongs to the appropriate Application/business module.\*\*

18\. \*\*SignalR is a delivery mechanism, not the durable source of notification state.\*\*

19\. \*\*Runtime and migration database credentials should be separated in production where schema modification is required.\*\*

20\. \*\*Infrastructure must not reference ReadModel.\*\*

21\. \*\*ReadModel remains a separate specialized read-side subsystem using ADO.NET and DbUp.\*\*

22\. \*\*No distributed infrastructure should be introduced without a concrete architectural requirement.\*\*

23\. \*\*Infrastructure performance optimization should be driven by measured workload characteristics rather than speculation.\*\*

24\. \*\*Generated `bin/` and `obj/` directories are not part of the documented architecture and should remain excluded from source control.\*\*



\---



\## 40. Final Architecture Summary



E-Commerce.Infrastructure is the technical implementation boundary of the modular monolith.



It connects the stable inner architecture to concrete technologies:



```text

&#x20;                   DOMAIN

&#x20;                      │

&#x20;                Business Rules

&#x20;                      │

&#x20;                      ▼

&#x20;                 APPLICATION

&#x20;                      │

&#x20;              Use Cases + Contracts

&#x20;                      │

&#x20;                      ▼

&#x20;               INFRASTRUCTURE

&#x20;                      │

&#x20;         ┌────────────┼─────────────┐

&#x20;         │            │             │

&#x20;         ▼            ▼             ▼

&#x20;     Persistence  External Systems  Runtime

&#x20;         │            │             │

&#x20;         ▼            ▼             ▼

&#x20;     SQL Server    Paymob/Twilio   Hangfire

&#x20;     EF Core       Firebase/SMTP   Redis

&#x20;     Identity      Providers       SignalR

&#x20;         │            │             │

&#x20;         └────────────┼─────────────┘

&#x20;                      │

&#x20;                      ▼

&#x20;               OPERABLE SYSTEM

```



The architectural objective is not to make Infrastructure invisible.



Its objective is to \*\*contain technical complexity so that the Domain and Application layers do not become coupled to it\*\*.



The resulting separation is:



\*\*Domain defines business truth. Application orchestrates business use cases. Infrastructure implements the technical mechanisms required to execute those use cases. API exposes them through HTTP. ReadModel provides specialized read-side execution where the workload justifies it.\*\*



This preserves the modular monolith's primary architectural properties:



\* Strong dependency direction

\* Framework-independent Domain

\* Explicit Application boundaries

\* Replaceable infrastructure implementations

\* Transactional consistency

\* Durable asynchronous processing

\* Controlled external integrations

\* Operational observability

\* Security isolation

\* Room for future evolution without prematurely introducing distributed-system complexity



\---



\*\*Document version:\*\* 1.0 – Final

\*\*Feature:\*\* Infrastructure Architecture



