using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Entities;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Exceptions;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Policies;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Services;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Behaviors
{
    public partial class File
    {
        public bool HasAccess(Guid identityId, AccessLevelEnum requiredLevel, FileAccessPolicy policy, FileVisibilityService visibilityService)
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            if (visibilityService == null) throw new ArgumentNullException(nameof(visibilityService));

            var effectiveVisibility = visibilityService.ResolveVisibility(this, identityId, requiredLevel);
            if (effectiveVisibility == FileVisibilityEnum.Private && OwnerId != identityId)
            {
                // Note: This logic depends on how ResolveVisibility is implemented.
                // For now, we use it as a pre-check.
            }

            if (Status == FileStatusEnum.Deleted) return false;

            // Owner always has full access
            if (OwnerId == identityId) return true;

            var rule = _accessRules.FirstOrDefault(r => r.IdentityId == identityId && r.IsValid);
            if (rule == null) return false;

            return policy.CanAccess(requiredLevel, rule.AccessLevel);
        }

        public void GrantAccess(Guid receiverId, AccessLevelEnum level, Guid senderId, FileSharingService sharingService, DateTime? expiresAt = null)
        {
            if (sharingService == null) throw new ArgumentNullException(nameof(sharingService));

            if (!sharingService.CanShare(this, senderId, receiverId))
            {
                throw new FileManagementDomainException("Sharing policy violation: Cannot share this file with the specified user.");
            }

            var existingRule = _accessRules.FirstOrDefault(r => r.IdentityId == receiverId);
            if (existingRule != null)
            {
                _accessRules.Remove(existingRule);
            }

            _accessRules.Add(new FileAccessRule(receiverId, level, expiresAt));
        }

        public void RevokeAccess(Guid identityId)
        {
            var rule = _accessRules.FirstOrDefault(r => r.IdentityId == identityId);
            if (rule != null)
            {
                _accessRules.Remove(rule);
            }
        }

        public void UpdateAccess(Guid identityId, AccessLevelEnum newLevel)
        {
            var rule = _accessRules.FirstOrDefault(r => r.IdentityId == identityId);
            if (rule == null)
                throw new FileManagementDomainException("Access rule not found for the specified identity.");

            _accessRules.Remove(rule);
            _accessRules.Add(new FileAccessRule(identityId, newLevel, rule.ExpiresAt));
        }

    }
}

