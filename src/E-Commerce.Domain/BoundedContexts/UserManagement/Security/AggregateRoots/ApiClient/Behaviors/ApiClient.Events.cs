using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Events;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Behaviors
{
    public partial class ApiClient
    {
        private void RaiseRegisteredEvent() =>
            AddDomainEvent(new ApiClientRegistered(Id, ClientId, Name));

        private void RaiseActivatedEvent() =>
            AddDomainEvent(new ApiClientActivated(Id, ClientId));

        private void RaiseDeactivatedEvent() =>
            AddDomainEvent(new ApiClientDeactivated(Id, ClientId));

        private void RaiseSuspendedEvent(string reason) =>
            AddDomainEvent(new ApiClientSuspended(Id, ClientId, reason));
    }
}
