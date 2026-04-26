#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.PromotionPolicy.Entities;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.PromotionPolicy.ValueObjects;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.PromotionPolicy.Behaviors
{
    public partial class PromotionPolicy : BaseEntity, IAggregateRoot
    {
        public string Name { get; private set; }
        public PolicyType Type { get; private set; }
        public bool IsActive { get; private set; }

        private readonly List<EligibilityRule> _rules = new();
        public IReadOnlyCollection<EligibilityRule> Rules => _rules.AsReadOnly();

        public PromotionPolicy(string name, PolicyType type)
        {
            Name = name;
            Type = type;
            IsActive = true;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}

#endif