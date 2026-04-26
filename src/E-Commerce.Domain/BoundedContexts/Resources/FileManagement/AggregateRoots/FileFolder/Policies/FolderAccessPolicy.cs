using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.Policies
{
    public class FolderAccessPolicy
    {
        public bool CanAccess(AccessLevelEnum requiredLevel, AccessLevelEnum providedLevel)
        {
            return providedLevel >= requiredLevel;
        }
    }
}
