using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Services

{
    /// <summary>
    /// Pure logic service to decide the best storage provider for a file.
    /// </summary>
    public class StorageRoutingService
    {
        private const long CloudThreshold = 50 * 1024 * 1024; // 50MB

        public StorageProvider DecideStorage(FileType type, FileSize size)
        {
            // Route based on technical size threshold for cloud tiering
            if (size.Bytes > CloudThreshold)
            {
                return new StorageProvider(StorageProviderEnum.AzureBlob, "high-capacity-bucket");
            }

            // Route based on type
            if (type.Value == FileTypeEnum.Video)
            {
                return new StorageProvider(StorageProviderEnum.AzureBlob, "media-storage");
            }

            // Default to local for small, generic files
            return new StorageProvider(StorageProviderEnum.Local, "default-uploads");
        }
    }
}


