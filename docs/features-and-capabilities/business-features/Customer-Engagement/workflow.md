# Customer Engagement — Workflow Technical Document

**Project:** E-Commerce Modular Monolith
**Scope:** Product Ratings, Wishlist Management, and Customer Interaction
**Version:** 1.3 – Final

---

## 1. Overview

This document defines the end-to-end workflows for the Customer Engagement bounded context. It covers:

* Submitting and updating product ratings.
* Deleting product ratings.
* Retrieving product rating summaries.
* Retrieving a customer's own ratings.
* Adding products to a wishlist, with automatic wishlist creation.
* Removing products from a wishlist.
* Retrieving a customer's wishlist.

The context is self-contained and does not produce or consume integration events. All operations are local to Customer Engagement and use synchronous database transactions through the application's Unit of Work, with the exception of physical rating deletion, which uses a direct SQL `DELETE`.

---

## 2. Actors and Components

### 2.1 Actors

| Actor         | Description                                                             |
| ------------- | ----------------------------------------------------------------------- |
| Customer      | Authenticated user who can rate products and manage their wishlist.     |
| Guest         | Anonymous user who can view product rating summaries.                   |
| Admin/Support | Not explicitly used in this version; may be added later for moderation. |

### 2.2 Key Components

| Component                 | Layer                 | Responsibility                                                            |
| ------------------------- | --------------------- | ------------------------------------------------------------------------- |
| `Rating` aggregate        | Domain                | Rating lifecycle, invariants, and star value validation.                  |
| `Wishlist` aggregate      | Domain                | Wishlist lifecycle, item management, and invariants.                      |
| `StarRating` value object | Domain                | Enforces the 1–5 rating scale.                                            |
| `IRatingRepository`       | Domain/Infrastructure | Rating persistence, summary queries, and hard deletion.                   |
| `IWishlistRepository`     | Domain/Infrastructure | Wishlist persistence and retrieval.                                       |
| `IUnitOfWork`             | SharedKernel          | Transaction boundary for tracked mutations, such as creation and updates. |
| Application handlers      | Application           | Execute commands and queries and orchestrate domain logic.                |
| EF Core configurations    | Infrastructure        | Database mappings, unique constraints, and `RowVersion` configuration.    |

**Note:** `IUnitOfWork` is not used in the rating deletion workflow because `HardDeleteAsync` executes a direct SQL `DELETE` outside the change tracker.

---

## 3. Workflow 1: Submit or Update Rating

### 3.1 Description

A customer rates a product. If they have already rated that product, the existing rating is updated; otherwise, a new rating is created.

### 3.2 Flow

```text
Customer
   ↓ POST /api/engagement/ratings
RatingsController (role: Customer)
   ↓ SubmitRatingCommand
SubmitRatingCommandHandler
   ├── Load existing rating by CustomerId + ProductId
   │      ├── If exists → update StarRating
   │      └── If not exists → create new Rating
   ├── Add or update through the repository
   ├── UnitOfWork.SaveChangesAsync
   └── Return rating ID
```

### 3.3 Important Rules

* `StarRating` must be between 1 and 5 inclusive.
* One rating per customer per product is enforced by a unique database index.
* If a rating already exists, `UpdateStarRating` replaces the value on the existing aggregate; it does not create a new record.

---

## 4. Workflow 2: Delete Rating

### 4.1 Description

A customer removes their own rating for a product. The deletion is permanent and performed through a direct database command.

### 4.2 Flow

```text
Customer
   ↓ DELETE /api/engagement/ratings/{ratingId}
RatingsController (role: Customer)
   ↓ DeleteRatingCommand
DeleteRatingCommandHandler
   ├── Load rating by ID
   ├── Verify resource ownership
   ├── Call IRatingRepository.HardDeleteAsync(ratingId)
   └── Return success
```

### 4.3 Important Rules

* Only the owner of the rating can delete it.
* Deletion is physical; no soft delete is used.
* `HardDeleteAsync` uses EF Core's `ExecuteDeleteAsync`, which issues a direct SQL `DELETE`, bypassing the change tracker and `SaveChanges` pipeline.
* As a result, `IUnitOfWork` is not required for this operation.

---

## 5. Workflow 3: Get Product Ratings Summary

### 5.1 Description

Retrieve the average rating, total count, and distribution for a product. This operation is available to guests.

### 5.2 Flow

```text
User (Guest or Customer)
   ↓ GET /api/engagement/ratings/product/{productId}
RatingsController ([AllowAnonymous])
   ↓ GetProductRatingsQuery
GetProductRatingsQueryHandler
   ├── Call IRatingRepository.GetProductRatingsSummaryAsync(productId)
   │      └── Database-side aggregation (AVG, COUNT, GROUP BY)
   ├── Map result to ProductRatingsSummaryDto
   └── Return summary
```

### 5.3 Important Rules

* Aggregation is performed in the database, not in memory.
* If no ratings exist, return a zero-valued summary with an empty distribution.

---

## 6. Workflow 4: Get Customer's Own Ratings

### 6.1 Description

A customer retrieves all ratings they have submitted.

### 6.2 Flow

```text
Customer
   ↓ GET /api/engagement/ratings/customer
RatingsController (role: Customer)
   ↓ GetCustomerRatingsQuery
GetCustomerRatingsQueryHandler
   ├── Call IRatingRepository.GetByCustomerIdAsync(currentUserId)
   ├── Map results to a list of RatingDto objects
   └── Return list
```

### 6.3 Important Rules

* Only the authenticated customer's ratings are returned.
* No administrative override is available in this version.

---

## 7. Workflow 5: Add to Wishlist (Auto-Create)

### 7.1 Description

A customer adds a product to their wishlist. If they do not have a wishlist, one is created automatically.

### 7.2 Flow

```text
Customer
   ↓ POST /api/engagement/wishlist/items
WishlistController (role: Customer)
   ↓ AddWishlistItemCommand
AddWishlistItemCommandHandler
   ├── Load wishlist by CustomerId
   │      └── If null → Wishlist.Create(customerId)
   ├── wishlist.AddItem(productId)
   │      ├── If product exists → idempotent no-op
   │      └── If new → add item
   ├── Add or update through the repository
   └── UnitOfWork.SaveChangesAsync
```

### 7.3 Important Rules

* There is no separate wishlist creation endpoint.
* One wishlist per customer is enforced by a unique database index.
* Adding an existing product does not create a duplicate.

---

## 8. Workflow 6: Remove from Wishlist

### 8.1 Description

A customer removes a product from their wishlist.

### 8.2 Flow

```text
Customer
   ↓ DELETE /api/engagement/wishlist/items/{productId}
WishlistController (role: Customer)
   ↓ RemoveWishlistItemCommand
RemoveWishlistItemCommandHandler
   ├── Load wishlist by CustomerId
   │      └── If null → return success (no-op)
   ├── wishlist.RemoveItem(productId)
   │      ├── If present → remove
   │      └── If absent → no-op
   ├── Update repository (only if wishlist existed)
   └── UnitOfWork.SaveChangesAsync (if needed)
```

### 8.3 Important Rules

* Removing a product that is not in the wishlist is ignored.
* If the customer has no wishlist, the operation returns success without making changes.

---

## 9. Workflow 7: Get Wishlist

### 9.1 Description

A customer retrieves their wishlist. If none exists, an empty wishlist is returned.

### 9.2 Flow

```text
Customer
   ↓ GET /api/engagement/wishlist
WishlistController (role: Customer)
   ↓ GetCustomerWishlistQuery
GetCustomerWishlistQueryHandler
   ├── Call IWishlistRepository.GetByCustomerIdAsync(currentUserId)
   │      ├── If null → return empty WishlistDto
   │      └── If found → map to WishlistDto
   └── Return result
```

### 9.3 Important Rules

* The API must return a successful response with an empty wishlist representation when no wishlist exists.
* The API must not treat the absence of a persisted wishlist as a failure.

---

## 10. Domain Events

**No domain events are raised by any aggregate in this version.**

If future cross-context communication is required, domain events can be introduced and translated into integration events through the Outbox.

---

## 11. Transaction Boundaries

* Each mutation that uses tracked entity changes, such as creation or update, is persisted through the application's `IUnitOfWork`.
* Database changes for a mutation are committed atomically.
* The hard-delete operation (`HardDeleteAsync`) is a direct database command and does not require `SaveChangesAsync` or `IUnitOfWork`.
* Queries execute as read operations and do not require an explicit transaction.
* No cross-context synchronous operations are required.

---

## 12. Concurrency and Idempotency

* The `RowVersion` shadow property on `Rating` and `Wishlist` provides optimistic concurrency detection.
* Unique database indexes enforce invariants at the persistence level.
* Application-level fallback and recovery for unique-constraint violations are intentionally deferred and are not implemented in this version.
* Idempotent behavior is implemented in the domain for wishlist item addition and removal.
* For rating submissions, the handler checks for an existing rating and updates it, but race-condition recovery is deferred.

---

## 13. Conclusion

The Customer Engagement workflows are simple, self-contained, and aligned with the overall modular monolith architecture. They rely on domain aggregates for invariants, application handlers for orchestration, and Infrastructure for persistence and constraints.

The absence of domain events and integration events keeps this context independent and easy to maintain. Physical deletion is handled through a direct SQL command, while other mutations use the standard Unit of Work. Future extensions can introduce cross-context communication without altering the core workflows.

---

**Document Version:** 1.3 – Final
**Feature:** Customer Engagement Workflows
