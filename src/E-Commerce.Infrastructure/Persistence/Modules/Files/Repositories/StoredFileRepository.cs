using E_Commerce.Infrastructure.Files.Entities;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Modules.Files.Repositories;

public sealed class StoredFileRepository
{
    private readonly AppDbContext _dbContext;

    public StoredFileRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Set<StoredFile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    public async Task AddAsync(StoredFile storedFile, CancellationToken ct = default)
    {
        await _dbContext.Set<StoredFile>().AddAsync(storedFile, ct);
    }

    /// <summary>
    /// Atomically marks a file as PendingDeletion only if it is currently Available.
    /// Returns true if the update occurred.
    /// </summary>
    public async Task<bool> MarkPendingDeletionAsync(
        Guid id,
        DateTime requestedAtUtc,
        CancellationToken ct = default)
    {
        var affected = await _dbContext.Set<StoredFile>()
            .Where(f => f.Id == id && f.Status == StoredFileStatus.Available)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(f => f.Status, StoredFileStatus.PendingDeletion)
                .SetProperty(f => f.DeletionRequestedAtUtc, requestedAtUtc),
                ct);

        return affected > 0;
    }

    /// <summary>
    /// Atomically claims a batch of PendingDeletion records for a given claim ID.
    /// Only rows still in PendingDeletion are updated.
    /// Returns the rows actually claimed by this worker.
    /// </summary>
    public async Task<List<StoredFile>> ClaimPendingDeletionBatchAsync(
        int batchSize,
        Guid claimId,
        CancellationToken ct = default)
    {
        var pendingIds = await _dbContext.Set<StoredFile>()
            .Where(f => f.Status == StoredFileStatus.PendingDeletion)
            .OrderBy(f => f.DeletionRequestedAtUtc)
            .Take(batchSize)
            .Select(f => f.Id)
            .ToListAsync(ct);

        if (pendingIds.Count == 0)
            return new List<StoredFile>();

        var now = DateTime.UtcNow;
        await _dbContext.Set<StoredFile>()
            .Where(f => pendingIds.Contains(f.Id) &&
                        f.Status == StoredFileStatus.PendingDeletion)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(f => f.Status, StoredFileStatus.ProcessingDeletion)
                .SetProperty(f => f.DeletionClaimId, claimId)
                .SetProperty(f => f.DeletionClaimedAtUtc, now),
                ct);

        return await _dbContext.Set<StoredFile>()
            .Where(f => pendingIds.Contains(f.Id) &&
                        f.Status == StoredFileStatus.ProcessingDeletion &&
                        f.DeletionClaimId == claimId)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Recovers processing-deletion records whose claim has expired.
    /// </summary>
    public async Task RecoverExpiredDeletionClaimsAsync(
        TimeSpan lease,
        CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow - lease;

        await _dbContext.Set<StoredFile>()
            .Where(f => f.Status == StoredFileStatus.ProcessingDeletion &&
                        f.DeletionClaimedAtUtc != null &&
                        f.DeletionClaimedAtUtc < cutoff)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(f => f.Status, StoredFileStatus.PendingDeletion)
                .SetProperty(f => f.DeletionClaimId, (Guid?)null)
                .SetProperty(f => f.DeletionClaimedAtUtc, (DateTime?)null),
                ct);
    }

    /// <summary>
    /// Removes a StoredFile record only if the claim ID still matches and the status is ProcessingDeletion.
    /// Returns true if the row was deleted.
    /// </summary>
    public async Task<bool> RemoveIfClaimedAsync(
        Guid id,
        Guid claimId,
        CancellationToken ct = default)
    {
        var affected = await _dbContext.Set<StoredFile>()
            .Where(f => f.Id == id &&
                        f.Status == StoredFileStatus.ProcessingDeletion &&
                        f.DeletionClaimId == claimId)
            .ExecuteDeleteAsync(ct);

        return affected > 0;
    }

    /// <summary>
    /// Reverts a failed ProcessingDeletion record back to PendingDeletion only if the claim ID matches.
    /// Returns true if the row was reverted.
    /// </summary>
    public async Task<bool> RevertToPendingDeletionAsync(
        Guid id,
        Guid claimId,
        CancellationToken ct = default)
    {
        var affected = await _dbContext.Set<StoredFile>()
            .Where(f => f.Id == id &&
                        f.Status == StoredFileStatus.ProcessingDeletion &&
                        f.DeletionClaimId == claimId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(f => f.Status, StoredFileStatus.PendingDeletion)
                .SetProperty(f => f.DeletionClaimId, (Guid?)null)
                .SetProperty(f => f.DeletionClaimedAtUtc, (DateTime?)null),
                ct);

        return affected > 0;
    }
}