using E_Commerce.Application.Shared.Files.Services;
using E_Commerce.Infrastructure.Files.Abstractions;
using E_Commerce.Infrastructure.Files.Configuration;
using E_Commerce.Infrastructure.Files.Entities;
using E_Commerce.Infrastructure.Persistence.Context;
using E_Commerce.Infrastructure.Persistence.Modules.Files.Repositories;
using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Files.Services;

public sealed class FileStorageCleanupService : IFileStorageCleanupService
{
    private readonly IFileStorageProvider _storageProvider;
    private readonly StoredFileRepository _storedFileRepository;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<FileStorageCleanupService> _logger;
    private readonly IOptions<FileStorageOptions> _options;

    public FileStorageCleanupService(
        IFileStorageProvider storageProvider,
        StoredFileRepository storedFileRepository,
        AppDbContext dbContext,
        ILogger<FileStorageCleanupService> logger,
        IOptions<FileStorageOptions> options)
    {
        _storageProvider = storageProvider;
        _storedFileRepository = storedFileRepository;
        _dbContext = dbContext;
        _logger = logger;
        _options = options;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        // Recover expired claims before processing
        await _storedFileRepository.RecoverExpiredDeletionClaimsAsync(
            _options.Value.DeletionClaimLease, ct);

        await ProcessPendingDeletionsAsync(ct);
        await CleanupOrphanedFilesAsync(ct);
    }

    private async Task ProcessPendingDeletionsAsync(CancellationToken ct)
    {
        var claimId = Guid.NewGuid();
        var batch = await _storedFileRepository.ClaimPendingDeletionBatchAsync(
            100, claimId, ct);

        foreach (var file in batch)
        {
            try
            {
                // Idempotent physical deletion. Failures throw and cause retry.
                await _storageProvider.DeleteAsync(file.StorageKey, ct);

                // Remove DB record only if we still own the claim.
                var removed = await _storedFileRepository.RemoveIfClaimedAsync(
                    file.Id,
                    claimId,
                    ct);

                if (removed)
                {
                    _logger.LogInformation(
                        "Processed deletion for file {FileId} (claim {ClaimId})",
                        file.Id,
                        claimId);
                }
                else
                {
                    _logger.LogWarning(
                        "File {FileId} was physically deleted but the claim was lost; another worker may have processed it.",
                        file.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to process deletion for file {FileId}. Reverting to PendingDeletion if still owner of claim {ClaimId}.",
                    file.Id,
                    claimId);

                var reverted = await _storedFileRepository.RevertToPendingDeletionAsync(
                    file.Id,
                    claimId,
                    ct);

                if (!reverted)
                {
                    _logger.LogWarning(
                        "Could not revert file {FileId} because claim {ClaimId} was lost; another worker likely reclaimed it.",
                        file.Id,
                        claimId);
                }
            }
        }
    }

    private async Task CleanupOrphanedFilesAsync(CancellationToken ct)
    {
        IReadOnlyCollection<StorageObject> physicalObjects;

        try
        {
            physicalObjects = await _storageProvider.ListObjectsAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Orphan cleanup failed because provider objects could not be listed.");
            return;
        }

        if (physicalObjects.Count == 0)
            return;

        var knownKeys = await _dbContext.Set<StoredFile>()
            .AsNoTracking()
            .Select(f => f.StorageKey)
            .ToListAsync(ct);

        var knownSet = knownKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var gracePeriod = _options.Value.OrphanGracePeriod;
        var cutoff = DateTime.UtcNow - gracePeriod;

        foreach (var obj in physicalObjects)
        {
            if (knownSet.Contains(obj.Key))
                continue;

            // Unknown creation time: be conservative and skip deletion.
            if (!obj.CreatedAtUtc.HasValue)
            {
                _logger.LogWarning(
                    "Skipping orphan candidate {Key} because its creation timestamp is unavailable.",
                    obj.Key);
                continue;
            }

            if (obj.CreatedAtUtc.Value > cutoff)
            {
                _logger.LogDebug(
                    "Skipping potential orphan {Key} because it is within grace period.",
                    obj.Key);
                continue;
            }

            try
            {
                await _storageProvider.DeleteAsync(obj.Key, ct);
                _logger.LogInformation("Deleted orphan file with storage key {StorageKey}", obj.Key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to delete orphan file with storage key {StorageKey}",
                    obj.Key);
            }
        }
    }
}