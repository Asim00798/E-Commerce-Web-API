namespace E_Commerce.Infrastructure.Files.Configuration;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string Provider { get; set; } = "Local";
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
    public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
    public LocalFileStorageOptions Local { get; set; } = new();

    /// <summary>
    /// Grace period for orphan cleanup. Objects younger than this are not deleted.
    /// </summary>
    public TimeSpan OrphanGracePeriod { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// How long a deletion claim remains valid before it can be reclaimed.
    /// </summary>
    public TimeSpan DeletionClaimLease { get; set; } = TimeSpan.FromMinutes(10);
}