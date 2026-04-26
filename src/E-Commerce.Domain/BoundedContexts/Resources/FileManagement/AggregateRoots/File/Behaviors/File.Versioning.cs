using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Entities;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Behaviors
{
    public partial class File
    {
        public void CreateVersion(string versionTag, FilePath path, FileSize size)
        {
            if (Status == FileStatusEnum.Deleted)
                throw new FileManagementDomainException("Cannot add a version to a deleted file.");

            var newVersion = new FileVersion(versionTag, path, size);
            _versions.Add(newVersion);

            // Update current file properties to match the latest version
            Path = path;
            Size = size;
        }

        public void RollbackToVersion(Guid versionId)
        {
            if (Status == FileStatusEnum.Deleted)
                throw new FileManagementDomainException("Cannot rollback a deleted file.");

            var version = _versions.FirstOrDefault(v => v.Id == versionId);
            if (version == null)
                throw new FileManagementDomainException("Version not found.");

            Path = version.Path;
            Size = version.Size;
        }

    }
}
