using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.ValueObjects;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Entities
{
    public class FileVersion : BaseEntity
    {
        public string VersionTag { get; private set; }
        public FilePath Path { get; private set; }
        public FileSize Size { get; private set; }

        public FileVersion(string versionTag, FilePath path, FileSize size)
        {
            VersionTag = versionTag;
            Path = path;
            Size = size;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
