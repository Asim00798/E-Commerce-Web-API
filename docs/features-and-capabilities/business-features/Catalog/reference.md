# Catalog Feature — Technical Reference Document

**Project:** E-Commerce Modular Monolith
**Bounded Context:** Catalog — Brands, Categories, Products
**Version:** 1.0 – Final

---

## 1. Overview

The Catalog bounded context manages the products that the business offers. It owns brands, categories, products, product variants, images, tags, pricing, stock, and product lifecycle status.

Catalog is a true Domain bounded context with its own aggregates, invariants, and state transitions. It is a foundational context consumed by Ordering, Shipping, and other contexts.

Catalog answers questions such as:

* What brands exist?
* What categories exist?
* What products are offered?
* What variants does a product have?
* What are the SKU, price, and stock quantity?
* Is the product published, draft, or discontinued?
* What images are associated with a product or category?

---

## 2. Architecture and Placement

Catalog is located at:

```text
Domain/BoundedContexts/Core/Catalog
```

Application use cases live under:

```text
Application/BoundedContexts/Catalog
```

Infrastructure persistence and services live under:

```text
Infrastructure/Persistence/Modules/Catalog
Infrastructure/Catalog
```

API controllers and DTOs live under:

```text
API/Controllers/Catalog
API/DTOs/Catalog
```

### 2.1 Layer Responsibilities

| Layer          | Responsibility                                                                                                |
| -------------- | ------------------------------------------------------------------------------------------------------------- |
| Domain         | Aggregates: Brand, Category, Product; value objects; enums; domain logic; repository interfaces               |
| Application    | Commands, queries, handlers, DTOs, validators, shared stock contract, file upload orchestration               |
| Infrastructure | EF Core configurations, repositories, file storage integration, stock service implementation, DI registration |
| API            | Thin controllers, request/response DTOs, role-based authorization                                             |

---

## 3. Domain Model

### 3.1 Aggregates

Catalog has three aggregate roots:

```text
Brand
Category
Product
```

#### Brand

Represents a product brand.

**Properties:**

* `Id`
* `Name`
* `DescriptionText`
* `Logo` (`BrandLogo` value object containing `FileId`)

**Behaviors:**

* `Create`
* `UpdateName`
* `UpdateDescription`
* `UpdateLogo`

#### Category

Represents a product category.

**Properties:**

* `Id`
* `Name`
* `Description`
* `ParentCategoryId`
* `Images` (`CategoryImage` entity collection)

**Behaviors:**

* `Create`
* `UpdateName`
* `UpdateDescription`
* `AssignParent`
* `ClearParent`
* `AddImage`
* `RemoveImage`

#### Product

Represents a sellable product.

**Properties:**

* `Id`
* `Description` (`ProductDescription` value object)
* `BrandId`
* `CategoryId`
* `Status` (`ProductStatus`)
* `Tags`
* `Images` (`ProductImage` entity collection)
* `Variants` (`ProductVariant` entity collection)

**Behaviors:**

* `Create`
* `Publish`
* `Draft`
* `Discontinue`
* `AddImage`
* `SetMainImage`
* `RemoveImage`
* `AddVariant`
* `UpdatePrice`
* `RemoveVariant`
* `IncreaseStock`
* `DecreaseStock`
* `AddTag`
* `RemoveTag`

### 3.2 Child Entities

* `CategoryImage`: owned by `Category`.
* `ProductImage`: owned by `Product`.
* `ProductVariant`: owned by `Product`.

Child entities are modified only through their aggregate root. They do not have repositories or independent lifecycles.

### 3.3 Value Objects

* `BrandLogo`: wraps a `Guid FileId`.
* `ProductDescription`: contains the name, short and long descriptions, dimensions, weight, dates, material, and color.

### 3.4 Enums

`ProductStatus`:

* `Draft`
* `Published`
* `Discontinued`

### 3.5 Repositories

```text
IBrandRepository
ICategoryRepository
IProductRepository
```

They extend `IRepository<T>` and add specific query methods as needed.

---

## 4. Domain Invariants

The Domain layer enforces the following invariants:

* Brand name, description, and logo must be valid.
* Category name and description must be valid.
* A category cannot be its own parent.
* Product description must be valid.
* Product must have a valid brand and category.
* Only draft products can be published.
* Only published products can be moved back to draft.
* Only draft products can be modified for certain operations.
* Product variant SKU must be unique within the product.
* Product variant price must be positive.
* Stock cannot be negative.
* Stock changes are allowed regardless of product status because stock represents inventory, not lifecycle.
* Product image `FileId` must be valid.
* A tag cannot be empty.
* Duplicate tags are not allowed.

---

## 5. Product Lifecycle

```text
Draft
  ↓ Publish
Published
  ↓ Draft
Draft

Draft/Published
  ↓ Discontinue
Discontinued
```

**Allowed transitions:**

* `Draft → Published`
* `Published → Draft`
* `Draft → Discontinued`
* `Published → Discontinued`

`Discontinued` is a terminal state.

---

## 6. Stock Management

Stock is currently owned by `ProductVariant` because inventory complexity is low: there is one inventory, with no warehouses, transfers, or reservations.

Ordering should not directly modify `ProductVariant.StockQuantity`. Instead, it uses a shared Application-layer contract:

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

The contract is a synchronous cross-context capability. Ordering calls it during order placement to reduce stock atomically within the same transaction.

If an Inventory context is introduced later, `IStockService` can be implemented by Inventory without changing Ordering, provided the contract's semantics remain compatible.

---

## 7. File Storage Integration

Catalog stores file references, not physical files.

* Brand logo → `BrandLogo(FileId)`
* Category image → `CategoryImage(FileId)`
* Product image → `ProductImage(FileId)`

The Application layer orchestrates file uploads through the shared `IFileService`.

Before upload, the context-specific validator checks the file type using `IFileContentInspector`.

### Brand Logo Validation

`BrandLogoFileValidator` expects `FileType.Image` and a valid status.

### Category Image Validation

`CategoryImageFileValidator` expects `FileType.Image` and a valid status.

### Product Image Validation

`ProductImageFileValidator` expects `FileType.Image` and a valid status.

### Upload Flow

```text
API receives IFormFile
       ↓
Maps to FileUpload
       ↓
Handler calls validator
       ↓
Validator uses IFileContentInspector
       ↓
IFileContentInspector detects actual content type
       ↓
Validator returns success/failure
       ↓
If valid, handler calls IFileService.UploadAsync
       ↓
Returns FileId
       ↓
Handler sets FileId on aggregate
```

This keeps File Storage generic and reusable while Catalog owns its file-type policies.

---

## 8. Application Layer

### 8.1 Commands

| Aggregate | Command                            | Purpose                            |
| --------- | ---------------------------------- | ---------------------------------- |
| Brand     | `CreateBrandCommand`               | Create a new brand                 |
| Brand     | `UpdateBrandCommand`               | Update brand details and logo      |
| Category  | `CreateCategoryCommand`            | Create a new category              |
| Category  | `UpdateCategoryCommand`            | Update category details and parent |
| Category  | `AddCategoryImageCommand`          | Add an image to a category         |
| Category  | `RemoveCategoryImageCommand`       | Remove an image from a category    |
| Product   | `CreateProductCommand`             | Create a new product               |
| Product   | `PublishProductCommand`            | Publish a product                  |
| Product   | `DraftProductCommand`              | Move a product to draft            |
| Product   | `DiscontinueProductCommand`        | Discontinue a product              |
| Product   | `AddProductImageCommand`           | Add a product image                |
| Product   | `SetMainProductImageCommand`       | Set the main product image         |
| Product   | `RemoveProductImageCommand`        | Remove a product image             |
| Product   | `AddProductVariantCommand`         | Add a product variant              |
| Product   | `UpdateProductVariantPriceCommand` | Update a variant's price           |
| Product   | `RemoveProductVariantCommand`      | Remove a variant                   |
| Product   | `AddProductTagCommand`             | Add a product tag                  |
| Product   | `RemoveProductTagCommand`          | Remove a product tag               |
| Product   | `IncreaseProductStockCommand`      | Increase variant stock             |
| Product   | `DecreaseProductStockCommand`      | Decrease variant stock             |

All commands have `[AuthorizePermission]` attributes for fine-grained Application-layer authorization.

### 8.2 Queries

| Aggregate | Query                  | Purpose                            |
| --------- | ---------------------- | ---------------------------------- |
| Brand     | `GetBrandByIdQuery`    | Get a brand by ID                  |
| Brand     | `ListBrandsQuery`      | Retrieve a paginated brand list    |
| Category  | `GetCategoryByIdQuery` | Get a category by ID               |
| Category  | `ListCategoriesQuery`  | Retrieve a paginated category list |
| Product   | `GetProductByIdQuery`  | Get a product by ID                |
| Product   | `ListProductsQuery`    | Retrieve a paginated product list  |
| Product   | `SearchProductsQuery`  | Search published products          |

### 8.3 DTOs

* `BrandDto`
* `CategoryDto`
* `ProductDto`
* `ProductListDto`
* `ProductDescriptionDto`
* `ProductImageDto`
* `ProductVariantDto`

### 8.4 Validators

* `BrandLogoFileValidator`
* `CategoryImageFileValidator`
* `ProductImageFileValidator`

These validators ensure that uploaded files are images before storage.

---

## 9. Infrastructure Layer

### 9.1 Persistence

EF Core configurations:

* `BrandConfiguration`
* `CategoryConfiguration`
* `CategoryImageConfiguration`
* `ProductConfiguration`
* `ProductVariantConfiguration`
* `ProductImageConfiguration`

All configurations inherit from `BaseEntityConfiguration<TEntity>` to apply common audit and soft-delete conventions.

**Tables:**

```text
catalog.Brands
catalog.Categories
catalog.CategoryImages
catalog.Products
catalog.ProductVariants
catalog.ProductImages
```

**Repositories:**

* `BrandRepository`
* `CategoryRepository`
* `ProductRepository`

They inherit from the generic `Repository<T>`.

### 9.2 Stock Service

`StockService` implements `IStockService` and uses `IProductRepository` and `IUnitOfWork`.

### 9.3 File Storage

Catalog does not implement its own storage. It uses shared File Storage services:

* `IFileService`
* `IFileContentInspector`

These services are registered by the File Storage infrastructure.

### 9.4 DI Registration

`CatalogInfrastructureExtensions` registers repositories and `IStockService`.

The validators are registered in the Application layer or through Catalog-specific registration:

```csharp
services.AddScoped<BrandLogoFileValidator>();
services.AddScoped<CategoryImageFileValidator>();
services.AddScoped<ProductImageFileValidator>();
```

---

## 10. Authorization

Catalog permissions:

```text
Catalog.Brands.Read
Catalog.Brands.Manage

Catalog.Categories.Read
Catalog.Categories.Manage

Catalog.Products.Read
Catalog.Products.Manage
```

### 10.1 API Layer

Controllers use coarse-grained role-based authorization:

```csharp
[Authorize(Roles = "CatalogManager,Administrator")]
```

### 10.2 Application Layer

Commands and queries use fine-grained permission attributes:

```csharp
[AuthorizePermission(CatalogPermissions.ManageBrands)]
```

This provides defense in depth while separating role-based access from fine-grained permission enforcement.

---

## 11. Cross-Context Integration

Catalog exposes stock capability through `IStockService` to Ordering.

Catalog may later publish integration events for:

* Product published
* Product discontinued
* Low stock
* Brand or category changes

However, domain events are not currently required unless an internal consumer exists.

---

## 12. Folder Structure

### Domain

```text
Domain/
└── BoundedContexts/
    └── Core/
        └── Catalog/
            ├── AggregateRoots/
            │   ├── Brand/
            │   │   ├── Behaviors/
            │   │   │   ├── Brand.cs
            │   │   │   ├── Brand.State.cs
            │   │   │   └── Brand.Validation.cs
            │   │   ├── Exceptions/
            │   │   │   └── BrandException.cs
            │   │   └── ValueObjects/
            │   │       └── BrandLogo.cs
            │   ├── Category/
            │   │   ├── Behaviors/
            │   │   │   ├── Category.cs
            │   │   │   ├── Category.State.cs
            │   │   │   └── Category.Validation.cs
            │   │   ├── Entities/
            │   │   │   └── CategoryImage.cs
            │   │   └── Exceptions/
            │   │       └── CategoryException.cs
            │   └── Product/
            │       ├── Behaviors/
            │       │   ├── Product.cs
            │       │   ├── Product.State.cs
            │       │   └── Product.Validation.cs
            │       ├── Entities/
            │       │   ├── ProductImage.cs
            │       │   └── ProductVariant.cs
            │       ├── Enums/
            │       │   └── ProductStatus.cs
            │       ├── Exceptions/
            │       │   └── ProductException.cs
            │       └── ValueObjects/
            │           └── ProductDescription.cs
            └── Repositories/
                ├── IBrandRepository.cs
                ├── ICategoryRepository.cs
                └── IProductRepository.cs
```

### Application

```text
Application/
├── BoundedContexts/
│   └── Catalog/
│       ├── Brands/
│       │   ├── Commands/
│       │   ├── DTOs/
│       │   ├── Queries/
│       │   └── Validation/
│       ├── Categories/
│       │   ├── Commands/
│       │   ├── DTOs/
│       │   ├── Queries/
│       │   └── Validation/
│       └── Products/
│           ├── Commands/
│           ├── DTOs/
│           ├── Queries/
│           └── Validation/
└── Shared/
    └── Stock/
        └── IStockService.cs
```

### Infrastructure

```text
Infrastructure/
├── Stock/
│   ├── Extensions/
│   │   └── StockInfrastructureExtensions.cs
│   └── Services/
│       └── StockService.cs
└── Persistence/
    ├── Context/
    │   └── AppDbContext.Catalog.cs
    └── Modules/
        └── Catalog/
            ├── Brand/
            │   ├── Configurations/
            │   └── Repositories/
            ├── Category/
            │   ├── Configurations/
            │   └── Repositories/
            └── Product/
                ├── Configurations/
                └── Repositories/
```

### API

```text
API/
├── Controllers/
│   └── Catalog/
│       ├── BrandsController.cs
│       ├── CategoriesController.cs
│       └── ProductsController.cs
└── DTOs/
    └── Catalog/
        ├── Brands/
        ├── Categories/
        └── Products/
```

---

## 13. Workflows

### 13.1 Brand Creation

```text
API receives form-data including logo
       ↓
Maps IFormFile to FileUpload
       ↓
CreateBrandCommand
       ↓
BrandLogoFileValidator.ValidateAsync
       ↓
IFileContentInspector.InspectAsync
       ↓
If valid, IFileService.UploadAsync
       ↓
Brand.Create(...)
       ↓
BrandRepository.AddAsync
       ↓
UnitOfWork.SaveChangesAsync
```

### 13.2 Product Publication

```text
PublishProductCommand
       ↓
ProductRepository.GetByIdAsync
       ↓
Product.Publish()
       ↓
ProductRepository.UpdateAsync
       ↓
UnitOfWork.SaveChangesAsync
```

### 13.3 Stock Decrease from Ordering

```text
Ordering uses IStockService
       ↓
StockService.DecreaseStockAsync
       ↓
ProductRepository.GetByIdAsync
       ↓
Product.DecreaseStock(variantId, quantity)
       ↓
ProductRepository.UpdateAsync
       ↓
UnitOfWork.SaveChangesAsync
```

---

## 14. Testing

### 14.1 Domain Tests

* Brand creation and update invariants
* Category hierarchy rules
* Product lifecycle transitions
* Variant SKU uniqueness
* Price and stock invariants
* Tag handling

### 14.2 Application Tests

* File validation logic
* Command handlers with mocked repositories
* Stock service behavior
* Pagination and search logic

### 14.3 Integration Tests

* EF Core mappings
* Repository query behavior
* Soft-delete and audit fields
* Unique constraints

### 14.4 API Tests

* Role authorization
* Request/response mapping
* Error handling

---

## 15. Summary

Catalog is a cohesive, simplified bounded context that owns product, brand, category, variant, image, and stock information. It integrates with File Storage through shared technical services and exposes stock capability to Ordering via a shared Application-layer contract. The Domain remains free of infrastructure concerns, and authorization is enforced at both API and Application layers.

This design is ready for Ordering integration while remaining maintainable and evolution-friendly.
