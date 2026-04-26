#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.Entities
{
    public class SellerContact : BaseEntity
    {
        public string ContactName { get; private set; }
        public string Email { get; private set; }
        public string PhoneNumber { get; private set; }
        public bool IsPrimary { get; private set; }

        public SellerContact(string contactName, string email, string phoneNumber, bool isPrimary = false)
        {
            ContactName = contactName;
            Email = email;
            PhoneNumber = phoneNumber;
            IsPrimary = isPrimary;
        }

        public void SetAsPrimary() => IsPrimary = true;
    }
}

#endif