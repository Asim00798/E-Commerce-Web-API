# Catalog Feature – Requirements Technical Document

**Project:** E-Commerce Modular Monolith
**Bounded Context:** Catalog — Brands, Categories, Products
**Version:** 1.0 – Final

---

## 1. Introduction

This document defines the technical requirements for the Catalog bounded context within the e-commerce modular monolith.

Catalog is the foundational context that manages the products, brands, categories, variants, images, tags, pricing, and stock information offered by the business. It is consumed by other contexts such as Ordering and Shipping.

The feature spans Domain, Application, Infrastructure, and API layers following Clean Architecture and DDD principles. The Domain layer is independent of infrastructure concerns.

---

## 2. Scope

### 2.1 In Scope

* Brand management: create, update, and logo handling.
* Category management: create, update, hierarchical parent assignment, and image handling.
* Product management: create, lifecycle, image handling, variant management, tag management, stock management, and search/list queries.
* File references using `Guid FileId` through the shared File Storage capability.
* Application-level file type validation for images.
* Stock management as a synchronous cross-context capability.
* Pagination for list queries.
* Search for published products.
* Authorization at API and Application layers.
* Persistence via EF Core with existing base configurations.

### 2.2 Out of Scope

* Inventory bounded context (warehouses, transfers, reservations, reconciliation).
* External pricing engines or complex pricing rules.
* Discounts, coupons, and promotions.
* Product recommendation engine.
* Customer-facing storefront UI.
* Order placement logic.
* Payment and shipping calculations.
* Advanced media processing or image transformation.

---

## 3. Business Responsibility

Catalog answers:

> What products does the business offer, and what are their attributes?

Catalog owns:

* Brands
* Categories and category hierarchy
* Products and their lifecycle
* Product variants
* SKU
* Price
* Stock quantity (current simplified model)
* Product images
* Product tags
* Product status
* File references

Catalog does **not** own:

* Orders
* Payments
* Shipping
* Customer identity
* Inventory beyond simple stock quantity
* Promotions
* Pricing engines beyond product/variant price

---

## 4. Functional Requirements

### 4.1 Brand Management

**FR-1:** The system shall allow creating a new brand with a name, description, and logo.

**FR-2:** The brand name shall be required and must not exceed 150 characters.

**FR-3:** The brand description shall be optional but must not exceed 500 characters.

**FR-4:** The brand logo shall be required and stored as a `FileId` reference.

**FR-5:** The system shall validate that the uploaded brand logo is a valid image before storing.

**FR-6:** The system shall allow updating the brand name, description, and logo.

**FR-7:** Brand logo replacement shall follow the safe replacement strategy: upload the new file, update the reference, and allow the old unreferenced file to be cleaned up later.

### 4.2 Category Management

**FR-8:** The system shall allow creating a new category with a name, description, and optional parent category.

**FR-9:** The category name shall be required and must not exceed 150 characters.

**FR-10:** The category description shall be required and must not exceed 1000 characters.

**FR-11:** The system shall allow updating the category name, description, parent assignment, and clearing the parent assignment.

**FR-12:** A category shall not be able to assign itself as its own parent.

**FR-13:** The system shall allow adding images to a category.

**FR-14:** The system shall allow removing images from a category by `FileId`.

**FR-15:** Uploaded category images shall be validated as images before storage.

### 4.3 Product Management

**FR-16:** The system shall allow creating a new product with a description, brand ID, category ID, and optional tags.

**FR-17:** The product description shall include a name and optional short description, long description, dimensions, weight, manufacture/expiry dates, material, and color.

**FR-18:** The product shall start in `Draft` status upon creation.

**FR-19:** The system shall allow publishing a product from `Draft` to `Published`.

**FR-20:** The system shall allow moving a published product back to `Draft`.

**FR-21:** The system shall allow discontinuing a product from `Draft` or `Published`.

**FR-22:** The system shall enforce that only `Draft` products can be modified for structural changes such as images, variants, and tags.

**FR-23:** The system shall allow adding images to a product.

**FR-24:** The system shall allow setting one product image as the main image.

**FR-25:** The system shall allow removing product images.

**FR-26:** Uploaded product images shall be validated as images before storage.

### 4.4 Product Variants

**FR-27:** The system shall allow adding a variant to a product with a name, optional SKU, price, and initial stock quantity.

**FR-28:** The variant name shall be required.

**FR-29:** The variant price shall be positive.

**FR-30:** The variant stock quantity shall be non-negative.

**FR-31:** The SKU shall be optional, but if provided, it must be unique within the product.

**FR-32:** The system shall allow updating a variant's price.

**FR-33:** The system shall allow removing a product variant.

**FR-34:** Variant operations shall be allowed only on draft products.

### 4.5 Stock Management

**FR-35:** The system shall allow increasing stock for a product variant.

**FR-36:** The system shall allow decreasing stock for a product variant.

**FR-37:** Stock shall never become negative.

**FR-38:** Stock operations shall be allowed regardless of product status because stock represents inventory, not product lifecycle.

**FR-39:** The Ordering context shall use a shared `IStockService` contract to perform synchronous stock adjustments.

### 4.6 Tags

**FR-40:** The system shall allow adding tags to a product.

**FR-41:** The system shall allow removing tags from a product.

**FR-42:** Duplicate tags shall be ignored case-insensitively.

**FR-43:** Empty tags shall be rejected.

### 4.7 Queries

**FR-44:** The system shall allow retrieving a brand by ID.

**FR-45:** The system shall allow retrieving a paginated list of brands.

**FR-46:** The system shall allow retrieving a category by ID.

**FR-47:** The system shall allow retrieving a paginated list of categories.

**FR-48:** The system shall allow retrieving a product by ID, including its images, variants, and tags.

**FR-49:** The system shall allow retrieving a paginated list of products.

**FR-50:** The system shall allow searching published products by name or short description with pagination.

---

## 5. Non-Functional Requirements

### 5.1 Performance

**NFR-1:** List and search queries should support server-side pagination to avoid loading all records.

**NFR-2:** Product search should not load the entire product table into memory; filtering should occur at the database level.

### 5.2 Scalability

**NFR-3:** The API and Application layers should be stateless.

**NFR-4:** Concurrency conflicts on stock updates should be prevented through aggregate methods and database constraints.

### 5.3 Reliability

**NFR-5:** File upload and entity creation must handle partial failures according to the shared File Storage design.

**NFR-6:** Duplicate brand or category names should be prevented via unique constraints.

**NFR-7:** Duplicate SKU within a product should be prevented at the domain and database levels.

**NFR-8:** Product lifecycle transitions must be atomic and consistent.

### 5.4 Maintainability

**NFR-9:** Domain aggregates must not depend on EF Core, ASP.NET Core, or file storage infrastructure.

**NFR-10:** API controllers must be thin and delegate to MediatR commands and queries.

**NFR-11:** Application handlers must enforce authorization through `[AuthorizePermission]` attributes and resource checks where needed.

### 5.5 Testability

**NFR-12:** Domain rules must be testable without infrastructure.

**NFR-13:** Application handlers must be testable with mocked repositories and services.

**NFR-14:** File validators must be testable with a fake `IFileContentInspector`.

---

## 6. Security Requirements

### 6.1 Authorization

**SEC-1:** Catalog management operations shall require appropriate roles at the API layer.

**SEC-2:** Commands and queries shall carry `[AuthorizePermission]` attributes for fine-grained permission enforcement.

**SEC-3:** Read operations shall be available to authenticated users with the appropriate view permission.

### 6.2 File Type Validation

**SEC-4:** Uploaded brand, category, and product images shall be inspected before storage.

**SEC-5:** The inspector shall return generic technical facts; Application validators shall enforce that files are images.

**SEC-6:** Files with unknown content or mismatched content shall be rejected.

### 6.3 Stock Protection

**SEC-7:** Stock changes shall be performed through the Product aggregate to enforce invariants.

**SEC-8:** Direct external mutation of variant stock shall be prohibited.

### 6.4 Data Protection

**SEC-9:** File IDs shall not expose physical storage paths.

**SEC-10:** Product price and stock data shall not be exposed to unauthorized users.

---

## 7. Integration Requirements

### 7.1 File Storage

* Catalog uses the shared `IFileService` for file uploads.
* Catalog uses the shared `IFileContentInspector` for content inspection.
* Catalog stores only `FileId` references in its aggregates.

### 7.2 Ordering

* Ordering uses the shared `IStockService` to decrease or increase stock synchronously.
* Ordering shall not directly access Catalog repositories or mutate stock.

### 7.3 Persistence

* EF Core configurations shall be provided for Brand, Category, Product, and their child entities.
* Repositories shall implement `IRepository<T>` from SharedKernel.

### 7.4 Authorization

* Permissions shall be defined in `CatalogPermissions`.
* Role-based authorization shall be enforced at the controller level.
* Permission-based authorization shall be enforced at the command/query level.

---

## 8. Configuration Requirements

Catalog requires no external provider configuration.

Possible future configuration:

```text
Catalog:MaximumProductsPerPage
```

Initial defaults:

* Default page size: 20
* Maximum page size: 100

No secrets are required for Catalog itself. File Storage configuration is separate.

---

## 9. Assumptions and Constraints

### 9.1 Assumptions

* Product prices are simple `Money` values; no complex pricing engine is needed.
* The inventory model is simple: one stock quantity per variant.
* The File Storage capability already exists and is registered.
* The Ordering context will be built after Catalog.

### 9.2 Constraints

* Domain must not reference EF Core or ASP.NET Core.
* Catalog must not directly mutate Order, Payment, or Shipping data.
* Catalog must not implement its own physical file storage.
* Stock remains in Catalog until inventory complexity justifies extraction.

---

## 10. Traceability Matrix

| Requirement Area | Description                                        |
| ---------------- | -------------------------------------------------- |
| Brand            | FR-1 to FR-7                                       |
| Category         | FR-8 to FR-15                                      |
| Product          | FR-16 to FR-26                                     |
| Variants         | FR-27 to FR-34                                     |
| Stock            | FR-35 to FR-39                                     |
| Tags             | FR-40 to FR-43                                     |
| Queries          | FR-44 to FR-50                                     |
| Performance      | NFR-1 to NFR-2                                     |
| Scalability      | NFR-3 to NFR-4                                     |
| Reliability      | NFR-5 to NFR-8                                     |
| Maintainability  | NFR-9 to NFR-11                                    |
| Testability      | NFR-12 to NFR-14                                   |
| Authorization    | SEC-1 to SEC-3                                     |
| File validation  | SEC-4 to SEC-6                                     |
| Stock protection | SEC-7 to SEC-8                                     |
| Data protection  | SEC-9 to SEC-10                                    |
| Integration      | File Storage, Ordering, Persistence, Authorization |

---

## 11. Acceptance Criteria

The Catalog feature is considered complete when:

1. Brand, Category, and Product aggregates enforce all specified invariants.
2. File upload validation works for brand logos, category images, and product images.
3. Product lifecycle transitions are enforced.
4. Stock adjustments are synchronous and protected.
5. Ordering can use `IStockService` without coupling to Catalog internals.
6. Pagination and search work at the database level.
7. Authorization is enforced at both API and Application layers.
8. Existing modular-monolith patterns are reused.
9. Domain remains independent of infrastructure.
10. Unit, integration, and API tests cover critical paths.
11. No external-carrier or inventory complexity is introduced.

---

## 12. Summary

Catalog is a foundational, simplified bounded context that manages products, brands, categories, variants, images, tags, pricing, and stock. It integrates with shared File Storage for file references and exposes stock capability to Ordering via a shared application contract. The design balances DDD principles with practical simplicity, making it ready for Ordering integration while remaining maintainable and evolution-friendly.

**Document version:** 1.0 – Final
**Feature:** Catalog / Product, Brand, Category Management
