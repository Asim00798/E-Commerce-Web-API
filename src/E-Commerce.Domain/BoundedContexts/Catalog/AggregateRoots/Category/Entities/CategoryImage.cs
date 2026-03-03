using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using System;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Category.Entities
{
    public class CategoryImage : BaseEntity
    {
        public Guid CategoryId { get; private set; }
        public ImageUrl Url { get; private set; }
        public bool IsPrimary { get; private set; }

        public CategoryImage(Guid categoryId, ImageUrl url, bool isPrimary)
        {
            CategoryId = categoryId;
            Url = url;
            IsPrimary = isPrimary;
        }

        public void SetPrimary() => IsPrimary = true;
        public void UnsetPrimary() => IsPrimary = false;
    }

}
