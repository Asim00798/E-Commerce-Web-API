namespace E_Commerce.Application.Shared.Files.Services;

public sealed record StorageObject(
    string Key,
    DateTime? CreatedAtUtc);

public interface IFileStorageProvider
{
    Task StoreAsync(Stream content, string storageKey, CancellationToken ct = default);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct = default);

    /// <summary>
    /// Must be idempotent. No exception when the object does not exist.
    /// </summary>
    Task DeleteAsync(string storageKey, CancellationToken ct = default);

    Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default);

    /// <summary>
    /// Lists all storage objects with their creation timestamps.
    /// Used by orphan cleanup to apply a grace period.
    /// </summary>
    Task<IReadOnlyCollection<StorageObject>> ListObjectsAsync(CancellationToken ct = default);
}