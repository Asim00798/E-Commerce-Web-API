using E_Commerce.Domain.BoundedContexts.Catalog.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Brand.Behaviors
{
    public partial class Brand
    {
        public void AddLogo(string url, bool isPrimary = false)
        {
            _logos.Add(new BrandLogo(url, isPrimary));
        }

        public void RemoveLogo(string url) {
           _logos.Remove(new BrandLogo(url));
        }

        public void SetPrimaryLogo(string url)
        {
            var logo = _logos.FirstOrDefault(l => l.Url == url);
            if (logo != null)
            {
                foreach (var l in _logos) l.UnsetPrimary();
                logo.SetPrimary();
            }
        }

        public void SetSecondaryLogo(string url) {
            var logo = _logos.FirstOrDefault(l => l.Url == url);
            if (logo != null)
                logo.UnsetPrimary();
        }

    }
}
