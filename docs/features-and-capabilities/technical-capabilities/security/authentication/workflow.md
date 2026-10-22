# Authentication — Workflow Documentation

**Project:** E-Commerce Modular Monolith
**Platform:** .NET 8 / ASP.NET Core Web API
**Scope:** Local Login, Refresh Token Rotation, Logout, Google Authentication, Google Account Linking
**Version:** 1.1 — Corrected

---

## 1. Overview

This document describes the complete end-to-end authentication workflows for the E-Commerce modular monolith.

It covers:

* Local login using email and password.
* Refresh token rotation with atomic concurrency protection.
* Logout and refresh token revocation.
* External authentication using Google OAuth.
* Optional Google account linking for existing local users.

The Authentication capability is implemented through an Application module and its Infrastructure counterpart:

* **Application:** `Application/Modules/Authentication`
* **Infrastructure:** `Infrastructure/Security/Authentication`

The module integrates with ASP.NET Core Identity, EF Core, JWT, Data Protection, and Google OAuth.

All authentication workflows issue the application's own JWT access tokens and opaque refresh tokens. Google authentication establishes an external identity; Google-issued tokens are not used as the application's access or refresh tokens.

Authentication does not require the Outbox or notification infrastructure for its core operations.

## 2. Actors and Components

### 2.1 Actors

| Actor                         | Responsibility                                                                              |
| ----------------------------- | ------------------------------------------------------------------------------------------- |
| Client / Browser              | Initiates authentication requests and receives authentication results.                      |
| User                          | Interacts with the client application.                                                      |
| API Controller                | Handles HTTP requests, initiates OAuth challenges, and dispatches application commands.     |
| MediatR Command Handler       | Orchestrates the application use case.                                                      |
| `IAuthenticationService`      | Application abstraction for authentication operations.                                      |
| `AuthenticationService`       | Infrastructure implementation that coordinates Identity, token generation, and persistence. |
| ASP.NET Core Identity         | Manages user lookup, password validation, lockout, and external login associations.         |
| EF Core / AppDbContext        | Persists refresh tokens and Identity data.                                                  |
| Google OAuth / OpenID Connect | Authenticates users through Google.                                                         |
| Data Protection               | Protects the linking user's identifier carried through the linking workflow.                |

### 2.2 Key Abstractions

| Abstraction               | Implementation               | Responsibility                                                             |
| ------------------------- | ---------------------------- | -------------------------------------------------------------------------- |
| `IAuthenticationService`  | `AuthenticationService`      | Local login, refresh, logout, external authentication, and Google linking. |
| `IUserLinkStateProtector` | `UserLinkStateProtector`     | Protects and unprotects the user identifier used during Google linking.    |
| MediatR commands          | Application command handlers | Expose authentication use cases through the Application layer.             |

### 2.3 Infrastructure Components

| Component                          | Responsibility                                                                      |
| ---------------------------------- | ----------------------------------------------------------------------------------- |
| `JwtTokenGenerator`                | Generates short-lived JWT access tokens.                                            |
| `RefreshTokenHasher`               | Hashes refresh tokens using HMAC-SHA256.                                            |
| `RefreshTokenRepository`           | Provides refresh-token persistence, atomic revocation, and token-family operations. |
| `RefreshTokenConfiguration`        | Defines EF Core mapping for refresh-token persistence.                              |
| `SecurityInfrastructureExtensions` | Registers authentication services, dependencies, and authentication schemes.        |

---

## 3. Workflow 1: Local Login

### 3.1 Entry Point

* **Endpoint:** `POST /api/auth/login`
* **Command:** `LoginCommand`
* **Handler:** `LoginCommandHandler`
* **Service:** `IAuthenticationService.LoginAsync`

### 3.2 Workflow

```text
Client
 |
 | POST /api/auth/login
 v
AuthenticationController
 |
 | Dispatch LoginCommand
 v
LoginCommandHandler
 |
 | Call IAuthenticationService.LoginAsync
 v
AuthenticationService
 |
 |-- Validate required input
 |-- Normalize the email address
 |-- Find user through ASP.NET Core Identity
 |-- Check AccountStatus == Active
 |-- Check Identity lockout status
 |-- Validate the password
 |
 |-- Invalid credentials
 |     |-- Update failed-access count as appropriate
 |     |-- Return generic authentication failure
 |
 |-- Valid credentials
       |-- Reset failed-access count as appropriate
       |-- Generate JWT access token
       |-- Generate opaque refresh token
       |-- Hash refresh token for storage
       |-- Persist refresh-token record
       |-- Return TokenPairDto
 |
 v
Client receives authentication result
```

### 3.3 Security Rules

1. Password validation must use ASP.NET Core Identity.
2. The application must not store plaintext passwords.
3. Deactivated accounts must not authenticate.
4. Identity lockout must be enforced.
5. Unknown users and incorrect passwords should produce equivalent public error responses to reduce account enumeration.
6. Raw refresh tokens must never be persisted or written to logs.
7. Account status and lockout checks must be applied consistently across local login, external login, and refresh operations.

### 3.4 Persistence and Failure Handling

The JWT is generated before the refresh token is persisted.

This ordering prevents a JWT-generation failure from leaving behind a newly persisted refresh token.

However, generating a JWT does not activate it on the client or guarantee that the client receives it. If refresh-token persistence fails after JWT generation, the generated access token may remain cryptographically valid until expiry, but the client has not received it.

No explicit transaction is required when the login operation performs only a single refresh-token persistence operation after token generation. If additional database writes are introduced, transaction requirements must be reassessed.

### 3.5 Error Handling

| Condition                         | Public Response                                             |
| --------------------------------- | ----------------------------------------------------------- |
| Missing or invalid credentials    | Generic invalid-credentials response                        |
| Unknown email                     | Same generic invalid-credentials response                   |
| Incorrect password                | Same generic invalid-credentials response                   |
| Deactivated account               | Account unavailable response, according to the API contract |
| Locked account                    | Account locked response, according to the API contract      |
| Unexpected infrastructure failure | Global exception handling                                   |

Avoid revealing whether an email address exists through differences in credential-validation responses. Account-status and lockout messages should be chosen deliberately because they can reveal account information.

---

## 4. Workflow 2: Refresh Token Rotation

### 4.1 Entry Point

* **Endpoint:** `POST /api/auth/refresh`
* **Command:** `RefreshTokenCommand`
* **Handler:** `RefreshTokenCommandHandler`
* **Service:** `IAuthenticationService.RefreshAsync`

### 4.2 Workflow

```text
Client
 |
 | POST /api/auth/refresh
 v
AuthenticationController
 |
 v
RefreshTokenCommandHandler
 |
 v
AuthenticationService.RefreshAsync
 |
 |-- Validate that the supplied token is not empty
 |-- Hash the raw token using HMAC-SHA256
 |-- Begin database transaction
 |
 |-- Atomically revoke the existing token:
 |     UPDATE ... WHERE
 |       TokenHash = @hash
 |       AND IsRevoked = 0
 |       AND ExpiresAtUtc > @now
 |
 |-- Exactly one row updated
 |     |
 |     |-- Load the revoked token
 |     |-- Load the associated user
 |     |-- Check account status and lockout
 |     |-- Create replacement in the same token family
 |     |-- Set ReplacedByTokenHash on the old token
 |     |-- Persist replacement token
 |     |-- Generate replacement JWT
 |     |-- Commit transaction
 |     |-- Return replacement token pair
 |
 |-- Zero rows updated
       |
       |-- Load existing token state by hash
       |
       |-- Token is revoked and has a replacement
       |     |-- Treat as a potential reuse attempt
       |     |-- Revoke the token family according to policy
       |     |-- Commit the family revocation
       |     |-- Return an authentication failure
       |
       |-- Token is missing, expired, or otherwise invalid
             |-- Roll back or complete the transaction
                 without unintended changes
             |-- Return an invalid-token response
```

### 4.3 Atomic Rotation

The conditional database update is the concurrency gate.

Only one concurrent request can successfully transition the same eligible token from unrevoked to revoked.

The replacement token must be persisted in the same database transaction as the revocation of the previous token. If the operation fails before commit, the transaction must roll back so the old token remains usable, provided no other transaction has successfully rotated it.

The implementation must ensure that the update and subsequent reads operate against a consistent and correctly scoped database transaction.

### 4.4 Token Families

Each refresh token belongs to a token family identified by `TokenFamilyId`.

When a token is rotated:

1. The old token is revoked.
2. A replacement token is created.
3. The replacement retains the original family identifier.
4. The old token records the replacement token's hash.
5. The transaction commits both changes atomically.

The family identifier allows the system to revoke related refresh tokens when a previously rotated token is presented again.

### 4.5 Reuse Detection

A failed conditional update does not, by itself, prove that token reuse occurred.

The service must inspect the persisted token state and distinguish among:

* A token that does not exist.
* An expired token.
* A token revoked through logout or administrative action.
* A token already rotated and associated with a replacement.

A revoked token with a replacement is evidence of a potential reuse attempt.

If reuse is detected, family revocation must be implemented atomically and safely under concurrent requests. The application should avoid returning sensitive information about token-family state to the client.

### 4.6 JWT Generation and Commit Ordering

Generate the replacement JWT before committing the refresh-token transaction.

This ensures that a JWT-generation failure can roll back the token rotation, leaving the old refresh token valid if the transaction has not committed.

However, this ordering cannot eliminate the following failure scenario:

1. The database transaction commits successfully.
2. The server loses the connection or fails before the response reaches the client.
3. The client still possesses the old, now-revoked refresh token.
4. The client retries and may trigger reuse detection.

This is a fundamental response-loss problem in token rotation. The system must define its retry and recovery policy rather than assuming that transaction atomicity guarantees successful delivery to the client.

### 4.7 Account-State Enforcement

Before issuing replacement credentials, verify:

* `AccountStatus == Active`.
* The user is not locked out according to the configured Identity policy.

If either check fails, the system must not issue new credentials.

The implementation must also define what happens to the existing token and transaction when account validation fails after the conditional update. It must not accidentally commit a token revocation or leave partial replacement state inconsistent with the intended security policy.

### 4.8 Error Handling

| Condition                                      | Expected Behavior                                                  |
| ---------------------------------------------- | ------------------------------------------------------------------ |
| Missing refresh token                          | Reject the request.                                                |
| Unknown token                                  | Reject as invalid.                                                 |
| Expired token                                  | Reject as invalid or expired.                                      |
| Previously revoked token without a replacement | Reject according to the revocation policy.                         |
| Previously rotated token presented again       | Apply the configured reuse-detection and family-revocation policy. |
| Deactivated or locked account                  | Do not issue new credentials.                                      |
| Database or infrastructure failure             | Roll back uncommitted changes and use global exception handling.   |

---

## 5. Workflow 3: Logout

### 5.1 Entry Point

* **Endpoint:** `POST /api/auth/logout`
* **Command:** `LogoutCommand`
* **Handler:** `LogoutCommandHandler`
* **Service:** `IAuthenticationService.LogoutAsync`

### 5.2 Workflow

```text
Client
 |
 | POST /api/auth/logout
 v
AuthenticationController
 |
 v
LogoutCommandHandler
 |
 v
AuthenticationService.LogoutAsync
 |
 |-- Validate supplied refresh token
 |-- Hash token
 |-- Find persisted token
 |
 |-- Token missing or already revoked
 |     |-- Return idempotent success
 |
 |-- Token exists and is eligible for revocation
       |-- Revoke token
       |-- Record revocation timestamp
       |-- Persist change
       |-- Handle concurrency conflicts safely
 |
 v
Return success
```

### 5.3 Idempotency

Logout must be idempotent.

Repeated requests using the same token should not produce inconsistent state or fail merely because the token has already been revoked.

Concurrent revocation attempts must be handled safely.

### 5.4 Access-Token Limitation

Revoking a refresh token does not automatically invalidate an already-issued JWT access token.

Unless the application implements an additional access-token revocation mechanism, an issued JWT generally remains usable until it expires or another validation rule rejects it.

The short access-token lifetime limits this exposure but does not eliminate it.

---

## 6. Workflow 4: External Authentication — Google Login

### 6.1 Entry Points

* **Initiation:** `GET /api/auth/external/google`
* **Callback:** `GET /api/auth/external/google/callback`

### 6.2 Workflow

```text
Client / Browser
 |
 | GET /api/auth/external/google
 v
AuthenticationController
 |
 | Initiate Google authentication challenge
 v
Google OAuth / OpenID Connect
 |
 | Authenticate user and return callback
 v
Authentication Middleware
 |
 | Validate OAuth state and correlation
 | Validate the external authentication response
 | Establish temporary external authentication cookie
 v
AuthenticationController
 |
 | Authenticate using ExternalCookieScheme
 | Verify successful authentication
 | Read Google subject from validated principal
 | Never accept the subject from client-supplied input
 | Dispatch ExternalAuthenticationCommand
 v
ExternalAuthenticationCommandHandler
 |
 v
AuthenticationService.ExternalAuthenticateAsync
 |
 |-- Validate provider and subject
 |-- Find user through FindByLoginAsync("Google", subject)
 |
 |-- No linked user
 |     |-- Reject authentication
 |
 |-- Linked user found
       |-- Check account status
       |-- Check lockout
       |-- Generate application JWT
       |-- Generate and persist refresh token
       |-- Return application token pair
 |
 v
Clear temporary external cookie
 |
 v
Return authentication result
```

### 6.3 Trust Boundary

The Google subject identifier must come only from the validated external `ClaimsPrincipal`.

The API must not accept a Google subject identifier from the request body, query string, or arbitrary headers as proof of identity.

The application must:

1. Validate the external authentication response.
2. Validate OAuth state and correlation.
3. Read the subject identifier from the validated principal.
4. Resolve the external login using the provider and subject identifier.
5. Verify account status and lockout.
6. Issue the application's own credentials only after successful validation.

### 6.4 External Cookie

The external authentication cookie is temporary and dedicated to the OAuth workflow.

Configuration should include:

* A dedicated scheme represented by `AuthenticationConstants.ExternalCookieScheme`.
* HTTP-only cookie behavior.
* Secure transport in production.
* A short lifetime, approximately five minutes.
* A SameSite configuration compatible with the chosen OAuth callback flow.
* Cleanup after callback processing, including failure paths.

The external cookie must not be used as the default authentication scheme for API requests.

OAuth state and correlation validation must remain enabled. A protected user identifier used during account linking does not replace those protections.

### 6.5 Account Provisioning Policy

Google login does not automatically create a new local account under the specified workflow.

If no local account is linked to the Google subject, the authentication operation is rejected with a controlled response directing the user to the supported registration or linking process.

Automatic account linking based solely on matching email addresses is not permitted.

---

## 7. Workflow 5: Google Account Linking

### 7.1 Entry Points

* **Initiation:** `GET /api/auth/link/google`
* **Callback:** `GET /api/auth/link/google/callback`

The initiation endpoint requires an authenticated application user.

### 7.2 Workflow

```text
Authenticated Client
 |
 | GET /api/auth/link/google
 | Authorization: Bearer <access-token>
 v
AuthenticationController
 |
 |-- Obtain current user ID from authenticated identity
 |-- Protect user ID through IUserLinkStateProtector
 |-- Store protected value in OAuth authentication properties
 |-- Initiate Google challenge
 v
Google OAuth / OpenID Connect
 |
 | Authenticate Google account
 | Return callback
 v
Authentication Middleware
 |
 | Validate OAuth state and correlation
 | Validate external authentication response
 v
AuthenticationController
 |
 |-- Authenticate using ExternalCookieScheme
 |-- Extract Google subject from validated principal
 |-- Retrieve protected user ID from authentication properties
 |-- Dispatch LinkGoogleForUserCommand
 v
LinkGoogleForUserCommandHandler
 |
 |-- Unprotect user ID using IUserLinkStateProtector
 |-- Reject invalid or tampered protected state
 |-- Call IAuthenticationService.LinkGoogleAsync
 v
AuthenticationService
 |
 |-- Find local user by ID
 |-- Check account status
 |-- Check whether the Google login is already associated
 |
 |-- Already linked to the same user
 |     |-- Return idempotent success
 |
 |-- Linked to another user
 |     |-- Reject linking
 |
 |-- Not linked
       |-- Create UserLoginInfo
       |-- Call UserManager.AddLoginAsync
       |-- Handle uniqueness conflicts safely
 |
 v
Clear temporary external cookie
 |
 v
Return linking result
```

### 7.3 Protected Linking State

The controller obtains the current user ID from the authenticated application identity and protects it using `IUserLinkStateProtector`.

The protected value is carried through the OAuth authentication properties and recovered by the application command handler.

The command handler is responsible for unprotecting and validating the protected value. The controller must not bypass this validation or accept a raw user ID supplied by the callback request.

Data Protection provides confidentiality and integrity protection for the protected payload. It does not replace OAuth state validation, correlation checks, or browser-session protections.

### 7.4 Linking Rules

1. Only an authenticated application user can initiate account linking.
2. The Google subject must originate from the validated external principal.
3. Email-address equality must not automatically link accounts.
4. A Google login already associated with the same user may be treated as idempotent success.
5. A Google login associated with a different user must be rejected.
6. A uniqueness constraint must enforce the external-login association at the database level.
7. The application must handle a concurrent linking attempt that violates the uniqueness constraint.
8. A database exception must not automatically be interpreted as a duplicate-link conflict. The implementation should identify the relevant uniqueness violation and handle unrelated persistence failures separately.
9. External authentication cookies must be cleared after callback processing, including unsuccessful paths.

### 7.5 Concurrency Handling

Two requests may attempt to link the same Google account concurrently.

An application-level lookup alone cannot prevent this race.

The database uniqueness constraint is the final concurrency safeguard. If one operation succeeds and another encounters a uniqueness violation, the application may perform a follow-up lookup:

* If the Google login belongs to the same user, return idempotent success.
* If it belongs to another user, reject the linking request.
* If the result cannot be determined because persistence is unavailable, propagate an appropriate infrastructure failure.

---

## 8. Cross-Cutting Concerns

### 8.1 Token Storage

| Token                       | Storage Policy                                                                 |
| --------------------------- | ------------------------------------------------------------------------------ |
| JWT access token            | Stateless token; not persisted by default.                                     |
| Raw refresh token           | Returned to the client; never persisted or logged.                             |
| Refresh-token hash          | Persisted in the database using HMAC-SHA256.                                   |
| Replacement-token reference | Stored as the replacement token's hash, where required by the rotation design. |

The HMAC secret must be protected independently from the database. It must not be hardcoded or exposed through logs or error messages.

### 8.2 Token Lifetimes

| Token                          | Default Lifetime        | Configuration                       |
| ------------------------------ | ----------------------- | ----------------------------------- |
| JWT access token               | 15 minutes              | `JwtSettings.ExpiryMinutes`         |
| Refresh token                  | 30 days                 | `RefreshTokenOptions.TokenLifetime` |
| External authentication cookie | Approximately 5 minutes | External cookie configuration       |

Actual configuration names must match the application's options classes and configuration binding.

### 8.3 Data Protection

Data Protection is registered through ASP.NET Core's Data Protection services and used by `UserLinkStateProtector`.

In a multi-instance deployment or when processes restart, the application must persist and share an appropriately protected Data Protection key ring when continuity of protected state is required.

Data Protection keys and related secrets must be managed securely.

### 8.4 Account-State Enforcement

All authentication operations that issue credentials must enforce:

1. `AccountStatus == Active`.
2. The configured ASP.NET Core Identity lockout policy.

ASP.NET Core Identity does not automatically enforce a custom `AccountStatus` property. The application must explicitly check it.

The system should define how deactivation affects existing refresh tokens. If deactivation must invalidate all sessions immediately, the application needs an explicit refresh-token revocation policy rather than relying solely on the next refresh attempt.

### 8.5 Secret and Configuration Management

The following must not be hardcoded:

* JWT signing keys.
* Refresh-token HMAC secrets.
* Google client secrets.
* Database credentials.
* Data Protection key-encryption credentials.

Use appropriate development secrets, environment configuration, or a production secret manager.

Startup validation should verify that required secrets are present and have the expected encoding and minimum key length.

---

## 9. Error Handling and HTTP Responses

HTTP status codes should reflect the API's documented error contract. The following is a recommended mapping, not a requirement to expose every internal condition.

| Scenario                            |                                                 Suggested HTTP Status | Notes                                                               |
| ----------------------------------- | --------------------------------------------------------------------: | ------------------------------------------------------------------- |
| Invalid local credentials           |                                                    `401 Unauthorized` | Use a generic response for unknown users and incorrect passwords.   |
| Deactivated account                 |      `403 Forbidden` or a deliberately generic authentication failure | Avoid unintended account enumeration.                               |
| Locked account                      |      `403 Forbidden` or a deliberately generic authentication failure | Apply the documented security policy.                               |
| Missing or malformed request fields |                                                     `400 Bad Request` | Use request validation.                                             |
| Invalid or expired refresh token    |                                                    `401 Unauthorized` | Do not disclose sensitive token state.                              |
| Detected refresh-token reuse        |                                                    `401 Unauthorized` | Revoke the family according to policy and require reauthentication. |
| Failed Google authentication        |             `401 Unauthorized` or a controlled OAuth failure response | Do not expose provider internals.                                   |
| No linked Google account            | `401 Unauthorized` or the documented external-authentication response | Do not create an account implicitly.                                |
| Invalid linking state               |              `400 Bad Request` or a controlled authentication failure | Clear temporary authentication state.                               |
| Google login linked to another user |                                                        `409 Conflict` | Indicates a resource-association conflict.                          |
| Unexpected infrastructure failure   |                                           `500 Internal Server Error` | Handle centrally and log safely.                                    |

Authentication responses must never include passwords, raw refresh tokens, signing secrets, or sensitive internal exception details.

---

## 10. Transaction Boundaries

| Operation              | Transaction Strategy                                                                                                                                   |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Local login            | Generate the JWT before refresh-token persistence. Use a transaction if multiple database writes must be atomic.                                       |
| Refresh token rotation | Use one database transaction for revoking the old token, creating the replacement, and recording replacement metadata. Generate the JWT before commit. |
| Logout                 | Use an atomic persistence operation or a transaction if the operation includes multiple writes. Handle concurrency safely.                             |
| Google login           | Follow the local-login token issuance and persistence strategy.                                                                                        |
| Google linking         | Use Identity's persistence operation and a database uniqueness constraint. Handle concurrent association attempts.                                     |

### 10.1 Transaction Invariants

**Refresh rotation must preserve these invariants:**

* The old token must not be committed as revoked unless the intended rotation or revocation operation succeeds.
* The replacement token must not be committed without the corresponding old-token state transition.
* A failed JWT-generation step before commit must roll back the rotation.
* Token-family revocation must not partially commit.
* Concurrent requests must not successfully rotate the same token more than once.

**Important limitation:** A database transaction cannot guarantee that the client receives a response after commit. The client retry policy must account for response loss after successful rotation.

---

## 11. Integration Points

| Component                     | Purpose                                                                     |
| ----------------------------- | --------------------------------------------------------------------------- |
| ASP.NET Core Identity         | Password validation, user lookup, lockout, and external-login associations. |
| EF Core                       | Refresh-token persistence and transactional database operations.            |
| Google OAuth / OpenID Connect | External authentication and identity validation.                            |
| Data Protection               | Protects linking state carried through the OAuth flow.                      |
| MediatR                       | Dispatches authentication use cases through Application handlers.           |
| JWT bearer authentication     | Validates application access tokens on protected API endpoints.             |
| Global exception handling     | Converts unexpected failures into controlled HTTP responses.                |

The core authentication workflows do not require the Outbox or notification infrastructure. Those capabilities may be used by separate workflows when authentication-related notifications or other durable side effects are explicitly required.

---

## 12. Operational and Testing Requirements

The implementation should verify the following behaviors.

### 12.1 Local Login

* Unknown email and incorrect password return equivalent public responses.
* Deactivated and locked accounts cannot obtain credentials.
* Password verification is delegated to ASP.NET Core Identity.
* A refresh-token persistence failure does not leave a partially committed token record.

### 12.2 Refresh Token Rotation

* Only one concurrent request can rotate a given eligible refresh token.
* Failed transactions leave no partially committed replacement state.
* Expired and unknown tokens are rejected.
* Reuse detection distinguishes previously rotated tokens from other invalid tokens.
* Token-family revocation is concurrency-safe.
* Account deactivation prevents issuance of new credentials.
* Response loss after commit is handled according to the documented client recovery policy.

### 12.3 Logout

* Repeated logout requests are idempotent.
* Concurrent revocation does not produce inconsistent state.
* The application does not claim that refresh-token revocation immediately invalidates existing JWTs unless an additional mechanism provides that behavior.

### 12.4 Google Authentication and Linking

* Invalid OAuth state or correlation is rejected.
* Google subject identifiers come only from validated external principals.
* Unlinked Google accounts do not automatically create local accounts.
* Linking cannot be initiated without an authenticated application user.
* Tampered or invalid protected linking state is rejected.
* Concurrent linking attempts cannot associate one external login with multiple users.
* Temporary external authentication cookies are cleared on success and failure paths.

### 12.5 Infrastructure and Configuration

* Required signing and hashing secrets are validated at startup.
* Sensitive credentials are never written to logs.
* Database concurrency behavior is tested against the actual database provider used in production.
* Data Protection keys are available across instances and restarts where required.

---

## 13. Conclusion

The Authentication module provides local login, refresh-token rotation, logout, Google authentication, and optional Google account linking within the E-Commerce modular monolith.

Its design keeps authentication orchestration in the Application layer and Identity, token generation, OAuth integration, and persistence implementations in Infrastructure.

The principal security guarantees depend on explicit implementation details: atomic refresh-token rotation, transactional replacement creation, database-enforced external-login uniqueness, OAuth state and correlation validation, account-state enforcement, and secure secret management.

These guarantees must be verified through implementation and integration testing. Documentation alone does not establish that a workflow is secure or concurrency-safe.

**Document version:** 1.1 — Corrected.
