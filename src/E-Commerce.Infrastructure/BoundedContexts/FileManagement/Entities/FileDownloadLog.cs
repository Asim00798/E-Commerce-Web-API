namespace E_Commerce.Infrastructure.BoundedContexts.FileManagement.Entities;

/// <summary>
/// Technical entity that records each file download event.
/// </summary>
public sealed class FileDownloadLog
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public string DownloadedBy { get; set; } = string.Empty;
    public DateTimeOffset DownloadedAt { get; set; }

    // TODO: Add additional tracking fields
}
