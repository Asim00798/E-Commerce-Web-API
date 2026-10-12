# File Storage Integration Guide

**Project:** E-Commerce Modular Monolith
**Purpose:** Using the Shared File Storage Capability from Any Application Context
**Version:** 2.2 — Final

---

## 1. Purpose

The File Storage capability is a shared application/infrastructure capability, not a Domain bounded context. It allows any bounded context, module, or application use case to store and retrieve files without knowing the physical storage provider.

A consuming application context deals only with a stable `FileId`.

For example, a `Brand` aggregate references a `BrandLogo` value object that wraps a `FileId`:

```csharp
public sealed partial class Brand : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = null!;

    public string? DescriptionText { get; private set; }

    public BrandLogo Logo { get; private set; } = null!;

    // ...
}
```

The Brand domain does not need to know:

* Where the file is stored.
* Which storage provider is being used.
* What the physical storage key is.
* Whether the file is on disk or in cloud storage.
* How the file is uploaded or deleted.

This keeps file-storage infrastructure outside the Domain model.

**Version note:** This revision corrects §18 and §22 to match the actual `FilesController` implementation: the route is `/api/files` (folder-versioned, not URL-versioned), and authorization is a single class-level role gate. No permission policies are applied at the controller.

---

## 2. Core Design

The system follows this relationship:

```text
Application Context
        |
        | FileId
        v
   IFileService
        |
        v
   StoredFile metadata
        |
        | StorageKey
        v
IFileStorageProvider
        |
        v
  Physical Storage
```

For example:

```text
Brand
  └── Logo (BrandLogo)
          |
          | FileId
          v
      FileService
          |
          v
      StoredFile
      ├── FileId
      ├── StorageKey
      ├── FileName
      ├── ContentType
      └── Size
          |
          v
  LocalFileStorageProvider
          |
          v
      Local disk
```

The consuming context stores only the `FileId`, typically wrapped in a value object.

---

## 3. Why the Application Context Stores FileId

A business entity should reference a file through a stable identifier rather than storage-specific information.

### 3.1 Simple Identifier

The simplest representation is a nullable `Guid`:

```csharp
public Guid? InvoiceFileId { get; private set; }
```

### 3.2 Value-Object Wrapper

A richer representation wraps the `FileId` in a value object that carries domain invariants.

For example, `BrandLogo`:

```csharp
public sealed partial class Brand : BaseEntity, IAggregateRoot
{
    public BrandLogo Logo { get; private set; } = null!;

    public void UpdateLogo(BrandLogo newLogo)
    {
        if (newLogo is null)
        {
            throw new BrandException(
                "Logo cannot be null.",
                nameof(newLogo));
        }

        Logo = newLogo;
    }
}
```

The `BrandLogo` value object:

```csharp
public sealed record BrandLogo
{
    public Guid FileId { get; }

    public BrandLogo(Guid fileId)
    {
        if (fileId == Guid.Empty)
        {
            throw new BrandException("File id is required.");
        }

        FileId = fileId;
    }
}
```

Both shapes are valid. The choice depends on whether the domain benefits from the wrapper's invariants.

Either way, the entity does **not** contain storage-specific properties such as:

```csharp
string FilePath;
string StorageKey;
string BucketName;
string BlobUrl;
```

Those are storage concerns.

This provides provider independence. Changing from local disk to Amazon S3 does not require changing the Brand entity or its business logic.

---

## 4. Application Layer Contracts

The consuming application interacts with:

* `IFileService` — for file-storage operations.
* `IFileContentInspector` — optionally, for file-type inspection before upload.

These are shared Application contracts. They are stable and provider-agnostic.

### 4.1 IFileService

```csharp
public interface IFileService
{
    Task<Guid> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default);

    Task<FileDto?> GetAsync(
        Guid fileId,
        CancellationToken ct = default);

    Task<FileDownloadResult?> DownloadAsync(
        Guid fileId,
        CancellationToken ct = default);

    Task DeleteAsync(
        Guid fileId,
        CancellationToken ct = default);
}
```

* `GetAsync` returns file metadata.
* `DownloadAsync` returns both metadata and the content stream in one call to avoid duplicate database queries.
* `DeleteAsync` marks the file for deletion. Physical deletion and metadata removal happen through a background job.

### 4.2 IFileContentInspector

A generic technical service used by consuming contexts when they need to inspect file content before upload.

```csharp
public interface IFileContentInspector
{
    Task<FileInspectionResult> InspectAsync(
        Stream content,
        FileType expectedType,
        CancellationToken ct = default);
}
```

The inspector examines actual content bytes and returns technical facts:

* `DetectedType` — what the file appears to be based on its content signature.
* `ValidationStatus` — whether the content is technically valid for the expected type.

It does not enforce business policies such as "Brand logos must be PNG." Those rules belong to the consuming context.

**Stream behavior:** The inspector reads the leading bytes to determine the signature, then seeks back to the original position. A caller can safely inspect and then upload the same stream because the inspector preserves the stream position.

### 4.3 Shared File Models and DTOs

These types live under `Application/Shared/Files/`, split by intent.

| Type                   | Folder    | Reason                                             |
| ---------------------- | --------- | -------------------------------------------------- |
| `FileDto`              | `Dtos/`   | Crosses the HTTP boundary as serializable metadata |
| `FileType`             | `Models/` | Value enum, used in-process                        |
| `FileValidationStatus` | `Models/` | Value enum, used in-process                        |
| `FileInspectionResult` | `Models/` | In-process inspection result                       |
| `FileUpload`           | `Models/` | Holds a `Stream`; not a serialization DTO          |
| `FileDownloadResult`   | `Models/` | Holds a `Stream`; not a serialization DTO          |

#### FileType

```csharp
public enum FileType
{
    Unknown = 0,
    Image = 1,
    Document = 2,
    Video = 3,
    Audio = 4,
    Archive = 5
}
```

#### FileValidationStatus

```csharp
public enum FileValidationStatus
{
    Valid = 1,
    Invalid = 2,
    Unknown = 3
}
```

#### FileInspectionResult

```csharp
public sealed record FileInspectionResult(
    FileType DetectedType,
    FileValidationStatus ValidationStatus);
```

#### FileUpload

```csharp
public sealed record FileUpload(
    Stream Content,
    string FileName,
    string ContentType);
```

#### FileDownloadResult

```csharp
public sealed record FileDownloadResult(
    FileDto Metadata,
    Stream Content);
```

---

## 5. Storage Provider Contract

`IFileStorageProvider` and `StorageObject` are declared in `Application/Shared/Files/Services/`, within the shared Application layer.

Their intended usage is Infrastructure-only: business contexts must never inject them.

The placement is deliberate. The contract is accessible to Infrastructure without introducing a reverse dependency, while the rule that business contexts must not reference it is enforced by convention.

### 5.1 IFileStorageProvider

```csharp
public interface IFileStorageProvider
{
    Task StoreAsync(
        Stream content,
        string storageKey,
        CancellationToken ct = default);

    Task<Stream> OpenReadAsync(
        string storageKey,
        CancellationToken ct = default);

    /// <summary>
    /// Must be idempotent. No exception is thrown when
    /// the object does not exist.
    /// </summary>
    Task DeleteAsync(
        string storageKey,
        CancellationToken ct = default);

    Task<bool> ExistsAsync(
        string storageKey,
        CancellationToken ct = default);

    /// <summary>
    /// Lists all storage objects with their creation timestamps.
    /// Used by orphan cleanup to apply a grace period.
    /// </summary>
    Task<IReadOnlyCollection<StorageObject>> ListObjectsAsync(
        CancellationToken ct = default);
}
```

### 5.2 StorageObject

```csharp
public sealed record StorageObject(
    string Key,
    DateTime? CreatedAtUtc);
```

### 5.3 Key Properties

* `StoreAsync` parameter order is `(content, storageKey)`.
* `DeleteAsync` is idempotent: a missing object is treated as success, while a genuine storage failure throws an exception.
* `ListObjectsAsync` supports orphan cleanup by returning storage objects with creation timestamps.

**Important architectural discipline:** Business contexts must never depend on `IFileStorageProvider` or `StorageObject`. The interface exists in the shared layer so Infrastructure can implement and register it. The boundary is enforced by convention, not by project reference.

The cleanup coordinator, `IFileStorageCleanupService`, is separately declared in `Infrastructure/Files/Abstractions/` and is never exposed to Application or business contexts.

---

## 6. StoredFile

`StoredFile` is an Infrastructure persistence entity. It represents the application's knowledge about a stored file.

### 6.1 Status

```csharp
public enum StoredFileStatus
{
    Available = 1,
    PendingDeletion = 2,
    ProcessingDeletion = 3
}
```

### 6.2 Entity

```csharp
public sealed class StoredFile
{
    public Guid Id { get; set; } // FileId

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long Size { get; set; }

    public string StorageKey { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public StoredFileStatus Status { get; set; } =
        StoredFileStatus.Available;

    public DateTime? DeletionRequestedAtUtc { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Guid? DeletionClaimId { get; set; }

    public DateTime? DeletionClaimedAtUtc { get; set; }
}
```

### 6.3 Critical Distinction

* **FileId** — application-facing identity.
* **StorageKey** — infrastructure-facing physical identity.
* **Status, DeletionClaimId, and DeletionClaimedAtUtc** — support concurrency-safe asynchronous deletion.

The storage key must never become part of the business domain.

---

## 7. Example: Brand with a Logo

Suppose the Catalog context contains a Brand aggregate with a `BrandLogo` value object:

```csharp
public sealed partial class Brand : BaseEntity, IAggregateRoot
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string? DescriptionText { get; private set; }

    public BrandLogo Logo { get; private set; } = null!;
}
```

The workflow is:

```text
Client
   |
   | Upload image
   v
FilesController
   |
   v
IFileService.UploadAsync(...)
   |
   +-- Generate FileId
   |
   +-- Generate StorageKey
   |
   +-- Store physical file
   |
   +-- Persist StoredFile metadata
   |
   v
FileId
   |
   v
CreateBrandCommand
   |
   v
var logo = new BrandLogo(fileId);
Brand.Create(name, description, logo);
   |
   v
Brand persisted
```

If Brand creation should validate the file type before upload, the use case calls `IFileContentInspector` first. See §20.

---

## 8. Recommended Upload Workflow

Storage operations and database operations cannot normally participate in the same ACID transaction.

The system uses eventual consistency and reconciliation rather than attempting a distributed transaction.

The recommended flow when a context needs to validate file type is:

1. The context receives a `FileUpload` from the API layer.
2. The context calls `IFileContentInspector.InspectAsync` with the expected `FileType`.
3. If inspection is not valid, the use case returns a failure.
4. The context calls `IFileService.UploadAsync` to store the file.
5. The context receives the `FileId`.
6. The context assigns the `FileId` to the entity and persists it.
7. If entity persistence fails after upload, orphan cleanup eventually removes the file.

This separates file storage from the consuming context's business transaction while providing a recovery mechanism for orphaned files.

---

## 9. File Upload Consistency Strategy

The recommended upload sequence is:

1. Generate `FileId`.
2. Generate `StorageKey`.
3. Upload the physical file. The content stream is wrapped in `SizeLimitedReadStream` to enforce size limits without relying on `Stream.Length`.
4. Persist `StoredFile` metadata.
5. If database persistence fails, attempt immediate compensation by deleting the physical file.
6. If compensation also fails, the orphan cleanup job eventually removes the file.

The important principle is that the physical file is created before its metadata is committed.

If database persistence fails, the physical file temporarily becomes an orphan.

---

## 10. Orphan File Cleanup

A cleanup job periodically identifies and removes physical objects that do not have corresponding metadata.

The job uses `IFileStorageProvider.ListObjectsAsync()` to list physical objects with creation timestamps.

It deletes objects that meet all of the following conditions:

* They have no corresponding `StoredFile` record.
* They have a known creation timestamp.
* They are older than the configured `OrphanGracePeriod`.

Objects with unknown creation times are skipped conservatively to avoid racing with in-progress uploads.

---

## 11. Why the Cleanup Job Is Necessary

Consider this sequence:

```text
Upload file
     |
     v
Physical storage succeeds
     |
     v
SQL transaction begins
     |
     v
Business update fails
     |
     v
Transaction rolls back
```

The file still exists.

Without cleanup:

```text
Database = no reference
Storage  = file exists
```

The cleanup job converts this temporary inconsistency into eventual consistency by eventually removing the orphaned physical object.

---

## 12. Idempotency

The application already uses an idempotency mechanism. Therefore, retryable operations can safely be retried without creating duplicate logical operations, provided the relevant operation is correctly covered by that mechanism.

However, idempotency and orphan cleanup solve different problems:

* **Idempotency** — prevents duplicate logical processing.
* **Cleanup and reconciliation** — remove physical artifacts that have no valid database state.

Both are useful.

---

## 13. Deleting a File

Deletion is asynchronous and stateful.

```text
Available
   |
   | MarkPendingDeletionAsync()
   v
PendingDeletion
   |
   | ClaimPendingDeletionBatchAsync()
   v
ProcessingDeletion (with ClaimId)
   |
   +-- Physical delete fails
   |       |
   |       v
   |   RevertToPendingDeletionAsync(ClaimId)
   |
   +-- Physical delete succeeds
           |
           v
      RemoveIfClaimedAsync(ClaimId)
```

The background cleanup job claims records atomically using `DeletionClaimId`.

Only the current claim owner can finalize or revert deletion. Expired claims are recovered.

This protects against concurrent workers and stale workers attempting to finalize deletion after losing their claims.

---

## 14. File Replacement

Suppose a Brand currently has `Logo.FileId = A`, and the user uploads a new image.

The safer conceptual workflow is:

```text
Upload new file B
        |
        v
Persist B
        |
        v
Update Brand.Logo = new BrandLogo(B)
        |
        v
Commit transaction
        |
        v
Old file A becomes unreferenced
        |
        v
Cleanup eventually removes A
```

This is safer than:

```text
Delete A
   |
   v
Upload B
   |
   v
Update Brand
```

Failure after deleting A could leave the Brand without an image.

**Upload the replacement first, switch the reference second, and clean up the old file afterward.**

---

## 15. Reading a File

When a consumer needs a file:

```text
Brand.Logo.FileId
       |
       v
IFileService.DownloadAsync(...)
       |
       v
StoredFile
       |
       v
StorageKey
       |
       v
IFileStorageProvider (Infrastructure)
       |
       v
Physical file
```

The consumer never constructs a physical path.

Correct usage:

```csharp
var result = await _fileService.DownloadAsync(
    brand.Logo.FileId,
    cancellationToken);

if (result is not null)
{
    // Use result.Metadata and result.Content.
}
```

The consumer works with the application-facing file service and never needs the storage key.

---

## 16. Storage Provider Switching

The provider is selected through configuration and dependency injection.

**Development:**

```json
{
  "FileStorage": {
    "Provider": "Local"
  }
}
```

**Production:**

```json
{
  "FileStorage": {
    "Provider": "S3"
  }
}
```

Or:

```json
{
  "FileStorage": {
    "Provider": "AzureBlob"
  }
}
```

The application code remains unchanged, provided the selected provider is implemented, registered, configured, and can access the existing stored objects.

Changing the provider configuration alone does not migrate existing physical files. A real provider migration must transfer the stored objects and preserve or update the metadata mapping.

---

## 17. Configuration

Root configuration:

```json
{
  "FileStorage": {
    "Provider": "Local",
    "MaxFileSizeBytes": 10485760,
    "AllowedContentTypes": [
      "image/jpeg",
      "image/png",
      "image/webp"
    ],
    "AllowedExtensions": [
      ".jpg",
      ".jpeg",
      ".png",
      ".webp"
    ],
    "OrphanGracePeriod": "00:30:00",
    "DeletionClaimLease": "00:10:00",
    "Local": {
      "RootPath": "storage/files"
    }
  }
}
```

Unknown provider names cause startup failure. Provider-specific configuration remains separate and is owned by Infrastructure.

`FileStorageOptionsValidator` runs at startup and rejects:

* A missing provider.
* A non-positive maximum file size.
* A non-positive orphan grace period.
* A non-positive deletion claim lease.
* A missing local root path when the selected provider is `Local`.

**Configuration and secrets:** Provider credentials and other secrets must not be hardcoded in source code. Use environment variables or an appropriate secret-management mechanism.

---

## 18. API Boundary

The API exposes file operations through `FilesController`.

| Method   | Endpoint                   | Authorization                           |
| -------- | -------------------------- | --------------------------------------- |
| `POST`   | `/api/files`               | Role: Administrator, Customer, Employee |
| `GET`    | `/api/files/{id}`          | Role: Administrator, Customer, Employee |
| `GET`    | `/api/files/{id}/download` | Role: Administrator, Customer, Employee |
| `DELETE` | `/api/files/{id}`          | Role: Administrator, Customer, Employee |

Endpoint behavior:

* `GET /api/files/{id}` returns metadata.
* `GET /api/files/{id}/download` returns the actual file content.
* `DELETE /api/files/{id}` marks the file for asynchronous deletion and returns `202 Accepted`.

### Authorization Convention

File Storage uses a class-level role gate only.

Its operations are direct `IFileService` calls rather than MediatR commands, so there is no request type to decorate with `[AuthorizePermission]`. No permission policy is applied at the controller either.

The route is `/api/files`. The API uses folder-based versioning (`Controllers/v1/Files/`) without a `v1` segment in the URL, consistent with the rest of the codebase.

The API layer owns HTTP concerns such as:

* `IFormFile`.
* HTTP status codes.
* Request validation.
* Response contracts.
* Content disposition.
* Content type.

The Application layer does not depend on `IFormFile`.

---

## 19. Application DTO vs. API DTO

The Application layer exposes `FileDto`, while the API exposes `FileResponse`.

This keeps the external HTTP contract independent from internal application contracts.

Mapping:

```text
FileDto
   |
   v
FileResponse
```

Neither `StoredFile` nor `IFileStorageProvider` is exposed to clients.

---

## 20. Using File Storage from a Bounded Context

A typical use case may need to validate the file type before uploading.

### Example: Brand Logo

The Brand Application defines a validator:

```csharp
public sealed class BrandLogoFileValidator
{
    private readonly IFileContentInspector _fileContentInspector;

    public BrandLogoFileValidator(
        IFileContentInspector fileContentInspector)
    {
        _fileContentInspector = fileContentInspector;
    }

    public async Task<Result> ValidateAsync(
        FileUpload file,
        CancellationToken ct = default)
    {
        var inspection = await _fileContentInspector.InspectAsync(
            file.Content,
            FileType.Image,
            ct);

        if (inspection.ValidationStatus != FileValidationStatus.Valid)
        {
            return Result.Failure(
                "Brand logo must be a valid image.");
        }

        return Result.Success();
    }
}
```

The handler then performs validation, upload, and persistence:

```csharp
var validation = await _logoValidator.ValidateAsync(
    command.Logo,
    ct);

if (!validation.Succeeded)
{
    return Result<Guid>.Failure(validation.Errors);
}

var fileId = await _fileService.UploadAsync(
    command.Logo.Content,
    command.Logo.FileName,
    command.Logo.ContentType,
    ct);

var logo = new BrandLogo(fileId);

var brand = Brand.Create(
    command.Name,
    command.DescriptionText,
    logo);

await _brandRepository.AddAsync(brand, ct);

await _unitOfWork.SaveChangesAsync(ct);
```

The Product context can define similar validators for product images, manuals, and other file types.

**Important:** This example assumes the relevant `Result` APIs and application contracts match the codebase. It illustrates the integration pattern rather than prescribing a particular Result Pattern implementation.

---

## 21. Multiple Files

The same model naturally supports multiple files.

For example, a Product aggregate can hold multiple `ProductImage` entities, each wrapping a `FileId`:

```csharp
// Within the Product aggregate

public IReadOnlyCollection<ProductImage> Images =>
    _images.AsReadOnly();

public void AddImage(ProductImage image)
{
    // ...
}

public void SetMainImage(Guid imageId)
{
    // ...
}
```

Alternatively, a simpler shape uses nullable `Guid` properties when the domain does not need per-file invariants:

```csharp
public Guid? MainImageFileId { get; private set; }

public Guid? ManualFileId { get; private set; }

public Guid? SpecificationFileId { get; private set; }
```

Either shape works. The storage system does not need to know what each file represents.

---

## 22. File Ownership and Authorization

Possessing a `FileId` should not automatically mean that a user is authorized to access the file.

Authorization remains the responsibility of the application use case.

For example:

```text
GET Product
     |
     v
Does user have Product.Read?
     |
     v
Does user have access to this Product?
     |
     v
Obtain ImageFileId
     |
     v
Retrieve file
```

### Current Implementation

**The current File Storage API uses a single role gate at the controller.**

No file ownership check exists. Any authenticated user in the allowed roles (`Administrator`, `Customer`, or `Employee`) can access any file by `FileId`.

This is capability-based access: possession of the UUID is effectively sufficient to retrieve the file through the current API, subject to the controller's role gate.

Business contexts that expose `FileId` to end users are responsible for ensuring that the surrounding data access is itself authorized.

If a context exposes a `FileId` in a response that any authenticated user can fetch, any user who passes the File Storage role gate may be able to download that file.

The `FilePermissions` constants (`Files.Read`, `Files.Upload`, `Files.Delete`, and `Files.Manage`) are declared in the codebase but are not currently enforced by any layer.

If file-management commands or queries are introduced later, permission enforcement can be added at that time, either at the controller through policies or in the MediatR pipeline through `[AuthorizePermission]`.

For sensitive files, access should be controlled through the owning application's authorization rules or explicit per-file access tokens.

---

## 23. File Lifecycle

A useful conceptual lifecycle is:

```text
                 +--------------+
                 |   Uploaded   |
                 +------+-------+
                        |
                        v
                 +--------------+
                 |  Referenced  |
                 +------+-------+
                        |
              +---------+---------+
              |                   |
           Replaced             Deleted
              |                   |
              +---------+---------+
                        |
                        v
                 +--------------+
                 | Unreferenced  |
                 +------+-------+
                        |
                        v
                 +--------------+
                 |   Cleanup    |
                 +--------------+
```

The database is the authoritative source for application state. Physical storage is treated as an external resource that is reconciled with that state.

---

## 24. Important Consistency Principle

The system deliberately does not attempt to make SQL Server and the storage provider participate in a single distributed ACID transaction.

Instead, SQL Server and external storage are coordinated using:

* Database transactions.
* Idempotency.
* Retryable processing where appropriate.
* Background processing for asynchronous deletion.
* Orphan-file cleanup.
* Reconciliation.
* Safe replacement and deletion ordering.

This is a practical approach to coordinating relational databases and object storage.

---

## 25. What the Consuming Context Needs

A normal bounded context only needs a file reference.

**Simple representation:**

```csharp
public Guid? ImageFileId { get; private set; }
```

**Or a value-object wrapper for domain invariants:**

```csharp
public BrandLogo Logo { get; private set; }
```

Application dependencies:

```csharp
IFileService
```

Optionally:

```csharp
IFileContentInspector
```

The use-case workflow is:

```text
Optionally validate file type
          |
          v
        Upload
          |
          v
    Receive FileId
          |
          v
Assign FileId to entity
(raw or wrapped in a value object)
          |
          v
    Persist entity
```

Nothing else is required for basic integration.

---

## 26. What the Consuming Context Must Not Know

A consuming context should not know about:

* `IFileStorageProvider`.
* `StorageObject`.
* `StorageKey`.
* `IFileStorageCleanupService`.
* `LocalFileStorageProvider`.
* `S3FileStorageProvider`.
* `AzureBlobStorageProvider`.
* Physical paths.
* Buckets.
* Containers.
* Blob names.
* Provider SDKs.

The first three are declared in the shared Application layer for dependency-injection friendliness, but their consumption is intended to be Infrastructure-only. The remaining items are Infrastructure implementation details.

---

## 27. Architectural Result

The final dependency direction is:

```text
+-------------------------------------------+
| Application                               |
|                                           |
| IFileService                              |
| IFileContentInspector                     |
| IFileStorageProvider (contract)           |
| StorageObject                             |
| FileDto, FileUpload, FileDownloadResult   |
| ...                                       |
+---------------------+---------------------+
                      |
                      v
+-------------------------------------------+
| Infrastructure                            |
|                                           |
| FileService                               |
| FileContentInspector                      |
| FileStorageCleanupService                 |
| StoredFile                                |
| StoredFileRepository                      |
| LocalFileStorageProvider                  |
| (implements IFileStorageProvider)         |
+---------------------+---------------------+
                      |
             +--------+--------+
             |                 |
             v                 v
        SQL Server      Physical Storage
                        Local / S3 / Azure
```

The Domain remains independent.

Business contexts see only `IFileService` and, optionally, `IFileContentInspector`.

`IFileStorageProvider` is declared in the shared Application layer for dependency-injection friendliness but is consumed only by Infrastructure. The boundary is enforced by convention, not by project reference.

---

## 28. Final Usage Pattern

For any application context:

1. The user uploads a file.
2. The use case optionally validates the file type through `IFileContentInspector`.
3. `FileService` stores the file.
4. `FileService` returns a `FileId`.
5. The context stores the `FileId`, either directly or wrapped in a value object.
6. The business database transaction commits.
7. The file becomes associated with the business state.

### File Replacement

```text
Upload new file
        |
        v
Assign new FileId
        |
        v
Commit business transaction
        |
        v
Old file becomes unreferenced
        |
        v
Cleanup removes old file
```

### Failed Business Transaction

```text
Upload succeeds
        |
        v
Business transaction fails
        |
        v
File temporarily orphaned
        |
        v
Cleanup/reconciliation detects it
        |
        v
File removed
```

### Provider Migration

```text
Local Storage
      |
      v
Migrate physical objects and metadata
as required, then switch configuration
      |
      v
Cloud / Object Storage
```

The consuming application contexts remain unchanged.

---

## 29. Final Principle

The File Storage capability should be treated as a shared infrastructure capability with an application-facing abstraction.

The most important design decision is:

> Business contexts own the reference (`FileId`); File Storage Infrastructure owns the physical file.

This provides:

* Provider independence.
* Clean separation of concerns.
* No file-storage concepts in the Domain.
* Simple integration with any bounded context or module.
* Safe file replacement.
* Retry and idempotency support.
* Eventual consistency between SQL Server and physical storage.
* Orphan-file cleanup and reconciliation.
* Concurrency-safe asynchronous deletion.
* Generic technical content inspection with context-specific file-type policies.
* A straightforward path from local development storage to cloud object storage.

The result is intentionally solid without turning File Storage into an unnecessary Domain bounded context or distributed-transaction subsystem.

---

**Document version:** 2.2 — Final
**Feature:** File Storage / Integration Guide

---

## Summary of Changes from v2.1

| Section        | Change                                                                                                                                                                                                                                                            |
| -------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Version header | Bumped to 2.2; version note rewritten to reflect the actual controller: route is `/api/files`, with a role gate only.                                                                                                                                             |
| §18            | Corrected the endpoint table: `/api/files` rather than `/api/v1/files`; authorization now shows the role gate instead of `Permission:Files.*` policies.                                                                                                           |
| §18            | Rewrote the authorization note to clarify that the controller applies a class-level role gate and no permission policies are applied.                                                                                                                             |
| §22            | Replaced the claim that authorization is enforced through `Permission:Files.*` policies with the capability-based access model. Documented the role gate, absence of ownership checks, and the business-context responsibility for controlling `FileId` exposure. |
