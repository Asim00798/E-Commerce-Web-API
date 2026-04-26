using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Exceptions;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Events;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Policies;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Services;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Behaviors
{
    public partial class File
    {
        public void Rename(FileName newName)
        {
            if (newName == null) throw new ArgumentNullException(nameof(newName));
            if (Status == FileStatusEnum.Deleted) 
                throw new FileManagementDomainException("Cannot rename a deleted file.");

            Name = newName;
        }

        public void Move(FilePath newPath)
        {
            if (newPath == null) throw new ArgumentNullException(nameof(newPath));
            if (Status == FileStatusEnum.Deleted)
                throw new FileManagementDomainException("Cannot move a deleted file.");

            Path = newPath;
        }

        public void ChangeVisibility(FileVisibilityEnum visibility)
        {
            Visibility = visibility;
        }

        public void Archive(FileRetentionPolicy policy, int retentionDays)
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            if (Status == FileStatusEnum.Deleted)
                throw new FileManagementDomainException("Cannot archive a deleted file.");

            // Standard check: In a real scenario, we might need a domain-owned creation date
            // but for now we follow the user's instruction to stop using base properties if possible.
            // However, ShouldRetain might still need CreatedAt. 
            // Since CreatedAt is in BaseEntity and we didn't delete it (we UNDID the deletion), it's there.
            if (!policy.ShouldRetain(CreatedAt, retentionDays))
            {
                throw new FileManagementDomainException("File does not meet retention policy requirements for archival.");
            }

            Status = FileStatusEnum.Archived;
        }

        public void Delete(Guid deletedBy, FileDeletionEligibilityService eligibilityService, FileRetentionPolicy policy, int retentionDays)
        {
            if (eligibilityService == null) throw new ArgumentNullException(nameof(eligibilityService));
            
            if (!eligibilityService.CanDelete(this, deletedBy, policy, retentionDays))
            {
                throw new FileManagementDomainException("File is not eligible for deletion at this time.");
            }

            Status = FileStatusEnum.Deleted;

            AddDomainEvent(new FileDeletedEvent(Id));
        }


        public void Restore()
        {
            if (Status != FileStatusEnum.Deleted && Status != FileStatusEnum.Archived)
                throw new FileManagementDomainException("File is already active.");

            Status = FileStatusEnum.Active;

            AddDomainEvent(new FileRestoredEvent(Id));
        }

    }
}
