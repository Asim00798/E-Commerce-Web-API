using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors
{
    public partial class Brand
    {
        public void SetLogo(Guid fileId, bool isPrimary = false)
        {
            _logos.Add(new BrandLogo(fileId, isPrimary));
        }

        public void RemoveLogo(Guid fileId) {
           _logos.Remove(new BrandLogo(fileId));
        }

        public void SetPrimaryLogo(Guid fileId)
        {
            var logo = _logos.FirstOrDefault(l => l.FileId == fileId);
            if (logo != null)
            {
                foreach (var l in _logos) l.UnsetPrimary();
                logo.SetPrimary();
            }
        }

        public void SetSecondaryLogo(Guid fileId) {
            var logo = _logos.FirstOrDefault(l => l.FileId == fileId);
            if (logo != null)
                logo.UnsetPrimary();
        }

    }
}
