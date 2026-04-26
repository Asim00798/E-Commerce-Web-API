#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.ValueObjects
{
    public sealed record VerificationStatus
    {
        public bool IsVerified { get; init; }
        public DateTime? VerifiedAt { get; init; }
        public string? Remarks { get; init; }

        public VerificationStatus(bool isVerified, DateTime? verifiedAt = null, string? remarks = null)
        {
            IsVerified = isVerified;
            VerifiedAt = verifiedAt;
            Remarks = remarks;
        }

        public static VerificationStatus Pending => new(false);
        public static VerificationStatus Verified(DateTime at) => new(true, at);
    }
}

#endif