using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Exceptions;
using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.ValueObjects;
using Domain.BoundedContexts.UserManagement.Security.Enums;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Behaviors
{
    public partial class RefreshToken
    {
        public RefreshToken Rotate(
            string replacementTokenHash,
            TokenFingerprint replacementFingerprint,
            DeviceInfo deviceInfo,
            DateTime rotatedAt,
            DateTime newExpiresAt)
        {
            EnsureCanRotate(rotatedAt);

            if (string.IsNullOrWhiteSpace(replacementTokenHash))
                throw new BusinessRuleViolationException("Replacement token hash is required.");

            var replacement = Issue(UserId, replacementTokenHash, replacementFingerprint, deviceInfo, rotatedAt, newExpiresAt);

            Status = RefreshTokenStatus.Rotated;
            ReplacedByTokenId = replacement.Id;
            Metadata = Metadata.WithLastUsedAt(rotatedAt);

            RaiseRefreshedEvent(replacement.Id);
            return replacement;
        }

        private void EnsureCanRotate(DateTime at)
        {
            BusinessRuleChecker.Check(new RefreshTokenMustBeActiveRule(Status));

            if (Metadata.IsExpired(at))
            {
                Status = RefreshTokenStatus.Expired;
                RaiseExpiredEvent();
                throw new RefreshTokenExpiredException(Id);
            }
        }
    }
}
