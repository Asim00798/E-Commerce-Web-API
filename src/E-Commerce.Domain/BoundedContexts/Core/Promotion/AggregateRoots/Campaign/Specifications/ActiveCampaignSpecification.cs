#if false
using System.Linq.Expressions;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.Enums;
using E_Commerce.Domain.SharedKernel.Specifications;
using CampaignAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Behaviors.Campaign;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Specifications
{
    public class ActiveCampaignSpecification : ISpecification<CampaignAggregate>
    {
        public Expression<Func<CampaignAggregate, bool>> ToExpression()
        {
            return campaign => campaign.Status.Value == CampaignStatusEnum.Active && campaign.Period.StartDate <= DateTime.UtcNow && campaign.Period.EndDate >= DateTime.UtcNow;
        }

        public bool IsSatisfiedBy(CampaignAggregate entity)
        {
            return entity.Status.Value == CampaignStatusEnum.Active && entity.Period.IsInside(DateTime.UtcNow);
        }
    }
}

#endif