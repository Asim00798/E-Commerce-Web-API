using E_Commerce.Application.Shared.Files.Dtos;
using E_Commerce.Application.Shared.Files.Models;

namespace E_Commerce.Application.Shared.Files.Services;

/// <summary>
/// Application-facing capability for managing files.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Uploads a file.
    /// </summary>
    Task<Guid> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default);
    
    /// <summary>
    /// Gets a file's metadata.
    /// </summary>
    Task<FileDto?> GetAsync(Guid fileId, CancellationToken ct = default);
    
    /// <summary>
    /// Downloads a file.
    /// </summary>
    Task<FileDownloadResult?> DownloadAsync(Guid fileId, CancellationToken ct = default);

    /// <summary>
    /// Durably marks the file as PendingDeletion.
    /// Physical deletion and metadata removal happen asynchronously.
    /// </summary>
    Task DeleteAsync(Guid fileId, CancellationToken ct = default);
}