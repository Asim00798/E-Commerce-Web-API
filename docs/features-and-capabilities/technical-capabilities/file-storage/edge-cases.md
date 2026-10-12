# File Storage — Edge Cases, Failure Modes, and Recovery Documentation

**Project:** E-Commerce Modular Monolith
**Version:** 3.0 — Updated with File Content Inspection Failure Modes
**Status:** Final

---

## 1. Purpose

The File Storage subsystem manages file metadata in the database and physical file objects through a storage provider.

The system is intentionally designed around the following realities:

* Database operations and physical storage operations involve separate resources.
* Physical file deletion may fail independently of database operations.
* Workers may crash at any point.
* Multiple cleanup workers may execute concurrently.
* A worker may lose ownership of a deletion claim while still running.
* Physical deletion must therefore be idempotent.
* Database state transitions must be atomic and claim-aware.

The system prioritizes:

1. Preventing accidental deletion of active files.
2. Preventing unsafe database finalization by a worker that has lost ownership.
3. Ensuring eventual cleanup of files marked for deletion.
4. Recovering from worker crashes.
5. Safely handling orphaned physical files.
6. Making physical deletion idempotent.
7. Supporting application-level file type inspection and validation where required.

---

## 2. File Lifecycle State Machine

A file normally follows this lifecycle:

```text
                     Upload
                        |
                        v
                  +-----------+
                  | Available |
                  +-----+-----+
                        |
                   Delete request
                        |
                        v
                +-----------------+
                | PendingDeletion |
                +--------+--------+
                         |
                    Claim worker
                         |
                         v
              +------------------------+
              |  ProcessingDeletion    |
              |  ClaimId = Worker A     |
              +-----------+------------+
                          |
                 +--------+--------+
                 |                 |
          Physical delete   Physical delete
             succeeds           fails
                 |                 |
                 v                 v
          RemoveIfClaimed    RevertIfClaimed
                 |                 |
                 v                 v
           DB row removed    PendingDeletion
```

A `ProcessingDeletion` claim may also expire:

```text
ProcessingDeletion
        |
        | Lease expires
        v
PendingDeletion
        |
        v
Another worker may claim it
```

The state machine separates the logical deletion request from physical deletion. Once a file enters `PendingDeletion`, it is no longer available through normal file retrieval and download operations.

---

## 3. Core Invariants

### Invariant 1 — Only Available Files Can Be Newly Deleted

The transition:

```text
Available -> PendingDeletion
```

is performed atomically.

A file that is already `PendingDeletion` or `ProcessingDeletion` cannot be transitioned again by another delete request.

### Invariant 2 — Only the Owning Worker Can Finalize a Deletion

A worker may remove a database record only when both conditions hold:

```text
Status == ProcessingDeletion
AND
DeletionClaimId == workerClaimId
```

This ownership check is enforced directly in SQL.

### Invariant 3 — Only the Owning Worker Can Revert a Failed Deletion

The same ownership requirement applies when returning a file from:

```text
ProcessingDeletion -> PendingDeletion
```

A stale worker cannot overwrite the state belonging to a newer worker's claim.

### Invariant 4 — Physical Deletion Is Idempotent

Calling:

```csharp
DeleteAsync(storageKey)
```

when the physical object no longer exists is considered successful.

Actual storage failures must propagate so that the cleanup workflow can retry.

### Invariant 5 — Unknown Physical Objects Are Not Immediately Deleted

An object that exists in storage but cannot be associated with database metadata is considered a potential orphan.

It may be deleted only when all the following conditions hold:

* The object is not represented by database metadata.
* Its creation timestamp is known.
* Its age exceeds the configured orphan grace period.

If the creation timestamp is unknown, the object is skipped.

### Invariant 6 — File Type Inspection Is Separate from Storage

The application may use `IFileContentInspector` to inspect file content before upload.

The inspector returns technical validation results. Business acceptance policies remain the responsibility of the consuming application context.

---

## 4. Upload Failure Modes

### 4.1 Invalid File Metadata

Examples include:

* Empty filename.
* Empty content type.
* Disallowed content type.
* Disallowed file extension.

**Handling:**

* Validation occurs before physical storage.
* The upload is rejected.
* No physical object is created.
* No database record is created.

### 4.2 File Type Inspection and Validation Failures

Before upload, the application may call:

```csharp
IFileContentInspector.InspectAsync(content, expectedType)
```

Possible outcomes include the following.

| Failure or condition                           | Expected result                                                                                                    | Application responsibility                                          |
| ---------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------- |
| Expected type not provided                     | The operation is rejected or treated as invalid by the caller                                                      | Supply a valid expected `FileType`                                  |
| Detected type is `Unknown`                     | The inspector returns `ValidationStatus.Unknown` or `Invalid`, depending on the expected type and inspection rules | Decide whether unknown content is acceptable                        |
| Detected type does not match the expected type | `ValidationStatus.Invalid`                                                                                         | Reject the upload when the mismatch violates the business policy    |
| File content is technically invalid            | `ValidationStatus.Invalid`, where the inspector can establish invalidity                                           | Reject the upload                                                   |
| Stream is non-seekable                         | `ValidationStatus.Unknown`                                                                                         | Reject the upload or apply an explicitly defined fallback policy    |
| Inspector throws an exception                  | The exception propagates                                                                                           | Decide whether the failure is retryable or should reject the upload |
| Inspection reads only a prefix                 | The result reflects the inspection performed, not necessarily complete file validation                             | Understand the limits of signature-based inspection                 |

For example, if the application expects an image but the inspector identifies a document, the application should reject the upload.

**Important limitation:** The inspector may read only a small prefix of the stream. This is sufficient for magic-byte detection but not for full structural validation. A `Valid` result means that the inspected signature was recognized and no expected-type mismatch was detected; it does not necessarily mean that the entire file was parsed and validated.

If a stream read fails during inspection, the exception propagates. The application must distinguish transient infrastructure failures from invalid input rather than automatically treating every exception as a validation failure.

### 4.3 Empty File

The API rejects an empty upload:

```csharp
file.Length == 0
```

The response is:

```text
400 Bad Request
```

This prevents empty uploads from entering the storage lifecycle.

### 4.4 File Larger Than the Maximum Size

The application wraps the incoming stream in `SizeLimitedReadStream`.

If:

```text
TotalBytesRead > MaxFileSizeBytes
```

an exception is raised.

The local provider removes the partially created physical file when `StoreAsync` fails.

```text
Oversized upload
       |
       v
Stream throws
       |
       v
Provider catches failure
       |
       v
Partial physical file removed
       |
       v
Upload fails
```

No database record is created for the failed upload.

### 4.5 Storage Provider Fails During Upload

Examples include:

* Disk unavailable.
* Permission denied.
* Directory creation failure.
* I/O failure.
* Provider-specific network failure.

**Handling:**

* `StoreAsync` propagates the exception.
* Database metadata is not persisted.
* The local provider attempts to remove any partially created file.
* If compensation fails, the remaining object becomes an orphan candidate for later cleanup.

### 4.6 Metadata Persistence Fails After Physical Upload

The sequence is:

```text
Physical file created
        |
        v
Database INSERT fails
```

The database has no metadata record, but the physical object exists.

**Handling:**

* `FileService` attempts a compensating physical delete.
* If compensation succeeds, neither the physical object nor the database record remains.
* If compensation fails, orphan cleanup can eventually remove the object, subject to the configured grace period and orphan-detection rules.

### 4.7 Database Save Succeeds but the Request Fails Afterwards

If the database insert succeeds but the HTTP response fails, the file remains valid.

The client may retry or discover the file through the returned identifier if that identifier was received or can be retrieved through an appropriate application workflow.

The storage subsystem does not roll back a successfully committed upload merely because the client did not receive the response.

This situation requires care at the API or application level: retrying an upload may create another file unless the operation has an appropriate idempotency mechanism.

### 4.8 Duplicate Storage Key

Storage keys are generated using the file ID:

```text
yyyy/MM/dd/{fileId}.{extension}
```

Because `fileId` is a newly generated GUID, collisions are extremely unlikely.

The local provider uses:

```csharp
FileMode.CreateNew
```

An unexpected collision therefore fails rather than overwriting an existing file.

### 4.9 Path Traversal and Unsafe Storage Keys

The local provider:

1. Normalizes path separators.
2. Combines the storage key with the configured storage root.
3. Resolves the resulting full path.
4. Verifies that the resolved path remains under the storage root.

Invalid keys cause:

```csharp
throw new InvalidOperationException("Invalid storage key.");
```

This prevents a storage key from being used to address arbitrary paths outside the configured storage directory.

---

## 5. Download Failure Modes

### 5.1 Download of a Nonexistent Database Record

`GetAsync` or `DownloadAsync` returns `null` when no corresponding available file record exists.

The API returns:

```text
404 Not Found
```

### 5.2 Download During Deletion

A file in either of these states is not considered available:

* `PendingDeletion`
* `ProcessingDeletion`

Marking a file for deletion therefore immediately removes it from normal download visibility, even though physical deletion happens asynchronously.

### 5.3 Database Says the File Exists but the Physical Object Is Missing

Possible causes include:

* Manual deletion outside the application.
* Disk corruption.
* Storage provider failure.
* Other external storage inconsistencies.

The current behavior is for `OpenReadAsync` to fail. For the local provider, this results in a `FileNotFoundException`.

This is a genuine storage inconsistency. The system does not silently remove the database metadata.

**Operational response:** Log and monitor the failure, then investigate the mismatch between database metadata and physical storage.

---

## 6. Deletion Failure Modes

### 6.1 Delete Request for a Missing File

The API calls `GetAsync(fileId)`.

If no available file exists, the API returns:

```text
404 Not Found
```

No new deletion state is created.

### 6.2 Two Clients Delete the Same File Concurrently

Both clients attempt the atomic transition:

```text
Available -> PendingDeletion
```

Only one conditional update succeeds.

The other update affects zero rows, so only one deletion transition is created.

### 6.3 Two Cleanup Workers Select the Same Pending File

Both workers may initially observe the file in `PendingDeletion`.

Each uses a unique `ClaimId`. The claim update is conditional on:

```text
Status == PendingDeletion
```

Only one worker successfully acquires ownership.

The subsequent query also requires:

```text
DeletionClaimId == workerClaimId
```

A worker that does not own the claim cannot process the file as its own.

### 6.4 Worker Crashes After Claiming

The row remains in `ProcessingDeletion` with the worker's claim ID.

After the lease expires, `RecoverExpiredDeletionClaimsAsync` returns the row to `PendingDeletion`.

Another worker can then claim it.

### 6.5 Worker Takes Longer Than the Lease

The lease expires, and another worker claims the row.

The original worker's call:

```csharp
RemoveIfClaimedAsync(id, originalClaimId)
```

fails because the current claim no longer matches.

The original worker therefore cannot finalize the database deletion using a stale claim.

### 6.6 Worker Loses Its Claim After Physical Deletion

The sequence is:

1. Worker A physically deletes the object.
2. Worker A's claim expires.
3. Worker B claims the database row.
4. Worker A attempts `RemoveIfClaimedAsync`.
5. Worker A's finalization fails because its claim is no longer current.
6. Worker B attempts physical deletion again.
7. Because physical deletion is idempotent, the missing object is treated as success.
8. Worker B finalizes the deletion using its own claim.

The physical object is removed, and the database record is finalized only by the current claim owner.

### 6.7 Physical Deletion Fails

The provider throws an exception.

The cleanup job catches the failure and attempts:

```csharp
RevertToPendingDeletionAsync(id, claimId)
```

If the worker still owns the claim, the row returns to `PendingDeletion` for retry.

### 6.8 Physical Deletion Fails After the Claim Is Lost

Worker A's deletion attempt fails. Its claim expires, and Worker B acquires a new claim.

Worker A attempts:

```csharp
RevertIfClaimedAsync(id, claimId)
```

The conditional operation fails because Worker A no longer owns the claim.

Worker B remains responsible for processing the file.

### 6.9 Physical File Is Already Missing

The provider treats the missing file as successful deletion.

The cleanup job proceeds to `RemoveIfClaimedAsync` and removes the database record if the worker still owns the claim.

The desired final state is achieved.

### 6.10 Physical Deletion Succeeds but Database Removal Fails

`RemoveIfClaimedAsync` performs a conditional database operation.

If the database becomes unavailable after physical deletion, the row remains in `ProcessingDeletion`.

After the claim expires:

1. Another worker reclaims the row.
2. It attempts physical deletion again.
3. The missing object is treated as successful deletion.
4. The worker removes the database row if it still owns the claim.

The workflow converges through retry and lease recovery.

### 6.11 Cleanup Worker Crashes After Physical Deletion

Physical deletion succeeds, but the worker crashes before database finalization.

The row remains in `ProcessingDeletion`.

After the lease expires, another worker reclaims the row, repeats the idempotent physical deletion, and removes the database record.

### 6.12 Cleanup Worker Crashes Before Physical Deletion

The worker acquires a claim and then crashes before deleting the physical object.

The physical object remains.

After the lease expires, another worker claims the row and processes the deletion normally.

### 6.13 Cleanup Worker Crashes During Reversion

The row may remain in `ProcessingDeletion` if the worker crashes while attempting to revert a failed deletion.

After the lease expires, claim recovery returns it to `PendingDeletion`, allowing a later retry.

---

## 7. Orphan Cleanup Failure Modes

### 7.1 Orphan Object — Database Record Does Not Exist

A physical storage key has no corresponding database metadata.

It becomes a candidate orphan, but it is not deleted immediately.

### 7.2 Orphan Object Inside the Grace Period

If:

```text
ObjectAge < OrphanGracePeriod
```

the object is skipped.

This protects files that may have been uploaded successfully but whose metadata transaction or higher-level business operation has not yet completed.

### 7.3 Orphan Object Outside the Grace Period

If:

```text
ObjectAge >= OrphanGracePeriod
```

and the object has no corresponding database metadata, the provider attempts deletion.

If deletion fails, the failure is logged and the object remains for a future cleanup attempt.

### 7.4 Orphan Has an Unknown Creation Timestamp

If:

```text
CreatedAtUtc == null
```

the system does not delete the object.

The reason is that the system cannot establish whether the object is stale.

### 7.5 Orphan Listing Fails

If `ListObjectsAsync()` fails:

* Orphan cleanup stops for that execution.
* No orphan deletion is performed during that execution.
* The failure is logged.
* The next scheduled execution can retry.

### 7.6 One Orphan Deletion Fails

Per-object processing isolates failures.

A failed object remains in storage, while other eligible objects can continue to be processed.

### 7.7 Database Query Fails During Orphan Cleanup

The query for known storage keys fails.

Cleanup fails safely rather than treating every physical object as an orphan.

This is a critical safety measure: the system must not delete physical objects when it cannot reliably determine which objects have database metadata.

### 7.8 Storage Listing Returns an Empty Collection

If storage returns no objects, cleanup does nothing.

No database records are deleted by the orphan scan.

---

## 8. Provider Abstraction Failure

The Application layer depends on `IFileStorageProvider`, not on concrete storage providers.

Provider-specific failures are isolated behind the provider abstraction.

The provider's `DeleteAsync` implementation must be idempotent: deleting an object that is already absent is considered success, while genuine storage failures must propagate.

---

## 9. Cancellation Modes

### 9.1 Cancellation During Upload

The `CancellationToken` is passed through the call chain:

```text
Controller
    -> FileService
        -> StorageProvider
            -> CopyToAsync
```

The local provider attempts to remove the partial file when an upload fails, including cancellation.

Database metadata is not committed as a successful upload when the operation is cancelled before metadata persistence.

If compensation itself fails, the remaining object may require orphan cleanup.

### 9.2 Cancellation During Cleanup

A `CancellationToken` can stop the current cleanup operation.

The database state remains recoverable. Any `ProcessingDeletion` claim that is left behind can be recovered after its lease expires.

Cancellation does not guarantee immediate rollback of an operation already in progress. Recovery depends on the durable state and the cleanup workflow.

### 9.3 Cancellation During Download

The `CancellationToken` is passed to the provider's open operation.

The returned stream is handled by ASP.NET Core, which controls the HTTP response streaming lifecycle.

The implementation must ensure that the stream is disposed appropriately if the response is cancelled or terminates early.

---

## 10. Consistency Model

The architecture explicitly accepts temporary inconsistencies between the database and physical storage.

It uses:

* Compensation.
* Retry.
* Idempotency.
* Lease recovery.
* Orphan cleanup.

It does not pretend that the database and storage provider participate in a single atomic transaction.

### Upload Consistency

```text
Physical storage
       |
       v
Database metadata
       |
       +-- If metadata persistence fails:
              attempt compensating physical deletion
```

Physical storage is written first. If database metadata persistence fails, the service attempts to remove the physical object.

### Deletion Consistency

```text
Database state transition
       |
       v
Physical deletion
       |
       v
Claim-conditional database finalization
```

If a step fails, retry and lease recovery allow the workflow to continue safely.

---

## 11. API Delete Is Asynchronous

The DELETE endpoint returns:

```text
202 Accepted
```

This means the deletion request has been accepted and the file has been marked `PendingDeletion`. Background cleanup will perform physical deletion.

### Logical Immediacy

Although physical deletion is asynchronous, availability changes immediately:

```text
Available -> PendingDeletion
```

After this transition, `GetAsync` and `DownloadAsync` return `null` for normal availability lookups.

---

## 12. Duplicate Delete After Pending State

A second DELETE request that encounters a file already in `PendingDeletion` does not create another deletion transition.

Deletion is idempotent at the database state-transition level.

The API may return `404 Not Found` when it looks up only available files, depending on the controller's current implementation. This response does not mean that the original deletion request was lost; the file may already be awaiting background cleanup.

---

## 13. EF Core Tracking Is Not Used for State Transitions

The deletion workflow uses:

* `ExecuteUpdateAsync`
* `ExecuteDeleteAsync`

with conditional predicates rather than relying on tracked entities for critical state transitions.

This prevents stale in-memory entities from becoming the source of truth for deletion ownership.

The database predicates determine whether a state transition is still valid at the time the operation executes.

---

## 14. Why RowVersion Does Not Drive the Workflow

`RowVersion` exists as an EF Core concurrency field, but critical deletion operations use explicit conditional predicates.

The ownership rule is based on:

```text
Status + DeletionClaimId
```

`RowVersion` is not the mechanism that determines whether a cleanup worker owns a deletion claim.

---

## 15. Cleanup Batch Failure Isolation

Each claimed file is processed independently.

A failure while processing one file does not need to prevent the worker from processing other claimed files.

When physical deletion fails, the worker attempts to revert the file to `PendingDeletion`, provided it still owns the claim.

If it has lost ownership, the conditional revert fails and the current owner remains responsible.

---

## 16. Stale Worker Protection

A worker may continue running after its lease expires and another worker acquires the claim.

The claim ID prevents the stale worker from modifying the database state owned by the newer claim.

For example:

```text
Worker A claims file
       |
       v
Worker A's lease expires
       |
       v
Worker B acquires a new claim
       |
       v
Worker A attempts finalization
       |
       v
Conditional operation fails
```

This protects database finalization and reversion from stale workers.

**Important distinction:** Claim-aware database operations protect database ownership. They do not, by themselves, prevent a stale worker from performing a physical storage operation after its lease expires. Safety therefore also relies on the deletion lifecycle: a file already in `ProcessingDeletion` is no longer an active, downloadable file, and physical deletion must be idempotent.

---

## 17. Eventual Consistency Guarantee

The system aims to guarantee the following deletion progression:

```text
DELETE request
      |
      v
Durable PendingDeletion state
      |
      v
Background cleanup
      |
      v
Physical object removed
      |
      v
Database metadata removed
```

Convergence depends on the following conditions:

* Cleanup continues to run.
* The storage provider eventually becomes available.
* The database eventually becomes available.
* Failures remain retryable or can be operationally resolved.
* Claim recovery continues to operate.

The guarantee is eventual, not instantaneous. Persistent failures, exhausted retries, disabled cleanup, or unrecoverable storage errors may require operational intervention.

---

## 18. Failure Matrix

| Failure                                          | Result                                             | Recovery                                                       |
| ------------------------------------------------ | -------------------------------------------------- | -------------------------------------------------------------- |
| Invalid metadata                                 | Upload rejected                                    | None required; client corrects request                         |
| Empty file                                       | `400 Bad Request`                                  | Client sends a nonempty file                                   |
| File too large                                   | Upload fails                                       | Partial physical file is removed by the provider when possible |
| Storage write fails                              | Upload fails                                       | Provider cleanup and later orphan cleanup handle leftovers     |
| Database metadata insert fails                   | Compensation delete is attempted                   | Orphan cleanup is the fallback                                 |
| Compensation delete fails                        | Physical orphan may remain                         | Grace-period orphan cleanup                                    |
| Duplicate delete                                 | Only the first valid state transition succeeds     | No duplicate deletion transition                               |
| Worker crashes after claim                       | Claim remains until recovery                       | Lease recovery                                                 |
| Worker crashes after physical delete             | Database row remains                               | Reclaim and idempotent physical deletion                       |
| Physical object already missing                  | Deletion is considered successful                  | Claim-conditional database finalization                        |
| Physical deletion fails                          | Reversion is attempted if the claim is still owned | Retry                                                          |
| Worker loses claim                               | Conditional update fails                           | Current owner continues                                        |
| Orphan has unknown age                           | Object is not deleted                              | Operational or manual handling                                 |
| Orphan is inside grace period                    | Object is not deleted                              | Future cleanup                                                 |
| Orphan is outside grace period                   | Eligible object is deleted                         | None if successful                                             |
| Storage listing fails                            | Orphan cleanup is skipped                          | Next scheduled execution                                       |
| Database lookup fails during orphan scan         | Cleanup fails safely                               | Next execution after the database recovers                     |
| Download while deleting                          | File is not available                              | No recovery needed                                             |
| Database row exists but physical file is missing | Download fails                                     | Operational investigation                                      |
| Cleanup cancellation                             | Current work stops                                 | Durable state and lease recovery allow retry                   |
| Storage provider outage                          | Storage operations fail                            | Retry and operational recovery                                 |
| Expected file type does not match                | Upload is rejected by the application              | None required                                                  |
| Inspector returns `Unknown`                      | Application policy determines acceptance           | Reject or use an explicitly defined fallback                   |
| Non-seekable stream during inspection            | Validation result is `Unknown`                     | Application may reject or apply a defined fallback             |
| Inspector throws an exception                    | Exception propagates                               | Application decides whether to retry or reject                 |

---

## 19. Failure Classification

### Category A — Request Validation Failures

Examples:

* Invalid filename.
* Invalid or disallowed content type.
* Empty file.
* File too large.
* File type mismatch.
* Invalid inspected content.

These failures should normally be rejected without entering the storage lifecycle.

### Category B — Transient Infrastructure Failures

Examples:

* Database unavailable.
* Filesystem or cloud storage unavailable.
* Temporary I/O failure.

These failures may be recoverable through retry, compensation, or operational recovery.

### Category C — Concurrency Failures

Examples:

* Two clients deleting the same file.
* Multiple cleanup workers competing for a claim.
* Expired leases.
* Stale workers.
* Claim ownership changes.

Conditional database operations ensure that only valid state transitions succeed.

### Category D — Distributed Consistency Failures

Examples:

* Physical file exists without metadata.
* Metadata exists but the physical file is missing.
* Physical deletion succeeds but database removal fails.
* Database save fails after physical upload.

These failures are addressed through compensation, idempotency, lease recovery, retries, and orphan cleanup.

### Category E — File Content Inspection Failures

Examples:

* Unknown detected type.
* Expected and detected types do not match.
* Invalid or truncated content.
* Non-seekable stream.
* Stream read failure.
* Inspector exception.

The inspector provides technical findings. The consuming application decides the business acceptance policy and handles operational failures.

---

## 20. Operational Recovery Model

The cleanup job should run periodically.

Each execution follows this sequence:

1. Recover expired deletion claims.
2. Claim a batch of `PendingDeletion` records.
3. Delete the corresponding physical objects.
4. Remove database rows when the worker still owns the claims.
5. Revert failed deletions to `PendingDeletion` when the worker still owns the claims.
6. Scan physical storage for orphan candidates.
7. Delete only objects proven to be stale under the configured orphan-cleanup rules.

This design makes cleanup self-healing under recoverable failures.

It does not eliminate the need for monitoring. Persistent failures, unknown-age objects, and database/storage inconsistencies may require operational intervention.

---

## 21. Important Monitoring Signals

Production monitoring should track:

* Upload failures.
* Metadata persistence failures.
* Compensation delete failures.
* Physical deletion failures.
* Failed claim recovery.
* Failed database finalization.
* Claims that repeatedly expire.
* High numbers of expired claims.
* Orphan cleanup failures.
* Large or growing orphan counts.
* Objects repeatedly skipped because their creation time is unknown.
* `StoredFile` records in `Available` state whose physical objects are missing.
* Repeated file content inspection failures.
* Repeated storage provider failures or cancellation-related cleanup failures.

### Critical Alert

**An `Available` file whose physical object is missing is not a normal deletion lifecycle outcome.**

It indicates a storage inconsistency that should be investigated.

A growing number of expired claims can also indicate workers crashing, leases that are too short, provider latency, or database availability problems.

---

## 22. Safety Properties

The system is designed to prevent the following unsafe outcomes:

* An active database file being accidentally deleted by a stale cleanup worker.
* An unknown-age physical object being deleted immediately.
* A stale worker modifying a newer owner's database claim.
* An orphan scan deleting files when the database lookup needed to identify known files has failed.

All critical deletion finalization and reversion operations are claim-conditional.

The database claim is the authority for database state transitions. Physical deletion remains an external side effect and must be idempotent so that retries and competing workers can converge safely.

---

## 23. Final Design Principle

The system does not attempt to make database state and physical storage perfectly atomic. Instead, it makes state transitions recoverable and physical operations safe to retry.

The fundamental strategy is:

```text
Atomic database state transitions
              +
Explicit worker ownership
              +
Lease-based recovery
              +
Idempotent physical operations
              +
Compensation
              +
Grace-period orphan cleanup
              +
Eventual retry
```

This combination allows the system to recover from:

* Concurrent requests.
* Worker crashes.
* Storage provider failures.
* Database failures.
* Network failures.
* Partial uploads.
* Partial deletions.
* Stale workers.
* Expired claims.
* Orphaned physical objects.
* File inspection failures.

The central safety invariant is:

> A worker may modify a `ProcessingDeletion` record only while it still owns that record's current deletion claim.

The desired final state for deletion is always:

```text
Database metadata: removed
Physical object:   removed
```

The system may temporarily remain in an intermediate state, but its recovery mechanisms are designed to converge toward the desired state without allowing stale workers or uncertain orphan detection to cause unsafe deletion.

---

**Document Version:** 3.0 — Final
