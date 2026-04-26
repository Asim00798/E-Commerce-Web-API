#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.ValueObjects
{
    public sealed record RecommendationContext
    {
        public Guid? UserId { get; init; }
        public string? SessionId { get; init; }
        public List<Guid>? ExcludedProductIds { get; init; }
        public int MaxResults { get; init; }

        public RecommendationContext(Guid? userId, string? sessionId, int maxResults = 10)
        {
            UserId = userId;
            SessionId = sessionId;
            MaxResults = maxResults;
            ExcludedProductIds = new List<Guid>();
        }
    }
}

#endif