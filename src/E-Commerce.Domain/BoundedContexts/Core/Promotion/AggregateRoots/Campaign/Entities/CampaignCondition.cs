#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Entities
{
    public class CampaignCondition : BaseEntity
    {
        public string Attribute { get; private set; }
        public string Operator { get; private set; }
        public string ExpectedValue { get; private set; }

        public CampaignCondition(string attribute, string @operator, string expectedValue)
        {
            Attribute = attribute;
            Operator = @operator;
            ExpectedValue = expectedValue;
        }
    }
}

#endif