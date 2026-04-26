using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Events
{
    public class FileUploadedEvent : DomainEvent
    {
        public Guid FileId { get; }
        public string FileName { get; }

        public FileUploadedEvent(Guid fileId, string fileName)
        {
            FileId = fileId;
            FileName = fileName;
        }
    }
}
