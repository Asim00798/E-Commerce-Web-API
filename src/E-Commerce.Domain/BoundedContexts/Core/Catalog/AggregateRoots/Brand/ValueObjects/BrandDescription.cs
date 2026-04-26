using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.ValueObjects
{
    public sealed record BrandDescription
    {
        public string Name { get; init; }
        public string? Text { get; init; }
        public string? LogoUrl { get; init; }

        public BrandDescription(string name, string? text = null, string? logoUrl = null)
        {
            Name = ValidateName(name);
            Text = ValidateText(text);
            LogoUrl = logoUrl;
        }

        // ======================
        // "With" methods for immutability + validation
        // ======================

        public BrandDescription WithName(string name) =>
            this with { Name = ValidateName(name) };

        public BrandDescription WithText(string? text) =>
            this with { Text = ValidateText(text) };

        public BrandDescription WithLogoUrl(string? logoUrl) =>
            this with { LogoUrl = logoUrl };

        // ======================
        // Validation helpers
        // ======================

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessRuleViolationException("Brand name cannot be empty");
            return name.Trim();
        }

        private static string? ValidateText(string? text)
        {
            if (text != null && text.Length > 1000)
                throw new BusinessRuleViolationException("Brand description text cannot exceed 1000 characters.");
            return text?.Trim();
        }

        public override string ToString() =>
            $"{Name}" + (string.IsNullOrWhiteSpace(Text) ? "" : $" - {Text}");
    }
}
