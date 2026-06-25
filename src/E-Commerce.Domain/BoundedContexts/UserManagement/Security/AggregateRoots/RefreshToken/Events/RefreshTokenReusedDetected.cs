using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Events
{
    public sealed class RefreshTokenReusedDetected : DomainEvent
    {
        public Guid TokenId { get; }
        public Guid UserId { get; }
        public DateTime DetectedAt { get; }

        public RefreshTokenReusedDetected(Guid tokenId, Guid userId, DateTime detectedAt)
        {
            TokenId = tokenId;
            UserId = userId;
            DetectedAt = detectedAt;
        }
    }
}
