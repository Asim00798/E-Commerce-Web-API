\# Authentication Feature — Requirements Technical Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Scope:\*\* Functional, Non-Functional, Security, and Integration Requirements

\*\*Version:\*\* 1.1 — Corrected



\---



\## 1. Introduction



This document defines the technical requirements for the Authentication feature of the E-Commerce modular monolith. It serves as the specification for design, development, testing, and acceptance.



The Authentication feature handles user authentication \*\*after account creation\*\*. It encompasses local login, refresh-token lifecycle management, logout, external authentication through Google, and optional Google account linking for existing local users.



The feature is implemented as an Application module under `Application/Modules/Authentication`, with its Infrastructure implementation under `Infrastructure/Security/Authentication`, following the solution's Clean Architecture principles.



\---



\## 2. Scope



\### 2.1 In Scope



\* Local login using email and password.

\* Refresh-token rotation with concurrency protection and reuse detection.

\* Logout through refresh-token revocation.

\* External authentication through Google using ASP.NET Core's official Google authentication handler.

\* Optional Google linking for an already-authenticated local user.

\* Short-lived JWT access-token generation and opaque refresh-token generation.

\* Refresh-token hashing and persistence.

\* Account-status and lockout enforcement during authentication.

\* Protected OAuth state for Google account linking.

\* Application-specific token issuance after successful local or external authentication.



\### 2.2 Out of Scope



\* Registration and onboarding, which are handled by the Registration feature.

\* Password reset and email confirmation, which are handled by the Identity feature.

\* External-login management UI, including listing, linking, and unlinking management screens.

\* WebAuthn and passkeys.

\* API-key authentication and machine-to-machine authentication.

\* Refresh-token cleanup background jobs, if required.

\* Full session management and device tracking beyond refresh-token families.



\---



\## 3. Functional Requirements



\### 3.1 Local Login



| ID    | Requirement                                                                                                                                                                                 |

| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |

| FR-1  | The system shall accept an email address and password to authenticate a user.                                                                                                               |

| FR-2  | The system shall normalize the email address using ASP.NET Core Identity's configured normalization rules.                                                                                  |

| FR-3  | The system shall find the user using the normalized email address and the configured Identity lookup behavior.                                                                              |

| FR-4  | The system shall reject login when `AccountStatus != Active`.                                                                                                                               |

| FR-5  | The system shall reject login when `UserManager.IsLockedOutAsync` returns `true`.                                                                                                           |

| FR-6  | The system shall validate the password using `UserManager.CheckPasswordAsync` or an equivalent Identity-supported authentication flow.                                                      |

| FR-7  | On invalid credentials, the system shall apply the configured failed-access policy and return a generic error message, such as `Invalid email or password.`, to reduce account enumeration. |

| FR-8  | On successful authentication, the system shall reset the failed-access counter according to the configured Identity policy.                                                                 |

| FR-9  | On successful authentication, the system shall issue a short-lived JWT access token and a cryptographically random opaque refresh token.                                                    |

| FR-10 | The system shall prepare the access token before persisting the new refresh-token record, so token-generation failure does not leave an orphaned refresh token.                             |

| FR-11 | The raw refresh token shall never be persisted. Its stored representation shall be an HMAC-SHA256 hash.                                                                                     |

| FR-12 | The response shall contain the access token, refresh token, and their respective expiry timestamps.                                                                                         |



\### 3.2 Refresh-Token Rotation



| ID    | Requirement                                                                                                                                                                                                        |

| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |

| FR-13 | The system shall accept a raw refresh token and attempt to rotate it.                                                                                                                                              |

| FR-14 | The system shall hash the supplied refresh token before performing a database lookup.                                                                                                                              |

| FR-15 | The system shall use a database transaction to maintain atomicity during refresh-token rotation.                                                                                                                   |

| FR-16 | The system shall atomically revoke the old token using a conditional database update requiring `IsRevoked = 0` and `ExpiresAtUtc > @now`. Only the request that updates exactly one row may proceed with rotation. |

| FR-17 | If the conditional update affects zero rows, the system shall determine whether the token was previously rotated by examining persisted token state, including `ReplacedByTokenHash`.                              |

| FR-18 | If token reuse is detected according to the defined policy, the system shall revoke the entire associated token family.                                                                                            |

| FR-19 | A successful rotation shall create a replacement refresh token in the same `TokenFamilyId`.                                                                                                                        |

| FR-20 | The system shall generate the replacement JWT access token before committing the rotation transaction.                                                                                                             |

| FR-21 | The system shall commit only after the replacement refresh token and access token have been successfully prepared and the required database changes are staged.                                                    |

| FR-22 | A successful rotation response shall contain the new access token, new refresh token, and their expiry timestamps.                                                                                                 |



\*\*Concurrency requirement:\*\* The conditional database update is the primary concurrency guard. EF Core `RowVersion` provides additional optimistic concurrency protection for tracked updates but does not replace the atomic conditional update.



\*\*Failure-window requirement:\*\* If token generation or database persistence fails before commit, the transaction must roll back. A successful commit followed by a lost HTTP response remains a possible failure scenario and must be addressed by the documented client recovery policy.



\### 3.3 Logout



| ID    | Requirement                                                                                                                                                             |

| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |

| FR-23 | The system shall accept a raw refresh token and revoke it if the corresponding record exists and is not already revoked.                                                |

| FR-24 | Logout shall be idempotent. Repeated requests using the same token shall not cause an application error merely because the token is already revoked or cannot be found. |

| FR-25 | The system shall handle concurrent revocation attempts gracefully.                                                                                                      |



Revoking a refresh token prevents its future use for obtaining replacement credentials. It does not automatically invalidate an already-issued JWT access token unless an additional revocation mechanism is implemented.



\### 3.4 External Authentication — Google



| ID    | Requirement                                                                                                                                                                                          |

| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |

| FR-26 | The system shall initiate a Google OAuth challenge when the user selects \*\*Continue with Google\*\*.                                                                                                   |

| FR-27 | The system shall use ASP.NET Core's official Google authentication handler to process and validate the OAuth response.                                                                               |

| FR-28 | The validated Google principal shall be temporarily stored using a dedicated `ExternalAuthentication` cookie scheme configured as the Google handler's sign-in scheme.                               |

| FR-29 | The callback shall authenticate the external cookie and extract the Google subject identifier from the validated principal, using the configured claim mapping, such as `ClaimTypes.NameIdentifier`. |

| FR-30 | The system shall never trust a Google subject identifier supplied by the client in a request body, query string, or arbitrary request header.                                                        |

| FR-31 | The system shall locate the linked local user using `UserManager.FindByLoginAsync` with the Google provider name and validated subject identifier.                                                   |

| FR-32 | If no linked local user exists, the system shall reject authentication with a generic message such as `No account linked to this provider.`                                                          |

| FR-33 | Account status and lockout policy shall be enforced before issuing application credentials through external authentication.                                                                          |

| FR-34 | Successful external authentication shall issue the application's own JWT access token and refresh token pair.                                                                                        |

| FR-35 | The temporary external cookie shall be cleared after callback processing, including failure paths where applicable.                                                                                  |



The application shall not silently create or link a local account during external login unless that behavior is explicitly introduced through the Registration or Onboarding workflow.



\### 3.5 Google Account Linking



| ID    | Requirement                                                                                                                                         |

| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------- |

| FR-36 | The system shall allow an authenticated local user to initiate Google account linking.                                                              |

| FR-37 | The controller shall obtain the local user identifier from the authenticated principal.                                                             |

| FR-38 | The controller shall protect the local user identifier using `IUserLinkStateProtector` before placing it in the OAuth authentication properties.    |

| FR-39 | The callback shall retrieve the protected local user identifier from the validated authentication properties.                                       |

| FR-40 | The callback controller shall not unprotect or interpret the protected user identifier. That responsibility belongs to the Application handler.     |

| FR-41 | `LinkGoogleForUserCommandHandler` shall use `IUserLinkStateProtector.TryUnprotect`. Invalid or tampered state shall result in a controlled failure. |

| FR-42 | The system shall reject linking when the Google identity is already associated with a different local user.                                         |

| FR-43 | If the Google identity is already associated with the same local user, the operation shall return idempotent success.                               |

| FR-44 | The system shall create the association using `UserManager.AddLoginAsync` and `UserLoginInfo("Google", subjectId, "Google")`.                       |

| FR-45 | The system shall handle database uniqueness conflicts caused by concurrent linking attempts.                                                        |

| FR-46 | Google linking shall use the same dedicated `ExternalAuthentication` cookie scheme as external login.                                               |



Protecting the local user identifier does not replace OAuth state validation, correlation-cookie validation, or callback validation performed by the authentication middleware.



\---



\## 4. Non-Functional Requirements



\### 4.1 Performance



\*\*NFR-1:\*\* Authentication endpoints should complete within 300 milliseconds under normal local/application conditions, excluding external-provider latency. This is a performance target rather than a hard acceptance criterion.



\### 4.2 Scalability



\*\*NFR-2:\*\* JWT validation shall be stateless and support validation across multiple API instances configured with compatible validation settings and signing keys.



\*\*NFR-3:\*\* Refresh-token rotation shall remain safe under concurrent requests across multiple application instances by relying on an atomic database conditional update. `RowVersion` provides additional optimistic concurrency protection for tracked updates but is not the primary rotation guard.



\### 4.3 Reliability



\*\*NFR-4:\*\* A refresh token shall not successfully rotate more than once.



\*\*NFR-5:\*\* If JWT generation or another required operation fails before the refresh transaction commits, the transaction shall roll back so the original refresh token remains valid.



\*\*NFR-6:\*\* Logout and Google linking shall be idempotent where applicable.



\*\*NFR-7:\*\* The system shall explicitly handle the possibility that a refresh transaction commits successfully but the HTTP response containing the replacement tokens is lost.



\### 4.4 Maintainability



\*\*NFR-8:\*\* Authentication logic shall be separated from HTTP concerns. Controllers shall remain thin and delegate use cases to Application handlers.



\*\*NFR-9:\*\* The Application layer shall not reference ASP.NET Core Identity, EF Core, or JWT implementation details.



\*\*NFR-10:\*\* Authentication constants, including the provider name, external cookie scheme, and authentication-property keys, shall be centralized in `AuthenticationConstants`.



\### 4.5 Testability



\*\*NFR-11:\*\* Core authentication logic shall be unit-testable through injected dependencies, including appropriate abstractions for Identity operations, token generation, refresh-token persistence, token hashing, time, and protected linking state.



\*\*NFR-12:\*\* Refresh-token rotation shall be verifiable through integration tests against a relational database capable of exercising the required transaction and concurrency behavior.



\---



\## 5. Security Requirements



\### 5.1 Token Security



| ID    | Requirement                                                                                                                                                                |

| ----- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |

| SEC-1 | Access tokens shall be short-lived, with a default lifetime of 15 minutes, and signed using HMAC-SHA256. Validation shall check issuer, audience, lifetime, and signature. |

| SEC-2 | Refresh tokens shall be cryptographically random, contain 64 bytes of random material before encoding, and be stored only as HMAC-SHA256 hashes.                           |

| SEC-3 | The refresh-token HMAC secret shall contain at least 256 bits of cryptographic key material and be supplied through secure configuration.                                  |

| SEC-4 | The JWT signing key shall contain at least 256 bits of cryptographic key material and be supplied through secure configuration.                                            |

| SEC-5 | Raw refresh tokens shall never be logged or persisted.                                                                                                                     |



A minimum key length does not by itself establish cryptographic strength. Production keys should be generated using a cryptographically secure random-number generator.



If the HMAC secret is configured as Base64, validation shall decode it and check the byte length of the decoded key rather than the length of the Base64 string.



\### 5.2 Account Security



| ID    | Requirement                                                                                                                                                                           |

| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |

| SEC-6 | Authentication shall verify `AccountStatus == Active` before issuing application credentials.                                                                                         |

| SEC-7 | Authentication shall enforce the configured ASP.NET Core Identity lockout mechanism.                                                                                                  |

| SEC-8 | Failed login attempts shall update Identity's failed-access counter according to the configured policy. Lockout thresholds and durations shall be governed by Identity configuration. |

| SEC-9 | Authentication failures shall use generic public messages where necessary to prevent account enumeration.                                                                             |



Account status and lockout are separate controls. Both must be enforced during local login, external login, and refresh-token rotation.



\### 5.3 External Authentication Security



| ID     | Requirement                                                                                                                                                                                              |

| ------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |

| SEC-10 | The Google subject identifier shall be obtained only from the validated Google `ClaimsPrincipal`.                                                                                                        |

| SEC-11 | The external authentication cookie shall be secure, HTTP-only, short-lived, configured with appropriate SameSite behavior, and cleared after callback processing. Its intended lifetime is five minutes. |

| SEC-12 | The external authentication cookie shall never be used as the application's normal authentication scheme.                                                                                                |



The external cookie's SameSite setting must be compatible with the configured OAuth callback flow and must not disable the middleware's correlation protections.



\### 5.4 Linking Security



| ID     | Requirement                                                                                                                                       |

| ------ | ------------------------------------------------------------------------------------------------------------------------------------------------- |

| SEC-13 | The local user identifier carried through Google linking shall be protected using ASP.NET Core Data Protection through `IUserLinkStateProtector`. |

| SEC-14 | The callback controller shall not unprotect or validate the protected user identifier; the Application handler shall perform this operation.      |

| SEC-15 | Google linking shall not automatically associate accounts based on matching email addresses.                                                      |

| SEC-16 | Linking shall fail if the Google subject identifier is already associated with a different local user.                                            |



\### 5.5 Configuration Security



| ID     | Requirement                                                                                                                                                                                                                                           |

| ------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |

| SEC-17 | Security-sensitive configuration shall be validated at startup. Missing or weak required configuration shall cause startup failure.                                                                                                                   |

| SEC-18 | JWT signing keys, refresh-token HMAC keys, and Google client secrets shall be stored using secure configuration, such as environment variables, development user secrets, or a managed secret store. They shall never be committed to source control. |



\---



\## 6. Integration Requirements



\### 6.1 ASP.NET Core Identity



\* The feature shall use `UserManager<User>` for password validation, lockout-related operations, external-login lookup, and `AddLoginAsync`.

\* The `User` entity shall extend `IdentityUser<Guid>`.

\* The entity shall include `AccountStatus` and `PasswordLastChangedAtUtc`.

\* Identity persistence and Identity-specific implementation details shall remain in Infrastructure.



\### 6.2 EF Core and Persistence



The refresh-token entity shall map to:



```text id="x4zvws"

security.RefreshTokens

```



The persistence configuration shall include:



\* A unique index on `TokenHash`.

\* An index on `ExpiresAtUtc`.

\* An index on `TokenFamilyId`.

\* A `RowVersion` column for additional EF Core optimistic concurrency protection.



The database operation used for refresh rotation must enforce the conditional-revocation predicate atomically. The transaction and row-count behavior must be verified against the selected relational database provider.



\### 6.3 Google OAuth



\* The Google authentication handler shall use the dedicated `ExternalAuthentication` sign-in scheme.

\* `SaveTokens` shall be `false` because the application does not use Google's tokens for its own authorization.

\* The requested scope shall include email when required by the application's account workflow.

\* The handler shall validate the OAuth response and maintain state and correlation protections.

\* The callback shall obtain the external identity from the validated principal.



\### 6.4 MediatR



\* Authentication use cases shall be exposed through commands and corresponding handlers, following the application's established CQRS conventions.

\* Handlers shall delegate to `IAuthenticationService` or the appropriate Application abstraction.

\* Infrastructure services shall not be resolved directly by API controllers or Application handlers.



\### 6.5 Data Protection



\* ASP.NET Core Data Protection shall be registered through `services.AddDataProtection()`.

\* `IUserLinkStateProtector` shall use Data Protection to protect and unprotect the local user identifier carried through Google linking.

\* Data Protection keys shall be persisted securely when needed across application restarts or multiple instances.

\* Invalid, expired, or tampered protected state shall be rejected.



\---



\## 7. Configuration Requirements



The following configuration keys are required when their corresponding capabilities are enabled.



| Configuration Key                    | Purpose                        | Validation                                                                             |

| ------------------------------------ | ------------------------------ | -------------------------------------------------------------------------------------- |

| `Jwt:Issuer`                         | JWT issuer                     | Required; not empty                                                                    |

| `Jwt:Audience`                       | JWT audience                   | Required; not empty                                                                    |

| `Jwt:Secret`                         | JWT signing key                | Required; at least 32 bytes of key material                                            |

| `Jwt:ExpiryMinutes`                  | Access-token lifetime          | Required; greater than zero and within the approved policy                             |

| `RefreshTokenHasher:SecretKey`       | Refresh-token HMAC key         | Required; valid Base64 if configured as Base64; decoded key contains at least 32 bytes |

| `RefreshToken:TokenLifetime`         | Refresh-token lifetime         | Required; `TimeSpan > 0` and within the approved policy                                |

| `Authentication:Google:ClientId`     | Google OAuth client identifier | Required when Google authentication is enabled                                         |

| `Authentication:Google:ClientSecret` | Google OAuth client secret     | Required when Google authentication is enabled                                         |



Startup validation shall ensure that the configured values are actually bound to the options used by the relevant services.



Production configuration must not silently fall back to insecure placeholder secrets.



\---



\## 8. Assumptions and Constraints



\### 8.1 Assumptions



\* Users are registered through the separate Registration/Onboarding feature.

\* Email is the primary local-login identifier; username login is not required.

\* Google is the only supported external authentication provider in the current scope.

\* JWT bearer authentication is the default API authentication mechanism.

\* WebAuthn and passkeys are outside the current scope.

\* An external-login management UI is outside the current scope.

\* The application issues its own access and refresh tokens after successful authentication.



\### 8.2 Constraints



\* The Application layer shall not reference EF Core, ASP.NET Core Identity types, or JWT implementation classes.

\* The external authentication cookie shall use the dedicated `ExternalAuthentication` scheme.

\* The Authentication module shall not depend on the removed `TokenManagement` Application module.

\* External authentication shall not automatically create or link local accounts.

\* Refresh-token rotation shall rely on database-enforced concurrency control rather than application-level read-then-write checks alone.



\---



\## 9. Traceability Matrix



| Requirement Group          | Description                                                                |

| -------------------------- | -------------------------------------------------------------------------- |

| FR-1 to FR-12              | Local login                                                                |

| FR-13 to FR-22             | Refresh-token rotation                                                     |

| FR-23 to FR-25             | Logout                                                                     |

| FR-26 to FR-35             | External authentication through Google                                     |

| FR-36 to FR-46             | Google account linking                                                     |

| NFR-1 to NFR-12            | Performance, scalability, reliability, maintainability, and testability    |

| SEC-1 to SEC-5             | Token security                                                             |

| SEC-6 to SEC-9             | Account security                                                           |

| SEC-10 to SEC-12           | External authentication security                                           |

| SEC-13 to SEC-16           | Google linking security                                                    |

| SEC-17 to SEC-18           | Configuration security                                                     |

| Integration Requirements   | Identity, EF Core, Google OAuth, MediatR, and Data Protection              |

| Configuration Requirements | JWT, refresh-token hashing, refresh-token lifetime, and Google credentials |



\---



\## 10. Acceptance Criteria



The Authentication feature is considered complete when:



1\. Local login succeeds for valid credentials and enforces account status and lockout.

2\. Invalid credentials return an appropriate generic error.

3\. Refresh-token rotation is atomic, and concurrent requests cannot both successfully rotate the same token.

4\. Reuse of a previously rotated token is detected according to policy and causes the associated token family to be revoked.

5\. Logout is idempotent and handles concurrent revocation safely.

6\. Google login succeeds for a previously linked account and issues application-owned tokens.

7\. Google login rejects an unlinked identity without silently creating or linking an account.

8\. Google linking works for an authenticated local user.

9\. Tampered or invalid protected linking state is rejected.

10\. Google linking does not automatically associate accounts based on matching email addresses.

11\. Required security-sensitive configuration is validated at startup.

12\. Sensitive credentials and token values are not logged or persisted in raw form.

13\. The Application layer remains free of Infrastructure dependencies.

14\. Refresh-token rotation rolls back if JWT generation fails before commit.

15\. The documented policy covers the case where the rotation transaction commits but the HTTP response is lost.

16\. Integration tests verify concurrency behavior against the configured relational database provider.



\---



\## 11. Summary



These requirements define a production-oriented Authentication feature aligned with the E-Commerce modular monolith architecture.



The feature supports local and Google-based authentication, refresh-token lifecycle management, and protected external account linking. It enforces account status and lockout, uses database-level concurrency control for token rotation, and maintains clear boundaries between Application orchestration and Infrastructure implementation.



Acceptance depends not only on the successful authentication flows but also on their failure behavior: concurrent refresh requests, token reuse, OAuth state validation, persistence failures, lost responses, and secure configuration must all be addressed explicitly.



\*\*Document:\*\* `requirements.md`

\*\*Module:\*\* Authentication

\*\*Version:\*\* 1.1 — Corrected



