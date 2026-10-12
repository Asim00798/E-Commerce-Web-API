\# Customer Engagement — Technical Reference Document



\*\*Project:\*\* E-Commerce Modular Monolith

\*\*Bounded Context:\*\* Customer Engagement

\*\*Scope:\*\* Product Ratings and Customer Wishlist

\*\*Version:\*\* 1.3 – Final



\---



\## 1. Introduction



The Customer Engagement bounded context manages customer interactions with products beyond ordering. It owns:



\* \*\*Product Ratings\*\* — customers can rate products on a 1–5 star scale.

\* \*\*Wishlist\*\* — customers can save products to a personal list for later reference.



The context is intentionally simple and does not yet include written reviews or moderation workflows. Additional capabilities can be introduced later without changing the core design.



\---



\## 2. Scope



\### 2.1 In Scope



\* Submit, update, and delete a rating for a product.

\* Retrieve average rating, total count, and distribution for a product.

\* Retrieve a customer's own ratings.

\* Automatically create a wishlist on first use.

\* Add and remove items from a wishlist.

\* Retrieve a customer's wishlist.

\* Authorization at the API and Application layers.



\### 2.2 Out of Scope



\* Written reviews and comments.

\* Moderation or approval of ratings.

\* Public or shared wishlists.

\* Price alerts or notifications for wishlist items.

\* Product catalog management, which is owned by Catalog.

\* Customer identity management, which is owned by Identity/People.



\---



\## 3. Business Responsibility



Customer Engagement answers the question:



> \*\*How do customers express interest in products and save them for later?\*\*



\### This Context Owns



\* Rating records for products.

\* Wishlists and wishlist items.

\* Customer/product associations for these interactions.



\### This Context Does Not Own



\* Product master data, owned by Catalog.

\* Customer identity or personal data, owned by Identity/People.

\* Ordering or payment data.



\---



\## 4. Domain Model



\### 4.1 Aggregates



\#### Rating



Represents a single customer's star rating for a specific product.



\*\*Properties:\*\*



\* `Id`

\* `CustomerId`

\* `ProductId`

\* `StarRating` — value object

\* `CreatedAtUtc`

\* `UpdatedAtUtc`



\*\*Behaviors:\*\*



\* `Create(customerId, productId, starRating)`

\* `UpdateStarRating(starRating)`



\*\*Invariants:\*\*



\* `CustomerId` and `ProductId` are required.

\* `StarRating` must be between 1 and 5 inclusive.

\* A customer can have \*\*one active rating per product\*\*. Updating replaces the current `StarRating` value on the existing `Rating` aggregate; it does not create a new aggregate.

\* Deletion is \*\*physical\*\*: the `Rating` record is permanently removed from the database. Physical deletion is performed through the repository's `HardDeleteAsync` operation, not through a domain method.



\*\*No domain events are raised by the aggregate in this version.\*\*



\#### Wishlist



Represents a customer's personal list of saved products.



\*\*Properties:\*\*



\* `Id`

\* `CustomerId`

\* `Items` — collection of `WishlistItem` entities

\* `CreatedAtUtc`



\*\*Behaviors:\*\*



\* `Create(customerId)` — internal; called automatically when first needed.

\* `AddItem(productId)`

\* `RemoveItem(productId)`

\* `Clear()`



\*\*Invariants:\*\*



\* A customer can have \*\*at most one\*\* wishlist. This is enforced by a unique index on `CustomerId`.

\* Each product can appear only once in the wishlist.

\* Adding an existing product is idempotent and is ignored.

\* Removing a nonexistent product is ignored.



\*\*No domain events are raised by the aggregate in this version.\*\*



\#### Entities



\* \*\*`WishlistItem`\*\* — owned by `Wishlist`. Contains `ProductId` and `AddedAtUtc`.



\### 4.2 Value Objects



\#### StarRating



A value object representing a rating on a 1–5 scale.



```csharp

public sealed record StarRating

{

&#x20;   public int Value { get; }



&#x20;   public StarRating(int value)

&#x20;   {

&#x20;       if (value < 1 || value > 5)

&#x20;           throw new RatingException("Star rating must be between 1 and 5.");



&#x20;       Value = value;

&#x20;   }

}

```



\### 4.3 Repositories



\#### `IRatingRepository : IRepository<Rating>`



Specialized operations:



```csharp

Task<Rating?> GetByCustomerAndProductAsync(

&#x20;   Guid customerId,

&#x20;   Guid productId,

&#x20;   CancellationToken ct = default);



Task<IReadOnlyList<Rating>> GetByProductIdAsync(

&#x20;   Guid productId,

&#x20;   CancellationToken ct = default);



Task<IReadOnlyList<Rating>> GetByCustomerIdAsync(

&#x20;   Guid customerId,

&#x20;   CancellationToken ct = default);



Task<ProductRatingsSummary> GetProductRatingsSummaryAsync(

&#x20;   Guid productId,

&#x20;   CancellationToken ct = default);

```



`GetProductRatingsSummaryAsync` performs database-side aggregation to retrieve the average rating, total count, and rating distribution.



\#### `IWishlistRepository : IRepository<Wishlist>`



Specialized operation:



```csharp

Task<Wishlist?> GetByCustomerIdAsync(

&#x20;   Guid customerId,

&#x20;   CancellationToken ct = default);

```



Additionally, the generic `IRepository<T>` interface exposes:



```csharp

Task<bool> HardDeleteAsync(

&#x20;   Guid id,

&#x20;   CancellationToken ct = default);

```



This method issues a direct SQL `DELETE` using EF Core's `ExecuteDeleteAsync`, bypassing change tracking and the `SaveChanges` pipeline. It also bypasses the application's normal soft-delete mechanism, if one exists.



\---



\## 5. Application Layer



\### 5.1 Commands



| Command                     | Purpose                                                                                   | Permission Attribute                     |

| --------------------------- | ----------------------------------------------------------------------------------------- | ---------------------------------------- |

| `SubmitRatingCommand`       | Create a new rating for a product, or update the existing rating when one already exists. | `CustomerEngagementPermissions.Rate`     |

| `UpdateRatingCommand`       | Update an existing rating.                                                                | `CustomerEngagementPermissions.Rate`     |

| `DeleteRatingCommand`       | Delete a rating.                                                                          | `CustomerEngagementPermissions.Rate`     |

| `AddWishlistItemCommand`    | Add a product to the wishlist, creating the wishlist automatically if needed.             | `CustomerEngagementPermissions.Wishlist` |

| `RemoveWishlistItemCommand` | Remove a product from the wishlist.                                                       | `CustomerEngagementPermissions.Wishlist` |



There is no separate `CreateWishlistCommand`; the wishlist is created automatically when the first item is added.



\### 5.2 Queries



| Query                      | Purpose                                                                          | Permission Attribute                 |

| -------------------------- | -------------------------------------------------------------------------------- | ------------------------------------ |

| `GetProductRatingsQuery`   | Retrieve the average rating, total count, and rating distribution for a product. | `CustomerEngagementPermissions.Read` |

| `GetCustomerRatingsQuery`  | Retrieve all ratings submitted by a customer.                                    | `CustomerEngagementPermissions.Read` |

| `GetCustomerWishlistQuery` | Retrieve a customer's wishlist.                                                  | `CustomerEngagementPermissions.Read` |



Queries perform resource checks using the current-user context. Customers access their own ratings and wishlist through that context. Administrative or support access is permitted only where explicitly configured.



\### 5.3 Domain Event Handlers



None. No domain events exist in this version.



\### 5.4 Integration Events



None. The context is self-contained and does not communicate asynchronously with other bounded contexts.



\---



\## 6. Infrastructure Layer



\### 6.1 Persistence



\* `AppDbContext.CustomerEngagement.cs` contains `DbSet<Rating>` and `DbSet<Wishlist>`.

\* There is no independent `DbSet<WishlistItem>`; items are managed through the `Wishlist` aggregate configuration.



\*\*Configurations:\*\*



\* `RatingConfiguration`

\* `WishlistConfiguration`, including the owned relationship for `WishlistItem`.



All configurations inherit from `BaseEntityConfiguration<T>` and define table names, keys, relationships, owned value objects (`StarRating`), and the `RowVersion` shadow property where appropriate.



\### 6.2 Database Constraints



| Entity         | Constraint                                | Index Name                              |

| -------------- | ----------------------------------------- | --------------------------------------- |

| `Rating`       | Unique index on `(CustomerId, ProductId)` | `UX\_Ratings\_CustomerId\_ProductId`       |

| `Wishlist`     | Unique index on `CustomerId`              | `UX\_Wishlists\_CustomerId`               |

| `WishlistItem` | Unique index on `(WishlistId, ProductId)` | `UX\_WishlistItems\_WishlistId\_ProductId` |



\### 6.3 Repositories



\* `RatingRepository` implements `IRatingRepository`.

\* `WishlistRepository` implements `IWishlistRepository` and includes `Items` when loading a wishlist.

\* Repositories are discovered by the shared `AddRepositories` Scrutor scan on the Infrastructure assembly. The scan registers classes whose names end in `Repository` against their implemented interfaces with scoped lifetime.

\* Customer Engagement has no context-specific DI extension because it has no infrastructure services to register beyond the automatically discovered repositories.



\### 6.4 Generic Hard Delete



The base `Repository<T>` provides a generic `HardDeleteAsync` implementation using EF Core's `ExecuteDeleteAsync`.



This operation:



\* Issues a direct SQL `DELETE`.

\* Bypasses EF Core change tracking.

\* Does not require `SaveChangesAsync` to execute the deletion.

\* Bypasses soft-delete interceptors that operate during `SaveChanges`.



This method is used for permanent deletion of ratings.



\---



\## 7. API Layer



Controllers are thin, use role-based authorization, and delegate to MediatR commands and queries.



\### 7.1 Controllers



| Controller           | Route                      | Role       |

| -------------------- | -------------------------- | ---------- |

| `RatingsController`  | `/api/engagement/ratings`  | `Customer` |

| `WishlistController` | `/api/engagement/wishlist` | `Customer` |



\### 7.2 Endpoints



\#### Ratings



```http

POST   /api/engagement/ratings

PUT    /api/engagement/ratings/{ratingId}

DELETE /api/engagement/ratings/{ratingId}

GET    /api/engagement/ratings/product/{productId}

GET    /api/engagement/ratings/customer

```



The product-rating summary endpoint permits anonymous access.



\#### Wishlist



```http

GET    /api/engagement/wishlist

POST   /api/engagement/wishlist/items

DELETE /api/engagement/wishlist/items/{productId}

```



There is no explicit wishlist creation endpoint. The wishlist is created automatically on the first `POST /api/engagement/wishlist/items` request.



\### 7.3 Wishlist GET Contract



If the customer has no wishlist, the endpoint returns a successful response containing an empty wishlist representation rather than a failure.



The Application layer must return a valid `WishlistDto` with no items, and the API must handle that response without assuming `Data` is non-null.



Permissions are enforced at the Application layer; controllers enforce roles.



\---



\## 8. Authorization



\### 8.1 Permissions



```csharp

public static class CustomerEngagementPermissions

{

&#x20;   public const string Read = "Engagement.Read";

&#x20;   public const string Rate = "Engagement.Rate";

&#x20;   public const string Wishlist = "Engagement.Wishlist";

}

```



\### 8.2 Enforcement



\* \*\*API layer:\*\* Role-based authorization (`Customer` for customer endpoints). The product-rating summary endpoint uses `\[AllowAnonymous]`.

\* \*\*Application layer:\*\* `\[AuthorizePermission]` attributes on commands and queries.

\* \*\*Handlers:\*\* Resource ownership checks are performed through the current-user context. Customers can only manage their own ratings and wishlist.



No `Manage` permission exists in this version; administrative and support access is not implemented.



\---



\## 9. Workflows



\### 9.1 Submit Rating



```text

Customer

&#x20;  ↓ POST /api/engagement/ratings

RatingsController (role: Customer)

&#x20;  ↓ SubmitRatingCommand

SubmitRatingCommandHandler

&#x20;  ├── Check whether the customer already has a rating for the product

&#x20;  │      ├── If it exists → update the existing rating

&#x20;  │      └── Otherwise → create a new rating

&#x20;  ├── Add or update through the repository

&#x20;  └── UnitOfWork.SaveChangesAsync

```



\### 9.2 Update Rating



1\. Load the rating.

2\. Verify ownership.

3\. Call `UpdateStarRating`.

4\. Save through the Unit of Work.



\### 9.3 Delete Rating



```text

Load rating by ID

&#x20;  ↓

Verify ownership

&#x20;  ↓

Call IRatingRepository.HardDeleteAsync(ratingId)

&#x20;  ↓

Return success

```



No `IUnitOfWork` or `SaveChangesAsync` call is required; the hard delete executes immediately.



\### 9.4 Add Wishlist Item



1\. Load the wishlist by customer ID.

2\. If no wishlist exists, create one.

3\. Call `AddItem(productId)`.

4\. Save through the Unit of Work.



\### 9.5 Remove Wishlist Item



1\. Load the wishlist by customer ID.

2\. If no wishlist exists, return success without making changes.

3\. Call `RemoveItem(productId)`.

4\. Save through the Unit of Work.



\---



\## 10. Transaction Boundaries and Consistency



\* Each mutation that uses tracked entity changes, such as creation or update, is persisted through the application's Unit of Work. Database changes for a mutation are committed atomically.

\* The hard-delete operation (`HardDeleteAsync`) is a direct database command and does not require `SaveChangesAsync` or `IUnitOfWork`.

\* Queries execute as read operations and do not require an explicit transaction.

\* No cross-context synchronous operations are required.



\---



\## 11. Concurrency and Idempotency



\* `RowVersion` optimistic concurrency is configured for `Rating` and `Wishlist`.

\* Unique database indexes enforce invariants at the persistence level.

\* Application handlers currently do \*\*not\*\* implement retry or fallback behavior for unique-constraint violations; this is deferred. The database remains the final protection.

\* Idempotent behavior is implemented in the domain for wishlist item addition (ignore duplicates) and removal (ignore missing items).

\* For ratings, duplicate submissions result in an update, but race-condition handling is not yet implemented.



\---



\## 12. Testing



\### 12.1 Domain Tests



\* `StarRating` validation.

\* Rating creation and update behavior; there is no domain-level delete method.

\* Wishlist item addition and removal invariants.



\### 12.2 Application Tests



\* Command handlers with mocked repositories.

\* Resource authorization checks.

\* Idempotent wishlist item addition.



\### 12.3 Integration Tests



\* EF Core mappings, unique constraints, and concurrency tokens.

\* Physical deletion of ratings using `HardDeleteAsync`, verifying direct SQL deletion and bypass of soft delete.



\### 12.4 API Tests



\* Role-based authorization.

\* Customer isolation.

\* CRUD operations.



\---



\## 13. Summary



The Customer Engagement bounded context provides focused capabilities for product ratings and customer wishlists.



The domain is intentionally minimal: no domain events are raised, and the `Rating` aggregate does not expose a deletion method. Physical deletion is achieved through a generic hard-delete method that issues a direct SQL `DELETE`, bypassing the normal `SaveChanges` pipeline and soft-delete mechanism.



The feature remains self-contained, with no integration events or cross-context dependencies. The design aims to keep the domain focused and maintainable while leaving room for future extensions.



\---



\*\*Document Version:\*\* 1.3 – Final

\*\*Feature:\*\* Customer Engagement — Product Ratings and Wishlist



