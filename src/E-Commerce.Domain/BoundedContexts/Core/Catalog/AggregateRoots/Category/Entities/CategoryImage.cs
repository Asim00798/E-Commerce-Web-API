using E_Commerce.Domain.BoundedContexts.UserManagement.PersonalData.ValueObjects;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Entities
{
    public class CategoryImage : BaseEntity
    {
        public Guid FileId { get; private set; }
        public Guid CategoryId { get; private set; }
        public bool IsPrimary { get; private set; }

        public CategoryImage(Guid categoryId, Guid fileId, bool isPrimary)
        {
            FileId = fileId;
            CategoryId = categoryId;
            IsPrimary = isPrimary;
        }

        public void SetPrimary() => IsPrimary = true;
        public void UnsetPrimary() => IsPrimary = false;
    }

}
