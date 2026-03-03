using E_Commerce.Domain.BoundedContexts.Catalog.Enums;
using System;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Brand.ValueObjects
{
    public sealed record BrandSocialLink
    {
        public SocialPlatform Platform { get; init; }
        public string Url { get; init; }

        public BrandSocialLink(SocialPlatform platform, string url)
        {
            if (!Enum.IsDefined(typeof(SocialPlatform), platform))
                throw new ArgumentException("Invalid social platform.", nameof(platform));

            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                throw new ArgumentException("Invalid social URL.", nameof(url));

            Platform = platform;
            Url = url.Trim();
        }

        public BrandSocialLink ChangeUrl(string newUrl)
        {
            if (!Uri.TryCreate(newUrl, UriKind.Absolute, out _))
                throw new ArgumentException("Invalid social URL.", nameof(newUrl));

            return this with { Url = newUrl.Trim() };
        }

        public BrandSocialLink ChangePlatform(SocialPlatform newPlatform)
        {
            if (!Enum.IsDefined(typeof(SocialPlatform), newPlatform))
                throw new ArgumentException("Invalid social platform.", nameof(newPlatform));
            return this with { Platform = newPlatform };
        }

        public override string ToString() 
            => $"{Platform}: {Url}";
    }
}
