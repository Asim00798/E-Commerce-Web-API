using Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.ValueObjects;
using Domain.BoundedContexts.UserManagement.Security.Enums;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Behaviors
{
    public partial class RefreshToken : BaseEntity, IAggregateRoot
    {
        public Guid UserId { get; private set; }
        public string TokenHash { get; private set; } = null!;
        public TokenFingerprint Fingerprint { get; private set; } = null!;
        public DeviceInfo DeviceInfo { get; private set; } = null!;
        public TokenMetadata Metadata { get; private set; } = null!;
        public RefreshTokenStatus Status { get; private set; }
        public Guid? ReplacedByTokenId { get; private set; }
        public bool ReuseDetected { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public string? RevocationReason { get; private set; }

        private RefreshToken() { }

        private RefreshToken(
            Guid userId,
            string tokenHash,
            TokenFingerprint fingerprint,
            DeviceInfo deviceInfo,
            TokenMetadata metadata)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            TokenHash = tokenHash;
            Fingerprint = fingerprint;
            DeviceInfo = deviceInfo;
            Metadata = metadata;
            Status = RefreshTokenStatus.Active;
        }
    }
}
