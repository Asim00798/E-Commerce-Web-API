using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Events
{
    public sealed class RefreshTokenIssued : DomainEvent
    {
        public Guid TokenId { get; }
        public Guid UserId { get; }
        public DateTime ExpiresAt { get; }

        public RefreshTokenIssued(Guid tokenId, Guid userId, DateTime expiresAt)
        {
            TokenId = tokenId;
            UserId = userId;
            ExpiresAt = expiresAt;
        }
    }
}
