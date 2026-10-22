# Registration & Account Verification — Workflow Technical Document

**Project:** E-Commerce Modular Monolith

**Scope:** End-to-end workflows for registration creation, verification, resend, expiry, cleanup, and account provisioning

**Version:** 1.0 — Final

**Related Documents:** Registration Technical Requirements, Registration Technical Reference, Registration Domain Model Technical Reference, Outbox Workflow Technical Document

---

## 1. Overview

This document defines the complete end-to-end workflows for the Registration & Account Verification feature. It covers:

* Registration creation with dual OTP generation.
* Email and phone verification.
* Resend of verification codes with cooldown and limit enforcement.
* Registration expiry by lifetime and by attempt exhaustion.
* Background cleanup of expired registrations.
* Account provisioning triggered by full verification.
* Correlation ID propagation across the asynchronous boundary.

Each workflow documents its trigger, actors, step-by-step flow, transaction boundaries, failure modes, and idempotency behavior. Together, they describe the complete runtime behavior of the feature.

---

## 2. Actors and Components

### 2.1 Actors

| Actor                        | Description                                                            |
| ---------------------------- | ---------------------------------------------------------------------- |
| Client                       | Unauthenticated user interacting with registration endpoints.          |
| Registration API             | Thin HTTP adapter that dispatches commands.                            |
| Registration Command Handler | Application-layer handler executing a use case.                        |
| Registration Aggregate       | Domain entity enforcing verification invariants.                       |
| Verification Code Service    | Infrastructure service generating and comparing OTP hashes.            |
| Password Hasher              | Infrastructure service hashing passwords before they reach the domain. |
| Outbox                       | Durable integration event store.                                       |
| Outbox Processing Job        | Background dispatcher for integration events.                          |
| Email Channel                | Notification subsystem component delivering the email OTP.             |
| SMS Channel                  | Notification subsystem component delivering the SMS OTP.               |
| Identity Service             | Infrastructure service creating ASP.NET Core Identity users.           |
| Role Management Service      | Application abstraction for assigning roles to users.                  |
| Registration Cleanup Service | Infrastructure service bulk-deleting expired registrations.            |
| Cleanup Job                  | Background job triggering the cleanup service on schedule.             |

### 2.2 Key Components

| Component                                     | Layer          | Responsibility                                                                         |
| --------------------------------------------- | -------------- | -------------------------------------------------------------------------------------- |
| `RegisterCommandHandler`                      | Application    | Orchestrates registration creation and OTP generation.                                 |
| `VerifyEmailCommandHandler`                   | Application    | Orchestrates email channel verification.                                               |
| `VerifyPhoneCommandHandler`                   | Application    | Orchestrates phone channel verification.                                               |
| `ResendEmailCommandHandler`                   | Application    | Orchestrates email code resend.                                                        |
| `ResendPhoneCommandHandler`                   | Application    | Orchestrates phone code resend.                                                        |
| `RegistrationFullyVerifiedDomainEventHandler` | Application    | Translates the fully verified domain event into an outbox row.                         |
| `SendVerificationEmailHandler`                | Application    | Delivers the email OTP via `IEmailChannel`.                                            |
| `SendVerificationSmsHandler`                  | Application    | Delivers the SMS OTP via `ISmsChannel`.                                                |
| `CreateUserOnRegistrationVerifiedHandler`     | Application    | Provisions the Identity user, assigns the Customer role, and removes the registration. |
| `ExpiredRegistrationCleanupJobHandler`        | Application    | Invokes the cleanup service.                                                           |
| `Registration`                                | Domain         | Aggregate root enforcing all verification invariants.                                  |
| `VerificationCodeService`                     | Infrastructure | Generates and compares OTP hashes.                                                     |
| `RegistrationRepository`                      | Infrastructure | Persists the aggregate.                                                                |
| `RegistrationCleanupService`                  | Infrastructure | Bulk-deletes expired registrations.                                                    |
| `IdentityService`                             | Infrastructure | Creates Identity users with pre-hashed passwords.                                      |
| `RoleManagementService`                       | Infrastructure | Assigns roles to users.                                                                |

---

## 3. Workflow 1 — Registration Creation

### 3.1 Trigger

A client submits a registration request to the registration endpoint.

### 3.2 Flow Diagram

```text
Client
 |
 | POST /api/auth/register
 | { email, phone, username, password }
 v
Registration Controller (anonymous)
 |
 v
RegisterCommand
 |
 v
RegisterCommandHandler
 |
 ├── Normalize input (email, phone, username)
 |
 ├── Hash password via IPasswordHasher
 |
 ├── Generate email OTP:
 |     IVerificationCodeService.GenerateCode(out emailCode)
 |     → returns (emailCode, emailCodeHash)
 |
 ├── Generate phone OTP:
 |     IVerificationCodeService.GenerateCode(out phoneCode)
 |     → returns (phoneCode, phoneCodeHash)
 |
 ├── Create Registration aggregate:
 |     Registration.Create(email, phone, username, passwordHash)
 |
 ├── Set verification channels:
 |     registration.SetEmailVerificationCode(emailCodeHash, utcNow)
 |     registration.SetPhoneVerificationCode(phoneCodeHash, utcNow)
 |
 ├── Write EmailVerificationRequestedIntegrationEvent
 |     (carries plaintext emailCode)
 |     IOutboxMessageWriter.WriteAsync(...)
 |
 ├── Write PhoneVerificationRequestedIntegrationEvent
 |     (carries plaintext phoneCode)
 |     IOutboxMessageWriter.WriteAsync(...)
 |
 ├── IRegistrationRepository.AddAsync(registration, ct)
 |
 └── IUnitOfWork.SaveChangesAsync(ct)
       |
       ├── Save #1: aggregate persisted
       |
       ├── Domain events collected and dispatched:
       |     RegistrationCreatedDomainEvent
       |     EmailVerificationCodeGeneratedDomainEvent
       |     PhoneVerificationCodeGeneratedDomainEvent
       |
       ├── Save #2: outbox rows persisted
       |
       └── Commit transaction
 |
 v
Return 200 with { registrationId }
```

### 3.3 Transaction Boundary

The entire flow executes in a single transaction. The aggregate, both verification channels, and both outbox rows commit together or not at all.

### 3.4 Important Rules

* The password is hashed **before** the aggregate is created. The plaintext never reaches the domain.
* The plaintext OTP codes are available only inside the handler. They are placed into the integration events and then discarded. The aggregate stores only the hashes.
* The two integration events are separate. Each carries only its own channel's code.
* The correlation ID from `IAppContext` is set on both integration events.
* The registration handler writes the integration events **directly**, rather than through domain event handlers. This is the one place in the codebase where that pattern is used because the plaintext codes are not available to any domain event handler.

### 3.5 Failure Behavior

| Failure Point                                             | Result                                         |
| --------------------------------------------------------- | ---------------------------------------------- |
| Input validation fails                                    | HTTP 400 before any database write.            |
| Password hashing fails                                    | Exception; no database write.                  |
| Code generation fails                                     | Exception; no database write.                  |
| Duplicate email, phone, or username (database constraint) | `DbUpdateException` → HTTP 409 via middleware. |
| Aggregate creation fails (domain rule)                    | `DomainException` → HTTP 400.                  |
| Outbox write fails                                        | Transaction rolls back.                        |
| Commit fails                                              | Transaction rolls back; no state is persisted. |

### 3.6 Idempotency

The unique indexes on email, phone, and username prevent duplicate registrations. The database constraint is the authority; application-level checks may exist as fast paths, but the database enforces uniqueness.

If a client retries the same registration request while the original is still active, the second attempt hits the unique constraint and returns HTTP 409.

---

## 4. Workflow 2 — Email Verification

### 4.1 Trigger

The client submits a verification code for the email channel.

### 4.2 Flow Diagram

```text
Client
 |
 | POST /api/auth/verify-email
 | { registrationId, code }
 v
Registration Controller (anonymous)
 |
 v
VerifyEmailCommand
 |
 v
VerifyEmailCommandHandler
 |
 ├── IRegistrationRepository.GetByIdAsync(registrationId, ct)
 |
 ├── If registration is null:
 |     → Result.Failure("Registration not found")
 |
 ├── If registration.EmailVerification.IsVerified:
 |     → Result.Success (idempotent)
 |
 ├── IVerificationCodeService.VerifyCode(
 |       submittedCode,
 |       registration.EmailVerification.CodeHash)
 |     → bool isValid (constant-time comparison)
 |
 ├── registration.VerifyEmail(isValid, utcNow)
 |     |
 |     ├── AssertNotExpired(utcNow) — throws if expired
 |     |
 |     ├── AssertNotCompleted() — throws if completed
 |     |
 |     ├── If !isValid:
 |     |     AttemptCount++
 |     |     If AttemptCount >= MaxAttemptsPerChannel:
 |     |       MarkExpired()
 |     |       → RegistrationExpiredDomainEvent
 |     |     Else:
 |     |       no state change beyond the attempt counter
 |     |
 |     └── If isValid:
 |           EmailVerification = VerificationChannel.Verified
 |           AddDomainEvent(EmailVerifiedDomainEvent)
 |           TryCompleteRegistration()
 |
 └── IUnitOfWork.SaveChangesAsync(ct)
       |
       ├── Save #1: aggregate state persisted
       |
       ├── Dispatch domain events:
       |     EmailVerifiedDomainEvent
       |     Possibly RegistrationFullyVerifiedDomainEvent
       |
       ├── Save #2: any outbox rows produced by domain event handlers
       |
       └── Commit
 |
 v
Return 204 (success) or 400 (invalid code / expired)
```

### 4.3 Transaction Boundary

A single transaction covers the aggregate update and any domain-event-driven outbox writes.

### 4.4 Important Rules

* `VerifyEmail` guards against expired and completed states before any logic runs.
* Failed attempts increment the counter. The counter is persisted even when the code is wrong.
* Successful verification clears the channel's code hash. The submitted code cannot be reused.
* If the channel is already verified, the handler returns success idempotently. This handles retries and duplicate submissions.
* Constant-time comparison is implemented inside `VerificationCodeService`; handlers do not implement it themselves.

### 4.5 Failure Behavior

| Failure Point                          | Result                                                                                |
| -------------------------------------- | ------------------------------------------------------------------------------------- |
| Registration not found                 | HTTP 404.                                                                             |
| Channel already verified               | HTTP 200 (idempotent success).                                                        |
| Code expired                           | `DomainException` from `VerifyEmail` → HTTP 400.                                      |
| Attempt limit exceeded on this attempt | Registration is marked expired; `RegistrationExpiredDomainEvent` is raised; HTTP 400. |
| Incorrect code, attempts remaining     | HTTP 400; attempt counter persisted.                                                  |
| Registration expired                   | `AssertNotExpired` throws; HTTP 400.                                                  |

### 4.6 Idempotency

* **Duplicate submission of a valid code:** The second call sees `IsVerified` and returns success.
* **Duplicate submission of an invalid code:** The attempt counter increments again. This is intentional and matches the rule that each attempt counts.

---

## 5. Workflow 3 — Phone Verification

### 5.1 Trigger

The client submits a verification code for the phone channel.

### 5.2 Flow Diagram

The flow is the same as Workflow 2, substituting:

* `registration.PhoneVerification`
* `registration.VerifyPhone(isValid, utcNow)`
* `PhoneVerifiedDomainEvent`
* `POST /api/auth/verify-phone`

The behaviors, guards, transaction boundaries, and idempotency rules are identical. The phone channel is independent of the email channel: verifying the phone does not affect the email channel state.

### 5.3 Important Rules

* Attempt and resend counters are **per channel**. An email attempt does not count toward the phone attempt limit.
* If both channels become verified during this workflow, `RegistrationFullyVerifiedDomainEvent` is raised in addition to `PhoneVerifiedDomainEvent`.

---

## 6. Workflow 4 — Resend Verification Code

### 6.1 Trigger

The client requests a new verification code for a channel whose current code has not yet been verified.

### 6.2 Flow Diagram

```text
Client
 |
 | POST /api/auth/resend-email
 | { registrationId }
 v
Registration Controller (anonymous)
 |
 v
ResendEmailCommand
 |
 v
ResendEmailCommandHandler
 |
 ├── IRegistrationRepository.GetByIdAsync(registrationId, ct)
 |
 ├── If registration is null:
 |     → Result.Failure("Registration not found")
 |
 ├── If registration.EmailVerification.IsVerified:
 |     → Result.Failure("Email already verified")
 |
 ├── IVerificationCodeService.GenerateCode(out newCode)
 |     → returns (newCode, newCodeHash)
 |
 ├── registration.SetEmailVerificationCode(newCodeHash, utcNow)
 |     |
 |     ├── AssertNotExpired(utcNow) — throws if expired
 |     |
 |     ├── AssertNotCompleted() — throws if completed
 |     |
 |     ├── If ResendCount >= MaxResendsPerChannel:
 |     |     throw RegistrationException("Resend limit reached")
 |     |
 |     ├── If SentAt + ResendCooldown > utcNow:
 |     |     throw RegistrationException("Resend cooldown not elapsed")
 |     |
 |     └── EmailVerification = new VerificationChannel {
 |           CodeHash = newCodeHash,
 |           SentAt = utcNow,
 |           ExpiresAt = utcNow + OTPLifetime,
 |           ResendCount = ResendCount + 1,
 |           AttemptCount = AttemptCount (unchanged)
 |         }
 |         → AddDomainEvent(EmailVerificationCodeGeneratedDomainEvent)
 |
 ├── Write EmailVerificationRequestedIntegrationEvent
 |     (carries new plaintext newCode)
 |     IOutboxMessageWriter.WriteAsync(...)
 |
 └── IUnitOfWork.SaveChangesAsync(ct)
       |
       ├── Save #1: aggregate state persisted
       ├── Domain events dispatched
       ├── Save #2: outbox row persisted
       └── Commit
 |
 v
Return 204 (success) or 400 (resend not allowed)
```

### 6.3 Transaction Boundary

A single transaction covers the aggregate update and the outbox row.

### 6.4 Important Rules

* The previous code is invalidated by replacing the entire `VerificationChannel` record with a new one. The old hash is discarded.
* `ResendCount` increments; `AttemptCount` is preserved.
* The new code has a fresh five-minute expiry. The previous code's expiry does not carry over.
* `SentAt` is updated, which is used by the next resend cooldown check.
* The resend limit is `RegistrationConstants.MaxResendsPerChannel` (3).
* The cooldown is `RegistrationConstants.ResendCooldown` (60 seconds).

### 6.5 Failure Behavior

| Failure Point            | Result                                |
| ------------------------ | ------------------------------------- |
| Registration not found   | HTTP 404.                             |
| Channel already verified | HTTP 400.                             |
| Resend limit reached     | `RegistrationException` → HTTP 400.   |
| Cooldown not elapsed     | `RegistrationException` → HTTP 400.   |
| Registration expired     | `AssertNotExpired` throws → HTTP 400. |

### 6.6 Idempotency

A resend is not idempotent by design: each resend generates a new code and increments the counter. Retrying a resend immediately after success fails the cooldown check, preventing accidental rapid duplicate resends.

---

## 7. Workflow 5 — Full Verification and Completion Detection

### 7.1 Trigger

Both `EmailVerification.IsVerified` and `PhoneVerification.IsVerified` become `true`. The second channel to be verified triggers this workflow as part of its own `Verify*` call.

### 7.2 Flow Diagram

```text
Verify{Email|Phone}CommandHandler
 |
 v
registration.Verify{Email|Phone}(isValid, utcNow)
 |
 ├── Channel verification succeeds
 |
 ├── AddDomainEvent({Email|Phone}VerifiedDomainEvent)
 |
 └── TryCompleteRegistration()
       |
       ├── If EmailVerification.IsVerified
       |   && PhoneVerification.IsVerified
       |   && !RegistrationCompleted
       |
       ├── RegistrationCompleted = true
       |
       └── AddDomainEvent(RegistrationFullyVerifiedDomainEvent)
 |
 v
IUnitOfWork.SaveChangesAsync(ct)
 |
 ├── Save #1
 |
 ├── Dispatch domain events:
 |     |
 |     ├── {Email|Phone}VerifiedDomainEvent
 |     |     → no handlers or logging only
 |     |
 |     └── RegistrationFullyVerifiedDomainEvent
 |           → RegistrationFullyVerifiedDomainEventHandler
 |               → IOutboxMessageWriter.WriteAsync(
 |                   RegistrationFullyVerifiedIntegrationEvent,
 |                   correlationId = IAppContext.CorrelationId)
 |
 ├── Save #2: outbox row persisted
 |
 └── Commit
```

### 7.3 Important Rules

* `TryCompleteRegistration` is idempotent: `RegistrationCompleted` prevents a duplicate `RegistrationFullyVerifiedDomainEvent` if the aggregate's state is reprocessed.
* `RegistrationFullyVerifiedIntegrationEvent` carries `RegistrationId`, `Email`, `PhoneNumber`, `Username`, and `CorrelationId`.
* The integration event is the **only trigger** for account provisioning.

---

## 8. Workflow 6 — Account Provisioning

### 8.1 Trigger

The Outbox processing job delivers `RegistrationFullyVerifiedIntegrationEvent` to `CreateUserOnRegistrationVerifiedHandler`.

### 8.2 Flow Diagram

```text
Outbox Processing Job
 |
 v
IdempotentIntegrationEventHandler<RegistrationFullyVerifiedIntegrationEvent>
 |
 ├── Check (EventId, HandlerIdentifier) in ProcessedEvents
 |     └── If processed → return (skip)
 |
 └── Invoke inner handler
 |
 v
CorrelationScopeIntegrationEventHandler (outermost — runs first)
 |
 ├── Push CorrelationId onto log scope
 |
 v
CreateUserOnRegistrationVerifiedHandler.HandleAsync
 |
 ├── IRegistrationRepository.GetByIdAsync(evt.RegistrationId, ct)
 |
 ├── If registration is null:
 |     → return (already processed or removed)
 |
 ├── Create Identity user:
 |     IIdentityService.CreateUserWithPrehashedPasswordAsync({
 |       Email = evt.Email,
 |       PhoneNumber = evt.PhoneNumber,
 |       Username = evt.Username,
 |       PasswordHash = registration.PasswordHash.Value
 |     })
 |     → userId
 |
 ├── Assign Customer role:
 |     IRoleManagementService.AssignRoleToUserAsync(
 |       userId,
 |       SystemRoles.Customer,
 |       ct)
 |
 ├── Remove registration:
 |     IRegistrationRepository.Remove(registration)
 |
 ├── IUnitOfWork.SaveChangesAsync(ct)
 |     └── Single commit covering user creation, role assignment,
 |         and registration removal
 |
 └── Log success:
       "Account provisioned for registration {RegistrationId}, user {UserId}"
```

### 8.3 Transaction Boundary

A single transaction covers the provisioning operation. `IIdentityService`, `IRoleManagementService`, and `IRegistrationRepository` all stage changes in the shared `AppDbContext`. The `SaveChangesAsync` call is the sole commit point.

If any step throws, the transaction rolls back and the registration remains in place. The Outbox retries the message on the next cycle.

### 8.4 Important Rules

* The handler is wrapped by the pipeline's idempotency decorator. Repeated deliveries of the same event skip execution.
* If the registration is not found, the handler returns without error. This covers:

  1. A previous attempt already provisioned the account.
  2. The cleanup job removed an expired registration before provisioning ran.
  3. The event references an unknown registration.
* The Customer role is assigned using `SystemRoles.Customer`, not a string literal.
* No plaintext password appears anywhere. The pre-hashed value from the registration is used directly.

### 8.5 Failure Behavior

| Failure Point                       | Result                                               |
| ----------------------------------- | ---------------------------------------------------- |
| Registration not found              | Return without error (idempotent no-op).             |
| Identity user creation fails        | Exception → transaction rolls back → Outbox retries. |
| Role assignment fails               | Exception → transaction rolls back → Outbox retries. |
| Registration removal fails          | Exception → transaction rolls back → Outbox retries. |
| All steps succeed, but commit fails | Transaction rolls back → Outbox retries.             |

### 8.6 Idempotency

Two layers provide idempotency:

1. **Pipeline decorator:** `IdempotentIntegrationEventHandler<T>` prevents duplicate handler invocations for the same event.
2. **Registration null check:** Even if the decorator is bypassed, such as by a manual trigger, the handler is a no-op when the registration no longer exists.

---

## 9. Workflow 7 — Registration Expiry

### 9.1 Two Expiry Paths

#### Path A — Lifetime Expiry

The registration's `ExpiresAtUtc` is set to `CreatedAtUtc + 15 minutes`. Any public method called after that timestamp fails the `AssertNotExpired` guard.

The registration is not marked expired by a background process; expiry is derived from the current time.

#### Path B — Attempt Exhaustion

When a channel's `AttemptCount` reaches `RegistrationConstants.MaxAttemptsPerChannel` (5), the aggregate calls `MarkExpired()`:

```text
Verify{Email|Phone}(isValid = false, utcNow)
 |
 ├── channel.AttemptCount++
 |
 └── If AttemptCount >= MaxAttemptsPerChannel
       |
       v
     MarkExpired()
       |
       ├── AddDomainEvent(RegistrationExpiredDomainEvent)
       |
       └── ExpiresAtUtc = utcNow (or a sentinel)
```

### 9.2 Flow Diagram — Attempt Exhaustion

```text
Client submits incorrect code
 |
 v
Verify{Email|Phone}CommandHandler
 |
 v
registration.Verify{Email|Phone}(isValid: false, utcNow)
 |
 ├── AssertNotExpired ✓
 |
 ├── AssertNotCompleted ✓
 |
 ├── AttemptCount++ → 5
 |
 └── AttemptCount >= MaxAttemptsPerChannel
       |
       v
     MarkExpired()
       |
       ├── ExpiresAtUtc = utcNow
       |
       └── AddDomainEvent(RegistrationExpiredDomainEvent)
 |
 v
IUnitOfWork.SaveChangesAsync
 |
 ├── Save #1: aggregate expired
 |
 ├── Domain events dispatched
 |
 └── Commit
 |
 v
Return HTTP 400 to client
```

### 9.3 Important Rules

* Lifetime expiry is **implicit**: the aggregate enforces it through guards, with no explicit state change.
* Attempt exhaustion is **explicit**: the aggregate marks the registration expired and raises a domain event.
* Both paths lead to the same observable outcome: the registration cannot be verified or resent further.
* `RegistrationExpiredDomainEvent` is raised only on attempt exhaustion. Lifetime expiry has no event because no code path is triggered.

---

## 10. Workflow 8 — Background Cleanup

### 10.1 Trigger

The cleanup job fires on its configured schedule. The default is daily at 03:00 UTC; production deployment should override it to run every 15–30 minutes.

### 10.2 Flow Diagram

```text
Scheduling Module
 |
 v
ExpiredRegistrationCleanupTrigger ([RecurringJob])
 |
 ├── Enqueues ExpiredRegistrationCleanupJob
 |
 v
JobOrchestrator applies policies
(timeout, circuit breaker, tracing)
 |
 v
ExpiredRegistrationCleanupJobHandler
 |
 ├── IRegistrationCleanupService.DeleteInactiveExpiredAsync(utcNow)
 |     |
 |     v
 |   RegistrationCleanupService
 |     |
 |     v
 |   _dbContext.Registrations
 |     .Where(r => r.ExpiresAtUtc < utcNow)
 |     .ExecuteDeleteAsync(ct)
 |
 └── Log: "Removed {count} expired registrations"
```

### 10.3 Transaction Boundary

The cleanup runs as a single bulk delete. `ExecuteDeleteAsync` executes one SQL `DELETE` statement. No explicit transaction is needed because the operation is inherently atomic at the statement level.

### 10.4 Important Rules

* The bulk delete bypasses the change tracker. No entities are loaded into memory.
* The soft-delete interceptor does not apply to `ExecuteDeleteAsync`; the delete is physical.
* The cleanup does not raise domain events. It operates directly on the database.
* The cleanup does not interact with the Outbox. The deleted registrations either never produced integration events or produced ones that were already processed.

### 10.5 Failure Behavior

| Failure Point            | Result                                                                                                |
| ------------------------ | ----------------------------------------------------------------------------------------------------- |
| Database unreachable     | Exception → Hangfire retries the job.                                                                 |
| Concurrent modifications | SQL Server row locks handle the operation; the delete affects whichever rows match at execution time. |
| Timeout                  | The `JobOrchestrator` policy aborts the operation; the job is retried.                                |

### 10.6 Idempotency

The delete is idempotent: running it multiple times removes nothing extra after the first run.

---

## 11. Correlation ID Propagation

### 11.1 Flow Diagram

```text
HTTP request with X-Correlation-Id
 |
 v
CorrelationIdMiddleware
 |
 ├── Reads or generates correlation ID
 |
 ├── Stores it in HttpContext.Items
 |
 ├── Pushes it onto the log scope
 |
 └── Echoes it in the response header
 |
 v
RegisterCommandHandler
 |
 ├── Reads IAppContext.CorrelationId
 |
 ├── EmailVerificationRequestedIntegrationEvent
 |     { CorrelationId = ... }
 |
 └── PhoneVerificationRequestedIntegrationEvent
       { CorrelationId = ... }
 |
 v
[Transaction commits; later, in the Outbox processing job]
 |
 v
CorrelationScopeIntegrationEventHandler
 |
 ├── Reads evt.CorrelationId
 |
 └── Pushes it onto the log scope
 |
 v
SendVerificationEmailHandler / SendVerificationSmsHandler
 |
 └── All log entries carry the original correlation ID
```

### 11.2 Continuity Across the Feature

* The registration's correlation ID propagates to the two verification-requested events, which carry it into the notification pipeline.
* The verification endpoints (verify email and verify phone) produce their own correlation IDs from their respective requests. Those IDs propagate to the resulting integration events, such as `RegistrationFullyVerifiedIntegrationEvent`.
* The provisioning handler receives the correlation ID from whichever verification request completed the flow.

**Note:** The correlation ID of the registration creation request and the correlation ID of the completing verification request are different. Registration and verification occur in separate HTTP requests and are linked by `RegistrationId`, not by the correlation ID. Following the full flow requires searching logs by `RegistrationId` across both correlation IDs.

---

## 12. Cross-Workflow Summary

### 12.1 Transaction Boundaries

| Workflow                    | Transaction                                     | Participants                                             |
| --------------------------- | ----------------------------------------------- | -------------------------------------------------------- |
| Registration creation       | Single                                          | Aggregate + 2 outbox rows.                               |
| Email verification          | Single                                          | Aggregate state + any domain-event-driven outbox writes. |
| Phone verification          | Single                                          | Same as email verification.                              |
| Resend                      | Single                                          | Aggregate state + 1 outbox row.                          |
| Full verification detection | Part of the verification workflow's transaction | Same as the corresponding verification workflow.         |
| Account provisioning        | Single                                          | Identity user + role assignment + registration removal.  |
| Cleanup                     | Single statement                                | Bulk delete.                                             |

### 12.2 Concurrency

| Concern                          | Mechanism                                                                       |
| -------------------------------- | ------------------------------------------------------------------------------- |
| Duplicate registration           | Unique indexes on email, phone, and username.                                   |
| Concurrent verification attempts | `RowVersion` on the aggregate.                                                  |
| Concurrent provisioning          | `IdempotentIntegrationEventHandler<T>` decorator.                               |
| Cleanup vs. active registration  | Cleanup filters on `ExpiresAtUtc < utcNow`; a non-expired row is never touched. |

### 12.3 Idempotency Summary

| Operation                                 | Idempotent? | Mechanism                                                          |
| ----------------------------------------- | ----------- | ------------------------------------------------------------------ |
| Register                                  | No          | Duplicate is rejected by the database constraint.                  |
| Verify — channel already verified         | Yes         | Handler checks `IsVerified` before processing.                     |
| Verify — invalid code, attempts remaining | No          | Each attempt counts.                                               |
| Resend                                    | No          | Each resend increments the counter; cooldown limits rapid retries. |
| Provision                                 | Yes         | Pipeline decorator + null-registration check.                      |
| Cleanup                                   | Yes         | Delete is naturally idempotent.                                    |

### 12.4 Failure Isolation

* A failed email delivery does not affect phone delivery; the two channels are independent through the Outbox.
* A failed verification attempt does not affect the other channel's attempt counter.
* A failed provisioning attempt does not affect the registration; the transaction rolls back and the Outbox retries.
* A failed cleanup does not affect registrations that are still active.

---

## 13. Testing Workflows

### 13.1 Registration Creation

* Register with valid input → HTTP 200 with registration ID; aggregate persisted; two outbox rows created.
* Register with duplicate email → HTTP 409.
* Register with malformed email → HTTP 400 before any database write.
* Register with weak password → HTTP 400.

### 13.2 Verification

* Verify email with the correct code → HTTP 204; `EmailVerifiedDomainEvent` raised.
* Verify email with an incorrect code (attempts 1–4) → HTTP 400; attempt counter incremented.
* Verify email with an incorrect code (attempt 5) → HTTP 400; registration marked expired; `RegistrationExpiredDomainEvent` raised.
* Verify an already-verified channel → HTTP 204 idempotently.
* Verify with an expired code → HTTP 400.

### 13.3 Resend

* Resend before 60 seconds → HTTP 400.
* Resend after 60 seconds → HTTP 204; new code generated; resend counter incremented.
* Resend after 3 successful resends → HTTP 400.
* Resend on a verified channel → HTTP 400.

### 13.4 Full Flow

* Register → verify email → verify phone → Outbox event → provisioning runs → user created with Customer role → registration removed.
* Duplicate delivery of `RegistrationFullyVerifiedIntegrationEvent` → user created once.

### 13.5 Cleanup

* Expired registration older than the schedule interval → removed.
* Active registration → not removed.
* Cleanup runs twice → second run is a no-op.

---

## 14. Conclusion

The Registration & Account Verification feature is defined by eight workflows:

1. **Registration creation** — aggregate + two outbox rows committed atomically.
2. **Email verification** — code checked in constant time; attempt counter maintained; completion detected.
3. **Phone verification** — same as email, with an independent channel.
4. **Resend** — cooldown and limit enforcement; old code invalidated.
5. **Full verification detection** — both channels verified → `RegistrationFullyVerifiedDomainEvent`.
6. **Account provisioning** — asynchronous, idempotent, and atomic across user creation, role assignment, and registration removal.
7. **Expiry** — derived from time for lifetime expiry; explicit for attempt exhaustion.
8. **Cleanup** — background job bulk-deletes expired registrations.

Each workflow has a single, well-defined transaction boundary. Idempotency is enforced at the level appropriate to each operation. Correlation IDs propagate from HTTP requests through the Outbox pipeline to asynchronous handlers. No plaintext password ever reaches the domain; OTP codes are hashed at rest and compared in constant time.

The feature is production-ready for a single-instance deployment. The known limitations—plaintext OTPs in the Outbox, no client-observable provisioning signal, and a daily default cleanup schedule—are documented in the Requirements and Technical Reference and are addressed through deployment configuration or explicit acceptance.

---

**Document Version:** 1.0 — Final

**Document Name:** Registration & Account Verification Workflow Technical Document

Defines eight end-to-end workflows covering the full lifecycle of a registration. Each workflow documents its trigger, flow, transaction boundary, failure modes, and idempotency. Cross-workflow sections summarize transaction boundaries, concurrency, idempotency, and correlation propagation.
