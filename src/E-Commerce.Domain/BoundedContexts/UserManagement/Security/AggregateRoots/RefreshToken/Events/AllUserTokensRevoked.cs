using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.Events
{
    public sealed class AllUserTokensRevoked : DomainEvent
    {
        public Guid UserId { get; }
        public int TokenCount { get; }
        public string? Reason { get; }

        public AllUserTokensRevoked(Guid userId, int tokenCount, string? reason = null)
        {
            UserId = userId;
            TokenCount = tokenCount;
            Reason = reason;
        }
    }
}
