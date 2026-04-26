#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.Entities
{
    public class ScoreHistoryEntry : BaseEntity
    {
        public float Score { get; private set; }
        public DateTime RecordedAt { get; private set; }

        public ScoreHistoryEntry(float score)
        {
            Score = score;
            RecordedAt = DateTime.UtcNow;
        }
    }
}

#endif