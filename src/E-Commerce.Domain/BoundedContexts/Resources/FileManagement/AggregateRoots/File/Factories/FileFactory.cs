using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.ValueObjects;
using FileAggregate = E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Behaviors.File;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Factories
{
    /// <summary>
    /// Pure static factory to encapsulate the complexity of File aggregate instantiation.
    /// Following the decoupled pattern: Decisions (Routing) are made outside this factory.
    /// Signaling (Events) is managed internally by the Aggregate.
    /// </summary>
    public static class FileFactory
    {
        public static FileAggregate Create(
            FileName name, 
            FilePath path, 
            FileSize size, 
            FileType type, 
            Guid ownerId, 
            StorageProvider storage,
            Guid? folderId = null)
        {
            if (storage == null) throw new ArgumentNullException(nameof(storage));

            // Factory focuses purely on structural creation
            return new FileAggregate(
                name, 
                path, 
                size, 
                type, 
                ownerId, 
                storage, 
                folderId);
        }
    }
}
