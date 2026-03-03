
namespace E_Commerce.Domain.BoundedContexts.Catalog.ValueObjects
{
    public sealed record BrandLogo
    {
        public string Url { get; init; }
        public bool IsPrimary { get; init; }

        public BrandLogo(string url, bool isPrimary = false)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                throw new ArgumentException("Invalid logo URL.", nameof(url));

            Url = url.Trim();
            IsPrimary = isPrimary;
        }

        internal BrandLogo SetPrimary() => this with { IsPrimary = true };

        internal BrandLogo UnsetPrimary() => this with { IsPrimary = false };

        public BrandLogo ChangeUrl(string newUrl)
        {
            if (!Uri.TryCreate(newUrl, UriKind.Absolute, out _))
                throw new ArgumentException("Invalid logo URL.", nameof(newUrl));

            return this with { Url = newUrl.Trim() };
        }
    }
}
