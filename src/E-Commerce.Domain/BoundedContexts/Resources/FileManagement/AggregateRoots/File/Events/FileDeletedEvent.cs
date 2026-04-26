using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Events
{
    public class FileDeletedEvent : DomainEvent
    {
        public Guid FileId { get; }

        public FileDeletedEvent(Guid fileId)
        {
            FileId = fileId;
        }
    }
}
