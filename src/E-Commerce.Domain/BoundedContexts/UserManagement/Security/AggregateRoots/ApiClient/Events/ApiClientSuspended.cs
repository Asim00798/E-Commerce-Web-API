using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Events
{
    public sealed class ApiClientSuspended : DomainEvent
    {
        public Guid ApiClientId { get; }
        public string ClientId { get; }
        public string Reason { get; }

        public ApiClientSuspended(Guid apiClientId, string clientId, string reason)
        {
            ApiClientId = apiClientId;
            ClientId = clientId;
            Reason = reason;
        }
    }
}
