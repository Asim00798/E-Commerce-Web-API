using Domain.BoundedContexts.UserManagement.Security.Enums;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiClient.Behaviors
{
    public partial class ApiClient : BaseEntity, IAggregateRoot
    {
        public string ClientId { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public ApiClientStatus Status { get; private set; }
        public string? SuspensionReason { get; private set; }
        public DateTime RegisteredAt { get; private set; }
        public DateTime? ActivatedAt { get; private set; }
        public DateTime? DeactivatedAt { get; private set; }
        public DateTime? SuspendedAt { get; private set; }

        private ApiClient() { }

        private ApiClient(string clientId, string name, DateTime registeredAt)
        {
            Id = Guid.NewGuid();
            ClientId = clientId;
            Name = name;
            Status = ApiClientStatus.Pending;
            RegisteredAt = registeredAt;
        }
    }
}
