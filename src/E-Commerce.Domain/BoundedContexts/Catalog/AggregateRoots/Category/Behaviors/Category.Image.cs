using E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Category.Entities;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Category.Behaviors
{
    public partial class Category
    {
        public void AddImage(CategoryImage image)
        {
            if (_images.Any(i => i.Id == image.Id))
                return;
            _images.Add(image);
        }

        public void RemoveImage(Guid imageId)
        {
            var image = _images.FirstOrDefault(i => i.Id == imageId);
            if (image != null) _images.Remove(image);
        }

        public void SetPrimaryImage(Guid imageId)
        {
            foreach (var img in _images)
                img.UnsetPrimary();
            var primary = _images.FirstOrDefault(i => i.Id == imageId);
            primary?.SetPrimary();
        }
    }
}
