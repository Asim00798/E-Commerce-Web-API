using System;

namespace E_Commerce.Domain.DomainEvents.Identity.RolePermission
{
    public sealed class PermissionAssignedToRole : DomainEvent
    {
        public Guid PermissionAssignedToRoleId { get; }

        public PermissionAssignedToRole(Guid permissionAssignedToRoleId)
        {
            PermissionAssignedToRoleId = permissionAssignedToRoleId;
        }
    }
}