#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Storefront.Entities;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Storefront.ValueObjects;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.ValueObjects;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Storefront.Behaviors
{
    public partial class Storefront : BaseEntity, IAggregateRoot
    {
        public SellerId SellerId { get; private set; }
        public StoreName Name { get; private set; }
        public StoreStatus Status { get; private set; }
        
        private readonly List<StoreCategory> _categories = new();
        private readonly List<StorePolicy> _policies = new();

        public IReadOnlyCollection<StoreCategory> Categories => _categories.AsReadOnly();
        public IReadOnlyCollection<StorePolicy> Policies => _policies.AsReadOnly();

        public Storefront(SellerId sellerId, StoreName name)
        {
            SellerId = sellerId;
            Name = name;
            Status = StoreStatus.Active;
        }

        public void UpdateName(StoreName newName) => Name = newName;
        public void ChangeStatus(StoreStatus newStatus) => Status = newStatus;
    }
}

#endif