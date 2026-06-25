using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.ValueObjects;
using Domain.BoundedContexts.UserManagement.Security.Enums;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.Behaviors
{
    public partial class ApiKey
    {
        public static ApiKey Create(
            Guid apiClientId,
            string keyHash,
            string name,
            ApiKeyPermissions permissions,
            ApiKeyScope scope,
            DateTime createdAt,
            DateTime expiresAt)
        {
            if (apiClientId == Guid.Empty)
                throw new BusinessRuleViolationException("API client identifier is required.");

            if (string.IsNullOrWhiteSpace(keyHash))
                throw new BusinessRuleViolationException("API key hash is required.");

            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessRuleViolationException("API key name is required.");

            if (expiresAt <= createdAt)
                throw new BusinessRuleViolationException("API key expiration must be after creation time.");

            return new ApiKey(
                apiClientId,
                keyHash.Trim(),
                name.Trim(),
                permissions,
                scope,
                createdAt,
                expiresAt);
        }

        public void Activate(DateTime activatedAt)
        {
            EnsurePending();

            Status = ApiKeyStatus.Active;
            ActivatedAt = activatedAt;

            RaiseActivatedEvent();
        }

        private void EnsurePending()
        {
            if (Status != ApiKeyStatus.Pending)
                throw new BusinessRuleViolationException(
                    $"Only pending API keys can be activated. Current status: '{Status}'.");
        }
    }
}
