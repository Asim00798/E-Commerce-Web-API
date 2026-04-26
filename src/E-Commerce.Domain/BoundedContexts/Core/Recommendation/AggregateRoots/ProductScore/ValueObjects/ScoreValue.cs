#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.ValueObjects
{
    public sealed record ScoreValue
    {
        public float Value { get; init; }

        public ScoreValue(float value)
        {
            if (value < 0) throw new ArgumentException("Score cannot be negative");
            Value = value;
        }

        public static ScoreValue Zero => new(0);
    }
}

#endif