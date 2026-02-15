using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Enums;
using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.ValueObjects;

namespace E_Commerce.Domain.Entities.Catalog
{
    public class CategoryAttribute : BaseEntity
    {
        public Guid CategoryId { get; private set; }
        public CategoryAttributeInfo Info { get; private set; }

        // Navigation
        public Category? Category { get; private set; }

        public CategoryAttribute(Guid categoryId, CategoryAttributeInfo info)
        {
            CategoryId = categoryId;
            Info = info ?? throw new ArgumentNullException(nameof(info));
        }

        // -----------------------
        // Aggregate controlled behavior
        // -----------------------

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
