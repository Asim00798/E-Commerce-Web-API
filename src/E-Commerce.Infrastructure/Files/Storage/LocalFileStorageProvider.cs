using E_Commerce.Application.Shared.Files.Services;
using E_Commerce.Infrastructure.Files.Configuration;
using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Files.Storage;

public sealed class LocalFileStorageProvider : IFileStorageProvider
{
    private readonly LocalFileStorageOptions _options;
    private readonly ILogger<LocalFileStorageProvider> _logger;
    private readonly string _rootPath;

    public LocalFileStorageProvider(
        IOptions<LocalFileStorageOptions> options,
        ILogger<LocalFileStorageProvider> logger)
    {
        _options = options.Value;
        _logger = logger;

        var root = _options.RootPath;
        if (!Path.IsPathRooted(root))
            root = Path.Combine(AppContext.BaseDirectory, root);

        _rootPath = Path.GetFullPath(root);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task StoreAsync(
        Stream content,
        string storageKey,
        CancellationToken ct = default)
    {
        var fullPath = GetSafeFullPath(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        try
        {
            await using var fileStream = new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await content.CopyToAsync(fileStream, 81920, ct);
        }
        catch (Exception)
        {
            TryDeleteFile(fullPath);
            throw;
        }
    }

    public Task<Stream> OpenReadAsync(
        string storageKey,
        CancellationToken ct = default)
    {
        var fullPath = GetSafeFullPath(storageKey);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found.", fullPath);

        var stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        return Task.FromResult<Stream>(stream);
    }

    public Task DeleteAsync(
        string storageKey,
        CancellationToken ct = default)
    {
        var fullPath = GetSafeFullPath(storageKey);
        File.Delete(fullPath); // Idempotent for missing file, throws on real failures.
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(
        string storageKey,
        CancellationToken ct = default)
    {
        var fullPath = GetSafeFullPath(storageKey);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task<IReadOnlyCollection<StorageObject>> ListObjectsAsync(
        CancellationToken ct = default)
    {
        if (!Directory.Exists(_rootPath))
            return Task.FromResult<IReadOnlyCollection<StorageObject>>(Array.Empty<StorageObject>());

        var objects = Directory
            .EnumerateFiles(_rootPath, "*", SearchOption.AllDirectories)
            .Select(fullPath =>
            {
                var relative = Path.GetRelativePath(_rootPath, fullPath)
                                   .Replace(Path.DirectorySeparatorChar, '/');
                var creationTime = File.GetCreationTimeUtc(fullPath);
                return new StorageObject(relative, creationTime);
            })
            .ToList();

        return Task.FromResult<IReadOnlyCollection<StorageObject>>(objects);
    }

    private string GetSafeFullPath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
            throw new ArgumentException("Storage key is required.", nameof(storageKey));

        var normalizedKey = storageKey.Replace('/', Path.DirectorySeparatorChar);
        var combined = Path.Combine(_rootPath, normalizedKey);
        var fullPath = Path.GetFullPath(combined);

        var rootWithSeparator = _rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? _rootPath
            : _rootPath + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid storage key.");

        return fullPath;
    }

    private void TryDeleteFile(string fullPath)
    {
        try
        {
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete local file {Path}", fullPath);
        }
    }
}