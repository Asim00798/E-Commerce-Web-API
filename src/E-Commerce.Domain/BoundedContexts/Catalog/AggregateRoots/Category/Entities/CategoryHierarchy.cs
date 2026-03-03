using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Category.Entities
{
    public class CategoryHierarchy : BaseEntity
    {
        public Guid ParentCategoryId { get; private set; }
        public Guid ChildCategoryId { get; private set; }

        public int Level { get; private set; }
        public int SortOrder { get; private set; }
        public bool IsActive { get; private set; } = true;

        private CategoryHierarchy() { }

        public static CategoryHierarchy Create(Guid parentId, Guid childId, int sortOrder)
        {
            if (parentId == childId)
                throw new BusinessRuleViolationException("A category cannot be its own parent.");

            return new CategoryHierarchy
            {
                ParentCategoryId = parentId,
                ChildCategoryId = childId,
                SortOrder = sortOrder,
                Level = 0,
                IsActive = true
            };
        }

        public void Move(int newSortOrder)
            => SortOrder = newSortOrder;

        public void Disable()
            => IsActive = false;
    }

}
