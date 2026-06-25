using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Exceptions
{
    public sealed class RefreshTokenExpiredException : DomainException
    {
        public RefreshTokenExpiredException(Guid tokenId)
            : base($"Refresh token '{tokenId}' has expired.") { }
    }
}
