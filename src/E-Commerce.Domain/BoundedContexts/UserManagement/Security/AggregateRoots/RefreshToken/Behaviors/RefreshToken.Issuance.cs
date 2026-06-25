using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.ValueObjects;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Behaviors
{
    public partial class RefreshToken
    {
        public static RefreshToken Issue(
            Guid userId,
            string tokenHash,
            TokenFingerprint fingerprint,
            DeviceInfo deviceInfo,
            DateTime issuedAt,
            DateTime expiresAt)
        {
            if (userId == Guid.Empty)
                throw new BusinessRuleViolationException("User identifier is required.");

            if (string.IsNullOrWhiteSpace(tokenHash))
                throw new BusinessRuleViolationException("Token hash is required.");

            var metadata = new TokenMetadata(issuedAt, expiresAt);
            var token = new RefreshToken(userId, tokenHash.Trim(), fingerprint, deviceInfo, metadata);

            token.RaiseIssuedEvent();
            return token;
        }
    }
}
