using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Exceptions;
using Domain.BoundedContexts.UserManagement.Security.Enums;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Behaviors
{
    public partial class ApiClient
    {
        public void Activate(DateTime activatedAt)
        {
            if (Status == ApiClientStatus.Active)
                throw new BusinessRuleViolationException($"API client '{ClientId}' is already active.");

            if (Status == ApiClientStatus.Suspended)
                throw new ApiClientAlreadySuspendedException(ClientId);

            if (Status != ApiClientStatus.Pending && Status != ApiClientStatus.Inactive)
                throw new BusinessRuleViolationException(
                    $"Cannot activate API client '{ClientId}' from status '{Status}'.");

            Status = ApiClientStatus.Active;
            ActivatedAt = activatedAt;
            DeactivatedAt = null;
            SuspensionReason = null;
            SuspendedAt = null;

            RaiseActivatedEvent();
        }

        public void Deactivate(DateTime deactivatedAt)
        {
            if (Status == ApiClientStatus.Inactive)
                throw new BusinessRuleViolationException($"API client '{ClientId}' is already inactive.");

            if (Status == ApiClientStatus.Suspended)
                throw new ApiClientAlreadySuspendedException(ClientId);

            if (Status != ApiClientStatus.Active)
                throw new ApiClientNotActiveException(ClientId);

            Status = ApiClientStatus.Inactive;
            DeactivatedAt = deactivatedAt;

            RaiseDeactivatedEvent();
        }
    }
}
