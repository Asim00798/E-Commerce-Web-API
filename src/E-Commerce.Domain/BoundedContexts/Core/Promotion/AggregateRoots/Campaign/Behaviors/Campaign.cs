#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Entities;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.ValueObjects;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Behaviors
{
    public partial class Campaign : BaseEntity, IAggregateRoot
    {
        public CampaignName Name { get; private set; }
        public CampaignPeriod Period { get; private set; }
        public CampaignPriority Priority { get; private set; }
        public CampaignStatus Status { get; private set; }

        private readonly List<CampaignRule> _rules = new();
        private readonly List<CampaignCondition> _conditions = new();
        private readonly List<CampaignUsage> _usages = new();

        public IReadOnlyCollection<CampaignRule> Rules => _rules.AsReadOnly();
        public IReadOnlyCollection<CampaignCondition> Conditions => _conditions.AsReadOnly();
        public IReadOnlyCollection<CampaignUsage> Usages => _usages.AsReadOnly();

        public Campaign(CampaignName name, CampaignPeriod period, CampaignPriority priority)
        {
            Name = name;
            Period = period;
            Priority = priority;
            Status = CampaignStatus.Draft;
        }

        public void Activate() => Status = CampaignStatus.Active;
    }
}

#endif