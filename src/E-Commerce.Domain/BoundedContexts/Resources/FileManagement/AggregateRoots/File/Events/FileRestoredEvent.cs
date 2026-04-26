using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Events
{
    public class FileRestoredEvent : DomainEvent
    {
        public Guid FileId { get; }

        public FileRestoredEvent(Guid fileId)
        {
            FileId = fileId;
        }
    }
}
