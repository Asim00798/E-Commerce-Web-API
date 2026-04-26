#if false
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.Rules
{
    public class ScoreMustBeWithinValidRangeRule : IBusinessRule
    {
        private readonly float _score;

        public ScoreMustBeWithinValidRangeRule(float score)
        {
            _score = score;
        }

        public bool IsSatisfied() => _score >= 0 && _score <= 1000000; // Arbitrary max for now

        public string Message => "Product score must be within a valid range (0-1,000,000).";
    }
}

#endif