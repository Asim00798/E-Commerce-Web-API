using E_Commerce.Application.Shared.Files.Dtos;
using E_Commerce.Application.Shared.Files.Models;
using E_Commerce.Application.Shared.Files.Services;
using E_Commerce.Infrastructure.Files.Configuration;
using E_Commerce.Infrastructure.Files.Entities;
using E_Commerce.Infrastructure.Persistence.Context;
using E_Commerce.Infrastructure.Persistence.Modules.Files.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Files.Services;

public sealed class FileService : IFileService
{
    private readonly IFileStorageProvider _storageProvider;
    private readonly StoredFileRepository _storedFileRepository;
    private readonly AppDbContext _dbContext;
    private readonly IOptions<FileStorageOptions> _options;
    private readonly ILogger<FileService> _logger;

    public FileService(
        IFileStorageProvider storageProvider,
        StoredFileRepository storedFileRepository,
        AppDbContext dbContext,
        IOptions<FileStorageOptions> options,
        ILogger<FileService> logger)
    {
        _storageProvider = storageProvider;
        _storedFileRepository = storedFileRepository;
        _dbContext = dbContext;
        _options = options;
        _logger = logger;
    }

    public async Task<Guid> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type is required.", nameof(contentType));

        var options = _options.Value;
        ValidateFileMetadata(fileName, contentType, options);

        var fileId = Guid.NewGuid();
        var storageKey = GenerateStorageKey(fileId, fileName);

        await using var limitedStream = new SizeLimitedReadStream(content, options.MaxFileSizeBytes);

        try
        {
            await _storageProvider.StoreAsync(limitedStream, storageKey, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Physical storage failed for file {FileId}", fileId);
            throw;
        }

        var storedFile = new StoredFile
        {
            Id = fileId,
            FileName = fileName,
            ContentType = contentType,
            Size = limitedStream.TotalBytesRead,
            StorageKey = storageKey,
            CreatedAtUtc = DateTime.UtcNow,
            Status = StoredFileStatus.Available
        };

        try
        {
            await _storedFileRepository.AddAsync(storedFile, ct);
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Metadata persistence failed for file {FileId}. Attempting compensation delete.",
                fileId);

            try
            {
                await _storageProvider.DeleteAsync(storageKey, CancellationToken.None);
            }
            catch (Exception cleanupEx)
            {
                _logger.LogError(cleanupEx,
                    "Compensation delete failed for {FileId}. Orphan cleanup will handle it.",
                    fileId);
            }

            throw;
        }

        _logger.LogInformation("File uploaded. FileId: {FileId}, Size: {Size}",
            fileId, storedFile.Size);

        return fileId;
    }

    public async Task<FileDto?> GetAsync(Guid fileId, CancellationToken ct = default)
    {
        var storedFile = await _storedFileRepository.GetByIdAsync(fileId, ct);
        if (storedFile is null || storedFile.Status != StoredFileStatus.Available)
            return null;

        return MapToDto(storedFile);
    }

    public async Task<FileDownloadResult?> DownloadAsync(Guid fileId, CancellationToken ct = default)
    {
        var storedFile = await _storedFileRepository.GetByIdAsync(fileId, ct);
        if (storedFile is null || storedFile.Status != StoredFileStatus.Available)
            return null;

        var stream = await _storageProvider.OpenReadAsync(storedFile.StorageKey, ct);
        return new FileDownloadResult(MapToDto(storedFile), stream);
    }

    public async Task DeleteAsync(Guid fileId, CancellationToken ct = default)
    {
        var marked = await _storedFileRepository.MarkPendingDeletionAsync(
            fileId,
            DateTime.UtcNow,
            ct);

        if (!marked)
            return;

        _logger.LogInformation("File marked for deletion. FileId: {FileId}", fileId);
    }

    private static void ValidateFileMetadata(
        string fileName,
        string contentType,
        FileStorageOptions options)
    {
        if (options.AllowedContentTypes.Length > 0 &&
            !options.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Content type '{contentType}' is not allowed.");
        }

        if (options.AllowedExtensions.Length > 0)
        {
            var extension = Path.GetExtension(fileName);
            if (!options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"File extension '{extension}' is not allowed.");
            }
        }
    }

    private static string GenerateStorageKey(Guid fileId, string fileName)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".bin";

        var datePath = DateTime.UtcNow.ToString("yyyy/MM/dd");
        return $"{datePath}/{fileId}{extension.ToLowerInvariant()}";
    }

    private static FileDto MapToDto(StoredFile storedFile)
    {
        return new FileDto(
            storedFile.Id,
            storedFile.FileName,
            storedFile.ContentType,
            storedFile.Size,
            storedFile.CreatedAtUtc);
    }
}