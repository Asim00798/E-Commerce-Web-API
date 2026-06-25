using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.Events
{
    public sealed class ApiKeyCompromised : DomainEvent
    {
        public Guid ApiKeyId { get; }
        public Guid ApiClientId { get; }
        public string? Details { get; }

        public ApiKeyCompromised(Guid apiKeyId, Guid apiClientId, string? details = null)
        {
            ApiKeyId = apiKeyId;
            ApiClientId = apiClientId;
            Details = details;
        }
    }
}
