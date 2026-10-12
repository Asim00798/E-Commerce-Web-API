# ADR-0010: File Storage Is a Technical Capability, Not a Business Bounded Context

* **Status:** Accepted
* **Date:** 2026-10-13
* **Decision Type:** Module Classification / Architecture

## Context

The E-Commerce modular monolith provides file storage functionality for business features that need to upload, retrieve, or delete files.

Potential consumers include Catalog, People, Customer Engagement, and other features that manage business records associated with images, documents, or attachments.

File Storage provides provider-independent abstractions and implementation mechanisms for managing physical file content and its lifecycle. Its responsibilities include storage-provider integration, file operations, deletion coordination, and storage-related infrastructure concerns.

Although File Storage may contain application contracts, persistence models, services, and lifecycle states, these implementation details do not automatically make it a business bounded context.

The architectural question is whether File Storage owns an independent business domain with its own business concepts and invariants, or whether it primarily provides a reusable technical mechanism to support business features.

## Decision

**Classify File Storage as a technical capability rather than a business bounded context.**

File Storage owns the technical mechanisms for storing and managing file content. Business features retain ownership of the business meaning, authorization rules, and lifecycle of the records that reference those files.

The capability is documented under:

`docs/technical-capabilities/file-storage/`

It must not be classified as an independent business bounded context merely because it has persistence, application services, state transitions, or operational workflows.

This decision reflects the current architecture and responsibilities. It does not establish that file-related functionality can never become a business domain in a different system or under materially different requirements.

## Responsibilities

### 1. File Storage Owns Technical File Management

The File Storage capability is responsible for reusable technical functionality, including:

* Provider-independent file storage abstractions.
* Storage-provider implementations, such as local filesystem storage.
* Upload and deletion mechanisms.
* File lifecycle coordination required by the storage implementation.
* Storage metadata and technical persistence where needed.
* File size and storage-operation constraints.
* Deferred deletion and cleanup coordination.
* Provider-specific integration details.
* Storage failure handling and operational concerns.

The existing design uses an `IFileService` abstraction and an `IFileStorageProvider` abstraction to separate application-facing file operations from the underlying storage provider.

This separation allows the storage implementation to evolve without requiring every consuming business feature to depend directly on a specific provider.

### 2. Business Features Own Business Meaning

Business features own the business records that refer to stored files and determine how those references are used.

For example:

* Catalog owns the business meaning of a product image.
* People owns the business meaning of a person's profile image or personal document reference.
* Customer Engagement owns the business meaning of an attachment associated with one of its business records, where applicable.

File Storage does not determine whether a product image is valid for publication, whether a document satisfies a business requirement, or whether a particular customer may access a business record.

Those rules belong to the feature that owns the relevant business concept.

The distinction is between managing the physical file and enforcing the business rules governing the file's use.

### 3. File Metadata Does Not Automatically Imply a Business Domain

File Storage may persist metadata such as:

* File identifier.
* Storage location or provider reference.
* Content type.
* File size.
* Lifecycle status.
* Creation and deletion timestamps.
* Technical processing information.

It may also maintain lifecycle states such as `Available`, `PendingDeletion`, and `ProcessingDeletion`.

These states coordinate storage operations and protect the integrity of the storage lifecycle. They do not, by themselves, establish an independent business bounded context.

A module's classification depends on its primary responsibility, ownership of business meaning, and role in the overall domain—not simply on the presence of entities, database tables, or state transitions.

## Architectural Boundaries

### 1. Business Features Depend on File Storage Through Contracts

Business features should use the appropriate application-facing file contracts rather than depend directly on filesystem APIs or concrete storage providers.

The implementation of those contracts belongs to the outer infrastructure portion of the architecture.

The dependency direction must preserve the established Clean Architecture boundaries:

* Domain remains independent of storage providers and infrastructure frameworks.
* Application depends on appropriate abstractions when a use case requires file operations.
* Infrastructure implements storage operations and integrates with the selected provider.
* API handles HTTP-level concerns, such as accepting uploads and returning responses, without taking ownership of storage implementation details.

The precise contract placement must follow the existing project-reference rules and the responsibilities of each abstraction.

### 2. File Storage Does Not Own Consumer Business Transactions

A consuming feature remains responsible for the business transaction that creates or updates its own authoritative records.

File Storage must not become the owner of unrelated business transactions merely because a feature references a stored file.

Physical file operations may not participate in the same database transaction as business-record changes. For example, a filesystem write cannot ordinarily be rolled back by rolling back a SQL Server transaction.

Consequently, workflows that coordinate file creation or deletion with business changes must explicitly account for partial failures, cleanup, and recovery.

The architecture must not assume that a database rollback automatically reverses an external storage operation.

### 3. File Lifecycle and Business Lifecycle Are Distinct

A business record may be deleted while its physical file remains temporarily present. Conversely, a file may become unavailable even though a business record still references it.

These conditions require explicit lifecycle coordination rather than an assumption that the two lifecycles are transactionally identical.

The File Storage capability owns the technical rules for safe physical deletion and cleanup. The consuming business feature owns the business decision that makes a file eligible for removal.

For example, a business feature may determine that an attachment is no longer required. File Storage then coordinates the corresponding deletion operation according to its lifecycle and failure-handling rules.

### 4. Provider Independence

Business features should not depend directly on the current storage provider.

The capability may begin with local filesystem storage and later adopt a different provider, such as Amazon S3 or Azure Blob Storage, if concrete operational requirements justify the change.

Provider independence does not mean that every provider behaves identically. Differences in consistency, deletion semantics, permissions, availability, and failure modes must be handled by the relevant implementation.

The application-facing abstraction should expose the behavior the application actually requires without unnecessarily leaking provider-specific details.

## Consequences

### Positive Consequences

* File Storage remains reusable across multiple business features.
* Business rules remain with the modules that own the relevant concepts.
* Storage-provider changes are less likely to affect business logic.
* Technical file lifecycle concerns have a clear ownership boundary.
* The documentation structure distinguishes business requirements from reusable infrastructure behavior.
* The architecture avoids creating a business bounded context solely because a technical capability has persistence and state transitions.
* Storage-specific failure handling can evolve independently of individual business features, within their established contracts.

### Trade-Offs

* File Storage must provide sufficiently clear contracts for different consumers.
* Cross-feature use requires careful coordination of authorization and file-reference ownership.
* Technical lifecycle management can become complex when database state and physical storage operations fail independently.
* Provider-specific capabilities may not map perfectly onto a provider-independent abstraction.
* If file management later acquires substantial independent business rules, the existing classification may need to be revisited.

These trade-offs are acceptable because the current responsibility is reusable file management rather than ownership of a distinct business domain.

## Alternatives Considered

### 1. Treat File Storage as a Business Bounded Context

File Storage would be classified alongside Catalog, Ordering, Finance, People, and other business features.

**Rejected for the current architecture:** Its primary responsibility is managing physical file content and storage operations. The business meaning of the files and the rules governing their use remain with the consuming features.

The presence of persistence, metadata, and lifecycle states does not independently justify bounded-context status.

### 2. Let Each Business Feature Implement Its Own Storage Mechanism

Each feature would implement provider integration, upload and deletion operations, and technical lifecycle management independently.

**Rejected:** This would duplicate infrastructure behavior and make consistent provider changes, cleanup, and failure handling more difficult.

Business features should own their file-related business rules, not duplicate the underlying storage implementation.

### 3. Place All File Logic in Infrastructure Without a Dedicated Capability

Storage implementation would exist as miscellaneous infrastructure code without a clearly documented File Storage responsibility.

**Not adopted:** A dedicated technical capability provides an explicit ownership boundary, reusable contracts, and focused documentation without claiming ownership of a separate business domain.

## Reconsideration Criteria

Revisit this decision if File Storage acquires substantial independent business responsibilities, such as:

* A first-class document-management domain with its own business invariants and lifecycle.
* Business-level retention, classification, legal-hold, or records-management policies that require independent domain ownership.
* A product-level document service with its own workflows and externally meaningful business contracts.
* Independent business processes that cannot be appropriately owned by the existing consuming features.

Such a change would require evaluating the new business responsibilities and their ownership boundaries. Additional technical complexity alone would not automatically justify reclassification.

## Related Decisions

* [ADR-0001: Modular Monolith](0001-modular-monolith.md)
* [ADR-0002: Clean Architecture Dependency Direction](0002-clean-architecture-dependency-direction.md)
* [ADR-0003: Selective CQRS](0003-selective-cqrs.md)
* [ADR-0009: Scheduling Abstraction over Hangfire](0009-scheduling-abstraction-over-hangfire.md)

## Architectural Principle

**Business features own the meaning and rules governing their files; File Storage owns the reusable mechanisms for storing and managing file content.**

The capability remains a technical responsibility unless its business responsibilities materially change and justify a different architectural classification.
