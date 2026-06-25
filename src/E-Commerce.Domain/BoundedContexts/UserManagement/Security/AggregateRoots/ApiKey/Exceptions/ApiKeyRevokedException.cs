using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.Exceptions
{
    public sealed class ApiKeyRevokedException : DomainException
    {
        public ApiKeyRevokedException(Guid apiKeyId)
            : base($"API key '{apiKeyId}' has been revoked.") { }
    }
}
