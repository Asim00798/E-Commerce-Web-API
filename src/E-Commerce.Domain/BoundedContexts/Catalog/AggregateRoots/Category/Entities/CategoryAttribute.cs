using E_Commerce.Domain.BoundedContexts.Catalog.ValueObjects;
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Enums;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Category.Entities
{
    public class CategoryAttribute : BaseEntity
    {
        public Guid CategoryId { get; private set; }
        public CategoryAttributeInfo Info { get; private set; }

        public CategoryAttribute(Guid categoryId, CategoryAttributeInfo info)
        {
            CategoryId = categoryId;
            Info = info ?? throw new ArgumentNullException(nameof(info));
        }

        // -----------------------------
        // Aggregate controlled behavior
        // -----------------------------

        public void Rename(string newName)
        {
            Info = Info.Rename(newName);
        }

        public void ChangeType(AttributeType newType)
        {
            if (IsInUse())
                throw new BusinessRuleViolationException("Cannot change type of attribute already in use.");

            Info = Info.ChangeType(newType);
        }

        public void UpdateDescription(string? description)
        {
            Info = Info.UpdateDescription(description);
        }

        private bool IsInUse()
        {
            // Placeholder: domain rule hook
            return false;
        }
    }
}
