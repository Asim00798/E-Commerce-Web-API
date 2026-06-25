using Domain.BoundedContexts.UserManagement.Security.Enums;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Behaviors
{
    public partial class RefreshToken
    {
        public void Revoke(string reason, DateTime revokedAt)
        {
            if (Status == RefreshTokenStatus.Revoked)
                throw new BusinessRuleViolationException($"Refresh token '{Id}' is already revoked.");

            if (Status == RefreshTokenStatus.Rotated)
                throw new BusinessRuleViolationException("Rotated refresh tokens cannot be revoked individually.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new BusinessRuleViolationException("Revocation reason is required.");

            Status = RefreshTokenStatus.Revoked;
            RevokedAt = revokedAt;
            RevocationReason = reason.Trim();

            RaiseRevokedEvent(reason.Trim());
        }

        public void Expire(DateTime expiredAt)
        {
            if (Status == RefreshTokenStatus.Expired)
                return;

            if (Status != RefreshTokenStatus.Active)
                throw new BusinessRuleViolationException(
                    $"Only active refresh tokens can expire. Current status: '{Status}'.");

            Status = RefreshTokenStatus.Expired;
            RaiseExpiredEvent();
        }
    }
}
