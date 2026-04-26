using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Events
{
    public class CategoryCreatedEvent : DomainEvent
    {
        public Guid CategoryId { get; }
        public string CategoryName { get; }

        public CategoryCreatedEvent(Guid categoryId, string categoryName)
        {
            CategoryId = categoryId;
            CategoryName = categoryName;
        }
    }
}
