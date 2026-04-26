using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.Events
{
    public class FolderCreatedEvent : DomainEvent
    {
        public Guid FolderId { get; }
        public string FolderName { get; }

        public FolderCreatedEvent(Guid folderId, string folderName)
        {
            FolderId = folderId;
            FolderName = folderName;
        }
    }
}
