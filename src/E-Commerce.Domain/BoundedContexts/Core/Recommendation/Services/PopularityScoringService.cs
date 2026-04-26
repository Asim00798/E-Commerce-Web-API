#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.Services
{
    public class PopularityScoringService
    {
        public float CalculatePopularityScore(int views, int purchases, int ratings)
        {
            return (views * 0.1f) + (purchases * 0.7f) + (ratings * 0.2f);
        }
    }
}

#endif