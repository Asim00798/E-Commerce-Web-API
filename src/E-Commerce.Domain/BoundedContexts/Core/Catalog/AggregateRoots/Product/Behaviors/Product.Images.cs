using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Entities;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors
{
    public partial class Product
    {
        public void AddImage(Guid fileId, string? altText = null)
        {
            EnsureIsDraft();
            var image = new ProductImage(Id, fileId, altText);
            _images.Add(image);
        }
        public void SetMainImage(Guid imageId)
        {
            EnsureIsDraft();
            var image = _images.FirstOrDefault(i => i.Id == imageId)
                ?? throw new BusinessRuleViolationException("Image not found.");

            foreach (var img in _images)
                img.IsMain = false;

            image.IsMain = true;
        }
    }

}
