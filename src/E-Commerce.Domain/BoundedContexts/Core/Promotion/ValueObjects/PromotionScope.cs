#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.ValueObjects
{
    public sealed record PromotionScope
    {
        public bool IsGlobal { get; init; }
        public List<Guid> TargetStorefronts { get; init; } = new();
        public List<Guid> TargetCategories { get; init; } = new();

        public PromotionScope(bool isGlobal)
        {
            IsGlobal = isGlobal;
        }

        public static PromotionScope Global => new(true);
    }
}

#endif