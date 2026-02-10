using E_Commerce.Domain.Exceptions;

namespace E_Commerce.Domain.ValueObjects
{
    public sealed record BrandDescription
    {
        public string Name { get; private set; }
        public string? Text { get; private set; }
        public string? LogoUrl { get; private set; }

        public BrandDescription(string name, string? text = null, string? logoUrl = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessRuleViolationException("Brand name cannot be empty");
            if (text != null && text.Length > 1000)
                throw new BusinessRuleViolationException("Brand description text cannot exceed 1000 characters.");

            Name = name;
            Text = text;
            LogoUrl = logoUrl;
        }

        public void Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName) || Name == newName) return;
            Name = newName;
        }

        public void UpdateDescription(string? newText)
        {
            if (Text == newText) return;
            Text = newText;
        }

        public void UpdateLogo(string? newLogoUrl)
        {
            if (LogoUrl == newLogoUrl) return;
            LogoUrl = newLogoUrl;
        }
    }
}
