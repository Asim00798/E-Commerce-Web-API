\# Registration \& Account Verification — Technical Reference Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Scope:\*\* Onboarding — Registration aggregate, verification workflow, provisioning, persistence, background cleanup

\*\*Version:\*\* 1.0 — Final

\*\*Related documents:\*\* Registration Technical Requirements, Registration Domain Model Technical Reference, Registration Workflow, Notifications Technical Reference, Outbox Technical Reference



\---



\## 1. Overview



The Registration \& Account Verification feature implements the onboarding workflow from first user contact to provisioned Identity account. It spans the Domain, Application, Infrastructure, and API layers, and it depends on the Outbox pipeline for durable notification delivery and the Notifications subsystem for email and SMS transport.



The feature covers:



\* Creation of a pending registration with email, phone, username, and password hash.

\* Generation and hashed storage of one-time verification codes per channel.

\* Delivery of verification codes through the Outbox → Notifications pipeline.

\* Verification of email and phone channels with attempt, resend, and cooldown limits.

\* Detection of full verification and emission of an integration event that triggers provisioning.

\* Provisioning of an ASP.NET Core Identity user with the Customer role, atomic with registration removal.

\* Background cleanup of expired or abandoned registrations.



The Domain aggregate owns verification state and invariants. The Application layer orchestrates use cases. The Infrastructure layer owns persistence, code hashing, and job scheduling. The API layer exposes the endpoints to clients.



\---



\## 2. Architecture and Placement



\### 2.1 Layer Overview



```text

Domain/

└── BoundedContexts/UserManagement/Onboarding/

&#x20;   └── AggregateRoots/Registration/       ← Aggregate and value objects



Application/

└── BoundedContexts/Onboarding/            ← Commands, queries, handlers, integration events



Infrastructure/

├── Persistence/Modules/Onboarding/        ← EF configuration, repository, cleanup service

└── Security/Verification/                 ← Verification code service



API/

└── Controllers/v1/Security/               ← AuthenticationController (registration endpoints)

```



\### 2.2 Layer Responsibilities



| Layer          | Responsibility                                                                                                                                                                                                                                                                                                                        |

| -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |

| Domain         | Registration aggregate; value objects (`Username`, `CredentialHash`, `VerificationChannel`); enums; domain events; `IRegistrationRepository`                                                                                                                                                                                          |

| Application    | Commands (`Register`, `VerifyEmail`, `VerifyPhone`, `ResendEmail`, `ResendPhone`); domain event handler (`RegistrationFullyVerifiedDomainEventHandler`); integration event handlers (`SendVerificationEmailHandler`, `SendVerificationSmsHandler`, `CreateUserOnRegistrationVerifiedHandler`); jobs (`ExpiredRegistrationCleanupJob`) |

| Infrastructure | `RegistrationConfiguration`; `RegistrationRepository`; `RegistrationCleanupService`; `VerificationCodeService`; `VerificationOptions`; cleanup job handler and trigger                                                                                                                                                                |

| API            | Controllers that accept register, verify, and resend requests and dispatch commands; request and response DTOs                                                                                                                                                                                                                        |



\### 2.3 Dependency Direction



```text

API → Application → Domain



Infrastructure → Application, Domain

```



The Domain layer references no external framework. The Application layer references only Domain plus shared abstractions. Infrastructure implements the abstractions. The API layer is thin.



\---



\## 3. Folder Structure



\### 3.1 Domain



```text

Domain/

└── BoundedContexts/

&#x20;   └── UserManagement/

&#x20;       └── Onboarding/

&#x20;           ├── AggregateRoots/

&#x20;           │   └── Registration/

&#x20;           │       ├── Behaviors/

&#x20;           │       │   ├── Registration.cs                  // State, constructor, guards

&#x20;           │       │   ├── Registration.Email.cs            // Email verification behaviors

&#x20;           │       │   ├── Registration.Phone.cs            // Phone verification behaviors

&#x20;           │       │   └── Registration.Lifecycle.cs        // Completion and expiry

&#x20;           │       ├── Constants/

&#x20;           │       │   └── RegistrationConstants.cs         // Lifetimes, limits, cooldowns

&#x20;           │       ├── Events/

&#x20;           │       │   ├── EmailVerificationCodeGeneratedDomainEvent.cs

&#x20;           │       │   ├── EmailVerifiedDomainEvent.cs

&#x20;           │       │   ├── PhoneVerificationCodeGeneratedDomainEvent.cs

&#x20;           │       │   ├── PhoneVerifiedDomainEvent.cs

&#x20;           │       │   ├── RegistrationCreatedDomainEvent.cs

&#x20;           │       │   ├── RegistrationExpiredDomainEvent.cs

&#x20;           │       │   └── RegistrationFullyVerifiedDomainEvent.cs

&#x20;           │       ├── Exceptions/

&#x20;           │       │   └── RegistrationException.cs

&#x20;           │       └── ValueObjects/

&#x20;           │           ├── CredentialHash.cs

&#x20;           │           ├── Username.cs

&#x20;           │           └── VerificationChannel.cs

&#x20;           └── Repositories/

&#x20;               └── IRegistrationRepository.cs

```



\### 3.2 Application



```text

Application/

└── BoundedContexts/

&#x20;   └── Onboarding/

&#x20;       ├── Abstractions/

&#x20;       │   └── IRegistrationCleanupService.cs

&#x20;       ├── Commands/

&#x20;       │   ├── Register/

&#x20;       │   │   ├── RegisterCommand.cs

&#x20;       │   │   └── RegisterCommandHandler.cs

&#x20;       │   ├── VerifyEmail/

&#x20;       │   │   ├── VerifyEmailCommand.cs

&#x20;       │   │   └── VerifyEmailCommandHandler.cs

&#x20;       │   ├── VerifyPhone/

&#x20;       │   │   ├── VerifyPhoneCommand.cs

&#x20;       │   │   └── VerifyPhoneCommandHandler.cs

&#x20;       │   ├── ResendEmail/

&#x20;       │   │   ├── ResendEmailCommand.cs

&#x20;       │   │   └── ResendEmailCommandHandler.cs

&#x20;       │   └── ResendPhone/

&#x20;       │       ├── ResendPhoneCommand.cs

&#x20;       │       └── ResendPhoneCommandHandler.cs

&#x20;       ├── DomainEventHandlers/

&#x20;       │   └── RegistrationFullyVerifiedDomainEventHandler.cs

&#x20;       ├── IntegrationEventHandlers/

&#x20;       │   ├── CreateUserOnRegistrationVerifiedHandler.cs

&#x20;       │   ├── SendVerificationEmailHandler.cs

&#x20;       │   └── SendVerificationSmsHandler.cs

&#x20;       ├── IntegrationEvents/

&#x20;       │   ├── EmailVerificationRequestedIntegrationEvent.cs

&#x20;       │   ├── PhoneVerificationRequestedIntegrationEvent.cs

&#x20;       │   └── RegistrationFullyVerifiedIntegrationEvent.cs

&#x20;       ├── Jobs/

&#x20;       │   ├── ExpiredRegistrationCleanupJob.cs

&#x20;       │   ├── ExpiredRegistrationCleanupJobHandler.cs

&#x20;       │   └── ExpiredRegistrationCleanupTrigger.cs

&#x20;       └── Models/

&#x20;           ├── RegistrationVerificationEmail.cs

&#x20;           └── RegistrationVerificationSms.cs

```



\### 3.3 Infrastructure



```text

Infrastructure/

├── Persistence/

│   └── Modules/

│       └── Onboarding/

│           ├── Configuration/

│           │   └── RegistrationConfiguration.cs

│           ├── Repository/

│           │   └── RegistrationRepository.cs

│           └── Services/

│               └── RegistrationCleanupService.cs

└── Security/

&#x20;   └── Verification/

&#x20;       ├── VerificationCodeService.cs

&#x20;       └── VerificationOptions.cs

```



\### 3.4 API



```text

API/

├── Controllers/

│   └── v1/

│       └── Security/

│           └── AuthenticationController.cs  // Register, verify, resend endpoints

└── DTOs/

&#x20;   └── v1/

&#x20;       └── Security/

&#x20;           ├── Requests/

&#x20;           │   ├── RegisterRequest.cs

&#x20;           │   ├── VerifyEmailRequest.cs

&#x20;           │   ├── VerifyPhoneRequest.cs

&#x20;           │   ├── ResendEmailRequest.cs

&#x20;           │   └── ResendPhoneRequest.cs

&#x20;           └── Responses/

&#x20;               └── RegistrationResponse.cs

```



> \*\*Verification required:\*\* Check the exact API DTO file names and locations against the current tree. The folder dump reviewed earlier showed `Controllers/v1/Security/AuthenticationController.cs`, but the DTO paths under `v1/Security/` were not itemized.



\---



\## 4. Domain Layer



\### 4.1 Registration Aggregate



The aggregate root inherits `BaseEntity` for identity, audit fields, and domain event collection, and implements `IAggregateRoot`.



\*\*Properties (private set):\*\*



| Property                | Type                  | Purpose                                                            |

| ----------------------- | --------------------- | ------------------------------------------------------------------ |

| `Email`                 | `Email`               | The registration's email address                                   |

| `PhoneNumber`           | `PhoneNumber`         | The registration's phone number                                    |

| `Username`              | `Username`            | Unique username for the future account                             |

| `PasswordHash`          | `CredentialHash`      | Pre-hashed password                                                |

| `EmailVerification`     | `VerificationChannel` | Immutable state of the email channel                               |

| `PhoneVerification`     | `VerificationChannel` | Immutable state of the phone channel                               |

| `ExpiresAtUtc`          | `DateTime?`           | Absolute lifetime of the registration                              |

| `RegistrationCompleted` | `bool`                | Guard flag preventing further modification after full verification |



The aggregate is physically removed after account provisioning. It does not use soft-delete.



\### 4.2 Behaviors



The aggregate is split across four partial files for readability:



| File                        | Contents                                                                                                                                 |

| --------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |

| `Registration.cs`           | Static factory `Create`; property declarations; `CollectDomainEvents` helpers; guard methods `AssertNotExpired` and `AssertNotCompleted` |

| `Registration.Email.cs`     | `SetEmailVerificationCode(hashedCode, utcNow)`; `VerifyEmail(isCodeValid, utcNow)`                                                       |

| `Registration.Phone.cs`     | `SetPhoneVerificationCode(hashedCode, utcNow)`; `VerifyPhone(isCodeValid, utcNow)`                                                       |

| `Registration.Lifecycle.cs` | `TryCompleteRegistration()`; `MarkExpired()`; completion logic                                                                           |



> \*\*Verification required:\*\* Confirm the exact method names and signatures against the actual files. This document describes the intended behavior based on the Domain Model Technical Reference; the code may use slightly different names.



\### 4.3 Value Objects



\#### `VerificationChannel`



An immutable record:



```csharp

public sealed record VerificationChannel

{

&#x20;   public string? CodeHash { get; init; }



&#x20;   public DateTime? SentAt { get; init; }



&#x20;   public DateTime? ExpiresAt { get; init; }



&#x20;   public DateTime? VerifiedAt { get; init; }



&#x20;   public int AttemptCount { get; init; }



&#x20;   public int ResendCount { get; init; }



&#x20;   public static readonly VerificationChannel Empty = new();



&#x20;   public bool IsVerified => VerifiedAt.HasValue;



&#x20;   public bool IsCodeActive(DateTime utcNow) =>

&#x20;       ExpiresAt.HasValue \&\& utcNow <= ExpiresAt.Value;

}

```



State changes are performed by replacing the entire record with a new instance—never by mutating individual properties.



\#### `Username`



A non-empty, trimmed, lower-cased value object. It serves as the unique identifier for the registration.



\#### `CredentialHash`



Wraps a pre-hashed password string. The Domain layer never hashes or verifies the password; it only stores the hash.



\#### Shared Kernel Value Objects



`Email` and `PhoneNumber` come from the Shared Kernel and are reused across bounded contexts.



\### 4.4 Business Constants



\*\*File:\*\* `Domain/BoundedContexts/UserManagement/Onboarding/AggregateRoots/Registration/Constants/RegistrationConstants.cs`



```csharp

public static class RegistrationConstants

{

&#x20;   public const int MaxAttemptsPerChannel = 5;



&#x20;   public const int MaxResendsPerChannel = 3;



&#x20;   public static readonly TimeSpan DefaultLifetime = TimeSpan.FromMinutes(15);



&#x20;   public static readonly TimeSpan OTPLifetime = TimeSpan.FromMinutes(5);



&#x20;   public static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);

}

```



The aggregate references these constants exclusively—no magic numbers.



\### 4.5 Domain Events



| Event                                       | Raised when                                                                     |

| ------------------------------------------- | ------------------------------------------------------------------------------- |

| `RegistrationCreatedDomainEvent`            | The `Create` factory completes successfully                                     |

| `EmailVerificationCodeGeneratedDomainEvent` | A new email code is set, either initially or during resend                      |

| `PhoneVerificationCodeGeneratedDomainEvent` | A new phone code is set, either initially or during resend                      |

| `EmailVerifiedDomainEvent`                  | The email channel is successfully verified                                      |

| `PhoneVerifiedDomainEvent`                  | The phone channel is successfully verified                                      |

| `RegistrationFullyVerifiedDomainEvent`      | Both channels are verified and `RegistrationCompleted` is set                   |

| `RegistrationExpiredDomainEvent`            | The registration is marked expired due to attempt exhaustion or lifetime expiry |



Domain events are declarative and carry immutable data, such as `RegistrationId`, `Email`, `PhoneNumber`, and `Username`, as applicable. They are collected by `BaseEntity` and dispatched by the Unit of Work inside the transaction.



\*\*Note on event usage:\*\* The core four events—`EmailVerifiedDomainEvent`, `PhoneVerifiedDomainEvent`, `RegistrationFullyVerifiedDomainEvent`, and `RegistrationExpiredDomainEvent`—are the ones that matter to the pipeline. `RegistrationCreatedDomainEvent` and the code-generated events also exist; verify whether they have handlers before treating them as part of the flow.



\### 4.6 Repository Interface



\*\*File:\*\* `Domain/.../Repositories/IRegistrationRepository.cs`



```csharp

public interface IRegistrationRepository : IRepository<Registration>

{

&#x20;   Task<Registration?> GetByEmailAsync(

&#x20;       string email,

&#x20;       CancellationToken ct = default);



&#x20;   Task<Registration?> GetByPhoneAsync(

&#x20;       string phone,

&#x20;       CancellationToken ct = default);



&#x20;   Task<bool> ExistsByEmailAsync(

&#x20;       string email,

&#x20;       CancellationToken ct = default);



&#x20;   Task<bool> ExistsByPhoneAsync(

&#x20;       string phone,

&#x20;       CancellationToken ct = default);



&#x20;   Task<bool> ExistsByUsernameAsync(

&#x20;       string username,

&#x20;       CancellationToken ct = default);

}

```



> \*\*Verification required:\*\* Confirm the exact method set against the file. The domain model requires these lookups for duplicate-prevention checks. The interface may also include a bulk-expired query used by the cleanup service.



\---



\## 5. Application Layer



\### 5.1 Commands



| Command              | Purpose                               |

| -------------------- | ------------------------------------- |

| `RegisterCommand`    | Create a new registration             |

| `VerifyEmailCommand` | Submit a code for email verification  |

| `VerifyPhoneCommand` | Submit a code for phone verification  |

| `ResendEmailCommand` | Request a new email verification code |

| `ResendPhoneCommand` | Request a new phone verification code |



\#### `RegisterCommandHandler` Responsibilities



1\. Normalize and validate the input, using value objects on the aggregate side or pre-checks in the handler for fast duplicate rejection.

2\. Hash the password using `IPasswordHasher`.

3\. Generate the email and phone OTP codes using `IVerificationCodeService`.

4\. Create the `Registration` aggregate.

5\. Set the email and phone verification codes on the aggregate.

6\. Write two integration events—`EmailVerificationRequestedIntegrationEvent` and `PhoneVerificationRequestedIntegrationEvent`—to the Outbox via `IOutboxMessageWriter`. Each carries the plaintext code.

7\. Save via `IUnitOfWork.SaveChangesAsync`.

8\. Return `Result<Guid>` with the registration ID.



\*\*Important design note:\*\* The register handler writes the two integration events directly, not via domain event handlers. This is the one place in the codebase where integration events are produced from a command handler rather than from a domain event handler. The reason is that the plaintext OTP codes are available only at generation time, while the Domain stores only their hashes.



\#### `VerifyEmailCommandHandler` and `VerifyPhoneCommandHandler` Responsibilities



1\. Load the registration by ID.

2\. If the channel is already verified, return success (idempotent).

3\. Verify the submitted code using `IVerificationCodeService`.

4\. Call `registration.VerifyEmail(isValid, utcNow)` or `registration.VerifyPhone(isValid, utcNow)`.

5\. Save via `IUnitOfWork.SaveChangesAsync`.

6\. Return `Result`.



\#### `ResendEmailCommandHandler` and `ResendPhoneCommandHandler` Responsibilities



1\. Load the registration by ID.

2\. Generate a new code via `IVerificationCodeService`.

3\. Call `registration.SetEmailVerificationCode(hashedCode, utcNow)` or the phone equivalent. The aggregate enforces resend limits and cooldown.

4\. Write a new integration event for delivery with the new plaintext code.

5\. Save via `IUnitOfWork.SaveChangesAsync`.

6\. Return `Result`.



\### 5.2 Domain Event Handler



`RegistrationFullyVerifiedDomainEventHandler` reacts to `RegistrationFullyVerifiedDomainEvent` by translating it into `RegistrationFullyVerifiedIntegrationEvent` and writing it to the Outbox via `IOutboxMessageWriter`. This is the only path that triggers provisioning.



The handler sets `CorrelationId` from `IAppContext` so the request's correlation ID flows into the asynchronous provisioning step.



\### 5.3 Integration Event Handlers



| Handler                                   | Triggered by                                 | Responsibility                                                                     |

| ----------------------------------------- | -------------------------------------------- | ---------------------------------------------------------------------------------- |

| `SendVerificationEmailHandler`            | `EmailVerificationRequestedIntegrationEvent` | Compose and send the email containing the OTP                                      |

| `SendVerificationSmsHandler`              | `PhoneVerificationRequestedIntegrationEvent` | Compose and send the SMS containing the OTP                                        |

| `CreateUserOnRegistrationVerifiedHandler` | `RegistrationFullyVerifiedIntegrationEvent`  | Provision the Identity user, assign the Customer role, and remove the registration |



\#### `CreateUserOnRegistrationVerifiedHandler` Flow



1\. Load the registration by ID. If it is `null`, return because it may already have been processed or removed.

2\. Create the Identity user with `IIdentityService.CreateUserWithPrehashedPasswordAsync`.

3\. Assign the Customer role via `IRoleManagementService.AssignRoleToUserAsync`.

4\. Remove the registration via `IRegistrationRepository.Remove`.

5\. Save via `IUnitOfWork.SaveChangesAsync`.

6\. Log success with the registration ID and user ID.



The handler is wrapped by the pipeline's idempotency and correlation decorators. It contains no idempotency or correlation logic of its own.



\*\*Atomicity:\*\* The identity service, role service, and registration repository share the same `AppDbContext`. All three operations are staged in the same context and committed by the single `SaveChangesAsync` call. User creation, role assignment, and registration removal therefore persist together or roll back together, assuming they participate in the same database transaction.



> \*\*Verification required:\*\* Confirm that `IIdentityService.CreateUserWithPrehashedPasswordAsync` throws on failure rather than returning `Guid.Empty`. If it can return an empty GUID, the handler must validate the return value before proceeding.



\### 5.4 Jobs



| Job                                    | Purpose                                                                                           |

| -------------------------------------- | ------------------------------------------------------------------------------------------------- |

| `ExpiredRegistrationCleanupJob`        | Recurring trigger that invokes the cleanup service                                                |

| `ExpiredRegistrationCleanupJobHandler` | Calls `IRegistrationCleanupService.DeleteInactiveExpiredAsync`                                    |

| `ExpiredRegistrationCleanupTrigger`    | Implements `IRecurringJobTrigger` and uses the `\[RecurringJob]` attribute to provide the schedule |



The job follows the Scheduling module pattern: payload (`IJob`), handler (`IJobHandler<T>`), and trigger (`IRecurringJobTrigger`). It does not register Hangfire directly.



\### 5.5 Models



`RegistrationVerificationEmail` is a typed model implementing `IEmailNotificationModel`. It carries the recipient email, customer name (if available), and plaintext OTP code. `TemplateName = "RegistrationVerification"`.



`RegistrationVerificationSms` is a typed model implementing `ISmsNotificationModel`. It carries the phone number and plaintext OTP code. `TemplateName = "RegistrationVerification"`.



Both models are consumed by the notification channels through direct channel injection, without using `INotificationDispatcher`.



> \*\*Verification required:\*\* Confirm the exact property sets and interface implementations against the actual files. The template name and model shape must match the Razor templates under `Infrastructure/Communication/Notifications/Templates/`.



\---



\## 6. Infrastructure Layer



\### 6.1 Persistence Configuration



\*\*File:\*\* `Infrastructure/Persistence/Modules/Onboarding/Configuration/RegistrationConfiguration.cs`



Inherits `BaseEntityConfiguration<Registration>` to reuse audit and soft-delete conventions.



\*\*Mappings:\*\*



\* `Email`, `PhoneNumber`, `Username`, and `PasswordHash` are owned value objects with explicit column names.

\* `EmailVerification` and `PhoneVerification` are \*\*JSON columns\*\*, with each value stored as a serialized string. This preserves the immutability of the value object and avoids an extra table.

\* A shadow `RowVersion` property provides optimistic concurrency.



\*\*Unique indexes:\*\*



\* Plain unique index on the `Email.Value` column.

\* Plain unique index on the `PhoneNumber.Value` column.

\* Plain unique index on the `Username.Value` column.



The indexes are plain, not filtered, because `IsActive` was removed from the aggregate. Expired-but-not-yet-cleaned registrations block new registrations until the cleanup job removes them.



> \*\*Verification required:\*\* Confirm the exact column names and JSON column configuration against `RegistrationConfiguration.cs`.



\### 6.2 Repository



\*\*File:\*\* `Infrastructure/Persistence/Modules/Onboarding/Repository/RegistrationRepository.cs`



Inherits `Repository<Registration>` and implements `IRegistrationRepository`. It adds duplicate-prevention queries such as `GetByEmailAsync`, `ExistsByEmailAsync`, and related methods.



\### 6.3 Verification Code Service



\*\*File:\*\* `Infrastructure/Security/Verification/VerificationCodeService.cs`

\*\*Implements:\*\* `IVerificationCodeService` (Application abstraction)



\*\*Responsibilities:\*\*



\* `GenerateCode(out string plainCode)` returns a six-digit numeric code and its hash. The hash is computed using HMAC-SHA256 with a secret key from `VerificationOptions`.

\* `VerifyCode(submittedCode, storedHash)` recomputes the HMAC of the submitted code and compares it with the stored hash using `CryptographicOperations.FixedTimeEquals` for constant-time comparison.



\*\*Configuration:\*\* `VerificationOptions` holds the HMAC secret. It is bound from configuration and validated at startup. The secret must be at least 256 bits.



\*\*Security properties:\*\*



\* Codes are never stored in plaintext in the database.

\* Comparison is constant-time, reducing timing-side-channel risk.

\* The secret is held only in Infrastructure; the Domain never sees it.



\### 6.4 Cleanup Service



\*\*File:\*\* `Infrastructure/Persistence/Modules/Onboarding/Services/RegistrationCleanupService.cs`

\*\*Implements:\*\* `IRegistrationCleanupService` (Application abstraction)



`DeleteInactiveExpiredAsync(utcNow)` uses `ExecuteDeleteAsync` to bulk-delete registrations whose `ExpiresAtUtc < utcNow`. The operation is a single database round-trip, not a load-then-delete loop.



\### 6.5 Jobs



The cleanup job is registered through the standard Scheduling module pattern. The trigger carries a `\[RecurringJob("expired-registration-cleanup", "<cron>")]` attribute.



The default cron expression is daily at 03:00 UTC. However, the deployment \*\*must\*\* override it to every 15–30 minutes in production because the registration lifetime is only 15 minutes.



> \*\*Verification required:\*\* Check the actual cron expression in `ExpiredRegistrationCleanupTrigger.cs` and whether the deployment override is documented in `appsettings.Production.json`.



\---



\## 7. API Layer



\### 7.1 Controller



\*\*File:\*\* `API/Controllers/v1/Security/AuthenticationController.cs`



The controller exposes registration endpoints alongside login and external-authentication endpoints. Alternatively, a dedicated `RegistrationController` may exist; verify against the current tree.



\*\*Typical endpoint structure:\*\*



| Method | Route                    | Purpose                  |

| ------ | ------------------------ | ------------------------ |

| `POST` | `/api/auth/register`     | Create a registration    |

| `POST` | `/api/auth/verify-email` | Verify email with a code |

| `POST` | `/api/auth/verify-phone` | Verify phone with a code |

| `POST` | `/api/auth/resend-email` | Request a new email code |

| `POST` | `/api/auth/resend-phone` | Request a new phone code |



\*\*Authorization:\*\* All registration endpoints are anonymous because they are consumed by unauthenticated users.



\*\*Rate limiting:\*\* The endpoints are subject to the API-layer rate-limiting policy. Registration endpoints should fall under the strictest policy available.



\*\*Error shape:\*\* All failure responses use `ProblemDetails`, consistent with the rest of the API layer.



\### 7.2 DTOs



\*\*Request DTOs\*\* carry the user's input:



\* `RegisterRequest` — email, phone, username, and password.

\* `VerifyEmailRequest` — registration ID and code.

\* `VerifyPhoneRequest` — registration ID and code.

\* `ResendEmailRequest` — registration ID.

\* `ResendPhoneRequest` — registration ID.



\*\*Response DTOs\*\* are minimal:



\* `RegistrationResponse` — the registration ID, returned on successful registration.



The verification and resend endpoints return `204 No Content` on success.



> \*\*Verification required:\*\* Confirm the actual DTO names and fields against the API tree.



\---



\## 8. Configuration



\### 8.1 Verification Options



```json

{

&#x20; "Verification": {

&#x20;   "SecretKey": "...",

&#x20;   "CodeLength": 6

&#x20; }

}

```



`SecretKey` is the HMAC secret and must contain at least 256 bits of key material.



The configuration is bound to `VerificationOptions` and validated at startup. In production, the secret \*\*must\*\* be supplied through environment variables or a secret manager—never committed to source control.



\### 8.2 Cleanup Job Schedule



The `\[RecurringJob]` attribute on `ExpiredRegistrationCleanupTrigger` declares the default schedule. The production deployment \*\*must\*\* override it to run every 15–30 minutes.



> \*\*Verification required:\*\* Confirm whether the schedule is configurable per environment. If it is hardcoded in the attribute, adjusting it requires a code change, which is a limitation worth documenting.



\### 8.3 Outbox Integration



The feature relies on the Outbox configuration (`Outbox:MaxRetryCount`) for delivery retries. No additional configuration is required for the registration feature itself.



\---



\## 9. Database Schema



\### 9.1 Registrations



| Column                                             | Type               | Notes                                   |

| -------------------------------------------------- | ------------------ | --------------------------------------- |

| `Id`                                               | `uniqueidentifier` | Primary key                             |

| `Email\_Value`                                      | `nvarchar`         | Owned value object column; unique index |

| `PhoneNumber\_Value`                                | `nvarchar`         | Owned value object column; unique index |

| `Username\_Value`                                   | `nvarchar`         | Owned value object column; unique index |

| `PasswordHash\_Value`                               | `nvarchar`         | Owned value object column               |

| `EmailVerification`                                | `nvarchar(max)`    | JSON-serialized `VerificationChannel`   |

| `PhoneVerification`                                | `nvarchar(max)`    | JSON-serialized `VerificationChannel`   |

| `ExpiresAtUtc`                                     | `datetime2`        | Registration lifetime                   |

| `RegistrationCompleted`                            | `bit`              | Completion flag                         |

| `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` | —                  | Audit fields from `BaseEntity`          |

| `RowVersion`                                       | `rowversion`       | Optimistic concurrency                  |



\*\*Indexes:\*\*



\* Unique index on `Email\_Value`.

\* Unique index on `PhoneNumber\_Value`.

\* Unique index on `Username\_Value`.



The exact column names depend on how the owned-type configurations name their properties.



> \*\*Verification required:\*\* Confirm the column names against `RegistrationConfiguration.cs`.



\### 9.2 No Soft Delete



The `Registrations` table participates in the soft-delete interceptor from `BaseEntity`, but the cleanup job performs a \*\*physical delete\*\* through `ExecuteDeleteAsync`. The interceptor's `IsDeleted` flag is not the cleanup mechanism.



> \*\*Verification required:\*\* Check whether the interceptor's global soft-delete query filter affects `Registration` reads. If `Registration` inherits `BaseEntity` and the filter applies, a soft-deleted registration would be hidden from queries. The cleanup path bypasses the filter by design.



\---



\## 10. Cross-Cutting Concerns



\### 10.1 Idempotency



| Operation             | Mechanism                                                                                                                      |

| --------------------- | ------------------------------------------------------------------------------------------------------------------------------ |

| Registration creation | Unique database constraints on email, phone, and username                                                                      |

| Verification          | Handler pre-checks `IsVerified` and returns success if already verified                                                        |

| Resend                | Aggregate enforces limits; cooldown prevents rapid duplicate requests                                                          |

| Provisioning          | Pipeline's `IdempotentIntegrationEventHandler<T>` decorator; composite key `(EventId, HandlerIdentifier)` in `ProcessedEvents` |



\### 10.2 Correlation



The correlation ID from the HTTP request propagates through the workflow:



1\. `CorrelationIdMiddleware` adds it to the log scope and `HttpContext.Items`.

2\. The register handler adds it to `EmailVerificationRequestedIntegrationEvent.CorrelationId` and `PhoneVerificationRequestedIntegrationEvent.CorrelationId`.

3\. A later verification request supplies the correlation ID for `RegistrationFullyVerifiedIntegrationEvent.CorrelationId`.

4\. `CorrelationScopeIntegrationEventHandler<T>` restores it into the log scope for the provisioning handler.



\### 10.3 Logging



Key log entries include:



\* Registration created (informational; no OTP or password).

\* Verification code delivered (informational, from the notification pipeline; the OTP is not logged).

\* Verification succeeded or failed (informational).

\* Registration expired (informational, from the aggregate's expiry path).

\* Account provisioned (informational, from the provisioning handler).

\* Cleanup job completed (informational, including the count of removed rows).



No plaintext OTP or password appears in any log entry.



\### 10.4 Persistence Transactions



\* \*\*Registration creation:\*\* One transaction for the aggregate and two Outbox rows.

\* \*\*Verification:\*\* One transaction for the aggregate update.

\* \*\*Resend:\*\* One transaction for the aggregate update and Outbox row.

\* \*\*Provisioning:\*\* One transaction for user creation, role assignment, and registration removal.



Each `SaveChangesAsync` call on `IUnitOfWork` is the single commit point for its operation.



\---



\## 11. Testing Guidance



\### 11.1 Domain Tests



\* `Registration.Create` validates input and sets the expected state.

\* `SetEmailVerificationCode` respects resend limits and cooldown.

\* `VerifyEmail` fails when the code has expired.

\* `VerifyEmail` increments the attempt counter when the code is incorrect.

\* Attempt exhaustion expires the registration and raises `RegistrationExpiredDomainEvent`.

\* `VerifyEmail` triggers `RegistrationFullyVerifiedDomainEvent` when called after `VerifyPhone` has already succeeded.

\* `AssertNotExpired` and `AssertNotCompleted` block invalid operations.



\### 11.2 Application Tests



\* `RegisterCommandHandler` produces two integration events, one for each channel.

\* `RegisterCommandHandler` hashes the password before passing it to the aggregate.

\* `VerifyEmailCommandHandler` returns success idempotently when the channel is already verified.

\* `RegistrationFullyVerifiedDomainEventHandler` writes a `RegistrationFullyVerifiedIntegrationEvent` to the Outbox with the correlation ID.

\* `CreateUserOnRegistrationVerifiedHandler` provisions the user, assigns the role, and removes the registration.

\* `CreateUserOnRegistrationVerifiedHandler` returns without error when the registration is not found.



\### 11.3 Integration Tests



\* Unique constraints reject duplicate emails, phones, and usernames.

\* The registration transaction persists the aggregate and both Outbox rows atomically.

\* Verification code delivery reaches the notification pipeline.

\* Provisioning is idempotent when the same event is delivered twice.

\* The cleanup job removes only expired registrations.



\### 11.4 API Tests



\* `POST /api/auth/register` returns the registration ID.

\* Registering with a duplicate email returns a conflict (`400` or `409`).

\* Verification with a correct code returns `204`.

\* Verification with an incorrect code returns `400`.

\* Resending within the cooldown returns `400`.

\* Registration endpoints are anonymous.



\---



\## 12. Known Limitations



| Limitation                                         | Impact                                                                                   | Notes                                                                                |

| -------------------------------------------------- | ---------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------ |

| Plain OTP in Outbox payloads                       | Codes persist in the Outbox and dead-letter tables until a retention policy removes them | Documented in the tradeoffs reference                                                |

| No client-observable provisioning signal           | After verification, the client must poll login until it succeeds; no SignalR hint exists | The provisioning handler logs success but does not publish an event                  |

| Daily default cleanup schedule                     | Expired registrations can block re-registration for up to 24 hours by default            | Deployment must override the schedule                                                |

| No filtered indexes                                | Expired-but-not-cleaned registrations block duplicate registration attempts              | Accepted as a deliberate simplification                                              |

| Verification code entropy is bounded by six digits | The code space contains approximately one million possible values                        | The five-attempt limit makes brute-force attempts infeasible under the stated limits |

| No OTP delivery confirmation                       | The system knows the code was dispatched, not that the user received it                  | The user can request a resend                                                        |



\---



\## 13. Summary



The Registration \& Account Verification feature spans all four layers:



\* \*\*Domain:\*\* The `Registration` aggregate with encapsulated state, immutable value objects, centralized constants, and seven domain events.

\* \*\*Application:\*\* Commands for creation, verification, and resend; a domain event handler for full verification; and three integration event handlers for email, SMS, and provisioning.

\* \*\*Infrastructure:\*\* EF configuration with JSON-serialized `VerificationChannel` and plain unique indexes; a verification code service using HMAC-SHA256 with constant-time comparison; a bulk-delete cleanup service; and a recurring cleanup job.

\* \*\*API:\*\* Thin anonymous endpoints under `/api/auth`.



The feature relies on the Outbox pipeline for durable code delivery and provisioning, the Notifications subsystem for email and SMS transport, and ASP.NET Core Identity for user creation. It is idempotent at every step, propagates correlation across the asynchronous boundary, and enforces its business invariants within the aggregate.



\---



\*\*Document version:\*\* 1.0 — Final Registration \& Account Verification Technical Reference.



This document describes the folder structure, domain model, application commands and handlers, infrastructure persistence and services, API surface, configuration, schema, and testing guidance. Unverified specifics—including method names, column names, cron expression, and DTO paths—are marked for verification against the current code. The implementation aligns with the Registration Technical Requirements and the Registration Domain Model Technical Reference.



