# Identity & User Management — Technical Reference Document

**Project:** E-Commerce Modular Monolith
**Scope:** Identity Lifecycle, Account State, Credentials
**Version:** 1.5 — Final

---

## 1. Overview

The Identity & User Management module manages the lifecycle of application accounts, account state, and credentials within the E-Commerce modular monolith.

It is an **Application-layer module backed by ASP.NET Core Identity in Infrastructure**. It is not a Domain bounded context and does not introduce identity-related aggregates into the Domain layer.

The module provides:

* Account activation and deactivation.
* Administrative account locking and unlocking.
* Account information updates.
* Password changes and password resets.
* Account and security information queries.
* Identity provisioning support for the Onboarding module.
* Durable password-reset email delivery through the Outbox.

### 1.1 Architectural Position

The module follows the existing layered architecture:

* **API:** Exposes endpoints and applies authentication and authorization.
* **Application:** Defines use cases, contracts, handlers, and orchestration.
* **Infrastructure:** Implements account and credential operations using ASP.NET Core Identity and EF Core.
* **Domain:** Remains unchanged by this module.

ASP.NET Core Identity entities, `UserManager<User>`, Identity stores, and persistence-specific implementations must remain in Infrastructure.

Application handlers depend on abstractions rather than directly referencing ASP.NET Core Identity.

### 1.2 Scope Boundaries

This module owns application-account management and credential operations.

The following concerns remain outside its scope:

| Concern                                           | Responsible Module                                |
| ------------------------------------------------- | ------------------------------------------------- |
| Registration workflow and verification            | Onboarding                                        |
| Account provisioning                              | Identity & User Management, invoked by Onboarding |
| Human/customer information                        | People                                            |
| External authentication providers, such as Google | Authentication                                    |
| Authorization policies and permissions            | Authorization                                     |
| Email delivery                                    | Notifications                                     |
| Durable asynchronous delivery                     | Outbox and Scheduling                             |

External authentication is a separate capability. ASP.NET Core Identity can persist external-login associations through its built-in Identity tables, but provider sign-in orchestration belongs to the Authentication module.

---

## 2. Project Structure

### 2.1 Application

```text
Application/
└── Modules/
    └── Identity/
        ├── AccountManagement/
        │   ├── Abstractions/
        │   │   ├── IAccountManagement.cs
        │   │   └── IAccountReader.cs
        │   ├── Commands/
        │   ├── Queries/
        │   ├── Models/
        │   └── Handlers/
        │
        ├── Credentials/
        │   ├── Abstractions/
        │   │   └── ICredentialManagement.cs
        │   ├── Commands/
        │   ├── Handlers/
        │   └── Models/
        │
        └── Shared/
            └── Exceptions/
                └── IdentityOperationException.cs
```

The exact placement of shared contracts and exceptions should follow the solution's established conventions. The structure above illustrates the module's logical separation.

### 2.2 Infrastructure

```text
Infrastructure/
└── Security/
    └── Identity/
        ├── Entities/
        │   └── User.cs
        ├── Services/
        │   ├── AccountManagementService.cs
        │   ├── CredentialManagementService.cs
        │   ├── AccountReader.cs
        │   └── IdentityService.cs
        └── Configuration/
```

The Identity infrastructure implementation uses ASP.NET Core Identity and the application's configured EF Core persistence.

No custom `UserLogin` or `UserCredential` entities are introduced by this module. External-login associations use ASP.NET Core Identity's built-in persistence where applicable.

---

## 3. Identity User Model

### 3.1 Custom User Entity

The application extends `IdentityUser<Guid>` with application-specific account metadata.

```csharp
public class User : IdentityUser<Guid>
{
    public DateTime CreatedAtUtc { get; set; }

    public DateTime? PasswordLastChangedAtUtc { get; set; }

    public AccountStatus AccountStatus { get; set; }
}
```

This entity belongs to Infrastructure because it extends an ASP.NET Core Identity type.

It must not be moved into the Domain layer.

### 3.2 Account Status

```csharp
public enum AccountStatus
{
    Active = 1,
    Deactivated = 2
}
```

`AccountStatus` represents the application's administrative account state.

It is independent of ASP.NET Core Identity's lockout mechanism.

| Mechanism              | Purpose                                                              |
| ---------------------- | -------------------------------------------------------------------- |
| `AccountStatus`        | Determines whether the application account is active or deactivated. |
| `LockoutEnd`           | Represents the end of an Identity lockout period.                    |
| `AccessFailedCount`    | Tracks failed access attempts according to Identity configuration.   |
| `EmailConfirmed`       | Indicates whether the email address has been confirmed.              |
| `PhoneNumberConfirmed` | Indicates whether the phone number has been confirmed.               |

A deactivated account and a locked account are not equivalent. The account can be active but locked, or deactivated without being locked.

### 3.3 Lockout Semantics

The account-management service supports the following operations:

* **Lock:** Apply an administrative lockout.
* **Unlock:** Clear the lockout and reset the failed-access counter when appropriate.

If the application represents an indefinite administrative lock using `DateTimeOffset.MaxValue`, that convention must be documented and consistently enforced.

Identity lockout must be enabled and configured correctly. The application must also check the result of Identity operations rather than assuming they succeeded.

### 3.4 Authentication Enforcement

ASP.NET Core Identity does not automatically enforce the custom `AccountStatus` property.

The authentication pipeline must explicitly prevent deactivated accounts from authenticating or accessing protected application operations.

The implementation must account for the authentication mechanisms used by the application, including cookie authentication or bearer-token authentication.

For long-lived tokens, the application must also decide how account deactivation, lockout, and security changes affect existing sessions and tokens.

---

## 4. Application Contracts

Application use cases depend on contracts. Infrastructure supplies their implementations.

### 4.1 Account Management

**Contract:** `IAccountManagement`

Responsibilities:

* Activate an account.
* Deactivate an account.
* Lock an account.
* Unlock an account.
* Update account information.

Illustrative contract:

```csharp
public interface IAccountManagement
{
    Task ActivateAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task LockAsync(
        Guid userId,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task UnlockAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        UpdateAccountRequest request,
        CancellationToken cancellationToken = default);
}
```

The exact signatures should match the application's established Result Pattern and cancellation-token conventions.

### 4.2 Account Reader

**Contract:** `IAccountReader`

**Location:**

```text
Application/Modules/Identity/
└── AccountManagement/
    └── Abstractions/
        └── IAccountReader.cs
```

Responsibilities:

* Retrieve an account by identifier.
* Retrieve an account by email.
* Retrieve account security information.

The reader should return application-facing models rather than exposing Infrastructure's `User` entity to handlers.

Email lookups must use the configured Identity normalization and lookup behavior.

### 4.3 Credential Management

**Contract:** `ICredentialManagement`

Responsibilities:

* Change a password for an authenticated user.
* Generate a password-reset token.
* Reset a password using a valid reset token.

Illustrative contract:

```csharp
public interface ICredentialManagement
{
    Task ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<string> GeneratePasswordResetTokenAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(
        Guid userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);
}
```

Password-reset tokens are security-sensitive values. They must never be written to application logs.

### 4.4 Shared Identity Service

**Contract:** `IIdentityService`

This contract is consumed by Onboarding when it needs to check account existence or provision an Identity user.

The dependency direction is intentional:

* Onboarding determines **when** an Identity account should be created.
* Identity infrastructure determines **how** that account is created and persisted.
* Identity & User Management does not consume its own `IIdentityService` contract.

The service is limited to the capabilities required by Onboarding, such as account-existence checks and account provisioning.

It must not become a general-purpose interface exposing every ASP.NET Core Identity operation.

### 4.5 Current User

**Contract:** `ICurrentUser`

This shared abstraction exposes the authenticated user's identity to Application use cases.

For self-service operations, the target user identifier must be derived from the authenticated context rather than trusted from a client-supplied identifier.

---

## 5. Infrastructure Services

### 5.1 AccountManagementService

`AccountManagementService` implements `IAccountManagement` and wraps the relevant ASP.NET Core Identity operations.

Responsibilities include:

* Changing `AccountStatus`.
* Applying and clearing administrative lockouts.
* Updating email, phone number, and username.
* Resetting confirmation flags when contact information changes.
* Coordinating persistence where an operation requires multiple database changes.

The service uses `UserManager<User>` and the configured Identity store.

#### Account Updates

Identity operations such as `SetEmailAsync`, `SetPhoneNumberAsync`, and `SetUserNameAsync` may persist changes through the Identity store.

If an update consists of multiple operations that must be atomic, the implementation must ensure that:

1. The Identity store and application persistence use the intended scoped `DbContext`.
2. All relevant writes participate in the same database transaction.
3. Every Identity operation result is checked.
4. Any failure that invalidates the overall operation causes the transaction to roll back.

A transaction does not automatically make multiple Identity operations atomic unless they actually participate in it.

#### Contact Information Changes

When an email address or phone number changes, the corresponding confirmation state must be reset.

* Email changes must reset `EmailConfirmed`.
* Phone-number changes must reset `PhoneNumberConfirmed`.

The user must complete the appropriate verification workflow before the changed contact information is treated as verified.

The re-verification workflow is outside this module's current scope.

### 5.2 CredentialManagementService

`CredentialManagementService` implements `ICredentialManagement`.

It wraps the appropriate `UserManager<User>` operations for:

* Changing passwords.
* Generating password-reset tokens.
* Resetting passwords.
* Updating `PasswordLastChangedAtUtc` when a password change or reset succeeds.

Password changes must use ASP.NET Core Identity's password-management APIs rather than directly modifying password hashes.

Where updating the password and its audit metadata must be atomic, the implementation must ensure that both changes participate in the same transaction.

The service must check Identity results and preserve useful validation errors without exposing sensitive internal details.

### 5.3 AccountReader

`AccountReader` implements `IAccountReader`.

It retrieves account data through the configured Identity persistence mechanisms and returns application-facing read models.

It must not expose password hashes, reset tokens, security stamps, or other sensitive Identity data through general account queries.

### 5.4 IdentityService

`IdentityService` implements `IIdentityService`.

It provides the Identity operations required by Onboarding, including account-existence checks and account provisioning.

During provisioning, the service must apply the intended initial account state and confirmation flags.

The exact initial values must align with the Onboarding workflow. In this application, email and phone verification are mandatory before the Identity account is provisioned, so the provisioning flow must not mark either contact as confirmed before its required verification succeeds.

---

## 6. Use Cases

### 6.1 Account Management Commands

| Use Case           | Description                                                     | Authorization              |
| ------------------ | --------------------------------------------------------------- | -------------------------- |
| Activate Account   | Reactivates a deactivated account.                              | `Accounts.Activate`        |
| Deactivate Account | Deactivates an account.                                         | `Accounts.Deactivate`      |
| Lock Account       | Applies an administrative lockout.                              | `Accounts.Lock`            |
| Unlock Account     | Clears an administrative lockout.                               | `Accounts.Unlock`          |
| Update Account     | Updates the authenticated user's permitted account information. | Self-service authorization |

Administrative operations require the appropriate permissions.

Self-service operations must derive the target account from `ICurrentUser` and must not permit a caller to update another user's account by supplying an arbitrary identifier.

### 6.2 Account Queries

| Use Case             | Description                                                          | Authorization              |
| -------------------- | -------------------------------------------------------------------- | -------------------------- |
| Get Current Account  | Retrieves the authenticated user's account information.              | Authenticated user         |
| Get Account by ID    | Retrieves an account by identifier.                                  | `Accounts.Read`            |
| Get Account Security | Retrieves permitted security information for the authenticated user. | Self-service authorization |

Read models must expose only the information required by the caller and must not return credential secrets.

### 6.3 Credential Commands

| Use Case               | Description                                                                      | Authorization                    |
| ---------------------- | -------------------------------------------------------------------------------- | -------------------------------- |
| Change Password        | Changes the authenticated user's password after validating the current password. | Authenticated user               |
| Request Password Reset | Initiates a password-reset workflow.                                             | Anonymous                        |
| Reset Password         | Resets a password using a valid reset token.                                     | Anonymous, with token validation |

Password-reset endpoints must not disclose whether an email address belongs to an existing account.

The reset operation must validate the token against the intended user and use ASP.NET Core Identity's reset-password mechanism.

---

## 7. Password Reset and the Outbox

### 7.1 Overview

Password-reset email delivery is an asynchronous side effect.

The request flow must avoid sending an email directly from the command handler. Instead, the application records a durable Outbox message and allows the existing Outbox processing pipeline to deliver the email.

The process is:

1. Receive the password-reset request.
2. Apply rate limiting and other abuse protections.
3. Resolve the account without revealing its existence to the caller.
4. Generate a password-reset token when an eligible account exists.
5. Stage the appropriate Outbox message.
6. Commit the Outbox message durably.
7. Let the Outbox pipeline dispatch the integration event.
8. Let the Notifications module deliver the email.
9. Include the reset link in the email using a trusted frontend origin.

The public response must remain generic whether or not an eligible account exists.

### 7.2 Integration Event

The password-reset integration event contains the information required by the email-delivery workflow.

Illustrative contract:

```csharp
public sealed record PasswordResetRequestedIntegrationEvent(
    Guid EventId,
    DateTime OccurredAtUtc,
    string CorrelationId,
    Guid UserId,
    string Email,
    string ResetToken);
```

The exact contract should follow the solution's integration-event conventions.

**Security warning:** `ResetToken` is a sensitive bearer secret. Anyone who obtains a valid token may be able to reset the associated user's password.

The event must therefore be treated as sensitive data throughout its lifecycle.

Controls should include:

* Never logging the token.
* Never including it in exception messages or diagnostic scopes.
* Restricting access to Outbox tables and operational tooling.
* Protecting database backups and any exported Outbox payloads.
* Using an HTTPS reset link and a trusted, configured frontend origin.
* URL-encoding token values correctly.
* Applying an explicit retention and cleanup policy.
* Avoiding unnecessary copies of the token in logs, traces, analytics, and monitoring systems.

Because the token must be available to the email-delivery process, storing it in the Outbox requires a deliberate security decision. Consider protecting sensitive payloads at rest and minimizing the time they remain stored.

### 7.3 Channel 5 — Direct Outbox Write

This module uses a deliberate exception to the normal domain-event-to-integration-event translation pipeline.

The password-reset request does not require an Identity domain aggregate or a domain event. Identity is an Application/Infrastructure concern in this architecture.

Therefore, the command handler may use `IOutboxMessageWriter` to stage the integration-event message directly.

The required flow is:

```text
Password Reset Request
        |
        v
Credential / Password-Reset Handler
        |
        v
Generate Identity Reset Token
        |
        v
IOutboxMessageWriter
        |
        v
Stage Outbox Message
        |
        v
Commit Database Transaction
        |
        v
Outbox Processing Pipeline
        |
        v
PasswordResetRequestedIntegrationEvent
        |
        v
Notifications Module
        |
        v
Password-Reset Email
```

This is an intentional application-level integration-event publication path, not a general permission for command handlers to bypass the Outbox abstraction.

### 7.4 Atomicity Requirements

The Outbox message must be durably committed before the system reports successful acceptance of the reset request.

`IOutboxMessageWriter` must stage the message using the same persistence context and transaction used by the relevant Unit of Work.

The implementation must verify that:

* The Outbox message is persisted before the operation is considered accepted.
* The Unit of Work commits the staged message.
* The request does not report successful durable acceptance if the commit fails.
* The Outbox processor can safely retry delivery.
* Duplicate delivery does not create unsafe downstream behavior.

Generating a reset token and staging an Outbox message do not, by themselves, guarantee atomicity. Atomicity depends on the actual database transaction and persistence configuration.

### 7.5 Delivery Semantics

The Outbox pipeline provides durable, **at-least-once delivery**.

It does not guarantee exactly-once email delivery. A failure after an email provider accepts a message but before the dispatch process records success may result in another delivery attempt.

The Notifications module and Outbox dispatcher must handle retries and failures without treating duplicate delivery as impossible.

---

## 8. Execution Pipelines

### 8.1 Account Update

```text
API Request
    |
    v
Authentication and Authorization
    |
    v
Application Handler
    |
    v
Resolve Current User
    |
    v
Validate Request
    |
    v
IAccountManagement
    |
    v
AccountManagementService
    |
    v
ASP.NET Core Identity
    |
    v
EF Core / SQL Server
    |
    v
Result
```

The Application handler coordinates the use case. Infrastructure implements the Identity-specific operations.

### 8.2 Password Change

```text
API Request
    |
    v
Authentication and Authorization
    |
    v
ChangePassword Handler
    |
    v
Resolve Current User
    |
    v
ICredentialManagement
    |
    v
CredentialManagementService
    |
    v
UserManager.ChangePasswordAsync
    |
    v
Update PasswordLastChangedAtUtc
    |
    v
Persist Changes
    |
    v
Result
```

The implementation must check the Identity result and update the password-change timestamp only when the password change succeeds.

### 8.3 Password Reset

```text
Anonymous Reset Request
    |
    v
Rate Limiting and Request Validation
    |
    v
Password Reset Handler
    |
    v
Resolve Eligible Account
    |
    v
Generate Reset Token
    |
    v
Stage Outbox Message
    |
    v
Commit Outbox Transaction
    |
    v
Return Generic Public Response
    |
    v
Outbox Processing
    |
    v
Notifications Module
    |
    v
Email Delivery
```

The password-reset completion flow is separate:

```text
Reset Link
    |
    v
Extract User Identifier and Token
    |
    v
Submit New Password
    |
    v
ResetPassword Handler
    |
    v
ICredentialManagement
    |
    v
UserManager.ResetPasswordAsync
    |
    v
Update PasswordLastChangedAtUtc
    |
    v
Persist Changes
```

The reset token must be validated by ASP.NET Core Identity for the specified user. The application must not implement a substitute token-validation mechanism that bypasses Identity's configured token providers.

---

## 9. Security Requirements

### 9.1 Authorization

* Administrative account operations require their corresponding permissions.
* Self-service operations must target the authenticated user's account.
* Account and security queries must apply appropriate authorization.
* Anonymous password-reset endpoints must not reveal account existence.
* Authorization failures must be handled before the protected operation executes.

### 9.2 Account State Enforcement

* Deactivated accounts must be denied access according to the application's authentication policy.
* Identity lockout must be enforced independently of `AccountStatus`.
* The application must define how account-state changes affect existing sessions and tokens.
* Reactivation must not silently bypass a still-active lockout unless the use case explicitly intends that behavior.

### 9.3 Credential Protection

* Passwords must be processed through ASP.NET Core Identity.
* Password hashes must never be exposed through application APIs.
* Reset tokens must never be logged.
* Reset links must use a trusted frontend origin and HTTPS.
* Reset requests must be rate-limited.
* Password-reset responses must not reveal whether an account exists.
* Reset-token payloads must be protected while stored and processed.

### 9.4 Contact Verification

* Email and phone verification are mandatory during onboarding.
* Identity provisioning must not occur before the required verification steps succeed.
* Changes to email or phone number must invalidate the relevant confirmation state.
* Updated contact information must not be treated as verified until the corresponding verification workflow succeeds.

### 9.5 Input and Error Handling

* Validate incoming account-update requests.
* Apply Identity password and username validation rules.
* Avoid returning internal exception details to clients.
* Do not expose sensitive Identity persistence details in DTOs.
* Use rate limiting and appropriate abuse controls for anonymous credential endpoints.

---

## 10. Error Handling

Identity operation failures should be translated into application-level outcomes.

A useful distinction is:

| Failure Category                  | Expected Handling                                                                                      |
| --------------------------------- | ------------------------------------------------------------------------------------------------------ |
| Invalid user input                | Validation failure                                                                                     |
| Password policy violation         | Validation failure                                                                                     |
| Invalid or expired reset token    | Safe credential-operation failure                                                                      |
| Account not found                 | Appropriate not-found behavior for authorized operations; generic response for password-reset requests |
| Permission denied                 | Authorization failure                                                                                  |
| Optimistic concurrency conflict   | Conflict response or application-level concurrency failure                                             |
| Database or persistence failure   | Server-side failure handling                                                                           |
| Unexpected infrastructure failure | Global exception handling and appropriate logging                                                      |

An `IdentityOperationException` may carry Identity-specific failure information from Infrastructure. Application handlers should translate it into the application's Result Pattern or other established outcome model.

Do not map every Identity exception to HTTP 400. Infrastructure failures, concurrency conflicts, authorization failures, and validation failures require different handling.

Internal details and secrets must not be included in public error responses.

---

## 11. Persistence and Transaction Boundaries

The module uses the existing application persistence infrastructure.

The implementation must respect these requirements:

1. Identity persistence remains in Infrastructure.
2. Identity stores use the configured database and scoped persistence context.
3. Multi-step operations that require atomicity use an appropriate transaction.
4. Identity operation results are checked before the operation is considered successful.
5. Failed multi-step operations roll back when the contract requires atomicity.
6. Outbox messages are committed atomically with the Unit of Work that stages them.
7. Password-reset token payloads receive appropriate storage protection and retention.
8. Concurrency conflicts are handled deliberately rather than silently overwritten.

A database transaction does not automatically encompass external email delivery or other external side effects. Those are handled through the Outbox and Notifications pipeline.

---

## 12. Testing Strategy

Testing should verify behavior, persistence, security boundaries, and failure handling.

### 12.1 Unit Tests

Cover:

* Account activation and deactivation behavior.
* Administrative lock and unlock rules.
* Account-update validation.
* Self-service account targeting.
* Password-change and reset result handling.
* Identity error translation.
* Generic password-reset responses.
* Correct handling of failed operations.

When mocking `UserManager<User>`, configure the underlying dependencies and Identity results carefully. A mock should not be treated as proof that the real Identity store behaves correctly.

### 12.2 Integration Tests

Use the actual configured Identity persistence implementation where practical.

Cover:

* Account creation and lookup.
* Email and phone confirmation state.
* Account updates and normalization.
* Lockout persistence.
* Password changes and password-reset token validation.
* Transaction rollback after partial failure.
* `PasswordLastChangedAtUtc` updates.
* Outbox message persistence.
* Atomic persistence of Outbox messages within the intended Unit of Work.
* Failure to commit the Outbox message.
* Protection against logging sensitive reset tokens.

### 12.3 Security Tests

Verify that:

* Deactivated accounts cannot authenticate or access protected operations according to policy.
* Lockout is enforced independently of account deactivation.
* Users cannot update other users' accounts through self-service requests.
* Unauthorized administrative operations are rejected.
* Reset requests do not reveal whether an account exists.
* Invalid and expired reset tokens are rejected.
* Reset tokens and password hashes are not exposed in API responses or logs.
* Rate limiting is applied to anonymous credential endpoints.

---

## 13. Architectural Invariants

The following rules are mandatory:

1. **Identity remains an Application/Infrastructure concern.** It does not introduce an Identity domain aggregate.
2. **ASP.NET Core Identity remains in Infrastructure.** Application handlers must not depend directly on `UserManager<User>`.
3. **Onboarding owns registration orchestration.** Identity provides account-existence and provisioning capabilities.
4. **Authentication is separate from account management.** External provider sign-in belongs to the Authentication module.
5. **Account status and Identity lockout remain independent.** Both must be enforced according to policy.
6. **Self-service operations use the authenticated identity.** Client-supplied identifiers must not override the current user's identity.
7. **Contact changes reset confirmation state.** Changed contact information must be re-verified.
8. **Password operations use ASP.NET Core Identity.** Password hashes are never manipulated directly by application handlers.
9. **Password-reset requests use the Outbox.** Email delivery is asynchronous and durable.
10. **Outbox persistence must be atomic.** The message must be committed before durable acceptance is reported.
11. **Reset tokens are sensitive bearer secrets.** They must not be logged and must receive appropriate storage protection.
12. **Outbox delivery is at least once.** Duplicate delivery remains possible and must be handled safely.
13. **Errors are mapped by category.** Validation, authorization, concurrency, and infrastructure failures must not be indiscriminately converted into client errors.
14. **Secrets are never hardcoded.** Configuration and secret material must use appropriate environment-based or managed secret storage.

---

## 14. Summary

The Identity & User Management module centralizes account lifecycle operations and credential management while preserving the modular monolith's architectural boundaries.

Application defines the use cases and abstractions. Infrastructure implements them using ASP.NET Core Identity and EF Core. Onboarding owns registration orchestration, while Authentication owns external provider sign-in.

The module deliberately distinguishes account deactivation from Identity lockout, enforces self-service authorization, resets contact-confirmation state when contact details change, and uses the existing Outbox pipeline for durable password-reset email delivery.

The direct Outbox write used for password-reset requests is a documented exception to the normal domain-event translation pipeline. It remains subject to the same durability, transaction, security, and delivery requirements as other Outbox messages.

The result is an Identity implementation that remains consistent with the E-Commerce modular monolith without introducing an unnecessary Identity domain model or distributed architecture.

---

**Document:** `reference.md`
**Module:** Identity & User Management
**Version:** 1.5 — Final
