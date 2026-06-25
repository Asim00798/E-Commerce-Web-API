using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Exceptions
{
    public sealed class ApiClientAlreadySuspendedException : DomainException
    {
        public ApiClientAlreadySuspendedException(string clientId)
            : base($"API client '{clientId}' is already suspended.") { }
    }
}
