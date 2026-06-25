using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Events
{
    public sealed class ApiClientActivated : DomainEvent
    {
        public Guid ApiClientId { get; }
        public string ClientId { get; }

        public ApiClientActivated(Guid apiClientId, string clientId)
        {
            ApiClientId = apiClientId;
            ClientId = clientId;
        }
    }
}
