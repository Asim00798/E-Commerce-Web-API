namespace E_Commerce.Infrastructure.BoundedContexts.FileManagement.Entities;

/// <summary>
/// Technical entity holding metadata about a generated file preview.
/// </summary>
public sealed class FilePreviewInfo
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public string PreviewUrl { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; set; }

    // TODO: Add additional preview metadata fields
}
