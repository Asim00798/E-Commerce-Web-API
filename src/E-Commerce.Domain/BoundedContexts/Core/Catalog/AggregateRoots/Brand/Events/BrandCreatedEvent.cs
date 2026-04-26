using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Events
{
    public class BrandCreatedEvent : DomainEvent
    {
        public Guid BrandId { get; }
        public string BrandName { get; }

        public BrandCreatedEvent(Guid brandId, string brandName)
        {
            BrandId = brandId;
            BrandName = brandName;
        }
    }
}
