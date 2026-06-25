using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Events
{
    public sealed class RefreshTokenRevoked : DomainEvent
    {
        public Guid TokenId { get; }
        public Guid UserId { get; }
        public string? Reason { get; }

        public RefreshTokenRevoked(Guid tokenId, Guid userId, string? reason = null)
        {
            TokenId = tokenId;
            UserId = userId;
            Reason = reason;
        }
    }
}
