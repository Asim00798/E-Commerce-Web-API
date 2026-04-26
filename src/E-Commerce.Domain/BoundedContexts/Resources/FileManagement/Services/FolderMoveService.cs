using FolderAggregate = E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.Behaviors.FileFolder;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Services
{
    /// <summary>
    /// Pure logic service to validate folder movement within a hierarchy.
    /// </summary>
    public class FolderMoveService
    {
        public bool CanMove(FolderAggregate source, FolderAggregate targetFolder)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            
            // Root move is always fine structurally
            if (targetFolder == null) return true;

            // Cannot move a folder into itself
            if (source.Id == targetFolder.Id) return false;

            // Cannot move if target is a sub-item of source (hierarchy constraint)
            // This usually requires checking the full path or parent chain
            if (targetFolder.Path.Value.StartsWith(source.Path.Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}
