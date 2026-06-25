using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Events
{
    public sealed class RefreshTokenRefreshed : DomainEvent
    {
        public Guid TokenId { get; }
        public Guid UserId { get; }
        public Guid ReplacementTokenId { get; }

        public RefreshTokenRefreshed(Guid tokenId, Guid userId, Guid replacementTokenId)
        {
            TokenId = tokenId;
            UserId = userId;
            ReplacementTokenId = replacementTokenId;
        }
    }
}
