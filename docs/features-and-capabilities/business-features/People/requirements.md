\# People Feature — Requirements Technical Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Scope:\*\* Functional, Non-Functional, Security, and Integration Requirements

\*\*Version:\*\* 1.0 – Final



\---



\## 1. Introduction



This document defines the technical requirements for the People feature of the E-Commerce modular monolith. It serves as the specification for design, development, testing, and acceptance.



The People feature manages the customer's personal profile — name, date of birth, gender, phone number, email, home address, and personal image — and its one-to-one link to an Identity user. It is a supporting bounded context whose data is consumed by other contexts, primarily Ordering, when they need contact or delivery information.



The feature is implemented across Domain, Application, Infrastructure, and API layers, following Clean Architecture and DDD principles. The Domain layer is independent of infrastructure concerns. The context is a pull-only leaf: it raises no domain events, publishes no integration events, and initiates no synchronous calls to other business contexts.



\---



\## 2. Scope



\### 2.1 In Scope



\* Person aggregate lifecycle: create, update, and soft-delete.

\* Linking a Person to an Identity user.

\* Personal image upload coordination via `IFileService`.

\* Self-service profile management: create, read, update, and delete one's own Person.

\* Profile retrieval for the authenticated user.

\* Authorization at API and Application layers.

\* Cross-context read contract (`IPersonRepository`) for consumers that need live Person data.



\### 2.2 Out of Scope



\* Identity accounts, credentials, login, and tokens, which are owned by the Identity module.

\* Historical snapshots of contact/address data, which are owned by Ordering and Shipping.

\* Administrative profile management, including retrieving by ID, listing, or editing any Person; reserved for the future.

\* Address validation against a geocoding service; `LocationMapUrl` is treated as opaque.

\* Customer identity propagation across contexts; coordinated by consumers.

\* Written reviews, ratings, and wishlists, which are owned by CustomerEngagement.

\* Multi-address support; a Person has one `HomeAddress` in this version.



\---



\## 3. Functional Requirements



\### 3.1 Person Creation



\*\*FR 1:\*\* The system shall allow an authenticated user to create their own Person profile.



\*\*FR 2:\*\* The system shall reject a create request if the current user already has an active Person.



\*\*FR 3:\*\* The system shall require `FirstName`, `LastName`, `DateOfBirth`, `Gender`, `PhoneNumber`, `Email`, and `HomeAddress` on creation.



\*\*FR 4:\*\* The system shall allow `SecondName` and `ThirdName` to be optional.



\*\*FR 5:\*\* The system shall validate that `DateOfBirth` is not in the future.



\*\*FR 6:\*\* The system shall validate that `Gender` is a defined `Gender` enum value.



\*\*FR 7:\*\* The system shall validate `PhoneNumber` and `Email` using the shared kernel value objects.



\*\*FR 8:\*\* The system shall validate that `Address.Street` and `Address.City` are non-empty and shall trim whitespace.



\*\*FR 9:\*\* The system shall validate `Address.LocationMapUrl` as a well-formed absolute URI when supplied.



\*\*FR 10:\*\* On successful creation, the system shall link the new Person to the authenticated user's Identity account via `LinkIdentityUser`.



\*\*FR 11:\*\* The system shall reject linking if the Person is already linked.



\*\*FR 12:\*\* The system shall persist the Person and the identity link atomically in a single transaction.



\### 3.2 Person Update



\*\*FR 13:\*\* The system shall allow the authenticated user to update the phone number on their own Person.



\*\*FR 14:\*\* The system shall allow the authenticated user to update the email on their own Person.



\*\*FR 15:\*\* The system shall allow the authenticated user to update the home address on their own Person.



\*\*FR 16:\*\* The system shall treat name, date of birth, and gender as immutable after creation. There is no update path for these fields in this version.



\*\*FR 17:\*\* Update operations shall be no-ops when the supplied value equals the current value.



\*\*FR 18:\*\* Update operations shall validate the new value using the same rules applied at creation.



\*\*FR 19:\*\* The system shall reject update requests when the current user has no active Person.



\### 3.3 Personal Image



\*\*FR 20:\*\* The system shall allow the authenticated user to upload a personal image via `multipart/form-data`.



\*\*FR 21:\*\* The system shall coordinate the upload through `IFileService.UploadAsync`, passing the file content, name, and content type.



\*\*FR 22:\*\* The system shall assign the resulting `FileId` to `Person.PersonalImage`.



\*\*FR 23:\*\* The system shall treat setting a personal image as a replace operation when a personal image already exists.



\*\*FR 24:\*\* The system shall not expose file storage paths, keys, or provider types to the Application or Domain layers; only the `FileId` is used.



\*\*FR 25:\*\* On failure of the Person save after a successful file upload, the system shall leave the orphan file for the File Storage cleanup job to handle.



\### 3.4 Person Deletion



\*\*FR 26:\*\* The system shall allow the authenticated user to delete their own Person.



\*\*FR 27:\*\* Deletion shall be soft: the Person row remains in the database with `IsDeleted = true`, `DeletedAt` set, and `DeletedBy` populated.



\*\*FR 28:\*\* Deletion shall not affect the Identity user account.



\*\*FR 29:\*\* After deletion, the authenticated user shall be able to create a new Person for the same Identity account.



\*\*FR 30:\*\* After deletion, the email previously used by the deleted Person shall become available for reuse by another active Person.



\### 3.5 Person Query



\*\*FR 31:\*\* The system shall allow the authenticated user to retrieve their own Person.



\*\*FR 32:\*\* The system shall return the profile with `IdentityUserId` included internally but hidden from the API response.



\*\*FR 33:\*\* The system shall return a failure indicating "not found" when the current user has no active Person.



\### 3.6 Cross-Context Read



\*\*FR 34:\*\* The system shall expose `IPersonRepository.GetByIdentityUserIdAsync(Guid identityUserId)` as the sole cross-context read path.



\*\*FR 35:\*\* The method shall return the active Person for the given Identity user, or `null` if none exists or the Person has been soft-deleted.



\*\*FR 36:\*\* The method shall not participate in a shared transaction with the caller.



\*\*FR 37:\*\* Consumers shall not modify Person records through the repository.



\### 3.7 Domain Model Requirements



\*\*FR 38:\*\* The system shall model `FullName`, `Address`, and `PersonalImage` as value objects.



\*\*FR 39:\*\* The system shall model `Gender` and `AddressType` as enums, persisted as strings.



\*\*FR 40:\*\* The system shall enforce that `IdentityUserId` can be set at most once per Person instance.



\*\*FR 41:\*\* The system shall expose a `Validate()` override on Person called by the `ValidationInterceptor` on every save.



\*\*FR 42:\*\* The Domain layer shall not reference any infrastructure type, file storage abstraction, or ASP.NET Core type.



\*\*FR 43:\*\* The Person aggregate shall not raise any domain events in this version.



\---



\## 4. Non-Functional Requirements



\### 4.1 Performance



\*\*NFR 1:\*\* Profile read and write operations should complete within 200 ms under normal load, excluding external file storage latency for image uploads.



\*\*NFR 2:\*\* The `GetByIdentityUserIdAsync` query shall use the filtered unique index on `IdentityUserId` for the lookup.



\### 4.2 Scalability



\*\*NFR 3:\*\* The system must support multiple API instances. Person updates must be concurrency-safe using `RowVersion` optimistic concurrency.



\*\*NFR 4:\*\* Soft-deleted Person rows shall not accumulate unindexed; the filtered unique indexes shall be maintained as rows transition to soft-deleted state.



\### 4.3 Reliability



\*\*NFR 5:\*\* A failure during the personal image upload flow shall not corrupt the Person record — the aggregate is only mutated after the upload succeeds.



\*\*NFR 6:\*\* A crash after a successful file upload but before the Person save shall leave an orphan file; the File Storage cleanup job shall eventually remove it.



\*\*NFR 7:\*\* Database-level constraints (filtered unique indexes) shall be the final protection against duplicate active Persons for the same Identity user.



\### 4.4 Maintainability



\*\*NFR 8:\*\* The Application layer shall not reference EF Core, file storage providers, or any infrastructure implementation.



\*\*NFR 9:\*\* The Domain shall express all state mutations as explicit methods, with no public setters on stateful properties.



\*\*NFR 10:\*\* The cross-context contract (`IPersonRepository`) shall be a stable abstraction; introducing a consumer-oriented wrapper, such as `IPersonReader`, later shall not require changes to the Person aggregate.



\### 4.5 Testability



\*\*NFR 11:\*\* The Person aggregate, value objects, and enums shall be unit-testable without external dependencies.



\*\*NFR 12:\*\* Command handlers shall be testable with mocked repositories, `ICurrentUser`, `IUnitOfWork`, and `IFileService`.



\*\*NFR 13:\*\* Filtered unique indexes and `RowVersion` behavior shall be verifiable through integration tests against a real SQL Server instance.



\---



\## 5. Security Requirements



\### 5.1 Self-Service Isolation



\*\*SEC 1:\*\* Every People endpoint shall require authentication.



\*\*SEC 2:\*\* The system shall resolve the Person from the authenticated user's identity (`ICurrentUser`), never from a route parameter or request body.



\*\*SEC 3:\*\* No endpoint shall expose a way to read or modify another user's Person.



\### 5.2 Permission Enforcement



\*\*SEC 4:\*\* The system shall enforce fine-grained permissions (`People.ReadOwn`, `People.CreateOwn`, `People.UpdateOwn`, `People.DeleteOwn`) on commands and queries via `\[AuthorizePermission]`.



\*\*SEC 5:\*\* The system shall reserve the non-Own permissions (`People.Read`, `People.Update`, `People.Delete`) for future administrative endpoints and shall not assign them to any current user role.



\### 5.3 Personal Data Protection



\*\*SEC 6:\*\* The system shall not expose `IdentityUserId` to API clients.



\*\*SEC 7:\*\* The system shall validate `PhoneNumber` and `Email` using the shared kernel value objects to prevent malformed data from being persisted.



\*\*SEC 8:\*\* The system shall persist `Gender` and `AddressType` as strings to preserve schema stability across enum growth.



\*\*SEC 9:\*\* The system shall not log sensitive personal data (phone number, email, address, personal image `FileId`) beyond what is necessary for diagnosis and audit.



\### 5.4 File Upload Safety



\*\*SEC 10:\*\* The system shall enforce file size limits via the File Storage layer's `SizeLimitedReadStream` and metadata validation via `FileStorageOptions.AllowedContentTypes` / `AllowedExtensions`.



\*\*SEC 11:\*\* The system shall not expose storage keys, local paths, or provider details to API clients.



\---



\## 6. Integration Requirements



\### 6.1 Repository Integration



\* The system shall expose `IPersonRepository.GetByIdentityUserIdAsync` as the sole cross-context read path.

\* Consumers shall not establish a shared transaction with the Person repository.

\* Consumers shall not mutate Person records through the repository.



\### 6.2 File Storage Integration



\* The system shall coordinate personal image uploads via `IFileService`.

\* The Application handler shall translate the API-layer `IFormFile` into the Application-layer `FileUpload` model before calling `IFileService`.

\* The Domain shall reference only `Guid FileId`, never any file storage type.



\### 6.3 Outbox Integration



\* The People context shall not publish integration events.

\* The People context shall not consume integration events.

\* No domain event handlers or integration event handlers shall exist in this version.



\### 6.4 Background Processing



\* The People context shall not host background jobs.

\* No recurring triggers, scheduled jobs, or fire-and-forget jobs originate from People.



\### 6.5 Persistence



\* The Person aggregate shall be persisted in the `user` schema using EF Core.

\* Value objects (`FullName`, `Address`, `PersonalImage`) shall be mapped as owned types.

\* `RowVersion` shall be configured as a shadow property on the aggregate root.

\* Filtered unique indexes shall enforce one active Person per Identity user and per email.



\### 6.6 MediatR



\* All application operations shall be exposed as commands or queries with MediatR handlers.

\* `\[AuthorizePermission]` attributes shall be applied to each command and query.

\* No direct Outbox writes shall occur inside command handlers.



\---



\## 7. Configuration Requirements



The People feature consumes the following configuration sections indirectly, through File Storage and security infrastructure.



| Section / Key                     | Purpose                | Validation                                 |

| --------------------------------- | ---------------------- | ------------------------------------------ |

| `FileStorage:MaxFileSizeBytes`    | Maximum upload size    | Enforced by File Storage options validator |

| `FileStorage:AllowedContentTypes` | Content type allowlist | Enforced by File Storage options validator |

| `FileStorage:AllowedExtensions`   | Extension allowlist    | Enforced by File Storage options validator |



People itself requires no configuration. There are no People-specific options, secrets, or feature flags.



\---



\## 8. Assumptions and Constraints



\### 8.1 Assumptions



\* Identity and Person are independent concepts. Registration creates only an Identity user; Person creation is a separate, user-initiated action.

\* A user may exist and authenticate in the system without a Person record.

\* A user may have at most one active Person at any time.

\* Deleting a Person does not delete the Identity user.

\* Consumers (Ordering and Shipping) capture immutable snapshots of Person data at the moment they need it; they do not re-read after the snapshot is taken.

\* `LocationMapUrl` is treated as opaque text. No geocoding or address validation is performed.



\### 8.2 Constraints



\* The Domain layer must not reference any infrastructure type, file storage abstraction, or ASP.NET Core type.

\* The Person aggregate must not raise domain events in this version.

\* Cross-context reads must go through `IPersonRepository`; no direct table access from other contexts.

\* The API must not expose `IdentityUserId`.

\* The Application handler must not accept `IFormFile`; the API controller is responsible for translating it to a `FileUpload` model.

\* The system must enforce soft-delete for Person; hard-delete is reserved for the exceptional case where a physical removal is explicitly required.



\---



\## 9. Traceability Matrix



| Requirement ID  | Description                                                                       |

| --------------- | --------------------------------------------------------------------------------- |

| FR 1 to FR 12   | Person creation                                                                   |

| FR 13 to FR 19  | Person update                                                                     |

| FR 20 to FR 25  | Personal image upload                                                             |

| FR 26 to FR 30  | Person soft-deletion                                                              |

| FR 31 to FR 33  | Person query                                                                      |

| FR 34 to FR 37  | Cross-context read contract                                                       |

| FR 38 to FR 43  | Domain model requirements                                                         |

| NFR 1 to NFR 13 | Performance, scalability, reliability, maintainability, and testability           |

| SEC 1 to SEC 11 | Security requirements                                                             |

| Integration     | Repository, File Storage, Outbox, background processing, persistence, and MediatR |

| Configuration   | File Storage options consumed indirectly                                          |



\---



\## 10. Acceptance Criteria



The People feature is considered complete when:



1\. An authenticated user can create a Person, and the Person is linked to their Identity account atomically.

2\. A second create request from the same user returns a failure.

3\. The Person profile can be retrieved via `GET /api/v1/people/me`.

4\. Update operations modify phone, email, and home address while leaving name, date of birth, and gender unchanged.

5\. A personal image can be uploaded and the resulting `FileId` is stored on the Person.

6\. Deleting a Person soft-deletes the record; a subsequent create succeeds and the email is reusable.

7\. `IPersonRepository.GetByIdentityUserIdAsync` returns the active Person and `null` after deletion.

8\. All endpoints are authenticated and self-service; no endpoint allows addressing another user's Person.

9\. `IdentityUserId` is never returned in an API response.

10\. The Domain layer contains no infrastructure references and raises no domain events.

11\. Filtered unique indexes on `IdentityUserId` and `Email` are present and functional.

12\. `RowVersion` optimistic concurrency is enforced on the Person aggregate.



\---



\## 11. Summary



The People feature provides the customer's live personal profile as a supporting bounded context. It is deliberately the smallest business context in the system:



\* One aggregate, one repository, five use cases.

\* No domain events, no integration events, and no jobs.

\* Self-service only, with ownership enforced by construction rather than by explicit checks.

\* One cross-context contract (`IPersonRepository`) consumed by Ordering.

\* Soft-delete semantics with filtered unique indexes that allow re-creation.



The result is a production-oriented, maintainable, and trivially extractable context that owns the live Person record without imposing asynchronous or transactional coupling on the rest of the system.



\---



\*\*Document version:\*\* 1.0 – Final

\*\*Feature:\*\* People / Customer Profile and Identity Linkage



