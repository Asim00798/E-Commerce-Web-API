using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Events
{
    public sealed class ApiClientDeactivated : DomainEvent
    {
        public Guid ApiClientId { get; }
        public string ClientId { get; }

        public ApiClientDeactivated(Guid apiClientId, string clientId)
        {
            ApiClientId = apiClientId;
            ClientId = clientId;
        }
    }
}
