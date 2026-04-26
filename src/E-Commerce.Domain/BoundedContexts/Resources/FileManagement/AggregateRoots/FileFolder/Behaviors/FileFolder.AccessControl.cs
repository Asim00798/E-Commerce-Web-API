using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.Entities;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.Exceptions;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.Policies;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.Behaviors
{
    public partial class FileFolder
    {
        public bool HasAccess(Guid identityId, AccessLevelEnum requiredLevel, FolderAccessPolicy policy)
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            if (Status == FileStatusEnum.Deleted) return false;

            // Owner always has full access
            if (OwnerId == identityId) return true;

            var rule = _accessRules.FirstOrDefault(r => r.IdentityId == identityId);
            if (rule == null) return false;

            return policy.CanAccess(requiredLevel, rule.AccessLevel);
        }

        public void GrantAccess(Guid identityId, AccessLevelEnum level)
        {
            if (Status == FileStatusEnum.Deleted) throw new FolderDomainException("Cannot grant access to a deleted folder.");

            var existingRule = _accessRules.FirstOrDefault(r => r.IdentityId == identityId);
            if (existingRule != null)
            {
                _accessRules.Remove(existingRule);
            }

            _accessRules.Add(new FolderAccessRule(identityId, level));
        }

        public void RevokeAccess(Guid identityId)
        {
            var rule = _accessRules.FirstOrDefault(r => r.IdentityId == identityId);
            if (rule != null)
            {
                _accessRules.Remove(rule);
            }
        }

    }
}

