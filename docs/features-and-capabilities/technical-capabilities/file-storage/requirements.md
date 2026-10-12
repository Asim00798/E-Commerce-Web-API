# File Storage Capability — Technical Requirements Document

**Project:** E-Commerce Modular Monolith
**Feature:** File Storage / File Provider Service
**Version:** 3.1 — Final Implementation Specification
**Status:** Final

---

## 1. Purpose

The File Storage capability provides a centralized, reusable abstraction for storing and retrieving files used by any application context.

Entities from different contexts can reference files through a stable identifier:

```csharp
Guid FileId
```

Business contexts do not need to know:

* Where the file is physically stored.
* Which storage vendor is being used.
* How the storage key is generated.
* How files are retrieved.
* How files are deleted.

The physical storage implementation must be replaceable through configuration and dependency injection.

The initial implementation supports local file storage for development while allowing future providers such as Amazon S3, Azure Blob Storage, Google Cloud Storage, MinIO, and others to be introduced without modifying business-context code.

**Version note:** This revision aligns the folder structure and interface placement with the final implementation:

* `IFileStorageProvider` and `StorageObject` live in `Application/Shared/Files/Services/`.
* `IFileStorageCleanupService` lives in `Infrastructure/Files/Abstractions/`.
* Cleanup is split across `Jobs/` and `Services/`.
* `FileDownloadResult` lives in `Models/`.

---

## 2. Architectural Decision

### 2.1 File Storage Is Not a Domain Bounded Context

The File Storage capability must not be introduced as a Domain bounded context.

Files have no independent business domain behavior requiring aggregates, domain events, domain services, domain invariants, business workflows, or a dedicated bounded context.

File Storage is an application/infrastructure capability rather than a Domain bounded context. This avoids unnecessary DDD complexity while keeping the design extensible.

### 2.2 Cross-Context Shared Capability

The feature is placed under `Application/Shared/Files/` because it is consumed by multiple bounded contexts and modules.

The contracts are split by intent.

**Cross-cutting application-facing abstractions:**

* `IFileService`
* `IFileContentInspector`

**Provider-facing contract in the shared application layer:**

* `IFileStorageProvider`
* `StorageObject`

**Infrastructure-internal abstraction:**

* `IFileStorageCleanupService`

**Shared models:**

* `FileType`
* `FileValidationStatus`
* `FileInspectionResult`
* `FileUpload`
* `FileDownloadResult`

**Shared DTO:**

* `FileDto`

`IFileStorageProvider` is placed in `Application/Shared/Files/Services/` because it is a contract whose implementation can be replaced through dependency injection.

However, its intended usage is Infrastructure-only. Business contexts must never depend on it.

The distinction between placement and usage is deliberate: the interface exists in a layer accessible to Infrastructure without introducing a reverse dependency, while the rule that business contexts must not reference it is enforced by architectural convention.

---

## 3. Architectural Goal

The primary architectural goal is:

> Business contexts depend on a stable file identifier and application abstraction, while the physical storage mechanism remains completely replaceable.

The intended dependency structure is:

```text
Application Context
        |
        | FileId
        v
   IFileService
        |
        v
File Storage Service
   (Infrastructure)
        |
        v
IFileStorageProvider
        |
        +---- Local Storage
        |
        +---- S3
        |
        +---- Azure Blob
        |
        +---- Future Provider
```

Business contexts must never depend directly on a storage vendor or on `IFileStorageProvider`.

---

## 4. Cross-Context File Reference

Any application context may contain a file reference.

Examples:

```csharp
public Guid? ImageFileId { get; private set; }

public Guid? InvoiceFileId { get; private set; }

public Guid? AvatarFileId { get; private set; }
```

Conceptually:

```text
Catalog.Product
    └── ImageFileId

Orders.Order
    └── InvoiceFileId

Identity.Person
    └── AvatarFileId
```

The context only knows the `FileId`. It does not know the storage provider, storage path, storage bucket, storage key, cloud URL, or physical file location.

---

## 5. FileId vs. StorageKey

These concepts must remain separate.

| Concept      | Layer                | Purpose                                                        |
| ------------ | -------------------- | -------------------------------------------------------------- |
| `FileId`     | Application-level    | Stable identifier used by business contexts                    |
| `StorageKey` | Infrastructure-level | Physical storage identifier used only by the storage subsystem |

The relationship is:

```text
FileId
   |
   v
StoredFile
   |
   └── StorageKey
          |
          v
   Storage Provider
          |
          v
    Physical Object
```

A business context must never persist or depend on a `StorageKey`.

---

## 6. Stored File Metadata

Although File Storage is not a Domain bounded context, the system maintains a small persistence record representing each stored file.

```csharp
public enum StoredFileStatus
{
    Available = 1,
    PendingDeletion = 2,
    ProcessingDeletion = 3
}

public sealed class StoredFile
{
    public Guid Id { get; set; }

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

`StoredFile` is an Infrastructure persistence model, not a Domain entity.

It provides stable application-level metadata and a mapping between `FileId` and `StorageKey`. Its status and claim fields support concurrency-safe background deletion.

---

## 7. Responsibilities

### 7.1 File Service

The application-facing file service is responsible for:

* Creating file identifiers.
* Generating storage keys.
* Storing physical file content.
* Creating `StoredFile` records.
* Retrieving file metadata.
* Retrieving file content.
* Marking files for deletion.
* Coordinating persistence and storage operations.

`IFileService` does not enforce file-type business rules. It only stores and retrieves files.

### 7.2 Storage Provider

The storage provider represents the physical storage mechanism.

`IFileStorageProvider` represents the following responsibility:

> Store and retrieve file bytes.

The provider must not be responsible for:

* Application-level `FileId`.
* Business ownership.
* Authorization.
* Domain entities.
* Business rules.
* Generating application DTOs.

The contract is declared in `Application/Shared/Files/Services/`. Implementations live in Infrastructure.

Business contexts must never inject `IFileStorageProvider`.

### 7.3 File Storage Cleanup Service

The cleanup coordinator is an Infrastructure-internal abstraction.

`IFileStorageCleanupService` is declared in:

```text
Infrastructure/Files/Abstractions/
```

It is implemented by `FileStorageCleanupService` in:

```text
Infrastructure/Files/Services/
```

It is invoked by `FileStorageCleanupJobHandler`. Business contexts never see it.

---

## 8. Application Abstractions

### 8.1 IFileService

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

`DownloadAsync` returns both metadata and the content stream in one call to avoid performing separate database queries for metadata and download information.

`DeleteAsync` marks the file as `PendingDeletion`. Physical deletion occurs asynchronously.

### 8.2 IFileContentInspector

A shared, generic technical capability for inspecting uploaded file content.

```csharp
public interface IFileContentInspector
{
    Task<FileInspectionResult> InspectAsync(
        Stream content,
        FileType expectedType,
        CancellationToken ct = default);
}
```

The inspector returns technical facts. It does not enforce business policies.

Business contexts may use the inspection result to enforce their own file-type requirements.

### 8.3 IFileStorageProvider

A contract for physical storage operations, declared in the shared Application layer but implemented only by Infrastructure.

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

Important contract requirements:

* `StoreAsync` takes `(content, storageKey)` in that order.
* `DeleteAsync` is idempotent.
* `ListObjectsAsync` exposes creation timestamps when the provider can determine them.
* Business contexts must never inject or reference this interface.

### 8.4 Shared File Models

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

#### StorageObject

```csharp
public sealed record StorageObject(
    string Key,
    DateTime? CreatedAtUtc);
```

### 8.5 DTOs

`FileDto` is the application-level file metadata DTO.

It contains:

* `Id`
* `FileName`
* `ContentType`
* `Size`
* `CreatedAtUtc`

### 8.6 Permissions

```csharp
public static class FilePermissions
{
    public const string Read = "Files.Read";
    public const string Upload = "Files.Upload";
    public const string Delete = "Files.Delete";
    public const string Manage = "Files.Manage";
}
```

These constants provide API-level policy names and are available for future MediatR authorization if file-management commands are introduced.

---

## 9. Provider Implementations

The initial provider is `LocalFileStorageProvider`, which stores files on the local filesystem.

Future providers can implement the same abstraction:

```text
        IFileStorageProvider
                 ^
                 |
        +--------+---------+
        |        |         |
      Local      S3    Azure Blob
```

Business contexts never see `IFileStorageProvider`. They depend only on `IFileService`.

---

## 10. Configuration-Driven Provider Selection

The provider is selected through configuration.

### Example Configuration

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

Development uses:

```json
{
  "FileStorage": {
    "Provider": "Local"
  }
}
```

Production can select S3, Azure Blob Storage, or another supported provider.

Unknown provider names throw an `InvalidOperationException` during startup. Provider-specific configuration remains isolated.

The configuration structure can accommodate future providers:

```text
FileStorage
├── Provider
├── MaxFileSizeBytes
├── AllowedContentTypes
├── AllowedExtensions
├── OrphanGracePeriod
├── DeletionClaimLease
├── Local
│   └── RootPath
├── S3
│   ├── Bucket
│   ├── Region
│   └── ...
└── AzureBlob
    ├── Container
    └── ...
```

The Application layer never references provider-specific configuration classes.

---

## 11. Dependency Injection Registration

Provider implementations are registered behind the abstraction.

```csharp
services.AddFileStorage(configuration);
```

Inside `FileStorageInfrastructureExtensions`:

* `"Local"` registers `LocalFileStorageProvider` as `IFileStorageProvider`.
* Any unsupported provider value throws an `InvalidOperationException`.

The registration also includes:

* `IFileService` → `FileService`
* `IFileContentInspector` → `FileContentInspector`
* `FileStorageCleanupService` as a concrete service for the cleanup job handler to resolve.

`IFileStorageProvider` is injected into `FileService` and `FileStorageCleanupService`, but never into business contexts.

---

## 12. File Storage Flow

### 12.1 Upload

```text
Application Context
        |
        v
IFileService.UploadAsync(...)
        |
        +-- Generate FileId
        |
        +-- Generate StorageKey
        |
        +-- Stream through SizeLimitedReadStream
        |
        +-- Store physical object
        |
        +-- Persist StoredFile metadata
        |
        +-- If DB fails, compensate by deleting
        |   the physical object
        |
        +-- Return FileId
```

The calling context stores the returned `FileId`.

### 12.2 Download

```text
FileId
  |
  v
StoredFile
  |
  └── StorageKey
          |
          v
  IFileStorageProvider
          |
          v
        Stream
```

The caller never needs to know the storage key.

### 12.3 Delete

```text
FileId
  |
  v
Mark StoredFile as PendingDeletion
(atomic status change)
  |
  v
Background cleanup job claims the record
  |
  v
Delete physical object
  |
  v
Remove StoredFile metadata
(claim-conditional)
```

Actual physical deletion is asynchronous.

---

## 13. No Direct Storage Access From Contexts

The following dependencies are prohibited:

```text
Order    --> S3 SDK
Product  --> File System
Identity --> Azure Blob SDK
```

Instead, business contexts use the shared application abstraction:

```text
Order
  |
  v
IFileService
  |
  v
FileService (Infrastructure)
  |
  v
IFileStorageProvider
  |
  v
Physical Storage
```

This prevents infrastructure and vendor-specific dependencies from leaking into business contexts.

---

## 14. No Navigation Properties Between Context Entities and Files

A business entity should not contain a navigation property to `StoredFile`:

```csharp
public StoredFile File { get; set; }
```

Such coupling should only be introduced if a future requirement explicitly justifies it.

Prefer:

```csharp
public Guid? FileId { get; private set; }
```

This keeps contexts loosely coupled.

Value-object wrappers such as `BrandLogo` or `PersonalImage` that contain a `FileId` are acceptable and preferred when the domain benefits from the wrapper's invariants.

---

## 15. File Ownership

The File Storage capability does not automatically assume business ownership.

For example:

```csharp
Product.ImageFileId
```

means that the Product references a file, not that File Storage owns the Product lifecycle.

Ownership rules belong to the application context that uses the file.

---

## 16. Orphan Files

Orphan files are physical objects without corresponding `StoredFile` metadata.

They may occur when:

* Physical storage succeeds, but metadata persistence fails.
* Metadata is deleted, but physical deletion fails.
* A provider or application crash occurs at an unfortunate point in the workflow.

A background cleanup job handles orphan detection and removal.

The initial implementation does not use reference counting or complex file lifecycle management.

---

## 17. Consistency Model

SQL Server and physical file storage are separate resources. They cannot participate in the same database transaction.

The design uses **eventual consistency, compensation, and retry**.

### 17.1 Upload Consistency

```text
Generate FileId + StorageKey
          |
          v
Store physical object
          |
          v
Persist StoredFile metadata
          |
          v
If DB fails, attempt immediate
physical deletion
          |
          v
If compensation fails,
orphan cleanup handles it later
```

The intended invariants are:

* Every committed `StoredFile` record should eventually have a corresponding physical object.
* Every physical object without committed metadata should eventually be removed.

### 17.2 Deletion Consistency

The deletion lifecycle is:

```text
Available
    |
    v
PendingDeletion
(atomic status change)
    |
    v
ProcessingDeletion
(claimed by worker)
    |
    +-- Physical deletion fails
    |       |
    |       v
    |   Revert to PendingDeletion
    |   (claim-safe)
    |
    +-- Physical deletion succeeds
            |
            v
        Claim-conditional
        metadata removal
```

Expired claims are recovered to `PendingDeletion` by the cleanup job.

---

## 18. Deletion State Machine

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

### Key Invariants

1. `Available` → `PendingDeletion` is atomic.
2. `PendingDeletion` → `ProcessingDeletion` is conditionally atomic.
3. Only the current claim owner can finalize deletion.
4. Only the current claim owner can revert a failed deletion.
5. Expired claims can be recovered.
6. Physical deletion is idempotent.

These rules protect against concurrent workers and stale workers attempting to finalize a deletion after losing their claims.

---

## 19. Orphan Cleanup

The cleanup job uses `ListObjectsAsync` to list physical storage objects.

It deletes objects that meet **all** of the following conditions:

* They have no corresponding `StoredFile` record.
* They have a known creation timestamp.
* They are older than `OrphanGracePeriod`.

Objects with an unknown creation time are skipped conservatively to avoid racing with uploads.

---

## 20. Authorization

File Storage integrates with the existing authorization architecture.

### 20.1 Permissions

```text
Files.Read
Files.Upload
Files.Delete
Files.Manage
```

### 20.2 API-Level Authorization

For the current version, authorization is enforced at the API layer using policy names:

```csharp
[Authorize(Policy = "Permission:Files.Upload")]
[Authorize(Policy = "Permission:Files.Read")]
[Authorize(Policy = "Permission:Files.Delete")]
```

This is a deliberate deviation from the MediatR-pipeline convention used by other contexts.

File Storage operations are direct `IFileService` calls rather than MediatR commands, so there is no request type to decorate with `[AuthorizePermission]`. The policy gate is therefore applied at the controller.

Permission authorization and resource authorization remain separate concerns. Resource ownership rules belong to the owning application context.

---

## 21. Security Requirements

The subsystem must:

* Enforce configurable file-size limits.
* Avoid trusting client-provided storage paths.
* Generate storage keys internally.
* Prevent path traversal.
* Avoid exposing physical storage paths.
* Avoid exposing provider credentials.
* Avoid storing secrets in source code.
* Support cancellation.
* Handle storage failures safely.

File-type validation is not the responsibility of File Storage.

Business contexts may use `IFileContentInspector` to enforce their own specific file-type rules.

Provider credentials must come from secure configuration or environment-based secret management, not hardcoded source-code values.

---

## 22. Final Folder Structure

```text
Application/
└── Shared/
    ├── Files/
    │   ├── Dtos/
    │   │   └── FileDto.cs
    │   ├── Models/
    │   │   ├── FileDownloadResult.cs
    │   │   ├── FileInspectionResult.cs
    │   │   ├── FileType.cs
    │   │   ├── FileUpload.cs
    │   │   └── FileValidationStatus.cs
    │   └── Services/
    │       ├── IFileContentInspector.cs
    │       ├── IFileService.cs
    │       ├── IFileStorageProvider.cs
    │       └── StorageObject.cs
    └── Security/
        └── Authorization/
            └── Permissions/
                └── FilePermissions.cs

Infrastructure/
├── Files/
│   ├── Abstractions/
│   │   └── IFileStorageCleanupService.cs
│   ├── Configuration/
│   │   ├── FileStorageOptions.cs
│   │   ├── FileStorageOptionsValidator.cs
│   │   └── LocalFileStorageOptions.cs
│   ├── Entities/
│   │   └── StoredFile.cs
│   ├── Extensions/
│   │   └── FileStorageInfrastructureExtensions.cs
│   ├── Jobs/
│   │   ├── FileStorageCleanupJobHandler.cs
│   │   ├── FileStorageCleanupJobPayload.cs
│   │   └── FileStorageCleanupTrigger.cs
│   ├── Services/
│   │   ├── FileContentInspector.cs
│   │   ├── FileService.cs
│   │   ├── FileStorageCleanupService.cs
│   │   └── SizeLimitedReadStream.cs
│   └── Storage/
│       └── LocalFileStorageProvider.cs
└── Persistence/
    ├── Context/
    │   └── AppDbContext.Files.cs
    └── Modules/
        └── Files/
            ├── Configurations/
            │   └── StoredFileConfiguration.cs
            └── Repositories/
                └── StoredFileRepository.cs

API/
├── Controllers/
│   └── v1/
│       └── Files/
│           └── FilesController.cs
└── DTOs/
    └── v1/
        └── Files/
            ├── Requests/
            │   └── UploadFileRequest.cs
            └── Responses/
                └── FileResponse.cs
```

---

## 23. What This Feature Is Not

The initial feature is not:

* A Domain bounded context.
* A file-sharing platform.
* A document-management system.
* A media-processing system.
* A cloud abstraction containing every possible provider feature.
* A generic "do everything with files" service.

Its responsibility is intentionally narrow: provide a stable application-level file abstraction backed by a replaceable physical storage provider.

---

## 24. Design Principles

* **Stable identity** — `FileId` is the cross-context reference.
* **Provider independence** — Business code does not know the storage vendor.
* **Configuration-driven infrastructure** — Storage provider selection is configurable.
* **Minimal abstraction** — `IFileStorageProvider` exposes only the required operations.
* **No domain pollution** — File persistence does not become a Domain concern without actual business behavior.
* **Loose coupling** — Contexts reference files by ID rather than navigation properties.
* **Explicit boundaries** — `IFileService` handles application file management; `IFileStorageProvider` handles physical storage. The provider contract lives in the shared Application layer but is implemented only by Infrastructure and is never injected into business contexts.
* **Generic technical inspection** — `IFileContentInspector` reports facts; business contexts decide what those facts mean.
* **Evolution over speculation** — Start simple and introduce advanced capabilities only when requirements justify them.
* **Eventual consistency** — Do not pretend SQL Server and physical storage form a distributed transaction.

---

## 25. Final Architecture

### 25.1 Application-Level Dependencies

```text
                     APPLICATION
                          |
            +-------------+-------------+
            |             |             |
          Orders        Catalog       Identity
            |             |             |
          FileId        FileId        FileId
            |             |             |
            +-------------+-------------+
                          |
                          v
                     IFileService
                          |
                          v
                FileService (Infrastructure)
                          |
                          v
                 IFileStorageProvider
                          |
             +------------+------------+
             |            |            |
             v            v            v
           Local          S3       Azure Blob
             |            |            |
             +------------+------------+
                          |
                    Configuration
```

### 25.2 Metadata Relationship

```text
Application Context
        |
        | Guid FileId
        v
    StoredFile
        |
        | StorageKey
        v
IFileStorageProvider
        |
        v
  Physical Storage
```

---

## 26. Final Architectural Conclusion

The agreed design is:

File Storage is a cross-cutting application/infrastructure capability, not a Domain bounded context. It provides a stable `FileId` abstraction that any application context can use without coupling itself to physical storage.

The system maintains a lightweight `StoredFile` persistence record containing metadata, status, and the provider-specific `StorageKey`.

Physical storage is hidden behind `IFileStorageProvider`. The contract is declared in `Application/Shared/Files/Services/` for dependency-injection friendliness, but its implementations live in Infrastructure, and business contexts never reference it.

Development can use `LocalFileStorageProvider`, while production can switch to S3, Azure Blob Storage, MinIO, or another provider without modifying business contexts.

The architecture deliberately avoids premature complexity while including concurrency and consistency mechanisms needed for reliable operation:

* Atomic deletion state transitions.
* Claim-based background processing.
* Lease recovery for stuck workers.
* Orphan cleanup with a grace period.
* Idempotent physical deletions.
* Streaming size validation.
* Generic content inspection with application-level file-type policies.

The target is a file-storage capability that is simple, reliable, replaceable by configuration, isolated from business logic, and extensible when real requirements emerge.

---

**Document version:** 3.1 — Final
**Feature:** File Storage / File Provider Service

---

## Summary of Changes from v3.0

| Section | Change                                                                                                                                                                                     |
| ------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| §2.2    | `IFileStorageProvider` and `StorageObject` are in `Application/Shared/Files/Services/`, not Infrastructure. Added `IFileStorageCleanupService` as the Infrastructure-internal abstraction. |
| §3      | Corrected the diagram: `IFileStorageProvider` is no longer labeled "Infrastructure internal."                                                                                              |
| §7      | Split into §7.1 File Service, §7.2 Storage Provider, and §7.3 File Storage Cleanup Service.                                                                                                |
| §8.3    | Added the full `IFileStorageProvider` interface with `StorageObject`.                                                                                                                      |
| §8.4    | Separated `FileDownloadResult` and `StorageObject` into their appropriate model locations.                                                                                                 |
| §8.5    | Confirmed that only `FileDto` lives in `Dtos/`.                                                                                                                                            |
| §9      | Removed the claim that the provider contract is internal to Infrastructure.                                                                                                                |
| §11     | Added `FileStorageCleanupService` registration.                                                                                                                                            |
| §20     | Documented the deliberate deviation from the MediatR-pipeline authorization convention.                                                                                                    |
| §22     | Corrected the folder structure: `Dtos/` versus `Models/`, the `Jobs/` folder, `IFileStorageCleanupService`, and the API `v1/` segment.                                                     |
