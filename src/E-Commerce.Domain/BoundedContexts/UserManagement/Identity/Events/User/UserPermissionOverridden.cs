using System;

namespace E_Commerce.Domain.Events.Identity.User
{
    public sealed class UserPermissionOverridden : DomainEvent
    {
        public Guid UserPermissionOverriddenId { get; }

        public UserPermissionOverridden(Guid userPermissionOverriddenId)
        {
            UserPermissionOverriddenId = userPermissionOverriddenId;
        }
    }
}