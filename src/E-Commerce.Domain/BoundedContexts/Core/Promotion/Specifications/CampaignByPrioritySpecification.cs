#if false
using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Specifications;
using CampaignAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Behaviors.Campaign;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.Specifications
{
    public class CampaignByPrioritySpecification : ISpecification<CampaignAggregate>
    {
        private readonly int _minPriority;

        public CampaignByPrioritySpecification(int minPriority)
        {
            _minPriority = minPriority;
        }

        public Expression<Func<CampaignAggregate, bool>> ToExpression()
        {
            return campaign => campaign.Priority.Value >= _minPriority;
        }

        public bool IsSatisfiedBy(CampaignAggregate entity)
        {
            return entity.Priority.Value >= _minPriority;
        }
    }
}

#endif