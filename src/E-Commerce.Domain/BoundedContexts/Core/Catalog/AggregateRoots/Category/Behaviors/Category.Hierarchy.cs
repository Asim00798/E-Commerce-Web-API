using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Category.Entities;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Category.Behaviors
{
    public partial class Category
    {
        public void AddChild(Category child, int sortOrder = 0)
        {
            if (_hierarchies.Any(h => h.ChildCategoryId == child.Id))
                throw new BusinessRuleViolationException("Child already exists.");
            _hierarchies.Add(CategoryHierarchy.Create(Id, child.Id, sortOrder));
        }

        public void MoveUnder(Category parent)
        {
            if (parent.Id == Id)
                throw new BusinessRuleViolationException("Category cannot be its own parent.");
            if (IsDescendantOf(parent))
                throw new BusinessRuleViolationException("Cannot move a category under its descendant.");
        }

        private bool IsDescendantOf(Category potentialAncestor)
        {
            // Implement hierarchy check logic
            return false;
        }

        public bool IsRoot()
        {
            return !_hierarchies.Any(h => h.ChildCategoryId == Id);
        }
    }
}
