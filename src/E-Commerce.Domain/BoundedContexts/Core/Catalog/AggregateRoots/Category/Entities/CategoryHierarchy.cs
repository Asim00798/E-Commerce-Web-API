using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Exceptions;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Entities;

public class CategoryHierarchy : BaseEntity
{
    public Guid ParentCategoryId { get; private set; }
    public Guid ChildCategoryId { get; private set; }
    public int SortOrder { get; private set; }

    private CategoryHierarchy(Guid parentId, Guid childId, int sortOrder)
    {
        ParentCategoryId = parentId;
        ChildCategoryId = childId;
        SortOrder = sortOrder;
    }

    public static CategoryHierarchy Create(Guid parentId, Guid childId, int sortOrder)
    {
        if (parentId == Guid.Empty)
            throw new CategoryException("Parent category ID cannot be empty.");
        if (childId == Guid.Empty)
            throw new CategoryException("Child category ID cannot be empty.");
        if (parentId == childId)
            throw new CategoryException("A category cannot be its own parent.");
        if (sortOrder < 0)
            throw new CategoryException("Sort order cannot be negative.");

        return new CategoryHierarchy(parentId, childId, sortOrder);
    }

    public void UpdateSortOrder(int newSortOrder)
    {
        if (newSortOrder < 0)
            throw new CategoryException("Sort order cannot be negative.");
        SortOrder = newSortOrder;
    }
}