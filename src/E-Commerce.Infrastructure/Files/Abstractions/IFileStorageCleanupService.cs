
namespace E_Commerce.Infrastructure.Files.Abstractions
{
    /// <summary>
    /// Defines a service for cleaning up file storage.
    /// </summary>
    public interface IFileStorageCleanupService
    {
        /// <summary>
        /// Executes the file storage cleanup operation.
        /// </summary>
        /// <param name="ct">A cancellation token.</param>
        Task ExecuteAsync(CancellationToken ct = default);
    }
}
