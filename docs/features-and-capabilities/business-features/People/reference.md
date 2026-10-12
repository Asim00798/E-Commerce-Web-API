\# People Feature — Technical Reference Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Bounded Context:\*\* People — Customer Profile and Identity Linkage

\*\*Version:\*\* 1.0 — Final



\---



\## 1. Introduction



The People bounded context manages the customer's personal profile. It owns:



\* \*\*Person profile\*\* — name, date of birth, gender, phone number, email, home address, and personal image.

\* \*\*Identity linkage\*\* — the association between a Person and an Identity user account.



The context is intentionally minimal: one aggregate, no domain events, and no cross-context asynchronous communication. It serves as the live, mutable source of truth for customer contact and delivery data, which consumers (Ordering, Shipping) snapshot when needed.



\---



\## 2. Scope



\### 2.1 In Scope



\* Create the current user's Person profile.

\* Link the Person to the authenticated Identity user.

\* Update phone number, email, and home address.

\* Set or replace the personal image.

\* Delete (soft-delete) the current user's Person profile.

\* Retrieve the current user's own Person.

\* Enforce authorization at the API and Application layers.



\### 2.2 Out of Scope



\* Identity accounts, credentials, login, and tokens (owned by the Identity module).

\* Historical snapshots of contact and address data (owned by Ordering and Shipping).

\* Administrative profile management, including retrieving by ID, listing, and editing any Person; reserved for the future.

\* Address validation against a geocoding service; `LocationMapUrl` is treated as opaque.

\* Customer identity propagation across contexts; coordinated by consumers through `IPersonRepository`.



\---



\## 3. Business Responsibility



People answers:



> Who is this customer, and how can we reach them or deliver to them?



This context owns:



\* The live Person record: name, date of birth, gender, phone, email, address, and personal image.

\* The one-to-one link between a Person and an Identity user.

\* Self-service profile rules, including one active Person per user.



This context does not own:



\* Authentication or credentials (Identity module).

\* Historical snapshots of contact and address data (Ordering and Shipping).

\* Ordering decisions that gate on Person existence; Ordering enforces those rules.

\* File storage mechanics (File Storage capability).



\---



\## 4. Domain Model



\### 4.1 Aggregates



\#### Person



Represents a customer's personal profile.



\*\*Properties:\*\*



\* `Id`

\* `IdentityUserId` — nullable until linked.

\* `Name` — `FullName` value object.

\* `DateOfBirth` — `DateOnly`.

\* `Gender` — `Gender` enum.

\* `PhoneNumber` — `PhoneNumber` value object.

\* `Email` — `Email` value object.

\* `HomeAddress` — nullable `Address` value object.

\* `PersonalImage` — nullable `PersonalImage` value object.



\*\*Behaviors:\*\*



\* `Constructor(...)` — public DDD constructor with `Require\*` validation helpers.

\* `LinkIdentityUser(identityUserId)`

\* `UpdatePhoneNumber(phoneNumber)`

\* `UpdateEmail(email)`

\* `UpdateAddress(homeAddress)`

\* `SetPersonalImage(fileId)`

\* `Validate()` — override called by the `ValidationInterceptor`.



\*\*Invariants:\*\*



\* `Name`, `PhoneNumber`, and `Email` must be non-null on every save.

\* `DateOfBirth` cannot be in the future.

\* `Gender` must be a defined enum value.

\* `PersonalImage.FileId` must be non-empty when set.

\* `IdentityUserId` can be set at most once per Person instance.

\* A user can have at most one active Person, enforced by a filtered unique index on `IdentityUserId` where `IsDeleted = 0`.



No domain events are raised by the aggregate in this version.



\### 4.2 Value Objects



\#### FullName



A value object representing a person's full name in four parts.



```csharp

public sealed record FullName

{

&#x20;   public string FirstName { get; init; }

&#x20;   public string? SecondName { get; init; }

&#x20;   public string? ThirdName { get; init; }

&#x20;   public string LastName { get; init; }



&#x20;   public FullName(

&#x20;       string firstName,

&#x20;       string? secondName,

&#x20;       string? thirdName,

&#x20;       string lastName)

&#x20;   {

&#x20;       FirstName = ValidateName(firstName, "First name");

&#x20;       LastName = ValidateName(lastName, "Last name");

&#x20;       SecondName = secondName;

&#x20;       ThirdName = thirdName;

&#x20;   }

}

```



\#### Address



A value object representing a home address.



```csharp

public sealed record Address

{

&#x20;   public string Street { get; init; }

&#x20;   public string City { get; init; }

&#x20;   public AddressType Type { get; init; } = AddressType.Home;

&#x20;   public string? LocationMapUrl { get; init; }

}

```



\#### PersonalImage



A value object wrapping the file ID of the personal image.



```csharp

public sealed record PersonalImage

{

&#x20;   public Guid FileId { get; }



&#x20;   public PersonalImage(Guid fileId)

&#x20;   {

&#x20;       if (fileId == Guid.Empty)

&#x20;           throw new ArgumentNullException(nameof(fileId));



&#x20;       FileId = fileId;

&#x20;   }

}

```



\### 4.3 Repositories



`IPersonRepository : IRepository<Person>`



\*\*Specialized operation:\*\*



```csharp

GetByIdentityUserIdAsync(

&#x20;   Guid identityUserId,

&#x20;   CancellationToken ct = default)

```



This is the sole cross-context read path. It returns the active Person for a given Identity user, or `null` if none exists or the Person has been soft-deleted.



\---



\## 5. Application Layer



\### 5.1 Commands



| Command                     | Purpose                                                   | Permission Attribute          |

| --------------------------- | --------------------------------------------------------- | ----------------------------- |

| `CreateMyPersonCommand`     | Create the current user's Person and link it to Identity. | `PeoplePermissions.CreateOwn` |

| `UpdateMyPersonCommand`     | Update phone, email, and home address.                    | `PeoplePermissions.UpdateOwn` |

| `SetMyPersonalImageCommand` | Upload and set the personal image.                        | `PeoplePermissions.UpdateOwn` |

| `DeleteMyPersonCommand`     | Soft-delete the current user's Person.                    | `PeoplePermissions.DeleteOwn` |



There is no separate `LinkIdentityUserCommand`; linking is performed inline during creation.



Name, date of birth, and gender are set at creation and are not updatable through any command in this version.



\### 5.2 Queries



| Query              | Purpose                             | Permission Attribute        |

| ------------------ | ----------------------------------- | --------------------------- |

| `GetMyPersonQuery` | Retrieve the current user's Person. | `PeoplePermissions.ReadOwn` |



Queries perform resource checks using the current user context. Users access their own Person through `ICurrentUser`, with no route parameter through which another user could be addressed.



\### 5.3 Domain Event Handlers



None. No domain events exist in this version.



\### 5.4 Integration Events



None. The context is self-contained and does not communicate asynchronously with other bounded contexts.



\---



\## 6. Infrastructure Layer



\### 6.1 Persistence



\* `AppDbContext.People.cs` — defines `DbSet<Person>`. All value objects (`FullName`, `Email`, `PhoneNumber`, `HomeAddress`, and `PersonalImage`) are configured as owned types.

\* \*\*Configuration:\*\* `PersonConfiguration`, including owned relationships for all value objects.

\* All configurations inherit `BaseEntityConfiguration<T>` and define table names, keys, owned value objects, and the `RowVersion` shadow property where appropriate.



\### 6.2 Database Constraints



\* \*\*Person — Identity linkage:\*\* Filtered unique index on `IdentityUserId` where `\[IdentityUserId] IS NOT NULL AND \[IsDeleted] = 0`, named `IX\_People\_IdentityUserId\_Unique`.

\* \*\*Person — Email:\*\* Filtered unique index on `Email` where `\[IsDeleted] = 0`, named `IX\_People\_Email\_Unique`.



\### 6.3 Repositories



\* `PersonRepository` implements `IPersonRepository`, including `GetByIdentityUserIdAsync`.

\* Repositories are registered as scoped services through repository assembly scanning.



\### 6.4 Soft Delete



The People context uses soft delete exclusively.



Deletion through `PersonRepository.Remove(person)` marks the aggregate for deletion. The `AuditAndSoftDeleteInterceptor` converts the EF `Deleted` state to `Modified`, setting:



\* `IsDeleted = true`

\* `DeletedAt`

\* `DeletedBy`



The filtered unique indexes on `IdentityUserId` and `Email` exclude soft-deleted rows, allowing a user to recreate their Person or reuse their email after deletion.



\---



\## 7. API Layer



Controllers are thin, use role-based authorization, and delegate to MediatR commands and queries.



\### 7.1 Controllers



| Controller         | Route            | Role(s)                            |

| ------------------ | ---------------- | ---------------------------------- |

| `PeopleController` | `/api/v1/people` | Authenticated user (`\[Authorize]`) |



\### 7.2 Endpoints



```text

POST   /api/v1/people

GET    /api/v1/people/me

PUT    /api/v1/people/me

DELETE /api/v1/people/me

PUT    /api/v1/people/me/image

```



The `me` segment identifies the current authenticated user's Person. Ownership is enforced in the Application layer through `ICurrentUser`.



Permissions are enforced at the Application layer; controllers enforce authentication only.



\---



\## 8. Authorization



\### 8.1 Permissions



```csharp

public static class PeoplePermissions

{

&#x20;   public const string ReadOwn = "People.ReadOwn";

&#x20;   public const string CreateOwn = "People.CreateOwn";

&#x20;   public const string UpdateOwn = "People.UpdateOwn";

&#x20;   public const string DeleteOwn = "People.DeleteOwn";



&#x20;   // Reserved for future admin endpoints.

&#x20;   public const string Read = "People.Read";

&#x20;   public const string Update = "People.Update";

&#x20;   public const string Delete = "People.Delete";

}

```



The `Own` suffix distinguishes self-service operations from future administrative operations.



\### 8.2 Enforcement



\* \*\*API layer:\*\* Requires an authenticated user (`\[Authorize]`). No role is specified; every authenticated principal is authorized to manage their own Person.

\* \*\*Application layer:\*\* Uses `\[AuthorizePermission]` attributes on commands and queries.

\* \*\*Handlers:\*\* Resource ownership is implicit in the query. The Person is resolved through `ICurrentUser.UserId`, so there is no route parameter or input through which another user's Person could be addressed. No additional ownership check is required.



\---



\## 9. Workflows



\### 9.1 Create Person



```text

POST /api/v1/people

&#x20;       ↓

CreateMyPersonCommand

&#x20;       ↓

Check that no Person exists for the current Identity user

&#x20;       ↓

Construct FullName, PhoneNumber, Email, and Address value objects

&#x20;       ↓

new Person(...)

&#x20;       ↓

person.LinkIdentityUser(currentUserId)

&#x20;       ↓

personRepository.AddAsync(person)

&#x20;       ↓

UnitOfWork.SaveChangesAsync

```



\### 9.2 Get Person



```text

GET /api/v1/people/me

&#x20;       ↓

GetMyPersonQuery

&#x20;       ↓

personRepository.GetByIdentityUserIdAsync(currentUserId)

&#x20;       ↓

Map to PersonDto → PersonResponse

```



If no Person exists, the operation returns a failure that the API maps to HTTP `404 Not Found`.



\### 9.3 Update Person



Load the Person by the current Identity user's ID, apply `UpdatePhoneNumber`, `UpdateEmail`, and `UpdateAddress`, and save.



\### 9.4 Set Personal Image



Load the Person by the current Identity user's ID. Call `IFileService.UploadAsync` to receive a `FileId`, then call `person.SetPersonalImage(fileId)` and save.



\### 9.5 Delete Person



Load the Person by the current Identity user's ID, call `personRepository.Remove(person)`, and invoke `UnitOfWork.SaveChangesAsync`.



The soft-delete interceptor converts the deletion to `IsDeleted = true`.



\---



\## 10. Transaction Boundaries and Consistency



\* Each mutation using tracked entity changes is persisted through the application's Unit of Work and committed atomically.

\* People does not initiate cross-context synchronous operations.

\* `SetMyPersonalImageCommandHandler` calls `IFileService.UploadAsync` (which uses its own transaction) before mutating the aggregate. If the subsequent database save fails, an orphan file remains and is handled by the File Storage cleanup job.

\* Queries execute as read operations and do not require an explicit transaction.



\---



\## 11. Concurrency and Idempotency



\* `RowVersion` provides optimistic concurrency control on Person.

\* Filtered unique indexes enforce one active Person per Identity user and one active Person per email at the persistence level.

\* Application handlers currently do not implement retry or fallback behavior for unique constraint violations; this is deferred. The database remains the final protection.

\* Idempotent behavior is implemented in domain update methods, which perform no operation when the supplied value is unchanged.



\---



\## 12. Testing



\### 12.1 Domain Tests



\* Person constructor invariants: required name, phone, and email; date of birth not in the future; valid gender.

\* `LinkIdentityUser` behavior: rejects an empty ID and attempts to relink.

\* Update methods: no-op on unchanged values; reject invalid input.

\* `Validate()` catches invalid state on save.



\### 12.2 Application Tests



\* Command handlers with mocked repositories, `ICurrentUser`, `IUnitOfWork`, and `IFileService`.

\* `CreateMyPerson` fails when a Person already exists.

\* `SetMyPersonalImage` calls `IFileService.UploadAsync` before mutating the aggregate.

\* `DeleteMyPerson` uses `Remove` (soft delete), not `HardDeleteAsync`.



\### 12.3 Integration Tests



\* EF mappings, owned types, nullable columns, and filtered unique indexes.

\* Filtered unique indexes allow soft-delete-then-recreate.

\* `RowVersion` concurrency behavior.



\### 12.4 API Tests



\* Authenticated-only access.

\* Customer isolation through `ICurrentUser`-based resolution.

\* HTTP `404 Not Found` when no Person exists.

\* HTTP `400 Bad Request` for invalid input, such as an invalid email or future date of birth.



\---



\## 13. Summary



The People bounded context provides the live customer profile that other contexts consume through `IPersonRepository`.



The domain is intentionally minimal: one aggregate, no domain events, and no integration events. Identity linkage is one-shot and enforced by the aggregate and database. Deletion is soft, preserving audit information and allowing recreation through filtered unique indexes.



The feature remains self-contained, with no asynchronous communication and no cross-context coordination initiated by People.



This design is intended to be production-ready and maintainable.



\---



\*\*Document version:\*\* 1.0 — Final

\*\*Feature:\*\* People — Customer Profile and Identity Linkage



