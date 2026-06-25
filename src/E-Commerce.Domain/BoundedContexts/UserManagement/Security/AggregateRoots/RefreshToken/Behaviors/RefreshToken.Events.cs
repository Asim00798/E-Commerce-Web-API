using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Events;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Behaviors
{
    public partial class RefreshToken
    {
        private void RaiseIssuedEvent() =>
            AddDomainEvent(new RefreshTokenIssued(Id, UserId, Metadata.ExpiresAt));

        private void RaiseRefreshedEvent(Guid replacementTokenId) =>
            AddDomainEvent(new RefreshTokenRefreshed(Id, UserId, replacementTokenId));

        private void RaiseRevokedEvent(string reason) =>
            AddDomainEvent(new RefreshTokenRevoked(Id, UserId, reason));

        private void RaiseExpiredEvent() =>
            AddDomainEvent(new RefreshTokenExpired(Id, UserId));

        private void RaiseReuseDetectedEvent(DateTime detectedAt) =>
            AddDomainEvent(new RefreshTokenReusedDetected(Id, UserId, detectedAt));
    }
}
