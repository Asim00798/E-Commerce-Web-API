using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Policies
{
    public class FileAccessPolicy
    {
        public bool CanAccess(AccessLevelEnum requiredLevel, AccessLevelEnum providedLevel)
        {
            return providedLevel >= requiredLevel;
        }
    }
}
