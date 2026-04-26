namespace E_Commerce.Infrastructure.BoundedContexts.FileManagement.Entities;

/// <summary>
/// Technical entity representing an active or completed file upload session.
/// </summary>
public sealed class FileUploadSession
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    // TODO: Add uploader identity and storage path
}
