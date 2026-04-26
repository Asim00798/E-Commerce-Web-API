using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors
{
    public partial class Brand
    {
        public void AddSocialLink(SocialPlatform platform, string url)
        {
            _socialLinks.Add(new BrandSocialLink(platform, url));
        }

        public void RemoveSocialLink(SocialPlatform platform)
        {
            var link = _socialLinks.FirstOrDefault(s => s.Platform == platform);
            if (link != null)
                _socialLinks.Remove(link);
        }

        public void UpdateSocialLink(SocialPlatform platform, string newUrl)
        {
            var link = _socialLinks.FirstOrDefault(s => s.Platform == platform);
            if (link != null)
            {
                // Since BrandSocialLink properties are private set, we need to remove and re-add it
                _socialLinks.Remove(link);
                _socialLinks.Add(new BrandSocialLink(platform, newUrl));
            }
        }


    }
}
