#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.ValueObjects
{
    public sealed record ModerationStatus
    {
        public bool IsApproved { get; init; }
        public string ModeratorRemarks { get; init; }
        public DateTime ModeratedAt { get; init; }

        public ModerationStatus(bool isApproved, string moderatorRemarks, DateTime moderatedAt)
        {
            IsApproved = isApproved;
            ModeratorRemarks = moderatorRemarks;
            ModeratedAt = moderatedAt;
        }

        public static ModerationStatus Pending => new(false, "Pending Moderation", DateTime.MinValue);
    }
}

#endif