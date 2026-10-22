# Customer Engagement — Requirements Document

**Project:** E-Commerce Modular Monolith
**Bounded Context:** Customer Engagement
**Scope:** Product Ratings and Customer Wishlist
**Version:** 1.2 – Final

---

## 1. Introduction

The Customer Engagement feature allows customers to interact with products by rating them and saving them to a personal wishlist. These capabilities are separate from purchasing and are intended to enhance the customer experience and provide product feedback.

This document defines the business requirements, functional and non-functional needs, and acceptance criteria. Implementation details are intentionally omitted and are covered in the Technical Reference Document.

---

## 2. Business Goals

* Enable customers to express product satisfaction through a simple 1–5 star rating.
* Allow customers to save products for later consideration via a wishlist.
* Provide product rating summaries, including average rating, rating count, and rating distribution.
* Maintain a clean separation between engagement data and core commerce data (Catalog, Ordering, and Identity).

---

## 3. Functional Requirements

### 3.1 Product Ratings

**R1.** Any authenticated customer may rate any product on a scale of 1 to 5 stars.

**R2.** A customer can have only one active rating per product.

* If the customer submits a rating for a product they have already rated, the existing rating is updated.

**R3.** The customer may update their rating for a product at any time.

**R4.** The customer may delete their rating for a product.

* Deleting a rating permanently removes it; it is not soft-deleted.
* Once deleted, the customer may submit a new rating for the same product later.

**R5.** The system must be able to provide the following product rating summaries:

* Average star rating for a product (e.g., 4.3).
* Total number of ratings for a product.
* Distribution of ratings (count for each star value) for a product.

**R6.** A customer may view all ratings they have submitted.

**R7.** The system does not require the customer to have purchased the product to rate it. Any authenticated customer is eligible.

**R8.** Product rating operations do not require product existence validation at this stage. `ProductId` is treated as an external identifier. Validation can be added later if the business requires it.

### 3.2 Wishlist

**R9.** Each authenticated customer has at most one wishlist.

**R10.** The wishlist is created automatically the first time the customer adds a product to it. There is no separate wishlist creation operation.

**R11.** A customer may add a product to their wishlist.

* If the product is already in the wishlist, the operation is treated as idempotent; no duplicate entry is created.

**R12.** A customer may remove a product from their wishlist.

* Removing a product that is not in the wishlist is ignored.

**R13.** A customer may view their wishlist, including all saved products and the date each product was added.

**R14.** The wishlist does not support public sharing, price alerts, or notifications in this version.

---

## 4. Non-Functional Requirements

### 4.1 Performance

* Rating and wishlist operations should be responsive under typical database latency.
* Product rating summaries (average, count, and distribution) must be computed efficiently to support product pages.

### 4.2 Reliability

* Rating and wishlist operations must be atomic. A failure should leave the system in a consistent state.
* Duplicate rating submissions or wishlist additions must not cause data corruption.

### 4.3 Scalability

* The system should support growth in rating and wishlist data without requiring architectural redesign.

### 4.4 Maintainability

* The domain must remain independent of infrastructure concerns.
* Future extensions (e.g., written reviews, moderation, and wishlist sharing) should be possible without major rework.

### 4.5 Security

* Customers may only manage their own ratings and wishlist.
* Administrators or support personnel may have read access to ratings for moderation or analytics purposes.

---

## 5. Business Rules

* **StarRating:** Must be an integer from 1 to 5 inclusive.
* **One active rating per customer per product:** Enforced.
* **One wishlist per customer:** Enforced.
* **Rating deletion is permanent:** No retention or audit requirement exists.
* **Wishlist creation is implicit:** A wishlist is created upon the first addition; no standalone creation action is available.
* **Rating eligibility:** All authenticated customers may rate products; a purchase is not required.

---

## 6. Assumptions and Constraints

* Customers are authenticated and have a valid user identity.
* Product identifiers are stable and owned by the Catalog context.
* The context does not need to communicate with other contexts for its core operations.
* Written reviews and moderation are intentionally deferred.

---

## 7. Out of Scope

* Written reviews and comments.
* Rating moderation or approval workflows.
* Purchase-based rating eligibility.
* Public or shared wishlists.
* Wishlist price alerts or notifications.
* Product catalog management.
* Customer identity and profile management.

---

## 8. Use Cases (High-Level)

### UC-1: Rate a Product

**Actor:** Customer
**Preconditions:** The customer is authenticated.

**Main Flow:**

1. The customer selects a product and chooses a star rating from 1 to 5.
2. The system creates a new rating for that product.
3. If an existing rating exists, the system updates it instead of creating a duplicate.
4. The system confirms success.

### UC-2: Update Rating

**Actor:** Customer
**Preconditions:** The customer has an existing rating for a product.

**Main Flow:**

1. The customer changes the star rating.
2. The system updates the existing rating record.

### UC-3: Delete Rating

**Actor:** Customer
**Preconditions:** The customer has an existing rating.

**Main Flow:**

1. The customer removes the rating.
2. The system permanently deletes the rating record.

### UC-4: View Product Ratings

**Actor:** Customer or Guest
**Preconditions:** None.

**Main Flow:**

1. The user views a product.
2. The system displays the average rating, total rating count, and rating distribution.

### UC-5: Add to Wishlist

**Actor:** Customer
**Preconditions:** The customer is authenticated.

**Main Flow:**

1. The customer chooses to save a product.
2. The system automatically creates a wishlist if none exists.
3. The system adds the product to the wishlist, ignoring duplicates.

### UC-6: Remove from Wishlist

**Actor:** Customer
**Preconditions:** The customer has a wishlist containing the product.

**Main Flow:**

1. The customer removes the product.
2. The system updates the wishlist.

### UC-7: View Wishlist

**Actor:** Customer
**Preconditions:** The customer is authenticated.

**Main Flow:**

1. The customer opens their wishlist.
2. The system displays all saved products.

---

## 9. Acceptance Criteria

### 9.1 Rating

1. A customer can submit a rating between 1 and 5 for any product.
2. Submitting a second rating for the same product updates the existing rating rather than creating a duplicate.
3. Deleting a rating permanently removes it.
4. The product's average rating, total count, and rating distribution accurately reflect the current ratings.
5. A customer can view all their own ratings.

### 9.2 Wishlist

1. Each customer has at most one wishlist.
2. The wishlist is created automatically when the first product is added.
3. Adding a product already in the wishlist is idempotent.
4. Removing a product updates the wishlist correctly.
5. A customer can view their wishlist and its saved products.

### 9.3 Authorization

1. Customers can only access and modify their own ratings and wishlist.
2. Unauthorized access is rejected.

### 9.4 Reliability

1. Concurrent duplicate submissions do not create duplicate ratings or wishlist items.
2. The system remains consistent after any operation.

---

## 10. Future Considerations

* Written reviews and review moderation.
* Purchase-based rating eligibility.
* Wishlist sharing and price alerts.
* Integration events for rating updates (e.g., refreshing the Catalog's average rating).

---

**Document Version:** 1.2 – Final
**Feature:** Customer Engagement Requirements
