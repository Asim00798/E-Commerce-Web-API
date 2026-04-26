using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.ValueObjects
{
    public sealed record StorageProvider
    {
        public StorageProviderEnum Provider { get; init; }
        public string BucketName { get; init; }

        public StorageProvider(StorageProviderEnum provider, string bucketName)
        {
            Provider = provider;
            BucketName = bucketName;
        }
    }
}
