using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.Exceptions
{
    public sealed class ApiKeyExpiredException : DomainException
    {
        public ApiKeyExpiredException(Guid apiKeyId)
            : base($"API key '{apiKeyId}' has expired.") { }
    }
}
