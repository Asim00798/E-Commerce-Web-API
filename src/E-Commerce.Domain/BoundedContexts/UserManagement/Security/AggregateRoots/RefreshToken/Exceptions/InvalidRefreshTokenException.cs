using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Exceptions
{
    public sealed class InvalidRefreshTokenException : DomainException
    {
        public InvalidRefreshTokenException(string message) : base(message) { }
    }
}
