using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.Exceptions;
using Domain.BoundedContexts.UserManagement.Security.Enums;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.Behaviors
{
    public partial class ApiKey
    {
        public void Revoke(string reason, DateTime revokedAt)
        {
            if (Status == ApiKeyStatus.Revoked)
                throw new ApiKeyRevokedException(Id);

            if (Status == ApiKeyStatus.Compromised)
                throw new BusinessRuleViolationException("Compromised API keys cannot be revoked separately.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new BusinessRuleViolationException("Revocation reason is required.");

            Status = ApiKeyStatus.Revoked;
            RevokedAt = revokedAt;
            RevocationReason = reason.Trim();

            RaiseRevokedEvent(reason.Trim());
        }

        public void MarkCompromised(string details, DateTime compromisedAt)
        {
            if (Status == ApiKeyStatus.Compromised)
                throw new BusinessRuleViolationException($"API key '{Id}' is already marked as compromised.");

            if (Status == ApiKeyStatus.Revoked)
                throw new ApiKeyRevokedException(Id);

            Status = ApiKeyStatus.Compromised;
            CompromisedAt = compromisedAt;
            RevocationReason = details?.Trim();

            RaiseCompromisedEvent(details?.Trim());
        }
    }
}
