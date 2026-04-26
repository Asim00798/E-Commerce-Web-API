using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Entities;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Events;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Behaviors
{
    public partial class File : BaseEntity, IAggregateRoot
    {
        public FileName Name { get; private set; }
        public FilePath Path { get; private set; }
        public FileSize Size { get; private set; }
        public FileType Type { get; private set; }
        public FileStatusEnum Status { get; private set; }
        public FileVisibilityEnum Visibility { get; private set; }
        public Guid OwnerId { get; private set; }
        public Guid? FolderId { get; private set; }
        public StorageProvider Storage { get; private set; }

        private readonly List<FileVersion> _versions = new();
        private readonly List<FileTag> _tags = new();
        private readonly List<FileAccessRule> _accessRules = new();
        private readonly List<FileMetadata> _metadata = new();

        public IReadOnlyCollection<FileVersion> Versions => _versions.AsReadOnly();
        public IReadOnlyCollection<FileTag> Tags => _tags.AsReadOnly();
        public IReadOnlyCollection<FileAccessRule> AccessRules => _accessRules.AsReadOnly();
        public IReadOnlyCollection<FileMetadata> Metadata => _metadata.AsReadOnly();

        public File(FileName name, FilePath path, FileSize size, FileType type, Guid ownerId, StorageProvider storage, Guid? folderId = null)
        {
            Name = name;
            Path = path;
            Size = size;
            Type = type;
            OwnerId = ownerId;
            Storage = storage ?? throw new ArgumentNullException(nameof(storage));
            FolderId = folderId;
            Status = FileStatusEnum.Active;
            Visibility = FileVisibilityEnum.Private;

            AddDomainEvent(new FileUploadedEvent(Id, Name.Name));
        }

    }
}


