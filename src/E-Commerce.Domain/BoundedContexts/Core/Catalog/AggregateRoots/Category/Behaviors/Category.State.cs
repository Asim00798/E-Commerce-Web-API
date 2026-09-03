using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Behaviors;

public sealed partial class Category
{
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new CategoryException("Category name is required.");

        Name = newName;
    }

    public void UpdateDescription(string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newDescription))
            throw new CategoryException("Category description is required.");

        Description = newDescription;
    }

    public void AssignParent(Guid parentCategoryId)
    {
        if (parentCategoryId == Id)
            throw new CategoryException("Category cannot be its own parent.");

        ParentCategoryId = parentCategoryId;
    }

    public void ClearParent()
    {
        ParentCategoryId = null;
    }

    public void AddImage(Guid fileId, string? altText = null)
    {
        var image = new CategoryImage(Id, fileId, altText);
        _images.Add(image);
    }

    public void RemoveImage(Guid imageFileId)
    {
        var image = _images.FirstOrDefault(x => x.FileId == imageFileId);
        if (image is not null)
        {
            _images.Remove(image);
        }
    }
}