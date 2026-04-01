
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Behaviors
{
    public partial class Product
    {
        public void SetMainImage(Guid imageId)
        {
            EnsureIsDraft();

            foreach (var img in _images)
                img.IsMain = false;

            var image = _images.First(i => i.Id == imageId);
            image.IsMain = true;
        }
    }

}
