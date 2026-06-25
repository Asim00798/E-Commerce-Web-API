#if false
using System;

namespace E_Commerce.Domain.Events.Identity.RolePermission
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
#endif