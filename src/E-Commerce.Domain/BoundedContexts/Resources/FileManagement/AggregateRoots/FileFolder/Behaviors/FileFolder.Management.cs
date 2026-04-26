using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.Exceptions;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Services;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.Behaviors
{
    public partial class FileFolder
    {
        public void Rename(FolderName newName)
        {
            if (newName == null) throw new ArgumentNullException(nameof(newName));
            if (Status == FileStatusEnum.Deleted) throw new FolderDomainException("Cannot rename a deleted folder.");

            Name = newName;
        }

        public void Move(FolderPath newPath, Guid? newParentId, FileFolder targetFolder, FolderMoveService moveService)
        {
            if (newPath == null) throw new ArgumentNullException(nameof(newPath));
            if (moveService == null) throw new ArgumentNullException(nameof(moveService));
            if (Status == FileStatusEnum.Deleted) throw new FolderDomainException("Cannot move a deleted folder.");

            if (!moveService.CanMove(this, targetFolder))
            {
                throw new FolderDomainException("Invalid move operation: hierarchy violation or circular dependency detected.");
            }

            Path = newPath;
            ParentId = newParentId;
        }

        public void ChangeParent(Guid? newParentId, FolderPath newPath, FileFolder targetFolder, FolderMoveService moveService)
        {
            Move(newPath, newParentId, targetFolder, moveService);
        }

        public void Delete(Guid deletedBy)
        {
            if (Status == FileStatusEnum.Deleted) return;

            Status = FileStatusEnum.Deleted;
        }

        public void Restore()
        {
            if (Status != FileStatusEnum.Deleted) throw new FolderDomainException("Folder is not deleted.");

            Status = FileStatusEnum.Active;
        }

    }
}

