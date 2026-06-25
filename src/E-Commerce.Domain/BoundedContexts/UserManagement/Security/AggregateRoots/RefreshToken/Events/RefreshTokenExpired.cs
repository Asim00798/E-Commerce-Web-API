using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Events
{
    public sealed class RefreshTokenExpired : DomainEvent
    {
        public Guid TokenId { get; }
        public Guid UserId { get; }

        public RefreshTokenExpired(Guid tokenId, Guid userId)
        {
            TokenId = tokenId;
            UserId = userId;
        }
    }
}
