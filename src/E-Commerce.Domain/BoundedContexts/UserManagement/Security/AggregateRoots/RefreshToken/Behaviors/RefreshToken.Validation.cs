using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Exceptions;
using Domain.BoundedContexts.UserManagement.Security.Enums;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Behaviors
{
    public partial class RefreshToken
    {
        public bool IsActive => Status == RefreshTokenStatus.Active;

        public bool IsUsable(DateTime at) =>
            Status == RefreshTokenStatus.Active && !Metadata.IsExpired(at);

        public void EnsureNotRevokedOrRotated()
        {
            if (Status is RefreshTokenStatus.Revoked or RefreshTokenStatus.Rotated)
                throw new InvalidRefreshTokenException(
                    $"Refresh token '{Id}' is no longer valid (status: {Status}).");
        }
    }
}
