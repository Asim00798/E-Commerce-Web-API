using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Exceptions;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Behaviors;

public sealed partial class Category : BaseEntity, IAggregateRoot
{
    private readonly List<CategoryImage> _images = new();

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid? ParentCategoryId { get; private set; }

    public IReadOnlyCollection<CategoryImage> Images => _images.AsReadOnly();

    private Category()
    {
        // EF Core
    }

    private Category(string name, string description, Guid? parentCategoryId = null)
    {
        Name = name;
        Description = description;
        ParentCategoryId = parentCategoryId;
    }

    public static Category Create(string name, string description, Guid? parentCategoryId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new CategoryException("Category name is required.");

        if (string.IsNullOrWhiteSpace(description))
            throw new CategoryException("Category description is required.");

        return new Category(name, description, parentCategoryId);
    }
}