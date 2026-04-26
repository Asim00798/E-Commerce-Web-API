#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceCampaign.Entities;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceCampaign.ValueObjects;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceCampaign.Behaviors
{
    public partial class MarketplaceCampaign : BaseEntity, IAggregateRoot
    {
        public string Title { get; private set; }
        public CampaignPeriod Period { get; private set; }
        public CampaignPriority Priority { get; private set; }
        public bool IsActive { get; private set; }

        private readonly List<CampaignRule> _rules = new();
        public IReadOnlyCollection<CampaignRule> Rules => _rules.AsReadOnly();

        public MarketplaceCampaign(string title, CampaignPeriod period, CampaignPriority priority)
        {
            Title = title;
            Period = period;
            Priority = priority;
            IsActive = true;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
}

#endif