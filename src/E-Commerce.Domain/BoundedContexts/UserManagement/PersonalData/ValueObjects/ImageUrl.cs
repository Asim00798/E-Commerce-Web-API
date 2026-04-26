#if false
namespace E_Commerce.Domain.BoundedContexts.UserManagement.PersonalData.ValueObjects
{
    public sealed class ImageUrl
    {
        public string Value { get; }

        public ImageUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Image URL cannot be empty.", nameof(url));

            // Optional: Add regex validation for URL format
            Value = url;
        }

        public ImageUrl ChangeUrl(string newUrl) => new ImageUrl(newUrl);

        public override string ToString() => Value;
    }
}

#endif