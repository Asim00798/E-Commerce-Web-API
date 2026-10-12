# File Storage Capability — Technical Reference Document

**Project:** E-Commerce Modular Monolith
**Version:** 1.2 — Final
**Status:** Final

---

## 1. Overview

The File Storage capability provides a centralized, reusable abstraction for storing and retrieving files used by any application context.

The capability allows entities from different contexts to reference files through a stable identifier:

```csharp
Guid FileId
```

without knowing:

* Where the file is physically stored.
* Which storage vendor is being used.
* How the storage key is generated.
* How files are retrieved.
* How files are deleted.

The physical storage implementation is replaceable through configuration and dependency injection. The initial implementation supports local file storage for development, while allowing future providers such as Amazon S3, Azure Blob Storage, Google Cloud Storage, or MinIO to be introduced without modifying business-context code.

**Version note:** This revision aligns the folder structure, interface placement, and API surface with the final implementation:

* `IFileStorageProvider` and `StorageObject` are declared in `Application/Shared/Files/Services/`.
* `IFileStorageCleanupService` is declared in `Infrastructure/Files/Abstractions/`.
* The cleanup subsystem is split across `Jobs/` and `Services/`.
* Models and DTOs are separated according to their serialization constraints.
* The API route is `api/files`, without a `v1` URL segment.
* Authorization uses a role gate without a permission policy layer.

---

## 2. Architectural Decision

### 2.1. Not a Domain Bounded Context

File Storage is not a Domain bounded context. Files have no independent business domain behavior requiring aggregates, domain events, domain services, or domain invariants.

The capability is primarily concerned with technical file persistence.

File Storage is an application/infrastructure capability rather than a Domain bounded context. This avoids unnecessary DDD complexity while keeping the design extensible.

### 2.2. Cross-Cutting Shared Capability

The feature is placed under `Application/Shared/Files` because it is consumed by multiple bounded contexts and modules:

* **Catalog:** Product image files.
* **Orders:** Invoice files.
* **Identity/People:** Avatar files.

The contracts are split by intent.

**Cross-cutting application-facing abstractions:**

* `IFileService` — the abstraction business contexts consume.
* `IFileContentInspector` — generic technical content inspection.

**Provider-facing contracts in the shared Application layer:**

* `IFileStorageProvider`
* `StorageObject`

**Infrastructure-internal abstraction:**

* `IFileStorageCleanupService`

`IFileStorageProvider` is placed in `Application/Shared/Files/Services/` because it is a contract implemented by Infrastructure. However, its intended consumers are exclusively within Infrastructure: business contexts must never inject it.

The distinction between placement and usage is deliberate. The interface resides in the shared Application layer to maintain dependency direction, while its Infrastructure-only usage is enforced by architectural convention.

---

## 3. Architecture at a Glance

```text
                         Application
                              |
                              v
                         IFileService
                              |
                              v
                        Infrastructure
                              |
                 +------------+------------+
                 |                         |
                 v                         v
        StoredFileRepository       IFileStorageProvider
                 |                         |
                 v                +--------+--------+
             SQL Server           |        |        |
                                  v        v        v
                                Local      S3      Azure
```

Business contexts only know:

```csharp
Guid FileId
```

and:

```csharp
IFileService
```

They do not know:

* Local file-system paths.
* S3 keys.
* Azure blob names.
* Storage containers.
* Provider SDKs.

---

## 4. FileId vs. StorageKey

These concepts must remain separate.

| Concept      | Layer                | Purpose                                                         |
| ------------ | -------------------- | --------------------------------------------------------------- |
| `FileId`     | Application-level    | Stable identifier used by business contexts.                    |
| `StorageKey` | Infrastructure-level | Physical storage identifier used only by the storage subsystem. |

The relationship is:

```text
FileId
   |
   v
StoredFile
   |
   +---- StorageKey
              |
              v
       Storage Provider
              |
              v
       Physical Object
```

A business context must never persist or depend on a `StorageKey`.

---

## 5. Folder Structure

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

### Folder Rules

* `Dtos/` holds shapes that cross a serialization boundary. Only `FileDto` qualifies because it is returned in HTTP responses.
* `Models/` holds internal shapes. `FileUpload` and `FileDownloadResult` carry streams and cannot be serialized directly as ordinary JSON DTOs.
* `Services/` under `Application/Shared/Files/` holds the shared contracts. `IFileStorageProvider` and `StorageObject` are colocated with `IFileService` because they are declared in the same shared layer.

---

## 6. Application Layer

### 6.1. IFileService

The application-facing file management abstraction.

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

`DeleteAsync` durably marks the file as `PendingDeletion`. Physical deletion and metadata removal happen asynchronously.

### 6.2. IFileContentInspector

A generic technical capability for inspecting uploaded file content. It returns technical facts and does not enforce business policies.

```csharp
public interface IFileContentInspector
{
    Task<FileInspectionResult> InspectAsync(
        Stream content,
        FileType expectedType,
        CancellationToken ct = default);
}
```

The implementation preserves stream position: it reads the leading bytes to determine the signature, then seeks back to the original position. This allows a caller to inspect and subsequently upload the same stream.

Currently recognized image signatures include PNG, JPEG, GIF, and WebP.

### 6.3. IFileStorageProvider

An abstraction for physical storage operations. It is declared in the shared Application layer but implemented and consumed only by Infrastructure.

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
    /// Must be idempotent. No exception when the object does not exist.
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

**Parameter order:** `StoreAsync(content, storageKey)`.

**Deletion semantics:** `DeleteAsync` is idempotent. A missing object is treated as success; a genuine storage failure throws an exception.

#### StorageObject

```csharp
public sealed record StorageObject(
    string Key,
    DateTime? CreatedAtUtc);
```

`CreatedAtUtc` supports the orphan-cleanup grace period. A `null` timestamp indicates an unknown creation time, and the cleanup job skips such objects conservatively.

**Architectural discipline:** Business contexts must never depend on `IFileStorageProvider` or `StorageObject`. The interface exists in the shared layer so Infrastructure can implement and register it; the usage boundary is enforced by convention, not by project reference.

### 6.4. Shared Models

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

The API layer translates `IFormFile` into `FileUpload` before dispatching. The Application layer does not depend on `IFormFile`.

#### FileDownloadResult

```csharp
public sealed record FileDownloadResult(
    FileDto Metadata,
    Stream Content);
```

This model holds both metadata and the content stream, avoiding a second database query.

### 6.5. DTOs

**`FileDto`** represents application-level file metadata:

* `Id`
* `FileName`
* `ContentType`
* `Size`
* `CreatedAtUtc`

It is the only DTO in this capability because it is the only shape that crosses the HTTP serialization boundary.

### 6.6. Permissions

```csharp
public static class FilePermissions
{
    public const string Read = "Files.Read";
    public const string Upload = "Files.Upload";
    public const string Delete = "Files.Delete";
    public const string Manage = "Files.Manage";
}
```

These constants are declared for future use but are **not currently enforced by any layer**. The `FilesController` applies a role gate only. See Section 9.

---

## 7. Infrastructure Layer

### 7.1. StoredFile Entity

`StoredFile` is an Infrastructure persistence model, not a Domain entity.

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

`Id` is the externally referenced `FileId`.

### 7.2. FileService

`FileService` implements `IFileService` and coordinates storage operations with metadata persistence.

#### Upload Flow

1. Generate `FileId` and `StorageKey`.
2. Stream content through `SizeLimitedReadStream`.
3. Store the physical object through the configured provider.
4. Persist `StoredFile` metadata in SQL Server.
5. If database persistence fails, attempt to compensate by deleting the physical object.
6. If compensation fails, orphan cleanup will handle the remaining object.

#### Download Flow

Retrieves metadata and the content stream in one call.

#### Delete Flow

Atomically marks the file as `PendingDeletion`. It does not physically delete the file immediately.

### 7.3. FileContentInspector

`FileContentInspector` implements `IFileContentInspector`. It detects file types from leading-byte magic-number signatures.

* Preserves stream position by reading the required bytes and then seeking back.
* Returns `Unknown` for non-seekable streams.
* Currently recognizes PNG, JPEG, GIF, and WebP image signatures.
* Returns `Unknown` when content is empty or unrecognized.
* Returns `Invalid` when a recognized type does not match `expectedType`.
* Returns `Valid` when the detected type matches the expected type.

### 7.4. LocalFileStorageProvider

`LocalFileStorageProvider` implements `IFileStorageProvider` for the local file system.

* **Path traversal protection:** Uses a root-containment check with a trailing separator.
* **Idempotent deletion:** `DeleteAsync` treats a missing file as success but throws on genuine failures.
* **Incomplete-upload cleanup:** `StoreAsync` removes an incomplete file when storage fails.
* **Object enumeration:** `ListObjectsAsync` returns storage objects with creation timestamps.

### 7.5. FileStorageCleanupService

`FileStorageCleanupService` implements `IFileStorageCleanupService`, declared in `Infrastructure/Files/Abstractions/`.

It is invoked by `FileStorageCleanupJobHandler` and performs two responsibilities per run.

#### 1. Process Pending Deletions

1. Recover expired claims.
2. Atomically claim a batch using a new `ClaimId`.
3. Delete each physical object through the provider.
4. Remove the database record only if the worker still owns the claim, using `RemoveIfClaimedAsync`.
5. On failure, revert the record to `PendingDeletion`, provided the claim is still owned.

#### 2. Perform Orphan Cleanup

1. List physical objects with creation timestamps.
2. Identify objects absent from the database and older than `OrphanGracePeriod`.
3. Delete eligible orphan objects.
4. Skip objects with unknown creation times.

### 7.6. Cleanup Jobs

The cleanup subsystem follows the scheduling-module pattern and is split into three files under `Infrastructure/Files/Jobs/`.

* **`FileStorageCleanupJobPayload`** — implements `IJob`. The payload is empty.
* **`FileStorageCleanupJobHandler`** — implements `IJobHandler<FileStorageCleanupJobPayload>` and delegates to `IFileStorageCleanupService.ExecuteAsync`.
* **`FileStorageCleanupTrigger`** — implements `IRecurringJobTrigger` and is marked with `[RecurringJob("file-storage-cleanup", "*/30 * * * *")]`. It enqueues the payload.

All three components are automatically discovered by the existing scan patterns.

### 7.7. SizeLimitedReadStream

`SizeLimitedReadStream` wraps the input stream and enforces the maximum file size during copying. It does not rely on `Stream.Length`.

* Tracks cumulative bytes read through `Read` and `ReadAsync` overrides.
* Throws `InvalidOperationException` when the cumulative count exceeds `MaxFileSizeBytes`.
* Reports `TotalBytesRead` after a successful copy, which `FileService` persists as `StoredFile.Size`.
* Disposes the inner stream when disposed. The wrapper owns the input stream.

### 7.8. Configuration

#### FileStorageOptions

* `Provider` — defaults to `"Local"`.
* `MaxFileSizeBytes`
* `AllowedContentTypes`
* `AllowedExtensions`
* `OrphanGracePeriod`
* `DeletionClaimLease`
* `Local` — nested `LocalFileStorageOptions`.

#### LocalFileStorageOptions

* `RootPath` — defaults to `"storage/files"`.

**Validation:** `FileStorageOptionsValidator` ensures that the provider, size limit, grace period, deletion lease, and local root path (when the provider is `Local`) are valid. Validation is registered with `ValidateOnStart`.

### 7.9. Extensions

`FileStorageInfrastructureExtensions.AddFileStorage(configuration)` performs the following registrations:

* Binds `FileStorageOptions` with `ValidateOnStart`.
* Registers `FileStorageOptionsValidator`.
* Registers the provider based on configuration:

  * `"Local"` → `LocalFileStorageProvider` as `IFileStorageProvider`.
  * Any other value → throws `InvalidOperationException` during startup.
* Registers `IFileService` → `FileService`.
* Registers `IFileContentInspector` → `FileContentInspector`.
* Registers `IFileStorageCleanupService` → `FileStorageCleanupService`.
* Registers `StoredFileRepository` as a concrete type.

`StoredFileRepository` is registered explicitly because the shared `AddRepositories` Scrutor scan uses `AsImplementedInterfaces()`, while `StoredFileRepository` has no interface. Explicit registration is required for `FileService` and `FileStorageCleanupService` to resolve it.

---

## 8. Persistence

### 8.1. DbContext and Entity Configuration

* `AppDbContext.Files.cs` exposes `DbSet<StoredFile>`.
* `StoredFileConfiguration` maps the entity to `files.StoredFiles`.

The mapping includes:

* A unique index on `StorageKey`.
* A non-unique index on `Status`.
* A non-unique index on `DeletionClaimId`.
* `RowVersion` as a concurrency token.

### 8.2. StoredFileRepository

`StoredFileRepository` provides the following operations:

| Method                              | Responsibility                                                                                                                    |
| ----------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| `GetByIdAsync`                      | Uses `AsNoTracking`; returns `null` when the record is absent.                                                                    |
| `AddAsync`                          | Stages the entity; the caller saves changes.                                                                                      |
| `MarkPendingDeletionAsync`          | Uses atomic `ExecuteUpdate`, conditional on `Status = Available`.                                                                 |
| `ClaimPendingDeletionBatchAsync`    | Selects pending IDs, conditionally updates them to `ProcessingDeletion` with a claim ID, then reselects rows matching that claim. |
| `RecoverExpiredDeletionClaimsAsync` | Atomically resets expired claims to `PendingDeletion`.                                                                            |
| `RemoveIfClaimedAsync`              | Uses atomic `ExecuteDelete`, conditional on claim ownership.                                                                      |
| `RevertToPendingDeletionAsync`      | Uses atomic `ExecuteUpdate`, conditional on claim ownership.                                                                      |

The claim-based methods use conditional `ExecuteUpdate` and `ExecuteDelete` operations with `WHERE` clauses that include the claim ID. This prevents concurrent workers from successfully claiming or finalizing the same row under conflicting claim ownership.

---

## 9. API Layer

### 9.1. FilesController

`FilesController` is a thin HTTP adapter.

| Method   | Endpoint                   | Authorization                            |
| -------- | -------------------------- | ---------------------------------------- |
| `POST`   | `/api/files`               | Roles: Administrator, Customer, Employee |
| `GET`    | `/api/files/{id}`          | Roles: Administrator, Customer, Employee |
| `GET`    | `/api/files/{id}/download` | Roles: Administrator, Customer, Employee |
| `DELETE` | `/api/files/{id}`          | Roles: Administrator, Customer, Employee |

`DELETE` marks the file for asynchronous deletion and returns `202 Accepted`.

#### Authorization Convention

File Storage uses a class-level role gate only:

```csharp
[Authorize(
    Roles = $"{SystemRoles.Administrator},{SystemRoles.Customer},{SystemRoles.Employee}")]
```

Its operations call `IFileService` directly rather than using MediatR commands. Consequently, there is no request type to decorate with `[AuthorizePermission]`, and no permission policy is applied at the controller.

The route is `api/files`. The API uses folder-based versioning (`Controllers/v1/Files/`) without a `v1` segment in the URL, consistent with the other controllers in the codebase.

**Ownership limitation:** No ownership check exists. Any authenticated user in the allowed roles can access any file by `FileId`. Access is capability-based: possession of the UUID is the effective capability.

### 9.2. Request and Response DTOs

* **`UploadFileRequest`** — contains `IFormFile`.
* **`FileResponse`** — represents metadata returned by the API.

The API layer owns HTTP concerns, including `IFormFile`, status codes, request validation, response contracts, content disposition, and content type.

The Application layer does not depend on `IFormFile`.

---

## 10. Consistency Model

SQL Server and physical storage are separate resources. No distributed transaction is attempted.

### 10.1. Upload

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
If SQL persistence fails
          |
          v
Attempt immediate physical deletion
          |
          +---- Success --> No orphan remains
          |
          +---- Failure --> Orphan cleanup handles it later
```

**Invariant:** Every committed `StoredFile` should eventually have a physical object, and every physical object without committed metadata should eventually be removed.

### 10.2. Deletion

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
    +---- Success
    |        |
    |        v
    |    Delete physical object
    |        |
    |        v
    |    Claim-conditional database removal
    |
    +---- Failure
             |
             v
       Revert to PendingDeletion
       (claim-safe)
```

Expired claims are recovered to `PendingDeletion` by the cleanup job.

### 10.3. Orphan Cleanup

Physical objects absent from the database are considered orphan candidates.

A grace period (`OrphanGracePeriod`) prevents deletion of recently uploaded objects whose metadata may not yet have been committed.

Objects with unknown creation times are skipped conservatively.

---

## 11. Concurrency and Lease Safety

* Deletion claims use a `DeletionClaimId` GUID.
* Conditional `ExecuteUpdate` ensures that only one worker can successfully transition a row from `PendingDeletion` to `ProcessingDeletion`.
* Revert and removal operations also verify claim ownership.
* Expired claims are recovered based on `DeletionClaimLease`.
* Physical deletion is idempotent, and claim-conditional finalization prevents workers from removing database records after losing claim ownership.

---

## 12. Security

* **Authorization:** A single role gate at the controller permits Administrator, Customer, and Employee roles. There is no permission policy layer and no ownership check. Access is capability-based: possession of a `FileId` is the effective capability.
* **File metadata validation:** Allowed content types, extensions, and sizes are enforced by `FileService.ValidateFileMetadata` against `FileStorageOptions` allowlists. Empty allowlists skip validation for the corresponding dimension.
* **Path traversal prevention:** `LocalFileStorageProvider` uses a root-containment check with a trailing separator.
* **Provider credentials:** Sensitive configuration and provider credentials must be kept out of source control.
* **Content/signature validation:** `IFileContentInspector` is available but is not automatically enforced by File Storage itself. Business contexts invoke it when they need to enforce file-type policies.
* **Stream size enforcement:** `SizeLimitedReadStream` enforces the configured size limit without trusting `Stream.Length`.

---

## 13. Extensibility

Adding a new storage provider requires the following steps:

1. Implement `IFileStorageProvider`.
2. Add provider-specific configuration.
3. Register the provider in `FileStorageInfrastructureExtensions`.
4. Select the provider through the `FileStorage:Provider` configuration setting.

No business-context or Application code changes are required.

---

## 14. Summary

File Storage is a cross-cutting infrastructure capability with the following characteristics:

* A stable `FileId` abstraction.
* Provider-independent storage design.
* Explicit separation of `FileId` and `StorageKey`.
* `IFileStorageProvider` and `StorageObject` declared in the shared Application layer but implemented and consumed only by Infrastructure.
* `IFileStorageCleanupService` as an Infrastructure-internal contract.
* A cleanup subsystem split across `Jobs/` (payload, handler, trigger) and `Services/` (coordinator).
* Generic content inspection through `IFileContentInspector`, with stream-position preservation.
* Durable file metadata in SQL Server.
* Idempotent physical deletion.
* Claim-based background deletion with lease recovery.
* Orphan cleanup with a grace period.
* Role-based authorization at the controller, with no permission policy layer or ownership check.
* Configuration-driven provider selection.

This design avoids premature complexity while providing mechanisms for reliable file storage, asynchronous cleanup, and concurrent deletion handling.

---

## Appendix A. Summary of Changes from Version 1.0

| Section        | Change                                                                                                                                                                                                                                                             |
| -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Version header | Updated to version 1.2 and added the version note.                                                                                                                                                                                                                 |
| §2.2           | Corrected `IFileStorageProvider` placement: declared in the shared Application layer but implemented and consumed only by Infrastructure. Added `IFileContentInspector` and `IFileStorageCleanupService` contracts.                                                |
| §3             | Clarified the architectural boundary: business contexts see only `FileId` and `IFileService`.                                                                                                                                                                      |
| §5             | Corrected the folder structure, separated `Models/` and `Dtos/`, added `Abstractions/` and `Jobs/`, and included `FileContentInspector` and `FileStorageCleanupService` under `Services/`. Clarified that API folder paths include `v1/`, while the routes do not. |
| §6             | Split the section into `IFileService`, `IFileContentInspector`, `IFileStorageProvider`, shared models, DTOs, and permissions. Added stream-position preservation details.                                                                                          |
| §6.4           | Reclassified `FileDownloadResult` from a DTO to a model because it carries a stream.                                                                                                                                                                               |
| §6.6           | Clarified that `FilePermissions` constants exist but are not currently enforced.                                                                                                                                                                                   |
| §7             | Added `FileContentInspector`, `FileStorageCleanupService`, and the three-file cleanup job structure. Documented `SizeLimitedReadStream` ownership and the exact dependency registrations, including explicit `StoredFileRepository` registration.                  |
| §8             | Added `StoredFileRepository` concurrency-method details.                                                                                                                                                                                                           |
| §9             | Corrected the endpoint table to use `/api/files` rather than `/api/v1/files`. Updated authorization to reflect the role gate, absence of permission policies, and absence of ownership checks.                                                                     |
| §12            | Updated the security description to reflect the current authorization model and clarified that content inspection is available through `IFileContentInspector`.                                                                                                    |
| §14            | Aligned the summary with the final implementation.                                                                                                                                                                                                                 |

---

**Document Version:** 1.2 — Final
**Feature:** File Storage / File Provider Service
