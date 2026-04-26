#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.Entities;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.ValueObjects;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.Enums;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.Behaviors
{
    public partial class Seller : BaseEntity, IAggregateRoot
    {
        public SellerName Name { get; private set; }
        public SellerStatusEnum Status { get; private set; }
        public VerificationStatus Verification { get; private set; }
        
        private readonly List<SellerContact> _contacts = new();
        private readonly List<SellerDocument> _documents = new();
        private readonly List<SellerVerification> _verifications = new();

        public IReadOnlyCollection<SellerContact> Contacts => _contacts.AsReadOnly();
        public IReadOnlyCollection<SellerDocument> Documents => _documents.AsReadOnly();
        public IReadOnlyCollection<SellerVerification> Verifications => _verifications.AsReadOnly();

        public Seller(SellerName name)
        {
            Name = name;
            Status = SellerStatusEnum.Pending;
            Verification = VerificationStatus.Pending;
        }

        public void Activate() => Status = SellerStatusEnum.Active;
        public void Suspend() => Status = SellerStatusEnum.Suspended;
    }
}

#endif