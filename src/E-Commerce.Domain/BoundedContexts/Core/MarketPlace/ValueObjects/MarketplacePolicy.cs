#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.ValueObjects
{
    public sealed record MarketplacePolicy
    {
        public string Name { get; init; }
        public string Content { get; init; }
        public DateTime EffectiveDate { get; init; }

        public MarketplacePolicy(string name, string content, DateTime effectiveDate)
        {
            Name = name;
            Content = content;
            EffectiveDate = effectiveDate;
        }
    }
}

#endif