using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Events
{
    public sealed class ApiClientRegistered : DomainEvent
    {
        public Guid ApiClientId { get; }
        public string ClientId { get; }
        public string Name { get; }

        public ApiClientRegistered(Guid apiClientId, string clientId, string name)
        {
            ApiClientId = apiClientId;
            ClientId = clientId;
            Name = name;
        }
    }
}
