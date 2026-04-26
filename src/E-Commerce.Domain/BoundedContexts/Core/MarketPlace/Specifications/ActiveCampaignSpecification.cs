#if false
using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Specifications;
using CampaignAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceCampaign.Behaviors.MarketplaceCampaign;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.Specifications
{
    public class ActiveCampaignSpecification : ISpecification<CampaignAggregate>
    {
        public Expression<Func<CampaignAggregate, bool>> ToExpression()
        {
            return campaign => campaign.IsActive && campaign.Period.StartDate <= DateTime.UtcNow && campaign.Period.EndDate >= DateTime.UtcNow;
        }

        public bool IsSatisfiedBy(CampaignAggregate entity)
        {
            return entity.IsActive && entity.Period.IsInside(DateTime.UtcNow);
        }
    }
}

#endif