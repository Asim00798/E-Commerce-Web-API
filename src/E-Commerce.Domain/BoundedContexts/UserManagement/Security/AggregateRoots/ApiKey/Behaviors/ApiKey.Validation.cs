using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.Exceptions;
using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.ValueObjects;
using Domain.BoundedContexts.UserManagement.Security.Enums;
using Domain.BoundedContexts.UserManagement.Security.ValueObjects;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.Behaviors
{
    public partial class ApiKey
    {
        public void ValidateUsage(
            ApiKeyScope requiredScope,
            IEnumerable<string> requiredPermissions,
            RequestContext context)
        {
            ArgumentNullException.ThrowIfNull(requiredScope);
            ArgumentNullException.ThrowIfNull(requiredPermissions);
            ArgumentNullException.ThrowIfNull(context);

            EnsureUsable(context.RequestedAt);
            EnsureScope(requiredScope);
            EnsurePermissions(requiredPermissions);

            LastUsedAt = context.RequestedAt;
        }

        public void UpdateScope(ApiKeyScope newScope)
        {
            ArgumentNullException.ThrowIfNull(newScope);

            if (Status != ApiKeyStatus.Active && Status != ApiKeyStatus.Pending)
                throw new BusinessRuleViolationException(
                    $"Cannot update scope for API key in status '{Status}'.");

            Scope = newScope;
        }

        public void UpdatePermissions(ApiKeyPermissions newPermissions)
        {
            ArgumentNullException.ThrowIfNull(newPermissions);

            if (Status != ApiKeyStatus.Active && Status != ApiKeyStatus.Pending)
                throw new BusinessRuleViolationException(
                    $"Cannot update permissions for API key in status '{Status}'.");

            Permissions = newPermissions;
        }

        private void EnsureUsable(DateTime at)
        {
            if (Status == ApiKeyStatus.Revoked)
                throw new ApiKeyRevokedException(Id);

            if (Status == ApiKeyStatus.Compromised)
                throw new BusinessRuleViolationException($"API key '{Id}' has been compromised.");

            if (Status != ApiKeyStatus.Active)
                throw new BusinessRuleViolationException($"API key '{Id}' is not active.");

            if (at >= ExpiresAt)
            {
                Status = ApiKeyStatus.Expired;
                throw new ApiKeyExpiredException(Id);
            }
        }

        private void EnsureScope(ApiKeyScope requiredScope)
        {
            if (!Scope.Matches(requiredScope))
                throw new BusinessRuleViolationException(
                    $"API key scope '{Scope}' does not grant access to '{requiredScope}'.");
        }

        private void EnsurePermissions(IEnumerable<string> requiredPermissions)
        {
            if (!Permissions.HasAllPermissions(requiredPermissions))
                throw new BusinessRuleViolationException("API key does not grant the required permissions.");
        }
    }
}
