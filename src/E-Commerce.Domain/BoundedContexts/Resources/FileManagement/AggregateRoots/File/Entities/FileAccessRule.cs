using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Entities
{
    public class FileAccessRule : BaseEntity
    {
        public Guid IdentityId { get; private set; }
        public AccessLevelEnum AccessLevel { get; private set; }
        public DateTime? ExpiresAt { get; private set; }

        public FileAccessRule(Guid identityId, AccessLevelEnum accessLevel, DateTime? expiresAt = null)
        {
            IdentityId = identityId;
            AccessLevel = accessLevel;
            ExpiresAt = expiresAt;
        }

        public bool IsValid => !ExpiresAt.HasValue || ExpiresAt.Value > DateTime.UtcNow;
    }
}
