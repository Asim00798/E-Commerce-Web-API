using System;

namespace E_Commerce.Domain.DomainEvents.Identity.User
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