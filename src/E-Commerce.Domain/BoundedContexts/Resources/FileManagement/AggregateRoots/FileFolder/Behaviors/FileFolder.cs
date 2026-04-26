using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.Entities;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.Events;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.Behaviors
{
    public partial class FileFolder : BaseEntity, IAggregateRoot
    {
        public FolderName Name { get; private set; }
        public FolderPath Path { get; private set; }
        public Guid? ParentId { get; private set; }
        public Guid OwnerId { get; private set; }
        public FileStatusEnum Status { get; private set; }

        private readonly List<FolderAccessRule> _accessRules = new();
        public IReadOnlyCollection<FolderAccessRule> AccessRules => _accessRules.AsReadOnly();

        public FileFolder(FolderName name, FolderPath path, Guid? parentId, Guid ownerId)
        {
            Name = name;
            Path = path;
            ParentId = parentId;
            OwnerId = ownerId;
            Status = FileStatusEnum.Active;

            AddDomainEvent(new FolderCreatedEvent(Id, Name.Value));
        }
    }
}


