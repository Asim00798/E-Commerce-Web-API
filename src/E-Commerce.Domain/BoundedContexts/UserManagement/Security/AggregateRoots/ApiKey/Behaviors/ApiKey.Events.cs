using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.Events;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.Behaviors
{
    public partial class ApiKey
    {
        private void RaiseActivatedEvent() =>
            AddDomainEvent(new ApiKeyActivated(Id, ApiClientId));

        private void RaiseRevokedEvent(string reason) =>
            AddDomainEvent(new ApiKeyRevoked(Id, ApiClientId, reason));

        private void RaiseCompromisedEvent(string? details) =>
            AddDomainEvent(new ApiKeyCompromised(Id, ApiClientId, details));
    }
}
