namespace E_Commerce.Infrastructure.BoundedContexts.FileManagement.Entities;

/// <summary>
/// Technical entity representing an enqueued file processing job.
/// </summary>
public sealed class FileProcessingJob
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public string JobType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    // TODO: Add retry count, error message fields
}
