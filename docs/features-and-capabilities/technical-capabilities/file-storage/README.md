# File Storage Capability

**Project:** E-Commerce Modular Monolith
**Category:** Technical Capability
**Status:** Implemented

## 1. Overview

The File Storage capability provides a centralized mechanism for storing, retrieving, and deleting physical files while maintaining their metadata in the database.

It separates file storage infrastructure from business features, allowing modules such as Catalog, People, and other consuming contexts to manage file references without depending on a specific storage provider or physical storage implementation.

File Storage is a **shared technical capability, not a Domain bounded context**. Business contexts own the meaning and lifecycle of their file references, while the File Storage capability manages physical storage, storage metadata, and the consistency challenges associated with coordinating database and storage operations.

The design supports reliable file operations through explicit abstractions, compensating actions, idempotent deletion, claim-aware cleanup, lease-based recovery, and orphan detection.

## 2. Responsibilities

The capability is responsible for:

* Uploading files and storing their metadata.
* Providing stable file identifiers for use by business features.
* Retrieving file metadata and opening file content for download.
* Abstracting physical storage behind `IFileStorageProvider`.
* Enforcing configured file size, extension, and content-type restrictions.
* Supporting optional file content inspection through `IFileContentInspector`.
* Managing asynchronous file deletion.
* Recovering from interrupted deletion operations and expired worker claims.
* Detecting and cleaning up eligible orphaned physical files.
* Supporting storage-provider replacement without requiring consuming business contexts to depend on provider-specific implementations.

Business contexts remain responsible for deciding why a file is needed, which business entity references it, and which business rules govern its use.

## 3. Architectural Position

File Storage follows the modular monolith's existing dependency boundaries.

```text
Business Feature
       |
       v
Application
  IFileService
  IFileContentInspector (optional)
       |
       v
Infrastructure
  FileService
  FileContentInspector
  IFileStorageProvider implementation
  StoredFile persistence
  Cleanup and recovery
       |
       +-------------------+
       |                   |
       v                   v
   Database          Physical Storage
```

### Architectural Rules

* Business features consume `IFileService` rather than accessing physical storage directly.
* Business entities store a `FileId` or an appropriate domain value object that wraps it.
* Business contexts must not depend on physical storage keys or provider-specific APIs.
* `StoredFile` and storage persistence details remain in Infrastructure.
* `IFileStorageProvider` abstracts the physical storage implementation.
* `IFileContentInspector` provides technical inspection results; consuming contexts apply their own acceptance policies.
* File deletion is asynchronous at the physical storage level.
* Critical deletion state transitions use conditional database operations and explicit claim ownership.
* Orphan cleanup uses a grace period and does not delete objects whose creation timestamps are unknown.

These boundaries allow the capability to evolve independently of the business features that consume it.

## 4. Core Capabilities

| Capability             | Description                                                                                                      |
| ---------------------- | ---------------------------------------------------------------------------------------------------------------- |
| File upload            | Stores physical content and persists associated metadata.                                                        |
| File identification    | Exposes a stable `FileId` for business references.                                                               |
| File retrieval         | Retrieves available file metadata and opens file content.                                                        |
| Content inspection     | Supports technical inspection of file signatures and expected types where required.                              |
| Metadata consistency   | Attempts compensating deletion when metadata persistence fails after physical upload.                            |
| Asynchronous deletion  | Marks files for deletion before physical removal.                                                                |
| Claim-aware processing | Prevents stale workers from finalizing or reverting deletion records owned by newer workers.                     |
| Lease recovery         | Makes interrupted deletion work eligible for processing again after a claim expires.                             |
| Idempotent deletion    | Treats an already-missing physical object as successfully deleted.                                               |
| Orphan cleanup         | Removes eligible physical objects that have no database metadata and are older than the configured grace period. |
| Provider abstraction   | Keeps consuming contexts independent of local or future cloud storage implementations.                           |

## 5. File Lifecycle

### Upload

The upload process stores the physical object and then persists its database metadata.

If metadata persistence fails, the service attempts to delete the physical object. If compensation also fails, orphan cleanup provides a later recovery mechanism.

When a business operation uploads a file and subsequently fails to commit its own database changes, the resulting unreferenced file may be removed by orphan cleanup after the configured grace period.

### Download

Only files in the `Available` state are available through normal retrieval and download operations.

Files marked for deletion are no longer available through those operations, even if their physical objects have not yet been removed.

### Deletion

Deletion follows this state progression:

```text
Available
    |
    v
PendingDeletion
    |
    v
ProcessingDeletion
    |
    +---- Physical deletion succeeds
    |             |
    |             v
    |      Database record removed
    |
    +---- Physical deletion fails
                  |
                  v
          PendingDeletion
```

A cleanup worker must still own the current deletion claim before it can finalize or revert the database record. Expired claims can be recovered so that interrupted work can resume.

## 6. Main Abstractions

| Abstraction                  | Responsibility                                                                 |
| ---------------------------- | ------------------------------------------------------------------------------ |
| `IFileService`               | Provides the application-facing API for file operations.                       |
| `IFileContentInspector`      | Inspects file content and reports technical validation results when required.  |
| `IFileStorageProvider`       | Defines the contract for physical storage operations.                          |
| `StorageObject`              | Represents a physical storage object exposed through the provider abstraction. |
| `StoredFile`                 | Infrastructure persistence model for file metadata and deletion state.         |
| `IFileStorageCleanupService` | Coordinates cleanup and recovery operations.                                   |
| `FileStorageOptions`         | Holds the configuration for storage selection and cleanup behavior.            |

The application-facing service abstractions keep business features independent of physical storage details. Infrastructure implements the storage, persistence, and cleanup mechanisms.

## 7. Configuration and Storage Providers

The capability uses configuration to select a storage provider and control file storage behavior.

The current implementation uses local filesystem storage. The abstraction is designed to support alternative providers, such as Amazon S3 or Azure Blob Storage, without requiring consuming business contexts to change their file-reference model.

Configuration includes settings for:

* Storage provider selection.
* Maximum permitted file size.
* Allowed content types and file extensions.
* Local storage root path.
* Orphan cleanup grace period.
* Deletion claim lease duration.

Configuration is validated during application startup. An invalid provider selection or invalid required settings cause startup validation to fail rather than allowing the application to run with an unusable storage configuration.

See the technical reference for the exact configuration structure and defaults.

## 8. Security and Access Boundaries

The capability provides file operations through the application and API layers, but file storage and business authorization are distinct concerns.

The current API applies a class-level role restriction. It does not currently enforce file-specific permission policies or ownership checks.

Consequently, consuming business features must carefully control how they expose file identifiers and must enforce any additional authorization rules required by their business workflows.

A file identifier should not be treated as a substitute for authorization when a file contains sensitive information.

## 9. Reliability and Recovery

Database metadata and physical storage do not participate in a single atomic transaction. The capability therefore relies on recoverable operations rather than assuming perfect atomicity across both resources.

Its principal reliability mechanisms are:

* Compensating physical deletion after metadata persistence failure.
* Conditional database state transitions.
* Explicit deletion claim ownership.
* Lease-based recovery of expired claims.
* Idempotent physical deletion.
* Per-object failure isolation during cleanup.
* Grace-period orphan detection.
* Logging and operational monitoring of storage inconsistencies.

The system aims for eventual convergence when the database, storage provider, and cleanup workers become available and failures remain recoverable.

## 10. Documentation Guide

The File Storage documentation is divided by purpose so that requirements, implementation details, integration instructions, and recovery behavior remain independently maintainable.

| Document                                       | Purpose                                                                                                              |
| ---------------------------------------------- | -------------------------------------------------------------------------------------------------------------------- |
| [`requirements.md`](requirements.md)           | Defines the functional and non-functional requirements of the File Storage capability.                               |
| [`reference.md`](reference.md)                 | Describes the implementation, architecture, abstractions, persistence, configuration, API, and operational behavior. |
| [`integration-guide.md`](integration-guide.md) | Explains how consuming business features integrate with File Storage and manage file references.                     |
| [`edge-cases.md`](edge-cases.md)               | Documents failure modes, concurrency scenarios, recovery behavior, safety invariants, and operational monitoring.    |

### Which Document Should I Read?

* **Understanding the capability's requirements:** Start with `requirements.md`.
* **Understanding how the implementation works:** Read `reference.md`.
* **Integrating file uploads or file references into a business feature:** Read `integration-guide.md`.
* **Investigating failures, deletion races, orphan cleanup, or worker recovery:** Read `edge-cases.md`.

## 11. Design Summary

The File Storage capability centralizes physical file operations without transferring ownership of business meaning to Infrastructure.

Its design rests on four principles:

1. **Separation of concerns:** Business contexts manage file references; Infrastructure manages physical storage and persistence.
2. **Provider independence:** Application code depends on abstractions rather than a specific storage implementation.
3. **Recoverable consistency:** Compensation, retries, claim ownership, and lease recovery address failures across database and storage operations.
4. **Safe cleanup:** Idempotent deletion and conservative orphan detection prevent uncertain objects from being deleted prematurely.

The result is a shared, production-oriented technical capability that can evolve alongside the E-Commerce modular monolith while preserving its architectural boundaries.
