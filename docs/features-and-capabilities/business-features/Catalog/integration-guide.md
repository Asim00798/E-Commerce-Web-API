# Catalog Integration Guide

**Project:** E-Commerce Modular Monolith
**Purpose:** How Other Contexts and Clients Integrate with Catalog
**Version:** 1.0 – Final

---

## 1. Purpose

This document explains how other bounded contexts, especially Ordering, and client applications integrate with the Catalog bounded context.

It covers:

* Shared cross-context stock contract (`IStockService`).
* File Storage integration for brand, category, and product images.
* Application-level file validation.
* REST API integration for management and queries.
* Authorization and permission requirements.
* Integration rules and boundaries.

---

## 2. Integration Overview

Catalog is a foundational bounded context that owns product-related business data and logic. Other contexts do not modify Catalog internals directly. Instead, they interact through:

1. **Shared Application Contracts**

   Currently, `IStockService` for synchronous stock adjustments.

2. **Shared File Storage Contracts**

   `IFileService` and `IFileContentInspector` are used by Catalog Application handlers for uploads.

3. **REST APIs**

   Management endpoints for administrators and catalog managers, and query endpoints for authenticated users.

4. **Future Integration Events**

   These may be added for product lifecycle changes or stock alerts when needed.

---

## 3. Stock Integration for Ordering

### 3.1 Shared Contract

Ordering depends on `IStockService` from `Application.Shared.Stock`.

```csharp
public interface IStockService
{
    Task<Result> DecreaseStockAsync(
        Guid productId,
        Guid variantId,
        int quantity,
        CancellationToken ct = default);

    Task<Result> IncreaseStockAsync(
        Guid productId,
        Guid variantId,
        int quantity,
        CancellationToken ct = default);
}
```

Ordering **does not** access `IProductRepository` or modify `ProductVariant.StockQuantity` directly.

### 3.2 Synchronous Operation

Stock adjustment is synchronous because it participates in the order placement transaction.

**Flow:**

```text
Ordering Checkout/Place Order
       ↓
IStockService.DecreaseStockAsync(...)
       ↓
Catalog StockService
       ↓
ProductRepository.GetByIdAsync
       ↓
Product.DecreaseStock(variantId, quantity)
       ↓
UnitOfWork.SaveChangesAsync
```

If stock cannot be decreased, the entire order transaction rolls back.

### 3.3 Implementation Detail

`StockService` is implemented in Infrastructure and registered through dependency injection:

```csharp
services.AddScoped<IStockService, StockService>();
```

### 3.4 Future Evolution

If Inventory becomes a separate context, the same `IStockService` interface can be implemented by Inventory without changing Ordering, provided the contract's semantics remain compatible.

---

## 4. File Storage Integration

Catalog stores only `FileId` references:

* `BrandLogo.FileId`
* `CategoryImage.FileId`
* `ProductImage.FileId`

### 4.1 File Upload Flow

When creating or updating entities with files, the Application handler:

1. Receives a `FileUpload` model.
2. Validates the file using the appropriate validator.
3. Calls `IFileService.UploadAsync`.
4. Receives a `FileId`.
5. Sets the `FileId` on the aggregate.
6. Persists the aggregate.

### 4.2 Validators

| Validator                    | Expected `FileType` |
| ---------------------------- | ------------------- |
| `BrandLogoFileValidator`     | `Image`             |
| `CategoryImageFileValidator` | `Image`             |
| `ProductImageFileValidator`  | `Image`             |

Validators use `IFileContentInspector` to detect the actual content type and validation status.

### 4.3 Rules

* File content inspection is generic; business policies live in validators.
* Catalog does not depend on `IFileStorageProvider`.
* Physical storage details remain hidden.
* Old files become unreferenced and are cleaned up by the File Storage subsystem.

---

## 5. REST API Integration

### 5.1 Controllers

| Controller             | Route                     | Primary Role                                                                |
| ---------------------- | ------------------------- | --------------------------------------------------------------------------- |
| `BrandsController`     | `/api/catalog/brands`     | `CatalogManager`, `Administrator` for writes; authenticated users for reads |
| `CategoriesController` | `/api/catalog/categories` | Same as above                                                               |
| `ProductsController`   | `/api/catalog/products`   | Same as above                                                               |

### 5.2 Common Endpoints

The following endpoints describe the expected API surface.

#### Brands

```http
POST   /api/catalog/brands
PUT    /api/catalog/brands/{id}
GET    /api/catalog/brands/{id}
GET    /api/catalog/brands
```

#### Categories

```http
POST   /api/catalog/categories
PUT    /api/catalog/categories/{id}
GET    /api/catalog/categories/{id}
GET    /api/catalog/categories
POST   /api/catalog/categories/{id}/images
DELETE /api/catalog/categories/{id}/images/{fileId}
```

#### Products

```http
POST   /api/catalog/products
GET    /api/catalog/products/{id}
GET    /api/catalog/products
GET    /api/catalog/products/search

POST   /api/catalog/products/{id}/publish
POST   /api/catalog/products/{id}/draft
POST   /api/catalog/products/{id}/discontinue

POST   /api/catalog/products/{id}/images
POST   /api/catalog/products/{id}/images/{imageId}/main
DELETE /api/catalog/products/{id}/images/{imageId}

POST   /api/catalog/products/{id}/variants
PUT    /api/catalog/products/{id}/variants/{variantId}/price
DELETE /api/catalog/products/{id}/variants/{variantId}

POST   /api/catalog/products/{id}/tags
DELETE /api/catalog/products/{id}/tags

POST   /api/catalog/products/{id}/variants/{variantId}/stock/increase
POST   /api/catalog/products/{id}/variants/{variantId}/stock/decrease
```

### 5.3 Request/Response DTOs

Request DTOs are defined under:

```text
API/DTOs/Catalog/
```

Response DTOs are separate from Application DTOs.

---

## 6. Authorization Integration

### 6.1 Permissions

`CatalogPermissions` defines:

```text
Catalog.Brands.Read
Catalog.Brands.Manage

Catalog.Categories.Read
Catalog.Categories.Manage

Catalog.Products.Read
Catalog.Products.Manage
```

### 6.2 API Layer

Controllers use role-based authorization:

```csharp
[Authorize(Roles = "CatalogManager,Administrator")]
```

### 6.3 Application Layer

Commands and queries use fine-grained permission attributes:

```csharp
[AuthorizePermission(CatalogPermissions.ManageBrands)]
```

### 6.4 Stock Service

`StockService` does not enforce explicit permissions itself. Authorization for order placement is enforced by Ordering use cases before they call `IStockService`.

---

## 7. Integration Rules

* Do not modify Catalog tables directly from other contexts.
* Use `IStockService` for stock changes from Ordering.
* Do not call Catalog repositories from Ordering.
* Do not reference `Product`, `ProductVariant`, or other Catalog domain types directly from other contexts.
* Use shared contracts for cross-context operations.
* File uploads must be validated by the owning context's validator.
* Catalog does not publish integration events unless a real consumer exists.

---

## 8. Common Integration Scenarios

### 8.1 Ordering Places an Order

Ordering calls `IStockService.DecreaseStockAsync` for each order line before finalizing the order.

### 8.2 Ordering Calculates Totals

Ordering may need product prices from Catalog. For the current simplified scope, Ordering can include the price in order-line input or call a Catalog query/contract. A dedicated shared product-query contract may be added later.

### 8.3 Admin Creates a Product

An administrator uses the Products API to create a product, add variants, images, and tags, and then publish it.

### 8.4 Customer Views Products

An authenticated user calls the product list/search endpoints with the `Catalog.Products.Read` permission.

---

## 9. Testing Integration

### 9.1 Unit Tests

* Stock service uses a mocked repository.
* Validators use a fake inspector.

### 9.2 Integration Tests

* Stock adjustments persist correctly.
* Product lifecycle transitions are enforced.
* Database unique constraints work.

### 9.3 API Tests

* Role-based access is enforced.
* File uploads accept valid images and reject invalid files.
* Pagination and search return correct results.

---

## 10. Summary

Catalog integrates with other contexts primarily through shared Application contracts. `IStockService` is the main synchronous cross-context capability for Ordering.

File Storage remains generic and is hidden behind the shared `IFileService` and `IFileContentInspector`, with context-specific validators. REST APIs support management and queries with layered authorization.

This design keeps Catalog cohesive while allowing future extraction of Inventory without affecting consumers, provided the cross-context contracts remain stable.
