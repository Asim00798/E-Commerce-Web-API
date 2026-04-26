#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.ValueObjects
{
    public sealed record ScoreWeight
    {
        public float Views { get; init; }
        public float Purchases { get; init; }
        public float Ratings { get; init; }

        public ScoreWeight(float views, float purchases, float ratings)
        {
            Views = views;
            Purchases = purchases;
            Ratings = ratings;
        }

        public static ScoreWeight Default => new(1.0f, 5.0f, 3.0f);
    }
}

#endif