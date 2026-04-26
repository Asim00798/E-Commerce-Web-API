using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;
using FileAggregate = E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Behaviors.File;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Services
{
    /// <summary>
    /// Pure logic service to resolve effective visibility of a file for a user.
    /// </summary>
    public class FileVisibilityService
    {
        public FileVisibilityEnum ResolveVisibility(FileAggregate file, Guid userId, AccessLevelEnum accessLevel)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            // Owner sees everything as public (to them)
            if (file.OwnerId == userId) return FileVisibilityEnum.Public;

            // Deleted files are hidden
            if (file.Status == FileStatusEnum.Deleted) return FileVisibilityEnum.Private;

            // If user has high access, they might see it regardless
            if (accessLevel == AccessLevelEnum.Admin) return FileVisibilityEnum.Public;

            return file.Visibility;
        }
    }
}
