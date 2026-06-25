using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.Events
{
    public sealed class ApiKeyRevoked : DomainEvent
    {
        public Guid ApiKeyId { get; }
        public Guid ApiClientId { get; }
        public string? Reason { get; }

        public ApiKeyRevoked(Guid apiKeyId, Guid apiClientId, string? reason = null)
        {
            ApiKeyId = apiKeyId;
            ApiClientId = apiClientId;
            Reason = reason;
        }
    }
}
