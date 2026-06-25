using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Behaviors;
using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.Behaviors;
using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.ValueObjects;
using Domain.BoundedContexts.UserManagement.Security.ValueObjects;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.Services
{
    public sealed class ApiKeyValidationService
    {
        public void Validate(
            ApiKey apiKey,
            ApiClient apiClient,
            ApiKeyScope requiredScope,
            IEnumerable<string> requiredPermissions,
            RequestContext context)
        {
            ArgumentNullException.ThrowIfNull(apiKey);
            ArgumentNullException.ThrowIfNull(apiClient);
            ArgumentNullException.ThrowIfNull(requiredScope);
            ArgumentNullException.ThrowIfNull(requiredPermissions);
            ArgumentNullException.ThrowIfNull(context);

            if (apiKey.ApiClientId != apiClient.Id)
                throw new BusinessRuleViolationException("API key does not belong to the specified client.");

            apiClient.EnsureOperational();
            apiKey.ValidateUsage(requiredScope, requiredPermissions, context);
        }
    }
}
