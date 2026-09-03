namespace E_Commerce.Infrastructure.Files.Entities;

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
    public StoredFileStatus Status { get; set; } = StoredFileStatus.Available;
    public DateTime? DeletionRequestedAtUtc { get; set; }

    // Concurrency and claim fields
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Unique identifier for a cleanup worker's claim on this record.
    /// </summary>
    public Guid? DeletionClaimId { get; set; }

    /// <summary>
    /// Timestamp when the deletion claim was acquired.
    /// </summary>
    public DateTime? DeletionClaimedAtUtc { get; set; }
}