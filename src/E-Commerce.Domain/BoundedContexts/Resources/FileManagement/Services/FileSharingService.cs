using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;
using FileAggregate = E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Behaviors.File;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Services
{
    /// <summary>
    /// Pure logic service to decide if a file can be shared between users.
    /// </summary>
    public class FileSharingService
    {
        public bool CanShare(FileAggregate file, Guid senderId, Guid receiverId)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            
            // Basic rules for sharing
            if (file.Status == FileStatusEnum.Deleted) return false;
            
            // Only owner can share if it's private
            if (file.Visibility == FileVisibilityEnum.Private && file.OwnerId != senderId) return false;

            // Cannot share with yourself
            if (senderId == receiverId) return false;

            return true;
        }
    }
}
