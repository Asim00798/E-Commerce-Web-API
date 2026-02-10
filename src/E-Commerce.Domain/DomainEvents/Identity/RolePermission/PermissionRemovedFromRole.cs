using System;

namespace E_Commerce.Domain.DomainEvents.Identity.RolePermission
{
    public sealed class PermissionRemovedFromRole : DomainEvent
    {
        public Guid PermissionRemovedFromRoleId { get; }

        public PermissionRemovedFromRole(Guid permissionRemovedFromRoleId)
        {
            PermissionRemovedFromRoleId = permissionRemovedFromRoleId;
        }
    }
}