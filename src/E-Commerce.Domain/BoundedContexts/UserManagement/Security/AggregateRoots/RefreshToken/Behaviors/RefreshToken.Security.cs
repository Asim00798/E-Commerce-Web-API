using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Exceptions;
using Domain.BoundedContexts.UserManagement.Security.Enums;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Behaviors
{
    public partial class RefreshToken
    {
        public void DetectReuse(DateTime detectedAt)
        {
            if (ReuseDetected)
                return;

            if (Status == RefreshTokenStatus.Active)
                throw new BusinessRuleViolationException(
                    "Reuse can only be detected on non-active tokens.");

            ReuseDetected = true;
            RaiseReuseDetectedEvent(detectedAt);
            throw new RefreshTokenReuseException(Id, UserId);
        }

        public void ValidateActive(DateTime at)
        {
            if (Status != RefreshTokenStatus.Active)
                throw new InvalidRefreshTokenException(
                    $"Refresh token is not active. Current status: '{Status}'.");

            if (Metadata.IsExpired(at))
            {
                Status = RefreshTokenStatus.Expired;
                RaiseExpiredEvent();
                throw new RefreshTokenExpiredException(Id);
            }

            Metadata = Metadata.WithLastUsedAt(at);
        }
    }
}
