using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Events;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Evaluation;
using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors
{
    public partial class Brand : BaseEntity, IAggregateRoot, IComplianceTarget
    {
        private readonly List<BrandLogo> _logos = new();
        private readonly List<BrandSocialLink> _socialLinks = new();
        private readonly List<Contact> _contacts = new();

        public BrandDescription Description { get; private set; }

        public IReadOnlyCollection<BrandLogo> Logos => _logos.AsReadOnly();
        public IReadOnlyCollection<BrandSocialLink> SocialLinks => _socialLinks.AsReadOnly();
        public IReadOnlyCollection<Contact> Contacts => _contacts.AsReadOnly();

        private Brand(BrandDescription description)
        {
            Description = description ?? throw new ArgumentNullException(nameof(description));

            AddDomainEvent(new BrandCreatedEvent(Id, Description.Name));
        }

        public static Brand Create(BrandDescription description)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));

            // Factory-level invariants (redundant with VO but safe for Aggregate Root integrity)
            if (string.IsNullOrWhiteSpace(description.Name))
                throw new ArgumentException("Brand name is required.", nameof(description));

            return new Brand(description);
        }
    }
}
