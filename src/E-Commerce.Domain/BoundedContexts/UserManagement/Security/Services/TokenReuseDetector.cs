using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Behaviors;
using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Events;
using Domain.BoundedContexts.UserManagement.Security.Enums;

namespace Domain.BoundedContexts.UserManagement.Security.DomainServices
{
    public sealed class TokenReuseDetector
    {
        public bool IsReuseAttempt(RefreshToken token, DateTime attemptedAt)
        {
            ArgumentNullException.ThrowIfNull(token);

            return token.Status is RefreshTokenStatus.Rotated or RefreshTokenStatus.Revoked
                   && !token.ReuseDetected;
        }

        public void HandleReuse(RefreshToken token, DateTime detectedAt)
        {
            ArgumentNullException.ThrowIfNull(token);
            token.DetectReuse(detectedAt);
        }

        public AllUserTokensRevoked CreateMassRevocationEvent(
            Guid userId,
            int tokenCount,
            string reason) =>
            new(userId, tokenCount, reason);
    }
}
