\# Registration \& Account Verification — Technical Requirements Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Scope:\*\* Onboarding — registration creation, email and phone verification, account provisioning

\*\*Version:\*\* 1.0 — Final

\*\*Related documents:\*\* Registration Domain Model Technical Reference, Registration Technical Reference, Registration Workflow, Notifications Technical Reference, Outbox Technical Requirements



\---



\## 1. Introduction



This document defines the technical requirements for the Registration \& Account Verification feature—the entry point through which a new user becomes an authenticated member of the system with credentials and a role.



The requirements establish \*\*what must be true\*\* of the feature. Implementation choices (aggregate design, value objects, storage strategy, notification transport, and provisioning mechanism) are described in the related Technical References; this document establishes obligations.



Requirements are numbered for traceability. Each is classified as \*\*Must\*\*, \*\*Should\*\*, or \*\*May\*\*.



\---



\## 2. Scope



\### 2.1 In Scope



\* Creation of a pending registration from user-supplied email, phone, username, and password.

\* Delivery of one-time verification codes to email and phone.

\* Verification of email and phone channels via submitted codes.

\* Resend of verification codes with cooldown and count limits.

\* Enforcement of attempt limits, resend limits, OTP expiry, and registration lifetime.

\* Prevention of duplicate active registrations for the same email, phone, or username.

\* Completion detection when both channels are verified.

\* Provisioning of an ASP.NET Core Identity user upon full verification.

\* Assignment of the Customer role to newly provisioned users.

\* Cleanup of expired or abandoned registrations.

\* Idempotency across the entire flow.

\* Correlation ID propagation from registration request to account provisioning.



\### 2.2 Out of Scope



\* \*\*Authentication after provisioning\*\* — login, refresh, logout, and external providers. These belong to the Authentication module.

\* \*\*Post-registration account management\*\* — password change, email change, and account lock/unlock. These belong to the Identity module.

\* \*\*Person record creation\*\* — the `Person` aggregate is created on demand by the People bounded context when the user needs personal-data-dependent actions. It is not created during registration.

\* \*\*Notification transport details\*\* — SMTP, Twilio, and Firebase mechanics are covered by the Notifications Technical Reference.

\* \*\*Rate limiting at the API layer\*\* — registration endpoints are subject to API-layer rate-limiting policies, but the policies themselves are not defined here.

\* \*\*Marketing or promotional opt-in\*\* — registration creates the account; preferences are managed separately.

\* \*\*Multi-factor authentication enrollment\*\* — beyond email and phone verification at registration time.

\* \*\*Account recovery flows\*\* — password reset is a separate feature under the Identity module.



\---



\## 3. Business Responsibility



The Registration \& Account Verification feature answers the question:



\*\*How does an unauthenticated individual provide verifiable contact information, prove control of that information, and receive a usable authenticated account?\*\*



It is responsible for:



\* Accepting a registration request with email, phone, username, and password.

\* Ensuring both email and phone are verifiably controlled by the requester before an account is created.

\* Enforcing limits that prevent abuse of the verification process.

\* Creating the account once verification is complete.

\* Assigning the default role to the new account.



It is \*\*not\*\* responsible for authenticating the user after the account exists. That begins when provisioning completes and the user calls a login endpoint.



\---



\## 4. Functional Requirements



\### 4.1 Registration Creation



\*\*FR-1 (Must)\*\* — The system \*\*must\*\* accept a registration request containing email, phone number, username, and password.



\*\*FR-2 (Must)\*\* — The system \*\*must\*\* validate that email, phone number, and username are structurally well-formed before creating a registration. Malformed input \*\*must\*\* be rejected with a validation failure.



\*\*FR-3 (Must)\*\* — The system \*\*must\*\* hash the password before passing it to the Domain. The Domain \*\*must\*\* never receive a plaintext password.



\*\*FR-4 (Must)\*\* — The system \*\*must\*\* create a `Registration` aggregate with a lifetime of 15 minutes from creation. The lifetime \*\*must\*\* be sourced from `RegistrationConstants.DefaultLifetime`, not hardcoded.



\*\*FR-5 (Must)\*\* — The system \*\*must\*\* reject a registration request when an active registration already exists for the same email, phone, or username.



\*\*FR-6 (Must)\*\* — Uniqueness enforcement \*\*must\*\* be provided by a database-level constraint on the email, phone, and username columns. Application-level checks may exist as fast paths, but the constraint is authoritative.



\*\*FR-7 (Must)\*\* — On successful registration creation, the system \*\*must\*\* generate one verification code for email and one for phone. The codes \*\*must\*\* be independent.



\*\*FR-8 (Must)\*\* — Each verification code \*\*must\*\* have a lifetime of 5 minutes from generation. The lifetime \*\*must\*\* be sourced from `RegistrationConstants.OTPLifetime`.



\*\*FR-9 (Must)\*\* — Verification codes \*\*must\*\* be generated using a cryptographically secure random source. Predictable codes derived from timestamps, sequences, or user data are not permitted.



\*\*FR-10 (Must)\*\* — Verification codes \*\*must\*\* be hashed before being persisted in the aggregate. The plaintext code \*\*must not\*\* be stored in the Domain.



\*\*FR-11 (Must)\*\* — The plaintext code \*\*must\*\* be included in the integration event payload that triggers notification delivery because the notification handler needs it to compose the message.



\*\*FR-12 (Must)\*\* — The registration creation transaction \*\*must\*\* persist the aggregate and the two verification-requested integration events atomically via the Transactional Outbox.



\*\*FR-13 (Must)\*\* — The registration \*\*must\*\* raise a domain event when it is created. The system \*\*may\*\* use the domain event to trigger notification delivery via integration events. The current implementation writes the integration events directly from the command handler rather than from a domain event handler; either approach is acceptable provided FR-12 is satisfied.



\### 4.2 Verification Code Delivery



\*\*FR-14 (Must)\*\* — The email verification code \*\*must\*\* be delivered via the Notifications subsystem's email channel.



\*\*FR-15 (Must)\*\* — The phone verification code \*\*must\*\* be delivered via the Notifications subsystem's SMS channel.



\*\*FR-16 (Must)\*\* — Delivery \*\*must\*\* be asynchronous. The registration transaction \*\*must not\*\* block on SMTP or SMS provider calls.



\*\*FR-17 (Must)\*\* — Delivery \*\*must\*\* be durable. If the notification transport fails transiently, code delivery \*\*must\*\* be retried according to the Outbox pipeline's retry policy.



\*\*FR-18 (Must)\*\* — Email and phone verification requests \*\*must\*\* be separate integration events, not a single combined event. Each notification handler \*\*must\*\* receive only the code it needs.



\*\*FR-19 (Must)\*\* — Each integration event \*\*must\*\* carry the recipient's contact information and the plaintext code. It \*\*must not\*\* carry data unrelated to its specific channel.



\*\*FR-20 (Must)\*\* — The verification code \*\*must not\*\* be logged by the notification pipeline. The rendered template content \*\*must not\*\* be persisted in the notification log.



\### 4.3 Verification



\*\*FR-21 (Must)\*\* — The system \*\*must\*\* accept a submitted code for a given channel (email or phone) and a given registration.



\*\*FR-22 (Must)\*\* — Verification \*\*must\*\* reject a submitted code when:



\* The registration is expired.

\* The registration is already completed.

\* The channel is already verified.

\* The code has expired.

\* The submitted code does not match the stored hash.

\* The attempt limit has been reached.



\*\*FR-23 (Must)\*\* — Verification \*\*must\*\* succeed only when the submitted code matches the stored hash for the channel and the code has not expired.



\*\*FR-24 (Must)\*\* — Code comparison \*\*must\*\* use constant-time comparison to prevent timing attacks. Using `==` on hash strings is not permitted.



\*\*FR-25 (Must)\*\* — Each failed verification attempt \*\*must\*\* increment the channel's attempt counter.



\*\*FR-26 (Must)\*\* — When a channel's attempt count reaches `RegistrationConstants.MaxAttemptsPerChannel` (5), the registration \*\*must\*\* be marked expired and \*\*must\*\* raise `RegistrationExpiredDomainEvent`.



\*\*FR-27 (Must)\*\* — Successful verification \*\*must\*\* clear the channel's code hash and set the channel's verified timestamp. The submitted code \*\*must not\*\* be usable again for that channel.



\*\*FR-28 (Must)\*\* — Successful verification \*\*must\*\* raise `EmailVerifiedDomainEvent` or `PhoneVerifiedDomainEvent`, as appropriate.



\*\*FR-29 (Must)\*\* — When both channels become verified, the registration \*\*must\*\* set `RegistrationCompleted = true` and \*\*must\*\* raise `RegistrationFullyVerifiedDomainEvent`.



\*\*FR-30 (Must)\*\* — An attempt to verify an already-verified channel \*\*must\*\* be treated as idempotent success at the Application boundary. The aggregate \*\*must\*\* defend against a second verification internally, but the handler \*\*must\*\* check `IsVerified` first and return success without invoking the aggregate.



\*\*FR-31 (Must)\*\* — Verification \*\*must not\*\* be allowed after the registration has expired. The expiry check \*\*must\*\* occur before any verification logic.



\### 4.4 Resend



\*\*FR-32 (Must)\*\* — The system \*\*must\*\* allow a user to request a new verification code for a channel that has not yet been verified.



\*\*FR-33 (Must)\*\* — A resend \*\*must\*\* be rejected when:



\* The registration is expired.

\* The registration is completed.

\* The channel is already verified.

\* The resend count has reached `RegistrationConstants.MaxResendsPerChannel` (3).

\* The cooldown specified by `RegistrationConstants.ResendCooldown` (60 seconds) has not elapsed since the last send.



\*\*FR-34 (Must)\*\* — A successful resend \*\*must\*\* generate a new code, replace the existing code hash and expiry for that channel, and increment the resend counter.



\*\*FR-35 (Must)\*\* — A successful resend \*\*must\*\* produce a new integration event to deliver the new code. The previous code \*\*must\*\* be invalidated.



\*\*FR-36 (Must)\*\* — The resend cooldown and attempt counter \*\*must\*\* be independent. A resend \*\*must not\*\* reset the attempt counter, and an attempt \*\*must not\*\* count toward the resend limit.



\### 4.5 Registration Expiration and Cleanup



\*\*FR-37 (Must)\*\* — A registration \*\*must\*\* be considered expired when the current UTC time exceeds `ExpiresAtUtc` or when the aggregate is explicitly marked expired due to attempt exhaustion.



\*\*FR-38 (Must)\*\* — Any public operation on an expired registration \*\*must\*\* throw or fail with a domain exception. Silent no-ops on expired registrations are not permitted.



\*\*FR-39 (Must)\*\* — The system \*\*must\*\* provide a mechanism to physically remove expired registrations from the database.



\*\*FR-40 (Must)\*\* — The cleanup mechanism \*\*must\*\* be a background job scheduled at a frequency shorter than the registration lifetime. The default daily schedule is \*\*not\*\* acceptable for production. The deployment \*\*must\*\* override it so that expired registrations do not block new registrations for more than one hour.



\*\*FR-41 (Should)\*\* — Cleanup \*\*should\*\* use a bulk-delete operation rather than loading each expired registration into memory before deletion.



\*\*FR-42 (Must)\*\* — Cleanup \*\*must not\*\* remove registrations that have not yet expired.



\### 4.6 Account Provisioning



\*\*FR-43 (Must)\*\* — Provisioning \*\*must\*\* occur only after `RegistrationFullyVerifiedDomainEvent` is raised and translated into an integration event.



\*\*FR-44 (Must)\*\* — Provisioning \*\*must\*\* be triggered by an integration event handler, not by a domain event handler. The verification transaction \*\*must not\*\* perform provisioning directly.



\*\*FR-45 (Must)\*\* — Provisioning \*\*must\*\* create an ASP.NET Core Identity user with the email, phone number, and username from the registration.



\*\*FR-46 (Must)\*\* — Provisioning \*\*must\*\* use the password hash stored on the registration. The system \*\*must not\*\* re-hash the password at provisioning time.



\*\*FR-47 (Must)\*\* — Provisioning \*\*must\*\* assign the Customer role to the newly created Identity user.



\*\*FR-48 (Must)\*\* — Provisioning \*\*must\*\* remove the registration after the user is created and the role is assigned.



\*\*FR-49 (Must)\*\* — All provisioning operations—user creation, role assignment, and registration removal—\*\*must\*\* be staged in the same persistence context and committed as a single transaction. Partial commits are not permitted.



\*\*FR-50 (Must)\*\* — The provisioning handler \*\*must\*\* be idempotent. Re-execution of the same integration event \*\*must not\*\* create a duplicate Identity user or leave the system in an inconsistent state.



\*\*FR-51 (Must)\*\* — If the registration is not found when the provisioning handler runs, the handler \*\*must\*\* treat the absence as a completed-or-removed condition and return without error.



\*\*FR-52 (Must)\*\* — The role assigned at provisioning \*\*must\*\* be sourced from a shared constant, not a string literal. The same constant \*\*must\*\* be used by the role-seeding mechanism.



\*\*FR-53 (Should)\*\* — The provisioning handler \*\*should\*\* log successful provisioning with the registration ID and the new user ID.



\### 4.7 Idempotency



\*\*FR-54 (Must)\*\* — Every command in the registration flow \*\*must\*\* be safe to retry. Retrying a verification or resend command \*\*must not\*\* produce a duplicate registration, a duplicate code, or a duplicate user.



\*\*FR-55 (Must)\*\* — Idempotency for integration event handlers \*\*must\*\* be provided by the pipeline's per-handler decorator. Handlers \*\*must not\*\* implement their own idempotency logic.



\*\*FR-56 (Must)\*\* — Idempotency for database-level uniqueness (email, phone, username) \*\*must\*\* be enforced by constraints, not by application-level checks alone.



\### 4.8 Correlation



\*\*FR-57 (Must)\*\* — The correlation ID from the registration HTTP request \*\*must\*\* propagate into:



\* The `RegistrationFullyVerifiedIntegrationEvent` payload.

\* The provisioning handler's log scope.

\* Every log entry produced during provisioning.



\*\*FR-58 (Must)\*\* — Correlation propagation \*\*must\*\* be automatic via the pipeline's correlation decorator. Handlers \*\*must not\*\* manage correlation IDs manually.



\---



\## 5. Non-Functional Requirements



\### 5.1 Security



\*\*NFR-1\*\* — Verification codes \*\*must\*\* be generated with sufficient entropy to resist brute-force guessing within their five-minute lifetime. A six-digit numeric code combined with a five-attempt limit and five-minute expiry is sufficient for the current design.



\*\*NFR-2\*\* — Code comparison \*\*must\*\* be constant-time. Timing side channels are not permitted.



\*\*NFR-3\*\* — Plaintext verification codes \*\*must not\*\* appear in:



\* The Domain aggregate.

\* Server logs.

\* The notification log entity.

\* Error responses returned to the client.



\*\*NFR-4\*\* — Plaintext codes \*\*do\*\* appear in the Outbox payload that triggers delivery. This is intentional because the notification handler needs the code, but the exposure \*\*must\*\* be bounded:



\* The payload \*\*should\*\* be encrypted at rest if column-level or database-level encryption is available.

\* The Outbox and dead-letter tables \*\*should\*\* have a retention policy that removes rows containing verification codes once they are no longer needed.

\* Operators with database access to the Outbox table \*\*must\*\* be treated as having access to plaintext OTPs.



\*\*NFR-5\*\* — Passwords \*\*must\*\* be hashed with a password-hashing function suitable for authentication, such as ASP.NET Core Identity's default hasher or an equivalent. Fast hashes, such as SHA-256 without key stretching, are not permitted for password storage.



\*\*NFR-6\*\* — Registration endpoints \*\*must\*\* be rate-limited to prevent enumeration and abuse. Rate limits are configured at the API layer, but the registration feature \*\*must\*\* be compatible with them.



\*\*NFR-7\*\* — The registration flow \*\*must not\*\* reveal whether a given email, phone, or username is already registered. Duplicate rejection \*\*should\*\* return a generic error at the API boundary.



\### 5.2 Reliability



\*\*NFR-8\*\* — Registration creation \*\*must\*\* be atomic with its Outbox writes. A crash after commit \*\*must\*\* leave the registration and verification-requested events persisted.



\*\*NFR-9\*\* — A notification transport failure \*\*must not\*\* cause the registration to be lost. The Outbox retries delivery.



\*\*NFR-10\*\* — Account provisioning \*\*must\*\* be atomic with role assignment and registration removal. A failure in any step \*\*must\*\* leave the registration intact for retry.



\*\*NFR-11\*\* — The provisioning step \*\*must\*\* be idempotent under retry. Re-running the same integration event \*\*must not\*\* produce duplicate users.



\*\*NFR-12\*\* — The registration flow \*\*must not\*\* produce orphaned Identity users. A user \*\*must not\*\* be created without a role, and a registration \*\*must not\*\* be removed without corresponding user creation.



\### 5.3 Performance



\*\*NFR-13\*\* — Registration creation \*\*must not\*\* block on code delivery. The response to the client is returned after the transaction commits, not after the notification is sent.



\*\*NFR-14\*\* — Verification \*\*must\*\* require a single database round-trip to load the registration, plus one to save the update.



\*\*NFR-15\*\* — Lookups by email, phone, and username \*\*must\*\* be supported by indexes.



\*\*NFR-16\*\* — The cleanup job \*\*must\*\* complete in bounded time even when the expired registration count is large. Bulk deletion is the expected implementation.



\### 5.4 Consistency



\*\*NFR-17\*\* — The registration flow is \*\*eventually consistent\*\* between verification completion and account provisioning. Provisioning occurs asynchronously.



\*\*NFR-18\*\* — The client \*\*must\*\* be able to determine when provisioning completes. The system \*\*must\*\* provide either a polling mechanism or an event-based notification. Purely fire-and-forget provisioning without any client-observable signal is not acceptable.



\*\*NFR-19\*\* — Between registration completion and provisioning, the user's credentials \*\*must not\*\* be accepted for login. The Identity user does not exist yet; login attempts before provisioning completes \*\*must\*\* fail cleanly.



\### 5.5 Maintainability



\*\*NFR-20\*\* — The Domain aggregate \*\*must not\*\* reference infrastructure concerns: no ORM, hashing, email/SMS providers, or HTTP.



\*\*NFR-21\*\* — Business constants (lifetimes, limits, cooldowns) \*\*must\*\* be centralized in `RegistrationConstants`. Magic numbers in the aggregate are not permitted.



\*\*NFR-22\*\* — Adding a new verification channel, such as push-based OTP, \*\*should\*\* require changes only in the Application and Infrastructure layers, not in the Domain aggregate's core invariants.



\*\*NFR-23\*\* — The provisioning handler \*\*must not\*\* depend on EF Core types or `DbContext` directly if the transaction is managed by `IUnitOfWork`.



\### 5.6 Testability



\*\*NFR-24\*\* — The `Registration` aggregate \*\*must\*\* be unit-testable without a database. All behaviors and invariants \*\*must\*\* be exercisable in memory.



\*\*NFR-25\*\* — Verification code hashing and comparison \*\*must\*\* be behind an abstraction that can be replaced in tests.



\*\*NFR-26\*\* — Time-dependent behaviors (expiry and cooldown) \*\*must\*\* be testable with a controllable clock.



\---



\## 6. Domain Rules and Invariants



\*\*DR-1\*\* — A registration is active for exactly 15 minutes from creation. After that, no verification or resend succeeds, and the record is eligible for cleanup.



\*\*DR-2\*\* — A verification code is valid for exactly five minutes from generation. An expired code is rejected even if its hash matches.



\*\*DR-3\*\* — A channel can be verified at most once. A second verification of the same channel is rejected by the aggregate and treated as idempotent success at the Application boundary.



\*\*DR-4\*\* — Attempt exhaustion expires the registration. This is distinct from lifetime expiry but produces the same observable outcome.



\*\*DR-5\*\* — Resend and attempt limits are independent counters per channel.



\*\*DR-6\*\* — A resend invalidates the previous code for that channel. The old code is no longer accepted.



\*\*DR-7\*\* — The registration is removed after provisioning. It is not retained as a historical record.



\*\*DR-8\*\* — The Identity user created at provisioning has no `Person` record. The `Person` is created later when the user takes a personal-data-dependent action.



\*\*DR-9\*\* — The Customer role is the sole role assigned at provisioning. Additional roles may be assigned later through administrative action.



\*\*DR-10\*\* — A registration completed by successful verification \*\*must\*\* produce exactly one provisioning run. Duplicate delivery of the completion event \*\*must not\*\* produce a second user.



\---



\## 7. Constraints



\*\*C-1\*\* — Registration is part of the modular monolith. It shares the database and process with the rest of the system.



\*\*C-2\*\* — Notification delivery depends on the Outbox pipeline. Synchronously delivering codes from the registration transaction is not permitted.



\*\*C-3\*\* — Account provisioning depends on the Outbox pipeline. Provisioning synchronously from the verification transaction is not permitted.



\*\*C-4\*\* — The registration's uniqueness constraints are on plain unique indexes, not filtered. An expired-but-not-yet-cleaned registration blocks a new registration for the same email, phone, or username.



\*\*C-5\*\* — The cleanup schedule determines the maximum blocking window. A daily schedule is technically valid but operationally unacceptable; the deployment must override it.



\*\*C-6\*\* — The registration feature does not attempt to prevent an attacker from registering many different emails. That is a separate abuse-prevention concern addressed at the API layer through rate limiting.



\*\*C-7\*\* — Provisioning does not verify the Identity user's email or phone again. The registration's verification is the authoritative proof of ownership.



\---



\## 8. Assumptions



\*\*A-1\*\* — The Notifications subsystem is operational and provides the email and SMS channels.



\*\*A-2\*\* — The Outbox pipeline is operational and delivers integration events reliably.



\*\*A-3\*\* — The role-seeding mechanism runs at application startup and creates the Customer role before any provisioning occurs.



\*\*A-4\*\* — The Identity service (`IIdentityService`) throws on user-creation failure rather than returning a sentinel value. If it returns `Guid.Empty` on failure, the provisioning handler must validate the return.



\*\*A-5\*\* — The Identity store and registration repository share the same `AppDbContext`, allowing both writes to be staged in a single transaction.



\*\*A-6\*\* — The application runs as a single instance or accepts eventual consistency across instances. Multi-instance registration does not require distributed coordination beyond the database.



\*\*A-7\*\* — The password-hashing algorithm is chosen and configured outside the registration feature, in the Application or Infrastructure layer. The registration Domain treats the hash as an opaque value.



\*\*A-8\*\* — Clients retry verification or resend on transient failures. The feature does not guarantee delivery of the verification code to the user's actual inbox; it guarantees only that the code is dispatched.



\---



\## 9. Acceptance Criteria



The Registration \& Account Verification feature is considered complete when:



1\. \*\*AC-1\*\* — A registration request with valid email, phone, username, and password creates a `Registration` aggregate and produces two Outbox events (email and phone).

2\. \*\*AC-2\*\* — The email and phone Outbox events are delivered asynchronously to the Notifications subsystem.

3\. \*\*AC-3\*\* — A verification request with a correct code verifies the channel and marks it as verified.

4\. \*\*AC-4\*\* — A verification request with an incorrect code increments the attempt counter and fails.

5\. \*\*AC-5\*\* — After five failed attempts, the registration is marked expired and cannot be verified further.

6\. \*\*AC-6\*\* — A resend before the 60-second cooldown is rejected.

7\. \*\*AC-7\*\* — A resend after three successful resends is rejected.

8\. \*\*AC-8\*\* — A resend invalidates the previous code.

9\. \*\*AC-9\*\* — An expired code is rejected even when its hash matches.

10\. \*\*AC-10\*\* — Verifying both channels raises `RegistrationFullyVerifiedDomainEvent` and marks the registration completed.

11\. \*\*AC-11\*\* — Attempting to verify an already-verified channel returns success idempotently.

12\. \*\*AC-12\*\* — A duplicate registration for the same email, phone, or username is rejected at the database-constraint level.

13\. \*\*AC-13\*\* — Full verification produces an Outbox event that triggers `CreateUserOnRegistrationVerifiedHandler`.

14\. \*\*AC-14\*\* — Provisioning creates an Identity user with the registration's email, phone, username, and password hash.

15\. \*\*AC-15\*\* — Provisioning assigns the Customer role to the new user.

16\. \*\*AC-16\*\* — Provisioning removes the registration record.

17\. \*\*AC-17\*\* — User creation, role assignment, and registration removal commit atomically. A failure in any step leaves the registration available for retry.

18\. \*\*AC-18\*\* — Redelivery of the provisioning event does not create a duplicate user.

19\. \*\*AC-19\*\* — Provisioning logs a success entry with registration and user IDs.

20\. \*\*AC-20\*\* — The correlation ID from the original registration request appears in the provisioning handler's log entries.

21\. \*\*AC-21\*\* — The cleanup job removes expired registrations within the configured frequency.

22\. \*\*AC-22\*\* — The cleanup job uses a bulk-delete operation and completes in bounded time.

23\. \*\*AC-23\*\* — The Customer role is created by the seeder before the first provisioning run.

24\. \*\*AC-24\*\* — The registration flow is compatible with API-layer rate limiting without special handling.

25\. \*\*AC-25\*\* — Verification code comparison uses constant-time comparison. Timing attacks are not viable.

26\. \*\*AC-26\*\* — The `Registration` aggregate can be unit-tested in isolation without infrastructure dependencies.



\---



\## 10. Traceability Matrix



| Requirement Area                   | Description                                                                          | Related Document                                        |

| ---------------------------------- | ------------------------------------------------------------------------------------ | ------------------------------------------------------- |

| Aggregate structure and invariants | Rich aggregate, private setters, guards                                              | Registration Domain Model Technical Reference           |

| Business constants                 | Lifetimes, limits, cooldowns                                                         | Domain Model §4                                         |

| Value objects                      | `Email`, `PhoneNumber`, `Username`, `CredentialHash`, `VerificationChannel`          | Domain Model §3                                         |

| Domain events                      | `EmailVerified`, `PhoneVerified`, `RegistrationFullyVerified`, `RegistrationExpired` | Domain Model §7                                         |

| OTP delivery via Outbox            | Split integration events, atomic writes                                              | Outbox Technical Reference                              |

| Verification code hashing          | HMAC-based hashing, constant-time comparison                                         | Registration Technical Reference                        |

| Unique constraints                 | Plain indexes on email, phone, and username                                          | Registration Technical Reference                        |

| Provisioning via integration event | Decoupled, retryable, idempotent                                                     | Registration Workflow; Domain Model §7                  |

| Role assignment                    | Customer role via `IRoleManagementService`                                           | Registration Workflow                                   |

| Cleanup job                        | Background job, bulk delete, deployment override                                     | Registration Technical Reference                        |

| Idempotency                        | Handler decorator for integration events; constraints for uniqueness                 | End-to-End Outbox Dispatching + Per-Handler Idempotency |

| Correlation propagation            | Request → integration event → handler log scope                                      | Outbox Technical Reference §6                           |

| Password hashing                   | Hashing outside the Domain; hash passed in                                           | Registration Technical Reference                        |



\---



\## 11. Summary



The Registration \& Account Verification feature provides one guarantee:



\*\*Any individual who provides a well-formed email, phone, username, and password can prove control of the email and phone within a bounded lifetime and, upon doing so, receive an Identity account with the Customer role—without exposing a plaintext password or verification code to the Domain or logs.\*\*



That guarantee rests on six pillars:



1\. \*\*Registration as a bounded-lifetime aggregate\*\* — the state machine and its invariants are enforced at a single point.

2\. \*\*Durable, asynchronous code delivery\*\* — the Outbox pipeline uses separate email and phone events without blocking the registration transaction.

3\. \*\*Bounded verification\*\* — attempt limits, resend limits, expiry, and cooldown are enforced per channel, with codes hashed at rest and compared in constant time.

4\. \*\*Asynchronous provisioning\*\* — separated from the verification transaction, retryable, idempotent, and atomic across user creation, role assignment, and registration removal.

5\. \*\*Cleanup\*\* — expired registrations are removed by a background job at a frequency that does not block legitimate re-registration.

6\. \*\*Idempotency and correlation\*\* — the flow is safe to retry at every step, and the request's correlation ID follows the user through to account provisioning.



Every requirement in this document is expressed as an obligation the code must satisfy. The current implementation satisfies the vast majority; requirements classified as \*\*Should\*\* reflect operational maturity that can be added as needed. One requirement—\*\*NFR-18, provisioning observability\*\*—is worth confirming against the actual implementation because it determines what the client can rely on.



\---



\*\*Document version:\*\* 1.0 — Final Registration \& Account Verification Technical Requirements.



This document defines the behavioral and structural obligations of the onboarding feature across creation, verification, resend, expiry, cleanup, provisioning, and role assignment. Requirements are grouped by area, classified as Must/Should/May, and supported by a traceability matrix. Implementation details are deferred to the Registration Technical Reference, Workflow, and Domain Model documents.



