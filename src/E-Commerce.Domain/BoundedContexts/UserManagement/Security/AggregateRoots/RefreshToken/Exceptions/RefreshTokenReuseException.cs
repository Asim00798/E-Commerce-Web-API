using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Exceptions
{
    public sealed class RefreshTokenReuseException : DomainException
    {
        public RefreshTokenReuseException(Guid tokenId, Guid userId)
            : base($"Refresh token reuse detected for token '{tokenId}' belonging to user '{userId}'.")
        {
            TokenId = tokenId;
            UserId = userId;
        }

        public Guid TokenId { get; }
        public Guid UserId { get; }
    }
}
