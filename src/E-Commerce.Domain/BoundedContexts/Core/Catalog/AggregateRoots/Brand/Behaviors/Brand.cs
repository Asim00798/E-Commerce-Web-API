using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Brand.ValueObjects;
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Interfaces;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Brand.Behaviors
{
    public partial class Brand : BaseEntity,IAggregateRoot
    {
        public BrandDescription? Description { get; private set; }

        private readonly List<BrandLogo> _logos = new();
        public IReadOnlyCollection<BrandLogo> Logos => _logos;

        private readonly List<BrandSocialLink> _socialLinks = new();
        public IReadOnlyCollection<BrandSocialLink> SocialLinks => _socialLinks;

        private readonly List<Guid> _documentsIds = new();
        public IReadOnlyCollection<Guid> DocumentsIds => _documentsIds;

        private readonly List<Contact> _contacts = new();
        public IReadOnlyCollection<Contact> Contacts => _contacts;

        internal Brand(BrandDescription description)
        {
           Description = description;
        }
    }
}
