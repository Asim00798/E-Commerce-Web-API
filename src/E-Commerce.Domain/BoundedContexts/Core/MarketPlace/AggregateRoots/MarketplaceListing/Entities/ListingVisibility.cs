#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Entities
{
    public class ListingVisibility : BaseEntity
    {
        public bool IsVisible { get; private set; }
        public List<string> RestrictedRegions { get; private set; } = new();

        public ListingVisibility(bool isVisible)
        {
            IsVisible = isVisible;
        }

        public void Show() => IsVisible = true;
        public void Hide() => IsVisible = false;
    }
}

#endif