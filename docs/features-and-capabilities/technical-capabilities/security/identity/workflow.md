# Identity & User Management — Workflow

**Project:** E-Commerce Modular Monolith
**Feature:** Identity & User Management
**Scope:** Account Lifecycle and Credential Management
**Version:** 1.0

---

## 1. Purpose

This document describes the end-to-end workflows for managing existing user accounts and credentials in the E-Commerce modular monolith.

It covers the operations performed after the Registration/Onboarding feature has created the user's ASP.NET Core Identity account.

The workflows include:

* Account activation and deactivation.
* Account locking and unlocking.
* Self-service account updates.
* Password changes.
* Password-reset requests.
* Password resets using Identity-generated tokens.
* Error handling and asynchronous notification delivery.

Registration, email and phone verification during onboarding, and external login management are outside this document's scope.

## 2. Architectural Overview

The Identity & User Management feature follows the application's established layered architecture.

```text
HTTP Request
    |
    v
API Controller
    |
    v
MediatR Command / Query
    |
    v
Authorization / Self-Service Validation
    |
    v
Application Handler
    |
    v
Application Abstraction
    |
    v
Infrastructure Implementation
    |
    v
ASP.NET Core Identity / AppDbContext
    |
    v
SQL Server
```

The asynchronous password-reset notification workflow follows a separate path:

```text
RequestPasswordResetCommandHandler
    |
    v
Generate Identity Password-Reset Token
    |
    v
Create PasswordResetRequestedIntegrationEvent
    |
    v
IOutboxMessageWriter
    |
    v
Outbox Record Persisted
    |
    v
Outbox Processing Job
    |
    v
Integration Event Dispatcher
    |
    v
Password-Reset Email Handler
    |
    v
Email Channel
```

### Architectural Responsibilities

| Component               | Responsibility                                                                          |
| ----------------------- | --------------------------------------------------------------------------------------- |
| API                     | Exposes endpoints and translates HTTP requests into application operations.             |
| Application             | Coordinates workflows, performs application-level checks, and defines abstractions.     |
| Authorization behaviors | Enforce configured permission or role requirements before the command handler executes. |
| `ICurrentUser`          | Provides the authenticated user's identifier for self-service operations.               |
| `IAccountReader`        | Reads account and security information required by application workflows.               |
| `IAccountManagement`    | Abstracts account lifecycle and account metadata operations.                            |
| `ICredentialManagement` | Abstracts password changes, reset-token generation, and password resets.                |
| Infrastructure          | Implements the abstractions using ASP.NET Core Identity and persistence services.       |
| Outbox                  | Durably records integration events for asynchronous processing.                         |
| Scheduling              | Executes background jobs used by the Outbox processing workflow.                        |

ASP.NET Core Identity remains an Infrastructure concern. Application handlers interact with it through application-defined abstractions rather than directly depending on `UserManager<TUser>`.

---

## 3. Account Lifecycle Workflows

Account lifecycle operations manage whether an account is active and whether authentication is currently restricted.

The application maintains a distinction between:

* **Account status:** Whether the account is administratively active or deactivated.
* **Identity lockout:** Whether authentication is prevented by the configured lockout state.

These are independent states and must be evaluated separately.

### 3.1 Activate Account

**Purpose:** Reactivate a previously deactivated account.

**Initiator:** Authorized administrator.

**Required permission:** `Accounts.Activate`

#### Workflow

```text
Administrator Request
       |
       v
ActivateAccountCommand
       |
       v
Authorization Behavior
       |
       v
IAccountManagement.ActivateAsync(userId)
       |
       v
Find Identity User
       |
       v
Set AccountStatus = Active
       |
       v
Persist Account Changes
       |
       v
Return Result
```

#### Rules

* The operation requires the appropriate administrative permission.
* The account must exist.
* Activation changes the administrative account status.
* Activation does not automatically clear Identity lockout.
* An account that remains locked cannot authenticate merely because its administrative status has changed to active.

### 3.2 Deactivate Account

**Purpose:** Disable an account without deleting its Identity record.

**Initiator:** Authorized administrator.

**Required permission:** `Accounts.Deactivate`

#### Workflow

```text
Administrator Request
       |
       v
DeactivateAccountCommand
       |
       v
Authorization Behavior
       |
       v
IAccountManagement.DeactivateAsync(userId)
       |
       v
Find Identity User
       |
       v
Set AccountStatus = Deactivated
       |
       v
Persist Account Changes
       |
       v
Return Result
```

#### Rules

* Deactivation requires the appropriate administrative permission.
* Deactivation does not delete the user or their associated business data.
* Deactivation does not inherently clear or establish Identity lockout.
* Authentication must reject deactivated accounts independently of the Identity lockout mechanism.
* The account-status check must be enforced in the authentication or authorization path; checking it only in selected application commands is insufficient.

### 3.3 Lock Account

**Purpose:** Prevent authentication through the Identity lockout mechanism.

**Initiator:** Authorized administrator.

**Required permission:** `Accounts.Lock`

#### Workflow

```text
Administrator Request
       |
       v
LockAccountCommand(userId, lockoutEnd)
       |
       v
Authorization Behavior
       |
       v
IAccountManagement.LockAsync(userId, lockoutEnd)
       |
       v
Find Identity User
       |
       v
Determine Effective Lockout End
       |
       v
SetLockoutEndDateAsync
       |
       v
Return Result
```

#### Lockout Behavior

The application uses the following convention:

| Input               | Intended behavior                                                        |
| ------------------- | ------------------------------------------------------------------------ |
| Future `lockoutEnd` | Lock the account until the specified time.                               |
| `null`              | Lock the account indefinitely by substituting `DateTimeOffset.MaxValue`. |

ASP.NET Core Identity treats a `null` lockout end as no active time-based lockout. Therefore, the application must explicitly translate a request for an indefinite lock into its chosen representation.

The Infrastructure implementation must also respect Identity's configured lockout behavior and persistence requirements.

### 3.4 Unlock Account

**Purpose:** Remove the Identity lockout and reset the failed-access counter.

**Initiator:** Authorized administrator.

**Required permission:** `Accounts.Unlock`

#### Workflow

```text
Administrator Request
       |
       v
UnlockAccountCommand
       |
       v
Authorization Behavior
       |
       v
IAccountManagement.UnlockAsync(userId)
       |
       v
Find Identity User
       |
       v
Clear Lockout End
       |
       v
Reset Failed Access Count
       |
       v
Return Result
```

The Infrastructure implementation performs the equivalent of:

* `SetLockoutEndDateAsync(user, null)`
* `ResetAccessFailedCountAsync(user)`

#### Rules

* Unlocking requires the appropriate administrative permission.
* Unlocking clears the Identity lockout.
* Unlocking does not change `AccountStatus`.
* A deactivated account remains deactivated after being unlocked.
* Both Identity operations must succeed for the application to report successful completion.

If partial updates are possible between these operations, the implementation must handle that possibility rather than assuming that multiple Identity calls automatically form one atomic transaction.

### 3.5 Update Account — Self-Service

**Purpose:** Allow an authenticated user to update their own email address, phone number, or username.

**Initiator:** Authenticated user.

#### Workflow

```text
Authenticated Request
       |
       v
UpdateAccountCommand
       |
       v
Obtain ICurrentUser.UserId
       |
       v
Read Account Security Information
       |
       v
Verify Account Is Active
       |
       v
Build UpdateAccountRequest
       |
       v
IAccountManagement.UpdateAsync(request)
       |
       v
Update Requested Fields
       |
       v
Persist Changes
       |
       v
Return Result
```

#### Field-Specific Behavior

| Field        | Required behavior                                                                                                        |
| ------------ | ------------------------------------------------------------------------------------------------------------------------ |
| Email        | Update through the Identity abstraction and require email re-verification when the email changes.                        |
| Phone number | Update through the Identity abstraction and require phone re-verification when the phone number changes.                 |
| Username     | Update through the Identity abstraction while respecting Identity's configured username validation and uniqueness rules. |

#### Rules

* The target user identifier comes from `ICurrentUser.UserId`, not from an arbitrary client-supplied identifier.
* The operation must not permit a user to update another user's account.
* Deactivated accounts cannot perform this operation.
* Email and phone verification states must be invalidated when their corresponding values change.
* Identity validation failures must be translated into the application's established result format.
* If all requested changes are intended to be atomic, the persistence implementation must guarantee that either all changes commit or none do.

**Transaction consideration:** Multiple calls to ASP.NET Core Identity are not automatically one atomic operation. If the implementation promises atomicity, all relevant writes must participate in the same database transaction and persistence context, with failures handled correctly.

---

## 4. Credential Management Workflows

Credential management changes or recovers the credentials associated with an existing Identity account.

### 4.1 Change Password

**Purpose:** Allow an authenticated user to change their own password after providing the current password.

**Initiator:** Authenticated user.

#### Workflow

```text
Authenticated Request
       |
       v
ChangePasswordCommand
       |
       v
Obtain ICurrentUser.UserId
       |
       v
Read Account Security Information
       |
       v
Verify Account Is Active
       |
       v
ICredentialManagement.ChangePasswordAsync(...)
       |
       v
Validate Current Password
       |
       v
Validate and Change Password
       |
       v
Update PasswordLastChangedAtUtc
       |
       v
Persist Changes
       |
       v
Return Result
```

#### Rules

* The operation is restricted to the authenticated user.
* The current password must be validated by ASP.NET Core Identity.
* The new password must satisfy the configured Identity password policy.
* Deactivated accounts cannot change passwords through this workflow.
* The operation does not change the account's administrative status.
* `PasswordLastChangedAtUtc` must reflect a successful password change.
* If the timestamp update and password change are promised to be atomic, both must participate in a transaction that covers the relevant persistence operations.

Any required session revocation or security-stamp update must be implemented explicitly according to the application's authentication and session-management strategy.

### 4.2 Request Password Reset

**Purpose:** Initiate password recovery by generating an Identity password-reset token and arranging for the user to receive it through email.

**Initiator:** Anonymous caller.

#### Synchronous Workflow

```text
Anonymous Request
       |
       v
RequestPasswordResetCommand(email)
       |
       v
IAccountReader.GetByEmailAsync(email)
       |
       +------------------------------+
       |                              |
       v                              v
  User Not Found                  User Found
       |                              |
       v                              v
Return Generic Success        Generate Identity Reset Token
                                      |
                                      v
                            Create PasswordResetRequested
                                 Integration Event
                                      |
                                      v
                             IOutboxMessageWriter
                                      |
                                      v
                            Persist Outbox Message
                                      |
                                      v
                             Return Generic Success
```

The endpoint must return the same externally observable response whether the email corresponds to an account or not.

This reduces account enumeration risk. Response timing and other externally observable behavior should also be considered when implementing the workflow.

#### Asynchronous Email Workflow

```text
Outbox Processing Job
       |
       v
Retrieve Pending Outbox Message
       |
       v
Dispatch PasswordResetRequestedIntegrationEvent
       |
       v
Resolve Email Handler
       |
       v
Construct Password-Reset Email
       |
       v
Send Through IEmailChannel
       |
       v
Record Successful Processing
```

The Outbox and integration-event infrastructure own durable delivery, retries, and dead-letter handling.

#### Channel 5 — Direct Integration Event Publication

This workflow deliberately writes an integration event directly from the command handler rather than translating a domain event into an integration event.

The justification is specific to this operation:

* The Identity subsystem manages ASP.NET Core Identity users through application and infrastructure abstractions.
* It does not require an Identity domain aggregate solely to represent a password-reset request.
* The command handler's primary output is the request to initiate password recovery.
* Introducing a fictitious domain event solely to produce the integration event would add an unnecessary abstraction.

The direct write is acceptable only if it preserves the required durability guarantees.

**Transaction requirement:** The Outbox record must be committed through the application's intended persistence boundary. If the command uses a shared `AppDbContext` and Unit of Work, the Outbox write must participate in that same transaction. The message must not be reported as durably accepted before the transaction commits.

This is a deliberate exception to the usual domain-event-to-integration-event publication path. It does not authorize arbitrary command handlers throughout the application to bypass the established Outbox architecture.

#### Password-Reset Token Handling

The event contains the information required to deliver the reset link, including:

* `UserId`
* `Email`
* `ResetToken`

The raw reset token is sensitive.

The implementation must:

* Never log the raw token.
* Avoid exposing the token through telemetry, exception messages, or diagnostic output.
* Restrict access to serialized Outbox payloads.
* Protect database backups and other storage containing the token.
* Apply an appropriate retention policy to sensitive Outbox records.
* Ensure the reset URL uses the configured frontend origin rather than an untrusted host supplied by the request.

The token should be delivered only through the intended password-reset channel.

### 4.3 Reset Password

**Purpose:** Allow a user to establish a new password using a valid Identity-generated reset token.

**Initiator:** Anonymous caller possessing the reset link.

#### Reset-Link Format

The email template constructs a URL containing the user identifier and reset token:

```text
https://{configured-frontend-origin}/reset-password
    ?userId={userId}
    &token={url-encoded-reset-token}
```

The actual URL must be encoded correctly so that special characters in the token survive transport.

#### Workflow

```text
User Opens Reset Link
       |
       v
Frontend Reads userId and Token
       |
       v
User Submits New Password
       |
       v
ResetPasswordCommand(userId, token, newPassword)
       |
       v
ICredentialManagement.ResetPasswordAsync(...)
       |
       v
Find Identity User
       |
       v
Validate Reset Token Against That User
       |
       v
Apply New Password Through Identity
       |
       v
Update PasswordLastChangedAtUtc
       |
       v
Persist Changes
       |
       v
Return Result
```

#### Source of `userId`

Because the caller is anonymous, the application cannot derive the target account from `ICurrentUser`.

The frontend therefore forwards the `userId` supplied in the reset link. ASP.NET Core Identity validates the reset token against that specific user.

The identifier is not an authorization credential. Knowledge of a user's identifier must never be sufficient to reset their password; the Identity reset token must validate successfully.

#### Rules

* The reset token is validated by ASP.NET Core Identity.
* An invalid, expired, or otherwise unacceptable token must not change the password.
* The new password must satisfy the configured Identity password policy.
* The timestamp update must correspond to a successful password reset.
* The password change and timestamp update must share the intended transaction boundary if atomicity is promised.
* The raw reset token must not be stored in an application-owned domain aggregate.
* The reset endpoint must use appropriate rate limiting and generic error responses to reduce abuse and account enumeration.
* After a successful reset, the application must follow its defined session invalidation and credential-revocation policy.

The user identifier is a public identifier, but the reset token is a sensitive bearer credential. URLs containing tokens can leak through browser history, access logs, analytics, referrer headers, or monitoring systems. The frontend and infrastructure must minimize these exposures and avoid loading unnecessary third-party resources on the reset page.

---

## 5. Outbox and Asynchronous Boundary

The Identity subsystem requests asynchronous password-reset email delivery through `IOutboxMessageWriter`.

It does not own:

* Outbox persistence implementation.
* Integration-event serialization and dispatch.
* Background-job execution.
* Retry and dead-letter policies.
* Integration-event idempotency infrastructure.
* Email transport implementation.

Those responsibilities remain with the shared technical capabilities.

### End-to-End Durability

The expected sequence is:

1. The command handler generates the reset token through the Identity abstraction.
2. It creates `PasswordResetRequestedIntegrationEvent`.
3. It stages the event through `IOutboxMessageWriter`.
4. The Unit of Work commits the Outbox record.
5. The Outbox processor dispatches the event asynchronously.
6. The email handler requests delivery through `IEmailChannel`.
7. The integration-event infrastructure records successful processing only after the handler reaches its defined durable success boundary.

Outbox dispatch generally provides at-least-once delivery rather than guaranteed exactly-once side effects. Email delivery can be repeated after failures, so the handler and delivery infrastructure should use appropriate idempotency or duplicate-mitigation mechanisms.

A successful email-provider response also does not guarantee that the recipient has read the message or that the message will reach the inbox.

---

## 6. Error Handling

Errors are handled at the layer responsible for understanding them.

| Error condition                        | Handling location                                   | Expected outcome                                               |
| -------------------------------------- | --------------------------------------------------- | -------------------------------------------------------------- |
| Missing or invalid Identity user       | Application handler or Infrastructure abstraction   | Appropriate failure result.                                    |
| Invalid current password               | Identity abstraction and application result mapping | Password change rejected.                                      |
| Invalid or expired reset token         | Identity abstraction and application result mapping | Password reset rejected.                                       |
| Identity validation error              | Infrastructure abstraction / application mapping    | Validation failure returned without leaking sensitive details. |
| Missing required permission            | MediatR authorization behavior                      | Forbidden result; handler does not execute.                    |
| Unauthenticated request                | Authentication middleware                           | Authentication failure, normally HTTP 401.                     |
| Authenticated but unauthorized request | Authorization pipeline                              | Forbidden response, normally HTTP 403.                         |
| Concurrency conflict                   | Persistence or global exception middleware          | Conflict response where applicable.                            |
| Unique-constraint violation            | Persistence or global exception middleware          | Conflict or appropriate validation response.                   |
| Unexpected infrastructure exception    | Global exception middleware                         | HTTP 500 with safe external details.                           |
| Outbox dispatch failure                | Outbox processing infrastructure                    | Retry or dead-letter handling according to policy.             |
| Email delivery failure                 | Email delivery / integration-event infrastructure   | Failure propagated for configured retry and recovery.          |

The application's established API error contract determines how `Result` failures are translated into HTTP responses.

Authorization failures produced by `PermissionAuthorizationBehavior` or `RoleAuthorizationBehavior` occur before the application handler executes. They must not be documented as failures generated inside the command handler.

Sensitive data—including passwords, reset tokens, and authentication credentials—must never be included in ordinary logs or client-facing exception details.

---

## 7. Workflow Summary

| Operation              | Initiator                         | Main execution path                                       | Asynchronous processing                                    |
| ---------------------- | --------------------------------- | --------------------------------------------------------- | ---------------------------------------------------------- |
| Activate account       | Authorized administrator          | Authorization → account management → persistence          | None required by this workflow.                            |
| Deactivate account     | Authorized administrator          | Authorization → account management → persistence          | None required by this workflow.                            |
| Lock account           | Authorized administrator          | Authorization → Identity lockout management               | None required by this workflow.                            |
| Unlock account         | Authorized administrator          | Authorization → Identity lockout management               | None required by this workflow.                            |
| Update account         | Authenticated user                | Current user → active-state check → account management    | Verification workflows may follow separately.              |
| Change password        | Authenticated user                | Current user → active-state check → credential management | Session invalidation may be required by policy.            |
| Request password reset | Anonymous caller                  | Account lookup → token generation → Outbox write          | Reset-email delivery.                                      |
| Reset password         | Anonymous caller with reset token | Token validation → password reset → timestamp update      | Additional security notifications may be added separately. |

---

## 8. Workflow Invariants

The following rules must remain true across all implementations:

1. **Identity separation:** ASP.NET Core Identity remains an Infrastructure concern accessed through Application-defined abstractions.
2. **Account-state separation:** Administrative account status and Identity lockout are independent.
3. **Self-service ownership:** A user can modify only their own account through self-service operations.
4. **Permission enforcement:** Administrative lifecycle operations require the appropriate authorization.
5. **Verification integrity:** Changing an email address or phone number invalidates the corresponding verification state.
6. **Credential integrity:** Password changes and resets use ASP.NET Core Identity validation and password-management APIs.
7. **Transaction consistency:** Operations advertised as atomic must have a real transaction covering all relevant writes.
8. **Outbox durability:** Password-reset requests are not reported as durably accepted until the required Outbox write commits.
9. **Token confidentiality:** Raw password-reset tokens must never be logged or exposed through diagnostic systems.
10. **Anonymous reset validation:** The reset token must be validated against the user identified by the supplied `userId`.
11. **Account enumeration resistance:** Password-reset requests must return a generic external response regardless of account existence.
12. **Asynchronous ownership:** The Outbox and integration-event infrastructure own durable event delivery and its recovery lifecycle.
13. **Failure transparency:** Failures must not be reported as successful merely because an intermediate operation completed.
14. **Security after credential changes:** Session invalidation and credential revocation must follow an explicitly defined security policy.

---

## 9. Conclusion

The Identity & User Management workflow separates account administration, self-service account updates, and credential recovery into clear application-level operations.

ASP.NET Core Identity provides the underlying identity and credential-management mechanisms, while Application orchestrates the workflows through abstractions. Infrastructure handles Identity persistence, and the shared Outbox and Scheduling capabilities provide durable asynchronous password-reset notification processing.

Two design decisions are particularly important:

* **Direct Outbox publication:** The password-reset request may write an integration event directly because it does not require a fabricated domain aggregate or domain event. The write must still preserve the application's transaction and durability guarantees.
* **User identifier in the reset link:** The identifier selects the account against which ASP.NET Core Identity validates the reset token. The identifier alone grants no authority to reset the password.

These boundaries preserve the modular monolith's architecture while supporting maintainable account management, secure credential recovery, and reliable asynchronous processing.
