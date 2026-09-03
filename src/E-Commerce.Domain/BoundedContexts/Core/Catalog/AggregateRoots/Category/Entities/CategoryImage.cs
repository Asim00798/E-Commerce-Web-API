using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Entities;

public sealed class CategoryImage : BaseEntity
{
    public Guid CategoryId { get; private set; }
    public Guid FileId { get; private set; }
    public string? AltText { get; private set; }

    private CategoryImage()
    {
        // EF Core
    }

    internal CategoryImage(Guid categoryId, Guid fileId, string? altText = null)
    {
        CategoryId = categoryId;
        FileId = fileId;
        AltText = altText;
    }
}