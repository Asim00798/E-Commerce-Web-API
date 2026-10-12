People Feature – Integration Guide

E-Commerce Modular Monolith

How Other Contexts and Clients Integrate with the People Bounded Context

Version 1.0 – Final

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

1\. Purpose

This document explains how other bounded contexts and client applications integrate with the People bounded context.

It covers:

•	The shared cross-context contract (IPersonRepository).

•	The absence of integration events (inbound and outbound).

•	REST API usage for the authenticated user's own Person.

•	Authorization and permission requirements.

•	Integration rules and boundaries.

The guide is intended for developers in consuming contexts (primarily Ordering) and for client applications that expose profile management to users.

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

2\. Integration Overview

People is a pull-only leaf context. It does not publish integration events, does not consume them, does not host background jobs, and does not initiate synchronous calls to other business contexts. Consumers reach into People; People never reaches out.

There are two integration channels:

1\.	Shared application contract – IPersonRepository, consumed by contexts that need live Person data (primarily Ordering).

2\.	REST APIs – self-service endpoints for the authenticated user's own Person.

There are no integration events in either direction.

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

3\. Shared Contract Integration

3.1 Contract

Consumers depend on IPersonRepository (Application/Shared abstraction implemented by Infrastructure):

csharp

public interface IPersonRepository : IRepository<Person>

{

&#x20;   Task<Person?> GetByIdentityUserIdAsync(

&#x20;       Guid identityUserId,

&#x20;       CancellationToken ct = default);

}

GetByIdentityUserIdAsync is the sole cross-context read path. It returns the active Person for a given Identity user, or null if no Person exists or the Person has been soft-deleted.

3.2 Consumers

Consumer	Purpose

Ordering	Verify a Person exists before allowing order placement.

Ordering	Fetch contact and address data when the order transitions to shipping-ready, to build the OrderReadyForShippingIntegrationEvent payload.

No other context consumes Person data today. If a new consumer appears, it uses the same contract.

3.3 Transactional Model

IPersonRepository reads are outside any transaction. They are synchronous reads against the same AppDbContext (in the modular monolith) and return the current state. No shared transaction is required or established.

3.4 Future Direction

The Context Map specifies that People should eventually expose consumer-oriented contracts (e.g., IPersonReader) rather than the domain repository interface. In the current implementation, IPersonRepository serves as the shared contract; a consumer-oriented wrapper can be introduced later without changing People's internals.

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

4\. Integration Events

4.1 Outbound (from People)

None. People raises no domain events and publishes no integration events. The absence is deliberate: nothing outside People reacts to profile lifecycle facts. When Ordering or Shipping needs Person data, it pulls synchronously at the point of use.

4.2 Inbound (to People)

None. People does not subscribe to any integration events. There is no IntegrationEventHandlers/ folder, and no handler implementations exist.

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

5\. REST API Integration

5.1 Controller

PeopleController exposes self-service endpoints for the authenticated user's own Person.

Method	Route	Purpose

POST	/api/v1/people	Create the current user's Person

GET	/api/v1/people/me	Retrieve the current user's Person

PUT	/api/v1/people/me	Update phone, email, and home address

DELETE	/api/v1/people/me	Delete the current user's Person

PUT	/api/v1/people/me/image	Upload and set the personal image

5.2 Request/Response DTOs

Requests:

•	CreatePersonRequest – FirstName, SecondName?, ThirdName?, LastName, DateOfBirth, Gender, PhoneNumber, Email, HomeAddress.

•	UpdatePersonRequest – PhoneNumber, Email, HomeAddress.

•	SetPersonalImageRequest – Image (multipart/form-data).

•	AddressRequest – Street, City, Type, LocationMapUrl?.

Responses:

•	PersonResponse – Id, name parts, DateOfBirth, Gender (string), PhoneNumber, Email, HomeAddress?, PersonalImageFileId?. Omits the internal IdentityUserId.

5.3 Client Usage

All endpoints are self-service. The client does not need to know or pass a user identifier — the Person is resolved from the authenticated token via ICurrentUser on the server side. A client cannot address another user's Person through this API.

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

6\. Authorization Integration

6.1 Permissions

csharp

public static class PeoplePermissions

{

&#x20;   public const string ReadOwn   = "People.ReadOwn";

&#x20;   public const string CreateOwn = "People.CreateOwn";

&#x20;   public const string UpdateOwn = "People.UpdateOwn";

&#x20;   public const string DeleteOwn = "People.DeleteOwn";



&#x20;   // Reserved for future admin endpoints.

&#x20;   public const string Read   = "People.Read";

&#x20;   public const string Update = "People.Update";

&#x20;   public const string Delete = "People.Delete";

}

6.2 API Layer

\[Authorize] on PeopleController — any authenticated principal. No role is specified because every authenticated user manages their own Person.

6.3 Application Layer

Commands and queries carry \[AuthorizePermission(PeoplePermissions.\*Own)]. The permission system resolves the permission against the authenticated user's roles.

6.4 Resource Authorization

No explicit resource-ownership check is needed in handlers. The Person is resolved from ICurrentUser.UserId, and the query itself is the ownership check — there is no external identifier through which a caller could substitute another user.

6.5 Admin/Support Access

Not implemented in this version. The non-Own permissions (People.Read, People.Update, People.Delete) are reserved for future administrative endpoints and are not wired to any command or query today.

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

7\. Integration Rules

•	Do not modify People tables directly from other contexts. Use IPersonRepository or a future consumer-oriented contract.

•	Do not rely on People for historical data. Consumers capture their own immutable snapshots at the moment they are needed (Ordering and Shipping do this today).

•	Do not publish or consume events on behalf of People. People raises no domain events and publishes no integration events.

•	Do not depend on People's domain types outside People. Use the shared contract (IPersonRepository) and application DTOs if mapping is required.

•	Do not establish a shared transaction with People's repository. Reads are synchronous and do not participate in the caller's transaction.

•	Do not use IPersonRepository for write operations. All writes go through the People REST API or People commands. The repository exposes reads and standard IRepository<T> operations, but cross-context consumers must not mutate a Person.

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

8\. Common Integration Scenarios

8.1 Ordering gates order placement on Person existence

text

PlaceOrderCommandHandler

&#x20;  ↓

IPersonRepository.GetByIdentityUserIdAsync(currentUserId)

&#x20;  ├── null → return failure ("Complete your personal profile before placing an order.")

&#x20;  └── Person → continue with checkout

Ordering is the only context that enforces this gate. People does not know that Ordering exists.

8.2 Ordering snapshots Person data at shipping-ready

text

OrderPaidDomainEventHandler

&#x20;  ↓

IPersonRepository.GetByIdentityUserIdAsync(customerId)

&#x20;  ↓

Build OrderReadyForShippingIntegrationEvent with current name, phone, street, city, mapUrl

&#x20;  ↓

Write to Outbox

&#x20;  ↓

Shipping stores the payload as an immutable DeliveryAddressSnapshot

The Person record is read once, at the moment the shipping-ready event is built. After that, Shipping owns its snapshot. Later changes to the Person record do not affect shipments already created.

8.3 Client creates a Person profile

text

POST /api/v1/people  →  CreatePersonRequest

&#x20;  ↓

CreateMyPersonCommand

&#x20;  ↓

Person created and linked to the caller's Identity user

Response: 201 with Location: /api/v1/people/me. The client can immediately GET the new resource.

8.4 Client updates the personal image

text

PUT /api/v1/people/me/image  (multipart/form-data)

&#x20;  ↓

SetMyPersonalImageCommand(FileUpload)

&#x20;  ↓

IFileService.UploadAsync → FileId

&#x20;  ↓

person.SetPersonalImage(fileId)

The response is 204 No Content. To retrieve the new image, the client uses the PersonalImageFileId from GET /api/v1/people/me and fetches the file via the Files API.

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

9\. Testing Integration

9.1 Shared Contract Tests

•	Verify GetByIdentityUserIdAsync returns the active Person for a linked Identity user.

•	Verify it returns null when the Person is soft-deleted.

•	Verify it returns null for an Identity user with no Person.

9.2 Consumer Integration Tests

•	Ordering: PlaceOrderCommandHandler fails when no Person exists; succeeds when a Person exists.

•	Ordering: OrderPaidDomainEventHandler builds a shipping-ready payload containing the current Person's contact and address data.

•	Ordering: shipping-ready payload is not affected by subsequent Person changes (snapshot semantics).

9.3 API Tests

•	Authenticated-only access on all endpoints.

•	Customer isolation: user A cannot see or modify user B's Person.

•	404 when no Person exists for the current user.

•	400 on invalid input (invalid email, future DOB, empty required fields).

•	204 on successful update/delete.

•	201 on successful create, with correct Location header.

9.4 File Upload Tests

•	SetMyPersonalImageCommand calls IFileService.UploadAsync before mutating the aggregate.

•	A failure after upload leaves an orphan file handled by the File Storage cleanup job.

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

10\. Summary

People integrates with the rest of the system through exactly one synchronous contract: IPersonRepository. It publishes no integration events, consumes none, and hosts no jobs. Consumers read live Person data at the moment they need it and snapshot it if they need history.

The context is deliberately the narrowest integration surface in the system:

•	One repository method exposed cross-context (GetByIdentityUserIdAsync).

•	One REST controller for the authenticated user's own profile.

•	No events in either direction.

•	No cross-context writes — People is authoritative for its data, and only People writes to it.

This keeps the boundary clean, makes the context trivially extractable in a future split, and ensures that historical data ownership stays where the history is created.

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

Document version: 1.0 – Final

Feature: People / Customer Profile and Identity Linkage





