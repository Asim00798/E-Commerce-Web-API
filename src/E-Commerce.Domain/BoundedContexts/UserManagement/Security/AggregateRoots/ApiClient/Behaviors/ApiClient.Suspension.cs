using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Exceptions;
using Domain.BoundedContexts.UserManagement.Security.Enums;
using Domain.BoundedContexts.UserManagement.Security.Rules;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Behaviors
{
    public partial class ApiClient
    {
        public void Suspend(string reason, DateTime suspendedAt)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new BusinessRuleViolationException("Suspension reason is required.");

            if (Status == ApiClientStatus.Suspended)
                throw new ApiClientAlreadySuspendedException(ClientId);

            if (Status != ApiClientStatus.Active)
                throw new ApiClientNotActiveException(ClientId);

            BusinessRuleChecker.Check(new ApiClientMustBeActiveRule(Status, ClientId));

            Status = ApiClientStatus.Suspended;
            SuspensionReason = reason.Trim();
            SuspendedAt = suspendedAt;

            RaiseSuspendedEvent(reason.Trim());
        }

        public void Reinstate(DateTime reinstatedAt)
        {
            if (Status != ApiClientStatus.Suspended)
                throw new BusinessRuleViolationException(
                    $"Only suspended API clients can be reinstated. Current status: '{Status}'.");

            Status = ApiClientStatus.Active;
            ActivatedAt = reinstatedAt;
            SuspensionReason = null;
            SuspendedAt = null;

            RaiseActivatedEvent();
        }
    }
}
