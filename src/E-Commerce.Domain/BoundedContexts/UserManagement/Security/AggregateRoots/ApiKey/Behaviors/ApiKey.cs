using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.ValueObjects;
using Domain.BoundedContexts.UserManagement.Security.Enums;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.Behaviors
{
    public partial class ApiKey : BaseEntity, IAggregateRoot
    {
        public Guid ApiClientId { get; private set; }
        public string KeyHash { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public ApiKeyStatus Status { get; private set; }
        public ApiKeyPermissions Permissions { get; private set; } = null!;
        public ApiKeyScope Scope { get; private set; } = null!;
        public DateTime ExpiresAt { get; private set; }
        public DateTime? ActivatedAt { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public DateTime? CompromisedAt { get; private set; }
        public DateTime? LastUsedAt { get; private set; }
        public string? RevocationReason { get; private set; }

        private ApiKey() { }

        private ApiKey(
            Guid apiClientId,
            string keyHash,
            string name,
            ApiKeyPermissions permissions,
            ApiKeyScope scope,
            DateTime createdAt,
            DateTime expiresAt)
        {
            Id = Guid.NewGuid();
            ApiClientId = apiClientId;
            KeyHash = keyHash;
            Name = name;
            Permissions = permissions;
            Scope = scope;
            CreatedAt = createdAt;
            ExpiresAt = expiresAt;
            Status = ApiKeyStatus.Pending;
        }
    }
}
