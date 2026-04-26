using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.Entities
{
    public class FolderAccessRule : BaseEntity
    {
        public Guid IdentityId { get; private set; }
        public AccessLevelEnum AccessLevel { get; private set; }

        public FolderAccessRule(Guid identityId, AccessLevelEnum accessLevel)
        {
            IdentityId = identityId;
            AccessLevel = accessLevel;
        }
    }
}
