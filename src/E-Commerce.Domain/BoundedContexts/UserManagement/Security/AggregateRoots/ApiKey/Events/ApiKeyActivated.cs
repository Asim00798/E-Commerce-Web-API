using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.Events
{
    public sealed class ApiKeyActivated : DomainEvent
    {
        public Guid ApiKeyId { get; }
        public Guid ApiClientId { get; }

        public ApiKeyActivated(Guid apiKeyId, Guid apiClientId)
        {
            ApiKeyId = apiKeyId;
            ApiClientId = apiClientId;
        }
    }
}
