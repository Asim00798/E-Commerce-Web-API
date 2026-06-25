using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Exceptions
{
    public sealed class ApiClientNotActiveException : DomainException
    {
        public ApiClientNotActiveException(string clientId)
            : base($"API client '{clientId}' is not active.") { }
    }
}
