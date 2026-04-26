using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Behaviors
{
    public partial class Category
    {
        /// <summary>
        /// Adds a child category. Does NOT check cycles or depth – that's the caller's responsibility.
        /// </summary>
        public void AddChild(Category child, int sortOrder = 0)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (child.Id == Id)
                throw new CategoryException("Cannot add a category as its own child.");
            if (_hierarchies.Any(h => h.ChildCategoryId == child.Id))
                throw new CategoryException($"Category '{child.Id}' is already a child of this category.");

            var hierarchy = CategoryHierarchy.Create(Id, child.Id, sortOrder);
            _hierarchies.Add(hierarchy);
        }

        /// <summary>
        /// Removes a direct child.
        /// </summary>
        public void RemoveChild(Guid childId)
        {
            var hierarchy = _hierarchies.FirstOrDefault(h => h.ChildCategoryId == childId);
            if (hierarchy == null)
                throw new CategoryException($"Category '{childId}' is not a direct child.");

            _hierarchies.Remove(hierarchy);
        }

        /// <summary>
        /// Moves this category to a new parent. Does NOT check cycles – caller must ensure.
        /// </summary>
        public void MoveToParent(Guid newParentId, int sortOrder = 0)
        {
            if (newParentId == Id)
                throw new CategoryException("Cannot move a category under itself.");

            // Remove existing parent link (if any)
            var existingLink = _hierarchies.FirstOrDefault(h => h.ChildCategoryId == Id);
            if (existingLink != null)
                _hierarchies.Remove(existingLink);

            // Add new parent link
            _hierarchies.Add(CategoryHierarchy.Create(newParentId, Id, sortOrder));
        }

        /// <summary>
        /// Returns whether this category is a root (has no parent).
        /// </summary>
        public bool IsRoot() => !_hierarchies.Any(h => h.ChildCategoryId == Id);

        /// <summary>
        /// Returns the parent ID, or null if root.
        /// </summary>
        public Guid? GetParentId() =>
            _hierarchies.FirstOrDefault(h => h.ChildCategoryId == Id)?.ParentCategoryId;

        /// <summary>
        /// Returns all direct child IDs.
        /// </summary>
        public IReadOnlyList<Guid> GetChildIds() =>
            _hierarchies.Where(h => h.ParentCategoryId == Id).Select(h => h.ChildCategoryId).ToList();
    }
}


